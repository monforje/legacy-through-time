using UnityEngine;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// Story and scene title between two ornamented brackets over the dimmed painted title background (BackdropTint "title").
    sealed class TitleCard
    {
        const float TopShare = .24f, BottomShare = .56f;       // where the brackets stand, as a share of the screen height

        public TitleCard(Transform parent, TitleSpec spec, IAnimator anim)
        {
            var dim = Ui.Img(parent, "Dim", ProceduralArt.Gradient(Theme.TitleDimTop, Theme.TitleDimBottom));
            Ui.Stretch(dim.rectTransform);

            var box = Ui.Rect(parent, "Title");
            box.anchorMin = A.TopLeft; box.anchorMax = A.TopRight; box.pivot = A.TopCenter;
            box.offsetMin = new Vector2(Metrics.Side, -Viewport.Height * BottomShare);
            box.offsetMax = new Vector2(-Metrics.Side, -Viewport.Height * TopShare);
            var frame = Ui.Sliced(box, "Brackets", "title-brackets", fillCenter: false);
            Ui.Stretch(frame.rectTransform);

            // both ornaments hang from their line inside the box; the lower one is the upper one flipped
            var ornTop = Ui.Icon(box, "OrnTop", "title-orn", A.TopCenter, A.TopCenter, new Vector2(0, 2));
            var ornBottom = Ui.Icon(box, "OrnBottom", "title-orn", A.BottomCenter, A.TopCenter, new Vector2(0, -2));
            ornBottom.rectTransform.localScale = new Vector3(1, -1, 1);

            var name = Ui.Txt(box, "Name", spec.Name.ToUpperInvariant(), Art.Head, Metrics.FsTitle, Theme.Parchment, TextAnchor.MiddleCenter);
            Ui.Box(name.rectTransform, A.Center, A.Center, new Vector2(0, 28), new Vector2(Metrics.PlateWidth - 24, 44));
            var scene = Ui.Txt(box, "Scene", spec.Scene, Art.Sub, Metrics.FsValue, Theme.Parchment, TextAnchor.MiddleCenter);
            Ui.Box(scene.rectTransform, A.Center, A.Center, new Vector2(0, -31), new Vector2(Metrics.PlateWidth - 24, 36));
            foreach (var t in new[] { name, scene })
            {
                var shadow = t.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0, 0, 0, .4f); shadow.effectDistance = new Vector2(0, -2);
            }

            if (anim.Enabled) Animate(anim, dim, frame, ornTop, ornBottom, name, scene);
        }

        // Everything starts invisible before the first frame: a delayed tween that only sets its start value when it
        // begins would show the text at full strength and then blink it away. The brackets fade in place:
        // scaling a 9-slice would squash its ornamented corners.
        static void Animate(IAnimator anim, Image dim, Image frame, Image ornTop, Image ornBottom, Text name, Text scene)
        {
            dim.color = new Color(1, 1, 1, 0);
            var groups = new[] { Ui.Group(frame), Ui.Group(ornTop), Ui.Group(ornBottom) };
            foreach (var g in groups) g.alpha = 0;
            anim.Play(Tween.To(.5f, e => dim.color = new Color(1, 1, 1, e), Easing.Linear));
            anim.Play(Tween.To(.7f, e => { foreach (var g in groups) g.alpha = e; }, Easing.Linear, .15f));
            anim.Play(Effects.FadeUp(name, .4f));
            anim.Play(Effects.FadeUp(scene, .65f));
        }
    }
}
