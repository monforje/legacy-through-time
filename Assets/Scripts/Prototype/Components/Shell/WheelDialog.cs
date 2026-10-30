using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The "Колесо фортуны" popup (templates/shell/wheel.html), laid out by the numbers measured in the mockup: the ten
    /// prizes with their chances on top, the disc of ten sectors (WheelArt) with the prize icons on them, the light on the
    /// sector at the top, the hub with a star, the spins and two buttons. A spin turns the disc and stops on a prize picked
    /// by the weights (`Wheel.Pick`): the lit sector shows what falls out, the prize is paid out, sparks fly.
    sealed class WheelDialog
    {
        const float IconRadius = 85, IconSize = 42;

        readonly ShellDialog dialog;
        readonly IAnimator anim;
        readonly RectTransform disc;
        readonly ShellButton spin;
        readonly Text spins, spinCount;
        readonly Image spinIcon;
        readonly System.Random random = new();
        float angle;
        bool spinning;
        int turns;

        public bool IsOpen => dialog.IsOpen;
        public void Close() => dialog.Close();

        public WheelDialog(Transform parent, IAnimator anim, Action closed, Action buy)
        {
            this.anim = anim;
            dialog = new ShellDialog(parent, "Колесо фортуны", anim, closed);
            var c = MenuUi.List(dialog.Content, 0, 0, 0);                  // the content scrolls when the screen is short
            MenuUi.SetHeight(c, 616);
            // the scroll reaches 8 beyond the content on both sides (kit.css `.scroll`), so that the halo of the wheel is not cut at its edge
            var viewport = (RectTransform)c.parent;
            viewport.offsetMin = new Vector2(-Kit.Space2, viewport.offsetMin.y); viewport.offsetMax = new Vector2(Kit.Space2, viewport.offsetMax.y);
            c.offsetMin = new Vector2(Kit.Space2, c.offsetMin.y); c.offsetMax = new Vector2(-Kit.Space2, c.offsetMax.y);
            Prizes(c);

            // the wheel: the halo, the disc with the icons, the light, the hub; its centre is 298 down and in the middle
            var holder = Ui.Rect(c, "Wheel");
            Ui.Box(holder, A.TopCenter, A.TopCenter, new Vector2(0, -(298 - WheelArt.GlowSize / 2)), new Vector2(WheelArt.GlowSize, WheelArt.GlowSize));
            Layer(holder, "Halo", WheelArt.Glow, WheelArt.GlowSize);
            var discImage = Layer(holder, "Disc", WheelArt.Disc, WheelArt.DiscSize);
            disc = discImage.rectTransform;
            for (var i = 0; i < Wheel.Sectors; i++)
            {
                var a = i * Wheel.SectorDegrees * Mathf.Deg2Rad;
                var p = Wheel.Prizes[i];
                var icon = Kit.Icon(disc, "Prize " + p.Id, Kit.Pic(p.Pic), IconSize, A.Center, new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * IconRadius);
                icon.raycastTarget = false;
                icon.rectTransform.localRotation = Quaternion.Euler(0, 0, -i * Wheel.SectorDegrees);
            }
            Layer(holder, "Light", WheelArt.Light, WheelArt.LightSize);
            Layer(holder, "Hub", WheelArt.Hub, WheelArt.HubSize);
            Kit.Icon(holder, "Star", Kit.Pic("star"), 36).raycastTarget = false;

            // the spins: the square "+", the wheel and the number
            var plus = Parts.IconButton(c, "plus", buy, Kit.Touch, circle: false, accent: true);
            Ui.Box(plus.Root, A.TopLeft, A.TopLeft, new Vector2(0, -444), new Vector2(Kit.Touch, Kit.Touch));
            Kit.Icon(c, "Spins icon", Kit.Pic("wheel"), 22, A.TopLeft, new Vector2(52, -455)).raycastTarget = false;
            spins = Ui.Txt(c, "Spins", "", Kit.F800, Kit.FsHead, Kit.Text, TextAnchor.MiddleLeft);
            Ui.Box(spins.rectTransform, A.TopLeft, A.TopLeft, new Vector2(82, -454), new Vector2(40, 24));

            // "Вращать колесо": the label on the left, the wheel and the number on the right
            spin = Parts.Button(c, "Вращать колесо", ButtonStyle.Primary, Spin, 52);
            Ui.TopRow(spin.Root, 0, 0, 500, 52);
            Split(spin, 83);
            spinIcon = Kit.Icon(spin.Root, "Tail icon", Kit.Pic("wheel"), 22, A.MidRight, Vector2.zero);
            spinIcon.raycastTarget = false;
            spinCount = Ui.Txt(spin.Root, "Tail", "", Kit.F800, Kit.FsBody, Kit.AccentInk, TextAnchor.MiddleRight);
            Ui.Box(spinCount.rectTransform, A.MidRight, A.MidRight, new Vector2(-16, 0), new Vector2(40, 22));

            // "Удвоить шансы": the same row, secondary
            var boost = Parts.Button(c, "Удвоить шансы на редкие призы", ButtonStyle.Neutral,
                                     () => ShellToast.Show(dialog.Root, "Скоро: удвоение шансов за 150 алмазов", anim, 2.5f, 52), 52);
            Ui.TopRow(boost.Root, 0, 0, 564, 52);
            Split(boost, 83);
            var cost = Ui.Txt(boost.Root, "Cost", "150", Kit.F800, Kit.FsBody, Kit.Text, TextAnchor.MiddleRight);
            Ui.Box(cost.rectTransform, A.MidRight, A.MidRight, new Vector2(-16, 0), new Vector2(40, 22));
            var gemX = -(16 + cost.preferredWidth + 8);
            Kit.Icon(boost.Root, "Gem", Kit.Pic("gem"), 24, A.MidRight, new Vector2(gemX, 0)).raycastTarget = false;
            Refresh();
        }

        /// `.btn-split`: the label on the left in its own width (it wraps), the tail on the right.
        static void Split(ShellButton button, float labelRight)
        {
            button.Label.alignment = TextAnchor.MiddleLeft;
            button.Label.horizontalOverflow = HorizontalWrapMode.Wrap;
            Ui.Stretch(button.Label.rectTransform, 16, 0, labelRight, 0);              // stops before the tail, wraps
        }

        static Image Layer(Transform holder, string name, Sprite sprite, float size)
        {
            var img = Ui.Img(holder, name, sprite);
            img.raycastTarget = false;
            Ui.Box(img.rectTransform, A.Center, A.Center, Vector2.zero, new Vector2(size, size));
            return img;
        }

        // ------------------------------------------------------------------ the prizes on top: .prizes
        void Prizes(RectTransform c)
        {
            for (var i = 0; i < Wheel.Prizes.Length; i++)
            {
                var p = Wheel.Prizes[i];
                var top = 12 + (i / 5) * 74;
                // a cell is a fifth of the width; what is in it is centred
                var cell = Ui.Rect(c, "Prize " + p.Id);
                cell.anchorMin = new Vector2((i % 5) / 5f, 1); cell.anchorMax = new Vector2((i % 5 + 1) / 5f, 1);
                cell.pivot = new Vector2(.5f, 1);
                cell.anchoredPosition = new Vector2(0, -top); cell.sizeDelta = new Vector2(-Kit.Space1, 66);
                var pic = Kit.Icon(cell, "Icon", Kit.Pic(p.Pic), 46, A.TopCenter, new Vector2(0, 1));
                pic.raycastTarget = false;
                if (p.Amount > 0)
                {
                    var badge = Kit.Round(cell, "Badge", Kit.Accent, 9);
                    var amount = Ui.Txt(badge.rectTransform, "Amount", p.Amount.ToString(), Kit.F800, Kit.FsMicro, Kit.AccentInk, TextAnchor.MiddleCenter);
                    var w = Mathf.Max(20, amount.preferredWidth + 10);
                    Ui.Box(badge.rectTransform, A.TopCenter, A.TopCenter, new Vector2(28 - w / 2, 6), new Vector2(w, 18));
                    Ui.Stretch(amount.rectTransform);
                }
                var chance = p.Weight >= 10 ? Mathf.Floor((float)p.Weight).ToString() : p.Weight.ToString("0.##").Replace('.', ',');
                var pct = Ui.Txt(cell, "Chance", chance + "%", Kit.F800, Kit.FsSmall, Kit.Text, TextAnchor.MiddleCenter);
                Ui.Box(pct.rectTransform, A.TopCenter, A.TopCenter, new Vector2(0, -48), new Vector2(58, 18));
            }
        }

        // ------------------------------------------------------------------ spinning
        void Refresh()
        {
            var left = ShellPurse.Spins;
            spins.text = left.ToString();
            spinCount.text = left.ToString();
            Ui.Box(spinIcon.rectTransform, A.MidRight, A.MidRight, new Vector2(-(16 + spinCount.preferredWidth + 8), 0), new Vector2(22, 22));
            spin.SetEnabled(left > 0 && !spinning);
        }

        void Spin()
        {
            if (spinning || !ShellPurse.TrySpin()) { if (!spinning) ShellToast.Show(dialog.Root, "Попытки закончились", anim, 2.5f, 52); return; }
            spinning = true; Refresh();
            var sector = Wheel.Pick(random.NextDouble());
            turns += 4;
            var from = angle; var to = Wheel.Angle(sector, turns);
            if (!anim.Enabled) { Land(sector, to); return; }
            anim.Play(Spin(from, to, sector));
        }

        IEnumerator Spin(float from, float to, int sector)
        {
            for (var t = 0f; t < Kit.Dur.Spin; t += Time.unscaledDeltaTime)
            {
                if (!disc) yield break;
                var e = 1f - Mathf.Pow(1f - t / Kit.Dur.Spin, 4f);          // fast start, long settle
                disc.localRotation = Quaternion.Euler(0, 0, -Mathf.Lerp(from, to, e));
                yield return null;
            }
            Land(sector, to);
        }

        void Land(int sector, float to)
        {
            angle = to;
            if (disc) disc.localRotation = Quaternion.Euler(0, 0, -to);
            spinning = false;
            var prize = Wheel.Prizes[sector];
            var text = ShellPurse.Pay(prize);
            if (dialog.IsOpen)
            {
                ShellToast.Show(dialog.Root, "Выпало: " + text, anim, 2.5f, 52);
                var lit = (Vector2)dialog.Root.InverseTransformPoint(disc.position) + dialog.Root.rect.size * dialog.Root.pivot + new Vector2(0, IconRadius);
                ShellSparks.Burst(dialog.Root, lit, anim, 6);
                Refresh();
            }
        }
    }
}
