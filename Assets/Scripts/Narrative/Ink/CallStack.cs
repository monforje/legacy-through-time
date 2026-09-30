using System;
using System.Collections.Generic;

namespace LegacyThroughTime.Narrative.Ink
{
    /// <summary>One activation record: a tunnel, function or host-called function.</summary>
    sealed class Frame
    {
        public Pointer Pointer;
        public bool InExpression;
        public Dictionary<string, Value> Temps; // lazily allocated: most frames have none
        public PushPop Type;
        public int EvalHeightWhenPushed;
        public int FunctionStartInOutput;

        public Frame(PushPop type, Pointer pointer)
        {
            Type = type;
            Pointer = pointer;
        }

        public Frame Clone()
        {
            var f = new Frame(Type, Pointer);
            f.CopyFrom(this);
            return f;
        }

        public void CopyFrom(Frame o)
        {
            Type = o.Type;
            Pointer = o.Pointer;
            InExpression = o.InExpression;
            EvalHeightWhenPushed = o.EvalHeightWhenPushed;
            FunctionStartInOutput = o.FunctionStartInOutput;
            if (o.Temps == null || o.Temps.Count == 0)
            {
                Temps?.Clear();
                return;
            }
            if (Temps == null) Temps = new Dictionary<string, Value>(o.Temps);
            else
            {
                Temps.Clear();
                foreach (var kv in o.Temps) Temps.Add(kv.Key, kv.Value);
            }
        }
    }

    /// <summary>
    /// A flow of control (<c>&lt;- thread</c>). Choices capture a forked copy of the thread
    /// they were generated in, so picking one resumes in exactly that context.
    /// </summary>
    sealed class InkThread
    {
        public readonly List<Frame> Frames = new List<Frame>(4);
        public int Index;
        public Pointer Previous;

        public InkThread Clone()
        {
            var t = new InkThread();
            t.CopyFrom(this);
            return t;
        }

        /// <summary>Deep copy into this thread, reusing its own frame objects.</summary>
        public void CopyFrom(InkThread o)
        {
            Index = o.Index;
            Previous = o.Previous;
            int n = o.Frames.Count;
            for (int i = 0; i < n; i++)
            {
                if (i < Frames.Count) Frames[i].CopyFrom(o.Frames[i]);
                else Frames.Add(o.Frames[i].Clone());
            }
            if (Frames.Count > n) Frames.RemoveRange(n, Frames.Count - n);
        }
    }

    /// <summary>
    /// The pushdown component of the machine: a stack of threads, each a stack of frames.
    /// Temporary variables are addressed by context index — 0 is "global", n is frame n-1
    /// of the current thread — the convention variable pointers (<c>ref</c> params) use.
    /// </summary>
    sealed class CallStack
    {
        readonly List<InkThread> _threads;
        int _threadCounter;
        readonly Pointer _startOfRoot;

        public CallStack(Container root)
        {
            _startOfRoot = Pointer.StartOf(root);
            _threads = new List<InkThread>(2);
            Reset();
        }

        /// <summary>Deep copy into this stack, reusing the threads and frames it already owns.</summary>
        public void CopyFrom(CallStack o)
        {
            _threadCounter = o._threadCounter;
            int n = o._threads.Count;
            for (int i = 0; i < n; i++)
            {
                if (i < _threads.Count) _threads[i].CopyFrom(o._threads[i]);
                else _threads.Add(o._threads[i].Clone());
            }
            if (_threads.Count > n) _threads.RemoveRange(n, _threads.Count - n);
        }

        public void Reset()
        {
            _threads.Clear();
            var t = new InkThread();
            t.Frames.Add(new Frame(PushPop.Tunnel, _startOfRoot));
            _threads.Add(t);
        }

        public List<Frame> Frames => CurrentThread.Frames;
        public IReadOnlyList<InkThread> Threads => _threads;
        public int ThreadCounter { get => _threadCounter; set => _threadCounter = value; }

        public Frame Current
        {
            get
            {
                var frames = _threads[_threads.Count - 1].Frames;
                return frames[frames.Count - 1];
            }
        }

        public int CurrentFrameIndex => Frames.Count - 1;

        public InkThread CurrentThread
        {
            get => _threads[_threads.Count - 1];
            set
            {
                _threads.Clear();
                _threads.Add(value);
            }
        }

        public bool CanPopAny => Frames.Count > 1;
        public bool CanPop(PushPop type) => CanPopAny && Current.Type == type;
        public bool ElementIsEvaluateFromGame => Current.Type == PushPop.FunctionEvaluationFromGame;
        public bool CanPopThread => _threads.Count > 1 && !ElementIsEvaluateFromGame;

        public void PushThread()
        {
            var t = CurrentThread.Clone();
            t.Index = ++_threadCounter;
            _threads.Add(t);
        }

        public InkThread ForkThread()
        {
            var t = CurrentThread.Clone();
            t.Index = ++_threadCounter;
            return t;
        }

        public void PopThread()
        {
            if (!CanPopThread) throw new InkException("Can't pop thread");
            _threads.RemoveAt(_threads.Count - 1);
        }

        public void Push(PushPop type, int evalHeight = 0, int outputLength = 0) =>
            Frames.Add(new Frame(type, Current.Pointer)
            {
                EvalHeightWhenPushed = evalHeight,
                FunctionStartInOutput = outputLength,
            });

        public void Pop(PushPop? type = null)
        {
            if (!CanPopAny || (type.HasValue && Current.Type != type.Value))
                throw new InkException("Mismatched push/pop in Callstack");
            Frames.RemoveAt(Frames.Count - 1);
        }

        public Value GetTemp(string name, int contextIndex)
        {
            if (contextIndex == -1) contextIndex = CurrentFrameIndex + 1;
            var temps = Frames[contextIndex - 1].Temps;
            return temps != null && temps.TryGetValue(name, out var v) ? v : Value.None;
        }

        public void SetTemp(string name, Value value, bool declareNew, int contextIndex)
        {
            if (contextIndex == -1) contextIndex = CurrentFrameIndex + 1;
            var frame = Frames[contextIndex - 1];
            var temps = frame.Temps ??= new Dictionary<string, Value>();
            if (temps.TryGetValue(name, out var old)) value = Variables.RetainListOrigins(old, value);
            else if (!declareNew) throw new InkException("Could not find temporary variable to set: " + name);
            temps[name] = value;
        }

        public int ContextForVariableNamed(string name)
        {
            var temps = Current.Temps;
            return temps != null && temps.ContainsKey(name) ? CurrentFrameIndex + 1 : 0;
        }

        public InkThread ThreadWithIndex(int index)
        {
            foreach (var t in _threads)
                if (t.Index == index) return t;
            return null;
        }

        internal void LoadThreads(List<InkThread> threads, int counter)
        {
            if (threads.Count == 0) throw new FormatException("save: call stack has no threads");
            _threads.Clear();
            _threads.AddRange(threads);
            _threadCounter = counter;
        }
    }

    static class Variables
    {
        /// <summary>
        /// Assigning an empty list keeps the origins of the list it replaces, so
        /// <c>LIST_ALL(x)</c> still works after <c>~ x = ()</c>.
        /// </summary>
        public static Value RetainListOrigins(in Value oldValue, Value newValue)
        {
            if (oldValue.Kind == ValueKind.List && newValue.Kind == ValueKind.List && newValue.ListValue.Count == 0)
                return Value.List(InkList.WithOrigins(oldValue.ListValue.OriginNames));
            return newValue;
        }
    }
}
