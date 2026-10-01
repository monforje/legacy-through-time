using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace LegacyThroughTime.Editor
{
    /// The name and the face of the app: the title under the launcher icon, the icon itself (adaptive, round, legacy)
    /// and the start-up splash. Applied before every APK build (AndroidBuild) and by hand from the Build menu.
    /// The art comes from `task assets:brand` (docs/brainstorm/templates/assets/heritage/brand).
    public static class BrandSettings
    {
        public const string ProductName = "Наследие. Сквозь время";
        const string IconDir = "Assets/Art/Brand/";

        [MenuItem("Build/Apply brand (name, icon, splash)")]
        public static void Apply()
        {
            PlayerSettings.productName = ProductName;
            ApplyIcons();
            // The Unity logo splash goes away: the game shows its own loading screen with the logo at once.
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            Debug.Log($"Brand applied: \"{ProductName}\", launcher icons from {IconDir}");
        }

        static void ApplyIcons()
        {
            var background = Load("icon-bg");
            var foreground = Load("icon-fg");
            var round = Load("icon-round");
            var legacy = Load("icon-legacy");
            if (!background || !foreground || !round || !legacy)
            {
                Debug.LogWarning("Launcher icon art is missing in " + IconDir + " (run `task assets:brand`); the icon is left as it was.");
                return;
            }

            foreach (var kind in PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.Android))
            {
                var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
                var isRound = kind.ToString().IndexOf("Round", System.StringComparison.OrdinalIgnoreCase) >= 0;
                foreach (var icon in icons)
                {
                    if (icon.maxLayerCount >= 2)
                    {
                        icon.SetTexture(background, 0);        // adaptive: a background layer and a foreground layer
                        icon.SetTexture(foreground, 1);
                    }
                    else icon.SetTexture(isRound ? round : legacy, 0);
                }
                PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
            }
        }

        static Texture2D Load(string name)
        {
            var path = IconDir + name + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);       // so that the importer settings are applied
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
