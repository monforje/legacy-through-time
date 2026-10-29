using System;
using System.Collections.Generic;
using System.Text;
using LegacyThroughTime.Narrative.Json;

namespace LegacyThroughTime.Prototype
{
    /// The stream of /api/chat (server-sent events): `sources`, then `delta` {text} as the hero speaks, then `done`
    /// {flags}, or `error` {message}. Chunks come in as the network gives them; whole events come out.
    sealed class ChatStream
    {
        public readonly struct Event
        {
            public readonly string Name;
            public readonly Dictionary<string, object> Data;
            public Event(string name, Dictionary<string, object> data) { Name = name; Data = data; }

            public string Text => Data != null && Data.TryGetValue("text", out var t) ? t as string : null;
            public string Message => Data != null && Data.TryGetValue("message", out var m) ? m as string : null;

            /// The kinds of the checks her reply did not pass ("crisis", "future", ...).
            public List<string> Flags
            {
                get
                {
                    var kinds = new List<string>();
                    if (Data != null && Data.TryGetValue("flags", out var f) && f is List<object> list)
                        foreach (var item in list)
                            if (item is Dictionary<string, object> flag && flag.TryGetValue("kind", out var k) && k is string s) kinds.Add(s);
                    return kinds;
                }
            }
        }

        readonly StringBuilder buffer = new();

        public List<Event> Feed(string chunk)
        {
            var events = new List<Event>();
            buffer.Append(chunk.Replace("\r", ""));        // "\r\n" may be cut between two chunks; JSON has no raw \r
            var text = buffer.ToString();
            int end, start = 0;
            while ((end = text.IndexOf("\n\n", start, StringComparison.Ordinal)) >= 0)
            {
                var e = Parse(text.Substring(start, end - start));
                if (e.HasValue) events.Add(e.Value);
                start = end + 2;
            }
            buffer.Remove(0, start);
            return events;
        }

        static Event? Parse(string block)
        {
            string name = "message", data = null;
            foreach (var line in block.Split('\n'))
            {
                if (line.StartsWith("event:", StringComparison.Ordinal)) name = line.Substring(6).Trim();
                else if (line.StartsWith("data:", StringComparison.Ordinal)) data = (data == null ? "" : data + "\n") + line.Substring(5).TrimStart();
            }
            if (data == null) return null;
            try { return new Event(name, JsonReader.Parse(data) as Dictionary<string, object>); }
            catch (FormatException) { return null; }
        }
    }
}
