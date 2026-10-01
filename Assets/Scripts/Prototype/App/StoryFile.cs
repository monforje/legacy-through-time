using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace LegacyThroughTime.Prototype
{
    /// Reads a compiled story from StreamingAssets (`task ink` -> buckets/stories, `task apk` bundles it).
    /// On Android StreamingAssets lives inside the APK and needs a web request; elsewhere it is a plain file.
    static class StoryFile
    {
        public static IEnumerator Read(string relativePath, Action<string> done, Action<string> failed)
        {
            var path = Path.Combine(Application.streamingAssetsPath, relativePath);
#if UNITY_ANDROID && !UNITY_EDITOR
            using var request = UnityWebRequest.Get(path);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success) failed("Не удалось загрузить историю: " + request.error);
            else done(request.downloadHandler.text);
#else
            if (File.Exists(path)) done(File.ReadAllText(path));
            else failed("Нет файла истории: " + path);
            yield break;
#endif
        }
    }
}
