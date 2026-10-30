using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// Scene scaffolding shared by the screens demo and the story player: camera, input, a canvas that is at least
    /// 360x640 units, and the stand-in for the painted background.
    static class Stage
    {
        public static void Setup()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Application.targetFrameRate = 60;
            EnsureCamera();
            EnsureEventSystem();
            Viewport.Measure();
        }

        static void EnsureCamera()
        {
            if (Object.FindAnyObjectByType<Camera>()) return;
            var camera = new GameObject("Camera", typeof(Camera)).GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Theme.Backdrop;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>()) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        public static RectTransform BuildCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;      // the scale is Viewport.Scale: 360x640 always fits, a unit is at most 1.35 dp
            scaler.scaleFactor = Viewport.Scale;
            go.AddComponent<ScaleKeeper>().Scaler = scaler;
            return (RectTransform)go.transform;
        }

        /// Keeps the canvas scale in step with the screen (rotation, a resized window).
        sealed class ScaleKeeper : MonoBehaviour
        {
            public CanvasScaler Scaler;
            int w, h;

            void Update()
            {
                if (Screen.width == w && Screen.height == h) return;
                w = Screen.width; h = Screen.height;
                Viewport.Measure();
                if (Scaler) Scaler.scaleFactor = Viewport.Scale;
            }
        }

        /// Flat colour (the stand-in, tinted per location), two picture layers for the painted background (the new one
        /// fades in over the old) and dark edges.
        public static BackdropTint BuildBackground(RectTransform canvas)
        {
            var colour = Ui.Img(canvas, "Background", ProceduralArt.White);
            colour.color = Theme.Backdrop; Ui.Stretch(colour.rectTransform);
            var back = Ui.Img(canvas, "PictureBack", null); back.enabled = false;
            var front = Ui.Img(canvas, "PictureFront", null); front.enabled = false;
            Ui.Stretch(Ui.Img(canvas, "Vignette", ProceduralArt.Vignette()).rectTransform);
            return new BackdropTint(colour, back, front);
        }
    }
}
