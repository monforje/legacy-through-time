using System;
using System.Collections.Generic;
using System.Text;

namespace LegacyThroughTime.Narrative.Ink
{
    public class StoryException : Exception
    {
        public StoryException(string message) : base(message) { }
    }

    /// <summary>A runtime error raised by ink semantics; becomes a story error, never escapes Continue.</summary>
    sealed class InkException : Exception
    {
        public InkException(string message) : base(message) { }
    }

    enum OutKind : byte
    {
        Text,
        Glue,
        BeginString, // control commands: they stop whitespace trimming
        BeginTag,
        EndTag,
        Tag,         // pre-1.1 static tag object
        Other,       // a non-text value printed in content mode; invisible, but occupies a slot
    }

    readonly struct OutItem
    {
        public readonly OutKind Kind;
        public readonly string Text;
        public readonly bool IsNewline;
        public readonly bool IsInlineWhitespace;

        OutItem(OutKind kind, string text)
        {
            Kind = kind;
            Text = text;
            IsNewline = kind == OutKind.Text && text == "\n";
            bool ws = kind == OutKind.Text;
            if (ws)
                foreach (char c in text)
                    if (c != ' ' && c != '\t') { ws = false; break; }
            IsInlineWhitespace = ws;
        }

        public bool IsNonWhitespace => !IsNewline && !IsInlineWhitespace;
        public bool IsCommand => Kind == OutKind.BeginString || Kind == OutKind.BeginTag || Kind == OutKind.EndTag;

        public static readonly OutItem Newline = new OutItem(OutKind.Text, "\n");
        public static readonly OutItem Glue = new OutItem(OutKind.Glue, null);
        public static readonly OutItem BeginString = new OutItem(OutKind.BeginString, null);
        public static readonly OutItem BeginTag = new OutItem(OutKind.BeginTag, null);
        public static readonly OutItem EndTag = new OutItem(OutKind.EndTag, null);
        public static readonly OutItem Other = new OutItem(OutKind.Other, null);

        public static OutItem OfText(string text) => text == "\n" ? Newline : new OutItem(OutKind.Text, text);
        public static OutItem OfTag(string text) => new OutItem(OutKind.Tag, text);
    }

    /// <summary>A generated choice, bound to the thread it was generated in.</summary>
    sealed class ChoiceRecord
    {
        public string Text;
        public List<string> Tags;
        public Container Target;
        public string SourcePath;
        public bool IsInvisibleDefault;
        public InkThread Thread;
        public int OriginalThreadIndex;
    }

    /// <summary>
    /// The control part of the extended state: everything a glue lookahead may have to
    /// rewind wholesale. It is small (a line's worth of output, a few frames), so a
    /// snapshot is a plain deep copy.
    /// </summary>
    sealed class Cursor
    {
        public CallStack CallStack;
        public List<OutItem> Output = new List<OutItem>(16);
        public List<ChoiceRecord> Choices = new List<ChoiceRecord>(4);
        public List<Value> Eval = new List<Value>(16);
        public Pointer Diverted = Pointer.Null;
        public bool DidSafeExit;
        public int TurnIndex = -1;
        public int Seed;
        public int PreviousRandom;
        public List<string> Errors;
        public List<string> Warnings;

        int _beginStrings; // count of BeginString items in Output → O(1) "in string evaluation"

        /// <summary>
        /// Bumped by every removal from Output. While it is unchanged the stream has only
        /// grown, which lets the lookahead judge a newline from the appended suffix alone.
        /// </summary>
        public int Removals;
        bool _textDirty = true, _tagsDirty = true;
        string _text;
        List<string> _tags;

        public Cursor(Container root) => CallStack = new CallStack(root);

        /// <summary>
        /// Makes this cursor a deep copy of <paramref name="o"/>, reusing every list, thread
        /// and frame it already owns: in steady state a lookahead snapshot allocates nothing.
        /// </summary>
        public void CopyFrom(Cursor o)
        {
            CallStack.CopyFrom(o.CallStack);
            Output.Clear();
            Output.AddRange(o.Output);
            Choices.Clear();
            Choices.AddRange(o.Choices);
            Eval.Clear();
            Eval.AddRange(o.Eval);
            Diverted = o.Diverted;
            DidSafeExit = o.DidSafeExit;
            TurnIndex = o.TurnIndex;
            Seed = o.Seed;
            PreviousRandom = o.PreviousRandom;
            Errors = o.Errors == null ? null : new List<string>(o.Errors);
            Warnings = o.Warnings == null ? null : new List<string>(o.Warnings);
            _beginStrings = o._beginStrings;
            Removals = o.Removals;
            _textDirty = o._textDirty;
            _tagsDirty = o._tagsDirty;
            _text = o._text;
            _tags = o._tags;
        }

        public bool InStringEvaluation => _beginStrings > 0;
        public bool HasError => Errors != null && Errors.Count > 0;

        // ---- output stream primitives (all mutations go through here to keep caches valid)

        public void Add(in OutItem item)
        {
            Output.Add(item);
            if (item.Kind == OutKind.BeginString) _beginStrings++;
            Dirty();
        }

        public void RemoveAt(int i)
        {
            if (Output[i].Kind == OutKind.BeginString) _beginStrings--;
            Output.RemoveAt(i);
            Removals++;
            Dirty();
        }

        public void PopOutput(int count)
        {
            for (int i = Output.Count - count; i < Output.Count; i++)
                if (Output[i].Kind == OutKind.BeginString) _beginStrings--;
            Output.RemoveRange(Output.Count - count, count);
            if (count > 0) Removals++;
            Dirty();
        }

        public void ResetOutput(List<OutItem> items = null)
        {
            Output.Clear();
            Removals++;
            _beginStrings = 0;
            if (items != null)
                foreach (var it in items) Add(it);
            Dirty();
        }

        void Dirty()
        {
            _textDirty = true;
            _tagsDirty = true;
        }

        public bool EndsInNewline
        {
            get
            {
                for (int i = Output.Count - 1; i >= 0; i--)
                {
                    var o = Output[i];
                    if (o.IsCommand) break;
                    if (o.Kind != OutKind.Text) continue;
                    if (o.IsNewline) return true;
                    if (o.IsNonWhitespace) break;
                }
                return false;
            }
        }

        /// <summary>True when a tag was opened and not yet closed.</summary>
        public bool InsideTag
        {
            get
            {
                for (int i = Output.Count - 1; i >= 0; i--)
                {
                    if (Output[i].Kind == OutKind.EndTag) return false;
                    if (Output[i].Kind == OutKind.BeginTag) return true;
                }
                return false;
            }
        }

        public bool ContainsContent
        {
            get
            {
                foreach (var o in Output)
                    if (o.Kind == OutKind.Text) return true;
                return false;
            }
        }

        public string CurrentText
        {
            get
            {
                if (!_textDirty) return _text;
                var raw = RawBuffer();
                bool inTag = false;
                foreach (var o in Output)
                {
                    if (!inTag && o.Kind == OutKind.Text) raw.Append(o.Text);
                    else if (o.Kind == OutKind.BeginTag) inTag = true;
                    else if (o.Kind == OutKind.EndTag) inTag = false;
                }
                _text = CleanWhitespace(raw);
                _textDirty = false;
                return _text;
            }
        }

        [ThreadStatic] static StringBuilder t_raw, t_clean;

        static StringBuilder RawBuffer()
        {
            var sb = t_raw ??= new StringBuilder(256);
            sb.Clear();
            return sb;
        }

        public List<string> CurrentTags
        {
            get
            {
                if (!_tagsDirty) return _tags;
                var tags = new List<string>();
                bool inTag = false;
                StringBuilder sb = null;
                // RawBuffer is free here: CurrentText is never computed while tags are built.
                foreach (var o in Output)
                {
                    switch (o.Kind)
                    {
                        case OutKind.BeginTag:
                            if (inTag && sb != null && sb.Length > 0) { tags.Add(CleanWhitespace(sb)); sb.Clear(); }
                            inTag = true;
                            break;
                        case OutKind.EndTag:
                            if (sb != null && sb.Length > 0) { tags.Add(CleanWhitespace(sb)); sb.Clear(); }
                            inTag = false;
                            break;
                        case OutKind.Text:
                            if (inTag) (sb ??= RawBuffer()).Append(o.Text);
                            break;
                        case OutKind.Tag:
                            if (!inTag && !string.IsNullOrEmpty(o.Text)) tags.Add(o.Text);
                            break;
                    }
                }
                if (sb != null && sb.Length > 0) tags.Add(CleanWhitespace(sb));
                _tags = tags;
                _tagsDirty = false;
                return _tags;
            }
        }

        /// <summary>
        /// Collapses runs of spaces/tabs to one space and strips them at line starts and
        /// before newlines — ink's HTML-like inline whitespace rule.
        /// </summary>
        public static string CleanWhitespace(string str)
        {
            var raw = RawBuffer();
            raw.Append(str);
            return CleanWhitespace(raw);
        }

        static string CleanWhitespace(StringBuilder str)
        {
            var sb = t_clean ??= new StringBuilder(256);
            sb.Clear();
            int wsStart = -1;
            int lineStart = 0;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                bool ws = c == ' ' || c == '\t';
                if (ws && wsStart == -1) wsStart = i;
                if (!ws)
                {
                    if (c != '\n' && wsStart > 0 && wsStart != lineStart) sb.Append(' ');
                    wsStart = -1;
                    sb.Append(c);
                }
                if (c == '\n') lineStart = i + 1;
            }
            return sb.Length == 0 ? "" : sb.ToString();
        }
    }

    /// <summary>
    /// The data part of the extended state — globals, visit counts, turn indices — indexed
    /// by dense slots/ids resolved at load time. While a glue lookahead is open every write
    /// is journaled as (slot, previous value); rolling back replays the journal in reverse,
    /// i.e. applies the inverse delta. Cost is O(writes since the newline), not O(state).
    /// </summary>
    sealed class Store
    {
        public const int NoTurn = int.MinValue;

        enum Kind : byte { Global, Visit, Turn }

        readonly struct Entry
        {
            public readonly Kind Kind;
            public readonly int Slot;
            public readonly Value OldValue;
            public readonly int OldInt;

            public Entry(Kind kind, int slot, Value oldValue, int oldInt)
            {
                Kind = kind;
                Slot = slot;
                OldValue = oldValue;
                OldInt = oldInt;
            }
        }

        public readonly Value[] Globals;
        public Value[] Defaults;
        public readonly int[] Visits;
        public readonly int[] Turns;

        readonly List<Entry> _journal = new List<Entry>(32);
        bool _recording;

        public Store(int globals, int containers)
        {
            Globals = new Value[globals];
            Defaults = new Value[globals];
            Visits = new int[containers];
            Turns = new int[containers];
            for (int i = 0; i < Turns.Length; i++) Turns[i] = NoTurn;
        }

        public void SetGlobal(int slot, Value v)
        {
            if (_recording) _journal.Add(new Entry(Kind.Global, slot, Globals[slot], 0));
            Globals[slot] = v;
        }

        public void IncrementVisit(int id)
        {
            if (_recording) _journal.Add(new Entry(Kind.Visit, id, default, Visits[id]));
            Visits[id]++;
        }

        public void SetTurn(int id, int turn)
        {
            if (_recording) _journal.Add(new Entry(Kind.Turn, id, default, Turns[id]));
            Turns[id] = turn;
        }

        public void BeginRecording()
        {
            _journal.Clear();
            _recording = true;
        }

        /// <summary>Keeps every write made since <see cref="BeginRecording"/>.</summary>
        public void Commit()
        {
            _journal.Clear();
            _recording = false;
        }

        /// <summary>Undoes every write made since <see cref="BeginRecording"/>.</summary>
        public void Rollback()
        {
            for (int i = _journal.Count - 1; i >= 0; i--)
            {
                var e = _journal[i];
                switch (e.Kind)
                {
                    case Kind.Global: Globals[e.Slot] = e.OldValue; break;
                    case Kind.Visit: Visits[e.Slot] = e.OldInt; break;
                    case Kind.Turn: Turns[e.Slot] = e.OldInt; break;
                }
            }
            _journal.Clear();
            _recording = false;
        }
    }
}
