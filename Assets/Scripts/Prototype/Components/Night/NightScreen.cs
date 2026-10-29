using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The night after a chapter: the reader, the Voice of the heroine's dream, talks to her in their own words
    /// (docs/brainstorm/2026-10-29-character-chat.md). Her plates on the left as in the chapter, the Voice's smoky
    /// gauze on the right, the author's chips over the field, the beads of the replies left on a scarlet thread.
    /// When they are spent she wakes: the author's dawn, "Досмотреть сон" for coins, or back to the menu. Leaving
    /// has the server write what she keeps of the dream (DreamStore.Remember), for the next one.
    sealed class NightScreen
    {
        const float Gap = 42, ReportHeight = 24, FieldHeight = 52, ChipHeight = 38, FooterHeight = 34, DawnHeight = 168;
        const int MaxLetters = 300;

        public readonly RectTransform Root;
        readonly MonoBehaviour owner;
        readonly IAnimator anim;
        readonly ChapterEntry chapter;
        readonly ChapterStats stats;
        readonly string storyId;
        readonly Action leave;
        readonly Action<string, string> tell;           // a banner (text, icon) over the screen

        readonly RectTransform feed, panel, viewport;
        readonly Text title;
        readonly KeyboardLift lift;
        readonly ScrollRect scroll;
        readonly List<Row> rows = new();
        DreamNight night;
        Dream dream;
        Row waiting;                                    // her plate while she "is silent", then her reply as it comes
        string reply;
        InputField input;
        bool busy, leaving;

        sealed class Row
        {
            public RectTransform Root;
            public PlateView Plate;
            public float Height;
            public DreamLine Line;
        }

        public NightScreen(RectTransform parent, MonoBehaviour owner, StoryEntry story, ChapterEntry chapter, ChapterStats stats,
                           Action leave, Action<string, string> tell, bool animated = true)
        {
            this.owner = owner; this.chapter = chapter; this.stats = stats; this.leave = leave; this.tell = tell;
            storyId = story.Id;
            Root = Ui.Rect(parent, "Night");
            Ui.Stretch(Root);
            anim = animated ? Root.gameObject.AddComponent<CoroutineAnimator>() : NoAnimator.Instance;

            MenuUi.Veil(Root, .35f).raycastTarget = true;    // the night room darker; nothing under it takes a tap
            var top = Metrics.HudBottom + Viewport.ExtraTop;
            title = MenuUi.Fit(Ui.Txt(Root, "Title", "Сон", Art.Accent, 24, Theme.Parchment, TextAnchor.MiddleCenter), 16);
            Ui.TopRow(title.rectTransform, 58, 62, top - 52, 34);
            MenuUi.Shadow(title);
            var back = Ui.Icon(Root, "Back", "arrow", A.TopLeft, A.Center, new Vector2(Metrics.Side + 8, -(top - 35)), 22);
            back.rectTransform.localScale = new Vector3(-1, 1, 1);
            back.raycastTarget = true;
            PressButton.On(back.gameObject, Leave, .9f);

            var bottom = Metrics.InsetBottom + Viewport.ExtraBottom + FooterHeight + FieldHeight + ChipHeight + 24;
            feed = MenuUi.List(Root, top, bottom);
            viewport = (RectTransform)feed.parent;
            scroll = viewport.GetComponent<ScrollRect>();

            panel = Ui.Rect(Root, "Panel");
            Ui.Stretch(panel);
            lift = panel.gameObject.AddComponent<KeyboardLift>();
            lift.Feed = viewport; lift.FeedBottom = bottom;

            if (anim.Enabled)
            {
                var group = Ui.Group(Root); group.alpha = 0;
                anim.Play(Tween.To(.5f, e => { if (group) group.alpha = e; }, Easing.Linear));
            }
            Status("Сююмбике засыпает…");
            if (owner) owner.StartCoroutine(Open());       // without an owner (the screenshots) the dream is given by Show
        }

        // ------------------------------------------------------------ the dream of tonight
        IEnumerator Open()
        {
            string problem = null;
            yield return DreamServer.Night(n => night = n, p => problem = p);
            if (!Root) yield break;
            if (night == null || night.Id != chapter.Night) { Trouble(problem ?? "Этой ночи ещё нет", Retry); yield break; }

            var today = ShopCatalog.Day(DateTime.Now);
            dream = DreamStore.Read(storyId);
            if (dream != null && !dream.IsOf(night.Id, today))
            {
                yield return DreamStore.Remember(storyId, dream);           // last night's dream first, then tonight's
                if (!Root) yield break;
                dream = Dream.Begin(night, today, dream.memory, new System.Random());
            }
            dream ??= Dream.Begin(night, today, "", new System.Random());
            DreamStore.Write(storyId, dream);
            Show(dream, night);
        }

        void Retry()
        {
            Clear();
            Status("Сююмбике засыпает…");
            owner.StartCoroutine(Open());
        }

        /// The whole dream on the screen (also for the screenshots, without the server).
        public void Show(Dream d, DreamNight n)
        {
            dream = d; night = n;
            if (!string.IsNullOrEmpty(n.Title)) title.text = n.Title;
            Clear();
            foreach (var line in dream.lines) Add(line);
            Relayout(); ScrollDown();
            Bottom();
        }

        void Clear()
        {
            foreach (var r in rows) if (r.Root) UnityEngine.Object.Destroy(r.Root.gameObject);
            rows.Clear();
            waiting = null;
        }

        // ------------------------------------------------------------ the feed
        Row Add(DreamLine line, PlateStyle? style = null, string label = null)
        {
            var row = new Row { Root = Ui.Rect(feed, "Line " + rows.Count), Line = line };
            rows.Add(row);
            Fill(row, line.text, style ?? Style(line), label ?? Label(line));
            return row;
        }

        PlateStyle Style(DreamLine line) =>
            line.role == "voice" ? PlateStyle.Voice : line.text.StartsWith("[ВНЕ РОЛИ]") ? PlateStyle.Hint : PlateStyle.SpeechLeft;

        string Label(DreamLine line) =>
            line.role == "voice" ? "Голос" : line.text.StartsWith("[ВНЕ РОЛИ]") ? "Важно" : chapter.Heroine;

        /// (Re)builds the plate of a row; its height is measured from the text.
        void Fill(Row row, string text, PlateStyle style, string label)
        {
            if (row.Plate != null) UnityEngine.Object.Destroy(row.Plate.Root.gameObject);
            var shown = text.StartsWith("[ВНЕ РОЛИ]") ? text.Substring(10).TrimStart() : text;
            row.Plate = new PlateView(row.Root, new PlateSpec { Style = style, Label = label, Text = shown });
            Ui.TopRow(row.Plate.Root, 0, 0, Gap, row.Plate.Height);
            row.Height = Gap + row.Plate.Height;
            // her own replies, not the author's first line nor the plate she is still speaking on
            if (row.Line != null && row.Line.role == "heroine" && !string.IsNullOrEmpty(row.Line.text) && rows.IndexOf(row) > 0)
                ReportLink(row);
        }

        /// ⚑ under her reply: it goes to be checked (stores ask it of apps with generated text).
        void ReportLink(Row row)
        {
            var reported = row.Line.reported;
            var link = Ui.Img(row.Root, "Report", ProceduralArt.White);
            link.color = Color.clear; link.raycastTarget = !reported;
            Ui.Box(link.rectTransform, A.TopRight, A.TopRight, new Vector2(-6, -(row.Height + 2)), new Vector2(220, ReportHeight));
            var colour = reported ? Theme.Parchment.WithAlpha(.45f) : Theme.Parchment.WithAlpha(.7f);
            Ui.Icon(link.rectTransform, "Flag", "flag", A.MidRight, A.MidRight, new Vector2(-4, 0), 15, colour);
            var text = Ui.Txt(link.rectTransform, "Text", reported ? "Отправлено на проверку" : "Пожаловаться", Art.Sub, 11, colour, TextAnchor.MiddleRight);
            Ui.Stretch(text.rectTransform, 0, 0, 24, 0);
            MenuUi.Shadow(text, .5f);
            row.Height += ReportHeight;
            if (!reported) PressButton.On(link.gameObject, () => Report(row), .96f);
        }

        void Relayout()
        {
            var y = 0f;
            foreach (var r in rows)
            {
                Ui.TopRow(r.Root, 0, 0, y, r.Height);
                y += r.Height;
            }
            MenuUi.SetHeight(feed, y + 12);
        }

        void ScrollDown()
        {
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 0;
        }

        /// A quiet line in place of the feed: waking up, no network.
        void Status(string text)
        {
            var row = new Row { Root = Ui.Rect(feed, "Status") };
            rows.Add(row);
            var t = Ui.Txt(row.Root, "Text", text, Art.Body, 16, Theme.Parchment.WithAlpha(.85f), TextAnchor.MiddleCenter, FontStyle.Italic);
            Ui.Stretch(t.rectTransform);
            MenuUi.Shadow(t);
            row.Height = 120;
            Relayout();
        }

        void Trouble(string text, Action retry)
        {
            Clear();
            var row = new Row { Root = Ui.Rect(feed, "Trouble") };
            rows.Add(row);
            row.Plate = new PlateView(row.Root, new PlateSpec { Style = PlateStyle.Hint, Label = "Тишина", Text = text });
            Ui.TopRow(row.Plate.Root, 0, 0, Gap, row.Plate.Height);
            row.Height = Gap + row.Plate.Height + 64;
            var again = MenuUi.Button(row.Root, "Again", "Ещё раз", retry);
            Ui.Box(again.rectTransform, A.TopCenter, A.TopCenter, new Vector2(0, -(Gap + row.Plate.Height + 14)), new Vector2(160, 44));
            Relayout();
        }

        // ------------------------------------------------------------ speaking
        void Send(string text)
        {
            text = (text ?? "").Trim();
            if (text.Length > MaxLetters) text = text.Substring(0, MaxLetters);
            if (busy || leaving || dream == null || text.Length == 0 || dream.Left(night.Limit) == 0) return;
            var said = new DreamLine { role = "voice", text = text };
            dream.lines.Add(said);
            DreamStore.Write(storyId, dream);
            Add(said);
            busy = true; reply = "";
            waiting = Add(new DreamLine { role = "heroine", text = "" }, PlateStyle.SpeechLeft, chapter.Heroine);
            Fill(waiting, "<i>молчит…</i>", PlateStyle.SpeechLeft, chapter.Heroine);
            Relayout(); ScrollDown();
            Bottom();
            owner.StartCoroutine(DreamServer.Chat(dream.ChatBody(stats), OnDelta, flags => OnReply(flags), problem => OnFail(said, problem)));
        }

        void OnDelta(string text)
        {
            if (!Root || waiting == null) return;
            reply += text;
            Fill(waiting, reply.Trim(), Style(new DreamLine { role = "heroine", text = reply }), Label(new DreamLine { role = "heroine", text = reply }));
            Relayout(); ScrollDown();
        }

        void OnReply(List<string> flags)
        {
            busy = false;
            var text = reply.Trim();
            if (text.Length == 0) { OnFail(dream.lines[dream.lines.Count - 1], "Она промолчала. Скажите иначе"); return; }
            var line = new DreamLine { role = "heroine", text = text };
            dream.lines.Add(line);
            DreamStore.Write(storyId, dream);
            if (!Root) return;
            if (flags.Count > 0) Debug.Log("Dream reply flags: " + string.Join(", ", flags));
            waiting.Line = line;
            Fill(waiting, text, Style(line), Label(line));
            waiting = null;
            Relayout(); ScrollDown();
            Bottom();
        }

        /// No reply: the Voice's words come back into the field, nothing is spent.
        void OnFail(DreamLine said, string problem)
        {
            busy = false;
            dream.lines.Remove(said);
            DreamStore.Write(storyId, dream);
            if (!Root) return;
            if (waiting != null) { UnityEngine.Object.Destroy(waiting.Root.gameObject); rows.Remove(waiting); waiting = null; }
            var last = rows[rows.Count - 1];
            if (last.Line == said) { UnityEngine.Object.Destroy(last.Root.gameObject); rows.Remove(last); }
            Relayout(); ScrollDown();
            Bottom();
            if (input) input.text = said.text;
            tell(problem, "info");
        }

        void Report(Row row)
        {
            var index = dream.lines.IndexOf(row.Line);
            if (index < 0) return;
            owner.StartCoroutine(DreamServer.Report(dream.ReportBody(index, stats), ok =>
            {
                if (!ok) { tell("Не получилось отправить, попробуйте позже", "info"); return; }
                row.Line.reported = true;
                DreamStore.Write(storyId, dream);
                tell("Ответ отправлен на проверку. Спасибо", "check");
                if (!Root || !row.Root) return;
                Fill(row, row.Line.text, Style(row.Line), Label(row.Line));
                Relayout();
            }));
        }

        // ------------------------------------------------------------ the bottom: chips, field, beads / the dawn
        RectTransform bottomRoot;

        void Bottom()
        {
            if (bottomRoot) UnityEngine.Object.Destroy(bottomRoot.gameObject);
            bottomRoot = Ui.Rect(panel, "Bottom");
            Ui.Stretch(bottomRoot);
            var floor = Metrics.InsetBottom + Viewport.ExtraBottom;
            var left = dream.Left(night.Limit);
            Footer(floor, left);
            var dawn = left == 0 && !busy;
            if (dawn) Dawn(floor + FooterHeight);
            else { Field(floor + FooterHeight); Chips(floor + FooterHeight + FieldHeight + 10); }
            // the feed ends above what the bottom holds now, and its last plate is seen whole
            lift.FeedBottom = floor + FooterHeight + (dawn ? DawnHeight + 14 : FieldHeight + ChipHeight + 24);
            viewport.offsetMin = new Vector2(viewport.offsetMin.x, lift.FeedBottom + panel.anchoredPosition.y);
            ScrollDown();
        }

        void Footer(float floor, int left)
        {
            var total = night.Limit + dream.bought;
            var beads = Ui.Rect(bottomRoot, "Beads");
            Ui.BottomRow(beads, Metrics.Side, Metrics.Side, floor + 16, 14);
            var thread = Ui.Img(beads, "Thread", ProceduralArt.White);
            thread.color = Theme.Scarlet.WithAlpha(.8f);
            const float step = 15;
            var width = Mathf.Min(total, 20) * step;
            Ui.Box(thread.rectTransform, A.Center, A.Center, Vector2.zero, new Vector2(width + 10, 2));
            for (var i = 0; i < Mathf.Min(total, 20); i++)
                Ui.Icon(beads, "Bead " + i, i < total - left ? "chat-bead-out" : "chat-bead", A.Center, A.Center,
                        new Vector2(-width / 2 + step / 2 + i * step, 0), 12);
            var note = Ui.Txt(bottomRoot, "Note", "Вымышленный диалог по мотивам истории · осталось " + left,
                              Art.Sub, 10, Theme.Parchment.WithAlpha(.7f), TextAnchor.MiddleCenter);
            Ui.BottomRow(note.rectTransform, Metrics.Side, Metrics.Side, floor, 14);
            MenuUi.Shadow(note, .5f);
        }

        void Field(float bottom)
        {
            var box = Ui.Sliced(bottomRoot, "Field", "chat-field");
            box.raycastTarget = true;
            Ui.BottomRow(box.rectTransform, Metrics.Side, Metrics.Side + 56, bottom, FieldHeight);
            Ui.Lift(box, 4, .3f);

            var text = Ui.Txt(box.rectTransform, "Text", "", Art.Body, 16, Theme.Ink, TextAnchor.MiddleLeft);
            text.supportRichText = false;
            Ui.Stretch(text.rectTransform, 18, 8, 16, 8);
            var hint = Ui.Txt(box.rectTransform, "Placeholder", busy ? "Сююмбике молчит…" : "Сказать…", Art.Body, 16,
                              Theme.Ink.WithAlpha(.45f), TextAnchor.MiddleLeft, FontStyle.Italic);
            Ui.Stretch(hint.rectTransform, 18, 8, 16, 8);

            var keep = input ? input.text : "";
            input = box.gameObject.AddComponent<InputField>();
            input.targetGraphic = box; input.textComponent = text; input.placeholder = hint;
            input.characterLimit = MaxLetters;
            input.lineType = InputField.LineType.MultiLineSubmit;
            input.caretColor = Theme.Ink; input.customCaretColor = true;
            input.selectionColor = Theme.Scarlet.WithAlpha(.3f);
            input.interactable = !busy;
            input.text = keep;
            input.onSubmit.AddListener(_ => SendField());

            var send = Ui.Icon(bottomRoot, "Send", "chat-send", A.BottomRight, A.BottomRight,
                               new Vector2(-(Metrics.Side - 2), bottom + 2), 48, busy ? new Color(1, 1, 1, .45f) : Color.white);
            send.raycastTarget = true;
            PressButton.On(send.gameObject, SendField, .92f);
        }

        void SendField()
        {
            if (!input || busy) return;
            var text = input.text;
            if (string.IsNullOrWhiteSpace(text)) return;
            input.text = "";
            Send(text);
        }

        /// The author's chips not asked yet tonight, in a row that scrolls sideways.
        void Chips(float bottom)
        {
            var asked = new HashSet<string>();
            foreach (var l in dream.lines) if (l.role == "voice") asked.Add(l.text);
            var bleed = (Viewport.Width - Metrics.RefWidth) / 2;          // the row runs to the edges of a wide screen
            var view = Ui.Img(bottomRoot, "Chips", ProceduralArt.White);
            view.color = Color.clear; view.raycastTarget = true;
            Ui.BottomRow(view.rectTransform, -bleed, -bleed, bottom, ChipHeight + 6);
            view.gameObject.AddComponent<RectMask2D>();
            var content = Ui.Rect(view.rectTransform, "Content");
            content.anchorMin = A.BottomLeft; content.anchorMax = A.TopLeft; content.pivot = A.MidLeft;
            var side = bleed + Metrics.Side;
            var x = side;
            foreach (var c in night.Chips)
            {
                if (asked.Contains(c)) continue;
                var chip = Ui.Sliced(content, "Chip", "chat-chip");
                var label = Ui.Txt(chip.rectTransform, "Text", c, Art.Action, 14, Theme.Ink, TextAnchor.MiddleCenter);
                var w = Ui.TextWidth(label) + 34;
                Ui.Box(chip.rectTransform, A.MidLeft, A.MidLeft, new Vector2(x, 2), new Vector2(w, ChipHeight));
                Ui.Stretch(label.rectTransform, 12, 2, 12, 2);
                Ui.Lift(chip, 3, .3f);
                var said = c;
                PressButton.On(chip.gameObject, () => Send(said), .96f);
                if (busy) chip.color = new Color(1, 1, 1, .5f);
                x += w + 8;
            }
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(Mathf.Max(x + side - 8, Viewport.Width), 0);
            var sideways = view.gameObject.AddComponent<ScrollRect>();
            sideways.viewport = view.rectTransform; sideways.content = content;
            sideways.vertical = false; sideways.movementType = ScrollRect.MovementType.Elastic;
        }

        /// The replies are spent: she wakes. More of the dream for coins, or the menu.
        void Dawn(float bottom)
        {
            const float height = DawnHeight;
            var card = Ui.Sliced(bottomRoot, "Dawn", "card");
            card.raycastTarget = true;
            Ui.BottomRow(card.rectTransform, Metrics.Side - 6, Metrics.Side - 6, bottom + 4, height);
            Ui.Lift(card, 6, .45f);
            var text = Ui.Txt(card.rectTransform, "Wake", night.Wake, Art.Body, 14, Theme.Ink, TextAnchor.UpperLeft, FontStyle.Italic);
            Ui.TopRow(text.rectTransform, 22, 22, 18, 84);
            MenuUi.Fit(text, 11);

            var more = MenuUi.Button(card.rectTransform, "More", $"Досмотреть · {Dream.ExtraCost}", BuyMore, icon: "coin");
            Ui.Box(more.rectTransform, A.BottomLeft, A.BottomLeft, new Vector2(14, 16), new Vector2(178, 46));
            var wake = MenuUi.Button(card.rectTransform, "Wake", "Проснуться", Leave, "btn-chosen");
            Ui.Box(wake.rectTransform, A.BottomRight, A.BottomRight, new Vector2(-14, 16), new Vector2(128, 46));
            if (anim.Enabled)
            {
                var group = Ui.Group(card); group.alpha = 0;
                anim.Play(Tween.To(.6f, e => { if (group) group.alpha = e; }, Easing.Linear, .5f));
            }
        }

        void BuyMore()
        {
            if (Wallet.Coins < Dream.ExtraCost)
            {
                tell($"Нужно {Dream.ExtraCost} монет. Монеты — в магазине у костра", "info");
                return;
            }
            Wallet.Coins -= Dream.ExtraCost;
            dream.bought += Dream.ExtraReplies;
            DreamStore.Write(storyId, dream);
            Bottom();
        }

        // ------------------------------------------------------------ leaving
        /// Back to the menu; what she keeps of tonight is written meanwhile (the owner outlives this screen).
        public void Leave()
        {
            if (leaving) return;
            leaving = true;
            if (dream != null) owner.StartCoroutine(DreamStore.Remember(storyId, dream));
            leave();
        }

        public IEnumerator Close()
        {
            if (!Root) yield break;
            var group = Ui.Group(Root);
            group.blocksRaycasts = false;
            for (var t = 0f; t < .3f && Root; t += Time.unscaledDeltaTime)
            {
                group.alpha = 1 - t / .3f;
                yield return null;
            }
            if (Root) UnityEngine.Object.Destroy(Root.gameObject);
        }
    }

    /// The bottom of the night rises over the on-screen keyboard, and the feed ends above it.
    sealed class KeyboardLift : MonoBehaviour
    {
        public RectTransform Feed;
        public float FeedBottom;

        void Update()
        {
            var lift = 0f;
            if (TouchScreenKeyboard.isSupported && TouchScreenKeyboard.visible && Screen.height > 0)
                lift = Mathf.Max(0, TouchScreenKeyboard.area.height / Screen.height * Viewport.Height - Metrics.InsetBottom - Viewport.ExtraBottom);
            var rt = (RectTransform)transform;
            var y = Mathf.Lerp(rt.anchoredPosition.y, lift, Mathf.Min(1, Time.unscaledDeltaTime * 14));
            if (Mathf.Abs(y - lift) < .5f) y = lift;
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
            if (Feed) Feed.offsetMin = new Vector2(Feed.offsetMin.x, FeedBottom + y);
        }
    }
}
