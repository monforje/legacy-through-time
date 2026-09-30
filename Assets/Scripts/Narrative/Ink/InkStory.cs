using System;
using System.Collections.Generic;

namespace LegacyThroughTime.Narrative.Ink
{
    /// <summary>
    /// A compiled ink story: the immutable program. Load it once and create any number of
    /// <see cref="InkRunner"/>s (play-throughs) from it; runners never mutate it.
    /// </summary>
    public sealed class InkStory
    {
        internal readonly Container Root;
        internal readonly Container[] Containers;
        internal readonly ListDefinitions Lists;
        internal readonly string[] GlobalNames;
        internal readonly Dictionary<string, int> GlobalSlots;

        readonly Dictionary<string, DivertTarget> _targets;
        readonly object _targetsLock = new object();

        public int FormatVersion { get; }

        InkStory(InkLoader.Result r)
        {
            Root = r.Root;
            Containers = r.Containers;
            Lists = r.Lists;
            GlobalNames = r.GlobalNames;
            GlobalSlots = r.GlobalSlots;
            _targets = r.Targets;
            FormatVersion = r.Format;
        }

        /// <summary>Compiles inklecate JSON. Throws <see cref="FormatException"/> on malformed input.</summary>
        public static InkStory FromJson(string json) => new InkStory(InkLoader.Load(json));

        public IReadOnlyList<string> GlobalVariableNames => GlobalNames;

        /// <summary>Tags written at the very top of the story.</summary>
        public IReadOnlyList<string> GlobalTags => TagsAt("");

        /// <summary>Tags written at the top of a knot or <c>knot.stitch</c>.</summary>
        public IReadOnlyList<string> TagsAt(string path)
        {
            var comps = InkLoader.ParsePath(path, out _);
            var flow = InkLoader.ContentAtPath(Root, comps, 0, comps.Length, out _) as Container;
            while (flow != null && flow.Content.Length > 0 && flow.Content[0] is Container first) flow = first;
            if (flow == null) return Array.Empty<string>();

            var tags = new List<string>();
            bool inTag = false;
            foreach (var node in flow.Content)
            {
                if (node is CommandNode cmd && cmd.Cmd == Cmd.BeginTag) inTag = true;
                else if (node is CommandNode end && end.Cmd == Cmd.EndTag) inTag = false;
                else if (inTag && node is TextNode text) tags.Add(text.Text);
                else if (node is TagNode legacy) tags.Add(legacy.Text);
                else break;
            }
            return tags;
        }

        public bool HasFunction(string name) => KnotNamed(name) != null;

        internal Container KnotNamed(string name) =>
            name != null && Root.Named.TryGetValue(name, out var n) ? n as Container : null;

        internal int GlobalSlot(string name) => GlobalSlots.TryGetValue(name, out int slot) ? slot : -1;

        /// <summary>Divert target for a path, resolved once and shared (thread-safe).</summary>
        internal DivertTarget TargetFor(string path)
        {
            lock (_targetsLock)
            {
                if (!_targets.TryGetValue(path, out var t))
                {
                    t = InkLoader.MakeTarget(Root, path);
                    _targets[path] = t;
                }
                return t;
            }
        }

        /// <summary>Container at an exact path, or null.</summary>
        internal Container ContainerAt(string path)
        {
            var comps = InkLoader.ParsePath(path, out _);
            var node = InkLoader.ContentAtPath(Root, comps, 0, comps.Length, out bool approximate);
            return approximate ? null : node as Container;
        }
    }
}
