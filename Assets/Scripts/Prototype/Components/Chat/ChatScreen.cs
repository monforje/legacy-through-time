using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// A talk with the hero of a story (docs/notes/chat-rework.md): the hero's bubbles on the left, the reader's on the
    /// right. The top bar has the way back, the hero, the energy and the flag that reports the last answer; the
    /// reader sends with the arrow inside the field (or Enter). A message costs one energy; with none left the field
    /// is shut and a card sends the reader to the shop. The hero answers after "печатает…". No bottom bar, no
    /// hints, no mood: the chat is neutral, only the name and the avatar colour come from the story.
    sealed class ChatScreen
    {
        const float BarHeight = 56, FieldHeight = 52, Gap = Kit.Space3, Pad = 12, BubbleX = Kit.Space4, BubbleY = Kit.Space3, MaxShare = .82f;
        const string Note = "Диалог вымышленный: героя играет ИИ по мотивам истории. Факты смотрите в эпизодах.";

        public readonly RectTransform Root;
        readonly MonoBehaviour owner;
        readonly IAnimator anim;
        readonly StoryEntry story;
        readonly Chat chat;
        readonly Energy energy;
        readonly ChapterStats stats;
        readonly Action leave, openShop;
        readonly Action<string, string> tell;           // a banner (text, icon) over the screen

        readonly RectTransform feed, panel, viewport, bottomRoot;
        readonly ScrollRect scroll;
        readonly KeyboardLift lift;
        readonly List<Row> rows = new();
        readonly Text energyText;
        readonly RectTransform energyPill;
        Row limit;                                      // the card of the empty energy
        Row typing;                                     // the hero's bubble while she "types", then her reply as it comes
        string reply;
        InputField input;
        Image send;
        PressButton sendButton;
        float contentWidth;
        bool busy, leaving;

        sealed class Row
        {
            public RectTransform Root;
            public ChatLine Line;       // null: the note, the typing bubble, the card
            public float Height;
        }

        public bool Busy => busy;
        public int Count => rows.Count;

        public ChatScreen(RectTransform parent, MonoBehaviour owner, StoryEntry story, Chat chat, Energy energy, ChapterStats stats,
                          Action leave, Action openShop, Action<string, string> tell, bool animated = true)
        {
            this.owner = owner; this.story = story; this.chat = chat; this.energy = energy; this.stats = stats;
            this.leave = leave; this.openShop = openShop; this.tell = tell;
            Root = Ui.Rect(parent, "Chat");
            Ui.Stretch(Root);
            anim = animated ? Root.gameObject.AddComponent<CoroutineAnimator>() : NoAnimator.Instance;
            Kit.Ground(Root);
            SettingsToast.ShowGear(false);                   // the flag takes the corner of the gear
            contentWidth = Metrics.RefWidth - 2 * Kit.Side;

            // ---- the top bar (templates/shell/chat.html): back, hero, the energy pill and the flag
            var top = Metrics.InsetTop + Kit.Space3 + Viewport.ExtraTop;
            var bar = Ui.Rect(Root, "Bar");
            Ui.TopRow(bar, Kit.Side, Kit.Side, top, Kit.Touch);
            var back = Parts.IconButton(bar, "chevron-l", () => Leave(), Kit.Touch);
            back.Face.gameObject.name = "BackHit";
            Ui.Box(back.Root, A.MidLeft, A.MidLeft, Vector2.zero, new Vector2(Kit.Touch, Kit.Touch));
            var avatar = Kit.Avatar(bar, story.Hero, story.Id, 40);
            Ui.Box(avatar.rectTransform, A.MidLeft, A.MidLeft, new Vector2(52, 0), new Vector2(40, 40));
            var name = MenuUi.Fit(Ui.Txt(bar, "Name", story.Hero, Kit.F800, Kit.FsHead, Kit.Text), 12);
            Ui.Stretch(name.rectTransform, 100, 0, 52 + 56 + 8, 0);

            // the compact pill: 56 x 32, a 20 px picture, the number
            energyPill = Kit.Round(bar, "Energy", Kit.Surface, 16).rectTransform;
            Ui.Box(energyPill, A.MidRight, A.MidRight, new Vector2(-(Kit.Touch + 8), 0), new Vector2(56, 32));
            Kit.Icon(energyPill, "Bolt", Kit.Pic("energy"), 20, A.MidLeft, new Vector2(Kit.Space3, 0)).raycastTarget = false;
            energyText = Ui.Txt(energyPill, "Value", energy.value.ToString(), Kit.F800, Kit.FsSmall, Kit.Text, TextAnchor.MiddleLeft);
            Ui.Stretch(energyText.rectTransform, Kit.Space3 + 20 + Kit.Space1, 0, Kit.Space3, 0);

            var flag = Parts.IconButton(bar, "flag", Report, Kit.Touch);
            Ui.Box(flag.Root, A.MidRight, A.MidRight, Vector2.zero, new Vector2(Kit.Touch, Kit.Touch));

            // ---- the feed and the bottom (no tab bar on this screen)
            var floor = Metrics.InsetBottom + Viewport.ExtraBottom;
            var bottom = floor + FieldHeight + Gap;
            feed = MenuUi.List(Root, top + BarHeight - Kit.Space3 + Kit.Space3, bottom, 0);
            viewport = (RectTransform)feed.parent;
            scroll = viewport.GetComponent<ScrollRect>();

            panel = Ui.Rect(Root, "Panel");
            Ui.Stretch(panel);
            lift = panel.gameObject.AddComponent<KeyboardLift>();
            lift.Feed = viewport; lift.FeedBottom = bottom;
            bottomRoot = Ui.Rect(panel, "Bottom");
            Ui.Stretch(bottomRoot);

            AddNote();
            foreach (var line in chat.lines) Add(line, false);
            Field(floor);
            if (!energy.CanSend) OutOfEnergy();
            Relayout(); ScrollDown();

            if (anim.Enabled)
            {
                var group = Ui.Group(Root); group.alpha = 0;
                var rt = Root; var x0 = 18f;
                anim.Play(Tween.To(.28f, e => { if (rt) { group.alpha = e; rt.anchoredPosition = new Vector2(x0 * (1 - e), 0); } }));
            }
            if (chat.unread > 0) { chat.MarkSeen(); ChatStore.Write(chat); }
            ShowEnergy();
        }

        // ------------------------------------------------------------ the feed
        void AddNote()
        {
            var row = new Row { Root = Ui.Rect(feed, "Note") };
            rows.Add(row);
            var t = Ui.Txt(row.Root, "Text", Note, Kit.F600, Kit.FsSmall, Kit.Muted, TextAnchor.MiddleCenter);
            row.Height = Ui.TextHeight(t, contentWidth - 26) + Gap;     // measured first: on a stretched rect sizeDelta adds to the width
            Ui.Stretch(t.rectTransform, 13, Gap, 13, 0);
        }

        Row Add(ChatLine line, bool slide = true)
        {
            var row = new Row { Root = Ui.Rect(feed, line.role == ChatLine.Hero ? "Hero" : "Reader"), Line = line };
            rows.Add(row);
            Bubble(row, line.text, line.role == ChatLine.Reader);
            if (slide && anim.Enabled) SlideIn(row);
            return row;
        }

        /// (Re)builds the bubble of a row; its height is measured from the text.
        void Bubble(Row row, string text, bool mine)
        {
            foreach (Transform child in row.Root) UnityEngine.Object.Destroy(child.gameObject);
            var box = Ui.Img(row.Root, "Bubble", Tail(mine), Image.Type.Sliced);
            box.color = mine ? Kit.Accent : Kit.Surface;
            var label = Ui.Txt(box.rectTransform, "Text", text, Kit.F600, Kit.FsBody, mine ? Kit.AccentInk : Kit.Text, TextAnchor.UpperLeft);
            label.supportRichText = false;
            var max = contentWidth * MaxShare - 2 * BubbleX;
            var width = Mathf.Ceil(Mathf.Min(max, Ui.TextWidth(label))) + 2 * BubbleX;
            var textHeight = Ui.TextHeight(label, width - 2 * BubbleX);
            var height = Mathf.Ceil(textHeight) + 2 * BubbleY;
            var anchor = mine ? A.TopRight : A.TopLeft;
            Ui.Box(box.rectTransform, anchor, anchor, Vector2.zero, new Vector2(width, height));
            Ui.Stretch(label.rectTransform, BubbleX, BubbleY, BubbleX, BubbleY);
            row.Height = height + Gap;
        }

        static Sprite tailThem, tailMine;

        /// A bubble: 16 on every corner and 4 on the one the speech comes from (the bottom left for the hero, the bottom right
        /// for the reader), drawn 3 pixels to the unit as a 9-slice.
        static Sprite Tail(bool mine)
        {
            if (mine ? tailMine : tailThem) return mine ? tailMine : tailThem;
            const int n = 96, big = 48, small = 12;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var data = new Color32[n * n];
            for (var y = 0; y < n; y++)
                for (var x = 0; x < n; x++)
                {
                    float px = x + .5f, py = y + .5f;
                    var right = px > n / 2f; var topHalf = py > n / 2f;
                    var r = !topHalf && (mine ? right : !right) ? small : big;
                    float qx = Mathf.Abs(px - n / 2f) - (n / 2f - r), qy = Mathf.Abs(py - n / 2f) - (n / 2f - r);
                    var d = new Vector2(Mathf.Max(qx, 0), Mathf.Max(qy, 0)).magnitude + Mathf.Min(Mathf.Max(qx, qy), 0) - r;
                    data[y * n + x] = new Color(1, 1, 1, Mathf.Clamp01(.5f - d));
                }
            tex.SetPixels32(data); tex.Apply(false, true);
            var sprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(.5f, .5f), 300, 0, SpriteMeshType.FullRect, new Vector4(big, big, big, big));
            if (mine) tailMine = sprite; else tailThem = sprite;
            return sprite;
        }

        /// A new message comes a little from the side, with a fade.
        void SlideIn(Row row)
        {
            var group = Ui.Group(row.Root); group.alpha = 0;
            anim.Play(Tween.To(.3f, e => { if (group) group.alpha = e; }));
            var bubble = row.Root.Find("Bubble") as RectTransform;
            if (bubble == null) return;
            var mine = row.Line != null && row.Line.role == ChatLine.Reader;
            var x0 = bubble.anchoredPosition.x;
            anim.Play(Tween.To(.3f, e => { if (bubble) bubble.anchoredPosition = new Vector2(x0 + (mine ? 16 : -16) * (1 - e), bubble.anchoredPosition.y); }));
        }

        void Typing()
        {
            typing = new Row { Root = Ui.Rect(feed, "Typing") };
            rows.Add(typing);
            var box = Ui.Img(typing.Root, "Bubble", Tail(false), Image.Type.Sliced);
            box.color = Kit.Surface;
            Ui.Box(box.rectTransform, A.TopLeft, A.TopLeft, Vector2.zero, new Vector2(58, 40));
            var dots = new Graphic[3];
            for (var i = 0; i < 3; i++)
            {
                dots[i] = Ui.Img(box.rectTransform, "Dot " + i, ProceduralArt.Disc);
                dots[i].color = Kit.Muted;
                Ui.Box(dots[i].rectTransform, A.Center, A.Center, new Vector2((i - 1) * 10, 0), new Vector2(6, 6));
            }
            box.gameObject.AddComponent<TypingDots>().Dots = dots;
            typing.Height = 40 + Gap;
            if (anim.Enabled) SlideIn(typing);
        }

        /// The rows stand from the bottom of the feed (a messenger): with few of them the room is above, with many the feed scrolls.
        void Relayout()
        {
            var total = 0f;
            foreach (var r in rows) total += r.Height;
            total -= Gap;                                              // no gap under the last row: the field has its own
            Canvas.ForceUpdateCanvases();
            var room = viewport.rect.height;
            var y = Mathf.Max(0, room - total);
            foreach (var r in rows)
            {
                Ui.TopRow(r.Root, Kit.Side, Kit.Side, y, r.Height - Gap);
                y += r.Height;
            }
            MenuUi.SetHeight(feed, Mathf.Max(room, y - Gap));
        }

        void ScrollDown()
        {
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 0;
        }

        void Drop(Row row)
        {
            if (row == null) return;
            if (row.Root) UnityEngine.Object.Destroy(row.Root.gameObject);
            rows.Remove(row);
        }

        // ------------------------------------------------------------ speaking
        /// What the field can do now: send needs words, a free hand and energy.
        public bool CanSend => !busy && !leaving && energy.CanSend && input && !string.IsNullOrWhiteSpace(input.text);

        public void Send(string text)
        {
            text = Chat.Clean(text);
            if (text == null || busy || leaving || !energy.TrySpend()) return;
            ChatStore.Write(energy);
            ShowEnergy(true);
            var said = chat.Say(text, Progress.Now());
            ChatStore.Write(chat);
            Add(said);
            busy = true; reply = "";
            Typing();
            Relayout(); ScrollDown();
            Bottom();
            if (owner) owner.StartCoroutine(ChatServer.Chat(chat.ChatBody(stats), OnDelta, _ => OnReply(), problem => OnFail(said, problem)));
        }

        /// The server's answer without the network: the screens' tests and shots give the hero's reply, or a failure.
        internal void Deliver(string text) { OnDelta(text); OnReply(); }
        internal void Fail(string problem) { if (busy) OnFail(chat.Last, problem); }

        void OnDelta(string text)
        {
            if (!Root || typing == null) return;
            reply += text;
            Bubble(typing, reply.TrimStart(), false);
            Relayout(); ScrollDown();
        }

        void OnReply()
        {
            busy = false;
            var text = (reply ?? "").Trim();
            if (text.Length == 0) { OnFail(chat.Last, "Герой промолчал. Скажите иначе"); return; }
            var line = chat.Reply(text, Progress.Now(), seen: true);
            ChatStore.Write(chat);
            if (!Root) return;
            typing.Line = line;
            typing.Root.name = "Hero";
            Bubble(typing, text, false);
            typing = null;
            Relayout(); ScrollDown();
            Bottom();
        }

        /// No reply: the reader's words come back into the field, the energy is returned.
        void OnFail(ChatLine said, string problem)
        {
            busy = false;
            chat.Take(said);
            energy.Refund();
            ChatStore.Write(chat); ChatStore.Write(energy);
            if (!Root) return;
            Drop(typing); typing = null;
            var mine = rows.Find(r => r.Line == said);
            Drop(mine);
            Relayout(); ScrollDown();
            ShowEnergy();
            Bottom();
            if (input) input.text = said.text;
            tell(problem, "info");
        }

        // ------------------------------------------------------------ the flag
        void Report()
        {
            var index = chat.ReportableIndex();
            if (index < 0 || !owner) { tell("Пока не на что жаловаться", "info"); return; }
            var line = chat.lines[index];
            owner.StartCoroutine(ChatServer.Report(chat.ReportBody(index, stats), ok =>
            {
                if (!ok) { tell("Не получилось отправить, попробуйте позже", "info"); return; }
                line.reported = true;
                ChatStore.Write(chat);
                tell("Ответ отправлен на проверку. Спасибо", "check");
            }));
        }

        // ------------------------------------------------------------ energy
        void ShowEnergy(bool pulse = false)
        {
            energyText.text = energy.value.ToString();
            if (pulse && anim.Enabled)
            {
                var rt = energyPill;
                anim.Play(Tween.To(.35f, e => { if (rt) rt.localScale = Vector3.one * (1 + .18f * Mathf.Sin(e * Mathf.PI)); }, Easing.Linear));
            }
        }

        // ------------------------------------------------------------ the bottom: the field, or the card when energy is out
        void Field(float bottom)
        {
            var box = Kit.Round(bottomRoot, "Field", Kit.Surface);
            box.raycastTarget = true;
            Ui.BottomRow(box.rectTransform, Kit.Side, Kit.Side, bottom, FieldHeight);
            var out_ = !energy.CanSend;

            var text = Ui.Txt(box.rectTransform, "Text", "", Kit.F600, 15, Kit.Text, TextAnchor.MiddleLeft);
            text.supportRichText = false;
            Ui.Stretch(text.rectTransform, 16, 6, 56, 6);
            var hint = Ui.Txt(box.rectTransform, "Placeholder", out_ ? "Энергия закончилась" : "Сказать…", Kit.F600, 15,
                              Kit.Muted, TextAnchor.MiddleLeft);
            Ui.Stretch(hint.rectTransform, 16, 6, 56, 6);

            var keep = input ? input.text : "";
            input = box.gameObject.AddComponent<InputField>();
            input.targetGraphic = box; input.textComponent = text; input.placeholder = hint;
            input.characterLimit = Chat.MaxLetters;
            input.lineType = InputField.LineType.MultiLineSubmit;
            input.caretColor = Kit.Text; input.customCaretColor = true;
            input.selectionColor = Kit.Accent.WithAlpha(.35f);
            input.interactable = !out_ && !busy;
            input.text = keep;
            input.onSubmit.AddListener(_ => SendField());
            input.onValueChanged.AddListener(_ => ShowSend());

            send = Kit.Round(box.rectTransform, "Send", Kit.Accent, Kit.RadiusS);
            send.raycastTarget = true;
            Ui.Box(send.rectTransform, A.MidRight, A.MidRight, new Vector2(-4, 0), new Vector2(44, 44));
            Kit.Icon(send.rectTransform, "Arrow", Kit.Glyph("arrow-right", ink: true), 24).raycastTarget = false;
            sendButton = PressButton.On(send.gameObject, SendField, .92f);
            ShowSend();
        }

        void ShowSend()
        {
            if (!send) return;
            var on = CanSend;
            var group = Ui.Group(send);
            group.alpha = on ? 1 : .45f;
            sendButton.Interactable = on;
        }

        void SendField()
        {
            if (!input || !CanSend) return;
            var text = input.text;
            input.text = "";
            Send(text);
        }

        /// The bottom after a change: the field again, and the card above it when the energy is out.
        void Bottom()
        {
            var floor = Metrics.InsetBottom + Viewport.ExtraBottom;
            foreach (Transform child in bottomRoot) UnityEngine.Object.Destroy(child.gameObject);
            Drop(limit); limit = null;
            Field(floor);
            Relayout(); ScrollDown();
            if (!energy.CanSend && !busy) OutOfEnergy();
        }

        const float CardHeight = 150;

        void OutOfEnergy()
        {
            var card = Kit.Round(feed, "Limit", Kit.Surface);
            card.raycastTarget = true;
            var row = limit = new Row { Root = card.rectTransform, Height = CardHeight + Gap };
            rows.Add(row);
            var head = Ui.Txt(card.rectTransform, "Head", "Энергия закончилась", Kit.F800, 18, Kit.Text, TextAnchor.MiddleCenter);
            Ui.TopRow(head.rectTransform, Pad, Pad, 14, 26);
            var text = Ui.Txt(card.rectTransform, "Text", "Каждое сообщение тратит 1 энергию. Она восстанавливается со временем, или пополните её сейчас.",
                              Kit.F600, 13, Kit.Muted, TextAnchor.UpperCenter);
            Ui.TopRow(text.rectTransform, Pad + 4, Pad + 4, 44, 50);
            var button = Kit.Round(card.rectTransform, "Refill", Kit.Accent);
            button.raycastTarget = true;
            Ui.BottomRow(button.rectTransform, Pad, Pad, 12, 44);
            var label = Ui.Txt(button.rectTransform, "Text", "Пополнить энергию", Kit.F800, 16, Kit.AccentInk, TextAnchor.MiddleCenter);
            Ui.Stretch(label.rectTransform);
            PressButton.On(button.gameObject, () => { if (!leaving) openShop(); }, .97f);
            Relayout(); ScrollDown();
            if (anim.Enabled) SlideIn(row);
        }

        // ------------------------------------------------------------ leaving
        /// Back to the list; what the hero keeps of the talk is written meanwhile (the owner outlives this screen).
        public void Leave(Action next = null)
        {
            if (leaving) return;
            leaving = true;
            SettingsToast.ShowGear(true);
            chat.MarkSeen();
            ChatStore.Write(chat);
            if (owner) owner.StartCoroutine(ChatStore.Remember(chat));
            (next ?? leave)();
        }

        public IEnumerator Close()
        {
            if (!Root) yield break;
            var group = Ui.Group(Root);
            group.blocksRaycasts = false;
            for (var t = 0f; t < .2f && Root; t += Time.unscaledDeltaTime)
            {
                group.alpha = 1 - t / .2f;
                yield return null;
            }
            if (Root) UnityEngine.Object.Destroy(Root.gameObject);
        }
    }
}
