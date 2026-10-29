using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using LegacyThroughTime.Narrative.Json;

namespace LegacyThroughTime.Prototype
{
    /// One line of a dream: the heroine's ("heroine") or the reader's, the Voice of the dream ("voice").
    [Serializable]
    sealed class DreamLine
    {
        public string role;
        public string text;
        public bool reported;       // the reader sent it to be checked (⚑)
    }

    /// A talk with the heroine after a chapter (docs/brainstorm/2026-10-29-character-chat.md): the reader is the
    /// Voice of her dream, she answers through the chat server (DreamServer). One dream a day: the free replies of a
    /// day (the night's limit) and the ones bought with coins; a new day starts a new dream, and what she keeps of
    /// the old ones (`memory`, written by the server) goes into it. Kept by DreamStore, one per story.
    [Serializable]
    sealed class Dream
    {
        /// "Досмотреть сон": this many more replies for this many coins.
        public const int ExtraReplies = 5, ExtraCost = 10;
        /// The lines sent with a message; the server reads only the last few anyway.
        const int Window = 30;

        public string night;            // the night of the chat server ("ep01")
        public string day;              // "yyyy-MM-dd": the day this dream began
        public List<DreamLine> lines = new();
        public int bought;              // replies bought this day
        public string memory = "";      // what she keeps of the dreams before, and of this one up to `remembered`
        public int remembered;          // lines of this dream already in the memory

        public int Said
        {
            get
            {
                var n = 0;
                foreach (var l in lines) if (l.role == "voice") n++;
                return n;
            }
        }

        public int Left(int limit) => Math.Max(0, limit + bought - Said);

        /// Lines not yet in the memory, with at least one of the Voice's among them.
        public bool Unremembered
        {
            get
            {
                for (var i = remembered; i < lines.Count; i++) if (lines[i].role == "voice") return true;
                return false;
            }
        }

        /// The dream of this night and day; false when it is of another night or day (it is remembered first, then a
        /// new one begins).
        public bool IsOf(string nightId, string today) => night == nightId && day == today;

        /// A new dream: her first line is the author's, "again" once she has a memory of the Voice.
        public static Dream Begin(DreamNight n, string today, string memory, Random random)
        {
            var again = !string.IsNullOrWhiteSpace(memory);
            var openings = again && n.OpeningAgain.Count > 0 ? n.OpeningAgain : n.Opening;
            var dream = new Dream { night = n.Id, day = today, memory = memory ?? "" };
            if (openings.Count > 0) dream.lines.Add(new DreamLine { role = "heroine", text = openings[random.Next(openings.Count)] });
            return dream;
        }

        // ------------------------------------------------------------ what goes to the server
        /// POST /api/chat: the last lines (the Voice's last), the chapter's ink variables, the memory.
        public string ChatBody(ChapterStats stats)
        {
            var w = new JsonWriter().BeginObject();
            Lines(w.Key("history"), Math.Max(0, lines.Count - Window), lines.Count);
            State(w.Key("state"), stats);
            return w.Property("memory", memory ?? "").EndObject().ToString();
        }

        /// POST /api/summary: the lines not yet remembered and the memory so far.
        public string SummaryBody()
        {
            var w = new JsonWriter().BeginObject();
            Lines(w.Key("history"), Math.Max(remembered, lines.Count - Window), lines.Count);
            return w.Property("memory", memory ?? "").EndObject().ToString();
        }

        /// POST /api/report: her line and what led to it.
        public string ReportBody(int index, ChapterStats stats)
        {
            var w = new JsonWriter().BeginObject().Property("message", lines[index].text).Property("reason", "reader");
            Lines(w.Key("history"), Math.Max(0, index + 1 - Window), index + 1);
            State(w.Key("state"), stats);
            return w.EndObject().ToString();
        }

        void Lines(JsonWriter w, int from, int to)
        {
            w.BeginArray();
            for (var i = from; i < to; i++)
                if (!string.IsNullOrEmpty(lines[i].text))
                    w.BeginObject().Property("role", lines[i].role).Property("content", lines[i].text).EndObject();
            w.EndArray();
        }

        /// The variables as JSON values of their kind (the server keeps only those of its night).
        static void State(JsonWriter w, ChapterStats stats)
        {
            w.BeginObject();
            if (stats != null)
                foreach (var v in stats.vars)
                {
                    if (v?.name == null) continue;
                    switch (v.kind)
                    {
                        case "int" when int.TryParse(v.value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i):
                            w.Property(v.name, i); break;
                        case "float" when float.TryParse(v.value, NumberStyles.Float, CultureInfo.InvariantCulture, out var f):
                            w.Key(v.name).Float(f); break;
                        case "bool":
                            w.Property(v.name, v.value == "true"); break;
                        case "string":
                            w.Property(v.name, v.value ?? ""); break;
                    }
                }
            w.EndObject();
        }

        /// The night opens once the chapter is read, and only if she had the prophetic dream in it (the Voice is from
        /// there); a chapter read before its stats were kept counts as dreamt.
        public static bool Opens(bool finished, ChapterStats stats)
        {
            if (!finished) return false;
            if (stats == null) return true;
            foreach (var v in stats.vars) if (v?.name == "dream_flag") return v.value == "true";
            return true;
        }
    }

    /// The night as the chat server tells it (GET /api/night): written by the author, in content/stories/*/chat.
    sealed class DreamNight
    {
        public string Id, Title, Wake;
        public int Limit;
        public List<string> Opening = new(), OpeningAgain = new(), Chips = new();

        public static DreamNight FromJson(string json)
        {
            if (JsonReader.Parse(json) is not Dictionary<string, object> o) throw new FormatException("the night is not an object");
            return new DreamNight
            {
                Id = Str(o, "id"), Title = Str(o, "title"), Wake = Str(o, "wake"),
                Limit = o.TryGetValue("limit", out var l) && l is int i ? i : 10,
                Opening = Strings(o, "opening"), OpeningAgain = Strings(o, "opening_again"), Chips = Strings(o, "chips"),
            };
        }

        static string Str(Dictionary<string, object> o, string key) => o.TryGetValue(key, out var v) ? v as string : null;

        static List<string> Strings(Dictionary<string, object> o, string key)
        {
            var list = new List<string>();
            if (o.TryGetValue(key, out var v) && v is List<object> items)
                foreach (var item in items) if (item is string s) list.Add(s);
            return list;
        }
    }

    /// The stream of /api/chat (server-sent events): `sources`, then `delta` {text} as she speaks, then `done`
    /// {flags}, or `error` {message}. Chunks come in as the network gives them; whole events come out.
    sealed class DreamStream
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
