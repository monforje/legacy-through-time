// Общие компоненты экранов игровой сессии: разметка, русская типографика и поведение.
// Подключают session-screens.html (все экраны рядом) и prototype.html (сквозной проход).
  // Русская типографика: предлоги и союзы до двух букв и тире не отрываются от слова.
  const nb = t => t.replace(/(?<=^|[\s(«„])([А-Яа-яЁё]{1,2})\s/g, '$1\u00a0').replace(/ — /g, '\u00a0— ');
  const label = (text, side = 'center') => `<span class="label ${side}">${text}</span>`;
  const plate = (lbl, text, extra = '', cls = '') => `<div class="plate ${cls}">${lbl}${extra}${nb(text)}</div>`;
  const opts = list => `<div class="options">${list.map(o => `<div class="opt">${nb(o)}</div>`).join('')}</div>`;
  const stage = (...parts) => `<div class="stage">${parts.join('')}</div>`;
  const mirrorStage = (...parts) => `<div class="stage mirror">${parts.join('')}</div>`;
  // Значок валюты: страница может подменить эмодзи своей иконкой (window.GEM_ICON)
  const gemIcon = () => window.GEM_ICON || '💎';
  const gem = n => `<span class="cost">${n} ${gemIcon()}</span>`;

  const LEFT = (name, text, extra) => plate(label(name, 'left'), text, extra, 'tail-r');
  const RIGHT = (name, text, extra) => plate(label(name, 'right'), text, extra, 'tail-l');

  const START_GEMS = 20;
  const END = {
    title: 'Конец',
    note: 'конец прототипа',
    body: `<div class="dim"></div><div class="title"><span>Конец фрагмента</span><span>Тап — сначала</span></div>`,
  };
  const $ = (el, sel) => el.querySelector(sel);

  // Один экран: состояние в замыкании, разметка перерисовывается при сбросе.
  const REDUCED = matchMedia('(prefers-reduced-motion: reduce)').matches;
  // Короткие анимации по месту (Web Animations): листание значения, всплеск баланса.
  const play = (el, keyframes, ms = 260) => { if (el && !REDUCED) el.animate(keyframes, { duration: ms, easing: 'cubic-bezier(.22, 1, .36, 1)' }); };

  // Текст плашки разбивается на слова, чтобы проявляться по очереди.
  // Возвращает число слов. Пробелы внутри nb() (неразрывные) не режутся.
  function splitWords(plate) {
    let n = 0;
    [...plate.childNodes].filter(x => x.nodeType === 3 && x.textContent.trim()).forEach(node => {
      // плашка — flex-контейнер: без общей обёртки каждое слово стало бы отдельным flex-элементом
      const frag = document.createElement('span');
      node.textContent.split(' ').forEach((word, k, all) => {
        const span = document.createElement('span');
        span.className = 'w';
        span.style.setProperty('--i', n++);
        span.textContent = word + (k < all.length - 1 ? ' ' : '');
        frag.append(span);
      });
      node.replaceWith(frag);
    });
    return n;
  }

  // opts.bank — общий кошелёк между экранами; opts.onNext — «идём дальше» (прототип);
  // opts.onGems — подписка на изменение баланса.
  function mount(dev, s, opts = {}) {
    const st = { i: 0, locked: false };
    const bank = opts.bank || { gems: START_GEMS };
    let stopTimer = null, autoHide = 0, nextT = 0, revealT = 0, enterT = 0, shownGems = null;

    const shake = el => { el.classList.remove('shake'); void el.offsetWidth; el.classList.add('shake'); };

    const syncGems = () => {
      const b = $(dev, '.gems .bal');
      if (b) {
        b.textContent = bank.gems;
        if (shownGems !== null && shownGems !== bank.gems) play(b, [{ scale: 1.5, opacity: .5 }, { scale: 1, opacity: 1 }], 380);
      }
      shownGems = bank.gems;
      opts.onGems?.(bank.gems);
    };
    const next = delay => { if (opts.onNext) nextT = setTimeout(opts.onNext, delay); };
    const syncPicker = dir => {
      if (!s.values) return;
      const v = s.values[st.i];
      const value = $(dev, '.picker .value');
      value.innerHTML = nb(v.t);
      $(dev, '.picker .pick .cost').innerHTML = v.cost ? `${v.cost} ${gemIcon()}` : '';
      if (dir) play(value, [{ opacity: 0, translate: `${dir * 22}px 0` }, { opacity: 1, translate: '0 0' }]);
    };
    const spend = (cost, el) => {
      if (cost > bank.gems) { shake(el); return false; }
      bank.gems -= cost; syncGems(); return true;
    };
    const lock = chosen => {
      st.locked = true;
      stopTimer?.();
      dev.querySelectorAll('.opt').forEach(o => o.classList.add(o === chosen ? 'chosen' : 'rejected'));
      $(dev, '.picker')?.classList.add('locked');
    };
    const price = el => +(($(el, '.cost')?.textContent.match(/\d+/) || [0])[0]);

    function runTimer(el, seconds) {
      const t0 = performance.now();
      let raf = requestAnimationFrame(function tick(now) {
        const left = 1 - (now - t0) / (seconds * 1000);
        el.style.setProperty('--p', Math.max(left, 0) * 100 + '%');
        if (left < 3 / seconds) el.classList.add('urgent');
        if (left > 0) { raf = requestAnimationFrame(tick); return; }
        el.classList.remove('urgent');
        lock(null);
        next(0);
      });
      stopTimer = () => cancelAnimationFrame(raf);
    }

    // Набор текста: пока идёт, варианты и кнопки неактивны; тап показывает всё сразу.
    function finishReveal() {
      clearTimeout(revealT);
      st.revealing = false;
      dev.classList.remove('revealing');
      dev.classList.add('skip');
      showOptions();
    }
    // Варианты выезжают каскадом (см. .opts-pending / .opts-enter в skeleton.css)
    function showOptions() {
      if (!dev.classList.contains('opts-pending')) return;
      dev.classList.add('opts-enter');
      dev.classList.remove('opts-pending');
      enterT = setTimeout(() => dev.classList.remove('opts-enter'), 800);
    }

    function start() {
      stopTimer?.(); clearTimeout(autoHide); clearTimeout(nextT); clearTimeout(revealT); clearTimeout(enterT);
      if (!opts.bank) bank.gems = START_GEMS;
      Object.assign(st, { i: 0, locked: false, revealing: false });
      shownGems = null;
      dev.classList.remove('revealing', 'skip', 'opts-pending', 'opts-enter');
      dev.innerHTML = s.body;
      dev.querySelectorAll('.opt, .arrow, .pick, .collapse, .plus, .stat, .info').forEach(el => {
        el.setAttribute('role', 'button'); el.tabIndex = 0;
      });
      $(dev, '.arrow.prev')?.setAttribute('aria-label', 'Предыдущий вариант');
      $(dev, '.arrow.next')?.setAttribute('aria-label', 'Следующий вариант');
      $(dev, '.collapse')?.setAttribute('aria-label', 'Свернуть или развернуть блок');
      syncGems(); syncPicker();
      dev.querySelectorAll('.opt').forEach((o, k) => o.style.setProperty('--k', k));
      // блоки выбора заезжают снизу тем же движением, что и «свернуть»
      const panel = $(dev, '.picker, .choice-panel');
      if (panel && !REDUCED) {
        panel.classList.add('collapsed');
        setTimeout(() => panel.classList.remove('collapsed'), 120);
      }
      const plate = $(dev, '.plate');
      if (!REDUCED && $(dev, '.opt')) {
        dev.classList.add('opts-pending');
        if (!plate) enterT = setTimeout(showOptions, 350);   // блок выбора без плашки: после заезда панели
      }
      if (plate && !REDUCED) {
        const words = splitWords(plate);
        const total = words * 26 + 300 + 240;
        st.revealing = true;
        dev.classList.add('revealing');
        revealT = setTimeout(finishReveal, Math.min(total, 1800));
      }
      const stat = $(dev, '.stat');
      if (stat) autoHide = setTimeout(() => stat.classList.add('gone'), 3500);
      const timer = $(dev, '.timer');
      if (timer) runTimer(timer, s.seconds || 8);
    }

    dev.addEventListener('click', e => {
      const t = e.target;
      let el;
      if (st.revealing) { finishReveal(); return; }
      if ((el = t.closest('.collapse'))) { el.parentElement.classList.toggle('collapsed'); return; }
      if ((el = t.closest('.plus'))) { bank.gems += 10; syncGems(); return; }
      if ((el = t.closest('.arrow'))) {
        const n = s.values.length;
        st.i = (st.i + (el.classList.contains('next') ? 1 : -1) + n) % n;
        syncPicker(el.classList.contains('next') ? 1 : -1); return;
      }
      if ((el = t.closest('.pick'))) {
        const v = s.values[st.i];
        if (v.cost && !spend(v.cost, el)) return;
        lock(null); el.classList.add('chosen'); next(150); return;
      }
      if ((el = t.closest('.opt'))) {
        const cost = price(el);
        if (cost && !spend(cost, el)) return;
        lock(el); next(150); return;
      }
      if ((el = t.closest('.stat'))) { el.classList.add('gone'); return; }
      if ((el = t.closest('.info'))) { el.classList.add('gone'); return; }
      if ($(dev, '.title')) {
        if (opts.onNext) { opts.onNext(); return; }
        const gone = $(dev, '.title').classList.toggle('gone');
        $(dev, '.dim').classList.toggle('gone', gone);
        return;
      }
      if (t.closest('.plate') && !opts.onNext) {
        const info = $(dev, '.info');
        if (info?.classList.contains('gone')) { info.classList.remove('gone'); return; }
      }
      if (!$(dev, '.opt, .picker')) { opts.onNext?.(); }
    });
    dev.addEventListener('keydown', e => {
      if ((e.key === 'Enter' || e.key === ' ') && e.target.matches('[role="button"]')) { e.preventDefault(); e.target.click(); }
    });

    start.stop = () => { stopTimer?.(); clearTimeout(autoHide); clearTimeout(nextT); clearTimeout(revealT); clearTimeout(enterT); };
    start();
    return start;
  }
