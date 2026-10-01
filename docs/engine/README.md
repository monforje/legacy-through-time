# Нарративный движок — алгоритмическое описание

Движок исполняет истории, написанные на [ink](https://github.com/inkle/ink), и превращает их в
поток «битов» (`Beat`) для визуальной новеллы. Код: `Assets/Scripts/Narrative/`.
Движок ничего не знает про Unity, рендер, звук и время — это чистая C#-библиотека.

Содержание:

1. [Архитектура: два слоя](#1-архитектура-два-слоя)
2. [Загрузка: JSON → граф узлов](#2-загрузка-json--граф-узлов)
3. [Модель исполнения ink VM](#3-модель-исполнения-ink-vm)
4. [Шаг δ: `Step`](#4-шаг-δ-step)
5. [Вывод: склейка, пробелы, `Continue` с lookahead](#5-вывод-склейка-пробелы-continue-с-lookahead)
6. [Выбор: `ProcessChoice` → `ChooseChoiceIndex`](#6-выбор)
7. [Потоки, туннели, функции: `CallStack`](#7-потоки-туннели-функции-callstack)
8. [Счётчики визитов и ходов](#8-счётчики-визитов-и-ходов)
9. [Случайность](#9-случайность)
10. [Сохранение и загрузка](#10-сохранение-и-загрузка)
11. [Слой `StoryMachine`: автомат визуальной новеллы](#11-слой-storymachine)
12. [Стиль: разбор строк и вариантов](#12-стиль-разбор-строк-и-вариантов)
13. [Ошибки](#13-ошибки)
14. [Сложность](#14-сложность)
15. [Как это проверяется](#15-как-это-проверяется)

---

## 1. Архитектура: два слоя

```mermaid
flowchart TB
    subgraph Host["Хост (презентер, Unity UI)"]
        P[Presenter]
    end

    subgraph L2["Слой 2 — Machine/"]
        SM["StoryMachine<br/>автомат Мили: Advance / Choose(i)"]
        ST["StoryStyle<br/>разбор речи, голоса, цены"]
        BT["Beat<br/>выходной символ λ"]
    end

    subgraph L1["Слой 1 — Ink/ (собственная реализация ink-рантайма)"]
        IR["InkRunner<br/>VM: Step, Continue, Choose"]
        IS["InkStory<br/>неизменяемая программа"]
        LD["InkLoader<br/>JSON → граф"]
        SV["InkSave<br/>сериализация состояния"]
        CS["CallStack / Cursor / Store"]
    end

    JSON[("ink JSON<br/>inklecate v18–v21")] --> LD --> IS
    IS --> IR
    IR --- CS
    IR --- SV
    SM --> IR
    SM --> ST
    SM --> BT
    P -- "Advance(), Choose(i)" --> SM
    SM -- "Beat" --> P
```

* **Слой 1 (`Ink/`)** — воспроизводит *наблюдаемую* семантику референсного ink-рантайма
  (текст, теги, выборы, переменные, случайные числа), но с другим устройством: узлы с байтовым
  опкодом вместо типов, всё, что референс разрешает лениво (пути, имена глобалов), резолвится один
  раз при загрузке.
* **Слой 2 (`Machine/`)** — превращает «VM, которая выдаёт текст» в детерминированный автомат с
  двумя входами. Именно он — API для игры.

`InkStory` неизменяем после загрузки: один `InkStory` может обслуживать любое число `InkRunner`
(прохождений) — все изменяемые данные живут в раннере.

---

## 2. Загрузка: JSON → граф узлов

`InkLoader.Load(json)`:

```mermaid
flowchart TD
    A["JsonReader.Parse<br/>дерево Dictionary / List / string / int / float / bool"] --> B{"inkVersion<br/>в [18, 21]?"}
    B -- нет --> X[FormatException]
    B -- да --> C["ReadContainer(root)<br/>рекурсивно, каждому контейнеру плотный Id"]
    C --> D["AssignPaths<br/>абсолютный путь каждого контейнера"]
    D --> E["Разрешить diverts<br/>→ Pointer (контейнер, индекс)"]
    E --> F["Разрешить choice-targets и read-count"]
    F --> G["Глобалы = VAR= внутри 'global decl'<br/>→ плотные слоты по порядку объявления"]
    G --> H["Привязать VarRef/VarAssign к слотам<br/>или к одноэлементному LIST-значению"]
    H --> I["Разрешить литералы '^->' в DivertTarget"]
    I --> J["Result: Root, Containers[], GlobalNames[], Targets"]
```

**Модель программы** (`Program.cs`) — дерево из `Container` и листовых `Node`:

| Узел | Опкод `Op` | Смысл |
|---|---|---|
| `Container` | `Container` | массив `Content[]` + словарь `Named`, флаги подсчёта, `Id`, `Path` |
| `TextNode` | `Text` | текст (`^…` в JSON) |
| `GlueNode` | `Glue` | `<>` |
| `CommandNode` | `Command` | управляющая команда (`Cmd`, 26 штук) |
| `DivertNode` | `Divert` | переход: статический, по переменной, функция `f()`, туннель `->t->`, внешняя `x()` |
| `ChoicePointNode` | `ChoicePoint` | точка выбора + флаги (условие, start/choice-only текст, invisible default, once-only) |
| `VarRefNode` / `VarAssignNode` | `VarRef` / `VarAssign` | чтение / запись переменной |
| `ReadCountNode` | `ReadCount` | `CNT?` — число визитов |
| `NativeNode` | `Native` | оператор из таблицы `NativeOps` |
| `LiteralNode` | `Literal` | константа, divert-target, указатель на переменную, список |
| `TagNode` | `Tag` | статический тег до ink 1.1 |

**Программный счётчик** — `Pointer(Container, Index)`. `Index == -1` означает «сам контейнер,
будет введён на следующем шаге». Путь вида `knot.0.c-1` разбирается один раз; промах при разборе
возвращает самый глубокий достигнутый объект с пометкой `approximate` (то же правило восстановления,
что в референсе).

**Значения** (`Value`) — размеченное объединение-структура (без боксинга чисел). Порядок `ValueKind`
задаёт **решётку приведения** ink:

```
Bool < Int < Float < List < String < DivertTarget < VariablePointer
```

Бинарная операция поднимает оба операнда до большего вида (`NativeOps.Call`), затем применяет
оператор для этого вида: `int` — с переполнением по кругу, деление целых — с усечением и т. д.
`Void` и `Tag` — значения только для стека.

---

## 3. Модель исполнения ink VM

Формально `InkRunner` — **расширенный конечный автомат со стековой памятью**:

* **управляющее состояние** — `Pointer` текущего фрейма;
* **расширенное состояние** — `(CallStack, Eval, Output, Globals, Visits, Turns, TurnIndex, Seed, PreviousRandom)`;
* **функция переходов δ** — `Step()`;
* `Continue()` итерирует δ до границы строки.

Расширенное состояние делится на две части с разными стратегиями отката:

```mermaid
flowchart LR
    subgraph Cursor["Cursor — управляющая часть (малая)"]
        CSK["CallStack<br/>потоки → кадры → temp-переменные"]
        EV["Eval — стек вычислений"]
        OUT["Output — поток вывода"]
        CH["Choices — набранные выборы"]
        MISC["Diverted, TurnIndex,<br/>Seed, PreviousRandom"]
    end
    subgraph Store["Store — данные (большие)"]
        GL["Globals[слот]"]
        VI["Visits[Id контейнера]"]
        TU["Turns[Id контейнера]"]
        JR["Журнал (slot, старое значение)"]
    end
    Cursor -- "снимок = глубокая копия<br/>O(размер строки)" --> SNAP[(Snapshot)]
    Store -- "журналирование записей<br/>O(число записей)" --> JR
```

* `Cursor` мал (строка вывода, пара кадров) → снимок это простая глубокая копия
  (`CopyFrom`, с повторным использованием уже выделенных списков/кадров — в установившемся режиме
  без аллокаций).
* `Store` может быть большим → вместо копирования пишется **журнал**: пока открыт снимок, каждая
  запись пишет `(вид, слот, старое значение)`. Откат — проигрывание журнала в обратном порядке
  (применение обратной дельты), фиксация — просто очистка журнала. Цена — O(записей после
  новой строки), а не O(размер состояния).

---

## 4. Шаг δ: `Step`

```mermaid
flowchart TD
    S([Step]) --> N{"pointer = null?"}
    N -- да --> R([return])
    N -- нет --> D["Спуск в контейнеры:<br/>пока pointer.Resolve() — Container:<br/>VisitContainer(c, atStart=true),<br/>pointer = StartOf(c)"]
    D --> L["node = pointer.Resolve()<br/>PerformLogicAndFlowControl(node)"]
    L --> F{"узел — поток/логика?<br/>(Divert, Command, VarAssign,<br/>VarRef, ReadCount, Native)"}
    F -- да --> NX
    F -- нет --> K{"Op"}
    K -- ChoicePoint --> CP["ProcessChoice → Choices.Add"]
    K -- Text --> T{"InExpression?"}
    T -- да --> TP["Push(String)"]
    T -- нет --> TO["PushToOutput(text)"]
    K -- Glue --> G["PushItem(Glue)"]
    K -- Tag --> TG["PushItem(Tag) / Push(Tag)"]
    K -- Literal --> LT["Push(v) в выражении<br/>иначе 'Other' в Output"]
    CP --> NX
    TP --> NX
    TO --> NX
    G --> NX
    TG --> NX
    LT --> NX
    NX["NextContent()"] --> TH{"команда StartThread?"}
    TH -- да --> PT["CallStack.PushThread()<br/>(после сдвига указателя)"]
    TH -- нет --> E([конец шага])
    PT --> E
```

**`NextContent`** — выбор следующего указателя:

```mermaid
flowchart TD
    A([NextContent]) --> B["Previous = Current"]
    B --> C{"Diverted ≠ null?"}
    C -- да --> D["Current = Diverted; Diverted = null<br/>VisitChangedContainersDueToDivert()"]
    D --> E{"Current ≠ null?"}
    E -- да --> Z([готово])
    E -- нет --> F
    C -- нет --> F["IncrementContentPointer()"]
    F --> G{"нашёлся следующий?"}
    G -- да --> Z
    G -- нет --> H{"вершина стека — Function?"}
    H -- да --> I["PopCallstack; если в выражении — Push(Void)"]
    H -- нет --> J{"можно снять поток?"}
    J -- да --> K["PopThread"]
    J -- нет --> L["TryExitFunctionEvaluationFromGame"]
    I --> M{"снято и Current ≠ null?"}
    K --> M
    M -- да --> A
    M -- нет --> Z
    L --> Z
```

**`IncrementContentPointer`** — это «обход в глубину вверх»: увеличить индекс в текущем
контейнере; если вышли за конец — подняться к родителю и взять `container.Index + 1`; повторять,
пока не найдётся место или не дойдём до корня (тогда указатель становится `null` — конец потока).

**Режим выражения** (`InExpression`, флаг кадра). Между командами `ev` и `/ev` литералы, значения
переменных и текст кладутся в стек `Eval`, а не в `Output`; `out` выводит верхушку стека в поток.
Так скомпилирована вся арифметика, условия и интерполяция `{…}`.

**Команды** (`Cmd`) группируются так:

| Группа | Команды |
|---|---|
| вычисление | `EvalStart`, `EvalEnd`, `EvalOutput`, `Duplicate`, `PopEvaluatedValue`, `NoOp` |
| потоки управления | `PopFunction`, `PopTunnel`, `StartThread`, `Done`, `End` |
| строки / теги | `BeginString`, `EndString`, `BeginTag`, `EndTag` |
| метаданные | `ChoiceCount`, `Turns`, `TurnsSince`, `ReadCount`, `VisitIndex` |
| случайность | `Random`, `SeedRandom`, `SequenceShuffleIndex`, `ListRandom` |
| списки | `ListFromInt`, `ListRange` |

---

## 5. Вывод: склейка, пробелы, `Continue` с lookahead

### 5.1. Поток вывода

`Output` — список `OutItem` (`Text`, `Glue`, `BeginString`, `BeginTag`, `EndTag`, `Tag`, `Other`).
Текст добавляется через `PushItem`, который применяет правила обрезки:

```mermaid
flowchart TD
    A([PushItem item]) --> B{"item.Kind"}
    B -- Glue --> C["TrimNewlinesFromOutput:<br/>с хвоста назад удалить все '\\n' (и пробелы между)<br/>до первого непустого текста или команды"]
    C --> INC
    B -- Text --> D["glueTrimIndex = последний Glue (до BeginString)<br/>functionTrimIndex = начало вывода функции"]
    D --> E{"есть trimIndex?"}
    E -- да --> F{"item"}
    F -- "'\\n'" --> NO["не включать"]
    F -- "непустой текст" --> G["RemoveExistingGlue;<br/>сбросить FunctionStartInOutput у вложенных функций"]
    F -- "пробелы" --> INC
    G --> INC
    E -- нет --> H{"item = '\\n'?"}
    H -- да --> I{"вывод уже кончается на '\\n'<br/>или в нём нет текста?"}
    I -- да --> NO
    I -- нет --> INC
    H -- нет --> INC
    INC["Output.Add(item)"]
```

Инварианты: строка никогда не начинается с `\n` и не содержит двух `\n` подряд; перед `\n` и в
начале строки пробелы убираются; серии пробелов/табов схлопываются в один пробел
(`Cursor.CleanWhitespace`).

`PushToOutput` предварительно дробит строку функцией `SplitHeadTailWhitespace` на
«голова-перевод-строки / тело / хвост-перевод-строки», чтобы глю могло убирать переводы строк по
одному.

### 5.2. Почему нужен lookahead

Строка ink заканчивается на `\n`, но **последующая** глю `<>` может этот `\n` отменить:

```ink
Привет,
<> мир
```

Значит, увидев `\n`, нельзя сразу отдавать строку: надо проверить, что будет дальше. Эталонный
рантайм делает это через копию всего состояния; здесь — через **снимок + журнал** (раздел 3).

### 5.3. `Continue`

```mermaid
stateDiagram-v2
    [*] --> Running
    Running --> Running: Step, вывод не кончается на '\n'
    Running --> Lookahead: вывод кончился на '\n' и CanContinue<br/>TakeSnapshot() + Store.BeginRecording()
    Lookahead --> Lookahead: Step, нет изменений в выводе
    Lookahead --> Running: «\n» удалён глю (NewlineRemoved)<br/>DiscardSnapshot() — фиксация журнала
    Lookahead --> Done: ExtendedBeyondNewline — после '\n' появился<br/>текст/тег<br/>RestoreSnapshot() — откат всего после '\n'
    Lookahead --> Done: внешняя функция, не lookaheadSafe<br/>RestoreSnapshot()
    Lookahead --> Done: поток закончился (!CanContinue)<br/>RestoreSnapshot()
    Running --> Done: поток закончился
    Done --> [*]: возврат CurrentText
```

Исход `NewlineChangeSinceSnapshot` — один из четырёх: `NoChange`, `ExtendedBeyondNewline`,
`NewlineRemoved`, `Unknown`.

* **Быстрый путь.** Если с момента снимка из `Output` ничего не удалялось (`Removals` не
  изменился) и тег не открыт, то поток = снимок ++ суффикс, а очистка текста не меняет префикс,
  оканчивающийся на `\n`. Поэтому ответ определяется *только суффиксом*: достаточно пройти его и
  найти непустой `Text` — O(|суффикс|), без аллокаций. Любая работа с тегами или удалениями
  откатывается на эталонное определение на полностью перестроенном тексте.
* **Нет снимков внутри строкового вычисления** (`InStringEvaluation`: между `BeginString` и
  `EndString`, т. е. при построении текста выбора и `"{func()}"`).
* **Внешние функции.** Функция с побочным эффектом (`lookaheadSafe == false`) не должна
  запускаться спекулятивно: при открытом снимке вызов отменяется, выставляется
  `_sawLookaheadUnsafeFunctionAfterNewline`, и строка завершается (откат к снимку).

Снимок один за раз; «запасной» курсор (`_spare`) переиспользуется для следующего снимка.

---

## 6. Выбор

Каждая `ChoicePointNode` порождает кандидат-выбор при проходе по ней в `Step`:

```mermaid
flowchart TD
    A([ProcessChoice]) --> B{"HasCondition?"}
    B -- да --> C["Pop → если ложно: show = false"]
    B -- нет --> D
    C --> D["если HasChoiceOnlyContent: Pop строка + теги<br/>если HasStartContent: Pop строка + теги"]
    D --> E{"OnceOnly и VisitCount(target) > 0?"}
    E -- да --> NO["show = false"]
    E -- нет --> F{"show?"}
    NO --> F
    F -- нет --> Z([null])
    F -- да --> G["ChoiceRecord:<br/>Target, SourcePath, Tags,<br/>Thread = CallStack.ForkThread(),<br/>Text = (start + choiceOnly).Trim"]
```

Ключевые моменты:

* Текст выбора = «начальный» + «только в выборе» (`[ ]` в ink). Собираются из стека, куда они
  попали как строки через `BeginString`/`EndString`; теги выбора подбираются с вершины стека.
* **Выбор запоминает форк потока**, в котором он создан (`ForkThread`) — ведь когда игрок
  выберет, исходный поток уже может быть снят (`<-` threads). Выбор возобновляет *именно тот*
  контекст.
* `once-only` проверяется по счётчику визитов целевого контейнера — отсюда `Container.CountsVisits`
  для всех целей таких выборов (устанавливает компилятор через флаги).

**Конец потока и выборы.** Когда `CanContinue` стал ложным, `ContinueSingleStep` вызывает
`TryFollowDefaultInvisibleChoice`: если *все* набранные выборы — «невидимые по умолчанию»
(`* ->`, fallback), автоматически выбирается первый — без хода игрока и без инкремента хода
(`TurnIndex`). Если при этом открыт снимок, поток выбора форкается ещё раз, чтобы откат не испортил
его.

**`ChooseChoiceIndex(i)`:**

```mermaid
flowchart LR
    A["visible = выборы без invisible"] --> B{"0 ≤ i < |visible|?"}
    B -- нет --> X[ArgumentOutOfRange]
    B -- да --> C["CallStack.CurrentThread = choice.Thread<br/>(остальные потоки отброшены)"]
    C --> D["ChoosePath(Target.start, incrementTurn=true)<br/>Choices.Clear(); TurnIndex++"]
    D --> E["VisitChangedContainersDueToDivert()"]
```

`CurrentChoices` перенумеровывает видимые выборы с 0 (индексы невидимых «съедаются»).

---

## 7. Потоки, туннели, функции: `CallStack`

`CallStack` — **стек потоков, каждый поток — стек кадров**:

```mermaid
flowchart TB
    subgraph CS["CallStack"]
        direction TB
        T2["Thread #2 (текущий — последний в списке)"]
        T1["Thread #1"]
        T0["Thread #0 (корневой)"]
    end
    subgraph T2f["Frames потока"]
        direction TB
        F2["Frame: Function  pointer, temps, EvalHeightWhenPushed, FunctionStartInOutput"]
        F1["Frame: Tunnel"]
        F0["Frame: Tunnel (корень)"]
    end
    T2 --> T2f
```

| Тип кадра | Создаётся | Снимается |
|---|---|---|
| `Tunnel` (`->t->`) | divert с `PushesToStack` | команда `->->` (может переопределить цель) |
| `Function` (`f()`) | divert с `PushesToStack` | `~ret` или выход за конец контейнера функции |
| `FunctionEvaluationFromGame` | `EvaluateFunction` из хоста | после возврата значения хосту |

**Потоки** (`<- name`): `StartThread` форкает *текущий* поток (после сдвига указателя, чтобы
возврат из потока продолжил после `<-`), новый поток становится текущим. Достижение конца потока
(`Done`/выход за контейнер) снимает его и возвращает к родителю. Потоки нужны для сбора выборов из
нескольких источников; к концу `Continue` они должны быть «плоскими» (иначе ошибка).

**Временные переменные** живут в `Frame.Temps` (лениво создаваемый словарь). Адрес переменной —
**индекс контекста**: `0` — глобальная, `n` — кадр `n−1` текущего потока. Указатели
(`ref`‑параметры) хранят пару `(имя, contextIndex)`; чтение/запись идут по цепочке указателей до
настоящей переменной (`GetVariable` / `Assign`).

**Порядок поиска имени:** глобальные → одноэлементный LIST → temp (как в ink).

**`EvaluateFunction(name, args)`** — вызов ink-функции из хоста без продвижения истории:
сохраняет `Output`, кладёт кадр `FunctionEvaluationFromGame`, пушит аргументы, гонит `Continue` до
конца, достаёт возвращаемое значение со стека, восстанавливает `Output`.

---

## 8. Счётчики визитов и ходов

`Store.Visits[Id]` и `Store.Turns[Id]` — массивы, индексируемые плотным `Container.Id`
(без словарей по путям). Какие контейнеры считаются — решает компилятор ink через флаги
`#f` (`CountVisits`, `CountTurns`, `CountStartOnly`).

Две точки увеличения:

1. **`Step` при входе в контейнер сверху вниз** — `VisitContainer(c, atStart: true)`.
2. **`VisitChangedContainersDueToDivert`** после любого перехода: считаются все контейнеры,
   которые *впервые вошли* на пути вниз к цели.

```mermaid
flowchart TD
    A(["VisitChangedContainersDueToDivert"]) --> B["gen = ++generation<br/>отметить штампом gen всех предков Previous"]
    B --> C["container = родитель целевого узла<br/>allAtStart = true"]
    C --> D{"container ≠ null<br/>и (не помечен gen или CountsAtStartOnly)?"}
    D -- нет --> Z([конец])
    D -- да --> E["atStart = child — первый элемент контейнера<br/>и allAtStart"]
    E --> F["VisitContainer(container, atStart)"]
    F --> G["child = container; container = container.Parent"]
    G --> D
```

«Штамп поколения» (`_stamp[Id] = gen`) заменяет аллокацию `HashSet` на каждый переход: проверка
«этот контейнер предок прошлого указателя?» за O(1).

* `TURNS_SINCE(-> x)` = `TurnIndex − Turns[x]` либо `-1`, если не посещали (`NoTurn`).
* `TurnIndex` растёт только на выбор игрока (`ChooseChoiceIndex`, `ChoosePathString`), но не при
  автоматическом выборе невидимого default.

---

## 9. Случайность

Вся случайность воспроизводима и *побитово совпадает* с референсным ink (и с `System.Random`
.NET), поэтому результаты одинаковы в редакторе, inky и IL2CPP.

* `InkRandom` — **субтрактивный генератор Кнута** (алгоритм «Net5CompatSeed», остаточный
  массив из 56 слов, `MBig = int.MaxValue`, `MSeed = 161803398`). Реализован руками, чтобы не
  зависеть от версии рантайма.
* **`RANDOM(min, max)`:**
  `next = First(Seed + PreviousRandom)`;  результат `next % (max−min+1) + min`;
  затем `PreviousRandom = next`. То есть «состояние генератора» — пара `(Seed, PreviousRandom)`,
  и именно она сохраняется.
* **`SEED_RANDOM(x)`** — `Seed = x`, `PreviousRandom = 0`.
* **`LIST_RANDOM`** — аналогично, индекс = `next % Count`.
* **Начальный seed** — `First(DateTime.UtcNow.Millisecond) % 100` (как в ink); в тестах и играх с
  воспроизводимостью задаётся явно.

### Перемешанные последовательности (`{~a|b|c}`)

`NextSequenceShuffleIndex` — **детерминированная перестановка**, не зависящая от
`PreviousRandom`:

```mermaid
flowchart TD
    A["seqCount (сколько раз уже вычислялась), count (число элементов)"] --> B["loopIndex = seqCount / count<br/>iterationIndex = seqCount % count"]
    B --> C["hash = сумма кодов символов container.Path"]
    C --> D["rng = InkRandom(hash + loopIndex + Seed)"]
    D --> E["unpicked = [0 … count-1]"]
    E --> F["повторить iterationIndex+1 раз:<br/>chosen = rng.Next() % |unpicked|; выкинуть из unpicked"]
    F --> G(["вернуть выбранный на последней итерации"])
```

Следствие: в рамках одного «круга» (`loopIndex`) последовательность — настоящая перестановка
(без повторов), а при повторном заходе (`seqCount` растёт) порядок свой на каждый круг, но
воспроизводим для данной `(последовательность, seed)`.

---

## 10. Сохранение и загрузка

`InkSave` (формат `inkSave`, версия 1). Принципы:

* **Позиции — по путям**, не по индексам: `(container path, index)`. Сохранение переживает правки
  несвязанных частей истории.
* **Глобалы — по имени и только если отличаются от значений по умолчанию**
  (`Defaults` = значения сразу после выполнения `global decl`). Новая переменная в обновлённой
  истории получает значение по умолчанию.
* **Счётчики — по пути контейнера**; нулевые не пишутся.
* Значения кодируются теми же токенами, что и сам ink-JSON (`^text`, `{"^->":path}`,
  `{"list":{…}}`…).

Что входит в сохранение: `turn`, `seed`, `previousRandom`, `globals`, `visits`, `turns`,
`threads` (кадры → pointer, temps, тип…), `threadCounter`, `output`, `choices` (каждый со своим
потоком), `eval`, `diverted`.

```mermaid
sequenceDiagram
    participant H as Хост
    participant R as InkRunner
    participant S as InkSave
    H->>R: SaveState() (между Continue)
    R->>S: Write(runner)
    S-->>H: JSON
    Note over H,S: …позже…
    H->>R: LoadState(json)
    R->>S: Read(runner, json)
    S->>S: Globals = Defaults; применить diff
    S->>S: Visits/Turns: обнулить, применить
    S->>S: потоки, output, choices, eval → новый Cursor
    S->>R: ReplaceState(cursor); _snapshot = null
```

Если путь из сохранения не найден — `FormatException` с понятным текстом
(«Has the story changed since this save?»). Незнакомые глобалы пропускаются.

`StoryMachine.Save()` оборачивает это: `{ storyMachine: 1, phase, [error], ink: {…} }`.
При `Load` `Beat` **перестраивается из состояния** (из `CurrentText`/`CurrentChoices`), а не
хранится.

---

## 11. Слой `StoryMachine`

Автомат Мили `M = (Q, Σ, Λ, V, δ, λ, q₀, v₀)`:

| Компонент | В коде |
|---|---|
| `Q` — состояния | `StoryPhase`: `Ready`, `Line`, `Directive`, `Choice`, `End`, `Fault` |
| `Σ` — входы | `Advance`, `Choose(i)` |
| `Λ` — выходы | `Beat` (фаза, голос, спикер, текст, теги, опции, дельта переменных) |
| `V` — расширенное состояние | всё состояние `InkRunner` |
| `δ`, `λ` | `Transition` (внутри `Advance`/`Choose`) |
| `q₀` | `Ready` |

```mermaid
stateDiagram-v2
    [*] --> Ready
    Ready --> Line: Advance
    Ready --> Directive: Advance
    Ready --> Choice: Advance
    Ready --> End: Advance

    Line --> Line: Advance
    Line --> Directive: Advance
    Line --> Choice: Advance
    Line --> End: Advance

    Directive --> Line: Advance
    Directive --> Directive: Advance
    Directive --> Choice: Advance
    Directive --> End: Advance

    Choice --> Line: Choose(i)
    Choice --> Directive: Choose(i)
    Choice --> Choice: Choose(i)
    Choice --> End: Choose(i)

    Ready --> Fault: ошибка VM
    Line --> Fault: ошибка VM
    Directive --> Fault: ошибка VM
    Choice --> Fault: ошибка VM

    End --> [*]
    Fault --> [*]
```

`End` и `Fault` **поглощающие**: из них ни один вход не выводит.

**Охранные условия** (guards) делают δ тотальной:

* `CanAdvance` = фаза ∈ {`Ready`, `Line`, `Directive`};
* `CanChoose(i)` = фаза = `Choice` и `0 ≤ i < Options.Count`.

Недопустимый вход возвращает `false` и *не меняет ни `q`, ни `V`*.

### Алгоритм перехода

```mermaid
flowchart TD
    A(["Advance() / Choose(i)"]) --> G{"guard?"}
    G -- нет --> F0(["return false"])
    G -- да --> B["before = копия Globals<br/>очистить _errors, _warnings"]
    B --> C{"Choose?"}
    C -- да --> D["runner.ChooseChoiceIndex(i)"]
    C -- нет --> E
    D --> E["Produce()"]
    E --> H{"исключение StoryException?"}
    H -- да --> FB["Beat(Fault, error)"]
    H -- нет --> I["beat"]
    FB --> J
    I --> J["Current = beat.WithDelta(Δ, warnings)"]
    J --> K(["return true"])
```

**`Produce()`** — цикл «гнать VM, пока не появится что показать»:

```mermaid
flowchart TD
    A([Produce]) --> B{"runner.CanContinue?"}
    B -- да --> C["text = Continue()"]
    C --> D{"_errors не пусто?"}
    D -- да --> FA(["Beat Fault"])
    D -- нет --> E{"text пуст<br/>и тегов нет?"}
    E -- да --> B
    E -- нет --> LB(["LineBeat(text, tags)"])
    B -- нет --> F{"_errors не пусто?"}
    F -- да --> FA
    F -- нет --> G{"есть видимые выборы?"}
    G -- да --> CB(["ChoiceBeat"])
    G -- нет --> EN(["Beat End"])
```

Пустые строки (чистая логика ink: `~ x = 1`) в `Beat` не попадают — они молча проглатываются.
Строка без текста, но с тегами — `Directive` (команды сцене: фон, музыка).

**Дельта** `Δ = {(name, v, v′) | v ≠ v′}` — сравнение `Globals` до и после перехода по порядку
объявления; значения сравниваются `Value.SameAs`. Запись из хоста (`SetVariable`) в дельту не
попадает.

`Beat` иммутабелен; остаётся валидным до следующего принятого входа.

---

## 12. Стиль: разбор строк и вариантов

`StoryStyle` — чистые функции без состояния, правила автора можно менять без правки автомата.

**`ParseLine(line, tags) → (voice, speaker, text)`**

```mermaid
flowchart TD
    A([ParseLine]) --> B["text = line.Trim()"]
    B --> C["перебрать теги по порядку:<br/>первый тег без значения, ключ которого в VoiceTags → voice (tagged)"]
    C --> D["candidate = tagged ? voice : Dialogue"]
    D --> E{"candidate ∈ SpeakerVoices<br/>(Dialogue, Flashback)?"}
    E -- нет --> Z(["(voice, null, text)"])
    E -- да --> F["TrySplitSpeaker(text)"]
    F --> G{"успех?"}
    G -- да --> H(["(candidate, name, rest)"])
    G -- нет --> Z
```

**`TrySplitSpeaker`** принимает «Имя: реплика», только если:

1. двоеточие найдено, `0 < позиция ≤ MaxSpeakerLength (40)`;
2. после двоеточия пробел (чтобы «12:30» не считалось репликой);
3. в имени нет `. ! ? « " (`.

Неразмеченная строка без спикера остаётся `Narration` (`voice` по умолчанию).

Теги: `# narr`, `# thought`, `# system`, `# flashback`, `# sfx`.
`StoryTag.Parse("key: value")` делит по первому двоеточию; тег без двоеточия — `Value = null`.

**`ParseOption`** — платные варианты: начинаются с `💎`, заканчиваются `(N 💎)`:

```
💎 Взять нож (5 💎)   →   text = "Взять нож", isPremium = true, cost = 5
```

---

## 13. Ошибки

```mermaid
flowchart TD
    A["InkException внутри Step<br/>(ошибка семантики ink)"] --> B["ContinueInternal: catch → AddError"]
    B --> C["_c.Errors += сообщение<br/>ForceEnd() — сброс стека, выборов, указателя"]
    C --> D["ReportErrors()"]
    D --> E{"OnError задан?"}
    E -- да --> F["вызвать OnError(msg, isWarning)<br/>(в StoryMachine — копит в _errors/_warnings)"]
    E -- нет --> G["throw StoryException"]
    F --> H["Produce → Beat Fault"]
    G --> H
```

* **Ошибка** останавливает историю: `ForceEnd`, `CanContinue == false`. `StoryMachine` превращает
  её в `Beat(Fault)` — поглощающее состояние.
* **Предупреждение** (`Warning`: например, «переменная не найдена, использую 0») — копится и
  попадает в `Beat.Warnings`, историю не прерывает.
* После окончания потока без `-> END`/`-> DONE`/выбора выдаётся диагностическое сообщение
  (нужен ли `->->`, `~ return`, `-> DONE`).
* Ссылка на внешнюю функцию без привязки: если разрешены fallback (`AllowExternalFunctionFallbacks`
  по умолчанию), вызывается ink-функция с тем же именем.

---

## 14. Сложность

| Операция | Цена | Комментарий |
|---|---|---|
| Загрузка | O(размер JSON) | один проход + разрешение путей один раз |
| Шаг `Step` | O(1) амортизированно | диспетчеризация по байту `Op`, глобалы по слоту |
| Чтение/запись глобала | O(1) | плотные слоты вместо хеширования имён |
| Счётчики | O(1) | массивы по `Container.Id` |
| Divert → счётчики | O(глубина дерева) | штампы поколения, без аллокаций |
| Снимок lookahead | O(размер строки + глубина стека) | глубокая копия `Cursor`, переиспользование объектов |
| Журнал `Store` | O(числа записей после `\n`) | запись O(1), откат O(записей) |
| Решение «`\n` отменён?» | O(\|суффикс\|) | быстрый путь, иначе O(длина строки) |
| `PushItem` | O(\|вывод\|) в худшем случае | обход назад до глю/BeginString |
| Перемешанная последовательность | O(count²) | `RemoveAt` из списка; count мал |
| Save | O(контейнеры + глобалы) | пишутся только ненулевые/изменённые |
| `StoryMachine.Delta` | O(число глобалов) | копия `Globals` перед и сравнение после |

Память: один `InkStory` на все раннеры; раннер — массивы фиксированного размера
(`Globals`, `Visits`, `Turns`) + малый `Cursor`.

---

## 15. Как это проверяется

`task test` (в Docker, см. `Taskfile.yml`) гоняет `Tests/Narrative`:

| Набор | Что проверяет |
|---|---|
| `DifferentialTests` / `ApiDifferentialTests` | **дифференциальное тестирование** против официального ink-рантайма на корпусе историй: тот же текст, теги, выборы, переменные, счётчики; то же публичное API (`EvaluateFunction`, `ChoosePathString`, теги…) |
| `MachineTests.Invariants_Hold_On_Every_Beat` | на 40 случайных прогулках: нет `Fault`; `Choice ⇔ есть опции`; `Line` всегда с текстом; `Directive` всегда с тегами; текст без хвостового `\n` |
| `Transition_Function_Is_Total_Via_Guards` | недопустимый вход отвергается и не меняет состояние |
| `Deterministic_Given_Seed_And_Inputs` | один seed + одни входы → одинаковые `Beat` |
| `Load_After_Save_Is_Observationally_Identity` | сохранить/загрузить в любой точке → неотличимо |
| `Deltas_Compose_To_Final_Variables` | сумма дельт даёт финальные значения переменных |
| `UnitTests` | `InkRandom` совпадает с `System.Random`; JSON round-trip; откат журнала = точная обратная операция |
| `StyleTests` | разбор речи, голосов, тегов, цен |

`task bench` сравнивает время и аллокации на реальном эпизоде с официальным рантаймом.

Запуск ink-компилятора: `task ink` (`content/stories/**/*.ink` → `buckets/stories/**/*.json`).
