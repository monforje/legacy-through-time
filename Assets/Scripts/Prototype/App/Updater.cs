using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace LegacyThroughTime.Prototype
{
    /// Updates from the repository: asks GitHub for the latest release and, when it is newer than this build
    /// (BuildStamp), the menu offers it. "Обновить" downloads the APK of that release into the cache and hands it to the
    /// system installer, which asks for one tap and installs it over the game (saves stay). Android never lets an app
    /// install silently, so that tap is the only manual step. Once the install permission is granted the whole thing
    /// starts by itself when the menu finds a newer build. Only in a release build on a device: dev builds and the
    /// Editor never ask.
    static class Updater
    {
        const string Repo = "monforje/legacy-through-time";
        const string Latest = "https://api.github.com/repos/" + Repo + "/releases/latest";
        const string Apk = "https://github.com/" + Repo + "/releases/latest/download/game.apk";
        const string Mime = "application/vnd.android.package-archive";

        [Serializable]
        sealed class Release { public string tag_name = null; }   // filled by JsonUtility

        static bool busy;

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

        /// Downloads the APK and opens the installer. `status` gets a short line for the card ("Загрузка 40%", an error);
        /// null means it is done and the card can go back to normal. Falls back to the browser if the installer fails.
        public static IEnumerator Install(Action<string> status)
        {
            if (busy) yield break;
            busy = true;
            try
            {
                if (!CanInstall())
                {
                    status("Разрешите установку из этого приложения и нажмите ещё раз");
                    OpenInstallSettings();
                    yield break;
                }

                var path = Path.Combine(Application.temporaryCachePath, "update.apk");
                if (File.Exists(path)) File.Delete(path);

                using var request = UnityWebRequest.Get(Apk);
                request.downloadHandler = new DownloadHandlerFile(path) { removeFileOnAbort = true };
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    status("Загрузка " + Mathf.RoundToInt(operation.progress * 100) + "%");
                    yield return null;
                }
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("Update download: " + request.error);
                    status("Не удалось скачать, проверьте интернет");
                    yield break;
                }

                status(null);
                if (!Launch(path)) Application.OpenURL(Apk);
            }
            finally { busy = false; }
        }

        static AndroidJavaObject Activity()
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return player.GetStatic<AndroidJavaObject>("currentActivity");
        }

        /// False until the player has allowed this app to install packages (asked once, by the button).
        public static bool CanInstall()
        {
            using var activity = Activity();
            using var packages = activity.Call<AndroidJavaObject>("getPackageManager");
            return packages.Call<bool>("canRequestPackageInstalls");
        }

        /// "Install unknown apps" is a per-app switch; the page for this app is the only place to flip it.
        static void OpenInstallSettings()
        {
            using var activity = Activity();
            using var uri = new AndroidJavaClass("android.net.Uri")
                .CallStatic<AndroidJavaObject>("parse", "package:" + activity.Call<string>("getPackageName"));
            using var intent = new AndroidJavaObject("android.content.Intent", "android.settings.MANAGE_UNKNOWN_APP_SOURCES", uri);
            activity.Call("startActivity", intent);
        }

        static bool Launch(string path)
        {
            try
            {
                using var activity = Activity();
                using var file = new AndroidJavaObject("java.io.File", path);
                using var uri = new AndroidJavaClass("androidx.core.content.FileProvider").CallStatic<AndroidJavaObject>(
                    "getUriForFile", activity, activity.Call<string>("getPackageName") + ".updates", file);
                using var intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW");
                intent.Call<AndroidJavaObject>("setDataAndType", uri, Mime);
                intent.Call<AndroidJavaObject>("addFlags", 1 /* GRANT_READ_URI_PERMISSION */ | 0x10000000 /* NEW_TASK */);
                activity.Call("startActivity", intent);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Update install: " + e.Message);
                return false;
            }
        }
    }
}
