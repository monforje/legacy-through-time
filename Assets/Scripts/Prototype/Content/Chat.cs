using System;
using System.Collections.Generic;
using System.Globalization;
using LegacyThroughTime.Narrative.Json;

namespace LegacyThroughTime.Prototype
{
    /// One line of a chat: the hero's or the reader's own.
    [Serializable]
    sealed class ChatLine
    {
        public const string Hero = "hero", Reader = "reader";

        public string role;
        public string text;
        public bool reported;       // the reader sent it to be checked (⚑)
        public long at;             // unix ms
    }

    /// A talk with the hero of a story (docs/notes/chat-rework.md): one per story, not tied to a chapter or a night.
    /// The reader speaks as themselves. The hero answers through the chat server; how they feel comes from the ink
    /// variables of the chapters read so far (State), never shown. Kept by ChatStore, one per story.
    [Serializable]
    sealed class Chat
    {
        public const int MaxLetters = 300;
        /// The lines sent with a message; the server reads only the last few anyway.
        const int Window = 30;

        public string story;
        public List<ChatLine> lines = new();
        public int unread;              // hero's lines the reader has not seen
        public string memory = "";      // what the hero keeps of the talk before the window
        public int remembered;          // lines already in the memory

        public ChatLine Last => lines.Count > 0 ? lines[lines.Count - 1] : null;
        public bool Started => lines.Exists(l => l.role == ChatLine.Reader);

        /// A new chat: the hero's first line is the author's (null or blank: the chat starts empty).
        public static Chat Begin(string story, string opening, long now)
        {
            var chat = new Chat { story = story };
            if (!string.IsNullOrWhiteSpace(opening)) chat.lines.Add(new ChatLine { role = ChatLine.Hero, text = opening, at = now });
            return chat;
        }

        /// The reader's words, cut to the limit; null when there is nothing to send.
        public static string Clean(string text)
        {
            text = (text ?? "").Trim();
            if (text.Length == 0) return null;
            return text.Length > MaxLetters ? text.Substring(0, MaxLetters) : text;
        }

        public ChatLine Say(string text, long now)
        {
            var line = new ChatLine { role = ChatLine.Reader, text = text, at = now };
            lines.Add(line);
            return line;
        }

        /// The hero's reply; unread while the chat is not on screen.
        public ChatLine Reply(string text, long now, bool seen)
        {
            var line = new ChatLine { role = ChatLine.Hero, text = text, at = now };
            lines.Add(line);
            if (!seen) unread++;
            return line;
        }

        /// No reply came: the reader's words go back to the field, nothing is spent.
        public void Take(ChatLine line) => lines.Remove(line);

        public void MarkSeen() => unread = 0;

        /// The line the flag in the header reports: the hero's last answer, not yet reported. The author's first line
        /// is not an answer; -1 when there is none.
        public int ReportableIndex()
        {
            for (var i = lines.Count - 1; i > 0; i--)
                if (lines[i].role == ChatLine.Hero) return lines[i].reported ? -1 : i;
            return -1;
        }

        /// Lines not yet in the memory, with at least one of the reader's among them.
        public bool Unremembered
        {
            get
            {
                for (var i = remembered; i < lines.Count; i++) if (lines[i].role == ChatLine.Reader) return true;
                return false;
            }
        }

        // ------------------------------------------------------------ what goes to the server
        /// POST /api/chat: the last lines (the reader's last), the ink variables as of now, the memory.
        public string ChatBody(ChapterStats stats)
        {
            var w = new JsonWriter().BeginObject();
            Lines(w.Key("history"), Math.Max(0, lines.Count - Window), lines.Count);
            State(w.Key("state"), stats);
            return w.Property("story", story ?? "").Property("memory", memory ?? "").EndObject().ToString();
        }

        /// POST /api/summary: the lines not yet remembered and the memory so far.
        public string SummaryBody()
        {
            var w = new JsonWriter().BeginObject();
            Lines(w.Key("history"), Math.Max(remembered, lines.Count - Window), lines.Count);
            return w.Property("story", story ?? "").Property("memory", memory ?? "").EndObject().ToString();
        }

        /// POST /api/report: the hero's line and what led to it.
        public string ReportBody(int index, ChapterStats stats)
        {
            var w = new JsonWriter().BeginObject().Property("message", lines[index].text).Property("reason", "reader");
            Lines(w.Key("history"), Math.Max(0, index + 1 - Window), index + 1);
            State(w.Key("state"), stats);
            return w.Property("story", story ?? "").EndObject().ToString();
        }

        void Lines(JsonWriter w, int from, int to)
        {
            w.BeginArray();
            for (var i = from; i < to; i++)
                if (!string.IsNullOrEmpty(lines[i].text))
                    w.BeginObject().Property("role", lines[i].role).Property("content", lines[i].text).EndObject();
            w.EndArray();
        }

        /// The variables as JSON values of their kind (the server keeps only those it knows).
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
    }

    /// The reader's energy: every message to a hero costs some. The economy (price, refill, limits) is still a
    /// stub of the mockups; the screens only ask `CanSend`.
    [Serializable]
    sealed class Energy
    {
        public const int Max = 10, Cost = 1;

        public int value = Max;
        public string day = "";         // "yyyy-MM-dd": the day the energy was last filled

        /// The stub of the refill: a new day fills it up. True when it did.
        public bool NewDay(string today)
        {
            if (day == today) return false;
            var changed = value < Max;
            day = today; value = Math.Max(value, Max);     // a new day fills it up to the norm, but never takes away what was bought
            return changed;
        }

        public bool CanSend => value >= Cost;

        public bool TrySpend()
        {
            if (!CanSend) return false;
            value -= Cost;
            return true;
        }

        /// A message that got no reply gives its energy back.
        public void Refund() => value = Math.Min(Max, value + Cost);

        public void Refill() => value = Math.Max(value, Max);

        /// Energy from a purchase, a reward or the wheel: it may stand above the norm (Max is the free daily fill).
        public void Add(int amount) => value += Math.Max(0, amount);
    }

    /// One row of the "Чаты" list.
    sealed class ChatRow
    {
        public StoryEntry Story;
        public string Hero;
        public string Preview;      // the last line; null when nothing was said yet
        public long At;
        public int Unread;
    }

    static class ChatList
    {
        /// Heroes of the stories the reader has started, the latest talk first; stories never talked in come after,
        /// in the order of the menu. `chatOf` gives the chat of a story (null: none yet).
        public static List<ChatRow> Build(IEnumerable<StoryEntry> stories, Func<string, bool> started, Func<string, Chat> chatOf)
        {
            var rows = new List<ChatRow>();
            foreach (var s in stories)
            {
                if (s.Soon || string.IsNullOrEmpty(s.Hero) || !started(s.Id)) continue;
                var chat = chatOf(s.Id);
                var last = chat?.Last;
                rows.Add(new ChatRow
                {
                    Story = s, Hero = s.Hero, Preview = last?.text, At = last?.at ?? 0, Unread = chat?.unread ?? 0,
                });
            }
            // List.Sort is not stable; the index keeps the menu order among equals
            var order = new Dictionary<ChatRow, int>();
            for (var i = 0; i < rows.Count; i++) order[rows[i]] = i;
            rows.Sort((a, b) => a.At != b.At ? b.At.CompareTo(a.At) : order[a].CompareTo(order[b]));
            return rows;
        }

        /// The time on a row: the hour today, "Вчера", the weekday within the week, else the date. `at` is unix ms.
        public static string When(long at, DateTime now)
        {
            if (at <= 0) return "";
            var t = DateTimeOffset.FromUnixTimeMilliseconds(at).ToLocalTime().DateTime;
            var days = (now.Date - t.Date).Days;
            if (days <= 0) return t.ToString("HH:mm", CultureInfo.InvariantCulture);
            if (days == 1) return "Вчера";
            if (days < 7) return Weekdays[(int)t.DayOfWeek];
            return t.ToString("dd.MM", CultureInfo.InvariantCulture);
        }

        static readonly string[] Weekdays = { "Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб" };

        /// Which of the ten art colours a hero's avatar takes (stable: the same story, the same colour).
        public static int ArtIndex(string id)
        {
            // the stories of the mockups keep the colours they have there (art-8, art-2, art-5)
            switch (id)
            {
                case "golden-cage": return 7;
                case "zilant": return 1;
                case "shurale": return 4;
            }
            var sum = 0;
            foreach (var c in id ?? "") sum = (sum * 31 + c) % 10007;
            return sum % 10;
        }

        /// The stats the hero's mood comes from: those of the chapter read last.
        public static ChapterStats Latest(IEnumerable<ChapterStats> all)
        {
            ChapterStats best = null;
            foreach (var s in all) if (s != null && (best == null || s.at >= best.at)) best = s;
            return best;
        }
    }
}
