using System.Collections.Generic;

namespace LegacyThroughTime.Narrative.Ink
{
    /// <summary>Node opcode. The VM dispatches on this byte instead of on runtime types.</summary>
    enum Op : byte
    {
        Container,
        Text,
        Glue,
        Command,
        Divert,
        ChoicePoint,
        VarRef,
        ReadCount,
        VarAssign,
        Native,
        Literal,
        Tag,
    }

    enum Cmd : byte
    {
        EvalStart,
        EvalOutput,
        EvalEnd,
        Duplicate,
        PopEvaluatedValue,
        PopFunction,
        PopTunnel,
        BeginString,
        EndString,
        NoOp,
        ChoiceCount,
        Turns,
        TurnsSince,
        ReadCount,
        Random,
        SeedRandom,
        VisitIndex,
        SequenceShuffleIndex,
        StartThread,
        Done,
        End,
        ListFromInt,
        ListRange,
        ListRandom,
        BeginTag,
        EndTag,
    }

    enum PushPop : byte
    {
        Tunnel = 0,
        Function = 1,
        FunctionEvaluationFromGame = 2,
    }

    /// <summary>
    /// A node of the compiled story graph. Immutable after loading, so one
    /// <see cref="Story"/> can back any number of concurrent runners.
    /// </summary>
    abstract class Node
    {
        public readonly Op Op;
        public Container Parent;
        public int Index = -1; // index in Parent.Content, -1 for named-only content

        protected Node(Op op) => Op = op;

        /// <summary>Dot path as used by ink (<c>knot.0.c-1</c>). Diagnostics only.</summary>
        public virtual string PathString =>
            Parent == null ? "" : Join(Parent.PathString, Index.ToString(System.Globalization.CultureInfo.InvariantCulture));

        protected static string Join(string head, string tail) => head.Length == 0 ? tail : head + "." + tail;
    }

    sealed class Container : Node
    {
        public const int CountVisitsFlag = 1;
        public const int CountTurnsFlag = 2;
        public const int CountStartOnlyFlag = 4;

        public string Name;
        public Node[] Content;
        public Dictionary<string, Node> Named;
        public int Id;           // dense id: index into per-runner count arrays
        public int CountFlags;
        public string Path;      // absolute path, computed once by the loader

        public Container() : base(Op.Container) { }

        public bool CountsVisits => (CountFlags & CountVisitsFlag) != 0;
        public bool CountsTurns => (CountFlags & CountTurnsFlag) != 0;
        public bool CountsAtStartOnly => (CountFlags & CountStartOnlyFlag) != 0;
        public bool HasName => !string.IsNullOrEmpty(Name);

        public override string PathString => Path;
    }

    sealed class TextNode : Node
    {
        public readonly string Text;
        public TextNode(string text) : base(Op.Text) => Text = text;
    }

    sealed class GlueNode : Node
    {
        public GlueNode() : base(Op.Glue) { }
    }

    sealed class CommandNode : Node
    {
        public readonly Cmd Cmd;
        public CommandNode(Cmd cmd) : base(Op.Command) => Cmd = cmd;
    }

    sealed class DivertNode : Node
    {
        public string TargetPath;        // raw path string (static diverts)
        public Pointer Target;           // resolved once by the loader
        public string VariableName;      // -> var_name
        public bool PushesToStack;
        public PushPop StackType;
        public bool IsExternal;
        public int ExternalArgs;
        public bool IsConditional;

        public DivertNode() : base(Op.Divert) { }
    }

    sealed class ChoicePointNode : Node
    {
        public string TargetPathRaw;
        public Container Target;
        public bool HasCondition, HasStartContent, HasChoiceOnlyContent, IsInvisibleDefault, OnceOnly;

        public ChoicePointNode() : base(Op.ChoicePoint) { }
    }

    sealed class VarRefNode : Node
    {
        public readonly string Name;
        public int GlobalSlot = -1;     // resolved by the loader when the name is a global
        public InkList ListItem;        // resolved when the name is a LIST item

        public VarRefNode(string name) : base(Op.VarRef) => Name = name;
    }

    sealed class ReadCountNode : Node
    {
        public readonly string TargetPath;
        public Container Target;

        public ReadCountNode(string path) : base(Op.ReadCount) => TargetPath = path;
    }

    sealed class VarAssignNode : Node
    {
        public readonly string Name;
        public readonly bool IsGlobal;
        public readonly bool IsNewDeclaration;
        public int GlobalSlot = -1;

        public VarAssignNode(string name, bool isGlobal, bool isNewDeclaration) : base(Op.VarAssign)
        {
            Name = name;
            IsGlobal = isGlobal;
            IsNewDeclaration = isNewDeclaration;
        }
    }

    sealed class NativeNode : Node
    {
        public readonly NativeOp Fn;
        public readonly int Arity;
        public readonly string Name;

        public NativeNode(NativeOp fn, int arity, string name) : base(Op.Native)
        {
            Fn = fn;
            Arity = arity;
            Name = name;
        }
    }

    sealed class LiteralNode : Node
    {
        public Value Value; // written only by the loader
        public LiteralNode(Value value) : base(Op.Literal) => Value = value;
    }

    /// <summary>Pre-ink-1.1 static tag: <c>{"#": "text"}</c>.</summary>
    sealed class TagNode : Node
    {
        public readonly string Text;
        public TagNode(string text) : base(Op.Tag) => Text = text;
    }

    /// <summary>
    /// Divert target value (<c>-> knot</c> stored in a variable). Resolved once when the
    /// story loads; equality is path equality, exactly as in ink.
    /// </summary>
    sealed class DivertTarget
    {
        public readonly string Path;
        public readonly Pointer Pointer;      // where a divert to it lands
        public readonly Container Container;  // what read/turn counts refer to (exact match only)

        public DivertTarget(string path, Pointer pointer, Container container)
        {
            Path = path;
            Pointer = pointer;
            Container = container;
        }
    }

    /// <summary>
    /// Program counter: a position inside a container. <c>Index == -1</c> addresses the
    /// container itself (it will be entered on the next step).
    /// </summary>
    readonly struct Pointer
    {
        public readonly Container Container;
        public readonly int Index;

        public Pointer(Container container, int index)
        {
            Container = container;
            Index = index;
        }

        public static readonly Pointer Null = new Pointer(null, -1);
        public static Pointer StartOf(Container c) => new Pointer(c, 0);

        public bool IsNull => Container == null;

        public Node Resolve()
        {
            if (Index < 0) return Container;
            if (Container == null) return null;
            if (Container.Content.Length == 0) return Container;
            if (Index >= Container.Content.Length) return null;
            return Container.Content[Index];
        }

        public Pointer WithIndex(int index) => new Pointer(Container, index);

        public override string ToString() =>
            IsNull ? "null" : Index >= 0 ? (Container.Path.Length == 0 ? "" : Container.Path + ".") + Index : Container.Path;
    }
}
