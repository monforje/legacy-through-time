namespace LegacyThroughTime.Prototype
{
    enum PlateStyle
    {
        Narr,        // narration: mint plate
        Fact,        // a caption of the scene ("Место"), label with a gem
        Hint,        // rose plate, label "Подсказка"
        SpeechLeft,  // the heroine: label on the left, tail on the right
        SpeechRight, // everybody else: mirror
        Thought,     // dashed "drop" cloud with two bubbles
        ThoughtPuff, // scalloped cloud
    }

    sealed class Option
    {
        public readonly string Text;
        public readonly int Cost;
        /// Index in the story machine's list of choices (not necessarily the position on screen).
        public readonly int Index;
        public Option(string text, int cost = 0, int index = 0) { Text = text; Cost = cost; Index = index; }
    }

    sealed class TitleSpec { public string Name, Scene; }

    sealed class PlateSpec
    {
        public PlateStyle Style; public string Label, Text;
        /// The text was already read (the options of a choice appear next to the plate): show it at once.
        public bool Instant;
    }

    sealed class ChoiceSpec
    {
        public Option[] Items;
        public bool Mirror;                 // options shifted to the left (the plate has its label on the right)
        public string PanelLabel;           // set: the options sit in their own panel with this label, no plate
        public int Seconds;                 // > 0: timed choice, a timer over the plate
        public bool Free;                   // payment is done by the story (ink variable); the screen only reports the choice
        public int Coins = -1;              // >= 0: show the balance chip with this many coins
    }

    sealed class PickerSpec
    {
        public string Label;
        public Option[] Values;
        public bool ShowBalance;
    }

    /// One screen. Any combination of blocks; the view builds what is present.
    sealed class Page
    {
        public string Id;
        public TitleSpec Title;
        public PlateSpec Plate;
        public ChoiceSpec Choice;
        public PickerSpec Picker;
        public string Stat;                 // banner of a changed stat, at the top
        public string Info;                 // explanation cloud, at the top
    }
}
