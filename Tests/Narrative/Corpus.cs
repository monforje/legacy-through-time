using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LegacyThroughTime.Narrative.Tests
{
    /// <summary>Compiles .ink sources with inklecate (cached by content hash) for the tests.</summary>
    static class Corpus
    {
        static readonly Dictionary<string, string> Cache = new Dictionary<string, string>();

        public static string RepoRoot
        {
            get
            {
                var dir = new DirectoryInfo(AppContext.BaseDirectory);
                while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Assets"))) dir = dir.Parent;
                return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
            }
        }

        /// <summary>Test stories plus every root story of the game itself.</summary>
        public static IEnumerable<string> StoryFiles()
        {
            var corpus = Directory.GetFiles(Path.Combine(RepoRoot, "Tests/Narrative/Corpus"), "*.ink");
            var game = Directory.GetFiles(Path.Combine(RepoRoot, "content/stories"), "*.ink", SearchOption.AllDirectories)
                .Where(f => !File.ReadAllText(f).Contains("INCLUDE"));
            return corpus.Concat(game).OrderBy(f => f, StringComparer.Ordinal);
        }

        public static string Name(string file) => Path.GetFileNameWithoutExtension(file);

        public static string Json(string inkFile)
        {
            lock (Cache)
            {
                if (Cache.TryGetValue(inkFile, out var cached)) return cached;
                var source = File.ReadAllText(inkFile);
                var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)))[..16];
                var dir = Path.Combine(Path.GetTempPath(), "ink-corpus");
                Directory.CreateDirectory(dir);
                var output = Path.Combine(dir, Name(inkFile) + "-" + hash + ".json");
                if (!File.Exists(output)) Compile(inkFile, output);
                var json = File.ReadAllText(output);
                Cache[inkFile] = json;
                return json;
            }
        }

        public static string CompileSource(string inkSource)
        {
            var dir = Path.Combine(Path.GetTempPath(), "ink-snippets");
            Directory.CreateDirectory(dir);
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(inkSource)))[..16];
            var ink = Path.Combine(dir, hash + ".ink");
            var output = Path.Combine(dir, hash + ".json");
            if (!File.Exists(output))
            {
                File.WriteAllText(ink, inkSource);
                Compile(ink, output);
            }
            return File.ReadAllText(output);
        }

        static void Compile(string inkFile, string output)
        {
            var inklecate = Path.Combine(Environment.GetEnvironmentVariable("INK_DIR") ?? "/opt/inklecate", "inklecate");
            var psi = new ProcessStartInfo(inklecate, $"-o \"{output}\" \"{inkFile}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using var p = Process.Start(psi)!;
            var log = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0 || !File.Exists(output))
                throw new InvalidOperationException("inklecate failed for " + inkFile + ":\n" + log);
        }
    }
}
