using System;
using System.Collections.Generic;
using System.Text;

namespace LegacyThroughTime.Narrative.Ink
{
    /// <summary>A choice as offered to the player.</summary>
    public sealed class InkChoice
    {
        public int Index { get; }
        public string Text { get; }
        public IReadOnlyList<string> Tags { get; }
        public string SourcePath { get; }
        public string TargetPath { get; }

        internal InkChoice(int index, ChoiceRecord r)
        {
            Index = index;
            Text = r.Text;
            Tags = r.Tags ?? (IReadOnlyList<string>)Array.Empty<string>();
            SourcePath = r.SourcePath;
            TargetPath = r.Target?.Path;
        }
    }

    /// <summary>Host function callable from ink via <c>EXTERNAL</c>.</summary>
    public delegate object ExternalFunction(object[] args);

    /// <summary>
    /// Executes a compiled <see cref="InkStory"/> with the exact observable semantics of the
    /// reference ink runtime (text, tags, choices, variables, randomness).
    ///
    /// Formally it is an extended finite-state machine with a pushdown store:
    /// the control state is the program counter of the current frame; the extended state is
    /// (call stack, evaluation stack, output stream, globals, visit/turn counts, turn, seed).
    /// <see cref="Step"/> is the transition function δ; <see cref="Continue"/> iterates δ to the
    /// next line boundary. Deciding where a line ends needs one-line lookahead (a later glue
    /// <c>&lt;&gt;</c> can cancel a newline), so at every newline the runner takes a snapshot:
    /// the control part is copied, the data part switches to journaling, and the lookahead is
    /// either committed or rolled back exactly.
    /// </summary>
    public sealed class InkRunner
    {
        readonly InkStory _story;
        readonly Store _store;
        Cursor _c;

        // Lookahead snapshot (at most one at a time) and a recycled cursor to copy it into.
        Cursor _snapshot;
        Cursor _spare;
        int _snapshotOutputCount;
        int _snapshotRemovals;
        bool _snapshotInsideTag;
        bool _sawLookaheadUnsafeFunctionAfterNewline;

        readonly Dictionary<string, (ExternalFunction fn, bool lookaheadSafe)> _externals =
            new Dictionary<string, (ExternalFunction, bool)>();

        // Scratch for VisitChangedContainersDueToDivert: generation stamps per container.
        readonly int[] _stamp;
        int _stampGeneration;
        readonly List<string> _splitScratch = new List<string>(4);

        /// <summary>Called for each error/warning after a Continue. When null, errors throw.</summary>
        public Action<string, bool> OnError;

        /// <summary>Run the ink fallback of an unbound EXTERNAL function instead of failing.</summary>
        public bool AllowExternalFunctionFallbacks { get; set; } = true;

        public InkStory Story => _story;

        public InkRunner(InkStory story, int? seed = null)
        {
            _story = story ?? throw new ArgumentNullException(nameof(story));
            _store = new Store(story.GlobalNames.Length, story.Containers.Length);
            _stamp = new int[story.Containers.Length];
            _c = NewCursor(seed);
            ResetGlobals();
        }

        Cursor NewCursor(int? seed) => new Cursor(_story.Root)
        {
            // Same default as ink: a small time-derived seed; tests pass an explicit one.
            Seed = seed ?? InkRandom.First(DateTime.UtcNow.Millisecond) % 100,
        };

        // =====================================================================
        // Public surface
        // =====================================================================

        public bool CanContinue => !_c.CallStack.Current.Pointer.IsNull && !_c.HasError;
        public string CurrentText => _c.CurrentText;
        public IReadOnlyList<string> CurrentTags => _c.CurrentTags;
        public bool HasError => _c.HasError;
        public int CurrentTurnIndex => _c.TurnIndex;

        public int Seed
        {
            get => _c.Seed;
            set
            {
                _c.Seed = value;
                _c.PreviousRandom = 0;
            }
        }

        /// <summary>Visible choices (invisible defaults are filtered out and indices renumbered).</summary>
        public IReadOnlyList<InkChoice> CurrentChoices
        {
            get
            {
                var result = new List<InkChoice>();
                if (CanContinue) return result;
                foreach (var r in _c.Choices)
                    if (!r.IsInvisibleDefault) result.Add(new InkChoice(result.Count, r));
                return result;
            }
        }

        /// <summary>Runs to the end of the next line and returns it (with its trailing newline).</summary>
        public string Continue()
        {
            ContinueInternal();
            return CurrentText;
        }

        public string ContinueMaximally()
        {
            var sb = new StringBuilder();
            while (CanContinue) sb.Append(Continue());
            return sb.ToString();
        }

        public void ChooseChoiceIndex(int index)
        {
            var visible = VisibleChoiceRecords();
            if (index < 0 || index >= visible.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"choice {index} out of range (0..{visible.Count - 1})");
            var choice = visible[index];
            _c.CallStack.CurrentThread = choice.Thread;
            ChoosePath(new Pointer(choice.Target, -1), true);
        }

        /// <summary>Jumps to <c>knot</c> or <c>knot.stitch</c>, optionally passing arguments.</summary>
        public void ChoosePathString(string path, bool resetCallstack = true, params object[] args)
        {
            if (resetCallstack) ForceEnd();
            else if (_c.CallStack.Current.Type == PushPop.Function)
                throw new InvalidOperationException("Story was running a function when you called ChoosePathString(" + path + ")");
            PushHostArguments(args);
            ChoosePath(_story.TargetFor(path).Pointer, true);
        }

        public object EvaluateFunction(string name, params object[] args) => EvaluateFunction(name, out _, args);

        public object EvaluateFunction(string name, out string textOutput, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Function name is empty", nameof(name));
            var fn = _story.KnotNamed(name) ?? throw new ArgumentException("Function doesn't exist: '" + name + "'", nameof(name));

            var outputBefore = new List<OutItem>(_c.Output);
            _c.ResetOutput();

            _c.CallStack.Push(PushPop.FunctionEvaluationFromGame, _c.Eval.Count);
            _c.CallStack.Current.Pointer = Pointer.StartOf(fn);
            PushHostArguments(args);

            var sb = new StringBuilder();
            while (CanContinue) sb.Append(Continue());
            textOutput = sb.ToString();

            _c.ResetOutput(outputBefore);

            if (_c.CallStack.Current.Type != PushPop.FunctionEvaluationFromGame)
                throw new StoryException("Expected external function evaluation to be complete.");
            int height = _c.CallStack.Current.EvalHeightWhenPushed;
            var returned = Value.None;
            while (_c.Eval.Count > height)
            {
                var v = Pop();
                if (returned.IsNone) returned = v;
            }
            PopCallstack(PushPop.FunctionEvaluationFromGame);
            return returned.Kind == ValueKind.Void ? null : returned.ToHost();
        }

        public void BindExternalFunction(string name, ExternalFunction fn, bool lookaheadSafe = false)
        {
            if (fn == null) throw new ArgumentNullException(nameof(fn));
            if (_externals.ContainsKey(name)) throw new InvalidOperationException("Function '" + name + "' has already been bound.");
            _externals[name] = (fn, lookaheadSafe);
        }

        public void UnbindExternalFunction(string name) => _externals.Remove(name);

        // ---- variables (host view)

        public object GetVariable(string name)
        {
            int slot = _story.GlobalSlot(name);
            return slot < 0 ? null : _store.Globals[slot].ToHost();
        }

        public void SetVariable(string name, object value)
        {
            int slot = _story.GlobalSlot(name);
            if (slot < 0) throw new StoryException("Cannot assign to a variable (" + name + ") that hasn't been declared in the story");
            var v = Value.FromHost(value);
            if (v.IsNone) throw new ArgumentException("Invalid value passed to variable '" + name + "': " + value);
            SetGlobal(slot, v);
        }

        /// <summary>How many times the knot/stitch/label at <paramref name="path"/> was visited.</summary>
        public int VisitCount(string path)
        {
            var c = _story.ContainerAt(path) ?? throw new ArgumentException("Content at path not found: " + path);
            return _store.Visits[c.Id];
        }

        /// <summary>Serialises the whole play-through state. Call between Continue()s.</summary>
        public string SaveState() => InkSave.Write(this);

        /// <summary>Restores a state produced by <see cref="SaveState"/> for the same (or an edited) story.</summary>
        public void LoadState(string json) => InkSave.Read(this, json);

        internal void LoadState(Dictionary<string, object> parsed) => InkSave.Read(this, parsed);

        internal Store Store => _store;
        internal Cursor Cursor => _c;

        // =====================================================================
        // Continue: iterate δ up to a line boundary, with glue lookahead
        // =====================================================================

        void ContinueInternal()
        {
            if (!CanContinue) throw new InvalidOperationException("Can't continue - should check CanContinue before calling Continue");

            _c.DidSafeExit = false;
            _c.ResetOutput();

            bool endsInNewline = false;
            _sawLookaheadUnsafeFunctionAfterNewline = false;
            do
            {
                try
                {
                    endsInNewline = ContinueSingleStep();
                }
                catch (InkException e)
                {
                    AddError(e.Message, false);
                    break;
                }
                if (endsInNewline) break;
            } while (CanContinue);

            if (endsInNewline || !CanContinue)
            {
                if (_snapshot != null) RestoreSnapshot();

                if (!CanContinue)
                {
                    if (_c.CallStack.CanPopThread)
                        AddError("Thread available to pop, threads should always be flat by the end of evaluation?", false);
                    if (_c.Choices.Count == 0 && !_c.DidSafeExit)
                    {
                        if (_c.CallStack.CanPop(PushPop.Tunnel))
                            AddError("unexpectedly reached end of content. Do you need a '->->' to return from a tunnel?", false);
                        else if (_c.CallStack.CanPop(PushPop.Function))
                            AddError("unexpectedly reached end of content. Do you need a '~ return'?", false);
                        else if (!_c.CallStack.CanPopAny)
                            AddError("ran out of content. Do you need a '-> DONE' or '-> END'?", false);
                        else
                            AddError("unexpectedly reached end of content for unknown reason. Please debug compiler!", false);
                    }
                }
                _c.DidSafeExit = false;
                _sawLookaheadUnsafeFunctionAfterNewline = false;
            }

            ReportErrors();
        }

        bool ContinueSingleStep()
        {
            Step();

            if (!CanContinue && !_c.CallStack.ElementIsEvaluateFromGame)
                TryFollowDefaultInvisibleChoice();

            // No snapshots inside string evaluation (choice text building).
            if (_c.InStringEvaluation) return false;

            if (_snapshot != null)
            {
                var change = NewlineChangeSinceSnapshot();
                if (change == OutputChange.ExtendedBeyondNewline || _sawLookaheadUnsafeFunctionAfterNewline)
                {
                    // The newline really ended the line: rewind everything done after it.
                    RestoreSnapshot();
                    return true;
                }
                if (change == OutputChange.NewlineRemoved) DiscardSnapshot(); // glue ate it
            }

            if (_c.EndsInNewline)
            {
                if (CanContinue)
                {
                    if (_snapshot == null) TakeSnapshot();
                }
                else DiscardSnapshot();
            }
            return false;
        }

        enum OutputChange { NoChange, ExtendedBeyondNewline, NewlineRemoved, Unknown }

        /// <summary>
        /// Has the newline seen at the snapshot been cancelled (glue), confirmed (new visible
        /// text or a new tag), or neither yet? Fast path: if nothing was removed from the
        /// output since the snapshot, the stream is snapshot ++ suffix, and because text
        /// cleaning never alters a prefix that ends in '\n', the answer depends on the suffix
        /// only — O(|suffix|) and allocation-free. Anything involving tags or removals falls
        /// back to the reference definition on fully rebuilt text.
        /// </summary>
        OutputChange NewlineChangeSinceSnapshot()
        {
            if (_c.Removals == _snapshotRemovals && !_snapshotInsideTag)
            {
                var output = _c.Output;
                var change = OutputChange.NoChange;
                for (int i = _snapshotOutputCount; i < output.Count; i++)
                {
                    var o = output[i];
                    if (o.Kind == OutKind.BeginTag || o.Kind == OutKind.EndTag || o.Kind == OutKind.Tag)
                    {
                        change = OutputChange.Unknown;
                        break;
                    }
                    if (o.Kind == OutKind.Text && o.IsNonWhitespace) change = OutputChange.ExtendedBeyondNewline;
                }
                if (change != OutputChange.Unknown) return change;
            }
            return NewlineChange(_snapshot.CurrentText, _c.CurrentText, _snapshot.CurrentTags.Count, _c.CurrentTags.Count);
        }

        static OutputChange NewlineChange(string prev, string curr, int prevTags, int currTags)
        {
            bool newlineStillExists = curr.Length >= prev.Length && prev.Length > 0 && curr[prev.Length - 1] == '\n';
            if (prevTags == currTags && prev.Length == curr.Length && newlineStillExists) return OutputChange.NoChange;
            if (!newlineStillExists) return OutputChange.NewlineRemoved;
            if (currTags > prevTags) return OutputChange.ExtendedBeyondNewline;
            for (int i = prev.Length; i < curr.Length; i++)
                if (curr[i] != ' ' && curr[i] != '\t') return OutputChange.ExtendedBeyondNewline;
            return OutputChange.NoChange;
        }

        void TakeSnapshot()
        {
            _snapshotOutputCount = _c.Output.Count;
            _snapshotRemovals = _c.Removals;
            _snapshotInsideTag = _c.InsideTag;
            _snapshot = _spare ?? new Cursor(_story.Root);
            _spare = null;
            _snapshot.CopyFrom(_c);
            _store.BeginRecording();
        }

        void RestoreSnapshot()
        {
            _spare = _c;
            _c = _snapshot;
            _snapshot = null;
            _store.Rollback();
        }

        void DiscardSnapshot()
        {
            if (_snapshot == null) return;
            _spare = _snapshot;
            _snapshot = null;
            _store.Commit();
        }

        // =====================================================================
        // δ — one transition
        // =====================================================================

        Pointer CurrentPointer
        {
            get => _c.CallStack.Current.Pointer;
            set => _c.CallStack.Current.Pointer = value;
        }

        Pointer PreviousPointer
        {
            get => _c.CallStack.CurrentThread.Previous;
            set => _c.CallStack.CurrentThread.Previous = value;
        }

        bool InExpression
        {
            get => _c.CallStack.Current.InExpression;
            set => _c.CallStack.Current.InExpression = value;
        }

        void Step()
        {
            var pointer = CurrentPointer;
            if (pointer.IsNull) return;

            // Descend into containers, counting each one entered at its start.
            var enter = pointer.Resolve() as Container;
            while (enter != null)
            {
                VisitContainer(enter, true);
                if (enter.Content.Length == 0) break;
                pointer = Pointer.StartOf(enter);
                enter = pointer.Resolve() as Container;
            }
            CurrentPointer = pointer;

            var node = pointer.Resolve();
            bool isFlow = PerformLogicAndFlowControl(node);

            if (CurrentPointer.IsNull) return;

            if (!isFlow && node != null)
            {
                switch (node.Op)
                {
                    case Op.ChoicePoint:
                    {
                        var choice = ProcessChoice((ChoicePointNode)node);
                        if (choice != null) _c.Choices.Add(choice);
                        break;
                    }
                    case Op.Container:
                        break;
                    case Op.Text:
                    {
                        var text = ((TextNode)node).Text;
                        if (InExpression) Push(Value.String(text));
                        else PushToOutput(text);
                        break;
                    }
                    case Op.Glue:
                        if (!InExpression) PushItem(OutItem.Glue);
                        break;
                    case Op.Tag:
                    {
                        var tag = ((TagNode)node).Text;
                        if (InExpression) Push(Value.Tag(tag));
                        else PushItem(OutItem.OfTag(tag));
                        break;
                    }
                    case Op.Literal:
                    {
                        var v = ((LiteralNode)node).Value;
                        if (v.Kind == ValueKind.VariablePointer && v.ContextIndex == -1)
                            v = Value.Pointer(v.Str, _c.CallStack.ContextForVariableNamed(v.Str));
                        if (InExpression) Push(v);
                        else PushItem(OutItem.Other);
                        break;
                    }
                }
            }

            NextContent();

            // A thread starts after the pointer moves on, so returning from it resumes here.
            if (node is CommandNode cmd && cmd.Cmd == Cmd.StartThread) _c.CallStack.PushThread();
        }

        void VisitContainer(Container c, bool atStart)
        {
            if (c.CountsAtStartOnly && !atStart) return;
            if (c.CountsVisits) _store.IncrementVisit(c.Id);
            if (c.CountsTurns) _store.SetTurn(c.Id, _c.TurnIndex);
        }

        /// <summary>After a jump: count every container newly entered on the way down to the target.</summary>
        void VisitChangedContainersDueToDivert()
        {
            var previous = PreviousPointer;
            var pointer = CurrentPointer;
            if (pointer.IsNull || pointer.Index == -1) return;

            int gen = ++_stampGeneration;
            if (!previous.IsNull)
            {
                var ancestor = previous.Resolve() as Container ?? previous.Container;
                for (; ancestor != null; ancestor = ancestor.Parent) _stamp[ancestor.Id] = gen;
            }

            var child = pointer.Resolve();
            if (child == null) return;

            var container = child.Parent;
            bool allAtStart = true;
            while (container != null && (_stamp[container.Id] != gen || container.CountsAtStartOnly))
            {
                bool atStart = container.Content.Length > 0 && child == container.Content[0] && allAtStart;
                if (!atStart) allAtStart = false;
                VisitContainer(container, atStart);
                child = container;
                container = container.Parent;
            }
        }

        ChoiceRecord ProcessChoice(ChoicePointNode cp)
        {
            bool show = true;
            if (cp.HasCondition && !Pop().IsTruthy) show = false;

            string startText = "", choiceOnlyText = "";
            List<string> tags = null;
            if (cp.HasChoiceOnlyContent) choiceOnlyText = PopChoiceStringAndTags(ref tags);
            if (cp.HasStartContent) startText = PopChoiceStringAndTags(ref tags);

            if (cp.OnceOnly && VisitCountFor(cp.Target) > 0) show = false;
            if (!show) return null;

            return new ChoiceRecord
            {
                Target = cp.Target,
                SourcePath = cp.PathString,
                IsInvisibleDefault = cp.IsInvisibleDefault,
                Tags = tags,
                // Remember the thread this choice lives in: it may be discarded before the player picks.
                Thread = _c.CallStack.ForkThread(),
                Text = (startText + choiceOnlyText).Trim(' ', '\t'),
            };
        }

        string PopChoiceStringAndTags(ref List<string> tags)
        {
            var text = Pop().Str;
            while (_c.Eval.Count > 0 && Peek().Kind == ValueKind.Tag)
                (tags ??= new List<string>()).Insert(0, Pop().Str);
            return text;
        }

        bool PerformLogicAndFlowControl(Node node)
        {
            if (node == null) return false;
            switch (node.Op)
            {
                case Op.Divert: Divert((DivertNode)node); return true;
                case Op.Command: Command((CommandNode)node); return true;

                case Op.VarAssign:
                    Assign((VarAssignNode)node, Pop());
                    return true;

                case Op.VarRef:
                {
                    var r = (VarRefNode)node;
                    var v = GetVariable(r.Name, r.GlobalSlot, r.ListItem, -1);
                    if (v.IsNone)
                    {
                        Warning("Variable not found: '" + r.Name + "'. Using default value of 0 (false). This can happen with temporary variables if the declaration hasn't yet been hit. Globals are always given a default value on load if a value doesn't exist in the save state.");
                        v = Value.Int(0);
                    }
                    Push(v);
                    return true;
                }

                case Op.ReadCount:
                    Push(Value.Int(VisitCountFor(((ReadCountNode)node).Target)));
                    return true;

                case Op.Native:
                {
                    var fn = (NativeNode)node;
                    if (fn.Arity == 1) Push(NativeOps.Call(fn, Pop(), _story.Lists));
                    else
                    {
                        var b = Pop();
                        var a = Pop();
                        Push(NativeOps.Call(fn, a, b, _story.Lists));
                    }
                    return true;
                }
            }
            return false;
        }

        void Divert(DivertNode d)
        {
            if (d.IsConditional && !Pop().IsTruthy) return;

            if (d.VariableName != null)
            {
                var v = GetVariable(d.VariableName, _story.GlobalSlot(d.VariableName), null, -1);
                if (v.IsNone)
                    throw new InkException("Tried to divert using a target from a variable that could not be found (" + d.VariableName + ")");
                if (v.Kind != ValueKind.DivertTarget)
                {
                    var msg = "Tried to divert to a target from a variable, but the variable (" + d.VariableName + ") didn't contain a divert target, it ";
                    msg += v.Kind == ValueKind.Int && v.I == 0 ? "was empty/null (the value 0)." : "contained '" + v + "'.";
                    throw new InkException(msg);
                }
                _c.Diverted = v.Target.Pointer;
            }
            else if (d.IsExternal)
            {
                CallExternalFunction(d.TargetPath, d.ExternalArgs);
                return;
            }
            else _c.Diverted = d.Target;

            if (d.PushesToStack) _c.CallStack.Push(d.StackType, 0, _c.Output.Count);

            if (_c.Diverted.IsNull)
                throw new InkException("Divert resolution failed: " + (d.TargetPath ?? d.VariableName));
        }

        void Command(CommandNode node)
        {
            switch (node.Cmd)
            {
                case Cmd.EvalStart: InExpression = true; break;
                case Cmd.EvalEnd: InExpression = false; break;

                case Cmd.EvalOutput:
                    if (_c.Eval.Count > 0)
                    {
                        var v = Pop();
                        if (v.Kind != ValueKind.Void) PushToOutput(v.ToString());
                    }
                    break;

                case Cmd.NoOp: break;
                case Cmd.Duplicate: Push(Peek()); break;
                case Cmd.PopEvaluatedValue: Pop(); break;

                case Cmd.PopFunction:
                case Cmd.PopTunnel:
                {
                    var type = node.Cmd == Cmd.PopFunction ? PushPop.Function : PushPop.Tunnel;
                    DivertTarget overrideTarget = null;
                    if (type == PushPop.Tunnel)
                    {
                        var popped = Pop();
                        if (popped.Kind == ValueKind.DivertTarget) overrideTarget = popped.Target;
                        else if (popped.Kind != ValueKind.Void) throw new InkException("Expected void if ->-> doesn't override target");
                    }

                    if (TryExitFunctionEvaluationFromGame()) break;

                    var cs = _c.CallStack;
                    if (cs.Current.Type != type || !cs.CanPopAny)
                    {
                        string Name(PushPop t) => t == PushPop.Function ? "function return statement (~ return)" : "tunnel onwards statement (->->)";
                        var expected = cs.CanPopAny ? Name(cs.Current.Type) : "end of flow (-> END or choice)";
                        throw new InkException("Found " + Name(type) + ", when expected " + expected);
                    }
                    PopCallstack(null);
                    if (overrideTarget != null) _c.Diverted = overrideTarget.Pointer;
                    break;
                }

                case Cmd.BeginString:
                    PushItem(OutItem.BeginString);
                    InExpression = false;
                    break;

                case Cmd.BeginTag:
                    PushItem(OutItem.BeginTag);
                    break;

                case Cmd.EndTag:
                    if (_c.InStringEvaluation)
                    {
                        // A tag on a choice: fold its text into a Tag value for ProcessChoice.
                        var parts = new List<string>();
                        int consumed = 0;
                        for (int i = _c.Output.Count - 1; i >= 0; --i)
                        {
                            var o = _c.Output[i];
                            consumed++;
                            if (o.IsCommand)
                            {
                                if (o.Kind == OutKind.BeginTag) break;
                                throw new InkException("Unexpected ControlCommand while extracting tag from choice");
                            }
                            if (o.Kind == OutKind.Text) parts.Add(o.Text);
                        }
                        _c.PopOutput(consumed);
                        Push(Value.Tag(Cursor.CleanWhitespace(JoinReversed(parts))));
                    }
                    else PushItem(OutItem.EndTag);
                    break;

                case Cmd.EndString:
                {
                    var parts = new List<string>();
                    List<OutItem> retained = null;
                    int consumed = 0;
                    for (int i = _c.Output.Count - 1; i >= 0; --i)
                    {
                        var o = _c.Output[i];
                        consumed++;
                        if (o.Kind == OutKind.BeginString) break;
                        if (o.Kind == OutKind.Tag) (retained ??= new List<OutItem>()).Add(o);
                        if (o.Kind == OutKind.Text) parts.Add(o.Text);
                    }
                    _c.PopOutput(consumed);
                    if (retained != null)
                        for (int i = retained.Count - 1; i >= 0; i--) PushItem(retained[i]);
                    InExpression = true;
                    Push(Value.String(JoinReversed(parts)));
                    break;
                }

                case Cmd.ChoiceCount: Push(Value.Int(_c.Choices.Count)); break;
                case Cmd.Turns: Push(Value.Int(_c.TurnIndex + 1)); break;

                case Cmd.TurnsSince:
                case Cmd.ReadCount:
                {
                    var target = Pop();
                    if (target.Kind != ValueKind.DivertTarget)
                    {
                        var note = target.Kind == ValueKind.Int ? ". Did you accidentally pass a read count ('knot_name') instead of a target ('-> knot_name')?" : "";
                        throw new InkException("TURNS_SINCE expected a divert target (knot, stitch, label name), but saw " + target + note);
                    }
                    var container = target.Target.Container;
                    int count;
                    if (container != null)
                        count = node.Cmd == Cmd.TurnsSince ? TurnsSinceFor(container) : VisitCountFor(container);
                    else
                    {
                        count = node.Cmd == Cmd.TurnsSince ? -1 : 0;
                        Warning("Failed to find container for " + node.Cmd + " lookup at " + target.Target.Path);
                    }
                    Push(Value.Int(count));
                    break;
                }

                case Cmd.Random:
                {
                    var max = Pop();
                    var min = Pop();
                    if (min.Kind != ValueKind.Int) throw new InkException("Invalid value for minimum parameter of RANDOM(min, max)");
                    if (max.Kind != ValueKind.Int) throw new InkException("Invalid value for maximum parameter of RANDOM(min, max)");
                    long range = (long)max.I - min.I + 1;
                    if (range > int.MaxValue) throw new InkException("RANDOM was called with a range that exceeds the size that ink numbers can use.");
                    if (range <= 0) throw new InkException("RANDOM was called with minimum as " + min.I + " and maximum as " + max.I + ". The maximum must be larger");
                    int next = InkRandom.First(unchecked(_c.Seed + _c.PreviousRandom));
                    Push(Value.Int(next % (int)range + min.I));
                    _c.PreviousRandom = next;
                    break;
                }

                case Cmd.SeedRandom:
                {
                    var seed = Pop();
                    if (seed.Kind != ValueKind.Int) throw new InkException("Invalid value passed to SEED_RANDOM");
                    _c.Seed = seed.I;
                    _c.PreviousRandom = 0;
                    Push(Value.Void);
                    break;
                }

                case Cmd.VisitIndex:
                    Push(Value.Int(VisitCountFor(CurrentPointer.Container) - 1));
                    break;

                case Cmd.SequenceShuffleIndex:
                    Push(Value.Int(NextSequenceShuffleIndex()));
                    break;

                case Cmd.StartThread: break; // handled after the pointer advances

                case Cmd.Done:
                    if (_c.CallStack.CanPopThread) _c.CallStack.PopThread();
                    else
                    {
                        _c.DidSafeExit = true;
                        CurrentPointer = Pointer.Null;
                    }
                    break;

                case Cmd.End: ForceEnd(); break;

                case Cmd.ListFromInt:
                {
                    var n = Pop();
                    var listName = Pop();
                    if (n.Kind != ValueKind.Int) throw new InkException("Passed non-integer when creating a list element from a numerical value.");
                    if (!_story.Lists.TryGet(listName.Str, out var def)) throw new InkException("Failed to find LIST called " + listName.Str);
                    Push(Value.List(def.TryGetItemWithValue(n.I, out var item) ? InkList.Single(item, n.I) : new InkList()));
                    break;
                }

                case Cmd.ListRange:
                {
                    var max = Pop();
                    var min = Pop();
                    var target = Pop();
                    if (target.Kind != ValueKind.List || min.IsNone || max.IsNone)
                        throw new InkException("Expected list, minimum and maximum for LIST_RANGE");
                    Push(Value.List(target.ListValue.SubRange(min, max)));
                    break;
                }

                case Cmd.ListRandom:
                {
                    var v = Pop();
                    if (v.Kind != ValueKind.List) throw new InkException("Expected list for LIST_RANDOM");
                    var list = v.ListValue;
                    if (list.Count == 0)
                    {
                        Push(Value.List(new InkList()));
                        break;
                    }
                    int next = InkRandom.First(unchecked(_c.Seed + _c.PreviousRandom));
                    int index = next % list.Count;
                    InkList picked = null;
                    foreach (var kv in list)
                    {
                        if (index-- > 0) continue;
                        picked = InkList.Single(kv.Key, kv.Value);
                        break;
                    }
                    Push(Value.List(picked));
                    _c.PreviousRandom = next;
                    break;
                }

                default:
                    throw new InkException("unhandled ControlCommand: " + node.Cmd);
            }
        }

        static string JoinReversed(List<string> parts)
        {
            if (parts.Count == 0) return "";
            if (parts.Count == 1) return parts[0];
            var sb = new StringBuilder();
            for (int i = parts.Count - 1; i >= 0; i--) sb.Append(parts[i]);
            return sb.ToString();
        }

        int NextSequenceShuffleIndex()
        {
            var countVal = Pop();
            if (countVal.Kind != ValueKind.Int) throw new InkException("expected number of elements in sequence for shuffle index");
            int count = countVal.I;
            var container = CurrentPointer.Container;
            int seqCount = Pop().I;
            int loopIndex = seqCount / count;
            int iterationIndex = seqCount % count;

            // Deterministic per (sequence, loop, story seed): same shuffle on every revisit.
            int hash = 0;
            foreach (char ch in container.Path) hash += ch;
            var rng = new InkRandom(unchecked(hash + loopIndex + _c.Seed));

            var unpicked = new List<int>(count);
            for (int i = 0; i < count; ++i) unpicked.Add(i);
            for (int i = 0; i <= iterationIndex; ++i)
            {
                int chosen = rng.Next() % unpicked.Count;
                int chosenIndex = unpicked[chosen];
                unpicked.RemoveAt(chosen);
                if (i == iterationIndex) return chosenIndex;
            }
            throw new InkException("Should never reach here");
        }

        void NextContent()
        {
            PreviousPointer = CurrentPointer;

            if (!_c.Diverted.IsNull)
            {
                CurrentPointer = _c.Diverted;
                _c.Diverted = Pointer.Null;
                VisitChangedContainersDueToDivert();
                if (!CurrentPointer.IsNull) return;
                // A divert to the very end of a container falls through to the increment.
            }

            if (IncrementContentPointer()) return;

            // Ran off the end: leave a function, or finish a thread.
            bool didPop = false;
            var cs = _c.CallStack;
            if (cs.CanPop(PushPop.Function))
            {
                PopCallstack(PushPop.Function);
                if (InExpression) Push(Value.Void); // a function that returned nothing
                didPop = true;
            }
            else if (cs.CanPopThread)
            {
                cs.PopThread();
                didPop = true;
            }
            else TryExitFunctionEvaluationFromGame();

            if (didPop && !CurrentPointer.IsNull) NextContent();
        }

        bool IncrementContentPointer()
        {
            var frame = _c.CallStack.Current;
            var container = frame.Pointer.Container;
            int index = frame.Pointer.Index + 1;
            bool ok = true;
            while (index >= container.Content.Length)
            {
                ok = false;
                var parent = container.Parent;
                if (parent == null || container.Index < 0) break;
                index = container.Index + 1;
                container = parent;
                ok = true;
            }
            frame.Pointer = ok ? new Pointer(container, index) : Pointer.Null;
            return ok;
        }

        bool TryFollowDefaultInvisibleChoice()
        {
            var all = _c.Choices;
            ChoiceRecord first = null;
            int invisible = 0;
            foreach (var c in all)
            {
                if (!c.IsInvisibleDefault) continue;
                first ??= c;
                invisible++;
            }
            if (invisible == 0 || all.Count > invisible) return false;

            _c.CallStack.CurrentThread = first.Thread;
            // Keep the choice's own thread intact in case the lookahead is rewound.
            if (_snapshot != null) _c.CallStack.CurrentThread = _c.CallStack.ForkThread();
            ChoosePath(new Pointer(first.Target, -1), false);
            return true;
        }

        void ChoosePath(Pointer target, bool incrementTurn)
        {
            _c.Choices.Clear();
            if (!target.IsNull && target.Index == -1) target = target.WithIndex(0);
            CurrentPointer = target;
            if (incrementTurn) _c.TurnIndex++;
            VisitChangedContainersDueToDivert();
        }

        List<ChoiceRecord> VisibleChoiceRecords()
        {
            var visible = new List<ChoiceRecord>();
            if (CanContinue) return visible;
            foreach (var c in _c.Choices)
                if (!c.IsInvisibleDefault) visible.Add(c);
            return visible;
        }

        // =====================================================================
        // Output stream: whitespace, glue and function trimming rules
        // =====================================================================

        void PushToOutput(string text)
        {
            if (text == "\n")
            {
                PushItem(OutItem.Newline);
                return;
            }
            if (SplitHeadTailWhitespace(text, _splitScratch))
            {
                foreach (var part in _splitScratch) PushItem(OutItem.OfText(part));
                return;
            }
            PushItem(OutItem.OfText(text));
        }

        /// <summary>
        /// Splits leading/trailing newlines (with their surrounding spaces) off a string so
        /// glue can later remove them one by one. Returns false when nothing needs splitting.
        /// </summary>
        static bool SplitHeadTailWhitespace(string str, List<string> parts)
        {
            int headFirst = -1, headLast = -1;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c == '\n')
                {
                    if (headFirst == -1) headFirst = i;
                    headLast = i;
                }
                else if (c != ' ' && c != '\t') break;
            }

            int tailLast = -1, tailFirst = -1;
            for (int i = str.Length - 1; i >= 0; i--)
            {
                char c = str[i];
                if (c == '\n')
                {
                    if (tailLast == -1) tailLast = i;
                    tailFirst = i;
                }
                else if (c != ' ' && c != '\t') break;
            }

            if (headFirst == -1 && tailLast == -1) return false;

            parts.Clear();
            int innerStart = 0, innerEnd = str.Length;
            if (headFirst != -1)
            {
                if (headFirst > 0) parts.Add(str.Substring(0, headFirst));
                parts.Add("\n");
                innerStart = headLast + 1;
            }
            if (tailLast != -1) innerEnd = tailFirst;
            if (innerEnd > innerStart) parts.Add(str.Substring(innerStart, innerEnd - innerStart));
            if (tailLast != -1 && tailFirst > headLast)
            {
                parts.Add("\n");
                if (tailLast < str.Length - 1) parts.Add(str.Substring(tailLast + 1, str.Length - tailLast - 1));
            }
            return true;
        }

        void PushItem(OutItem item)
        {
            var output = _c.Output;
            bool include = true;

            if (item.Kind == OutKind.Glue)
            {
                TrimNewlinesFromOutput();
            }
            else if (item.Kind == OutKind.Text)
            {
                int functionTrimIndex = -1;
                var frame = _c.CallStack.Current;
                if (frame.Type == PushPop.Function) functionTrimIndex = frame.FunctionStartInOutput;

                int glueTrimIndex = -1;
                for (int i = output.Count - 1; i >= 0; i--)
                {
                    var o = output[i];
                    if (o.Kind == OutKind.Glue)
                    {
                        glueTrimIndex = i;
                        break;
                    }
                    if (o.Kind == OutKind.BeginString)
                    {
                        if (i >= functionTrimIndex) functionTrimIndex = -1;
                        break;
                    }
                }

                int trimIndex = glueTrimIndex != -1 && functionTrimIndex != -1
                    ? Math.Min(functionTrimIndex, glueTrimIndex)
                    : glueTrimIndex != -1 ? glueTrimIndex : functionTrimIndex;

                if (trimIndex != -1)
                {
                    if (item.IsNewline) include = false;
                    else if (item.IsNonWhitespace)
                    {
                        if (glueTrimIndex > -1) RemoveExistingGlue();
                        if (functionTrimIndex > -1)
                        {
                            // Real text seen: stop trimming function-start whitespace all the way up.
                            var frames = _c.CallStack.Frames;
                            for (int i = frames.Count - 1; i >= 0; i--)
                            {
                                if (frames[i].Type != PushPop.Function) break;
                                frames[i].FunctionStartInOutput = -1;
                            }
                        }
                    }
                }
                else if (item.IsNewline)
                {
                    // Never lead with a newline, never double one.
                    if (_c.EndsInNewline || !_c.ContainsContent) include = false;
                }
            }

            if (include) _c.Add(item);
        }

        void TrimNewlinesFromOutput()
        {
            var output = _c.Output;
            int removeFrom = -1;
            for (int i = output.Count - 1; i >= 0; i--)
            {
                var o = output[i];
                if (o.IsCommand || (o.Kind == OutKind.Text && o.IsNonWhitespace)) break;
                if (o.Kind == OutKind.Text && o.IsNewline) removeFrom = i;
            }
            if (removeFrom < 0) return;
            for (int i = removeFrom; i < output.Count;)
            {
                if (output[i].Kind == OutKind.Text) _c.RemoveAt(i);
                else i++;
            }
        }

        void RemoveExistingGlue()
        {
            var output = _c.Output;
            for (int i = output.Count - 1; i >= 0; i--)
            {
                var o = output[i];
                if (o.Kind == OutKind.Glue) _c.RemoveAt(i);
                else if (o.IsCommand) break;
            }
        }

        void TrimWhitespaceFromFunctionEnd()
        {
            int start = _c.CallStack.Current.FunctionStartInOutput;
            if (start == -1) start = 0;
            var output = _c.Output;
            for (int i = output.Count - 1; i >= start; i--)
            {
                var o = output[i];
                if (o.Kind != OutKind.Text) continue;
                if (o.IsNewline || o.IsInlineWhitespace) _c.RemoveAt(i);
                else break;
            }
        }

        void PopCallstack(PushPop? type)
        {
            if (_c.CallStack.Current.Type == PushPop.Function) TrimWhitespaceFromFunctionEnd();
            _c.CallStack.Pop(type);
        }

        bool TryExitFunctionEvaluationFromGame()
        {
            if (_c.CallStack.Current.Type != PushPop.FunctionEvaluationFromGame) return false;
            CurrentPointer = Pointer.Null;
            _c.DidSafeExit = true;
            return true;
        }

        void ForceEnd()
        {
            _c.CallStack.Reset();
            _c.Choices.Clear();
            CurrentPointer = Pointer.Null;
            PreviousPointer = Pointer.Null;
            _c.DidSafeExit = true;
        }

        // =====================================================================
        // Evaluation stack and variables
        // =====================================================================

        void Push(Value v) => _c.Eval.Add(v);

        Value Pop()
        {
            var eval = _c.Eval;
            if (eval.Count == 0) throw new InkException("Evaluation stack underflow");
            var v = eval[eval.Count - 1];
            eval.RemoveAt(eval.Count - 1);
            return v;
        }

        Value Peek() => _c.Eval[_c.Eval.Count - 1];

        void PushHostArguments(object[] args)
        {
            if (args == null) return;
            foreach (var a in args)
            {
                var v = Value.FromHost(a);
                if (v.IsNone)
                    throw new ArgumentException("ink arguments must be int, float, string, bool or InkList. Argument was " + (a == null ? "null" : a.GetType().Name));
                Push(v);
            }
        }

        /// <summary>Lookup order is ink's: globals, then LIST items, then temporaries.</summary>
        Value GetRawVariable(string name, int slot, InkList listItem, int contextIndex)
        {
            if (contextIndex == 0 || contextIndex == -1)
            {
                if (slot >= 0 && !_store.Globals[slot].IsNone) return _store.Globals[slot];
                var item = listItem ?? _story.Lists.FindSingleItemList(name);
                if (item != null) return Value.List(item);
            }
            return _c.CallStack.GetTemp(name, contextIndex);
        }

        Value GetVariable(string name, int slot, InkList listItem, int contextIndex)
        {
            var v = GetRawVariable(name, slot, listItem, contextIndex);
            while (v.Kind == ValueKind.VariablePointer)
                v = GetRawVariable(v.Str, _story.GlobalSlot(v.Str), null, v.ContextIndex);
            return v;
        }

        bool GlobalExists(int slot) => slot >= 0 && !_store.Globals[slot].IsNone;

        void Assign(VarAssignNode va, Value value)
        {
            string name = va.Name;
            int slot = va.GlobalSlot;
            int contextIndex = -1;
            bool setGlobal = va.IsNewDeclaration ? va.IsGlobal : GlobalExists(slot);

            if (va.IsNewDeclaration)
            {
                if (value.Kind == ValueKind.VariablePointer) value = ResolveVariablePointer(value);
            }
            else
            {
                // Assigning through a `ref` parameter: follow the pointer chain to the real variable.
                while (true)
                {
                    var existing = GetRawVariable(name, slot, null, contextIndex);
                    if (existing.Kind != ValueKind.VariablePointer) break;
                    name = existing.Str;
                    contextIndex = existing.ContextIndex;
                    slot = _story.GlobalSlot(name);
                    setGlobal = contextIndex == 0;
                }
            }

            if (setGlobal)
            {
                if (slot < 0) slot = _story.GlobalSlot(name);
                if (slot < 0) throw new InkException("Cannot assign to undeclared global variable '" + name + "'");
                SetGlobal(slot, value);
            }
            else _c.CallStack.SetTemp(name, value, va.IsNewDeclaration, contextIndex);
        }

        void SetGlobal(int slot, Value value)
        {
            value = Variables.RetainListOrigins(_store.Globals[slot], value);
            _store.SetGlobal(slot, value);
        }

        Value ResolveVariablePointer(Value pointer)
        {
            int contextIndex = pointer.ContextIndex;
            // Reference-runtime quirk kept on purpose: an unresolved temp maps to the current frame index.
            if (contextIndex == -1)
                contextIndex = GlobalExists(_story.GlobalSlot(pointer.Str)) ? 0 : _c.CallStack.CurrentFrameIndex;
            var target = GetRawVariable(pointer.Str, _story.GlobalSlot(pointer.Str), null, contextIndex);
            return target.Kind == ValueKind.VariablePointer ? target : Value.Pointer(pointer.Str, contextIndex);
        }

        int VisitCountFor(Container c)
        {
            if (c == null) throw new InkException("Read count target not found");
            if (!c.CountsVisits) throw new InkException("Read count for target (" + c.Name + " - on " + c.Path + ") unknown.");
            return _store.Visits[c.Id];
        }

        int TurnsSinceFor(Container c)
        {
            if (!c.CountsTurns) throw new InkException("TURNS_SINCE() for target (" + c.Name + " - on " + c.Path + ") unknown.");
            int turn = _store.Turns[c.Id];
            return turn == Store.NoTurn ? -1 : _c.TurnIndex - turn;
        }

        // =====================================================================
        // External functions
        // =====================================================================

        void CallExternalFunction(string name, int argCount)
        {
            bool found = _externals.TryGetValue(name, out var def);

            if (found && !def.lookaheadSafe && _c.InStringEvaluation)
                throw new InkException("External function " + name + " could not be called because 1) it wasn't marked as lookaheadSafe when BindExternalFunction was called and 2) the story is in the middle of string generation, either because choice text is being generated, or because you have ink like \"hello {func()}\". You can work around this by generating the result of your function into a temporary variable before the string or choice gets generated: ~ temp x = " + name + "()");

            // A side-effecting host call must not run speculatively during glue lookahead.
            if (found && !def.lookaheadSafe && _snapshot != null)
            {
                _sawLookaheadUnsafeFunctionAfterNewline = true;
                return;
            }

            if (!found)
            {
                var fallback = AllowExternalFunctionFallbacks ? _story.KnotNamed(name) : null;
                if (fallback == null)
                    throw new InkException("Trying to call EXTERNAL function '" + name + "' which has not been bound" +
                                           (AllowExternalFunctionFallbacks ? ", and fallback ink function could not be found." : " (and ink fallbacks disabled)."));
                _c.CallStack.Push(PushPop.Function, 0, _c.Output.Count);
                _c.Diverted = Pointer.StartOf(fallback);
                return;
            }

            var args = new object[argCount];
            for (int i = argCount - 1; i >= 0; i--) args[i] = Pop().ToHost();
            var result = def.fn(args);
            if (result == null) Push(Value.Void);
            else
            {
                var v = Value.FromHost(result);
                if (v.IsNone) throw new InkException("Could not create ink value from returned object of type " + result.GetType());
                Push(v);
            }
        }

        // =====================================================================
        // Errors
        // =====================================================================

        void Warning(string message) => AddError(message, true);

        void AddError(string message, bool isWarning)
        {
            var kind = isWarning ? "WARNING" : "ERROR";
            var pointer = CurrentPointer;
            message = pointer.IsNull
                ? "RUNTIME " + kind + ": " + message
                : "RUNTIME " + kind + ": (" + pointer + "): " + message;

            if (isWarning) (_c.Warnings ??= new List<string>()).Add(message);
            else
            {
                (_c.Errors ??= new List<string>()).Add(message);
                ForceEnd(); // a broken story stops
            }
        }

        void ReportErrors()
        {
            bool hasErrors = _c.Errors != null && _c.Errors.Count > 0;
            bool hasWarnings = _c.Warnings != null && _c.Warnings.Count > 0;
            if (!hasErrors && !hasWarnings) return;

            if (OnError != null)
            {
                if (hasErrors) foreach (var e in _c.Errors) OnError(e, false);
                if (hasWarnings) foreach (var w in _c.Warnings) OnError(w, true);
                _c.Errors = null;
                _c.Warnings = null;
                return;
            }

            var first = hasErrors ? _c.Errors[0] : _c.Warnings[0];
            throw new StoryException("Ink had " + (hasErrors ? _c.Errors.Count + " error(s)" : "") +
                                     (hasErrors && hasWarnings ? " and " : "") +
                                     (hasWarnings ? _c.Warnings.Count + " warning(s)" : "") +
                                     ". Assign InkRunner.OnError to handle them. The first issue was: " + first);
        }

        // =====================================================================
        // Initialisation
        // =====================================================================

        void ResetGlobals()
        {
            var decl = _story.KnotNamed("global decl");
            if (decl != null)
            {
                var original = CurrentPointer;
                ChoosePath(new Pointer(decl, -1), false);
                ContinueInternal();
                CurrentPointer = original;
            }
            Array.Copy(_store.Globals, _store.Defaults, _store.Globals.Length);
        }

        internal void ReplaceState(Cursor cursor)
        {
            _c = cursor;
            _snapshot = null;
        }
    }
}
