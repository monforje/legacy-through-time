using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace LegacyThroughTime.Editor
{
    public static class AndroidBuild
    {
        const string OutputDir = "Builds/Android";
        // `task ink` compiles stories into buckets/ (stand-in for remote storage); builds ship a copy.
        const string StoriesSource = "buckets/stories";
        const string StoriesTarget = "Assets/StreamingAssets/stories";

        [MenuItem("Build/Android APK (dev)")]
        public static void Dev() => Build(development: true, runOnDevice: false);

        [MenuItem("Build/Android APK (dev) + Run on Device")]
        public static void DevAndRun() => Build(development: true, runOnDevice: true);

        [MenuItem("Build/Android APK (release)")]
        public static void Release() => Build(development: false, runOnDevice: false);

        static void Build(bool development, bool runOnDevice)
        {
            BundleStories();
            BrandSettings.Apply();
            var android = NamedBuildTarget.Android;
            var projectConfig = PlayerSettings.GetIl2CppCompilerConfiguration(android);

            // Unoptimized C++ compiles several times faster; release keeps the project setting.
            if (development)
                PlayerSettings.SetIl2CppCompilerConfiguration(android, Il2CppCompilerConfiguration.Debug);
            EditorUserBuildSettings.buildAppBundle = false;

            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                target = BuildTarget.Android,
                locationPathName = Path.Combine(OutputDir, development ? "game-dev.apk" : "game.apk"),
                options = (development ? BuildOptions.Development : BuildOptions.None)
                          | (runOnDevice ? BuildOptions.AutoRunPlayer : BuildOptions.None),
            };

            try
            {
                var summary = BuildPipeline.BuildPlayer(options).summary;
                if (summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                    throw new BuildFailedException($"Android build {summary.result}: {summary.totalErrors} error(s)");

                var sizeMb = new FileInfo(summary.outputPath).Length / (1024 * 1024);
                Debug.Log($"APK: {summary.outputPath} ({sizeMb} MB in {summary.totalTime:mm\\:ss})");
            }
            finally
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(android, projectConfig);
            }
        }

        static void BundleStories()
        {
            if (!Directory.Exists(StoriesSource))
                throw new BuildFailedException($"No compiled stories in {StoriesSource}: run `task ink` first");
            FileUtil.DeleteFileOrDirectory(StoriesTarget);
            Directory.CreateDirectory(Path.GetDirectoryName(StoriesTarget));
            FileUtil.CopyFileOrDirectory(StoriesSource, StoriesTarget);
            AssetDatabase.Refresh();
        }
    }
}
