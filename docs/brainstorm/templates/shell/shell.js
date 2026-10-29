// Поведение макетов оболочки на чистом JS. Подключается в конце <body>, всё включается по атрибутам.
// Проверка раскладки в браузере: shellAudit() в консоли.

const $$ = (sel, root = document) => [...root.querySelectorAll(sel)];
if (self !== top) document.documentElement.classList.add('embed');   // в индексе рамки без полей

const token = name => parseFloat(getComputedStyle(document.documentElement).getPropertyValue(name));   // токены длительности в мс
const still = () => matchMedia('(prefers-reduced-motion: reduce)').matches;
const readNum = el => Number(el.textContent.replace(/\s/g, '')) || 0;
const fmt = n => n.toLocaleString('ru-RU');

// Число плавно меняется до нового значения (баланс валюты).
function countTo(el, to, ms = token('--dur-slow')) {
  const from = readNum(el);
  if (still() || from === to) { el.textContent = fmt(to); return; }
  const t0 = performance.now();
  const step = now => {
    const p = Math.min(1, (now - t0) / ms);
    el.textContent = fmt(Math.round(from + (to - from) * (1 - Math.pow(1 - p, 3))));
    if (p < 1) requestAnimationFrame(step);
  };
  requestAnimationFrame(step);
}

// Тост с появлением и уходом; повторный вызов продлевает показ.
const toastTimers = new WeakMap();
function showToast(el, text, ms = 2500) {
  if (text) el.textContent = text;
  clearTimeout(toastTimers.get(el));
  el.classList.remove('out'); el.hidden = false;
  toastTimers.set(el, setTimeout(() => {
    el.classList.add('out');
    el.addEventListener('animationend', () => { el.hidden = true; el.classList.remove('out'); }, { once: true });
  }, ms));
}

function initTabs(root) {
  const tabs = $$('.tab', root);
  tabs.forEach(t => t.addEventListener('click', () => {
    tabs.forEach(o => o.removeAttribute('aria-current'));
    t.setAttribute('aria-current', 'page');
  }));
}

function initPanel(button, panel) {
  const set = open => { panel.hidden = !open; button.setAttribute('aria-expanded', String(open)); };
  button.addEventListener('click', () => set(panel.hidden));
  document.addEventListener('click', e => { if (!panel.hidden && !panel.contains(e.target) && !button.contains(e.target)) set(false); });
  document.addEventListener('keydown', e => { if (e.key === 'Escape') set(false); });
  set(!panel.hidden);
}

function initCarousel(root) {
  const slides = $$('.hero', root);
  const dots = $$('.dots i', document.querySelector(root.dataset.dots));
  const center = i => { root.scrollLeft = slides[i].offsetLeft - (root.clientWidth - slides[i].offsetWidth) / 2; };
  const mark = () => {
    const mid = root.scrollLeft + root.clientWidth / 2;
    const near = slides.reduce((b, s, i) => Math.abs(s.offsetLeft + s.offsetWidth / 2 - mid) < Math.abs(slides[b].offsetLeft + slides[b].offsetWidth / 2 - mid) ? i : b, 0);
    dots.forEach((d, i) => d.classList.toggle('on', i === near));
    slides.forEach((s, i) => { s.classList.toggle('on', i === near); s.classList.toggle('before', i < near); s.classList.toggle('after', i > near); });
  };
  root.addEventListener('scroll', mark, { passive: true });
  center(Number(root.dataset.start ?? 0)); mark();
}

function initChips(root) {
  const chips = $$('.chip', root);
  chips.forEach(c => c.addEventListener('click', () => chips.forEach(o => o.setAttribute('aria-pressed', String(o === c)))));
}

function initSegmented(root) {
  const buttons = $$('button', root);
  root.style.setProperty('--n', buttons.length);
  const show = id => {
    root.style.setProperty('--i', buttons.findIndex(b => b.dataset.target === id));
    buttons.forEach(b => b.setAttribute('aria-selected', String(b.dataset.target === id)));
    $$('[data-panel]').forEach(p => { p.hidden = p.dataset.panel !== id; });
  };
  buttons.forEach(b => b.addEventListener('click', () => show(b.dataset.target)));
}

function initCountdown(el, seconds) {
  const label = el.querySelector('time');
  const action = el.closest('.foot')?.querySelector('[data-after-timer]');
  let left = location.search.includes('fast') ? Math.min(seconds, 5) : seconds;   // ?fast: таймеры по 5 секунд для показа
  const pad = n => String(n).padStart(2, '0');
  const draw = () => { label.textContent = `${pad(Math.floor(left / 3600))}:${pad(Math.floor(left % 3600 / 60))}:${pad(left % 60)}`; };
  draw();
  const id = setInterval(() => {
    left = Math.max(0, left - 1); draw();
    if (left === 0) {
      clearInterval(id);
      if (action) { action.disabled = false; action.classList.add('ready'); el.closest('.foot').querySelector('.btn-icon')?.setAttribute('hidden', ''); }
    }
  }, 1000);
}

function saved() { const t = document.getElementById('saved'); if (t) showToast(t, 'Сохранено', 1500); }

function initSwitches(root) {
  $$('.switch', root).forEach(s => {
    const flip = () => { s.setAttribute('aria-checked', String(s.getAttribute('aria-checked') !== 'true')); saved(); };
    s.addEventListener('click', flip);
  });
}

function initSteppers(root) {
  $$('.stepper[data-values]', root).forEach(st => {
    const values = st.dataset.values.split('|');
    const out = st.querySelector('output');
    let i = Math.max(0, values.indexOf(out.textContent));
    const [prev, next] = $$('button', st);
    const move = d => { i = (i + d + values.length) % values.length; out.textContent = values[i]; saved(); };
    prev.addEventListener('click', () => move(-1));
    next.addEventListener('click', () => move(1));
  });
}

function pick(prizes) {
  let r = Math.random() * prizes.reduce((s, p) => s + p.weight, 0);
  for (const p of prizes) { if ((r -= p.weight) < 0) return p.id; }
  return prizes[prizes.length - 1].id;
}

function initWheel(root, prizes) {
  const disc = root.querySelector('.wheel-disc');
  const spin = root.querySelector('[data-spin]');
  const toast = root.querySelector('.toast');
  const counters = $$('[data-attempts]', root);
  const seg = 360 / prizes.length;
  const still = matchMedia('(prefers-reduced-motion: reduce)').matches;
  let turns = 0, attempts = Number(root.dataset.attempts ?? 1);
  const draw = () => {
    counters.forEach(c => { c.textContent = String(attempts); });
    spin.disabled = attempts === 0;
  };
  const done = id => {
    showToast(toast, 'Выпало: ' + prizes.find(p => p.id === id).label); draw();
  };
  draw();
  spin.addEventListener('click', () => {
    if (attempts === 0) return;
    const id = pick(prizes);
    const index = prizes.findIndex(p => p.id === id);
    turns += 4; attempts -= 1;
    const angle = turns * 360 - index * seg;
    spin.disabled = true;
    disc.style.transition = still ? 'none' : 'transform var(--dur-spin) var(--ease-wheel)';
    disc.style.transform = `rotate(${angle}deg)`;
    if (still) done(id); else disc.addEventListener('transitionend', () => done(id), { once: true });
  });
}

document.addEventListener('keydown', e => { if (e.key === 'Escape' && document.querySelector('dialog.popup[open]')) location.href = 'home.html'; });

function initToastLinks() {
  $$('[data-toast]').forEach(b => {
    const toast = document.getElementById(b.dataset.toast);
    b.addEventListener('click', () => showToast(toast, null, 4000));
  });
}

// Награда: тап -> загрузка -> «Получено» и тост.
function initClaim() {
  const toast = document.getElementById('claim-toast');
  $$('[data-claim]').forEach(b => b.addEventListener('click', () => {
    b.classList.remove('ready'); b.classList.add('is-loading');
    setTimeout(() => {
      b.classList.remove('is-loading'); b.textContent = 'Получено'; b.disabled = true;
      showToast(toast, b.dataset.claim);
    }, still() ? 0 : token('--dur-net') * .7);
  }));
}

// Магазин: тап по цене -> загрузка -> баланс растёт, тост.
function initPurchase() {
  const toast = document.getElementById('shop-toast');
  const names = { gem: ['алмаз', 'алмаза', 'алмазов'], energy: ['энергия', 'энергии', 'энергии'] };
  const word = (kind, n) => { const w = names[kind]; return n % 10 === 1 && n % 100 !== 11 ? w[0] : (n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20)) ? w[1] : w[2]; };
  $$('.btn-price[data-gain]').forEach(b => b.addEventListener('click', () => {
    b.classList.add('is-loading');
    setTimeout(() => {
      b.classList.remove('is-loading');
      const gains = b.dataset.gain.split(' ').map(g => g.split(':'));
      gains.forEach(([kind, n]) => {
        const pill = document.querySelector('.pill-' + kind), value = pill.querySelector('span');
        countTo(value, readNum(value) + Number(n));
        pill.classList.remove('bump'); void pill.offsetWidth; pill.classList.add('bump');
      });
      showToast(toast, 'Зачислено: ' + gains.map(([kind, n]) => `${n} ${word(kind, Number(n))}`).join(', '));
    }, still() ? 0 : token('--dur-net'));
  }));
}

// Чат: отправка активна только при тексте; ответ героя приходит после «печатает…».
function initChat() {
  const chat = document.querySelector('.chat'); if (!chat) return;
  const msgs = chat.querySelector('.msgs');
  const replies = ['Не знаю. Об этом я ещё никому не говорила.', 'Может быть. Но иногда страх сильнее слов.', 'Ты говоришь так, будто знаешь меня давно.'];
  let n = 0;
  const add = (cls, html) => { const d = document.createElement('div'); d.className = 'bubble ' + cls; d.innerHTML = html; msgs.append(d); msgs.scrollTop = msgs.scrollHeight; return d; };
  $$('.chat-input', chat).forEach(form => {
    const input = form.querySelector('input'), send = form.querySelector('.send');
    const sync = () => { send.disabled = input.disabled || !input.value.trim(); };
    const submit = () => {
      const text = input.value.trim(); if (!text || send.disabled) return;
      const me = add('me', ''); me.textContent = text;
      input.value = ''; sync();
      msgs.querySelectorAll('.typing-bubble').forEach(t => t.remove());
      const typing = add('them typing-bubble', '<span class="typing" aria-label="Печатает"><i></i><i></i><i></i></span>');
      setTimeout(() => { typing.classList.remove('typing-bubble'); typing.textContent = replies[n++ % replies.length]; msgs.scrollTop = msgs.scrollHeight; }, still() ? 0 : token('--dur-net') * 1.4);
    };
    sync();
    input.addEventListener('input', sync);
    send.addEventListener('click', submit);
    input.addEventListener('keydown', e => { if (e.key === 'Enter') submit(); });
  });
}

// Поиск: крестик очистки, Esc, фильтр по списку и пустой результат.
function initSearch() {
  $$('.search').forEach(s => {
    const input = s.querySelector('input'), clear = s.querySelector('.clear');
    const empty = s.dataset.empty && document.getElementById(s.dataset.empty);
    const apply = () => {
      const q = input.value.trim().toLowerCase();
      if (clear) clear.hidden = !q;
      if (!s.dataset.filter) return;
      let shown = 0;
      $$(s.dataset.filter).forEach(el => { const hit = !q || el.textContent.toLowerCase().includes(q); el.hidden = !hit; if (hit) shown++; });
      if (empty) empty.hidden = shown > 0;
    };
    input.addEventListener('input', apply);
    input.addEventListener('keydown', e => { if (e.key === 'Escape') { input.value = ''; apply(); } });
    clear?.addEventListener('click', () => { input.value = ''; apply(); input.focus(); });
  });
}

// Карусель: «случайная история» крутит иконку и листает; тап по боковой карточке выводит её в центр.
function initShuffle() {
  const btn = document.querySelector('.shuffle'), c = document.querySelector('.carousel'); if (!btn || !c) return;
  const slides = $$('.hero', c);
  const go = i => c.scrollTo({ left: slides[i].offsetLeft - (c.clientWidth - slides[i].offsetWidth) / 2, behavior: still() ? 'auto' : 'smooth' });
  btn.addEventListener('click', () => {
    const cur = slides.findIndex(s => s.classList.contains('on'));
    let next; do { next = Math.floor(Math.random() * slides.length); } while (next === cur);
    go(next);
    btn.classList.remove('spun'); void btn.offsetWidth; btn.classList.add('spun');
  });
  slides.forEach((s, i) => s.addEventListener('click', e => { if (!s.classList.contains('on')) { e.preventDefault(); go(i); } }));
}

// Списки появляются по очереди.
function initStagger() { $$('[data-stagger]').forEach(p => [...p.children].slice(0, 8).forEach((ch, i) => ch.style.setProperty('--n', i))); }

// Состояния экрана для макета: кнопки [data-set] переключают блоки [data-s="..."]; [data-retry] имитирует повтор запроса.
function setScreenState(root, state) {
  $$('[data-set]', root).forEach(b => b.setAttribute('aria-pressed', String(b.dataset.set === state)));
  $$('[data-s]', root).forEach(el => { el.hidden = !el.dataset.s.split(' ').includes(state); });
}
function initScreenStates() {
  $$('[data-screen-demo]').forEach(root => {
    const buttons = $$('[data-set]', root);
    buttons.forEach(b => b.addEventListener('click', () => setScreenState(root, b.dataset.set)));
    if (buttons[0]) setScreenState(root, buttons[0].dataset.set);
    $$('[data-retry]', root).forEach(b => b.addEventListener('click', () => {
      b.classList.add('is-loading');
      setTimeout(() => { b.classList.remove('is-loading'); setScreenState(root, 'normal'); }, still() ? 0 : token('--dur-net'));
    }));
  });
}

function initChatStates(root) {
  const chat = root.querySelector('.chat');
  $$('[data-state-btn]', root).forEach(b => b.addEventListener('click', () => {
    chat.dataset.state = b.dataset.stateBtn;
    $$('[data-state-btn]', root).forEach(o => o.setAttribute('aria-pressed', String(o === b)));
  }));
}

function selector(el) {
  return el.tagName.toLowerCase() + (el.className && typeof el.className === 'string' ? '.' + el.className.trim().split(/\s+/).join('.') : '')
    + (el.textContent.trim() ? ` "${el.textContent.trim().slice(0, 24)}"` : '');
}

// Касания меньше 44px, переполнение внутри рамок, горизонтальная прокрутка страницы.
function shellAudit() {
  const visible = el => el.getClientRects().length > 0 && getComputedStyle(el).visibility !== 'hidden';
  const smallTargets = $$('.device button, .device a[href], .device input, .device [role=switch], .device [role=tab]')
    .filter(visible).filter(el => { return !el.matches('.card-reward .foot .btn, .search input, .search .clear') && (el.offsetWidth < 43.5 || el.offsetHeight < 43.5); })   // кнопки наград и поиск: 36px видимых, зона нажатия 44 через ::after   // размеры раскладки, без учёта scale у боковых карточек
    .map(selector);
  const overflow = $$('.device *').filter(visible).filter(el => {
    const o = getComputedStyle(el).overflowX;
    return el.scrollWidth > el.clientWidth + 1 && o !== 'auto' && o !== 'scroll' && el.clientWidth > 0 && !(el instanceof SVGElement) && !el.closest('[data-audit-skip]') && getComputedStyle(el).textOverflow !== 'ellipsis';   // вращающийся круг: рамка повёрнутого квадрата шире круга
  }).map(selector);
  return { smallTargets, overflow, hScroll: document.documentElement.scrollWidth > document.documentElement.clientWidth };
}

document.addEventListener('DOMContentLoaded', () => {
  $$('.tabbar').forEach(initTabs);
  $$('.chips').filter(c => !c.closest('.chat')).forEach(initChips);   // подсказки чата не фильтр, подсветка им не нужна
  $$('.segmented').forEach(initSegmented);
  $$('.carousel[data-dots]').forEach(initCarousel);
  $$('.timer[data-seconds]').forEach(t => initCountdown(t, Number(t.dataset.seconds)));
  initSwitches(document);
  initSteppers(document);
  $$('[data-sheet]').forEach(b => initPanel(b, document.getElementById(b.dataset.sheet)));
  $$('[data-wheel]').forEach(w => initWheel(w, JSON.parse(w.dataset.wheel)));
  $$('[data-chat-demo]').forEach(initChatStates);
  initToastLinks(); initClaim(); initPurchase(); initChat(); initSearch(); initShuffle(); initStagger(); initScreenStates();
});
