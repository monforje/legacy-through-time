// Поведение макетов оболочки на чистом JS. Подключается в конце <body>, всё включается по атрибутам.
// Проверка раскладки в браузере: shellAudit() в консоли.

const $$ = (sel, root = document) => [...root.querySelectorAll(sel)];
if (self !== top) document.documentElement.classList.add('embed');   // в индексе рамки без полей

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

function initChips(root) {
  const chips = $$('.chip', root);
  chips.forEach(c => c.addEventListener('click', () => chips.forEach(o => o.setAttribute('aria-pressed', String(o === c)))));
}

function initSegmented(root) {
  const buttons = $$('button', root);
  const show = id => {
    buttons.forEach(b => b.setAttribute('aria-selected', String(b.dataset.target === id)));
    $$('[data-panel]').forEach(p => { p.hidden = p.dataset.panel !== id; });
  };
  buttons.forEach(b => b.addEventListener('click', () => show(b.dataset.target)));
}

function initCountdown(el, seconds) {
  const label = el.querySelector('time');
  const action = el.closest('.foot')?.querySelector('[data-after-timer]');
  let left = seconds;
  const pad = n => String(n).padStart(2, '0');
  const draw = () => { label.textContent = `${pad(Math.floor(left / 3600))}:${pad(Math.floor(left % 3600 / 60))}:${pad(left % 60)}`; };
  draw();
  const id = setInterval(() => {
    left = Math.max(0, left - 1); draw();
    if (left === 0) { clearInterval(id); if (action) action.disabled = false; }
  }, 1000);
}

function initSwitches(root) {
  $$('.switch', root).forEach(s => {
    const flip = () => s.setAttribute('aria-checked', String(s.getAttribute('aria-checked') !== 'true'));
    s.addEventListener('click', flip);
  });
}

function initSteppers(root) {
  $$('.stepper[data-values]', root).forEach(st => {
    const values = st.dataset.values.split('|');
    const out = st.querySelector('output');
    let i = Math.max(0, values.indexOf(out.textContent));
    const [prev, next] = $$('button', st);
    const move = d => { i = (i + d + values.length) % values.length; out.textContent = values[i]; };
    prev.addEventListener('click', () => move(-1));
    next.addEventListener('click', () => move(1));
  });
}

function pick(prizes) {
  let r = Math.random() * prizes.reduce((s, p) => s + p.weight, 0);
  for (const p of prizes) { if ((r -= p.weight) < 0) return p.id; }
  return prizes[prizes.length - 1].id;
}

const plural = (n, [one, few, many]) => n % 10 === 1 && n % 100 !== 11 ? one : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? few : many;

function initWheel(root, prizes) {
  const disc = root.querySelector('.wheel-disc');
  const spin = root.querySelector('[data-spin]');
  const toast = root.querySelector('.toast');
  const counter = root.querySelector('[data-attempts]');
  const seg = 360 / prizes.length;
  const still = matchMedia('(prefers-reduced-motion: reduce)').matches;
  let turns = 0, attempts = Number(root.dataset.attempts ?? 1), hideTimer;
  const draw = () => {
    counter.textContent = `${attempts} ${plural(attempts, ['попытка', 'попытки', 'попыток'])}`;
    spin.disabled = attempts === 0;
  };
  const done = id => {
    toast.textContent = 'Выпало: ' + prizes.find(p => p.id === id).label;
    toast.hidden = false; draw();
    clearTimeout(hideTimer);
    hideTimer = setTimeout(() => { toast.hidden = true; }, 2500);
  };
  draw();
  spin.addEventListener('click', () => {
    if (attempts === 0) return;
    const id = pick(prizes);
    const index = prizes.findIndex(p => p.id === id);
    turns += 4; attempts -= 1;
    const angle = turns * 360 - index * seg;
    spin.disabled = true;
    disc.style.transition = still ? 'none' : 'transform 3.6s cubic-bezier(.12,.7,.15,1)';
    disc.style.transform = `rotate(${angle}deg)`;
    if (still) done(id); else disc.addEventListener('transitionend', () => done(id), { once: true });
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
    .filter(visible).filter(el => { const r = el.getBoundingClientRect(); return r.width < 43.5 || r.height < 43.5; })
    .map(selector);
  const overflow = $$('.device *').filter(visible).filter(el => {
    const o = getComputedStyle(el).overflowX;
    return el.scrollWidth > el.clientWidth + 1 && o !== 'auto' && o !== 'scroll' && el.clientWidth > 0 && !(el instanceof SVGElement) && !el.closest('[data-audit-skip]');   // вращающийся круг: рамка повёрнутого квадрата шире круга
  }).map(selector);
  return { smallTargets, overflow, hScroll: document.documentElement.scrollWidth > document.documentElement.clientWidth };
}

document.addEventListener('DOMContentLoaded', () => {
  $$('.tabbar').forEach(initTabs);
  $$('.chips').filter(c => !c.closest('.chat')).forEach(initChips);   // подсказки чата не фильтр, подсветка им не нужна
  $$('.segmented').forEach(initSegmented);
  $$('.timer[data-seconds]').forEach(t => initCountdown(t, Number(t.dataset.seconds)));
  initSwitches(document);
  initSteppers(document);
  $$('[data-sheet]').forEach(b => initPanel(b, document.getElementById(b.dataset.sheet)));
  $$('[data-wheel]').forEach(w => initWheel(w, JSON.parse(w.dataset.wheel)));
  $$('[data-chat-demo]').forEach(initChatStates);
});
