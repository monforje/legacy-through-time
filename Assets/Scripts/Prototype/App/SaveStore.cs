using System;
using System.IO;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// Where the reader stopped: the machine (ink state, `StoryMachine.Save`) at the last line shown, and the
    /// background that was on screen. Written at every line, so leaving the app at any moment loses nothing.
    [Serializable]
    sealed class StorySave
    {
        public string story;
        public string machine;
        public string background;
    }

    /// One file per story in persistentDataPath/saves; "finished" marks live in PlayerPrefs.
    static class SaveStore
    {
        static string Dir => Path.Combine(Application.persistentDataPath, "saves");
        static string FileOf(string id) => Path.Combine(Dir, id + ".json");

        public static bool Has(string id) => File.Exists(FileOf(id));

        public static StorySave Read(string id)
        {
            try { return Has(id) ? JsonUtility.FromJson<StorySave>(File.ReadAllText(FileOf(id))) : null; }
            catch (Exception e) { Debug.LogWarning($"Save of {id} is unreadable: {e.Message}"); return null; }
        }

        /// Atomic: the new state goes to a temporary file that then replaces the old one, so a kill mid-write
        /// leaves the previous save intact.
        public static void Write(StorySave save)
        {
            try
            {
                Directory.CreateDirectory(Dir);
                var file = FileOf(save.story);
                var tmp = file + ".tmp";
                File.WriteAllText(tmp, JsonUtility.ToJson(save));
                if (File.Exists(file)) File.Replace(tmp, file, null);
                else File.Move(tmp, file);
            }
            catch (Exception e) { Debug.LogWarning($"Could not save {save.story}: {e.Message}"); }
        }

        public static void Delete(string id)
        {
            try { if (Has(id)) File.Delete(FileOf(id)); }
            catch (Exception e) { Debug.LogWarning($"Could not delete the save of {id}: {e.Message}"); }
        }

        public static bool Finished(string id) => PlayerPrefs.GetInt("finished:" + id, 0) == 1;

        public static void MarkFinished(string id) { PlayerPrefs.SetInt("finished:" + id, 1); PlayerPrefs.Save(); }
    }
}
