# UI-кит оболочки и макеты экранов: план

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Нейтральный UI-кит оболочки приложения и 9 HTML-макетов экранов на нём, плюс индекс.

**Architecture:** Папка `docs/brainstorm/templates/shell/`, статический HTML без сборки. Один `kit.css` с токенами (слой токенов сменный, цвета обложек отдельно в `--art-*`) и компонентами; один `shell.js` с поведением и функцией аудита; один `icons.svg` со спрайтом. Каждый экран это отдельный `.html` в рамке S23. Проверки: статический `check.py` (токены, палитра, контраст, ссылки) и браузерный `shellAudit()` (касания, переполнение, прокрутка).

**Tech Stack:** HTML, CSS (custom properties), ванильный JS, Python 3 stdlib для `check.py`, просмотр через `task mockups`, браузерная проверка через chrome-devtools MCP.

**Spec:** `docs/superpowers/specs/2026-10-30-app-shell-ui-kit-design.md`. Референсы: `docs/brainstorm/refs/shell-*.png`.

## Global Constraints

- Рамка экрана: 360×780 CSS px, safe area сверху 32px, снизу 24px (как `docs/brainstorm/templates/_s23.html`).
- Без сборки и без внешних зависимостей, кроме шрифта из Google Fonts; шрифт меняется в одной переменной `--font`.
- Цвет и размер только из токенов `kit.css`; цвета обложек и колеса только из группы `--art-*`.
- Нет зелёно-красной палитры, орнамента и пергамента; нет глянца, звёзд и градиентных кнопок.
- Контраст текста не ниже WCAG AA (4.5:1 обычный, 3:1 крупный), касания не меньше 44×44px.
- Валюты: монеты и 💎 (как `Wallet`/`gems` в движке); аналог чая называется «энергия» (временное имя).
- Не трогаем `Assets/`, `skeleton.css`, `heritage.css` и экран чтения.

## Review Focus

- Длинные русские названия и описания в карточках: должны переноситься или обрезаться многоточием, а не вылезать (`audit` проверяет переполнение).
- Большие числа в плашке валюты (`99 999`) и ноль: плашка не ломается (в Task 3 фикстура в `kit.html`, `audit` проверяет).
- ИИ-чат: пустой диалог, длинное сообщение, исчерпанный лимит: у каждого свой видимый вид (Task 8, состояния переключаются кнопками в макете).
- Тумблеры, вкладки и панель работают с клавиатуры (Tab/Enter), а у иконочных кнопок есть `aria-label` (`check.py` ловит кнопки без подписи).
- Колесо при `prefers-reduced-motion`: вращение заменяется мгновенным результатом (Task 7).

---

## File Structure

```
docs/brainstorm/templates/shell/
  kit.css       токены (:root) + компоненты; единственное место с цветами
  icons.svg     SVG-спрайт, <symbol id="i-...">
  shell.js      вкладки, переключатели, панель, колесо, тосты, window.shellAudit()
  kit.html      каталог токенов и компонентов
  home.html  home-panel.html  rewards.html  shop.html  wheel.html
  settings.html  chat.html  chapters.html  library.html
  index.html    все экраны рядами + ссылка на kit.html
  check.py      статическая проверка
```

Общий шаблон экрана: `<div class="device"><div class="safe">…</div></div>` с `kit.css` и `shell.js`, плюс `<svg><use href="icons.svg#i-name"/></svg>` для иконок.

### Task 1: Каркас, токены и проверка `check.py`

**Files:**
- Create: `shell/check.py`, `shell/kit.css` (только `:root`, `.device`, `.safe`, шрифт), `shell/icons.svg` (пустой `<svg>`), `shell/shell.js` (пустой)

**Interfaces:**
- Produces: токены `--bg --surface --surface-2 --accent --accent-ink --ok --danger --text --text-muted --line --radius-s --radius-m --radius-l --space-1..6 --font --art-*`; команда `python3 -I docs/brainstorm/templates/shell/check.py` (код выхода 0/1, список ошибок построчно).

- [ ] **Step 1: Write `check.py` with the failing checks.** Функции: `colors_outside_tokens(html_css_files) -> list[str]` (hex/rgb/hsl вне `:root` в `kit.css` и вообще в `*.html`; `--art-*` разрешены); `forbidden_hues(tokens) -> list[str]` (токены кроме `--ok`, `--danger`, `--art-*` с оттенком 100–170° или 340–15° при насыщенности > 0.35); `contrast(tokens) -> list[str]` (пары `--text/--bg`, `--text/--surface`, `--text/--surface-2`, `--accent-ink/--accent`, `--text-muted/--bg` ≥ 4.5); `unlabeled_buttons(html) -> list[str]` (`<button>` без текста и без `aria-label`); `dead_links(html) -> list[str]` (относительные `href` на несуществующие файлы).
- [ ] **Step 2: Run it:** `python3 -I docs/brainstorm/templates/shell/check.py`. Expected: FAIL (в `kit.css` ещё нет токенов).
- [ ] **Step 3: Write `kit.css` `:root`.** Тёмная сине-фиолетовая палитра, один акцент, значения на выбор исполнителя в рамках Global Constraints; `.device` 360×780 с `overflow:hidden`, `.safe` с отступами 32/24.
- [ ] **Step 4: Run again.** Expected: PASS, вывод `ok`.
- [ ] **Step 5: Commit** `feat(mockups): shell kit tokens and static checks`.

### Task 2: Компоненты кита и иконки

**Files:**
- Modify: `shell/kit.css`, `shell/icons.svg`

**Interfaces:**
- Consumes: токены Task 1.
- Produces: классы `.btn .btn-primary .btn-secondary .btn-price .btn-icon[disabled]`, `.pill` (валюта: `.pill-coin .pill-gem .pill-energy`, с `.pill-plus`), `.card .card-story .card-reward .card-product .card-best`, `.switch`, `.stepper` (`‹ значение ›`), `.row` (строка настройки), `.search`, `.tabbar .tab[aria-current] .badge`, `.segmented`, `.chip[aria-pressed]`, `.modal`, `.sheet`, `.toast`, `.bubble`, `.chat-input`; иконки `i-coin i-gem i-energy i-gear i-gift i-ads i-wheel i-cart i-calendar i-timer i-lock i-search i-chat i-book i-star i-bag i-back i-chevron-l i-chevron-r i-close i-send i-flag i-shuffle i-user i-bell i-globe i-support`.

- [ ] **Step 1: Add the 33 symbols to `icons.svg`.** Одна толщина линии (2px), `currentColor`, viewBox 24.
- [ ] **Step 2: Implement the component classes in `kit.css`.** Касание не меньше 44px через `min-height: 44px` на интерактивных; состояния `:hover :active :focus-visible :disabled`; крупный фокус-контур токеном.
- [ ] **Step 3: Run `check.py`.** Expected: PASS (нет цветов вне токенов).
- [ ] **Step 4: Commit** `feat(mockups): shell components and icons`.

### Task 3: Каталог `kit.html`

**Files:**
- Create: `shell/kit.html`
- Modify: `shell/shell.js` (добавить `window.shellAudit()`)

**Interfaces:**
- Produces: `shellAudit(): {smallTargets: string[], overflow: string[], hScroll: boolean}`: перечисляет интерактивные элементы меньше 44×44 (с селектором), элементы с `scrollWidth > clientWidth` внутри `.device`, и горизонтальную прокрутку документа.

- [ ] **Step 1: Write `shellAudit()` in `shell.js`.**
- [ ] **Step 2: Build `kit.html`:** секции токенов (образцы цветов с именами, шкалы отступов и радиусов, типографика), компонентов каждого вида во всех состояниях; фикстуры в плашке валюты: `0`, `50`, `99 999`; карточка с очень длинным названием (80 символов) и описанием (300 символов).
- [ ] **Step 3: Serve and open.** `task mockups`, открыть `http://localhost:8080/shell/kit.html`, в devtools выполнить `shellAudit()` на ширине 360 и 600. Expected: `smallTargets: []`, `overflow: []`, `hScroll: false`.
- [ ] **Step 4: Run `check.py`.** Expected: `ok`.
- [ ] **Step 5: Commit** `feat(mockups): kit catalog page`.

### Task 4: Главная и панель (`home.html`, `home-panel.html`)

**Files:**
- Create: `shell/home.html`, `shell/home-panel.html`
- Modify: `shell/shell.js` (`initTabs`, `initPanel`, `initChips`)

**Interfaces:**
- Produces: `initTabs(root)`: переключает `aria-current` на `.tab`; `initPanel(button, panel)`: открывает/закрывает `.sheet` по клику и Esc; `initChips(root)`: один активный `.chip` в ряду.

- [ ] **Step 1: Build `home.html`** по `shell-2-home.png`/`shell-8-home-2.png`: сверху `.pill-energy`, `.pill-gem`, кнопка шестерни; карусель карточек историй (прокрутка по горизонтали, точки, бейдж «Рекомендуем», кнопка перемешать, жанр, описание, «Открыть»); поиск «Истории, авторы…»; ряд чипов («Для начала», «Новые», «Популярные», «По жанрам»); сетка карточек; нижняя `.tabbar` из 5 пунктов (Библиотека, Анкета, Чат, Образы, Награды) с бейджами.
- [ ] **Step 2: Build `home-panel.html`** по `shell-3-home-panel.png`: та же главная, `.sheet` над навигацией с тремя строками (реклама +3 💎 с таймером, награды, распродажа неактивна).
- [ ] **Step 3: Verify** в браузере: тапы по табам и чипам, открытие панели, `shellAudit()` на 360 и 600. Expected: пусто / `hScroll: false`.
- [ ] **Step 4: Run `check.py`.** Expected: `ok`. Commit `feat(mockups): home screens`.

### Task 5: Награды и магазин (`rewards.html`, `shop.html`)

**Files:**
- Create: `shell/rewards.html`, `shell/shop.html`
- Modify: `shell/shell.js` (`initSegmented`, `initCountdown`)

**Interfaces:**
- Produces: `initSegmented(root)`: переключает панели `[data-panel]` по `.segmented [data-target]`; `initCountdown(el, seconds)`: тикает `ЧЧ:ММ:СС`, по нулю включает кнопку.

- [ ] **Step 1: Build `rewards.html`** по `shell-1-rewards.png`: заголовок экрана с крестиком; три карточки награды (ежедневная энергия с таймером, реклама за 💎 с замком и «Смотреть рекламу», ежедневный вход «Получить»), четвёртая по решению исполнителя (см. spec).
- [ ] **Step 2: Build `shop.html`** по `shell-4-shop-gems.png`/`shell-6-shop-tea.png`: плашки валют, карточка «Для начинающих», `.segmented` «Энергия / Алмазы» с двумя панелями, «Лучшая выгода» и две малые карточки на панель, нижняя панель «Назад / Магазин / Покупки».
- [ ] **Step 3: Verify** переключение панелей, таймер, `shellAudit()` на 360 и 600.
- [ ] **Step 4: Run `check.py`.** Commit `feat(mockups): rewards and shop`.

### Task 6: Настройки (`settings.html`)

**Files:**
- Create: `shell/settings.html`
- Modify: `shell/shell.js` (`initSwitches`, `initSteppers`)

**Interfaces:**
- Produces: `initSwitches(root)`: `.switch[role=switch]` меняет `aria-checked` по клику и пробелу; `initSteppers(root)`: `.stepper` листает значения из `data-values` по `‹ ›`.

- [ ] **Step 1: Build `settings.html`** по `shell-5-settings.png`: модалка с крестиком; блок тумблеров (уведомления, звук) и степперов (язык, тема, приветствие); блок входа (имя/пароль, Google, Apple) с текстовыми кнопками без брендовой графики; ряд ссылок на соцсети как нейтральные круглые кнопки с буквами; список «Поддержка, Конфиденциальность, Соглашение, Инструменты».
- [ ] **Step 2: Verify** переключение тумблеров и степперов с клавиатуры, `shellAudit()`.
- [ ] **Step 3: Run `check.py`.** Commit `feat(mockups): settings`.

### Task 7: Колесо фортуны (`wheel.html`)

**Files:**
- Create: `shell/wheel.html`
- Modify: `shell/shell.js` (`initWheel`)

**Interfaces:**
- Produces: `initWheel(root, prizes: {id: string, weight: number}[]): void`: по «Вращать» выбирает приз по весам, крутит колесо CSS-трансформом 3–4 секунды и показывает `.toast` с призом; при `prefers-reduced-motion: reduce` показывает результат сразу.

- [ ] **Step 1: Build `wheel.html`** по `shell-7-wheel.png`: заголовок, два ряда призов с шансами (10 призов), колесо из 10 секторов цветами `--art-*`, указатель сверху, счётчик попыток с «+», кнопки «Вращать колесо» и «Усилить шансы за 150 💎».
- [ ] **Step 2: Implement `initWheel`.** Выбор по весам делает функция `pick(prizes) -> id`; в самой странице вывод в консоль 1000 розыгрышей не нужен, достаточно визуальной проверки.
- [ ] **Step 3: Verify** вращение, остановка на выбранном секторе, режим reduced-motion (эмуляция в devtools), `shellAudit()`.
- [ ] **Step 4: Run `check.py`.** Commit `feat(mockups): wheel of fortune`.

### Task 8: ИИ-чат, главы, библиотека (`chat.html`, `chapters.html`, `library.html`)

**Files:**
- Create: `shell/chat.html`, `shell/chapters.html`, `shell/library.html`
- Modify: `shell/shell.js` (`initChatStates`)

**Interfaces:**
- Produces: `initChatStates(root)`: кнопки `[data-state]` над макетом переключают `.chat[data-state]` между `normal`, `empty`, `long`, `limit`; видимость блоков по состоянию задаёт CSS.

- [ ] **Step 1: Build `chat.html`:** шапка (назад, аватар-заглушка и имя ГГ, настроение «Тревожится», ⚑); поток сообщений (ГГ слева, читатель справа, статус «печатает…»); чипы-подсказки; поле ввода с «Отправить»; под ним пометка «Вымышленный диалог по мотивам истории» и счётчик «7/10». Состояния: `empty` (приветственная реплика и чипы), `long` (длинная реплика, перенос), `limit` (поле неактивно, кнопка «Продолжить за 💎»).
- [ ] **Step 2: Build `chapters.html`** (обложка истории, прогресс, список глав со статусами «пройдена / идёт / новая / закрыта» и замком) и **`library.html`** (поиск, список терминов, раскрытая карточка термина).
- [ ] **Step 3: Verify** переключение состояний чата, `shellAudit()` на 360 и 600.
- [ ] **Step 4: Run `check.py`.** Commit `feat(mockups): chat, chapters, library`.

### Task 9: Индекс и финальная проверка

**Files:**
- Create: `shell/index.html`

- [ ] **Step 1: Build `index.html`:** ссылка на `kit.html`; девять экранов в `iframe` 360×780 с масштабом `.62`, подписями и ссылками на полные страницы; ряды перестраиваются без горизонтальной прокрутки на 360 и 600.
- [ ] **Step 2: Run `check.py`.** Expected: `ok`, мёртвых ссылок нет.
- [ ] **Step 3: Visual pass in Chrome:** индекс на 1280, 600 и 360; `shellAudit()` на каждом экране; убедиться, что ни одна страница не зелёно-красная, без орнамента и пергамента. Выписать расхождения со spec и поправить.
- [ ] **Step 4: Add a link to `shell/index.html`** строкой в таблицу макетов в `docs/brainstorm/2026-10-01-interface.md`. Commit `feat(mockups): shell index`.

---

## Self-Review

- **Spec coverage:** кит (Task 1–3), 9 экранов (Task 4–8), индекс и ширина ~600 (Task 9), токены и `--art-*` (Task 1, `check.py`), чат под новый замысел (Task 8), живость (Task 4–8). Блок «Для начинающих» и нижний блок наград по решению исполнителя (Task 5).
- **Types:** имена функций `shell.js` совпадают между задачами и экранами (`initTabs`, `initPanel`, `initChips`, `initSegmented`, `initCountdown`, `initSwitches`, `initSteppers`, `initWheel`, `initChatStates`, `shellAudit`).
- **Proportion:** план короче половины кода, который он описывает; тела функций оставлены исполнителю.
