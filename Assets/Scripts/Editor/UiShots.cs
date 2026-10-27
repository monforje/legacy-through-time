using System;
using System.Collections.Generic;
using System.IO;
using LegacyThroughTime.Narrative;
using LegacyThroughTime.Prototype;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Editor
{
    /// Screenshots of the real screens, to look for overlaps and artefacts: the menu (three states, the update card),
    /// the settings toast, every demo page and the first screen of every kind of plate and choice in the story,
    /// with both UI kits. `task ui:shots` -> Builds/ui-shots/<kit>/<name>.png, 1080 x 2340 (a phone, 3 px a unit).
    /// Batch mode with graphics (no -nographics): the canvas is drawn by a camera into a texture. The editor loop
    /// runs between the shots, so that the pictures loaded in the background arrive.
    public static class UiShots
    {
        const int Width = 1080, Height = 2340, Settle = 12;      // editor updates between building a screen and the shot
        const string EpisodePath = "Assets/StreamingAssets/stories/golden-cage/episode-01.json";
        const string Out = "Builds/ui-shots";

        sealed class Shot { public string Name; public Action<RectTransform> Build; }

        static readonly List<Shot> shots = new();
        static readonly List<Action> cleanup = new();
        static Camera camera;
        static RenderTexture target;
        static RectTransform canvas;
        static int index, wait, kit;

        public static void Run()
        {
            Viewport.Set(Metrics.RefHeight);
            Plan();
            camera = new GameObject("ShotCamera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Theme.Backdrop;
            camera.orthographic = true;
            target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            camera.targetTexture = target;

            var go = new GameObject("ShotCanvas", typeof(Canvas), typeof(CanvasScaler));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceCamera;
            c.worldCamera = camera;
            c.planeDistance = 10;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = Width / Metrics.RefWidth;
            canvas = (RectTransform)go.transform;

            index = 0; kit = 0; wait = -1;
            Art.BlenderKit = false;
            EditorApplication.update += Tick;
        }

        static void Tick()
        {
            try
            {
                if (wait < 0)
                {
                    Clear();
                    shots[index].Build(canvas);
                    wait = Settle;
                    return;
                }
                if (--wait > 0) return;
                Save(Path.Combine(Out, Art.BlenderKit ? "blender" : "svg", shots[index].Name + ".png"));
                wait = -1;
                if (++index < shots.Count) return;
                index = 0;
                if (++kit < 2) { Art.BlenderKit = true; return; }
                Clear();
                EditorApplication.update -= Tick;
                Debug.Log($"[shots] {shots.Count} screens x 2 kits -> {Out}");
                EditorApplication.Exit(Art.Missing > 0 ? 1 : 0);
            }
            catch (Exception e)
            {
                Debug.LogError($"[shots] {shots[index].Name}: {e}");
                EditorApplication.update -= Tick;
                EditorApplication.Exit(1);
            }
        }

        static void Clear()
        {
            foreach (var a in cleanup) a();
            cleanup.Clear();
            for (var i = canvas.childCount - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(canvas.GetChild(i).gameObject);
        }

        static void Save(string path)
        {
            Canvas.ForceUpdateCanvases();
            camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            tex.Apply();
            RenderTexture.active = previous;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        // ------------------------------------------------------------ what is shot
        static void Plan()
        {
            shots.Clear();
            var terms = Glossary.FromStory(File.ReadAllText(EpisodePath));
            Add("menu-stories", c => Menu(c, terms));
            Add("menu-locked", c => Menu(c, terms).Turn(1));
            Add("menu-update", c => Menu(c, terms).ShowUpdate("2099-01-01-0000", _ => { }));
            Add("menu-chapters", c => Menu(c, terms).ShowChapters(StoryCatalog.All[0]));
            Add("menu-library", c => Menu(c, terms).Select(MenuScreen.Tab.Library));
            Add("menu-term", c => Menu(c, terms).ShowTerm(terms[1]));
            Add("menu-shop", c => Menu(c, terms).Select(MenuScreen.Tab.Shop));
            Add("menu-settings", c =>
            {
                Menu(c, terms);
                new SettingsToast(c, NoAnimator.Instance).Open(null, () => { }, new SettingsToast.Cloud { Line = "Прогресс сохраняется · Даниил" });
            });
            Add("menu-settings-signin", c =>
            {
                Menu(c, terms);
                new SettingsToast(c, NoAnimator.Instance).Open(null, () => { }, new SettingsToast.Cloud { SignIn = () => { } });
            });
            Add("menu-restored", c =>
            {
                Menu(c, terms);
                new StatBanner(c, "Прогресс восстановлен из Google Play", NoAnimator.Instance, "check");
            });

            foreach (var page in DemoFlow.Pages())
                Add("demo-" + page.Id, c => Page(c, "khan_chamber", page));

            StoryScreens();
            Add("story-settings", c =>
            {
                Page(c, "khan_chamber", DemoFlow.Pages()[0]);
                new SettingsToast(c, NoAnimator.Instance).Open(() => { }, () => { });
            });
        }

        static void Add(string name, Action<RectTransform> build) => shots.Add(new Shot { Name = name, Build = build });

        /// The menu by the fire with the first chapter being read, two words of the library open (only for the shot).
        static MenuScreen Menu(RectTransform c, List<Glossary.Term> terms)
        {
            Backdrop(c, "menu_campfire");
            // the words open in the editor's own play are put back after the shot
            var before = PlayerPrefs.HasKey("library:open") ? PlayerPrefs.GetString("library:open") : null;
            PlayerPrefs.SetString("library:open", terms[0].Name + "\n" + terms[1].Name);
            cleanup.Add(() => { if (before != null) PlayerPrefs.SetString("library:open", before); else PlayerPrefs.DeleteKey("library:open"); });
            var menu = new MenuScreen(c, StoryCatalog.All,
                                      ch => ch.Soon ? MenuScreen.Progress.Locked : ch.Number == 1 ? MenuScreen.Progress.InProgress : MenuScreen.Progress.New,
                                      (_, _, _) => { }, terms, animated: false);
            cleanup.Add(menu.Dispose);
            SettingsToast.Gear(c, () => { });
            return menu;
        }

        static void Page(RectTransform c, string background, Page page)
        {
            Backdrop(c, background);
            var view = new PageView(c, page, new Bank(), () => { }, animated: false);
            SettingsToast.Gear(c, () => { });
            cleanup.Add(view.Dispose);
        }

        /// The painted background of the place, as the game covers the screen with it.
        static void Backdrop(RectTransform c, string id)
        {
            var bg = Ui.Img(c, "Backdrop", Resources.Load<Sprite>("Art/Backgrounds/" + id));
            var size = bg.sprite ? bg.sprite.rect.size : Vector2.one;
            var k = Mathf.Max(Metrics.RefWidth / size.x, Metrics.RefHeight / size.y);
            Ui.Box(bg.rectTransform, A.Center, A.Center, Vector2.zero, size * k);
            Ui.Stretch(Ui.Img(c, "Vignette", ProceduralArt.Vignette()).rectTransform);
        }

        /// The first screen of every plate style, the first choices, the tallest plate - with the place it is in.
        static void StoryScreens()
        {
            var machine = StoryMachine.FromJson(File.ReadAllText(EpisodePath), seed: 0);
            machine.Advance();
            var styles = new HashSet<PlateStyle>();
            var choices = 0;
            var bg = "khan_chamber";
            PlateSpec last = null;
            PlateSpec tallest = null; var tallestBg = bg;
            var italic = false;
            for (var step = 0; step < 3000 && !machine.IsFinished; step++)
            {
                var beat = machine.Current;
                if (beat.HasTag("bg") && beat.TagValue("bg") is { } id && id != "black") bg = id;
                if (beat.Phase == StoryPhase.Line)
                {
                    var plate = BeatMapper.Plate(beat);
                    if (plate != null)
                    {
                        last = plate;
                        var page = BeatMapper.LinePage(plate);
                        var where = bg;
                        if (styles.Add(plate.Style)) Add($"story-{plate.Style.ToString().ToLowerInvariant()}", c => Page(c, where, page));
                        if (tallest == null || plate.Text.Length > tallest.Text.Length) { tallest = plate; tallestBg = bg; }
                        // the first word a note explains: in italics on its plate, the note in the cloud at the top
                        if (!italic && BeatMapper.Note(beat) is { } note && plate.Text.Contains("<i>"))
                        {
                            italic = true;
                            var noted = BeatMapper.LinePage(plate);
                            noted.Info = note;
                            Add("story-note", c => Page(c, where, noted));
                        }
                    }
                    machine.Advance();
                }
                else if (beat.Phase == StoryPhase.Choice)
                {
                    if (choices < 3)
                    {
                        var page = BeatMapper.ChoicePage(last, beat, machine.GetInt("gems"));
                        var where = bg;
                        Add($"story-choice-{++choices}", c => Page(c, where, page));
                    }
                    machine.Choose(0);
                }
                else machine.Advance();
            }
            // the longest line, said in parts (StoryPlayer shows one screen a part, TextRules.Split)
            if (tallest != null)
            {
                var parts = TextRules.Split(tallest.Text);
                for (var k = 0; k < parts.Length; k++)
                {
                    var part = BeatMapper.LinePage(new PlateSpec { Style = tallest.Style, Label = tallest.Label, Text = parts[k] });
                    Add($"story-tallest-{k + 1}", c => Page(c, tallestBg, part));
                }
            }
        }
    }
}
