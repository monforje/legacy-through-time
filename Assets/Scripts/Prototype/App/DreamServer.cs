using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace LegacyThroughTime.Prototype
{
    /// The heroine chat server (chat/, `task chat:deploy`): the model's key lives there, never in the game. The game
    /// asks it for the night, streams her replies, has her memory of a dream written, sends the replies a reader
    /// reports. The Editor talks to `task chat:serve` on this machine; "dream:server" in PlayerPrefs points any build
    /// to another server.
    static class DreamServer
    {
        const string Public = "https://tender.aag.company/heroine/";
        /// Keeps out those who only find the address; the server limits the messages of one address besides.
        const string Token = "5433f6b2f99b7f1cc3195e923a04e3d9";

        static string Url => PlayerPrefs.GetString("dream:server", Application.isEditor ? "http://localhost:8765/" : Public);

        static DreamNight night;            // asked once a session

        public static IEnumerator Night(Action<DreamNight> done, Action<string> fail)
        {
            if (night != null) { done(night); yield break; }
            using var request = UnityWebRequest.Get(Url + "api/night");
            request.timeout = 15;
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success) { fail(Problem(request)); yield break; }
            try { night = DreamNight.FromJson(request.downloadHandler.text); }
            catch (Exception e) { fail("Сон не пришёл: " + e.Message); yield break; }
            done(night);
        }

        /// Her reply as it comes: `delta` for every piece of the text, `done` with the kinds of the checks it failed
        /// (`crisis`: she left her role to help), `fail` with a line for the reader.
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
                    case "error": ended = true; Debug.LogWarning("Dream: " + e.Message); fail("Голос не долетел. Попробуйте ещё раз"); break;
                }
            });
            using var request = Post("api/chat", body, handler);
            request.timeout = 90;
            yield return request.SendWebRequest();
            if (ended) yield break;
            if (request.result != UnityWebRequest.Result.Success) fail(Problem(request));
            else fail("Сон оборвался. Попробуйте ещё раз");
        }

        /// What she keeps of the dream; null on any failure (it is asked again next time).
        public static IEnumerator Summary(string body, Action<string> done)
        {
            using var request = Post("api/summary", body, new DownloadHandlerBuffer());
            request.timeout = 60;
            yield return request.SendWebRequest();
            string memory = null;
            if (request.result == UnityWebRequest.Result.Success)
                try { memory = JsonUtility.FromJson<Memory>(request.downloadHandler.text)?.memory; }
                catch (Exception e) { Debug.LogWarning("Dream summary: " + e.Message); }
            else Debug.LogWarning("Dream summary: " + request.error);
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
            Debug.LogWarning($"Dream: {request.url}: {request.responseCode} {request.error}");
            return request.responseCode switch
            {
                429 => "Голос устал. Вернитесь к Сююмбике позже",
                0 => "Голос не слышен. Проверьте интернет",
                _ => "Голос не долетел. Попробуйте ещё раз",
            };
        }

        /// The stream of server-sent events, read as it arrives (on the main thread); the UTF-8 of a letter may be cut
        /// between two pieces, the decoder keeps it.
        sealed class Stream : DownloadHandlerScript
        {
            readonly Decoder utf8 = Encoding.UTF8.GetDecoder();
            readonly DreamStream events = new();
            readonly Action<DreamStream.Event> on;
            readonly char[] chars = new char[16384];

            public Stream(Action<DreamStream.Event> on) : base(new byte[4096]) { this.on = on; }

            protected override bool ReceiveData(byte[] data, int length)
            {
                if (data == null || length == 0) return true;
                var n = utf8.GetChars(data, 0, length, chars, 0);
                foreach (var e in events.Feed(new string(chars, 0, n))) on(e);
                return true;
            }
        }
    }

    /// The dream of each story in PlayerPrefs (a small JSON); only on this phone.
    static class DreamStore
    {
        static string Key(string story) => "dream:" + story;

        public static Dream Read(string story)
        {
            var json = PlayerPrefs.GetString(Key(story), "");
            if (json.Length == 0) return null;
            try { return JsonUtility.FromJson<Dream>(json); }
            catch (Exception e) { Debug.LogWarning($"The dream of {story} is unreadable: {e.Message}"); return null; }
        }

        public static void Write(string story, Dream dream)
        {
            PlayerPrefs.SetString(Key(story), JsonUtility.ToJson(dream));
            PlayerPrefs.Save();
        }

        /// Her memory of what is not remembered yet, then the dream is saved with it; `done` when it is over either way.
        public static IEnumerator Remember(string story, Dream dream, Action done = null)
        {
            if (dream.Unremembered)
            {
                var upTo = dream.lines.Count;
                string memory = null;
                yield return DreamServer.Summary(dream.SummaryBody(), m => memory = m);
                if (!string.IsNullOrWhiteSpace(memory))
                {
                    dream.memory = memory;
                    dream.remembered = upTo;
                    Write(story, dream);
                }
            }
            done?.Invoke();
        }
    }
}
