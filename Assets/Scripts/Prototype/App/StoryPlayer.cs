using System;
using System.Collections;
using System.Collections.Generic;
using LegacyThroughTime.Narrative;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LegacyThroughTime.Prototype
{
    /// The app: loading screen -> the menu by the fire (MenuScreen: stories and their chapters, the library, the shop)
    /// -> a chapter of a story on the heritage screens: the
    /// Narrative engine produces beats, BeatMapper turns each into a screen, the ScreenHost shows it. Every line shown
    /// is saved (SaveStore), so a story continues where the reader left it; with Google Play Games the progress is kept
    /// in the reader's saved games too (CloudSave), merged in under the loading screen and sent up from the menu.
    /// The gear opens the settings toast (sound, Google Play, back to the menu); Android Back closes it, opens it in a
    /// story, leaves the app from the menu. Starts itself in any scene; the whole UI is built in code.
    public sealed class StoryPlayer : MonoBehaviour
    {
        ScreenHost host;
        BackdropTint tint;
        CastView cast;
        StoryAudio audio;
        SettingsToast settings;
        StorySettings storySettings;           // the modal settings of a story that has its own kit (null in the others)
        StoryHud hud;                          // the solid buttons over a story with its own kit
        bool exiting;                          // the curtain of the way out of a story is down
        MenuScreen menu;
        ChatScreen chatScreen;                 // the talk with the hero of a story; null elsewhere
        StoryEntry story;                      // null in the menu
        ChapterEntry chapter;                  // the chapter being read
        StoryMachine machine;
        PlateSpec lastPlate;
        string tintId;
        readonly List<string> hints = new();       // hints and notes waiting for the next screen, which shows them at the top
        string update;                         // tag of a newer release, null when there is none (or it was not checked yet)
        string news, newsIcon;                 // a line for the menu's banner ("Прогресс восстановлен"), shown with the menu
        bool signingIn;                        // the reader asked to sign in to Google Play: its result gets a banner
        int session;                           // bumps on every menu / story switch: a late callback of the old one is ignored
        int lastWidth, lastHeight;
        readonly Dictionary<string, string> texts = new();     // compiled stories already read

        internal PageView Current => host?.Current;
        internal StoryMachine Machine => machine;
        internal bool Ended { get; private set; }
        /// Test hooks: start this story from the beginning instead of the menu; write and read no saves.
        internal string AutoStart;
        internal bool Persist = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<StoryPlayer>() || FindAnyObjectByType<SessionPrototype>()) return;
            new GameObject("StoryPlayer").AddComponent<StoryPlayer>();
        }

        IEnumerator Start()
        {
            Stage.Setup();
            Settings.Apply();
            lastWidth = Screen.width; lastHeight = Screen.height;
            var canvas = Stage.BuildCanvas();
            tint = Stage.BuildBackground(canvas);
            cast = new CastView(canvas);
            audio = new StoryAudio(transform);
            host = new ScreenHost(this, canvas);
            settings = new SettingsToast(host.Overlay, host.OverlayAnimator);
            storySettings = new StorySettings(host.Overlay, host.OverlayAnimator);
            SettingsToast.Gear(host.Hud, OpenSettings);

            audio.Ui("loading_logo");
            var loading = host.ShowLoading();
            var started = Time.unscaledTime;
            yield return null;                                   // the logo is drawn first, then the warm-up runs under it
            Art.Prefetch(MenuBackground);
            Store.Start();                                       // Google Play: pending orders of an earlier session come now
            Coroutine cloud = null;
            if (Persist)                                         // Play Games signs in and the saved progress comes in meanwhile
            {
                CloudSave.Reading = () => story != null;
                CloudSave.Merged += OnCloudMerged;
                cloud = StartCoroutine(CloudSave.Start(CloudWait));
            }
            _ = Art.Head; _ = Art.Sub; _ = Art.Body; _ = Art.Action;
            foreach (var s in StoryCatalog.All)                   // the chapters are read under the logo too (a failure is retried on open)
                foreach (var c in s.Chapters)
                    if (!c.Soon) yield return StoryFile.Read(c.Path, t => texts[c.Path] = t, Debug.LogWarning);
            if (cloud != null) yield return cloud;
            var rest = LoadingScreen.MinSeconds - (Time.unscaledTime - started);     // the logo is seen for a moment at least
            if (rest > 0) yield return new WaitForSecondsRealtime(rest);
            // The loading screen leaves and the menu comes in under it: it starts when the spinner is gone, so that
            // its own entrance is seen as the logo dissolves.
            host.Hide(loading);
            yield return new WaitForSecondsRealtime(LoadingScreen.SpinnerFade);
            var auto = AutoStart != null ? StoryCatalog.Find(AutoStart) : null;
            if (auto != null) Open(auto, auto.First, true);
            else ShowMenu();
            if (Updater.Enabled) StartCoroutine(Updater.Check(tag => { update = tag; menu?.ShowUpdate(tag, ShowUpdateProgress, auto: Updater.CanInstall()); }));
        }

        void OnDestroy()
        {
            CloudSave.Merged -= OnCloudMerged;
            CloudSave.Reading = () => false;
        }

        /// Leaving the screen may be the last moment of the app: whatever is new goes up to the cloud.
        void OnApplicationPause(bool paused)
        {
            if (paused && Persist) CloudSave.Sync();
        }

        void Update()
        {
            tint?.Tick(Time.unscaledDeltaTime);
            cast?.Tick(Time.unscaledDeltaTime);
            audio?.Tick(Time.unscaledDeltaTime);
            var keyboard = Keyboard.current;                 // Android Back arrives as Escape
            if (settings != null && keyboard != null && keyboard.escapeKey.wasPressedThisFrame) Back();
            if (Screen.width == lastWidth && Screen.height == lastHeight) return;
            lastWidth = Screen.width; lastHeight = Screen.height;
            Viewport.Measure();                  // takes effect from the next screen
        }

        void Fail(string message)
        {
            Debug.LogError(message);
            var plate = new PlateSpec { Style = PlateStyle.Narr, Label = "Ошибка", Text = message };
            host.Show(BeatMapper.LinePage(plate), new Bank(), () => { });
        }

        // ------------------------------------------------------------ menu and settings
        void OpenSettings()
        {
            if (story != null && hud != null) { storySettings.Open(LeaveStory); return; }       // a story with its own kit has its own modal
            settings.Open(story != null ? () => ShowMenu() : chatScreen != null ? () => chatScreen.Leave() : null, Quit, CloudRow());
        }

        /// The way out of a story: a dark curtain comes down with a note that the progress is kept, the menu is built under
        /// it, and the curtain lifts. The picture of the story and the menu are never seen side by side.
        void LeaveStory()
        {
            if (story == null || exiting) return;
            exiting = true;
            StartCoroutine(LeaveRoutine());
        }

        IEnumerator LeaveRoutine()
        {
            var overlay = host.Overlay; var anim = host.OverlayAnimator;
            var curtain = Ui.Img(overlay, "Curtain", ProceduralArt.White);
            curtain.color = new Color(.03f, .05f, .09f, 0); curtain.raycastTarget = true;
            Ui.Stretch(curtain.rectTransform);
            var column = Kit.Column(overlay, "ExitColumn");
            var note = Ui.Sliced(column, "ExitNote", "plate-mint");
            Ui.Box(note.rectTransform, A.Center, A.Center, Vector2.zero, new Vector2(Kit.Width - 4 * Kit.Side, 108));
            var line = Ui.Txt(note.rectTransform, "Line", "Прогресс сохранён", Kit.F800, Kit.FsHead + 2, new Color32(28, 42, 58, 255), TextAnchor.MiddleCenter);
            Ui.Stretch(line.rectTransform, 30, 22, 30, 0);
            line.rectTransform.offsetMax = new Vector2(line.rectTransform.offsetMax.x, -34);
            line.rectTransform.offsetMin = new Vector2(line.rectTransform.offsetMin.x, 40);
            var sub = Ui.Txt(note.rectTransform, "Sub", "Возвращаемся в меню", Kit.F600, Kit.FsSmall, new Color32(28, 42, 58, 170), TextAnchor.MiddleCenter);
            Ui.Box(sub.rectTransform, A.BottomCenter, A.BottomCenter, new Vector2(0, 34), new Vector2(Kit.Width - 4 * Kit.Side - 60, 20));
            var group = Ui.Group(note);
            group.alpha = 0;
            if (anim.Enabled) anim.Play(Tween.To(.28f, e => { if (curtain) curtain.color = new Color(.03f, .05f, .09f, e); if (group) group.alpha = e; }, Easing.InOut));
            else { curtain.color = new Color(.03f, .05f, .09f, 1); group.alpha = 1; }
            yield return new WaitForSecondsRealtime(anim.Enabled ? .85f : 0);
            storySettings.Close();
            ShowMenu();
            yield return null;
            if (anim.Enabled) anim.Play(Tween.To(.4f, e => { if (curtain) curtain.color = new Color(.03f, .05f, .09f, 1 - e); if (group) group.alpha = 1 - e; }, Easing.InOut));
            yield return new WaitForSecondsRealtime(anim.Enabled ? .45f : 0);
            if (curtain) Destroy(curtain.gameObject);
            if (column) Destroy(column.gameObject);
            exiting = false;
        }

        /// The collapse arrow: the text and the options hide, the picture stays.
        void CollapseText(bool on) => host.ShowPages(!on);

        /// The Google Play row of the settings: who is signed in, or the button that signs in; none without Play Games.
        SettingsToast.Cloud CloudRow() => CloudSave.Status switch
        {
            CloudSave.State.Off => null,
            CloudSave.State.Connecting => new SettingsToast.Cloud { Line = "Подключение…" },
            CloudSave.State.Synced => new SettingsToast.Cloud
            {
                Line = string.IsNullOrEmpty(PlayGames.PlayerName) ? "Прогресс сохраняется" : "Прогресс сохраняется · " + PlayGames.PlayerName,
            },
            _ => new SettingsToast.Cloud { SignIn = SignInToCloud },
        };

        void SignInToCloud()
        {
            signingIn = true;
            CloudSave.SignIn(() => { signingIn = false; Tell("Не получилось войти в Google Play", "info"); });
        }

        /// The cloud copy is in: when it brought something, the menu is drawn again with it.
        void OnCloudMerged(bool restored)
        {
            if (restored && menu != null && story == null)
            {
                StartCoroutine(menu.Leave(opaque: true));
                menu = null;
                ShowMenu();
            }
            if (restored) Tell("Прогресс восстановлен из Google Play", "check");
            else if (signingIn) Tell("Прогресс сохраняется в Google Play", "check");
            signingIn = false;
        }

        /// A banner over the menu; under the loading screen and in a story it waits for the menu.
        void Tell(string text, string icon)
        {
            news = text; newsIcon = icon;
            if (menu != null) TellNews();
        }

        void TellNews()
        {
            if (news == null) return;
            ShellToast.Show(host.Shell, news, host.OverlayAnimator);          // the shell's own toast, not the story's ribbon
            news = null;
        }

        void ShowUpdateProgress(Action<string> status) => StartCoroutine(Updater.Install(status));

        /// The line on screen is already saved: leaving is just leaving.
        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void Back()
        {
            if (settings.IsOpen) settings.Close();
            else if (storySettings.IsOpen) storySettings.Close();
            else if (chatScreen != null) chatScreen.Leave();
            else if (story != null) OpenSettings();
            else if (menu == null || !menu.Back()) Quit();
        }

        const string MenuBackground = "menu_campfire";
        /// How long the loading screen waits for Play Games and the saved progress; a later answer redraws the menu.
        const float CloudWait = 6;

        /// The menu by the fire. Leaving a story needs no saving here: the line on screen is already saved. What is new
        /// goes up to the cloud, and a cloud copy that came during the chapter is merged in before the menu is drawn.
        void ShowMenu(MenuScreen.Tab tab = MenuScreen.Tab.Stories)
        {
            session++;
            story = null; chapter = null; machine = null; lastPlate = null; hints.Clear();
            Art.StoryKit = null;
            hud?.Destroy(); hud = null;
            host.ShowPages(true, false);
            host.Clear();
            if (chatScreen != null) { StartCoroutine(chatScreen.Close()); chatScreen = null; }
            if (Persist) CloudSave.Sync();
            Scenery(MenuBackground);
            menu = new MenuScreen(host.Shell, StoryCatalog.All, ProgressOf, Open, OpenChat, Terms(), animated: true);
            if (update != null) menu.ShowUpdate(update, ShowUpdateProgress);
            if (tab != MenuScreen.Tab.Stories) menu.Select(tab);
            TellNews();
        }

        internal static MenuScreen.Progress ProgressOf(ChapterEntry c) =>
            c.Soon ? MenuScreen.Progress.Locked :
            SaveStore.Has(c.SaveId) ? MenuScreen.Progress.InProgress :
            SaveStore.Finished(c.SaveId) ? MenuScreen.Progress.Finished : MenuScreen.Progress.New;

        /// The words of the library: every note of every chapter that has been read from disk.
        List<Glossary.Term> Terms()
        {
            var all = new List<Glossary.Term>();
            var seen = new HashSet<string>();
            foreach (var s in StoryCatalog.All)
                foreach (var c in s.Chapters)
                    if (!c.Soon && texts.TryGetValue(c.Path, out var json))
                        foreach (var t in Glossary.FromStory(json))
                            if (seen.Add(t.Name)) all.Add(t);
            return all;
        }

        /// The background and the loops of a place, without the scene-change chime.
        void Scenery(string backgroundId)
        {
            tintId = backgroundId;
            ResetCast();
            tint.Set(backgroundId); audio.Scene(backgroundId);
        }

        void ResetCast() => cast.Clear();

        /// The character of a line: one person stands on the screen, the one who speaks now (like in a romance visual
        /// novel); the next speaker rides in and the previous one rides out. A line without a speaker (narration, a
        /// sound, a system line) leaves whoever is there; a speaker without a picture too. `# char: id` shows that character
        /// (`# char: none` nobody), `# emo: sad` is the face, `# look: travel` the outfit of the heroine.
        void UpdateCast(Beat beat)
        {
            var speaker = beat.Voice switch
            {
                Voice.Thought => BeatMapper.Heroine,
                Voice.Dialogue or Voice.Flashback => beat.Speaker,
                _ => null,
            };
            var forced = beat.TagValue("char");
            if (forced == "none") { cast.Clear(); return; }
            var id = forced ?? Cast.Id(speaker);
            if (id == null) return;
            var look = beat.TagValue("look");
            var travel = look != null ? look == "travel" : Cast.Travel(tintId);
            var heroine = Cast.IsHero(id);
            cast.Show(Cast.Sprite(id, beat.TagValue("emo"), travel), Cast.Plain(id, travel), heroine, Cast.Height(id));
        }

        // ------------------------------------------------------------ flow
        /// A chapter from its beginning (`fresh`) or from its save; the title card first either way.
        void Open(StoryEntry entry, ChapterEntry part, bool fresh)
        {
            if (part == null || part.Soon) return;
            if (menu != null) { StartCoroutine(menu.Leave()); menu = null; }
            story = entry; chapter = part; Ended = false; lastPlate = null; hints.Clear();
            Cast.Use(entry.Id);
            Art.StoryKit = Cast.KitPrefix;
            hud?.Destroy(); hud = null;
            SettingsToast.ShowGear(false);                             // every story has its own tools over it, not the gear of the shell
            host.ShowPages(true, false);
            hud = new StoryHud(host.Hud, LeaveStory, CollapseText, OpenSettings, host.OverlayAnimator);
            StartCoroutine(Load(entry, part, fresh, ++session));
        }

        IEnumerator Load(StoryEntry entry, ChapterEntry part, bool fresh, int id)
        {
            if (!texts.TryGetValue(part.Path, out var json))
                yield return StoryFile.Read(part.Path, t => json = texts[part.Path] = t, Fail);
            if (id != session || json == null) yield break;

            machine = StoryMachine.FromJson(json);
            var save = fresh || !Persist ? null : SaveStore.Read(part.SaveId);
            if (save != null)
            {
                try { machine.Load(save.machine); }
                catch (Exception e)
                {
                    Debug.LogWarning($"The save of {part.SaveId} does not fit the story, starting over: {e.Message}");
                    save = null; machine = StoryMachine.FromJson(json);
                }
            }
            if (fresh && Persist) SaveStore.Delete(part.SaveId);
            if (save == null && Persist) ChapterStats.Before(entry, part, SaveStore.ReadStats)?.ApplyTo(machine);
            FillPurse();

            Scenery(entry.Cover ?? "title"); audio.Ui("title_in");       // the title card stands on the cover of this story
            var scene = $"Глава {part.Number}: {part.Title}";
            var title = new TitleSpec { Name = entry.Title, Scene = save != null ? scene + " · продолжение" : scene };
            host.Show(new Page { Id = "title", Title = title }, new Bank(), () =>
            {
                if (save != null) { if (save.background != null) Scenery(save.background); }
                else machine.Advance();
                Present();
            });
        }

        /// Where the reader is: the machine at the line on screen and the background under it.
        void Save()
        {
            if (Persist && story != null)
                SaveStore.Write(new StorySave { story = chapter.SaveId, machine = machine.Save(), background = tintId, at = Progress.Now() });
        }

        /// Walks the machine up to the next beat that needs a screen; stage directions and stat banners pass by.
        void Present()
        {
            TakePurse();
            if (hud != null && hud.Collapsed) { hud.Reset(); host.ShowPages(true); }          // a new screen brings the text back
            while (true)
            {
                var beat = machine.Current;
                if (beat.HasTag("bg"))
                {
                    var bg = beat.TagValue("bg");
                    if (bg != null && bg != tintId) audio.Ui("scene_change", .6f);
                    if (bg != tintId) ResetCast();
                    tintId = bg;
                    tint.Set(bg); audio.Scene(bg);
                }
                if (beat.HasTag("snd")) audio.Sfx(beat.TagValue("snd"));

                switch (beat.Phase)
                {
                    case StoryPhase.Directive:
                        machine.Advance();
                        continue;

                    case StoryPhase.Line:
                        var banner = BeatMapper.StatBanner(beat);
                        if (banner != null) { host.Banner(banner); audio.Ui(banner.StartsWith("-") ? "stat_down" : "stat_up"); machine.Advance(); continue; }
                        var hint = BeatMapper.Hint(beat);
                        if (hint != null)
                        {
                            hints.Add(hint);
                            audio.Ui(beat.Text.StartsWith("НОВЫЙ КВЕСТ") ? "quest_new" : "system_info");
                            machine.Advance();
                            continue;
                        }
                        var plate = BeatMapper.Plate(beat);
                        UpdateCast(beat);
                        Save();
                        var note = BeatMapper.Note(beat);
                        if (note != null) { hints.Add(note); audio.Ui("system_info"); Library.Unlock(beat.TagValue("note")); }
                        else audio.Ui("plate_in", .5f);
                        ShowLine(plate, TextRules.Split(plate.Text), 0);
                        return;

                    case StoryPhase.Choice:
                        // the plate of the last line stays; the options slide in next to it
                        audio.Ui("choice_show");
                        var choice = BeatMapper.ChoicePage(lastPlate, beat, Coins());
                        choice.Info = TakeHints();
                        Show(choice,
                             lastPlate != null ? ScreenHost.Transition.Replace : ScreenHost.Transition.Cross, OnChoiceDone);
                        return;

                    case StoryPhase.End:
                        Ended = true;
                        if (Persist)
                        {
                            // what the reader became goes on into the next chapter; the save of this one is done
                            SaveStore.WriteStats(ChapterStats.Of(machine, chapter.SaveId, Progress.Now()));
                            SaveStore.Delete(chapter.SaveId); SaveStore.MarkFinished(chapter.SaveId);
                        }
                        audio.Scene("black"); audio.Ui("episode_end");
                        Show(new Page { Id = "end", Title = new TitleSpec { Name = "Конец эпизода", Scene = "Тап — в меню" } },
                             ScreenHost.Transition.Cross, () => ShowMenu());
                        return;

                    default:
                        Fail(beat.Error ?? "История остановилась.");
                        return;
                }
            }
        }

        // ------------------------------------------------------------ the chat with the hero of a story
        /// The chat of a story in the shell: from the tab, or from the chapters. The hero feels what the chapters read
        /// so far left in the ink variables.
        void OpenChat(StoryEntry entry)
        {
            session++;
            if (menu != null) { StartCoroutine(menu.Leave(opaque: true)); menu = null; }      // the chat comes in over the menu, the picture behind does not show
            story = null; chapter = null; machine = null; lastPlate = null; hints.Clear();
            host.Clear();
            Scenery(MenuBackground);
            var read = new List<ChapterStats>();
            foreach (var c in entry.Chapters) if (!c.Soon) read.Add(SaveStore.ReadStats(c.SaveId));
            chatScreen = new ChatScreen(host.Shell, this, entry, ChatStore.Open(entry), ChatStore.Energy, ChatList.Latest(read),
                                        () => ShowMenu(MenuScreen.Tab.Chat), () => chatScreen.Leave(() => ShowMenu(MenuScreen.Tab.Shop)),
                                        (text, icon) => ShellToast.Show(host.Shell, text, host.OverlayAnimator, 2.5f, 96));      // over the field of the chat
        }

        /// A line on its plate; a long one in parts, one screen each (the speaker says it in two taps), the hints of
        /// the line on its first screen. The last part is the plate that stays for a choice that follows.
        void ShowLine(PlateSpec plate, string[] parts, int k)
        {
            lastPlate = new PlateSpec { Style = plate.Style, Label = plate.Label, Text = parts[k] };
            var line = BeatMapper.LinePage(lastPlate);
            line.Info = TakeHints();
            var id = session;
            Show(line, ScreenHost.Transition.Cross, k == parts.Length - 1 ? OnLineDone : () =>
            {
                if (id != session) return;
                audio.Ui("tap_next", .7f);
                ShowLine(plate, parts, k + 1);
            });
        }

        /// The hints gathered since the last screen as one text for the top of this one; null when there are none.
        string TakeHints()
        {
            if (hints.Count == 0) return null;
            var text = string.Join("\n", hints);
            hints.Clear();
            return text;
        }

        void Show(Page page, ScreenHost.Transition transition, Action next) =>
            host.Show(page, new Bank(), next, transition);

        void OnLineDone() { audio.Ui("tap_next", .7f); machine.Advance(); Present(); }

        void OnChoiceDone()
        {
            var index = host.Current.ChosenIndex;
            audio.Ui(machine.CanChoose(index) && machine.Current.Options[index].Cost > 0 ? "choice_paid" : "choice_pick");
            if (!machine.Choose(index)) { Fail("Выбор не принят: " + index); return; }
            Present();
        }

        /// The purse goes into the story's coins when a chapter opens (bought or got in the menu, spent in any story) ...
        void FillPurse()
        {
            try { machine.SetVariable("gems", Wallet.Coins); }
            catch (Exception) { }                            // a story without coins
        }

        /// ... and comes back after every step, so that what the story gives or takes is the reader's.
        void TakePurse()
        {
            if (machine == null) return;
            try { Wallet.Coins = machine.GetInt("gems"); }
            catch (Exception) { }
        }

        int Coins()
        {
            try { return machine.GetInt("gems"); }
            catch (Exception) { return 0; }
        }
    }
}
