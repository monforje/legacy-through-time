using System.Globalization;

namespace LegacyThroughTime.Narrative.Ink
{
    /// <summary>
    /// Value type tags. The numeric order of <see cref="Bool"/>..<see cref="VariablePointer"/>
    /// is the coercion lattice of ink: a binary operation lifts both operands to the
    /// greater of the two kinds (Bool &lt; Int &lt; Float &lt; List &lt; String &lt; ...).
    /// </summary>
    enum ValueKind : byte
    {
        None = 0,
        Bool = 1,
        Int = 2,
        Float = 3,
        List = 4,
        String = 5,
        DivertTarget = 6,
        VariablePointer = 7,

        // Stack-only pseudo values, never stored in variables.
        Void = 8,
        Tag = 9,
    }

    /// <summary>
    /// Tagged union for every runtime value. A struct, so evaluation never boxes numbers.
    /// <c>O</c> holds the reference payload: string (String/Tag/VariablePointer name),
    /// <see cref="DivertTarget"/> or <see cref="InkList"/>.
    /// </summary>
    readonly struct Value
    {
        public readonly ValueKind Kind;
        public readonly int I;   // Int, Bool (0/1), VariablePointer context index
        public readonly float F; // Float
        public readonly object O;

        Value(ValueKind kind, int i, float f, object o)
        {
            Kind = kind;
            I = i;
            F = f;
            O = o;
        }

        public static readonly Value None = default;
        public static readonly Value Void = new Value(ValueKind.Void, 0, 0, null);

        public static Value Int(int v) => new Value(ValueKind.Int, v, 0, null);
        public static Value Float(float v) => new Value(ValueKind.Float, 0, v, null);
        public static Value Bool(bool v) => new Value(ValueKind.Bool, v ? 1 : 0, 0, null);
        public static Value String(string v) => new Value(ValueKind.String, 0, 0, v);
        public static Value Tag(string v) => new Value(ValueKind.Tag, 0, 0, v);
        public static Value Divert(DivertTarget t) => new Value(ValueKind.DivertTarget, 0, 0, t);
        public static Value List(InkList l) => new Value(ValueKind.List, 0, 0, l);
        public static Value Pointer(string name, int contextIndex) => new Value(ValueKind.VariablePointer, contextIndex, 0, name);

        public bool IsNone => Kind == ValueKind.None;
        public bool BoolValue => I != 0;
        public string Str => (string)O;
        public InkList ListValue => (InkList)O;
        public DivertTarget Target => (DivertTarget)O;
        public int ContextIndex => I;

        public bool IsTruthy
        {
            get
            {
                switch (Kind)
                {
                    case ValueKind.Bool:
                    case ValueKind.Int: return I != 0;
                    case ValueKind.Float: return F != 0f;
                    case ValueKind.String: return Str.Length > 0;
                    case ValueKind.List: return ListValue.Count > 0;
                    case ValueKind.DivertTarget:
                        throw new InkException("Shouldn't use a divert target (to " + Target.Path + ") as a conditional value. Did you intend a function call 'likeThis()' or a read count check 'likeThis'? (no arrows)");
                    default:
                        throw new InkException("Shouldn't be checking the truthiness of a " + Kind);
                }
            }
        }

        /// <summary>Text form used when a value is printed into the story (<c>{x}</c>).</summary>
        public override string ToString()
        {
            switch (Kind)
            {
                case ValueKind.Bool: return I != 0 ? "true" : "false";
                case ValueKind.Int: return I.ToString(CultureInfo.InvariantCulture);
                case ValueKind.Float: return F.ToString(CultureInfo.InvariantCulture);
                case ValueKind.String:
                case ValueKind.Tag: return Str;
                case ValueKind.List: return ListValue.ToString();
                case ValueKind.DivertTarget: return "DivertTargetValue(" + Target.Path + ")";
                case ValueKind.VariablePointer: return "VariablePointerValue(" + Str + ")";
                case ValueKind.Void: return "void";
                default: return "";
            }
        }

        /// <summary>Lossless structural equality (used for change tracking and save diffs).</summary>
        public bool SameAs(in Value other)
        {
            if (Kind != other.Kind) return false;
            switch (Kind)
            {
                case ValueKind.Float: return F.Equals(other.F);
                case ValueKind.String:
                case ValueKind.Tag: return Str == other.Str;
                case ValueKind.VariablePointer: return I == other.I && Str == other.Str;
                case ValueKind.DivertTarget: return Target.Path == other.Target.Path;
                case ValueKind.List: return ListValue.Equals(other.ListValue);
                default: return I == other.I;
            }
        }

        /// <summary>
        /// ink cast rules. Returns <see cref="None"/> for a failed string → number parse,
        /// throws for casts ink forbids.
        /// </summary>
        public Value Cast(ValueKind to)
        {
            if (to == Kind) return this;
            switch (Kind)
            {
                case ValueKind.Bool:
                    if (to == ValueKind.Int) return Int(I);
                    if (to == ValueKind.Float) return Float(I != 0 ? 1f : 0f);
                    if (to == ValueKind.String) return String(I != 0 ? "true" : "false");
                    break;
                case ValueKind.Int:
                    if (to == ValueKind.Bool) return Bool(I != 0);
                    if (to == ValueKind.Float) return Float(I);
                    if (to == ValueKind.String) return String(I.ToString(CultureInfo.InvariantCulture));
                    break;
                case ValueKind.Float:
                    if (to == ValueKind.Bool) return Bool(F != 0f);
                    if (to == ValueKind.Int) return Int((int)F);
                    if (to == ValueKind.String) return String(F.ToString(CultureInfo.InvariantCulture));
                    break;
                case ValueKind.String:
                    if (to == ValueKind.Int)
                        return int.TryParse(Str, out int n) ? Int(n) : None;
                    if (to == ValueKind.Float)
                        return float.TryParse(Str, NumberStyles.Float, CultureInfo.InvariantCulture, out float f) ? Float(f) : None;
                    break;
                case ValueKind.List:
                {
                    var list = ListValue;
                    bool empty = list.Count == 0;
                    var max = list.MaxItem;
                    if (to == ValueKind.Int) return Int(empty ? 0 : max.Value);
                    if (to == ValueKind.Float) return Float(empty ? 0f : max.Value);
                    if (to == ValueKind.String) return String(empty ? "" : max.Key.FullName);
                    break;
                }
            }
            throw new InkException("Can't cast " + this + " from " + Kind + " to " + to);
        }

        /// <summary>Converts to the host-facing representation (int, float, bool, string, InkList).</summary>
        public object ToHost()
        {
            switch (Kind)
            {
                case ValueKind.Bool: return I != 0;
                case ValueKind.Int: return I;
                case ValueKind.Float: return F;
                case ValueKind.String: return Str;
                case ValueKind.List: return ListValue;
                case ValueKind.DivertTarget: return Target.Path;
                case ValueKind.VariablePointer: return Str;
                default: return null;
            }
        }

        public static Value FromHost(object v)
        {
            switch (v)
            {
                case null: return None;
                case bool b: return Bool(b);
                case int i: return Int(i);
                case long l: return Int((int)l);
                case float f: return Float(f);
                case double d: return Float((float)d);
                case string s: return String(s);
                case InkList list: return List(list);
                default: return None;
            }
        }
    }
}
