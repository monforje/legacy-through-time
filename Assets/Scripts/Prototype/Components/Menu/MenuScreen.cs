using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The menu by the night fire (Art/Backgrounds/menu_campfire): the purse top left (a tap opens the shop), the gear
    /// of the HUD top right, the tabs at the bottom - the stories (a carousel; a story opens its chapters), the
    /// library of words, the shop. A newer build of the game shows a card over the stories. Android Back steps back:
    /// a card, the chapters, another tab, then the owner's turn (it leaves the app).
    sealed class MenuScreen
    {
        public enum Tab { Stories, Library, Shop }
        public enum Progress { Locked, New, InProgress, Finished }

        const float UpdateHeight = 92, UpdateShift = 34;

        public readonly RectTransform Root;
        readonly IAnimator anim;
        readonly IReadOnlyList<StoryEntry> stories;
        readonly Func<ChapterEntry, Progress> progress;
        readonly Action<StoryEntry, ChapterEntry, bool> open;
        readonly IReadOnlyList<Glossary.Term> terms;
        readonly RectTransform stage, storiesTab;
        readonly TabBar tabs;
        readonly Text purse;
        StoryCarousel carousel;
        ChapterList chapters;
        LibraryPanel library;
        ShopPanel shop;
        RectTransform update;
        Tab tab;
        bool leaving;

        public Tab Current => tab;

        public MenuScreen(RectTransform column, IReadOnlyList<StoryEntry> stories, Func<ChapterEntry, Progress> progress,
                          Action<StoryEntry, ChapterEntry, bool> open, IReadOnlyList<Glossary.Term> terms, bool animated)
        {
            this.stories = stories; this.progress = progress; this.open = open; this.terms = terms;
            Root = Ui.Rect(column, "Menu");
            Ui.Stretch(Root);
            Root.gameObject.AddComponent<CanvasGroup>();
            anim = animated ? Root.gameObject.AddComponent<CoroutineAnimator>() : NoAnimator.Instance;

            // the night above and the ground under the fire a little darker: the top bar and the tabs read on them
            var dim = Ui.Img(Root, "Dim", ProceduralArt.Gradient(new Color(.01f, .03f, .03f, .55f), new Color(.01f, .03f, .03f, 0)));
            Ui.Bleed(dim.rectTransform);
            dim.rectTransform.anchorMin = new Vector2(dim.rectTransform.anchorMin.x, .78f);

            stage = Ui.Rect(Root, "Stage");
            Ui.Stretch(stage, 0, TabBar.Height + Metrics.InsetBottom + Viewport.ExtraBottom, 0, 0);
            storiesTab = Ui.Rect(stage, "Stories");
            Ui.Stretch(storiesTab);
            carousel = new StoryCarousel(storiesTab, stories, progress, OpenChapters, anim);
            Ui.TopRow(carousel.Root, 0, 0, CardTop(), StoryCarousel.CardHeight + 140);

            purse = MenuUi.Purse(Root, () => Select(Tab.Shop));
            Wallet.Changed += OnCoins;
            tabs = new TabBar(Root, Select);
            tabs.Show(Tab.Stories);

            if (anim.Enabled) Entrance();
        }

        void OnCoins(int coins) { if (purse) purse.text = coins.ToString(); }

        /// The card stands where the painting of the fire leaves its dark middle: the background covers the canvas
        /// (1080 x 1919, centred), and its middle is a little above the centre of the picture.
        static float CardTop()
        {
            var picture = 1919f * Mathf.Max(Viewport.Width / 1080f, Viewport.Height / 1919f);
            var middle = Viewport.Height / 2 - .0245f * picture;
            return Mathf.Max(Metrics.HudBottom + Viewport.ExtraTop + 16, middle - StoryCarousel.CardHeight / 2);
        }

        // ------------------------------------------------------------ tabs and screens
        public void Select(Tab next)
        {
            if (leaving) return;
            CloseChapters();
            library?.Close();
            if (next == tab && next != Tab.Stories) return;
            if (library != null) { UnityEngine.Object.Destroy(library.Root.gameObject); library = null; }
            if (shop != null) { shop.Dispose(); UnityEngine.Object.Destroy(shop.Root.gameObject); shop = null; }
            tab = next;
            storiesTab.gameObject.SetActive(tab == Tab.Stories);
            if (tab == Tab.Library) library = new LibraryPanel(stage, terms, anim);
            if (tab == Tab.Shop) shop = new ShopPanel(stage, anim);
            var panel = tab == Tab.Library ? library.Root : tab == Tab.Shop ? shop.Root : null;
            if (panel && anim.Enabled)
            {
                var group = Ui.Group(panel); group.alpha = 0;
                anim.Play(Tween.To(.22f, e => { if (group) group.alpha = e; }, Easing.Linear));
            }
            tabs.Show(tab);
        }

        /// For the tools (smoke, screenshots): the chapters of a story, a turn of the carousel, a word of the library.
        public void ShowChapters(StoryEntry story) => OpenChapters(story);
        public void Turn(int step) => carousel.Turn(step);
        public void ShowTerm(Glossary.Term term) { if (tab != Tab.Library) Select(Tab.Library); library.Show(term); }

        /// Lets go of the purse and the store (the menu is destroyed without Leave).
        public void Dispose()
        {
            Wallet.Changed -= OnCoins;
            shop?.Dispose();
        }

        void OpenChapters(StoryEntry story)
        {
            CloseChapters();
            chapters = new ChapterList(stage, story, progress, (chapter, fresh) => { if (!leaving) open(story, chapter, fresh); }, CloseChapters, anim);
            Carousel(false);                                 // the list over the fire, not over the cards
        }

        void CloseChapters()
        {
            if (chapters == null) return;
            UnityEngine.Object.Destroy(chapters.Root.gameObject);
            chapters = null;
            Carousel(true);
        }

        void Carousel(bool on)
        {
            carousel.Root.gameObject.SetActive(on);
            if (update) update.gameObject.SetActive(on);
        }

        /// Android Back: true when the menu stepped back itself; false at the stories, where Back leaves the app.
        public bool Back()
        {
            if (library != null && library.DetailOpen) { library.Close(); return true; }
            if (chapters != null) { CloseChapters(); return true; }
            if (tab != Tab.Stories) { Select(Tab.Stories); return true; }
            return false;
        }

        // ------------------------------------------------------------ the night after a chapter
        /// "Сон" between the purse and the gear: the heroine of a chapter read waits for the Voice of her dream.
        public void ShowDream(Action open)
        {
            if (!Root || Root.Find("Dream")) return;
            var pill = Ui.Img(Root, "Dream", Art.Sprite("pill"));
            pill.type = Art.Tiled("pill") ? Image.Type.Tiled : Image.Type.Sliced;
            pill.raycastTarget = true;
            Ui.Box(pill.rectTransform, A.TopCenter, A.TopCenter, new Vector2(0, -(Metrics.InsetTop + 16 + Viewport.ExtraTop)), new Vector2(104, 44));
            Ui.Lift(pill, 4, .3f);
            Ui.Icon(pill.rectTransform, "Bubble", "bubble", A.MidLeft, A.MidLeft, new Vector2(14, 1), 20);
            var label = Ui.Txt(pill.rectTransform, "Text", "Сон", Art.Accent, 18, Theme.Parchment, TextAnchor.MiddleLeft);
            Ui.Emboss(label);
            Ui.Box(label.rectTransform, A.MidLeft, A.MidLeft, new Vector2(42, 1), new Vector2(56, 30));
            PressButton.On(pill.gameObject, () => { if (!leaving) open(); }, .95f);
        }

        // ------------------------------------------------------------ a newer build
        /// A card over the stories: a newer build of the game can be downloaded; "Позже" removes it. `download` starts
        /// the update and reports progress through the callback it is given (null = back to normal). `auto` starts
        /// it right away, so the update comes by itself and the button is only a retry.
        public void ShowUpdate(string version, Action<Action<string>> download, bool auto = false)
        {
            if (update || !Root) return;
            var offer = "Сборка " + version + " — установится поверх";
            var card = Ui.Sliced(storiesTab, "Update", "card");
            update = card.rectTransform;
            // compact, between the top bar and the card: the text on the left, the button and "Позже" on the right
            Ui.TopRow(update, Metrics.Side, Metrics.Side, Metrics.HudBottom + Viewport.ExtraTop - 4, UpdateHeight);
            Ui.Lift(card, 6, .45f);
            var title = Ui.Txt(update, "Title", "Новая версия", Art.Accent, 19, Theme.Ink);
            Ui.TopRow(title.rectTransform, 18, 134, 16, 26);
            var line = Ui.Txt(update, "Version", offer, Art.Sub, 12, Theme.Scarlet, TextAnchor.UpperLeft);
            Ui.TopRow(line.rectTransform, 18, 134, 42, 34);
            var button = MenuUi.Button(update, "Download", "Обновить", () => { if (!leaving) Start(); });
            Ui.Box(button.rectTransform, A.TopRight, A.TopRight, new Vector2(-10, -12), new Vector2(118, 40));
            void Start() => download(s => { if (line) line.text = s ?? offer; });

            var later = Ui.Img(update, "Later", ProceduralArt.White);
            later.color = Color.clear; later.raycastTarget = true;
            Ui.Box(later.rectTransform, A.TopRight, A.TopRight, new Vector2(-10, -54), new Vector2(118, 28));
            var laterText = Ui.Txt(later.rectTransform, "Text", "Позже", Art.Action, 15, Theme.Ink.WithAlpha(.7f), TextAnchor.MiddleCenter);
            Ui.Stretch(laterText.rectTransform);
            PressButton.On(later.gameObject, HideUpdate, .97f);

            Shift(UpdateShift);
            if (anim.Enabled)
            {
                var group = Ui.Group(card); group.alpha = 0;
                anim.Play(Tween.To(.4f, e => group.alpha = e, Easing.Linear));
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

        /// The carousel moves down (or back up) under the update card.
        void Shift(float by) => carousel.Root.anchoredPosition -= new Vector2(0, by);

        // ------------------------------------------------------------ entrance and exit
        void Entrance()
        {
            var group = Root.GetComponent<CanvasGroup>();
            group.alpha = 0;
            anim.Play(Tween.To(.5f, e => group.alpha = e, Easing.Linear));
            var rt = carousel.Root; var y0 = rt.anchoredPosition.y;
            anim.Play(Tween.To(.6f, e => { if (rt) rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y0 - 22 * (1 - e)); }, Easing.OutExpo, .1f));
        }

        /// Fades away and destroys itself; run by the owner (the menu's own animator dies with it).
        public IEnumerator Leave()
        {
            if (leaving || !Root) yield break;
            leaving = true;
            Wallet.Changed -= OnCoins;
            shop?.Dispose();
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
