using System.Collections.Generic;
using System.IO;
using LegacyThroughTime.Narrative;
using LegacyThroughTime.Prototype;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Editor
{
    /// Builds every demo screen and every screen of the story without Play Mode and reports exceptions, missing
    /// sprites/fonts, plates that do not fit and the geometry of the key blocks. Run: `task prototype:smoke`.
    public static class PrototypeSmoke
    {
        const string EpisodePath = "Assets/StreamingAssets/stories/golden-cage/episode-01.json";
        static readonly string[] DumpPages = { "7", "8", "9a", "11", "12", "10", "0" };
        static readonly string[] DumpBlocks =
        {
            "Plate", "Label", "Option", "Panel", "Balance", "Collapse", "Stat", "Info", "Timer", "Title", "Brackets", "Knot", "Field", "GemChip",
        };

        public static void Run()
        {
            var failures = new List<string>();
            var canvas = NewCanvas();
            Viewport.Set(Metrics.RefHeight);

            foreach (var page in DemoFlow.Pages())
                Guard(failures, "page " + page.Id, () =>
                {
                    var view = new PageView(canvas, page, new Bank(), () => { }, animated: false);
                    Canvas.ForceUpdateCanvases();
                    var plate = view.Root.Find("Plate");
                    Debug.Log($"[smoke] page {page.Id}: {view.Root.GetComponentsInChildren<Graphic>(true).Length} graphics, " +
                              (plate ? $"plate {plate.GetComponent<RectTransform>().rect.size}" : "no plate"));
                    if (System.Array.IndexOf(DumpPages, page.Id) >= 0) Dump(view);
                    view.Dispose();
                });

            WalkStory(canvas, failures);

            if (Art.Missing > 0) failures.Add($"{Art.Missing} missing sprite(s)/font(s)");
            foreach (var f in failures) Debug.LogError("[smoke] FAIL " + f);
            Debug.Log(failures.Count == 0 ? "[smoke] OK" : $"[smoke] {failures.Count} failure(s)");
            EditorApplication.Exit(failures.Count == 0 ? 0 : 1);
        }

        static RectTransform NewCanvas()
        {
            var go = new GameObject("SmokeCanvas", typeof(Canvas), typeof(CanvasScaler));
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            go.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            var root = (RectTransform)go.transform;
            root.sizeDelta = new Vector2(Metrics.RefWidth, Metrics.RefHeight);
            return root;
        }

        static void Guard(List<string> failures, string what, System.Action action)
        {
            try { action(); }
            catch (System.Exception e) { failures.Add($"{what}: {e}"); }
        }

        /// Walks the whole story with several strategies (first option, last option, random) and builds a screen
        /// for every beat: no exceptions, no plate that is too tall, every voice of the story mapped.
        static void WalkStory(RectTransform canvas, List<string> failures)
        {
            var json = File.ReadAllText(EpisodePath);
            var voices = new Dictionary<Voice, int>();
            var tallest = 0f; var tallestText = "";
            var screens = 0; var banners = 0;

            for (var strategy = 0; strategy < 6; strategy++)
            {
                var rng = new System.Random(strategy);
                var machine = StoryMachine.FromJson(json, seed: strategy);
                machine.Advance();
                PlateSpec last = null;
                for (var step = 0; step < 2000 && !machine.IsFinished; step++)
                {
                    var beat = machine.Current;
                    try
                    {
                        if (beat.Phase == StoryPhase.Line)
                        {
                            voices[beat.Voice] = voices.TryGetValue(beat.Voice, out var n) ? n + 1 : 1;
                            var plate = BeatMapper.Plate(beat);
                            if (plate == null) banners++;
                            else
                            {
                                last = plate;
                                var view = new PageView(canvas, BeatMapper.LinePage(plate), new Bank(), () => { }, animated: false);
                                screens++;
                                var height = view.Root.Find("Plate").GetComponent<RectTransform>().rect.height;
                                if (height > tallest) { tallest = height; tallestText = Shorten(beat.Text); }
                                view.Dispose();
                            }
                            machine.Advance();
                        }
                        else if (beat.Phase == StoryPhase.Choice)
                        {
                            var view = new PageView(canvas, BeatMapper.ChoicePage(last, beat, machine.GetInt("gems")), new Bank(), () => { }, animated: false);
                            screens++;
                            view.Dispose();
                            machine.Choose(strategy == 0 ? 0 : strategy == 1 ? beat.Options.Count - 1 : rng.Next(beat.Options.Count));
                        }
                        else machine.Advance();
                    }
                    catch (System.Exception e) { failures.Add($"story strategy {strategy}, step {step}: {e}"); break; }
                }
                if (machine.Phase != StoryPhase.End) failures.Add($"story strategy {strategy} did not reach the end ({machine.Phase})");
            }

            Debug.Log($"[smoke] story: {screens} screens, {banners} stat banners; voices {string.Join(", ", voices)}; " +
                      $"tallest plate {tallest:0} for \"{tallestText}\"");
            if (tallest > 330) failures.Add($"a plate is {tallest:0} units tall: \"{tallestText}\"");
        }

        static string Shorten(string text) => text.Length > 60 ? text.Substring(0, 60) + "…" : text;

        /// Rects in canvas units from the bottom-left corner of the 360x780 screen, to compare with the web version.
        static void Dump(PageView view)
        {
            var corners = new Vector3[4];
            foreach (var rt in view.Root.GetComponentsInChildren<RectTransform>(true))
            {
                if (System.Array.IndexOf(DumpBlocks, rt.name) < 0) continue;
                rt.GetWorldCorners(corners);
                Debug.Log($"[smoke]   {view.Root.name} / {rt.name}: x {corners[0].x:0.#}..{corners[2].x:0.#}  y {corners[0].y:0.#}..{corners[2].y:0.#}  " +
                          $"({corners[2].x - corners[0].x:0.#} x {corners[2].y - corners[0].y:0.#})");
            }
        }
    }
}
