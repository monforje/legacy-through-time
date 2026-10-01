using System;
using System.IO;

namespace LegacyThroughTime.Prototype.Tests
{
    static class Project
    {
        /// The repository root (the directory with Assets/), found upward from the test binary.
        public static string Root
        {
            get
            {
                var dir = new DirectoryInfo(AppContext.BaseDirectory);
                while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Assets"))) dir = dir.Parent;
                return dir?.FullName ?? throw new InvalidOperationException("Assets/ not found above " + AppContext.BaseDirectory);
            }
        }

        public static string Path_(params string[] parts) => Path.Combine(Root, Path.Combine(parts));

        /// The compiled episode, from wherever `task ink` / `task apk` put it; null when it was not compiled.
        public static string EpisodeJson()
        {
            foreach (var candidate in new[]
            {
                Path_("Assets", "StreamingAssets", "stories", "golden-cage", "episode-01.json"),
                Path_("buckets", "stories", "golden-cage", "episode-01.json"),
            })
                if (File.Exists(candidate)) return File.ReadAllText(candidate);
            return null;
        }
    }
}
