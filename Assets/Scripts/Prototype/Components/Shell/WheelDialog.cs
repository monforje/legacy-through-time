using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// The "Колесо фортуны" popup (templates/shell/wheel.html): the prizes with their chances on top, the disc of ten
    /// sectors in two tones with the prize icons on them, the lit sector at the top, the hub with a star, the spins and
    /// two buttons. A spin turns the disc and stops on a prize picked by the weights (`Wheel.Pick`): the lit sector
    /// shows what falls out, the prize is paid out, sparks fly.
    sealed class WheelDialog
    {
        const float Disc = 252, Ring = 6, IconRadius = 92;

        readonly ShellDialog dialog;
        readonly IAnimator anim;
        readonly RectTransform disc;
        readonly ShellButton spin;
        readonly Text spins;
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
            var c = dialog.Content;
            Prizes(c);

            // the wheel
            var holder = Ui.Rect(c, "Wheel");
            Ui.Box(holder, A.TopCenter, A.TopCenter, new Vector2(0, -(112 + 16)), new Vector2(Disc + 2 * Ring + 8, Disc + 2 * Ring + 8));
            var outer = Ui.Img(holder, "Ring", ProceduralArt.Disc);
            outer.color = Kit.Line.WithAlpha(.55f); outer.raycastTarget = false;
            Ui.Stretch(outer.rectTransform);
            var rim = Ui.Img(holder, "Rim", ProceduralArt.Disc);
            rim.color = Kit.Surface2; rim.raycastTarget = false;
            Ui.Stretch(rim.rectTransform, 4, 4, 4, 4);
            disc = Ui.Rect(holder, "Disc");
            Ui.Box(disc, A.Center, A.Center, Vector2.zero, new Vector2(Disc, Disc));
            Sectors(disc);
            var light = Ui.Img(holder, "Lit", ProceduralArt.Disc);
            light.type = Image.Type.Filled; light.fillMethod = Image.FillMethod.Radial360; light.fillOrigin = (int)Image.Origin360.Top;
            light.fillClockwise = true; light.fillAmount = 1f / Wheel.Sectors;
            light.color = Kit.Text.WithAlpha(.2f); light.raycastTarget = false;
            Ui.Box(light.rectTransform, A.Center, A.Center, Vector2.zero, new Vector2(Disc, Disc));
            light.rectTransform.localRotation = Quaternion.Euler(0, 0, Wheel.SectorDegrees / 2);      // from -18 to +18 degrees around the top
            var hubRing = Ui.Img(holder, "HubRing", ProceduralArt.Disc);
            hubRing.color = Kit.Accent; hubRing.raycastTarget = false;
            Ui.Box(hubRing.rectTransform, A.Center, A.Center, Vector2.zero, new Vector2(74, 74));
            var hub = Ui.Img(hubRing.rectTransform, "Hub", ProceduralArt.Disc);
            hub.color = Kit.Surface; hub.raycastTarget = false;
            Ui.Box(hub.rectTransform, A.Center, A.Center, Vector2.zero, new Vector2(64, 64));
            Kit.Icon(hub.rectTransform, "Star", Kit.Pic("star"), 36).raycastTarget = false;

            // spins and buttons
            var plus = Parts.IconButton(c, "plus", buy, Kit.Touch, circle: false, accent: true);
            Ui.Box(plus.Root, A.BottomLeft, A.BottomLeft, new Vector2(0, 112), new Vector2(Kit.Touch, Kit.Touch));
            var icon = Kit.Icon(c, "Spins icon", Kit.Pic("wheel"), 26, A.BottomLeft, new Vector2(Kit.Touch + 20, 112 + 22));
            icon.raycastTarget = false;
            spins = Ui.Txt(c, "Spins", "", Art.Sub, Kit.FsHead, Kit.Text, TextAnchor.MiddleLeft);
            Ui.Box(spins.rectTransform, A.BottomLeft, A.BottomLeft, new Vector2(Kit.Touch + 40, 112), new Vector2(40, Kit.Touch));

            spin = Parts.Button(c, "Вращать колесо", ButtonStyle.Primary, Spin, 52);
            Ui.BottomRow(spin.Root, 0, 0, 52 + Kit.Space2, 52);
            spin.Label.alignment = TextAnchor.MiddleLeft;
            Ui.Stretch(spin.Label.rectTransform, 16, 0, 56, 0);
            var tail = Kit.Icon(spin.Root, "Spin icon", Kit.Pic("wheel"), 26, A.MidRight, new Vector2(-16, 0));
            tail.raycastTarget = false;

            var boost = Parts.Button(c, "Удвоить шансы на редкие призы", ButtonStyle.Neutral,
                                     () => ShellToast.Show(dialog.Root, "Скоро: удвоение шансов за 150 алмазов", anim), 52);
            Ui.BottomRow(boost.Root, 0, 0, 0, 52);
            boost.Label.alignment = TextAnchor.MiddleLeft;
            Ui.Stretch(boost.Label.rectTransform, 16, 0, 78, 0);
            Kit.Icon(boost.Root, "Gem", Kit.Pic("gem"), 26, A.MidRight, new Vector2(-50, 0)).raycastTarget = false;
            var cost = Ui.Txt(boost.Root, "Cost", "150", Art.Sub, Kit.FsHead, Kit.Text, TextAnchor.MiddleRight);
            Ui.Box(cost.rectTransform, A.MidRight, A.MidRight, new Vector2(-12, 0), new Vector2(40, 30));
            Refresh();
        }

        // ------------------------------------------------------------------ the prizes on top
        void Prizes(RectTransform c)
        {
            var row = Ui.Rect(c, "Prizes");
            Ui.TopRow(row, 0, 0, 0, 112);
            var w = (Metrics.RefWidth - 2 * Kit.Side - 2 * Kit.Space3) / 5f;
            for (var i = 0; i < Wheel.Prizes.Length; i++)
            {
                var p = Wheel.Prizes[i];
                var cell = Ui.Rect(row, "Prize " + p.Id);
                Ui.Box(cell, A.TopLeft, A.TopLeft, new Vector2((i % 5) * w, -(i / 5) * 56), new Vector2(w, 54));
                var pic = Kit.Icon(cell, "Icon", Kit.Pic(p.Pic), 40, A.TopCenter, new Vector2(0, -2));
                pic.raycastTarget = false;
                if (p.Amount > 0)
                {
                    var badge = Kit.Round(cell, "Badge", Kit.Accent, 9);
                    Ui.Box(badge.rectTransform, A.TopCenter, A.TopCenter, new Vector2(16, 2), new Vector2(p.Amount > 99 ? 32 : 24, 18));
                    var bt = Ui.Txt(badge.rectTransform, "Text", p.Amount.ToString(), Art.Sub, Kit.FsMicro, Kit.AccentInk, TextAnchor.MiddleCenter);
                    Ui.Stretch(bt.rectTransform);
                }
                var pct = Ui.Txt(cell, "Chance", p.Weight.ToString("0.##").Replace('.', ',') + "%", Art.Sub, Kit.FsSmall, Kit.Text, TextAnchor.LowerCenter);
                Ui.BottomRow(pct.rectTransform, 0, 0, 0, 16);
            }
        }

        // ------------------------------------------------------------------ the disc
        void Sectors(RectTransform d)
        {
            var n = Wheel.Sectors;
            // base disc and the wedges of the second tone
            var baseDisc = Ui.Img(d, "Base", ProceduralArt.Disc);
            baseDisc.color = Kit.Surface; baseDisc.raycastTarget = false;
            Ui.Stretch(baseDisc.rectTransform);
            for (var i = 0; i < n; i += 2)
            {
                var wedge = Ui.Img(d, "Wedge " + i, ProceduralArt.Disc);
                wedge.type = Image.Type.Filled; wedge.fillMethod = Image.FillMethod.Radial360; wedge.fillOrigin = (int)Image.Origin360.Top;
                wedge.fillClockwise = true; wedge.fillAmount = 1f / n;
                wedge.color = Kit.Surface2; wedge.raycastTarget = false;
                Ui.Stretch(wedge.rectTransform);
                wedge.rectTransform.localRotation = Quaternion.Euler(0, 0, -(i * Wheel.SectorDegrees - Wheel.SectorDegrees / 2));
            }
            for (var i = 0; i < n; i++)
            {
                var a = i * Wheel.SectorDegrees * Mathf.Deg2Rad;
                var line = Ui.Img(d, "Divider " + i, ProceduralArt.White);
                line.color = Kit.Line.WithAlpha(.6f); line.raycastTarget = false;
                Ui.Box(line.rectTransform, A.Center, A.BottomCenter, Vector2.zero, new Vector2(1, Disc / 2));
                line.rectTransform.localRotation = Quaternion.Euler(0, 0, -(i * Wheel.SectorDegrees - Wheel.SectorDegrees / 2));
                var p = Wheel.Prizes[i];
                var icon = Kit.Icon(d, "Prize " + p.Id, Kit.Pic(p.Pic), 42, A.Center, new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * IconRadius);
                icon.raycastTarget = false;
                icon.rectTransform.localRotation = Quaternion.Euler(0, 0, -i * Wheel.SectorDegrees);
            }
        }

        // ------------------------------------------------------------------ spinning
        void Refresh()
        {
            var left = ShellPurse.Spins;
            spins.text = left.ToString();
            spin.SetEnabled(left > 0 && !spinning);
        }

        void Spin()
        {
            if (spinning || !ShellPurse.TrySpin()) { if (!spinning) ShellToast.Show(dialog.Root, "Попытки закончились", anim); return; }
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
                ShellToast.Show(dialog.Root, "Выпало: " + text, anim);
                var at = new Vector2(dialog.Root.rect.width / 2, dialog.Root.rect.height / 2) + new Vector2(0, 40);
                var lit = (Vector2)dialog.Root.InverseTransformPoint(disc.position) + dialog.Root.rect.size * dialog.Root.pivot + new Vector2(0, IconRadius);
                ShellSparks.Burst(dialog.Root, lit.sqrMagnitude > 0 ? lit : at, anim, 6);
                Refresh();
            }
        }
    }
}
