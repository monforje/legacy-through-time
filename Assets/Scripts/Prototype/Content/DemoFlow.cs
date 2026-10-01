using System.Collections.Generic;

namespace LegacyThroughTime.Prototype
{
    /// The fixed screens of the demo walk-through. Port of SCREENS and FLOW_ORDER from session-screens-heritage.html.
    static class DemoFlow
    {
        const string KalfakQuestion = "«Тогда мне казалось, что женщины вышивают цветы. Теперь я знаю: они вышивают память.»";
        const string Heroine = "Сююмбике";

        static Page Say(string id, PlateStyle style, string label, string text, ChoiceSpec choice = null) =>
            new() { Id = id, Plate = new PlateSpec { Style = style, Label = label, Text = text }, Choice = choice };

        static Option[] Options(params string[] texts)
        {
            var list = new Option[texts.Length];
            for (var i = 0; i < list.Length; i++) list[i] = new Option(texts[i], 0, i);
            return list;
        }

        static readonly Dictionary<string, Page> all = new()
        {
            ["0"] = new Page { Id = "0", Title = new TitleSpec { Name = "Золотая клетка", Scene = "Сцена 1: Перед дорогой" } },
            ["1"] = Say("1", PlateStyle.Narr, "…", "Рассвет. Низкое солнце цепляется за край степи. Ветер перебирает сухую траву."),
            ["2"] = Say("2", PlateStyle.Fact, "Факт", "Ногайская Орда, 1533 год. Сююмбике ещё не в Казани."),
            ["3"] = Say("3", PlateStyle.Hint, "Подсказка", "Открыта информация: «Диван». В будущем вы сможете распознавать, когда слова правителя действительно его решение, а когда за ними стоит совет знати."),
            ["4"] = Say("4", PlateStyle.SpeechLeft, Heroine, "А если я не буду?"),
            ["5"] = Say("5", PlateStyle.SpeechRight, "Сафия", "Юсуф-бий велел, чтобы вы были готовы до полудня."),
            ["6"] = Say("6", PlateStyle.SpeechLeft, Heroine, "«Он пахнет сухой травой. И почему-то домом.»"),
            ["6a"] = Say("6a", PlateStyle.Thought, Heroine, "Он пахнет сухой травой. И почему-то домом."),
            ["6b"] = Say("6b", PlateStyle.ThoughtPuff, Heroine, "Он пахнет сухой травой. И почему-то домом."),
            ["7"] = Say("7", PlateStyle.SpeechLeft, Heroine, KalfakQuestion, new ChoiceSpec
            {
                Items = new[] { new Option("(Надеть калфак.)", 0, 0), new Option("(Не надевать.)", 0, 1), new Option("(Спрятать калфак в сундук.)", 8, 2) },
            }),
            ["7a"] = Say("7a", PlateStyle.SpeechRight, "Сафия", "Тогда мне велено привести вас.", new ChoiceSpec
            {
                Mirror = true,
                Items = Options("(Подойти и заговорить первой.)", "(Сделать вид, что всё равно.)", "(Ответить колко.)"),
            }),
            ["8"] = Say("8", PlateStyle.SpeechLeft, Heroine, "«Сафия ждёт ответа.»", new ChoiceSpec
            {
                Seconds = 8,
                Items = Options("(Подойти и заговорить первой.)", "(Сделать вид, что всё равно.)", "(Ответить колко.)", "(Промолчать.)"),
            }),
            ["9"] = new Page
            {
                Id = "9",
                Picker = new PickerSpec
                {
                    Label = "Выбери, что сделать с калфаком.",
                    Values = new[] { new Option("Надеть калфак"), new Option("Не надевать"), new Option("Спрятать в сундук", 8) },
                },
            },
            ["9a"] = new Page
            {
                Id = "9a",
                Picker = new PickerSpec
                {
                    Label = "Выбери наряд в дорогу.", ShowBalance = true,
                    Values = new[] { new Option("Вышитый камзол", 8), new Option("Войлочный плащ"), new Option("Шёлковый халат", 14) },
                },
            },
            ["10"] = new Page
            {
                Id = "10", Stat = "+1 Память степи",
                Plate = new PlateSpec { Style = PlateStyle.SpeechLeft, Label = Heroine, Text = "«Оказывается, иногда тебе просто дают одежду и говорят, куда идти.»" },
            },
            ["11"] = new Page
            {
                Id = "11",
                Choice = new ChoiceSpec
                {
                    PanelLabel = "Сафия ждёт ответа…",
                    Items = Options("(Подойти и заговорить первой.)", "(Ответить колко.)", "(Сделать вид, что всё равно.)"),
                },
            },
            ["12"] = new Page
            {
                Id = "12", Info = "Калфак — традиционный женский головной убор татарок.",
                Plate = new PlateSpec { Style = PlateStyle.Narr, Label = "…", Text = "Она разворачивает ткань: вышитая рубаха, лёгкий камзол и калфак." },
            },
        };

        /// Order of the walk-through (FLOW_ORDER of the web prototype).
        public static readonly string[] Order = { "0", "1", "2", "3", "12", "5", "4", "6", "6a", "6b", "7", "10", "7a", "11", "9", "9a", "8" };

        public static readonly Page End = new() { Id = "end", Title = new TitleSpec { Name = "Конец фрагмента", Scene = "Тап — сначала" } };

        public static Page Get(string id) => all[id];

        public static List<Page> Pages()
        {
            var list = new List<Page>();
            foreach (var id in Order) list.Add(all[id]);
            list.Add(End);
            return list;
        }
    }
}
