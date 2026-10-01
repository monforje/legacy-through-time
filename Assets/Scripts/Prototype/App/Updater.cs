using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace LegacyThroughTime.Prototype
{
    /// Updates from the repository: asks GitHub for the latest release and, when it is newer than this build
    /// (BuildStamp), the menu offers it. "Обновить" opens the APK of that release; the browser downloads it and Android
    /// installs it over the game (saves stay). Only in a release build on a device: dev builds and the Editor never ask.
    static class Updater
    {
        const string Repo = "monforje/legacy-through-time";
        const string Latest = "https://api.github.com/repos/" + Repo + "/releases/latest";
        const string Apk = "https://github.com/" + Repo + "/releases/latest/download/game.apk";

        [Serializable]
        sealed class Release { public string tag_name = null; }   // filled by JsonUtility

        public static bool Enabled => Application.platform == RuntimePlatform.Android && !Debug.isDebugBuild;

        /// Calls `newer` with the release tag when there is a newer build; silent on any failure (offline, rate limit).
        public static IEnumerator Check(Action<string> newer)
        {
            using var request = UnityWebRequest.Get(Latest);
            request.SetRequestHeader("Accept", "application/vnd.github+json");
            request.timeout = 10;
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success) yield break;

            string tag = null;
            try { tag = JsonUtility.FromJson<Release>(request.downloadHandler.text)?.tag_name; }
            catch (Exception e) { Debug.LogWarning("Update check: " + e.Message); }
            if (BuildStamp.IsNewer(tag, Application.version)) newer(tag);
        }

        public static void Download() => Application.OpenURL(Apk);
    }
}
