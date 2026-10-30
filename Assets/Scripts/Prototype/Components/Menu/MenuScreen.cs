using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The shell of the app (docs/brainstorm/templates/shell, docs/superpowers/plans/2026-10-30-shell-unity.md): the pills of
    /// energy and gems on top (a tap on "+" opens the shop), the home with the deck of stories, and the bar of five
    /// sections: stories, chats, the wheel, the words, the rewards. The wheel and the rewards are popups over the screen,
    /// the shop is a page of its own. The look is the neutral kit; "Наследие" stays inside the stories.
    /// A newer build of the game shows a card over the stories. Android Back steps back: a popup, a card, the chapters,
    /// the shop, another tab, then the owner's turn (it leaves the app).
    sealed class MenuScreen
    {
        public enum Tab { Stories, Chat, Library, Shop, Wheel, Rewards }
        public enum Progress { Locked, New, InProgress, Finished }

        const float UpdateHeight = 92, UpdateShift = 34;

        /// The sections of the bar, left to right.
        static readonly Tab[] Bar = { Tab.Stories, Tab.Chat, Tab.Wheel, Tab.Library, Tab.Rewards };

        public readonly RectTransform Root;
        readonly IAnimator anim;
        readonly IReadOnlyList<StoryEntry> stories;
        readonly Func<ChapterEntry, Progress> progress;
        readonly Action<StoryEntry, ChapterEntry, bool> open;
        readonly Action<StoryEntry> openChat;
        readonly IReadOnlyList<Glossary.Term> terms;
        readonly RectTransform stage, pills;
        readonly ShellNav nav;
        readonly ShellPill energyPill, gemPill;
        readonly HomeTab home;
        ChapterList chapters;
        LibraryPanel library;
        ShopScreen shop;
        ChatsPanel chats;
        WheelDialog wheel;
        RewardsDialog rewards;
        RectTransform update;
        Tab tab;
        bool leaving;

        public Tab Current => tab;

        /// Where the popups go: the column above the menu, so that they also cover the gear of the HUD.
        Transform Layer => Root.parent;

        public MenuScreen(RectTransform column, IReadOnlyList<StoryEntry> stories, Func<ChapterEntry, Progress> progress,
                          Action<StoryEntry, ChapterEntry, bool> open, Action<StoryEntry> openChat, IReadOnlyList<Glossary.Term> terms, bool animated)
        {
            this.stories = stories; this.progress = progress; this.open = open; this.openChat = openChat; this.terms = terms;
            Root = Ui.Rect(column, "Menu");
            Ui.Stretch(Root);
            Root.gameObject.AddComponent<CanvasGroup>();
            anim = animated ? Root.gameObject.AddComponent<CoroutineAnimator>() : NoAnimator.Instance;
            Kit.Ground(Root);

            stage = Ui.Rect(Root, "Stage");
            Ui.Stretch(stage, 0, Kit.NavHeight + Viewport.ExtraBottom, 0, 0);

            var top = Metrics.InsetTop + Kit.Space3 + Viewport.ExtraTop;
            home = new HomeTab(stage, stories, OpenChapters, anim, top + Kit.Touch + Kit.Space3);

            // the pills of the top bar: two equal ones, the corner on the right is the gear of the HUD
            pills = Ui.Rect(Root, "Pills");
            Ui.TopRow(pills, Kit.Side, Kit.Side + 52, top, Kit.Touch);
            var each = (Metrics.RefWidth - 2 * Kit.Side - 52 - Kit.Space2) / 2;
            energyPill = Parts.Pill(pills, "energy", ShellPurse.Energy, () => Select(Tab.Shop));
            Ui.Box(energyPill.Root, A.MidLeft, A.MidLeft, Vector2.zero, new Vector2(each, Kit.Touch));
            gemPill = Parts.Pill(pills, "gem", ShellPurse.Gems, () => Select(Tab.Shop));
            Ui.Box(gemPill.Root, A.MidRight, A.MidRight, Vector2.zero, new Vector2(each, Kit.Touch));
            ShellPurse.Changed += OnPurse;
            Wallet.Changed += OnCoins;

            nav = new ShellNav(Root, new[] { ("book", "Истории"), ("chat", "Чат"), ("wheel", "Колесо"), ("star", "Слова"), ("gift", "Награды") },
                               0, i => Select(Bar[i]));

            if (anim.Enabled) Entrance();
        }

        void OnPurse() { if (!Root) return; energyPill.Count(ShellPurse.Energy, anim); gemPill.Count(ShellPurse.Gems, anim); }
        void OnCoins(int coins) => OnPurse();

        // ------------------------------------------------------------ tabs and screens
        public void Select(Tab next)
        {
            if (leaving) return;
            // the wheel and the rewards are popups: the screen under them stays
            if (next == Tab.Wheel) { OpenWheel(); return; }
            if (next == Tab.Rewards) { OpenRewards(); return; }
            CloseChapters();
            library?.Close();
            if (next == tab && next != Tab.Stories) return;
            if (library != null) { UnityEngine.Object.Destroy(library.Root.gameObject); library = null; }
            if (shop != null) { shop.Dispose(); UnityEngine.Object.Destroy(shop.Root.gameObject); shop = null; }
            if (chats != null) { UnityEngine.Object.Destroy(chats.Root.gameObject); chats = null; }
            tab = next;
            home.Root.gameObject.SetActive(tab == Tab.Stories);
            if (tab == Tab.Chat) chats = new ChatsPanel(stage, ChatStore.Rows(stories, Started), s => { if (!leaving) openChat(s); }, () => Select(Tab.Stories), DateTime.Now, anim);
            if (tab == Tab.Library) library = new LibraryPanel(stage, terms, anim);
            if (tab == Tab.Shop) shop = new ShopScreen(Root, anim, () => Select(Tab.Stories));
            pills.gameObject.SetActive(tab == Tab.Stories);                    // the other sections have their own headers
            nav.Root.gameObject.SetActive(tab != Tab.Shop);                    // the shop has its own bar
            var panel = tab == Tab.Chat ? chats.Root : tab == Tab.Library ? library.Root : tab == Tab.Shop ? shop.Root : null;
            if (panel && anim.Enabled)
            {
                var group = Ui.Group(panel); group.alpha = 0;
                anim.Play(Tween.To(Kit.Dur.Normal, e => { if (group) group.alpha = e; }, Easing.Linear));
            }
            nav.Show(Array.IndexOf(Bar, tab));
        }

        void OpenWheel()
        {
            if (wheel != null && wheel.IsOpen) return;
            CloseRewards();
            wheel = new WheelDialog(Layer, anim, () => { wheel = null; nav.Show(Array.IndexOf(Bar, tab)); }, () => Select(Tab.Shop));
            nav.Show(Array.IndexOf(Bar, Tab.Wheel));
        }

        void OpenRewards()
        {
            if (rewards != null && rewards.IsOpen) return;
            CloseWheel();
            rewards = new RewardsDialog(Layer, anim, () => { rewards = null; nav.Show(Array.IndexOf(Bar, tab)); });
            nav.Show(Array.IndexOf(Bar, Tab.Rewards));
        }

        void CloseWheel() { if (wheel != null && wheel.IsOpen) wheel.Close(); wheel = null; }
        void CloseRewards() { if (rewards != null && rewards.IsOpen) rewards.Close(); rewards = null; }

        /// For the tools (smoke, screenshots): the chapters of a story, a turn of the deck, a word of the library.
        public void ShowChapters(StoryEntry story) => OpenChapters(story);
        public void Turn(int step) => home.Turn(step);

        /// A story the reader has begun (a chapter in progress or read): its hero is in the chats.
        bool Started(StoryEntry story)
        {
            foreach (var c in story.Chapters)
                if (!c.Soon && progress(c) is Progress.InProgress or Progress.Finished) return true;
            return false;
        }

        public void ShowTerm(Glossary.Term term) { if (tab != Tab.Library) Select(Tab.Library); library.Show(term); }

        /// Lets go of the purse and the store (the menu is destroyed without Leave).
        public void Dispose()
        {
            ShellPurse.Changed -= OnPurse;
            Wallet.Changed -= OnCoins;
            shop?.Dispose();
        }

        void OpenChapters(StoryEntry story)
        {
            CloseChapters();
            chapters = new ChapterList(stage, story, progress, (chapter, fresh) => { if (!leaving) open(story, chapter, fresh); },
                                       () => { if (!leaving) openChat(story); }, CloseChapters, anim);
            Home(false);                                     // the list over the ground, not over the deck
        }

        void CloseChapters()
        {
            if (chapters == null) return;
            UnityEngine.Object.Destroy(chapters.Root.gameObject);
            chapters = null;
            Home(true);
        }

        void Home(bool on)
        {
            home.Root.gameObject.SetActive(on && tab == Tab.Stories);
            pills.gameObject.SetActive(on && tab == Tab.Stories);
            if (update) update.gameObject.SetActive(on);
        }

        /// Android Back: true when the menu stepped back itself; false at the stories, where Back leaves the app.
        public bool Back()
        {
            if (wheel != null && wheel.IsOpen) { wheel.Close(); return true; }
            if (rewards != null && rewards.IsOpen) { rewards.Close(); return true; }
            if (library != null && library.DetailOpen) { library.Close(); return true; }
            if (chapters != null) { CloseChapters(); return true; }
            if (tab != Tab.Stories) { Select(Tab.Stories); return true; }
            return false;
        }

        // ------------------------------------------------------------ a newer build
        /// A card over the stories: a newer build of the game can be downloaded; "Позже" removes it. `download` starts
        /// the update and reports progress through the callback it is given (null = back to normal). `auto` starts
        /// it right away, so the update comes by itself and the button is only a retry.
        public void ShowUpdate(string version, Action<Action<string>> download, bool auto = false)
        {
            if (update || !Root) return;
            var offer = "Сборка " + version + " — установится поверх";
            var card = Kit.Round(stage, "Update", Kit.Surface2, Kit.RadiusM);
            update = card.rectTransform;
            card.raycastTarget = true;
            // compact, between the top bar and the deck: the text on the left, the button and "Позже" on the right
            Ui.TopRow(update, Kit.Side, Kit.Side, Metrics.InsetTop + Kit.Space3 + Viewport.ExtraTop + Kit.Touch + Kit.Space2, UpdateHeight);
            var title = Ui.Txt(update, "Title", "Новая версия", Kit.F800, Kit.FsHead, Kit.Text);
            Ui.TopRow(title.rectTransform, 16, 134, 14, 26);
            var line = Ui.Txt(update, "Version", offer, Kit.F600, Kit.FsSmall, Kit.Muted, TextAnchor.UpperLeft);
            Ui.TopRow(line.rectTransform, 16, 134, 42, 40);
            void Start() => download(s => { if (line) line.text = s ?? offer; });
            var button = Parts.Button(update, "Обновить", ButtonStyle.Primary, () => { if (!leaving) Start(); }, 40);
            Ui.Box(button.Root, A.TopRight, A.TopRight, new Vector2(-10, -10), new Vector2(112, 40));

            var later = Ui.Img(update, "Later", ProceduralArt.White);
            later.color = Color.clear; later.raycastTarget = true;
            Ui.Box(later.rectTransform, A.TopRight, A.TopRight, new Vector2(-10, -54), new Vector2(112, 30));
            var laterText = Ui.Txt(later.rectTransform, "Text", "Позже", Kit.F800, Kit.FsBody, Kit.Muted, TextAnchor.MiddleCenter);
            Ui.Stretch(laterText.rectTransform);
            PressButton.On(later.gameObject, HideUpdate, .97f);

            Shift(UpdateShift);
            if (anim.Enabled)
            {
                var group = Ui.Group(card); group.alpha = 0;
                anim.Play(Tween.To(Kit.Dur.Slow, e => group.alpha = e, Easing.Linear));
            }
            if (auto) Start();
        }

        void HideUpdate()
        {
            if (!update) return;
            UnityEngine.Object.Destroy(update.gameObject);
            update = null;
            Shift(-UpdateShift);
        }

        /// The home moves down (or back up) under the update card.
        void Shift(float by) => home.Root.anchoredPosition -= new Vector2(0, by);

        // ------------------------------------------------------------ entrance and exit
        void Entrance()
        {
            var group = Root.GetComponent<CanvasGroup>();
            group.alpha = 0;
            anim.Play(Tween.To(Kit.Dur.Slow * 1.5f, e => group.alpha = e, Easing.Linear));
            var rt = home.Root; var y0 = rt.anchoredPosition.y;
            anim.Play(Tween.To(Kit.Dur.Slow * 2f, e => { if (rt) rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y0 - 22 * (1 - e)); }, Easing.OutExpo, .1f));
        }

        /// Fades away and destroys itself; run by the owner (the menu's own animator dies with it).
        public IEnumerator Leave()
        {
            if (leaving || !Root) yield break;
            leaving = true;
            ShellPurse.Changed -= OnPurse;
            Wallet.Changed -= OnCoins;
            shop?.Dispose();
            CloseWheel(); CloseRewards();
            var group = Root.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            for (var t = 0f; t < .25f && Root; t += Time.unscaledDeltaTime)
            {
                group.alpha = 1 - t / .25f;
                yield return null;
            }
            if (Root) UnityEngine.Object.Destroy(Root.gameObject);
        }
    }
}
