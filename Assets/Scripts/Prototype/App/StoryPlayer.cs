using System;
using System.Collections;
using System.Collections.Generic;
using LegacyThroughTime.Narrative;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LegacyThroughTime.Prototype
{
    /// The app: loading screen -> the menu (a board of stories, StoryCatalog) -> a story on the heritage screens: the
    /// Narrative engine produces beats, BeatMapper turns each into a screen, the ScreenHost shows it. Every line shown
    /// is saved (SaveStore), so a story continues where the reader left it. The gear opens the settings toast (sound,
    /// back to the menu); Android Back closes it, opens it in a story, leaves the app from the menu.
    /// Starts itself in any scene; the whole UI is built in code.
    public sealed class StoryPlayer : MonoBehaviour
    {
        ScreenHost host;
        BackdropTint tint;
        StoryAudio audio;
        SettingsToast settings;
        StoryBoard board;
        StoryEntry story;                      // null in the menu
        StoryMachine machine;
        PlateSpec lastPlate;
        string tintId;
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
            audio = new StoryAudio(transform);
            host = new ScreenHost(this, canvas);
            settings = new SettingsToast(host.Overlay, host.OverlayAnimator);
            SettingsToast.Gear(host.Hud, OpenSettings);

            audio.Ui("loading_logo");
            var loading = host.ShowLoading();
            var started = Time.unscaledTime;
            yield return null;                                   // the logo is drawn first, then the warm-up runs under it
            Art.Prefetch("title");
            _ = Art.Head; _ = Art.Sub; _ = Art.Body; _ = Art.Action;
            foreach (var s in StoryCatalog.All)                   // the stories are read under the logo too (a failure is retried on open)
                if (!s.Soon) yield return StoryFile.Read(s.Path, t => texts[s.Path] = t, Debug.LogWarning);
            var rest = LoadingScreen.MinSeconds - (Time.unscaledTime - started);     // the logo is seen for a moment at least
            if (rest > 0) yield return new WaitForSecondsRealtime(rest);
            // The loading screen leaves and the menu comes in under it: it starts when the spinner is gone, so that
            // its own entrance is seen as the logo dissolves.
            host.Hide(loading);
            yield return new WaitForSecondsRealtime(LoadingScreen.SpinnerFade);
            var auto = AutoStart != null ? StoryCatalog.Find(AutoStart) : null;
            if (auto != null) Open(auto, true);
            else ShowMenu();
        }

        void Update()
        {
            tint?.Tick(Time.unscaledDeltaTime);
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
        void OpenSettings() => settings.Open(story != null ? ShowMenu : null);

        void Back()
        {
            if (settings.IsOpen) settings.Close();
            else if (story != null) OpenSettings();
            else Application.Quit();
        }

        /// The board of stories. Leaving a story needs no saving here: the line on screen is already saved.
        void ShowMenu()
        {
            session++;
            story = null; machine = null; lastPlate = null;
            host.Clear();
            Scenery("title");
            board = new StoryBoard(host.Pages, StoryCatalog.All, ProgressOf, Open, animated: true);
        }

        static StoryBoard.Progress ProgressOf(StoryEntry s) =>
            SaveStore.Has(s.Id) ? StoryBoard.Progress.InProgress :
            SaveStore.Finished(s.Id) ? StoryBoard.Progress.Finished : StoryBoard.Progress.New;

        /// The background and the loops of a place, without the scene-change chime.
        void Scenery(string backgroundId)
        {
            tintId = backgroundId;
            tint.Set(backgroundId); audio.Scene(backgroundId);
        }

        // ------------------------------------------------------------ flow
        /// The story from its beginning (`fresh`) or from its save; the title card first either way.
        void Open(StoryEntry entry, bool fresh)
        {
            if (board != null) { StartCoroutine(board.Leave()); board = null; }
            story = entry; Ended = false; lastPlate = null;
            StartCoroutine(Load(entry, fresh, ++session));
        }

        IEnumerator Load(StoryEntry entry, bool fresh, int id)
        {
            if (!texts.TryGetValue(entry.Path, out var json))
                yield return StoryFile.Read(entry.Path, t => json = texts[entry.Path] = t, Fail);
            if (id != session || json == null) yield break;

            machine = StoryMachine.FromJson(json);
            var save = fresh || !Persist ? null : SaveStore.Read(entry.Id);
            if (save != null)
            {
                try { machine.Load(save.machine); }
                catch (Exception e)
                {
                    Debug.LogWarning($"The save of {entry.Id} does not fit the story, starting over: {e.Message}");
                    save = null; machine = StoryMachine.FromJson(json);
                }
            }
            if (fresh && Persist) SaveStore.Delete(entry.Id);

            Scenery("title"); audio.Ui("title_in");
            var title = new TitleSpec { Name = entry.Title, Scene = save != null ? entry.Episode + " · продолжение" : entry.Episode };
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
                SaveStore.Write(new StorySave { story = story.Id, machine = machine.Save(), background = tintId });
        }

        /// Walks the machine up to the next beat that needs a screen; stage directions and stat banners pass by.
        void Present()
        {
            while (true)
            {
                var beat = machine.Current;
                if (beat.HasTag("bg"))
                {
                    var bg = beat.TagValue("bg");
                    if (bg != null && bg != tintId) audio.Ui("scene_change", .6f);
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
                        lastPlate = BeatMapper.Plate(beat);
                        Save();
                        if (beat.Voice == Voice.System) audio.Ui(beat.Text.StartsWith("НОВЫЙ КВЕСТ") ? "quest_new" : "system_info");
                        else audio.Ui("plate_in", .5f);
                        Show(BeatMapper.LinePage(lastPlate), ScreenHost.Transition.Cross, OnLineDone);
                        return;

                    case StoryPhase.Choice:
                        // the plate of the last line stays; the options slide in next to it
                        audio.Ui("choice_show");
                        Show(BeatMapper.ChoicePage(lastPlate, beat, Coins()),
                             lastPlate != null ? ScreenHost.Transition.Replace : ScreenHost.Transition.Cross, OnChoiceDone);
                        return;

                    case StoryPhase.End:
                        Ended = true;
                        if (Persist) { SaveStore.Delete(story.Id); SaveStore.MarkFinished(story.Id); }
                        audio.Scene("black"); audio.Ui("episode_end");
                        Show(new Page { Id = "end", Title = new TitleSpec { Name = "Конец эпизода", Scene = "Тап — в меню" } },
                             ScreenHost.Transition.Cross, ShowMenu);
                        return;

                    default:
                        Fail(beat.Error ?? "История остановилась.");
                        return;
                }
            }
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

        int Coins()
        {
            try { return machine.GetInt("gems"); }
            catch (Exception) { return 0; }
        }
    }
}
