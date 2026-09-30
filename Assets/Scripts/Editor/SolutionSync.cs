using System;
using System.IO;
using System.Linq;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.CodeEditor;
using UnityEditor;

namespace LegacyThroughTime.Editor
{
    // Unity only writes .sln/.csproj for editors it knows (VS, VS Code, Rider).
    // Zed reads them too, so regenerate them ourselves whenever code changes.
    sealed class SolutionSync : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (imported.Concat(deleted).Concat(moved).Any(IsCodeAsset))
                Generate();
        }

        [InitializeOnLoadMethod]
        static void GenerateIfMissing()
        {
            if (!Directory.EnumerateFiles(".", "*.sln").Any())
                Generate();
        }

        [MenuItem("Tools/Regenerate C# Project")]
        public static void Generate()
        {
            // With VS/VS Code selected as external editor, the package already keeps them in sync.
            if (CodeEditor.CurrentEditor is VisualStudioEditor)
                return;

            // Base ProjectGeneration writes no project header since 2.0.26 (NRE); the real
            // generator is internal, so create it by name.
            var sdkGenerator = typeof(ProjectGeneration).Assembly
                .GetType("Microsoft.Unity.VisualStudio.Editor.SdkStyleProjectGeneration", throwOnError: true);
            ((ProjectGeneration)Activator.CreateInstance(sdkGenerator)).Sync();
        }

        static bool IsCodeAsset(string path) =>
            path.EndsWith(".cs") || path.EndsWith(".asmdef") || path.EndsWith(".rsp");
    }
}
