using System;
using System.Collections.Generic;
using LegacyThroughTime.Narrative.Json;

namespace LegacyThroughTime.Narrative.Ink
{
    /// <summary>
    /// Save format for <see cref="InkRunner"/>. Positions are stored as (container path, index),
    /// counts by container path, globals by name and only when they differ from the story's
    /// defaults — so a save survives edits to unrelated parts of the story and picks up new
    /// variables with their declared defaults.
    /// </summary>
    static class InkSave
    {
        public const int Version = 1;

        public static string Write(InkRunner runner)
        {
            var story = runner.Story;
            var store = runner.Store;
            var c = runner.Cursor;
            var w = new JsonWriter();

            w.BeginObject();
            w.Property("inkSave", Version);
            w.Property("turn", c.TurnIndex);
            w.Property("seed", c.Seed);
            w.Property("previousRandom", c.PreviousRandom);

            w.Key("globals").BeginObject();
            for (int i = 0; i < story.GlobalNames.Length; i++)
            {
                if (store.Globals[i].SameAs(store.Defaults[i])) continue;
                w.Key(story.GlobalNames[i]);
                WriteValue(w, store.Globals[i]);
            }
            w.EndObject();

            w.Key("visits").BeginObject();
            foreach (var container in story.Containers)
                if (store.Visits[container.Id] != 0) w.Property(container.Path, store.Visits[container.Id]);
            w.EndObject();

            w.Key("turns").BeginObject();
            foreach (var container in story.Containers)
                if (store.Turns[container.Id] != Store.NoTurn) w.Property(container.Path, store.Turns[container.Id]);
            w.EndObject();

            w.Key("threads").BeginArray();
            foreach (var t in c.CallStack.Threads) WriteThread(w, t);
            w.EndArray();
            w.Property("threadCounter", c.CallStack.ThreadCounter);

            w.Key("output").BeginArray();
            foreach (var o in c.Output) WriteOutItem(w, o);
            w.EndArray();

            w.Key("choices").BeginArray();
            foreach (var ch in c.Choices)
            {
                w.BeginObject();
                w.Property("text", ch.Text);
                w.Property("target", ch.Target?.Path);
                w.Property("source", ch.SourcePath);
                if (ch.IsInvisibleDefault) w.Property("invisible", true);
                if (ch.Tags != null)
                {
                    w.Key("tags").BeginArray();
                    foreach (var tag in ch.Tags) w.String(tag);
                    w.EndArray();
                }
                w.Key("thread");
                WriteThread(w, ch.Thread);
                w.EndObject();
            }
            w.EndArray();

            w.Key("eval").BeginArray();
            foreach (var v in c.Eval) WriteValue(w, v);
            w.EndArray();

            if (!c.Diverted.IsNull)
            {
                w.Key("diverted");
                WritePointer(w, c.Diverted);
            }
            w.EndObject();
            return w.ToString();
        }

        public static void Read(InkRunner runner, string json) =>
            Read(runner, JsonReader.Parse(json) as Dictionary<string, object>);

        public static void Read(InkRunner runner, Dictionary<string, object> root)
        {
            var story = runner.Story;
            var store = runner.Store;
            if (root == null || !root.TryGetValue("inkSave", out var ver))
                throw new FormatException("save: not an ink save");
            if ((int)ver > Version) throw new FormatException("save: version " + ver + " is newer than this engine");

            var cursor = new Cursor(story.Root)
            {
                TurnIndex = (int)root["turn"],
                Seed = (int)root["seed"],
                PreviousRandom = (int)root["previousRandom"],
            };

            Array.Copy(store.Defaults, store.Globals, store.Globals.Length);
            foreach (var kv in (Dictionary<string, object>)root["globals"])
            {
                int slot = story.GlobalSlot(kv.Key);
                if (slot >= 0) store.Globals[slot] = ReadValue(story, kv.Value);
            }

            Array.Clear(store.Visits, 0, store.Visits.Length);
            foreach (var kv in (Dictionary<string, object>)root["visits"])
            {
                var container = story.ContainerAt(kv.Key);
                if (container != null) store.Visits[container.Id] = (int)kv.Value;
            }

            for (int i = 0; i < store.Turns.Length; i++) store.Turns[i] = Store.NoTurn;
            foreach (var kv in (Dictionary<string, object>)root["turns"])
            {
                var container = story.ContainerAt(kv.Key);
                if (container != null) store.Turns[container.Id] = (int)kv.Value;
            }

            var threads = new List<InkThread>();
            foreach (var t in (List<object>)root["threads"]) threads.Add(ReadThread(story, (Dictionary<string, object>)t));
            cursor.CallStack.LoadThreads(threads, (int)root["threadCounter"]);

            var output = new List<OutItem>();
            foreach (var o in (List<object>)root["output"]) output.Add(ReadOutItem(o));
            cursor.ResetOutput(output);

            foreach (Dictionary<string, object> ch in (List<object>)root["choices"])
            {
                var target = story.ContainerAt((string)ch["target"]) ?? throw new FormatException("save: choice target not found: " + ch["target"]);
                List<string> tags = null;
                if (ch.TryGetValue("tags", out var tagsObj))
                {
                    tags = new List<string>();
                    foreach (var tag in (List<object>)tagsObj) tags.Add((string)tag);
                }
                var thread = ReadThread(story, (Dictionary<string, object>)ch["thread"]);
                cursor.Choices.Add(new ChoiceRecord
                {
                    Text = (string)ch["text"],
                    Target = target,
                    SourcePath = (string)ch["source"],
                    IsInvisibleDefault = ch.ContainsKey("invisible"),
                    Tags = tags,
                    Thread = thread,
                    OriginalThreadIndex = thread.Index,
                });
            }

            foreach (var v in (List<object>)root["eval"]) cursor.Eval.Add(ReadValue(story, v));
            if (root.TryGetValue("diverted", out var diverted)) cursor.Diverted = ReadPointer(story, diverted);

            runner.ReplaceState(cursor);
        }

        // ---- threads and pointers

        static void WriteThread(JsonWriter w, InkThread t)
        {
            w.BeginObject();
            w.Property("index", t.Index);
            if (!t.Previous.IsNull)
            {
                w.Key("previous");
                WritePointer(w, t.Previous);
            }
            w.Key("frames").BeginArray();
            foreach (var f in t.Frames)
            {
                w.BeginObject();
                w.Property("type", (int)f.Type);
                if (!f.Pointer.IsNull)
                {
                    w.Key("pointer");
                    WritePointer(w, f.Pointer);
                }
                if (f.InExpression) w.Property("exp", true);
                if (f.EvalHeightWhenPushed != 0) w.Property("evalHeight", f.EvalHeightWhenPushed);
                if (f.FunctionStartInOutput != 0) w.Property("functionStart", f.FunctionStartInOutput);
                if (f.Temps != null && f.Temps.Count > 0)
                {
                    w.Key("temps").BeginObject();
                    foreach (var kv in f.Temps)
                    {
                        w.Key(kv.Key);
                        WriteValue(w, kv.Value);
                    }
                    w.EndObject();
                }
                w.EndObject();
            }
            w.EndArray();
            w.EndObject();
        }

        static InkThread ReadThread(InkStory story, Dictionary<string, object> obj)
        {
            var t = new InkThread { Index = (int)obj["index"] };
            if (obj.TryGetValue("previous", out var prev)) t.Previous = ReadPointer(story, prev);
            foreach (Dictionary<string, object> f in (List<object>)obj["frames"])
            {
                var frame = new Frame((PushPop)(int)f["type"], f.TryGetValue("pointer", out var p) ? ReadPointer(story, p) : Pointer.Null)
                {
                    InExpression = f.ContainsKey("exp"),
                    EvalHeightWhenPushed = f.TryGetValue("evalHeight", out var eh) ? (int)eh : 0,
                    FunctionStartInOutput = f.TryGetValue("functionStart", out var fs) ? (int)fs : 0,
                };
                if (f.TryGetValue("temps", out var temps))
                {
                    frame.Temps = new Dictionary<string, Value>();
                    foreach (var kv in (Dictionary<string, object>)temps) frame.Temps[kv.Key] = ReadValue(story, kv.Value);
                }
                t.Frames.Add(frame);
            }
            return t;
        }

        static void WritePointer(JsonWriter w, Pointer p) =>
            w.BeginArray().String(p.Container.Path).Int(p.Index).EndArray();

        static Pointer ReadPointer(InkStory story, object token)
        {
            var arr = (List<object>)token;
            var path = (string)arr[0];
            var container = story.ContainerAt(path) ?? throw new FormatException("save: story position not found: '" + path + "'. Has the story changed since this save?");
            return new Pointer(container, (int)arr[1]);
        }

        // ---- values (same token shapes as the ink format itself)

        static void WriteValue(JsonWriter w, in Value v)
        {
            switch (v.Kind)
            {
                case ValueKind.Int: w.Int(v.I); break;
                case ValueKind.Float: w.Float(v.F); break;
                case ValueKind.Bool: w.Bool(v.I != 0); break;
                case ValueKind.String: w.String("^" + v.Str); break;
                case ValueKind.Void: w.String("void"); break;
                case ValueKind.Tag: w.BeginObject().Property("#", v.Str).EndObject(); break;
                case ValueKind.DivertTarget: w.BeginObject().Property("^->", v.Target.Path).EndObject(); break;
                case ValueKind.VariablePointer: w.BeginObject().Property("^var", v.Str).Property("ci", v.ContextIndex).EndObject(); break;
                case ValueKind.List:
                {
                    var list = v.ListValue;
                    w.BeginObject().Key("list").BeginObject();
                    foreach (var kv in list) w.Property(kv.Key.FullName, kv.Value);
                    w.EndObject();
                    if (list.Count == 0 && list.OriginNames != null)
                    {
                        w.Key("origins").BeginArray();
                        foreach (var n in list.OriginNames) w.String(n);
                        w.EndArray();
                    }
                    w.EndObject();
                    break;
                }
                default: w.Null(); break;
            }
        }

        static Value ReadValue(InkStory story, object token)
        {
            switch (token)
            {
                case int i: return Value.Int(i);
                case float f: return Value.Float(f);
                case bool b: return Value.Bool(b);
                case string s when s == "void": return Value.Void;
                case string s when s.Length > 0 && s[0] == '^': return Value.String(s.Substring(1));
                case Dictionary<string, object> obj:
                    if (obj.TryGetValue("#", out var tag)) return Value.Tag((string)tag);
                    if (obj.TryGetValue("^->", out var path)) return Value.Divert(story.TargetFor((string)path));
                    if (obj.TryGetValue("^var", out var name)) return Value.Pointer((string)name, (int)obj["ci"]);
                    if (obj.TryGetValue("list", out var items))
                    {
                        var list = new InkList();
                        foreach (var kv in (Dictionary<string, object>)items) list.Add(InkListItem.Parse(kv.Key), (int)kv.Value);
                        if (list.Count == 0 && obj.TryGetValue("origins", out var origins))
                        {
                            var names = new List<string>();
                            foreach (var n in (List<object>)origins) names.Add((string)n);
                            list = InkList.WithOrigins(names);
                        }
                        return Value.List(list);
                    }
                    break;
            }
            throw new FormatException("save: bad value token");
        }

        static void WriteOutItem(JsonWriter w, in OutItem o)
        {
            switch (o.Kind)
            {
                case OutKind.Text: w.String(o.IsNewline ? "\n" : "^" + o.Text); break;
                case OutKind.Glue: w.String("<>"); break;
                case OutKind.BeginString: w.String("str"); break;
                case OutKind.BeginTag: w.String("#"); break;
                case OutKind.EndTag: w.String("/#"); break;
                case OutKind.Tag: w.BeginObject().Property("#", o.Text).EndObject(); break;
                default: w.String("?"); break;
            }
        }

        static OutItem ReadOutItem(object token)
        {
            switch (token)
            {
                case "\n": return OutItem.Newline;
                case "<>": return OutItem.Glue;
                case "str": return OutItem.BeginString;
                case "#": return OutItem.BeginTag;
                case "/#": return OutItem.EndTag;
                case "?": return OutItem.Other;
                case string s when s.Length > 0 && s[0] == '^': return OutItem.OfText(s.Substring(1));
                case Dictionary<string, object> obj when obj.TryGetValue("#", out var tag): return OutItem.OfTag((string)tag);
            }
            throw new FormatException("save: bad output token");
        }
    }
}
