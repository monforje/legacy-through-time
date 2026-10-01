# Prototype — экраны чтения в стиле «Наследие»

Весь UI чтения (облачко, варианты выбора, панели, баннеры, титул) строится кодом на uGUI. Ничего не лежит в сцене:
`StoryPlayer` сам стартует в любой сцене: загрузка → меню (доска историй) → история через движок `Narrative`; `SessionPrototype` — демо с
фиксированными экранами (его берут тесты). Дизайн и размеры: `docs/brainstorm/templates/heritage-ui-for-unity.md`.

## Устройство

```
Content/      чистый C# без Unity (тестируется в Docker: task test:reader)
  PageSpec     описание экрана: Page = Title | Plate | Choice | Picker | Stat | Info  (что показать)
  PlateSkin    как выглядит каждый стиль плашки (одна запись в таблице = один стиль)
  PlateLayout  где стоит плашка: верх на линии, текст растёт вниз, предел — зона вариантов, дальше вверх
  TextRules    неразрывные пробелы, КАПС -> предложение, баннер стата, тайминг печати по словам
  BeatMapper   бит истории -> экран (по соглашениям ink: Имя: текст, # thought, # system ...)
  Backdrops    тон фона по `# bg: id`        DemoFlow  экраны демо       Metrics  числа раскладки
  StoryCatalog карточки меню: id, название, эпизод, описание, обложка-превью, путь к истории
Core/         то, что нужно всем компонентам
  Theme Viewport Art ProceduralArt   цвета, размер холста (Expand: не меньше 360x780) и вырезы, спрайты и шрифты, текстуры из кода
  Settings      звук вкл/выкл (PlayerPrefs, громкость слушателя)
  Ui            билдеры RectTransform/Image/Text      Interaction  PressButton, Tap
  Tween Easing Effects Animator      анимации на корутинах; IAnimator = «где они играются» (NoAnimator в тестах)
Components/   по одному классу на элемент интерфейса, каждый строит себя и не знает про соседей
  PlateView (+PlateAnimator)  NameTag  TypedText  OptionButton  OptionStack  TimerDial
  SlidePanel  ChoicePanel  PickerPanel  BalancePill  StatBanner  InfoCloud  TitleCard
  StoryBoard    главное меню: карточки историй с превью, «Читать / Продолжить / Начать сначала»
  SettingsToast шестерёнка в углу и тост настроек: звук, «В меню»
  PageView      собирает экран из компонентов по Page; только связывает их (тап во время печати, выбор, конец)
App/          сцена и жизненный цикл
  Stage ScreenHost StoryFile BackdropTint   каркас сцены, смена экранов и баннеры, чтение истории, цвет фона
  SaveStore     сохранение на каждой показанной реплике (persistentDataPath/saves/<id>.json, атомарная запись)
  StoryPlayer SessionPrototype              два способа играть: история / демо
```

Зависимости идут вниз: `App → Components → Core → Content`. `Content/` не знает про Unity — это проверяет сборка тестов
(`Tests/Reader` компилирует эти файлы без UnityEngine).

## Как добавить

- **Новый стиль плашки.** Значение в `PlateStyle`, запись в `PlateSkins.For` (спрайт, отступы, ярлычок, орнамент), при
  необходимости спрайт в `docs/brainstorm/templates/assets/heritage/sprites/plates.py` с границами 9-slice. Раскладка,
  размеры, анимация и ярлычок подхватятся сами. Тест `EveryStyleHasASkin` и `EverySpriteTheCodeNamesExistsAsAPng` не даст забыть.
- **Новая правила истории → экран.** `BeatMapper` (+ тест в `Tests/Reader/BeatMapperTests.cs`); во `View` ничего менять не надо.
- **Новый блок экрана.** Поле-спецификация в `PageSpec.cs`, компонент в `Components/` (строит себя из спецификации, получает
  `IAnimator`), одна ветка в `PageView`. Компонент не должен ссылаться на другие компоненты, только на `Core` и `Content`.
- **Новое число раскладки.** `Metrics`. **Новый цвет.** `Theme`. **Новое время анимации плашки.** таблица `Timings` в `PlateAnimator`.
- **Новый спрайт.** модуль в `.../assets/heritage/sprites/`, регистрация вместе с границами; `task assets:png`, `task assets:unity`.

## Проверки

| Команда | Что проверяет | Нужен ли Unity |
|---|---|---|
| `task test:reader` | раскладка плашки, правила текста, скины, соответствие спрайтов, вся история -> экраны на 40 путях | нет |
| `task assets:test` | генератор спрайтов: SVG корректны, границы внутри спрайтов, вывод воспроизводим, файлы свежие | нет |
| `task prototype:smoke` | все экраны демо и истории строятся без исключений, ни один спрайт/шрифт не потерян, геометрия блоков | да (редактор закрыт) |
| `task prototype:play`, `task story:play` | проход всего демо / всего эпизода синтетическими тапами в PlayMode | да (редактор закрыт) |
