using System;
using System.Collections.Generic;
using System.Globalization;
using LegacyThroughTime.Narrative.Json;

namespace LegacyThroughTime.Narrative.Ink
{
    /// <summary>
    /// Compiles inklecate's JSON (format v18–v21) into the node graph the VM runs.
    /// Everything that the reference runtime resolves lazily on every execution —
    /// divert targets, choice targets, read-count containers, global variable names —
    /// is resolved here exactly once, so the hot loop never parses a path or hashes a
    /// variable name for globals.
    /// </summary>
    static class InkLoader
    {
        public const int MinFormat = 18;
        public const int MaxFormat = 21;

        static readonly Dictionary<string, Cmd> Commands = new Dictionary<string, Cmd>
        {
            ["ev"] = Cmd.EvalStart, ["out"] = Cmd.EvalOutput, ["/ev"] = Cmd.EvalEnd,
            ["du"] = Cmd.Duplicate, ["pop"] = Cmd.PopEvaluatedValue, ["~ret"] = Cmd.PopFunction,
            ["->->"] = Cmd.PopTunnel, ["str"] = Cmd.BeginString, ["/str"] = Cmd.EndString,
            ["nop"] = Cmd.NoOp, ["choiceCnt"] = Cmd.ChoiceCount, ["turn"] = Cmd.Turns,
            ["turns"] = Cmd.TurnsSince, ["readc"] = Cmd.ReadCount, ["rnd"] = Cmd.Random,
            ["srnd"] = Cmd.SeedRandom, ["visit"] = Cmd.VisitIndex, ["seq"] = Cmd.SequenceShuffleIndex,
            ["thread"] = Cmd.StartThread, ["done"] = Cmd.Done, ["end"] = Cmd.End,
            ["listInt"] = Cmd.ListFromInt, ["range"] = Cmd.ListRange, ["lrnd"] = Cmd.ListRandom,
            ["#"] = Cmd.BeginTag, ["/#"] = Cmd.EndTag,
        };

        public sealed class Result
        {
            public Container Root;
            public Container[] Containers;
            public ListDefinitions Lists;
            public string[] GlobalNames;
            public Dictionary<string, int> GlobalSlots;
            public Dictionary<string, DivertTarget> Targets;
            public int Format;
        }

        public static Result Load(string json)
        {
            if (!(JsonReader.Parse(json) is Dictionary<string, object> rootObj))
                throw new FormatException("ink JSON: top level must be an object");

            if (!rootObj.TryGetValue("inkVersion", out var versionObj) || !(versionObj is int version))
                throw new FormatException("ink JSON: 'inkVersion' not found. Is this a compiled .ink.json file?");
            if (version > MaxFormat)
                throw new FormatException($"ink JSON format v{version} is newer than this engine supports (v{MaxFormat}).");
            if (version < MinFormat)
                throw new FormatException($"ink JSON format v{version} is too old (minimum v{MinFormat}).");

            if (!rootObj.TryGetValue("root", out var rootToken) || !(rootToken is List<object> rootArray))
                throw new FormatException("ink JSON: 'root' container not found.");

            var result = new Result { Format = version };
            result.Lists = rootObj.TryGetValue("listDefs", out var defsObj) && defsObj is Dictionary<string, object> defs
                ? ReadListDefinitions(defs)
                : ListDefinitions.Empty;

            var ctx = new Context(result.Lists);
            result.Root = ctx.ReadContainer(rootArray);
            ctx.Finish(result);
            return result;
        }

        static ListDefinitions ReadListDefinitions(Dictionary<string, object> defs)
        {
            var list = new List<ListDefinition>(defs.Count);
            foreach (var kv in defs)
            {
                var items = new Dictionary<string, int>();
                foreach (var item in (Dictionary<string, object>)kv.Value) items.Add(item.Key, (int)item.Value);
                list.Add(new ListDefinition(kv.Key, items));
            }
            return new ListDefinitions(list);
        }

        sealed class Context
        {
            readonly ListDefinitions _lists;
            readonly List<Container> _containers = new List<Container>();
            readonly List<DivertNode> _diverts = new List<DivertNode>();
            readonly List<ChoicePointNode> _choices = new List<ChoicePointNode>();
            readonly List<ReadCountNode> _readCounts = new List<ReadCountNode>();
            readonly List<VarRefNode> _varRefs = new List<VarRefNode>();
            readonly List<VarAssignNode> _varAssigns = new List<VarAssignNode>();
            readonly List<(LiteralNode node, string path)> _targetLiterals = new List<(LiteralNode, string)>();
            readonly Dictionary<string, DivertTarget> _targets = new Dictionary<string, DivertTarget>();
            Container _root;

            public Context(ListDefinitions lists) => _lists = lists;

            public Container ReadContainer(List<object> arr)
            {
                var c = new Container { Id = _containers.Count };
                _containers.Add(c);

                int count = arr.Count - 1; // the last element is the terminator (null or dict)
                var content = new Node[Math.Max(count, 0)];
                for (int i = 0; i < count; i++)
                {
                    var node = ReadNode(arr[i]) ?? throw new FormatException("ink JSON: null content in container");
                    node.Parent = c;
                    node.Index = i;
                    content[i] = node;
                }
                c.Content = content;
                c.Named = new Dictionary<string, Node>();

                // Named children that are also in content (containers carrying "#n").
                foreach (var node in content)
                    if (node is Container child && child.HasName)
                        c.Named[child.Name] = child;

                if (count >= 0 && arr[arr.Count - 1] is Dictionary<string, object> terminator)
                {
                    foreach (var kv in terminator)
                    {
                        if (kv.Key == "#f") { c.CountFlags = (int)kv.Value; continue; }
                        if (kv.Key == "#n") { c.Name = (string)kv.Value; continue; }
                        var named = ReadNode(kv.Value);
                        if (named is Container sub) sub.Name = kv.Key;
                        named.Parent = c;
                        named.Index = -1;
                        c.Named[kv.Key] = named;
                    }
                }
                return c;
            }

            Node ReadNode(object tok)
            {
                switch (tok)
                {
                    case int i: return new LiteralNode(Value.Int(i));
                    case float f: return new LiteralNode(Value.Float(f));
                    case bool b: return new LiteralNode(Value.Bool(b));
                    case string s: return ReadStringToken(s);
                    case List<object> arr: return ReadContainer(arr);
                    case Dictionary<string, object> obj: return ReadObjectToken(obj);
                    case null: return null;
                }
                throw new FormatException("ink JSON: unexpected token " + tok);
            }

            Node ReadStringToken(string s)
            {
                if (s.Length > 0 && s[0] == '^') return new TextNode(s.Substring(1));
                if (s == "\n") return new TextNode("\n");
                if (s == "<>") return new GlueNode();
                if (Commands.TryGetValue(s, out var cmd)) return new CommandNode(cmd);
                if (NativeOps.TryGet(s, out var op, out int arity)) return new NativeNode(op, arity, s == "L^" ? "^" : s);
                if (s == "void") return new LiteralNode(Value.Void);
                throw new FormatException("ink JSON: unknown token '" + s + "'");
            }

            Node ReadObjectToken(Dictionary<string, object> obj)
            {
                if (obj.TryGetValue("^->", out var targetPath))
                {
                    var lit = new LiteralNode(Value.None); // patched once paths can be resolved
                    _targetLiterals.Add((lit, (string)targetPath));
                    return lit;
                }
                if (obj.TryGetValue("^var", out var varName))
                {
                    int ci = obj.TryGetValue("ci", out var ciObj) ? (int)ciObj : -1;
                    return new LiteralNode(Value.Pointer((string)varName, ci));
                }

                if (TryReadDivert(obj, out var divert)) return divert;

                if (obj.TryGetValue("*", out var choicePath))
                {
                    int flags = obj.TryGetValue("flg", out var flg) ? (int)flg : 0;
                    var cp = new ChoicePointNode
                    {
                        TargetPathRaw = (string)choicePath,
                        HasCondition = (flags & 1) != 0,
                        HasStartContent = (flags & 2) != 0,
                        HasChoiceOnlyContent = (flags & 4) != 0,
                        IsInvisibleDefault = (flags & 8) != 0,
                        OnceOnly = (flags & 16) != 0,
                    };
                    _choices.Add(cp);
                    return cp;
                }

                if (obj.TryGetValue("VAR?", out var refName))
                {
                    var vr = new VarRefNode((string)refName);
                    _varRefs.Add(vr);
                    return vr;
                }
                if (obj.TryGetValue("CNT?", out var cntPath))
                {
                    var rc = new ReadCountNode((string)cntPath);
                    _readCounts.Add(rc);
                    return rc;
                }

                bool isGlobal = obj.TryGetValue("VAR=", out var assignName);
                if (isGlobal || obj.TryGetValue("temp=", out assignName))
                {
                    var va = new VarAssignNode((string)assignName, isGlobal, !obj.ContainsKey("re"));
                    _varAssigns.Add(va);
                    return va;
                }

                if (obj.TryGetValue("#", out var tagText)) return new TagNode((string)tagText);

                if (obj.TryGetValue("list", out var listObj))
                {
                    var list = new InkList();
                    foreach (var kv in (Dictionary<string, object>)listObj)
                        list.Add(InkListItem.Parse(kv.Key), (int)kv.Value);
                    if (list.Count == 0 && obj.TryGetValue("origins", out var originsObj))
                    {
                        var names = new List<string>();
                        foreach (var n in (List<object>)originsObj) names.Add((string)n);
                        list = InkList.WithOrigins(names);
                    }
                    return new LiteralNode(Value.List(list));
                }

                throw new FormatException("ink JSON: unknown object token");
            }

            bool TryReadDivert(Dictionary<string, object> obj, out DivertNode divert)
            {
                divert = null;
                object target;
                var d = new DivertNode();
                if (obj.TryGetValue("->", out target)) { }
                else if (obj.TryGetValue("f()", out target)) { d.PushesToStack = true; d.StackType = PushPop.Function; }
                else if (obj.TryGetValue("->t->", out target)) { d.PushesToStack = true; d.StackType = PushPop.Tunnel; }
                else if (obj.TryGetValue("x()", out target))
                {
                    d.IsExternal = true;
                    d.StackType = PushPop.Function;
                    if (obj.TryGetValue("exArgs", out var args)) d.ExternalArgs = (int)args;
                }
                else return false;

                var str = target.ToString();
                if (obj.ContainsKey("var")) d.VariableName = str; else d.TargetPath = str;
                d.IsConditional = obj.ContainsKey("c");
                _diverts.Add(d);
                divert = d;
                return true;
            }

            public void Finish(Result result)
            {
                _root = result.Root;
                AssignPaths(_root, "");

                foreach (var d in _diverts)
                {
                    if (d.TargetPath == null || d.IsExternal) continue;
                    var comps = ParsePath(d.TargetPath, out bool relative);
                    var found = ResolveFrom(d, comps, relative, out _);
                    d.Target = comps.Length > 0 && comps[comps.Length - 1].IsIndex
                        ? new Pointer(found?.Parent, comps[comps.Length - 1].Index)
                        : found is Container fc ? Pointer.StartOf(fc) : Pointer.Null;
                }

                foreach (var cp in _choices)
                {
                    var comps = ParsePath(cp.TargetPathRaw, out bool relative);
                    cp.Target = ResolveFrom(cp, comps, relative, out _) as Container;
                }

                foreach (var rc in _readCounts)
                {
                    var comps = ParsePath(rc.TargetPath, out bool relative);
                    rc.Target = ResolveFrom(rc, comps, relative, out _) as Container;
                }

                // Globals are whatever "global decl" declares, in declaration order.
                var slots = new Dictionary<string, int>();
                var names = new List<string>();
                if (_root.Named.TryGetValue("global decl", out var decl) && decl is Container declContainer)
                {
                    foreach (var node in declContainer.Content)
                    {
                        if (node is VarAssignNode va && va.IsGlobal && !slots.ContainsKey(va.Name))
                        {
                            slots[va.Name] = names.Count;
                            names.Add(va.Name);
                        }
                    }
                }
                foreach (var va in _varAssigns)
                    if (slots.TryGetValue(va.Name, out int s)) va.GlobalSlot = s;
                foreach (var vr in _varRefs)
                {
                    if (slots.TryGetValue(vr.Name, out int s)) vr.GlobalSlot = s;
                    else vr.ListItem = _lists.FindSingleItemList(vr.Name);
                }

                foreach (var (lit, path) in _targetLiterals)
                    lit.Value = Value.Divert(TargetFor(path));

                result.Containers = _containers.ToArray();
                result.GlobalNames = names.ToArray();
                result.GlobalSlots = slots;
                result.Targets = _targets;
            }

            DivertTarget TargetFor(string path)
            {
                if (!_targets.TryGetValue(path, out var t))
                {
                    t = MakeTarget(_root, path);
                    _targets[path] = t;
                }
                return t;
            }

            static void AssignPaths(Container c, string path)
            {
                c.Path = path;
                foreach (var node in c.Content)
                    if (node is Container child)
                        AssignPaths(child, Join(path, child.HasName ? child.Name : child.Index.ToString(CultureInfo.InvariantCulture)));
                foreach (var kv in c.Named)
                    if (kv.Value is Container named && named.Index < 0)
                        AssignPaths(named, Join(path, kv.Key));
            }

            Node ResolveFrom(Node from, PathComponent[] comps, bool relative, out bool approximate)
            {
                if (!relative) return ContentAtPath(_root, comps, 0, comps.Length, out approximate);
                // Relative to a non-container: the leading "^" means "my container".
                var start = from as Container ?? from.Parent;
                int skip = from is Container ? 0 : 1;
                return ContentAtPath(start, comps, skip, comps.Length, out approximate);
            }
        }

        static string Join(string head, string tail) => head.Length == 0 ? tail : head + "." + tail;

        internal readonly struct PathComponent
        {
            public readonly int Index;   // >= 0 for index components
            public readonly string Name; // name, or "^" for parent

            public PathComponent(int index, string name)
            {
                Index = index;
                Name = name;
            }

            public bool IsIndex => Index >= 0;
            public bool IsParent => Name == "^";
        }

        internal static PathComponent[] ParsePath(string path, out bool relative)
        {
            relative = path.Length > 0 && path[0] == '.';
            var body = relative ? path.Substring(1) : path;
            if (body.Length == 0) return Array.Empty<PathComponent>();
            var parts = body.Split('.');
            var comps = new PathComponent[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                comps[i] = int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out int idx)
                    ? new PathComponent(idx, null)
                    : new PathComponent(-1, parts[i]);
            return comps;
        }

        /// <summary>
        /// Walks <paramref name="comps"/>[from..to) from <paramref name="start"/>. On a miss it
        /// stops and returns the deepest object reached, flagging the result approximate —
        /// the same recovery rule as the reference runtime.
        /// </summary>
        internal static Node ContentAtPath(Container start, PathComponent[] comps, int from, int to, out bool approximate)
        {
            approximate = false;
            Node current = start;
            var container = start;
            for (int i = from; i < to; i++)
            {
                if (container == null) { approximate = true; break; }
                var c = comps[i];
                Node found;
                if (c.IsIndex) found = c.Index < container.Content.Length ? container.Content[c.Index] : null;
                else if (c.IsParent) found = container.Parent;
                else container.Named.TryGetValue(c.Name, out found);

                if (found == null) { approximate = true; break; }
                var next = found as Container;
                if (i < to - 1 && next == null) { approximate = true; break; }
                current = found;
                container = next;
            }
            return current;
        }

        /// <summary>
        /// Resolves an absolute path the way a runtime divert-by-value does: an index tail
        /// addresses a slot in its container, a name tail addresses the container itself.
        /// </summary>
        internal static DivertTarget MakeTarget(Container root, string path)
        {
            var comps = ParsePath(path, out _);
            Pointer pointer = Pointer.Null;
            if (comps.Length > 0)
            {
                if (comps[comps.Length - 1].IsIndex)
                {
                    var holder = ContentAtPath(root, comps, 0, comps.Length - 1, out _) as Container;
                    if (holder != null && !(holder == root && comps.Length > 1))
                        pointer = new Pointer(holder, comps[comps.Length - 1].Index);
                }
                else
                {
                    var c = ContentAtPath(root, comps, 0, comps.Length, out _) as Container;
                    if (c != null && c != root) pointer = new Pointer(c, -1);
                }
            }
            var exact = ContentAtPath(root, comps, 0, comps.Length, out bool approximate);
            return new DivertTarget(path, pointer, approximate ? null : exact as Container);
        }
    }
}
