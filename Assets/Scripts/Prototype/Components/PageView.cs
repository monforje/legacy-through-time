using System;
using System.Collections;
using UnityEngine;

namespace LegacyThroughTime.Prototype
{
    /// One screen, composed of components from what its Page spec contains: title card, plate with options and
    /// timer, choice panel, picker, info cloud, stat banner. This class only wires them together (taps while
    /// the text types, a choice made, the end of the screen); the look and the motion live in the components.
    sealed class PageView
    {
        public readonly RectTransform Root;
        public readonly CanvasGroup Group;
        /// Option.Index of the chosen option once `next` was called for a choice; -1 otherwise.
        public int ChosenIndex { get; private set; } = -1;

        readonly Page page;
        readonly Bank bank;
        readonly Action next;
        readonly IAnimator anim;
        readonly float introDelay;

        PlateView plate;
        PlateAnimator plateAnimator;
        TypedText typed;
        OptionStack stack;
        PickerPanel picker;
        bool decided;

        /// `animated: false` builds the same screen without any motion (tests, the Editor smoke run).
        /// `introDelay`: the plate appears this much later (the previous plate is still leaving).
        public PageView(RectTransform parent, Page page, Bank bank, Action next, bool animated, float introDelay = 0)
        {
            this.page = page; this.bank = bank; this.next = next; this.introDelay = introDelay;

            Root = Ui.Rect(parent, "Page " + page.Id);
            Ui.Stretch(Root);
            Group = Root.gameObject.AddComponent<CanvasGroup>();
            anim = animated ? Root.gameObject.AddComponent<CoroutineAnimator>() : NoAnimator.Instance;

            var catcher = Ui.Img(Root, "Tap", ProceduralArt.White);
            catcher.color = Color.clear; catcher.raycastTarget = true;
            Ui.Bleed(catcher.rectTransform);                 // the side room of a tablet takes the tap too
            catcher.gameObject.AddComponent<Tap>().Click = OnTap;

            if (page.Title != null) { new TitleCard(Root, page.Title, anim); return; }

            if (page.Choice?.PanelLabel != null) BuildChoicePanel();
            else if (page.Picker != null) BuildPicker();
            else if (page.Plate != null) BuildStage();

            if (page.Info != null) new InfoCloud(Root, page.Info, Guard, anim);
            if (page.Stat != null) new StatBanner(Root, page.Stat, anim);
        }

        // ------------------------------------------------------------ lifetime
        public void Dispose()
        {
            picker?.Dispose();
            if (Root) UnityEngine.Object.Destroy(Root.gameObject);
        }

        /// The outgoing screen stops reacting and animating.
        public void Freeze()
        {
            Group.interactable = false; Group.blocksRaycasts = false;
            anim.Stop();
        }

        /// Run by the owner: the plate sinks and fades, the rest fades with it; then the view disposes itself.
        public IEnumerator Outro()
        {
            Freeze();
            const float duration = .24f;
            for (var t = 0f; t < duration && Root; t += Time.unscaledDeltaTime)
            {
                var e = Easing.InQuad(t / duration);
                plateAnimator?.PoseLeaving(e);
                Group.alpha = 1 - e;
                yield return null;
            }
            Dispose();
        }

        // ------------------------------------------------------------ input
        bool Typing => typed != null && typed.Typing;

        void OnTap()
        {
            if (Typing) { typed.Finish(); return; }
            if (page.Choice != null || page.Picker != null) return;      // a choice must be made
            next();
        }

        /// While the text is being typed out, any tap on a button shows the rest instead.
        Action Guard(Action action) => () => { if (Typing) typed.Finish(); else action(); };

        // ------------------------------------------------------------ plate (+ options, timer, balance)
        void BuildStage()
        {
            var choice = page.Choice;
            plate = new PlateView(Root, page.Plate);
            stack = choice != null ? new OptionStack(Root, choice.Items, Metrics.PlateWidth) : null;

            var floor = Metrics.OptionsLine + Viewport.ExtraBottom;
            var topLine = PlateLayout.TopLine(Viewport.Height);
            var bottom = PlateLayout.Bottom(topLine, plate.Height, floor, stack?.Height ?? 0);
            plate.PlaceBottom(bottom);
            plateAnimator = new PlateAnimator(plate);

            if (stack != null)
            {
                var shift = choice.Mirror ? -Metrics.OptionsShift : Metrics.OptionsShift;
                stack.PlaceUnderPlate(Metrics.Side + shift, Metrics.Side - shift, floor);
                stack.SetClick(Guard, Choose);
            }
            if (choice != null && choice.Seconds > 0)
                anim.Play(new TimerDial(plate.Root).Run(choice.Seconds, () => decided, OnTimeout));
            if (choice != null && choice.Coins >= 0) BalancePill.Corner(Root, choice.Coins);

            StartAppearing(topLine, floor, bottom);
        }

        void StartAppearing(float topLine, float floor, float bottom)
        {
            if (!anim.Enabled) return;
            var spec = page.Plate;
            if (!spec.Instant) { plateAnimator.PoseHidden(); anim.Play(plateAnimator.Intro(introDelay)); }
            else
            {
                // The choices appear on the plate that was just read: if they push it up, it slides there.
                var was = PlateLayout.Bottom(topLine, plate.Height, floor, 0);
                if (!Mathf.Approximately(was, bottom))
                    anim.Play(Tween.To(.35f, e => plate.Root.anchoredPosition = new Vector2(0, Mathf.Lerp(was, bottom, e))));
            }

            typed = new TypedText(plate.Body, plate.FullText, anim);
            if (stack != null) { stack.HideForEntrance(); typed.Finished += () => stack.CascadeIn(anim); }
            if (spec.Instant) typed.ShowAtOnce(); else typed.Start(introDelay);
        }

        // ------------------------------------------------------------ choosing
        void Choose(OptionButton button)
        {
            if (decided) return;
            if (!page.Choice.Free && button.Data.Cost > 0 && !bank.TrySpend(button.Data.Cost))
            {
                anim.Play(Effects.Shake(button.Root));
                return;
            }
            decided = true;
            ChosenIndex = button.Data.Index;
            stack.Decide(button, anim);
            anim.Play(Tween.After(.15f, next));
        }

        /// Time is up: nothing is chosen, every option dims, and the story moves on.
        void OnTimeout()
        {
            decided = true;
            stack.DimAll();
            next();
        }

        // ------------------------------------------------------------ bottom blocks
        void BuildChoicePanel()
        {
            var panel = new ChoicePanel(Root, page.Choice, anim);
            panel.Panel.CollapseButton.Click = Guard(panel.Panel.Toggle);
            stack = panel.Stack;
            stack.SetClick(Guard, Choose);
        }

        void BuildPicker()
        {
            picker = new PickerPanel(Root, page.Picker, bank, Guard, anim);
            picker.Confirmed += _ => { decided = true; anim.Play(Tween.After(.15f, next)); };
        }
    }
}
