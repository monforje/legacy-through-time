using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// Player settings (PlayerPrefs). The sound switch mutes the listener, so every source keeps its own logic.
    static class Settings
    {
        public static bool Sound
        {
            get => PlayerPrefs.GetInt("sound", 1) == 1;
            set { PlayerPrefs.SetInt("sound", value ? 1 : 0); PlayerPrefs.Save(); Apply(); }
        }

        public static void Apply() => AudioListener.volume = Sound ? 1 : 0;
    }
}
