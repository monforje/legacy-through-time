using System.Collections.Generic;

namespace LegacyThroughTime.Prototype
{
    /// One card of the story board in the main menu.
    sealed class StoryEntry
    {
        public string Id;          // also the name of the save slot
        public string Title;
        public string Episode;     // under the title, and the scene line of the title card
        public string Blurb;
        public string Cover;       // background id shown as the preview (Resources/Art/Backgrounds/<id>.png)
        public string Path;        // compiled story in StreamingAssets (`task ink`)
        public bool Soon;          // a placeholder card: shown, not playable
    }

    /// The stories of the main menu, in the order of the board. Plain data: tested by `task test:reader`.
    static class StoryCatalog
    {
        public static readonly IReadOnlyList<StoryEntry> All = new[]
        {
            new StoryEntry
            {
                Id = "golden-cage", Title = "Золотая клетка", Episode = "Эпизод 1",
                Blurb = "Ногайская Орда, 1533 год. Юную Сююмбике везут в Казань — к хану, которого она никогда не видела.",
                Cover = "title", Path = "stories/golden-cage/episode-01.json",
            },
            new StoryEntry
            {
                Id = "soon", Title = "Новые истории", Episode = "Скоро",
                Blurb = "Следующие главы наследия уже пишутся.", Soon = true,
            },
        };

        public static StoryEntry Find(string id)
        {
            foreach (var s in All) if (s.Id == id) return s;
            return null;
        }
    }
}
