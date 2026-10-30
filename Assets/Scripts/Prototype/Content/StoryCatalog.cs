using System.Collections.Generic;

namespace LegacyThroughTime.Prototype
{
    /// One chapter (episode) of a story. A chapter without a path is announced but not written yet.
    sealed class ChapterEntry
    {
        public int Number;
        public string Title;
        public string Path;        // compiled chapter in StreamingAssets (`task ink`); null: "скоро"
        public string SaveId;      // the save slot of the chapter
        public string Night;       // the night of the heroine chat after it (chat server, content/stories/*/chat); null: none
        public string Heroine;     // who speaks in that night
        public bool Soon => Path == null;
    }

    /// One story of the menu: a card in the carousel; its chapters open when it is tapped.
    sealed class StoryEntry
    {
        public string Id;
        public string Title;
        public string Episode;     // the scene line of the title card
        public string Blurb;
        public string Cover;       // background id shown on the card (Resources/Art/Backgrounds/<id>.png)
        public bool Soon;          // a story announced, not playable: a dark card with a lock
        public string Genre;       // the line under the title on the cards ("История · 1533")
        public bool Recommended;   // the card with the "Рекомендуем" flag, where the deck starts
        public string Hero;        // the main hero: who the reader talks to in the story's chat
        public string Opening;     // the hero's first line in an empty chat (the author's; the chat server takes over after it)
        public IReadOnlyList<ChapterEntry> Chapters = new ChapterEntry[0];

        /// The first chapter: the one a story opens with.
        public ChapterEntry First => Chapters.Count > 0 ? Chapters[0] : null;
        public string Path => First?.Path;
    }

    /// The stories of the menu, in the order of the carousel. Plain data: tested by `task test:reader`.
    static class StoryCatalog
    {
        public static readonly IReadOnlyList<StoryEntry> All = new[]
        {
            new StoryEntry
            {
                Id = "golden-cage", Title = "Золотая клетка", Episode = "Эпизод 1", Genre = "История · 1533", Recommended = true,
                Blurb = "Ногайская Орда, 1533 год. Юную Сююмбике везут в Казань — к хану, которого она никогда не видела.",
                Cover = "title", Hero = "Сююмбике", Opening = "Привет. Я рада, что ты пришёл. О чём поговорим?",
                Chapters = new[]
                {
                    // the save slot keeps the id of the story: the saves made before there were chapters still open
                    new ChapterEntry { Number = 1, Title = "Перед дорогой", Path = "stories/golden-cage/episode-01.json", SaveId = "golden-cage",
                                         Night = "ep01", Heroine = "Сююмбике" },
                    new ChapterEntry { Number = 2, Title = "Скоро", SaveId = "golden-cage-2" },
                    new ChapterEntry { Number = 3, Title = "Скоро", SaveId = "golden-cage-3" },
                    new ChapterEntry { Number = 4, Title = "Скоро", SaveId = "golden-cage-4" },
                },
            },
            new StoryEntry
            {
                Id = "zilant", Title = "Зилант", Episode = "Скоро", Genre = "Легенда · Казань",
                Blurb = "Легенда о крылатом змее, хранителе холма, на котором встала Казань.", Cover = "kazan_panorama", Soon = true,
            },
            new StoryEntry
            {
                Id = "shurale", Title = "Шурале", Episode = "Скоро", Genre = "Фэнтези · Легенда",
                Blurb = "Лесной дух щекочет путников до смерти. Но однажды он встретил дровосека.", Cover = "road_evening", Soon = true,
            },
        };

        public static StoryEntry Find(string id)
        {
            foreach (var s in All) if (s.Id == id) return s;
            return null;
        }
    }
}
