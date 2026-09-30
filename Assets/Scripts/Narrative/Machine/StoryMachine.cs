using System;
using System.Collections.Generic;
using LegacyThroughTime.Narrative.Ink;
using LegacyThroughTime.Narrative.Json;

namespace LegacyThroughTime.Narrative
{
    /// <summary>
    /// The visual-novel machine: a Mealy-type extended finite-state machine
    /// M = (Q, Σ, Λ, V, δ, λ, q₀, v₀) where
    /// <list type="bullet">
    /// <item>Q = <see cref="StoryPhase"/> — what the player is looking at;</item>
    /// <item>Σ = { Advance, Choose(i) } — the only two things a player can do;</item>
    /// <item>V = the ink state held by an <see cref="InkRunner"/> (variables, counts, call stack…);</item>
    /// <item>δ, λ — <see cref="Advance"/> and <see cref="Choose"/>: each accepted input yields exactly
    ///   one output <see cref="Beat"/> and the delta of V it caused;</item>
    /// <item>guards make δ total: an input not enabled in the current state is rejected
    ///   (returns false) and leaves both q and V untouched.</item>
    /// </list>
    /// The machine knows nothing about rendering, audio, Unity or time; a presenter feeds it
    /// inputs and draws its beats. Given the same story, seed and input sequence it always
    /// produces the same beats — so a save is also reproducible as (seed, inputs).
    /// </summary>
    public sealed class StoryMachine
    {
        readonly InkRunner _runner;
        readonly StoryStyle _style;
        readonly Value[] _before;
        readonly List<string> _errors = new List<string>();
        readonly List<string> _warnings = new List<string>();

        public StoryMachine(InkStory story, StoryStyle style = null, int? seed = null)
        {
            _runner = new InkRunner(story, seed) { OnError = CollectDiagnostic };
            _style = style ?? StoryStyle.Default;
            _before = new Value[story.GlobalNames.Length];
            Current = Beat.Initial;
        }

        /// <summary>Convenience: compile JSON and start a machine.</summary>
        public static StoryMachine FromJson(string inkJson, StoryStyle style = null, int? seed = null) =>
            new StoryMachine(InkStory.FromJson(inkJson), style, seed);

        public StoryPhase Phase => Current.Phase;

        /// <summary>The last output. Stays valid until the next accepted input.</summary>
        public Beat Current { get; private set; }

        public bool IsFinished => Phase == StoryPhase.End || Phase == StoryPhase.Fault;

        /// <summary>Guard of Advance: a line (or nothing yet) is on screen.</summary>
        public bool CanAdvance => Phase == StoryPhase.Ready || Phase == StoryPhase.Line || Phase == StoryPhase.Directive;

        /// <summary>Guard of Choose(i).</summary>
        public bool CanChoose(int index) => Phase == StoryPhase.Choice && index >= 0 && index < Current.Options.Count;

        /// <summary>Σ = Advance: show the next line, the options, or the end.</summary>
        public bool Advance()
        {
            if (!CanAdvance) return false;
            Transition(null);
            return true;
        }

        /// <summary>Σ = Choose(i): take option i and show what follows it.</summary>
        public bool Choose(int index)
        {
            if (!CanChoose(index)) return false;
            Transition(index);
            return true;
        }

        // ---- extended state access

        public IReadOnlyList<string> VariableNames => _runner.Story.GlobalVariableNames;

        public object GetVariable(string name) => _runner.GetVariable(name);

        public int GetInt(string name) => GetVariable(name) switch
        {
            int i => i,
            float f => (int)f,
            bool b => b ? 1 : 0,
            _ => 0,
        };

        public bool GetBool(string name) => GetVariable(name) switch
        {
            bool b => b,
            int i => i != 0,
            float f => f != 0f,
            string s => s.Length > 0,
            _ => false,
        };

        /// <summary>Host-side write (e.g. gems bought in the shop). Not reported as a beat delta.</summary>
        public void SetVariable(string name, object value) => _runner.SetVariable(name, value);

        public int VisitCount(string path) => _runner.VisitCount(path);

        /// <summary>Binds an ink <c>EXTERNAL</c>. Leave <paramref name="lookaheadSafe"/> false for side effects.</summary>
        public void BindExternal(string name, ExternalFunction fn, bool lookaheadSafe = false) =>
            _runner.BindExternalFunction(name, fn, lookaheadSafe);

        /// <summary>Calls an ink function (e.g. a formatter) without advancing the story.</summary>
        public object Evaluate(string function, params object[] args) => _runner.EvaluateFunction(function, args);

        // ---- persistence

        const int SaveVersion = 1;

        public string Save()
        {
            var w = new JsonWriter();
            w.BeginObject();
            w.Property("storyMachine", SaveVersion);
            w.Property("phase", Phase.ToString());
            if (Phase == StoryPhase.Fault) w.Property("error", Current.Error);
            w.Key("ink").Raw(_runner.SaveState());
            w.EndObject();
            return w.ToString();
        }

        /// <summary>Restores a <see cref="Save"/>d machine; <see cref="Current"/> is rebuilt from the state.</summary>
        public void Load(string save)
        {
            if (!(JsonReader.Parse(save) is Dictionary<string, object> root) || !root.TryGetValue("storyMachine", out _))
                throw new FormatException("Not a story machine save");
            if (!Enum.TryParse((string)root["phase"], out StoryPhase phase))
                throw new FormatException("Unknown phase in save: " + root["phase"]);

            _runner.LoadState(root["ink"] as Dictionary<string, object>);

            switch (phase)
            {
                case StoryPhase.Line:
                case StoryPhase.Directive:
                    Current = LineBeat(_runner.CurrentText, _runner.CurrentTags);
                    break;
                case StoryPhase.Choice:
                    Current = ChoiceBeat();
                    break;
                case StoryPhase.Fault:
                    Current = new Beat(StoryPhase.Fault, error: root.TryGetValue("error", out var e) ? (string)e : "fault");
                    break;
                default:
                    Current = phase == StoryPhase.End ? new Beat(StoryPhase.End) : Beat.Initial;
                    break;
            }
        }

        // ---- δ and λ

        void Transition(int? choice)
        {
            Array.Copy(_runner.Store.Globals, _before, _before.Length);
            _errors.Clear();
            _warnings.Clear();

            Beat beat;
            try
            {
                if (choice.HasValue) _runner.ChooseChoiceIndex(choice.Value);
                beat = Produce();
            }
            catch (StoryException e)
            {
                beat = new Beat(StoryPhase.Fault, error: e.Message);
            }

            Current = beat.WithDelta(Delta(), _warnings.Count > 0 ? _warnings.ToArray() : null);
        }

        /// <summary>Runs the VM until something presentable appears.</summary>
        Beat Produce()
        {
            while (true)
            {
                if (_runner.CanContinue)
                {
                    var text = _runner.Continue();
                    if (_errors.Count > 0) return new Beat(StoryPhase.Fault, error: string.Join("\n", _errors));
                    var tags = _runner.CurrentTags;
                    // Ink can emit empty lines (e.g. a line made only of logic): nothing to show.
                    if (text.Trim().Length == 0 && tags.Count == 0) continue;
                    return LineBeat(text, tags);
                }
                if (_errors.Count > 0) return new Beat(StoryPhase.Fault, error: string.Join("\n", _errors));
                return _runner.CurrentChoices.Count > 0 ? ChoiceBeat() : new Beat(StoryPhase.End);
            }
        }

        Beat LineBeat(string text, IReadOnlyList<string> rawTags)
        {
            var tags = ParseTags(rawTags);
            if (text.Trim().Length == 0) return new Beat(StoryPhase.Directive, tags: tags);
            _style.ParseLine(text, tags, out var voice, out var speaker, out var body);
            return new Beat(StoryPhase.Line, voice, speaker, body, tags);
        }

        Beat ChoiceBeat()
        {
            var choices = _runner.CurrentChoices;
            var options = new StoryOption[choices.Count];
            for (int i = 0; i < options.Length; i++)
            {
                var c = choices[i];
                _style.ParseOption(c.Text, out var text, out bool premium, out int cost);
                options[i] = new StoryOption(i, text, c.Text, ParseTags(c.Tags), premium, cost);
            }
            return new Beat(StoryPhase.Choice, options: options);
        }

        static IReadOnlyList<StoryTag> ParseTags(IReadOnlyList<string> raw)
        {
            if (raw.Count == 0) return Array.Empty<StoryTag>();
            var tags = new StoryTag[raw.Count];
            for (int i = 0; i < tags.Length; i++) tags[i] = StoryTag.Parse(raw[i]);
            return tags;
        }

        /// <summary>Δ = { (name, v, v') | v ≠ v' } over all globals, in declaration order.</summary>
        IReadOnlyList<VariableChange> Delta()
        {
            List<VariableChange> changes = null;
            var after = _runner.Store.Globals;
            var names = _runner.Story.GlobalNames;
            for (int i = 0; i < after.Length; i++)
            {
                if (after[i].SameAs(_before[i])) continue;
                (changes ??= new List<VariableChange>()).Add(new VariableChange(names[i], _before[i].ToHost(), after[i].ToHost()));
            }
            return changes;
        }

        void CollectDiagnostic(string message, bool isWarning) => (isWarning ? _warnings : _errors).Add(message);
    }
}
