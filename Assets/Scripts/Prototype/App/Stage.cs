using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LegacyThroughTime.Prototype
{
    /// Scene scaffolding shared by the screens demo and the story player: camera, input, a canvas that is 360
    /// units wide on every aspect ratio, and the stand-in for the painted background.
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
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(Metrics.RefWidth, Metrics.RefHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0;          // the layout is 360 wide on every aspect ratio
            return (RectTransform)go.transform;
        }

        /// Flat colour (tinted per location in the story) and dark edges; returns the colour layer.
        public static Image BuildBackground(RectTransform canvas)
        {
            var colour = Ui.Img(canvas, "Background", ProceduralArt.White);
            colour.color = Theme.Backdrop; Ui.Stretch(colour.rectTransform);
            Ui.Stretch(Ui.Img(canvas, "Vignette", ProceduralArt.Vignette()).rectTransform);
            return colour;
        }
    }
}
