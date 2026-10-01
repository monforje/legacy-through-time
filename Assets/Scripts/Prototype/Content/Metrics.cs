namespace LegacyThroughTime.Prototype
{
    /// Numbers of the layout, in canvas units = CSS px of the 360x780 reference screen (docs/brainstorm/templates/heritage.css).
    /// No Unity types here: this folder (Content/) is plain C# and is unit-tested outside Unity (`task test:reader`).
    static class Metrics
    {
        public const float RefWidth = 360, RefHeight = 780;
        public const float PlateWidth = RefWidth - 2 * Side;

        // Lines of the reading screen
        public const float Side = 20;               // margins of plates and panels
        public const float PlateTop = 330;          // top edge of the plate, from the bottom of the reference screen
        public const float OptionsLine = 94;        // bottom edge of the option list (and the lowest edge of a plate)
        public const float OptionsShift = 12;       // options are shifted sideways against the plate
        public const float OptionsOverlap = 16;     // and overlap it by this much
        public const float OptionGap = 10;
        public const float InsetTop = 32, InsetBottom = 24;   // status bar and gesture bar of the reference frame

        // Type scale (px)
        public const int FsTitle = 28, FsValue = 22, FsBody = 19, FsAction = 17, FsLabel = 15;

        // Blocks
        public const float PanelTextWidth = PlateWidth - 32;  // inside a bottom panel
        public const float PanelPaddingTop = 38, PanelPaddingBottom = 18;
        public const float PickerHeight = 184;
        public const float CollapseExtra = 40;       // how far past its own height a collapsed panel sinks
    }
}
