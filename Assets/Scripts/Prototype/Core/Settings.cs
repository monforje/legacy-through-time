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

        /// The music and the sounds of the story, each on its own (on top of the sound switch).
        public static bool Music
        {
            get => PlayerPrefs.GetInt("music", 1) == 1;
            set { PlayerPrefs.SetInt("music", value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool Effects
        {
            get => PlayerPrefs.GetInt("effects", 1) == 1;
            set { PlayerPrefs.SetInt("effects", value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool Notifications
        {
            get => PlayerPrefs.GetInt("notifications", 1) == 1;
            set { PlayerPrefs.SetInt("notifications", value ? 1 : 0); PlayerPrefs.Save(); }
        }

        /// The languages of the settings stepper; the text of the game is Russian only so far, the choice is kept for later.
        public static readonly string[] Languages = { "Русский", "English" };

        public static int Language
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt("language", 0), 0, Languages.Length - 1);
            set { PlayerPrefs.SetInt("language", (value % Languages.Length + Languages.Length) % Languages.Length); PlayerPrefs.Save(); }
        }

        public static void Apply() => AudioListener.volume = Sound ? 1 : 0;
    }
}
