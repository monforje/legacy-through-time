using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// One file per story in persistentDataPath/saves (a StorySave, written at every line, so leaving the app at any
    /// moment loses nothing); "finished" marks live in PlayerPrefs, with the time of the finish for the cloud.
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

        /// Every save on the phone (CloudSave sends them all).
        public static List<StorySave> All()
        {
            var all = new List<StorySave>();
            if (!Directory.Exists(Dir)) return all;
            foreach (var file in Directory.GetFiles(Dir, "*.json"))
            {
                var save = Read(Path.GetFileNameWithoutExtension(file));
                if (save != null && !string.IsNullOrEmpty(save.story)) all.Add(save);
            }
            return all;
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

        /// When the chapter was finished (unix ms); 0 when it was not, or before marks had a time.
        public static long FinishedAt(string id) =>
            long.TryParse(PlayerPrefs.GetString("finished-at:" + id, ""), out var at) ? at : 0;

        public static void MarkFinished(string id) => RestoreFinished(id, Progress.Now());

        /// A mark with the time it has (from the cloud; 0 stays 0, an old mark of unknown time).
        public static void RestoreFinished(string id, long at)
        {
            PlayerPrefs.SetInt("finished:" + id, 1);
            PlayerPrefs.SetString("finished-at:" + id, at.ToString());
            PlayerPrefs.Save();
        }
    }
}
