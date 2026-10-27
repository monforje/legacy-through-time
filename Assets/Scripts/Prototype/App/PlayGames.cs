using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Scripting;

namespace LegacyThroughTime.Prototype
{
    /// Google Play Games (Services v2) through Plugins/Android/PlayGames.androidlib. Play Games signs the reader in by
    /// itself when the game starts - no button and no account screen of the game's own; the game only asks how it went.
    /// Off (Available false) in the Editor and in a build without a Play Games project id (res/values/play_games.xml):
    /// then nothing here touches Play Games and the progress stays on the phone.
    static class PlayGames
    {
        const string Bridge = "com.dan.legacythroughtime.playgames.PlayGamesBridge";

        static bool? available;

        public static bool Available => available ??= Check();
        public static bool SignedIn => PlayerId != null;
        public static string PlayerId { get; private set; }
        /// The reader's Play Games name ("" when Play Games gave none).
        public static string PlayerName { get; private set; }

        static bool Check()
        {
            if (Application.platform != RuntimePlatform.Android) return false;
            try
            {
                using var bridge = new AndroidJavaClass(Bridge);
                using var activity = Activity();
                return bridge.CallStatic<bool>("configured", activity);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[play games] " + e.Message);
                return false;
            }
        }

        /// Not `interactive`: the sign-in Play Games made at launch. `interactive`: its sign-in screen (a tap of the reader).
        public static void SignIn(bool interactive, Action<bool> done) => Call("signIn", (ok, data) =>
        {
            if (ok)
            {
                var parts = data.Split('\n');
                PlayerId = parts[0];
                PlayerName = parts.Length > 1 ? parts[1] : "";
            }
            else Debug.Log("[play games] not signed in: " + data);
            done(ok);
        }, interactive);

        /// The text of a saved game: "" when it was never written, null when it could not be read.
        public static void Load(string name, Action<string> done) => Call("load", (ok, data) =>
        {
            if (!ok) Debug.LogWarning($"[play games] load {name}: {data}");
            done(ok ? data ?? "" : null);
        }, name);

        public static void Save(string name, string data, string description, Action<bool> done) => Call("save", (ok, error) =>
        {
            if (!ok) Debug.LogWarning($"[play games] save {name}: {error}");
            done(ok);
        }, name, data, description);

        /// The bridge's static `method(activity, args..., callback)`.
        static void Call(string method, Action<bool, string> done, params object[] args)
        {
            if (!Available) { done(false, "unavailable"); return; }
            try
            {
                using var bridge = new AndroidJavaClass(Bridge);
                using var activity = Activity();
                var all = new object[args.Length + 2];
                all[0] = activity;
                args.CopyTo(all, 1);
                all[all.Length - 1] = new Callback(done);
                bridge.CallStatic(method, all);
            }
            catch (Exception e) { done(false, e.Message); }
        }

        static AndroidJavaObject Activity()
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return player.GetStatic<AndroidJavaObject>("currentActivity");
        }

        /// The bridge answers on Android's main thread; the answer is handed to Unity's, where the game lives.
        sealed class Callback : AndroidJavaProxy
        {
            readonly Action<bool, string> answer;
            readonly SynchronizationContext unity = SynchronizationContext.Current;

            public Callback(Action<bool, string> answer) : base(Bridge + "$Callback") { this.answer = answer; }

            [Preserve]
            public void done(bool ok, string data)      // the name of the Java interface method
            {
                if (unity != null) unity.Post(_ => answer(ok, data), null);
                else answer(ok, data);
            }
        }
    }
}
