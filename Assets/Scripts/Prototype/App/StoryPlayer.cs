using System.Collections;
using LegacyThroughTime.Narrative;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// Plays the story (content/stories/golden-cage/episode-01.ink) on the heritage screens: the Narrative engine
    /// produces beats, BeatMapper turns each into a screen, the ScreenHost shows it. Starts itself in any scene;
    /// the whole UI is built in code.
    public sealed class StoryPlayer : MonoBehaviour
    {
        const string StoryPath = "stories/golden-cage/episode-01.json";

        ScreenHost host;
        BackdropTint tint;
        StoryMachine machine;
        PlateSpec lastPlate;
        string json;
        int lastWidth, lastHeight;

        internal PageView Current => host?.Current;
        internal StoryMachine Machine => machine;
        internal bool Ended { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<StoryPlayer>() || FindAnyObjectByType<SessionPrototype>()) return;
            new GameObject("StoryPlayer").AddComponent<StoryPlayer>();
        }

        IEnumerator Start()
        {
            Stage.Setup();
            lastWidth = Screen.width; lastHeight = Screen.height;
            var canvas = Stage.BuildCanvas();
            tint = Stage.BuildBackground(canvas);
            host = new ScreenHost(this, canvas);

            var loading = host.ShowLoading();
            var started = Time.unscaledTime;
            yield return StoryFile.Read(StoryPath, text => json = text, Fail);
            var rest = LoadingScreen.MinSeconds - (Time.unscaledTime - started);     // the logo is seen for a moment at least
            if (rest > 0) yield return new WaitForSecondsRealtime(rest);
            // The loading screen leaves and the title comes in under it: the title starts when the spinner is gone, so that
            // its own entrance is seen as the logo dissolves.
            host.Hide(loading);
            yield return new WaitForSecondsRealtime(LoadingScreen.SpinnerFade);
            if (json != null) Restart();
        }

        void Update()
        {
            tint?.Tick(Time.unscaledDeltaTime);
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

        // ------------------------------------------------------------ flow
        void Restart()
        {
            Ended = false; lastPlate = null;
            machine = StoryMachine.FromJson(json);
            tint.Set("title");
            host.Show(new Page { Id = "title", Title = new TitleSpec { Name = "Золотая клетка", Scene = "Эпизод 1" } },
                      new Bank(), () => { machine.Advance(); Present(); });
        }

        /// Walks the machine up to the next beat that needs a screen; stage directions and stat banners pass by.
        void Present()
        {
            while (true)
            {
                var beat = machine.Current;
                if (beat.HasTag("bg")) tint.Set(beat.TagValue("bg"));

                switch (beat.Phase)
                {
                    case StoryPhase.Directive:
                        machine.Advance();
                        continue;

                    case StoryPhase.Line:
                        var banner = BeatMapper.StatBanner(beat);
                        if (banner != null) { host.Banner(banner); machine.Advance(); continue; }
                        lastPlate = BeatMapper.Plate(beat);
                        Show(BeatMapper.LinePage(lastPlate), ScreenHost.Transition.Cross, OnLineDone);
                        return;

                    case StoryPhase.Choice:
                        // the plate of the last line stays; the options slide in next to it
                        Show(BeatMapper.ChoicePage(lastPlate, beat, Coins()),
                             lastPlate != null ? ScreenHost.Transition.Replace : ScreenHost.Transition.Cross, OnChoiceDone);
                        return;

                    case StoryPhase.End:
                        Ended = true;
                        Show(new Page { Id = "end", Title = new TitleSpec { Name = "Конец эпизода", Scene = "Тап — сначала" } },
                             ScreenHost.Transition.Cross, Restart);
                        return;

                    default:
                        Fail(beat.Error ?? "История остановилась.");
                        return;
                }
            }
        }

        void Show(Page page, ScreenHost.Transition transition, System.Action next) =>
            host.Show(page, new Bank(), next, transition);

        void OnLineDone() { machine.Advance(); Present(); }

        void OnChoiceDone()
        {
            var index = host.Current.ChosenIndex;
            if (!machine.Choose(index)) { Fail("Выбор не принят: " + index); return; }
            Present();
        }

        int Coins()
        {
            try { return machine.GetInt("gems"); }
            catch (System.Exception) { return 0; }
        }
    }
}
