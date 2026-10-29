using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace LegacyThroughTime.Prototype
{
    /// The hero chat server (chat/, `task chat:deploy`): the model's key lives there, never in the game. The game
    /// streams the hero's replies, has the memory of a talk written, sends the replies a reader reports. The Editor
    /// talks to `task chat:serve` on this machine; "chat:server" in PlayerPrefs points any build to another server.
    static class ChatServer
    {
        const string Public = "https://tender.aag.company/heroine/";
        /// Keeps out those who only find the address; the server limits the messages of one address besides.
        const string Token = "5433f6b2f99b7f1cc3195e923a04e3d9";

        static string Url => PlayerPrefs.GetString("chat:server", Application.isEditor ? "http://localhost:8765/" : Public);

        /// The hero's reply as it comes: `delta` for every piece of the text, `done` with the kinds of the checks it
        /// failed (`crisis`: the hero left the role to help), `fail` with a line for the reader.
        public static IEnumerator Chat(string body, Action<string> delta, Action<List<string>> done, Action<string> fail)
        {
            var ended = false;
            var handler = new Stream(e =>
            {
                if (ended) return;
                switch (e.Name)
                {
                    case "delta": if (e.Text != null) delta(e.Text); break;
                    case "done": ended = true; done(e.Flags); break;
                    case "error": ended = true; Debug.LogWarning("Chat: " + e.Message); fail("Ответ не дошёл. Попробуйте ещё раз"); break;
                }
            });
            using var request = Post("api/chat", body, handler);
            request.timeout = 90;
            yield return request.SendWebRequest();
            if (ended) yield break;
            if (request.result != UnityWebRequest.Result.Success) fail(Problem(request));
            else fail("Ответ оборвался. Попробуйте ещё раз");
        }

        /// What the hero keeps of the talk; null on any failure (it is asked again next time).
        public static IEnumerator Summary(string body, Action<string> done)
        {
            using var request = Post("api/summary", body, new DownloadHandlerBuffer());
            request.timeout = 60;
            yield return request.SendWebRequest();
            string memory = null;
            if (request.result == UnityWebRequest.Result.Success)
                try { memory = JsonUtility.FromJson<Memory>(request.downloadHandler.text)?.memory; }
                catch (Exception e) { Debug.LogWarning("Chat summary: " + e.Message); }
            else Debug.LogWarning("Chat summary: " + request.error);
            done(memory);
        }

        public static IEnumerator Report(string body, Action<bool> done)
        {
            using var request = Post("api/report", body, new DownloadHandlerBuffer());
            request.timeout = 20;
            yield return request.SendWebRequest();
            done(request.result == UnityWebRequest.Result.Success);
        }

        [Serializable] sealed class Memory { public string memory = null; }   // filled by JsonUtility

        static UnityWebRequest Post(string path, string body, DownloadHandler handler)
        {
            var request = new UnityWebRequest(Url + path, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)) { contentType = "application/json" },
                downloadHandler = handler,
            };
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-Game-Token", Token);
            return request;
        }

        static string Problem(UnityWebRequest request)
        {
            Debug.LogWarning($"Chat: {request.url}: {request.responseCode} {request.error}");
            return request.responseCode switch
            {
                429 => "Герой устал. Загляните позже",
                0 => "Нет связи. Проверьте интернет",
                _ => "Ответ не дошёл. Попробуйте ещё раз",
            };
        }

        /// The stream of server-sent events, read as it arrives (on the main thread); the UTF-8 of a letter may be cut
        /// between two pieces, the decoder keeps it.
        sealed class Stream : DownloadHandlerScript
        {
            readonly Decoder utf8 = Encoding.UTF8.GetDecoder();
            readonly ChatStream events = new();
            readonly Action<ChatStream.Event> on;
            readonly char[] chars = new char[16384];

            public Stream(Action<ChatStream.Event> on) : base(new byte[4096]) { this.on = on; }

            protected override bool ReceiveData(byte[] data, int length)
            {
                if (data == null || length == 0) return true;
                var n = utf8.GetChars(data, 0, length, chars, 0);
                foreach (var e in events.Feed(new string(chars, 0, n))) on(e);
                return true;
            }
        }
    }

    /// The chat of each story in PlayerPrefs (a small JSON), and the reader's energy; only on this phone.
    static class ChatStore
    {
        static string Key(string story) => "chat:" + story;

        public static Chat Read(string story)
        {
            var json = PlayerPrefs.GetString(Key(story), "");
            if (json.Length == 0) return null;
            try { return JsonUtility.FromJson<Chat>(json); }
            catch (Exception e) { Debug.LogWarning($"The chat of {story} is unreadable: {e.Message}"); return null; }
        }

        /// The chat of a story, begun with the author's first line when there is none yet (and written).
        public static Chat Open(StoryEntry story)
        {
            var chat = Read(story.Id);
            if (chat != null) return chat;
            chat = Chat.Begin(story.Id, story.Opening, Progress.Now());
            Write(chat);
            return chat;
        }

        public static void Write(Chat chat)
        {
            PlayerPrefs.SetString(Key(chat.story), JsonUtility.ToJson(chat));
            PlayerPrefs.Save();
        }

        /// What the hero keeps of the lines not remembered yet, then the chat is saved with it; `done` either way.
        public static IEnumerator Remember(Chat chat, Action done = null)
        {
            if (chat.Unremembered)
            {
                var upTo = chat.lines.Count;
                string memory = null;
                yield return ChatServer.Summary(chat.SummaryBody(), m => memory = m);
                if (!string.IsNullOrWhiteSpace(memory))
                {
                    chat.memory = memory;
                    chat.remembered = upTo;
                    Write(chat);
                }
            }
            done?.Invoke();
        }

        /// The rows of the "Чаты" list: heroes of the stories the reader has begun.
        public static List<ChatRow> Rows(IEnumerable<StoryEntry> stories, Func<StoryEntry, bool> started) =>
            ChatList.Build(stories, id => started(StoryCatalog.Find(id)), Read);

        // ------------------------------------------------------------ energy
        const string EnergyKey = "chat:energy";

        public static Energy Energy
        {
            get
            {
                var json = PlayerPrefs.GetString(EnergyKey, "");
                Energy energy = null;
                try { if (json.Length > 0) energy = JsonUtility.FromJson<Energy>(json); }
                catch (Exception e) { Debug.LogWarning("The energy is unreadable: " + e.Message); }
                energy ??= new Energy();
                energy.NewDay(ShopCatalog.Day(DateTime.Now));          // the stub of the refill, until the economy is made
                return energy;
            }
        }

        public static void Write(Energy energy)
        {
            PlayerPrefs.SetString(EnergyKey, JsonUtility.ToJson(energy));
            PlayerPrefs.Save();
        }
    }
}
