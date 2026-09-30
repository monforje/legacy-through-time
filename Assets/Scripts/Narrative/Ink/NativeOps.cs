using System;
using System.Collections.Generic;

namespace LegacyThroughTime.Narrative.Ink
{
    enum NativeOp : byte
    {
        Add, Subtract, Divide, Multiply, Mod, Negate,
        Equal, Greater, Less, GreaterOrEqual, LessOrEqual, NotEqual, Not,
        And, Or, Min, Max, Pow, Floor, Ceiling, Int, Float,
        Has, Hasnt, Intersect,
        ListMin, ListMax, ListAll, ListCount, ListValue, ListInvert,
    }

    /// <summary>
    /// ink's native operators as a single dispatch table. Semantics follow the reference
    /// runtime: operands are lifted to a common kind along the coercion lattice, then the
    /// operator is applied for that kind (ints wrap, int division truncates, and so on).
    /// </summary>
    static class NativeOps
    {
        static readonly Dictionary<string, (NativeOp op, int arity)> ByName = new Dictionary<string, (NativeOp, int)>
        {
            ["+"] = (NativeOp.Add, 2), ["-"] = (NativeOp.Subtract, 2), ["/"] = (NativeOp.Divide, 2),
            ["*"] = (NativeOp.Multiply, 2), ["%"] = (NativeOp.Mod, 2), ["_"] = (NativeOp.Negate, 1),
            ["=="] = (NativeOp.Equal, 2), [">"] = (NativeOp.Greater, 2), ["<"] = (NativeOp.Less, 2),
            [">="] = (NativeOp.GreaterOrEqual, 2), ["<="] = (NativeOp.LessOrEqual, 2), ["!="] = (NativeOp.NotEqual, 2),
            ["!"] = (NativeOp.Not, 1), ["&&"] = (NativeOp.And, 2), ["||"] = (NativeOp.Or, 2),
            ["MIN"] = (NativeOp.Min, 2), ["MAX"] = (NativeOp.Max, 2), ["POW"] = (NativeOp.Pow, 2),
            ["FLOOR"] = (NativeOp.Floor, 1), ["CEILING"] = (NativeOp.Ceiling, 1),
            ["INT"] = (NativeOp.Int, 1), ["FLOAT"] = (NativeOp.Float, 1),
            ["?"] = (NativeOp.Has, 2), ["!?"] = (NativeOp.Hasnt, 2), ["L^"] = (NativeOp.Intersect, 2),
            ["LIST_MIN"] = (NativeOp.ListMin, 1), ["LIST_MAX"] = (NativeOp.ListMax, 1),
            ["LIST_ALL"] = (NativeOp.ListAll, 1), ["LIST_COUNT"] = (NativeOp.ListCount, 1),
            ["LIST_VALUE"] = (NativeOp.ListValue, 1), ["LIST_INVERT"] = (NativeOp.ListInvert, 1),
        };

        public static bool TryGet(string name, out NativeOp op, out int arity)
        {
            if (ByName.TryGetValue(name, out var entry))
            {
                op = entry.op;
                arity = entry.arity;
                return true;
            }
            op = default;
            arity = 0;
            return false;
        }

        public static Value Call(NativeNode fn, in Value a, ListDefinitions lists)
        {
            if (a.Kind == ValueKind.Void) throw VoidOperand(fn);
            return Unary(fn, a.Kind < ValueKind.Int ? a.Cast(ValueKind.Int) : a, lists);
        }

        public static Value Call(NativeNode fn, in Value a, in Value b, ListDefinitions lists)
        {
            if (a.Kind == ValueKind.Void || b.Kind == ValueKind.Void) throw VoidOperand(fn);
            if (a.Kind == ValueKind.List || b.Kind == ValueKind.List) return BinaryWithList(fn, a, b, lists);

            var kind = a.Kind > b.Kind ? a.Kind : b.Kind;
            if (kind < ValueKind.Int) kind = ValueKind.Int;
            var x = a.Cast(kind);
            var y = b.Cast(kind);
            return Binary(fn, kind, x, y);
        }

        static Value Unary(NativeNode fn, in Value v, ListDefinitions lists)
        {
            switch (v.Kind)
            {
                case ValueKind.Int:
                    switch (fn.Fn)
                    {
                        case NativeOp.Negate: return Value.Int(-v.I);
                        case NativeOp.Not: return Value.Bool(v.I == 0);
                        case NativeOp.Floor:
                        case NativeOp.Ceiling:
                        case NativeOp.Int: return v;
                        case NativeOp.Float: return Value.Float(v.I);
                    }
                    break;
                case ValueKind.Float:
                    switch (fn.Fn)
                    {
                        case NativeOp.Negate: return Value.Float(-v.F);
                        case NativeOp.Not: return Value.Bool(v.F == 0f);
                        case NativeOp.Floor: return Value.Float((float)Math.Floor(v.F));
                        case NativeOp.Ceiling: return Value.Float((float)Math.Ceiling(v.F));
                        case NativeOp.Int: return Value.Int((int)v.F);
                        case NativeOp.Float: return v;
                    }
                    break;
                case ValueKind.List:
                {
                    var l = v.ListValue;
                    switch (fn.Fn)
                    {
                        case NativeOp.Not: return Value.Int(l.Count == 0 ? 1 : 0);
                        case NativeOp.ListInvert: return Value.List(l.Inverse(lists));
                        case NativeOp.ListAll: return Value.List(l.All(lists));
                        case NativeOp.ListMin: return Value.List(l.MinAsList());
                        case NativeOp.ListMax: return Value.List(l.MaxAsList());
                        case NativeOp.ListCount: return Value.Int(l.Count);
                        case NativeOp.ListValue: return Value.Int(l.MaxItem.Value);
                    }
                    break;
                }
            }
            throw new InkException("Cannot perform operation '" + fn.Name + "' on " + v.Kind);
        }

        static Value Binary(NativeNode fn, ValueKind kind, in Value x, in Value y)
        {
            switch (kind)
            {
                case ValueKind.Int:
                {
                    int a = x.I, b = y.I;
                    switch (fn.Fn)
                    {
                        case NativeOp.Add: return Value.Int(unchecked(a + b));
                        case NativeOp.Subtract: return Value.Int(unchecked(a - b));
                        case NativeOp.Multiply: return Value.Int(unchecked(a * b));
                        case NativeOp.Divide:
                            if (b == 0) throw new InkException("Division by zero");
                            return Value.Int(a / b);
                        case NativeOp.Mod:
                            if (b == 0) throw new InkException("Division by zero");
                            return Value.Int(a % b);
                        case NativeOp.Equal: return Value.Bool(a == b);
                        case NativeOp.Greater: return Value.Bool(a > b);
                        case NativeOp.Less: return Value.Bool(a < b);
                        case NativeOp.GreaterOrEqual: return Value.Bool(a >= b);
                        case NativeOp.LessOrEqual: return Value.Bool(a <= b);
                        case NativeOp.NotEqual: return Value.Bool(a != b);
                        case NativeOp.And: return Value.Bool(a != 0 && b != 0);
                        case NativeOp.Or: return Value.Bool(a != 0 || b != 0);
                        case NativeOp.Max: return Value.Int(Math.Max(a, b));
                        case NativeOp.Min: return Value.Int(Math.Min(a, b));
                        case NativeOp.Pow: return Value.Float((float)Math.Pow(a, b));
                    }
                    break;
                }
                case ValueKind.Float:
                {
                    float a = x.F, b = y.F;
                    switch (fn.Fn)
                    {
                        case NativeOp.Add: return Value.Float(a + b);
                        case NativeOp.Subtract: return Value.Float(a - b);
                        case NativeOp.Multiply: return Value.Float(a * b);
                        case NativeOp.Divide: return Value.Float(a / b);
                        case NativeOp.Mod: return Value.Float(a % b);
                        case NativeOp.Equal: return Value.Bool(a == b);
                        case NativeOp.Greater: return Value.Bool(a > b);
                        case NativeOp.Less: return Value.Bool(a < b);
                        case NativeOp.GreaterOrEqual: return Value.Bool(a >= b);
                        case NativeOp.LessOrEqual: return Value.Bool(a <= b);
                        case NativeOp.NotEqual: return Value.Bool(a != b);
                        case NativeOp.And: return Value.Bool(a != 0f && b != 0f);
                        case NativeOp.Or: return Value.Bool(a != 0f || b != 0f);
                        case NativeOp.Max: return Value.Float(Math.Max(a, b));
                        case NativeOp.Min: return Value.Float(Math.Min(a, b));
                        case NativeOp.Pow: return Value.Float((float)Math.Pow(a, b));
                    }
                    break;
                }
                case ValueKind.String:
                {
                    string a = x.Str, b = y.Str;
                    switch (fn.Fn)
                    {
                        case NativeOp.Add: return Value.String(a + b);
                        case NativeOp.Equal: return Value.Bool(a == b);
                        case NativeOp.NotEqual: return Value.Bool(a != b);
                        case NativeOp.Has: return Value.Bool(a.Contains(b));
                        case NativeOp.Hasnt: return Value.Bool(!a.Contains(b));
                    }
                    break;
                }
                case ValueKind.DivertTarget:
                    switch (fn.Fn)
                    {
                        case NativeOp.Equal: return Value.Bool(x.Target.Path == y.Target.Path);
                        case NativeOp.NotEqual: return Value.Bool(x.Target.Path != y.Target.Path);
                    }
                    break;
            }
            throw new InkException("Cannot perform operation '" + fn.Name + "' on " + kind);
        }

        static Value BinaryWithList(NativeNode fn, in Value a, in Value b, ListDefinitions lists)
        {
            // LIST ± int: shift every item along its own LIST definition.
            if ((fn.Fn == NativeOp.Add || fn.Fn == NativeOp.Subtract) && a.Kind == ValueKind.List && b.Kind == ValueKind.Int)
                return Value.List(Increment(a.ListValue, fn.Fn == NativeOp.Add ? b.I : -b.I, lists));

            if ((fn.Fn == NativeOp.And || fn.Fn == NativeOp.Or) && (a.Kind != ValueKind.List || b.Kind != ValueKind.List))
            {
                bool l = a.IsTruthy, r = b.IsTruthy;
                return Value.Bool(fn.Fn == NativeOp.And ? l && r : l || r);
            }

            if (a.Kind == ValueKind.List && b.Kind == ValueKind.List)
                return ListBinary(fn, a.ListValue, b.ListValue);

            throw new InkException("Can not call use '" + fn.Name + "' operation on " + a.Kind + " and " + b.Kind);
        }

        static InkList Increment(InkList list, int delta, ListDefinitions lists)
        {
            var result = new InkList();
            var origins = list.Origins(lists);
            foreach (var kv in list)
            {
                int target = unchecked(kv.Value + delta);
                if (origins == null) continue;
                foreach (var origin in origins)
                {
                    if (origin == null || origin.Name != kv.Key.Origin) continue;
                    if (origin.TryGetItemWithValue(target, out var item)) result.Set(item, target);
                    break;
                }
            }
            return result;
        }

        static Value ListBinary(NativeNode fn, InkList x, InkList y)
        {
            switch (fn.Fn)
            {
                case NativeOp.Add: return Value.List(x.Union(y));
                case NativeOp.Subtract: return Value.List(x.Without(y));
                case NativeOp.Has: return Value.Bool(x.Contains(y));
                case NativeOp.Hasnt: return Value.Bool(!x.Contains(y));
                case NativeOp.Intersect: return Value.List(x.Intersect(y));
                case NativeOp.Equal: return Value.Bool(x.Equals(y));
                case NativeOp.NotEqual: return Value.Bool(!x.Equals(y));
                case NativeOp.Greater: return Value.Bool(x.GreaterThan(y));
                case NativeOp.Less: return Value.Bool(x.LessThan(y));
                case NativeOp.GreaterOrEqual: return Value.Bool(x.GreaterThanOrEquals(y));
                case NativeOp.LessOrEqual: return Value.Bool(x.LessThanOrEquals(y));
                case NativeOp.And: return Value.Bool(x.Count > 0 && y.Count > 0);
                case NativeOp.Or: return Value.Bool(x.Count > 0 || y.Count > 0);
            }
            throw new InkException("Cannot perform operation '" + fn.Name + "' on List");
        }

        static InkException VoidOperand(NativeNode fn) =>
            new InkException("Attempting to perform " + fn.Name + " on a void value. Did you forget to 'return' a value from a function you called here?");
    }
}
