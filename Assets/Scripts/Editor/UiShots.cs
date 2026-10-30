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
        static int Width = 1080, Height = 2340;                  // LTT_SHOT=1080x1920 for a 16:9 phone (Huawei P10), 1600x2560 for a tablet
        const int Settle = 12;                                    // editor updates between building a screen and the shot
        const string EpisodePath = "Assets/StreamingAssets/stories/golden-cage/episode-01.json";
        static string Out = "Builds/ui-shots";

        sealed class Shot { public string Name; public Action<RectTransform> Build; }

        static readonly List<Shot> shots = new();
        static readonly List<Action> cleanup = new();
        static Camera camera;
        static RenderTexture target;
        static RectTransform canvas;
        static int index, wait, kit;

        public static void Run()
        {
            var size = Environment.GetEnvironmentVariable("LTT_SHOT");
            if (!string.IsNullOrEmpty(size))
            {
                var parts = size.Split('x');
                Width = int.Parse(parts[0]); Height = int.Parse(parts[1]);
                Out = "Builds/ui-shots-" + size;
            }
            float.TryParse(Environment.GetEnvironmentVariable("LTT_DPI"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var dpi);
            var scale = Viewport.ScaleFor(Width, Height, dpi, dpi <= 0);              // LTT_DPI=224 for a tablet: a unit is at most 1.35 dp
            Viewport.Set(Height / scale, 0, 0, Width / scale);
            Plan();
            var only = Environment.GetEnvironmentVariable("LTT_ONLY");     // `sa-,menu-stories`: only the shots whose names start so, with the Blender kit only
            if (!string.IsNullOrEmpty(only))
            {
                var prefixes = only.Split(',');
                shots.RemoveAll(shot => Array.TrueForAll(prefixes, pre => !shot.Name.StartsWith(pre)));
            }
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
            scaler.scaleFactor = scale;
            canvas = (RectTransform)go.transform;

            index = 0; kit = string.IsNullOrEmpty(only) ? 0 : 1; wait = -1;
            Art.BlenderKit = !string.IsNullOrEmpty(only);
            EditorApplication.update += Tick;
        }

        static void Tick()
        {
            try
            {
                if (wait < 0)
                {
                    Clear();
                    // the shell lives in its own column (as wide as the canvas up to 440), the story screens in a 360 one
                    var name = shots[index].Name;
                    var shell = name.StartsWith("menu-") || name.StartsWith("chat");
                    var column = Ui.Rect(canvas, shell ? "Shell" : "Pages");
                    var wide = shell ? Viewport.ColumnWidth : Metrics.RefWidth;
                    column.anchorMin = A.BottomCenter; column.anchorMax = A.TopCenter; column.pivot = A.Center;
                    column.offsetMin = new Vector2(-wide / 2, 0); column.offsetMax = new Vector2(wide / 2, 0);
                    shots[index].Build(column);
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
            Add("menu-wheel", c => Menu(c, terms).Select(MenuScreen.Tab.Wheel));
            Add("menu-rewards", c => Menu(c, terms).ShowRewards());
            Add("menu-wheel-to-shop", c => { var m = Menu(c, terms); m.Select(MenuScreen.Tab.Wheel); m.Select(MenuScreen.Tab.Shop); });     // the wheel is gone, the shop is in front
            Add("menu-sheet", c => Menu(c, terms).Select(MenuScreen.Tab.Rewards));
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

            Add("chats", c => Menu(c, terms).Select(MenuScreen.Tab.Chat));
            Add("chats-three", c =>                                       // several rows: the lines between them, the unread badges, the times
            {
                Backdrop(c, "menu_campfire");
                var now = DateTime.Now;
                long Ms(TimeSpan ago) => new DateTimeOffset(now - ago).ToUnixTimeMilliseconds();
                var rows = new List<ChatRow>
                {
                    new ChatRow { Story = StoryCatalog.All[0], Hero = "Сююмбике", Preview = "Привет. Я рада, что ты пришёл. О чём поговорим сегодня вечером, пока нет ветра?", At = Ms(TimeSpan.FromMinutes(20)), Unread = 2 },
                    new ChatRow { Story = StoryCatalog.All[1], Hero = "Зилант", Preview = "Холм помнит всё.", At = Ms(TimeSpan.FromDays(1)), Unread = 0 },
                    new ChatRow { Story = StoryCatalog.All[2], Hero = "Шурале", Preview = null, At = Ms(TimeSpan.FromDays(3)), Unread = 12 },
                };
                new ChatsPanel(c, rows, _ => { }, () => { }, now, NoAnimator.Instance);
                var nav = new ShellNav(c, new[] { ("book", "Истории"), ("chat", "Чат"), ("wheel", "Колесо"), ("star", "Слова"), ("gift", "Награды") }, 1, _ => { });
            });
            Add("chat", c => Chat(c, said: 2, energy: 7));
            Add("chat-long", c => Chat(c, said: 3, energy: 6, longReply: true));
            Add("chat-empty-energy", c => Chat(c, said: 2, energy: 0));

            foreach (var page in DemoFlow.Pages())
                Add("demo-" + page.Id, c => Page(c, "khan_chamber", page));

            CastShot("cast-yusuf", "yusuf_hall", "yusuf_angry", "yusuf", "Юсуф", "Ты поедешь в Казань. Это не просьба.");
            CastShot("cast-heroine", "yusuf_hall", "suyumbike_court_determined", "suyumbike", "Сююмбике", "Тогда я поеду так, как решу сама.");
            CastShot("cast-safiya", "tent_entrance", "safiya_smile", "safiya", "Сафия", "Ханбике, я принесла вам одеяние к дороге.");
            CastShot("cast-khan", "khan_chamber", "jan_ali_sad", "jan_ali", "Джан-Али", "Не тогда, когда речь идёт о ханском дворе.");
            CastShot("cast-child", "flashback_katyk", "suyumbike_child_smile", "suyumbike_child", "Маленькая Сююмбике", "А люди умеют?");
            StoryScreens();
            SikhoteShots();
            Add("story-settings", c =>
            {
                Page(c, "khan_chamber", DemoFlow.Pages()[0]);
                new StorySettings(c, NoAnimator.Instance).Open(() => { });
            });
        }

        /// The screens of «По хребтам Сихотэ-Алиня» in the kit of the story (Blender kit only): the title, the plates, a choice, the
        /// solid buttons, the settings modal, the stat banner.
        static void SikhoteShots()
        {
            void Sa(string name, Action<RectTransform> build) => Add(name, c =>
            {
                Cast.Use("sikhote-alin"); Art.StoryKit = "sa-";
                cleanup.Add(() => { Art.StoryKit = null; Cast.Use("golden-cage"); });
                build(c);
            });
            void Line(RectTransform c, string bg, string sprite, string id, string label, PlateStyle style, string text, string info = null)
            {
                Backdrop(c, bg);
                if (sprite != null)
                {
                    var cast = new CastView(c);
                    cast.Show(sprite, null, Cast.IsHero(id), Cast.Height(id));
                    cast.Tick(10);
                }
                var plate = new PlateSpec { Style = style, Label = label, Text = text, Instant = true };
                var page = BeatMapper.LinePage(plate);
                page.Info = info;
                var view = new PageView(c, page, new Bank(), () => { }, animated: false);
                cleanup.Add(view.Dispose);
                new StoryHud(c, () => { }, _ => { }, () => { }, NoAnimator.Instance);
            }
            const string nursery = "sa_nursery_day";
            Sa("sa-title", c =>
            {
                Backdrop(c, "sa_title");
                var title = new Page { Id = "title", Title = new TitleSpec { Name = "По хребтам Сихотэ-Алиня", Scene = "Глава 1: Мальчик, который хотел увидеть весь мир · продолжение" } };
                var view = new PageView(c, title, new Bank(), () => { }, animated: false);
                cleanup.Add(view.Dispose);
                new StoryHud(c, () => { }, _ => { }, () => { }, NoAnimator.Instance);
            });
            Sa("sa-line-hero", c => Line(c, nursery, "volodya_smile", "volodya", "Володя", PlateStyle.SpeechLeft, "Так… Это у нас море. Нет, погодите. Это ещё не море, это большое озеро."));
            Sa("sa-line-mother", c => Line(c, nursery, "mother_angry", "mother", "Мать", PlateStyle.SpeechRight, "Володенька! Ты опять испачкал пол?"));
            Sa("sa-narr", c => Line(c, nursery, null, null, "…", PlateStyle.Narr, "Маленький Володя лежит животом на ковре и болтает ногами. Перед ним расстелен большой лист бумаги."));
            Sa("sa-fact", c => Line(c, "sa_vladivostok_pier", null, null, "Место", PlateStyle.Fact, "Владивосток. 1900 год. Морской порт."));
            Sa("sa-hint", c => Line(c, nursery, null, null, "Подсказка", PlateStyle.Hint, "Скрытая метка: «Корабль на карте»."));
            Sa("sa-thought", c => Line(c, "sa_vladivostok_pier", "arsenyev_thoughtful", "arsenyev", "Арсеньев", PlateStyle.Thought, "В детстве мне казалось, что достаточно нарисовать дорогу, и можно отправляться в путь."));
            Sa("sa-thought-puff", c => Line(c, "sa_vladivostok_pier", "arsenyev_smile", "arsenyev", "Арсеньев", PlateStyle.ThoughtPuff, "Вот и Дальний Восток."));
            Sa("sa-note", c => Line(c, nursery, "volodya_laugh", "volodya", "Володя", PlateStyle.SpeechLeft, "А здесь горы. Очень высокие. Выше <i>Исаакия</i>!", "Исаакий — Исаакиевский собор, самое высокое здание Петербурга того времени."));
            Sa("sa-choice", c =>
            {
                Backdrop(c, "sa_ink_stain");
                var plate = new PlateSpec { Style = PlateStyle.Narr, Label = "…", Text = "Синее пятно на паркете всё ещё блестит.", Instant = true };
                var spec = new ChoiceSpec
                {
                    Items = new[] { new Option("Взять тряпку и убрать", 0, 0), new Option("Объявить пятно озером", 0, 1), new Option("Дорисовать у озера корабль", 5, 2) },
                    Free = true, Coins = 20, Mirror = false,
                };
                var view = new PageView(c, new Page { Plate = plate, Choice = spec }, new Bank(), () => { }, animated: false);
                cleanup.Add(view.Dispose);
                new StoryHud(c, () => { }, _ => { }, () => { }, NoAnimator.Instance);
            });
            Sa("sa-banner", c =>
            {
                Line(c, nursery, "volodya", "volodya", "Володя", PlateStyle.SpeechLeft, "Только нельзя же нарисовать дом и никого туда не поселить. Это было бы глупо.");
                new StatBanner(c, "ВООБРАЖЕНИЕ +1", NoAnimator.Instance);
            });
            Sa("sa-hud-collapsed", c =>
            {
                Backdrop(c, nursery);
                var cast = new CastView(c);
                cast.Show("volodya", null, true, Cast.Height("volodya")); cast.Tick(10);
                var hud = new StoryHud(c, () => { }, _ => { }, () => { }, NoAnimator.Instance);
            });
            Sa("sa-settings", c =>
            {
                Line(c, nursery, "volodya_smile", "volodya", "Володя", PlateStyle.SpeechLeft, "Так… Это у нас море.");
                new StorySettings(c, NoAnimator.Instance).Open(() => { });
            });
        }

        /// A line with its speaker on the screen: one person, full length, as in the game.
        static void CastShot(string name, string background, string sprite, string id, string label, string text) =>
            Add(name, c =>
            {
                Backdrop(c, background);
                var cast = new CastView(c);
                cast.Show(sprite, null, id.StartsWith("suyumbike"), Cast.Height(id));
                cast.Tick(10);
                var plate = new PlateSpec { Style = id.StartsWith("suyumbike") ? PlateStyle.SpeechLeft : PlateStyle.SpeechRight, Label = label, Text = text, Instant = true };
                var view = new PageView(c, BeatMapper.LinePage(plate), new Bank(), () => { }, animated: false);
                cleanup.Add(view.Dispose);
            });

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
                                      (_, _, _) => { }, _ => { }, terms, animated: false);
            cleanup.Add(menu.Dispose);
            SettingsToast.Gear(c, () => { });
            return menu;
        }

        /// A chat with the hero with a few lines said (no server: the lines are given).
        static void Chat(RectTransform c, int said, int energy, bool longReply = false)
        {
            var story = StoryCatalog.All[0];
            var talk = new[]
            {
                (ChatLine.Hero, story.Opening),
                (ChatLine.Reader, "Ты скучаешь по степи?"),
                (ChatLine.Hero, "Каждую ночь. Здесь стены, а там ветер. Ветер не спрашивает, чья ты дочь."),
                (ChatLine.Reader, "Каким тебе показался хан?"),
                (ChatLine.Hero, longReply
                    ? "Молодым. И чужим. Он смотрел на меня, как на подарок, который прислали не ему, и я никак не могла понять, радоваться мне этому или бояться, пока вокруг было так тихо."
                    : "Молодым. И чужим."),
            };
            var chat = new Chat { story = story.Id };
            for (var i = 0; i < 1 + 2 * said && i < talk.Length; i++) chat.lines.Add(new ChatLine { role = talk[i].Item1, text = talk[i].Item2 });
            SettingsToast.Gear(c, () => { });                              // as in the app: the gear exists, the chat hides it
            var view = new ChatScreen(c, null, story, chat, new Energy { value = energy }, null, () => { }, () => { }, (_, _) => { }, animated: false);
        }

        static void Page(RectTransform c, string background, Page page)
        {
            Backdrop(c, background);
            var view = new PageView(c, page, new Bank(), () => { }, animated: false);
            new StoryHud(c, () => { }, _ => { }, () => { }, NoAnimator.Instance);      // the tools over every story
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
