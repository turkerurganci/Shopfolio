namespace Shopfolio.IntegrationTests.Browser;

/// <summary>The scripts the manual-audit procedures (<see cref="AccessibilityAuditTests"/>) run inside the page; each is one function expression that returns a JSON text.</summary>
internal static class AuditScripts
{
    /// <summary>
    /// The interactive elements a keyboard reaches on the page, and the focus log that records each element a Tab press focuses. A radio group counts once (the arrow keys move inside a group), an element with a negative
    /// tab index is the script's own focus target and is not expected in the order, an element the page hides is not expected.
    /// </summary>
    internal const string Install = """
        () => {
          const visible = (e) => {
            const r = e.getBoundingClientRect(); const s = getComputedStyle(e);
            const closed = e.closest('details:not([open])'); const inClosed = closed && !(e.tagName === 'SUMMARY' && e.parentElement === closed);
            return (r.width > 0 && r.height > 0 || e.matches('.skip-link')) && s.visibility !== 'hidden' && s.display !== 'none' && !e.closest('[hidden],[inert]') && !inClosed;
          };
          const all = [...document.querySelectorAll('a[href],button,input:not([type=hidden]),select,textarea,summary,[tabindex],[role=button]')]
            .filter((e) => !e.disabled && e.getAttribute('tabindex') !== '-1' && visible(e));
          // A radio group is one stop (the arrow keys move inside it): the checked button, or the first when none is checked.
          const groupOf = (e) => (e.form ? [...document.forms].indexOf(e.form) : -1) + '|' + e.name;
          const checkedGroups = new Set(all.filter((e) => e.tagName === 'INPUT' && e.type === 'radio' && e.name && e.checked).map(groupOf));
          const seenGroups = new Set();
          const expected = all.filter((e) => {
            if (e.tagName === 'INPUT' && e.type === 'radio' && e.name) {
              const key = groupOf(e);
              if (seenGroups.has(key) || (checkedGroups.has(key) && !e.checked)) { return false; }
              seenGroups.add(key);
            }
            return true;
          });
          const describe = (e) => (e.tagName.toLowerCase() + (e.id ? '#' + e.id : '') + (e.getAttribute('name') ? '[name=' + e.getAttribute('name') + ']' : '') + (e.tagName === 'A' ? '[href=' + (e.getAttribute('href') || '').slice(0, 40) + ']' : ''));
          window.__a11y = { expected, log: [], left: false, wrapped: false, describe };
          document.addEventListener('focusin', (event) => {
            const e = event.target; if (e === document.body || e === document.documentElement) { return; }
            const s = getComputedStyle(e); const r = e.getBoundingClientRect();
            const outline = s.outlineStyle !== 'none' && parseFloat(s.outlineWidth) > 0;
            const shadow = s.boxShadow !== 'none';
            const entry = { e, d: describe(e), indicator: outline || shadow, pageY: r.top + window.scrollY, pageX: r.left + window.scrollX, inView: r.bottom > 0 && r.top < innerHeight && r.right > 0 && r.left < innerWidth };
            window.__a11y.log.push(entry);
            if (window.__a11y.log.length > 1 && e === window.__a11y.log[0].e) { window.__a11y.wrapped = true; }
          }, true);
          document.addEventListener('focusout', (event) => { if (event.relatedTarget === null) { window.__a11y.left = true; } }, true);
          if (document.activeElement && document.activeElement !== document.body) { document.activeElement.blur(); }
          window.scrollTo(0, 0);
          return JSON.stringify({ expected: expected.length, positiveTabindex: [...document.querySelectorAll('[tabindex]')].filter((e) => parseInt(e.getAttribute('tabindex'), 10) > 0).length });
        }
        """;

    /// <summary>Reads the focus log after the Tab presses: the elements reached, the expected ones that were not, a trap (the same element three times in a row), the missing indicators, the upward jumps of the order.</summary>
    internal const string Summarize = """
        () => {
          const a = window.__a11y; const names = [];
          // The Tab key past the last control wraps to the first one: the order ends where the first control comes again.
          const wrap = a.log.findIndex((x, i) => i > 0 && x.e === a.log[0].e);
          if (wrap > 0) { a.log = a.log.slice(0, wrap); }
          const nameOf = (e) => {
            const text = (n) => (n.textContent || '').replace(/\s+/g, ' ').trim();
            const lb = e.getAttribute('aria-labelledby');
            if (lb) { const t = lb.split(/\s+/).map((id) => document.getElementById(id)).filter(Boolean).map(text).join(' ').trim(); if (t) { return t; } }
            const al = e.getAttribute('aria-label'); if (al && al.trim()) { return al.trim(); }
            if (e.labels && e.labels.length) { const t = [...e.labels].map(text).join(' ').trim(); if (t) { return t; } }
            if (e.tagName === 'INPUT' && ['submit', 'button', 'reset'].includes(e.type)) { return e.value || ''; }
            let t = '';
            const walk = (n) => { if (n.nodeType === 3) { t += n.textContent; } else if (n.nodeType === 1 && n.getAttribute('aria-hidden') !== 'true') { if (n.tagName === 'IMG') { t += ' ' + (n.getAttribute('alt') || ''); } for (const c of n.childNodes) { walk(c); } } };
            walk(e); t = t.replace(/\s+/g, ' ').trim(); if (t) { return t; }
            return (e.getAttribute('title') || '').trim();
          };
          const reached = new Set(a.log.map((x) => x.e));
          const missing = a.expected.filter((e) => !reached.has(e)).map(a.describe);
          let trap = false; let run = 1;
          for (let i = 1; i < a.log.length; i++) { run = a.log[i].e === a.log[i - 1].e ? run + 1 : 1; if (run >= 3) { trap = true; } }
          const noIndicator = a.log.filter((x) => !x.indicator).map((x) => x.d);
          const notInView = a.log.filter((x) => !x.inView && !x.e.matches('.skip-link')).map((x) => x.d);
          const jumps = []; const backwards = [];
          for (let i = 1; i < a.log.length; i++) {
            const p = a.log[i - 1]; const q = a.log[i];
            if (q.pageY < p.pageY - 24 && q.pageX <= p.pageX) { jumps.push(p.d + ' -> ' + q.d); }
            if (a.log[i - 1].e.compareDocumentPosition(q.e) & Node.DOCUMENT_POSITION_PRECEDING) { backwards.push(p.d + ' -> ' + q.d); }
          }
          const order = a.log.map((x) => nameOf(x.e) || x.d);
          return JSON.stringify({ reached: reached.size, expected: a.expected.length, missing, trap, noIndicator, notInView, jumps, backwards, order, left: a.left });
        }
        """;

    /// <summary>The accessible names of the page: a name for every control, link, image and frame; a name that says nothing; a visible label the name does not contain (WCAG 2.5.3); the same name on links to different places with no context.</summary>
    internal const string Names = """
        () => {
          const text = (n) => (n.textContent || '').replace(/\s+/g, ' ').trim();
          const visible = (e) => { const r = e.getBoundingClientRect(); const s = getComputedStyle(e); return (r.width > 0 && r.height > 0 || e.matches('.skip-link,.u-visually-hidden')) && s.visibility !== 'hidden' && s.display !== 'none' && !e.closest('[hidden],[inert]'); };
          const own = (e) => {
            const lb = e.getAttribute('aria-labelledby');
            if (lb) { const t = lb.split(/\s+/).map((id) => document.getElementById(id)).filter(Boolean).map(text).join(' ').trim(); if (t) { return t; } }
            const al = e.getAttribute('aria-label'); if (al && al.trim()) { return al.trim(); }
            if (e.labels && e.labels.length) { const t = [...e.labels].map(text).join(' ').trim(); if (t) { return t; } }
            if (e.tagName === 'INPUT' && ['submit', 'button', 'reset'].includes(e.type)) { return e.value || ''; }
            let t = '';
            const walk = (n) => { if (n.nodeType === 3) { t += n.textContent; } else if (n.nodeType === 1 && n.getAttribute('aria-hidden') !== 'true') { if (n.tagName === 'IMG') { t += ' ' + (n.getAttribute('alt') || ''); } for (const c of n.childNodes) { walk(c); } } };
            walk(e); t = t.replace(/\s+/g, ' ').trim(); if (t) { return t; }
            return (e.getAttribute('title') || '').trim();
          };
          const visibleText = (e) => { let t = ''; const walk = (n) => { if (n.nodeType === 3) { t += n.textContent; } else if (n.nodeType === 1 && n.getAttribute('aria-hidden') !== 'true' && !n.matches('.u-visually-hidden')) { for (const c of n.childNodes) { walk(c); } } }; walk(e); return t.replace(/\s+/g, ' ').trim(); };
          const generic = /^(devamı|devamını oku|tıklayın|tıkla|buraya|buradan|link|bağlantı|düğme|button|daha fazla|\.\.\.|…|[x×>›»<‹«+\-])$/i;
          const empty = []; const meaningless = []; const labelNotInName = []; const byName = new Map();
          const controls = [...document.querySelectorAll('a[href],button,input:not([type=hidden]),select,textarea,summary,[role=button],[role=tab],[role=checkbox],[role=switch],[role=menuitem]')].filter(visible);
          for (const e of controls) {
            const d = e.tagName.toLowerCase() + (e.id ? '#' + e.id : '') + (e.getAttribute('name') ? '[name=' + e.getAttribute('name') + ']' : '');
            const n = own(e);
            if (!n) { empty.push(d); continue; }
            if (generic.test(n) || !/[\p{L}\p{N}]/u.test(n)) { meaningless.push(d + ' "' + n + '"'); }
            const al = e.getAttribute('aria-label'); const vt = visibleText(e);
            if (al && vt && !al.toLowerCase().replace(/\s+/g, ' ').includes(vt.toLowerCase())) { labelNotInName.push(d + ' label "' + al + '" text "' + vt + '"'); }
            if (e.tagName === 'A') { const key = n.toLowerCase(); const href = e.getAttribute('href'); if (!byName.has(key)) { byName.set(key, new Set()); } byName.get(key).add(href); }
          }
          const images = [...document.querySelectorAll('img')].filter((e) => visible(e) && !e.closest('[aria-hidden=true]') && e.getAttribute('role') !== 'presentation' && e.getAttribute('alt') === null).map((e) => (e.getAttribute('src') || '').slice(0, 60));
          const sameNameElsewhere = [...byName.entries()].filter(([, set]) => set.size > 1).map(([k, set]) => k + ' -> ' + set.size + ' addresses');
          return JSON.stringify({ controls: controls.length, empty, meaningless, labelNotInName, images, sameNameElsewhere });
        }
        """;

    /// <summary>The reflow: a horizontal scroll bar of the page, an element that reaches past the right edge, an element that clips its text.</summary>
    internal const string Reflow = """
        () => {
          const de = document.documentElement;
          const over = de.scrollWidth - de.clientWidth;
          const past = []; const clipped = [];
          for (const e of document.querySelectorAll('body *')) {
            const s = getComputedStyle(e); if (s.display === 'none' || s.visibility === 'hidden') { continue; }
            const r = e.getBoundingClientRect(); if (r.width === 0 && r.height === 0) { continue; }
            if (e.closest('.skip-link,.u-visually-hidden,[hidden],[inert],[aria-hidden=true]') || (r.width <= 2 && r.height <= 2)) { continue; }
            const d = e.tagName.toLowerCase() + (e.id ? '#' + e.id : '') + (e.className && typeof e.className === 'string' ? '.' + e.className.split(' ')[0] : '');
            if (r.right > innerWidth + 1 && s.position !== 'fixed') { past.push(d); }
            if ((s.overflowX === 'hidden' || s.overflowX === 'clip') && e.scrollWidth > e.clientWidth + 1 && e.clientWidth > 0 && (e.textContent || '').trim().length > 0) { clipped.push(d); }
          }
          return JSON.stringify({ over, past: past.slice(0, 5), clipped: clipped.slice(0, 5), width: innerWidth });
        }
        """;

    /// <summary>The page-level facts of the criteria no rule of the tool covers (lang, title, reload, motion, media, the sensory words of an instruction).</summary>
    internal const string Facts = """
        () => {
          const media = document.querySelectorAll('audio,video,object,embed,iframe,canvas,marquee,blink').length;
          const refresh = document.querySelectorAll('meta[http-equiv=refresh i]').length;
          const infinite = document.getAnimations().filter((a) => a.effect && a.effect.getComputedTiming().iterations === Infinity).length;
          const sensory = [];
          // A colour, a shape or a side is a sensory instruction; "aşağıdaki IBAN'a" names the element (IBAN) and is not one, so a position word counts only when it stands alone ("aşağıdaki." with no name after it).
          const re = /(sağdaki|soldaki|kırmızı|yeşil|mavi|sarı|turuncu|yuvarlak|üçgen|(?:yukarıdaki|aşağıdaki)(?=\s*[.,;:)]))/gi;
          for (const e of document.querySelectorAll('main p, main li, main label, main legend, main .hint, main .field__hint, main .message')) { if (e.closest('.u-visually-hidden')) { continue; } const m = (e.textContent || '').match(re); if (m) { sensory.push(m[0] + ': ' + (e.textContent || '').trim().slice(0, 60)); } }
          const help = [...document.querySelectorAll('footer a[href="/iletisim"]')].map((a) => [...a.closest('footer').querySelectorAll('a')].indexOf(a));
          const headings = [...document.querySelectorAll('h1,h2,h3,h4,h5,h6')].map((h) => parseInt(h.tagName[1], 10));
          let skipped = false; for (let i = 1; i < headings.length; i++) { if (headings[i] > headings[i - 1] + 1) { skipped = true; } }
          return JSON.stringify({ lang: document.documentElement.lang, title: document.title, media, refresh, infinite, sensory, help, headings: headings.length, skipped, main: document.querySelectorAll('main').length, skip: document.querySelectorAll('a.skip-link[href^="#"]').length });
        }
        """;
}
