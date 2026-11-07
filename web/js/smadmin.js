// Satis Materyalleri: Icerik Yonetimi paneli (vendor). Materyaller (duzenle, sirala, kaldir, ozet onerisi), Kategoriler, Sayfa metni.
(function () {
  var $ = function (id) { return document.getElementById(id); };
  var md = $('smAdmin'), btn = $('smAdminBtn'); if (!md || !btn) return;
  var err = $('smErr'), dirty = false, S = { items: [], cats: [], intro: '' }, uid = 0;
  var ICONS = [["form", "Form"], ["flow", "Akış"], ["doc", "Belge"], ["template", "Şablon"], ["pdf", "PDF"], ["table", "Tablo"], ["catalog", "Katalog"], ["database", "Veritabanı"], ["chart", "Rapor"], ["dashboard", "Pano"], ["api", "Entegrasyon"], ["settings", "Ayarlar"], ["team", "Ekip"], ["lock", "Yetki"], ["shield", "Güvenlik"], ["bell", "Bildirim"], ["calendar", "Takvim"], ["mail", "E-posta"], ["chat", "Sohbet"], ["ai", "Yapay Zekâ"], ["mobile", "Mobil"], ["globe", "Web"], ["cloud", "Bulut"], ["code", "Kod"], ["upload", "Yükleme"], ["search", "Arama"], ["check", "Onay"], ["sign", "İmza"], ["qr", "Barkod"], ["pin", "Konum"], ["image", "Galeri"], ["play", "Video"], ["news", "Haber"], ["cap", "Eğitim"], ["book", "Kitap"], ["idea", "Fikir"], ["target", "Hedef"], ["rocket", "Başlangıç"], ["cart", "Satış"], ["invoice", "Fatura"], ["puzzle", "Bileşen"], ["layers", "Katman"], ["building", "Intranet"], ["clock", "Zaman"]], THEMES = ["Okyanus", "Gün batımı", "Grafit", "Lavanta", "Zümrüt", "Kehribar", "Gül", "Gece", "Güneş", "Lagün", "Ametist", "Mercan", "Orman", "Şarap", "Buz"];
  function el(tag, cls, txt) { var e = document.createElement(tag); if (cls) e.className = cls; if (txt != null) e.textContent = txt; return e; }
  function svgUse(id) { var ns = 'http://www.w3.org/2000/svg', s = document.createElementNS(ns, 'svg'), u = document.createElementNS(ns, 'use'); s.setAttribute('aria-hidden', 'true'); u.setAttribute('href', 'assets/caticons.svg#i-' + id); s.appendChild(u); return s; }
  function api(op, data) {
    var opt = { credentials: 'same-origin' }, url = 'SalesMaterialAction.ashx';
    if (op === 'list') url += '?op=list';
    else { var fd = new URLSearchParams(); fd.append('op', op); for (var k in data) fd.append(k, data[k]); opt.method = 'POST'; opt.headers = { 'Content-Type': 'application/x-www-form-urlencoded' }; opt.body = fd.toString(); }
    return fetch(url, opt).then(function (r) { return r.json(); }).then(function (d) {
      if (d.data) S = d.data;
      err.textContent = d.ok ? '' : (d.error || 'İşlem tamamlanamadı.');
      if (d.ok && op !== 'list') dirty = true;
      return d;
    });
  }
  // ---- pencere ----
  function open() { md.classList.add('open'); api('list').then(renderAll); }
  function close() { md.classList.remove('open'); if (dirty) location.reload(); }   // degisiklikler sayfada gorunsun
  btn.addEventListener('click', open);
  $('smAdminX').addEventListener('click', close);
  md.addEventListener('keydown', function (e) { if (e.key === 'Escape' && !e.target.closest('.sm-ed')) close(); });
  [].forEach.call(md.querySelectorAll('.sm-tabs button'), function (b) {
    b.addEventListener('click', function () {
      [].forEach.call(md.querySelectorAll('.sm-tabs button'), function (x) { x.classList.toggle('on', x === b); x.setAttribute('aria-selected', x === b ? 'true' : 'false'); });
      [].forEach.call(md.querySelectorAll('.sm-pane'), function (p) { p.hidden = p.getAttribute('data-p') !== b.getAttribute('data-t'); });
    });
  });
  function pane(n) { return md.querySelector('.sm-pane[data-p="' + n + '"]'); }
  function renderAll() { renderItems(); renderCats(); renderIntro(); }

  // ---- Materyaller ----
  function renderItems() {
    var p = pane('items'); p.innerHTML = '';
    var top = el('div', 'sm-ptop');
    var fill = el('button', 'btn-ghost', 'Eksik özetleri doldur (PDF)'); fill.type = 'button'; fill.addEventListener('click', fillAll);
    top.appendChild(el('span', 'sm-hint', 'Özet, kartın üzerine gelindiğinde görünen tek cümlelik ön bilgidir.')); top.appendChild(fill); p.appendChild(top);
    var prog = el('p', 'sm-prog'); prog.id = 'smProg'; p.appendChild(prog);
    S.cats.forEach(function (c) {
      var list = S.items.filter(function (m) { return m.group === c.name; }); if (!list.length) return;
      p.appendChild(el('h4', 'sm-ph', c.name));
      list.forEach(function (m, i) { p.appendChild(itemRow(m, i, list.length)); });
    });
  }
  function itemRow(m, i, n) {
    var row = el('div', 'sm-row'), line = el('div', 'sm-line');
    var sw = el('span', 'sm-sw acg acg-' + (m.tint >= 0 ? m.tint : 0)); if (m.tint < 0) sw.classList.add('auto'); sw.appendChild(svgUse(m.icon || m.autoIcon)); line.appendChild(sw);
    var tx = el('span', 'sm-tx'); tx.appendChild(el('b', null, m.title));
    tx.appendChild(el('small', null, (m.summary ? '' : 'Özet yok · ') + m.views + ' görüntülenme · ' + m.sends + ' gönderim')); line.appendChild(tx);
    var tools = el('span', 'sm-tools');
    tools.appendChild(tb('↑', 'Yukarı taşı', function () { api('move', { id: m.id, dir: 'up' }).then(renderItems); }, i === 0));
    tools.appendChild(tb('↓', 'Aşağı taşı', function () { api('move', { id: m.id, dir: 'down' }).then(renderItems); }, i === n - 1));
    tools.appendChild(tb('Düzenle', 'Düzenle', function () { editor(row, m); }));
    line.appendChild(tools); row.appendChild(line); return row;
  }
  function tb(t, title, fn, dis) { var b = el('button', 'sm-tb', t); b.type = 'button'; b.title = title; b.setAttribute('aria-label', title); if (dis) b.disabled = true; b.addEventListener('click', fn); return b; }

  function editor(row, m) {
    var ex = row.querySelector('.sm-ed'); if (ex) { ex.remove(); return; }
    var f = el('form', 'sm-ed'), tint = m.tint, icon = m.icon || ''; f.setAttribute('data-noload', '1');
    function field(label, input) { var w = el('label', 'sm-f'); w.appendChild(el('span', null, label)); w.appendChild(input); f.appendChild(w); return input; }
    var t1 = el('input', 'input'); t1.value = m.title; t1.maxLength = 200; field('Başlık', t1);
    var t2 = el('input', 'input'); t2.value = m.titleEn || ''; t2.maxLength = 200; field('Başlık (İngilizce, isteğe bağlı)', t2);
    var g = el('select', 'input'); S.cats.forEach(function (c) { var o = el('option', null, c.name); o.value = c.name; if (c.name === m.group) o.selected = true; g.appendChild(o); }); field('Kategori', g);
    var sum = el('textarea', 'input'); sum.rows = 3; sum.maxLength = 400; sum.value = m.summary || ''; field('Özet (tek cümle)', sum);
    var sr = el('div', 'sm-sumrow'), cc = el('span', 'sm-cc'), sg = el('button', 'btn-ghost', 'Dokümandan öner'); sg.type = 'button';
    var upd = function () { cc.textContent = sum.value.length + ' / 400'; }; sum.addEventListener('input', upd); upd();
    if (m.ext === 'pdf') sr.appendChild(sg); else sr.appendChild(el('span', 'sm-hint', 'Öneri yalnız PDF dokümanlarda kullanılabilir.'));
    sr.appendChild(cc); f.appendChild(sr);
    sg.addEventListener('click', function () {
      sg.disabled = true; sg.textContent = 'Okunuyor…';
      suggest(m).then(function (s) { if (s) { sum.value = s; upd(); } else err.textContent = 'Dokümanda uygun bir cümle bulunamadı; özeti elle yazın.'; })
        .catch(function () { err.textContent = 'Doküman okunamadı.'; }).then(function () { sg.disabled = false; sg.textContent = 'Dokümandan öner'; });
    });
    // renk temasi
    var cw = el('div', 'sm-f'); cw.appendChild(el('span', null, 'Renk'));
    var sws = el('div', 'sm-sws'); sws.setAttribute('role', 'radiogroup'); sws.setAttribute('aria-label', 'Renk');
    var opts = [[-1, 'Otomatik']].concat(THEMES.map(function (n, k) { return [k, n]; }));
    opts.forEach(function (o) {
      var b = el('button', 'sm-swb' + (o[0] < 0 ? ' auto' : ' acg acg-' + o[0])); b.type = 'button'; b.title = o[1]; b.setAttribute('role', 'radio'); b.setAttribute('aria-label', o[1]);
      if (o[0] < 0) b.textContent = 'Otomatik';
      b.addEventListener('click', function () { tint = o[0]; mark(); }); sws.appendChild(b);
    });
    cw.appendChild(sws); f.appendChild(cw);
    // ikon
    var iw = el('div', 'sm-f'); iw.appendChild(el('span', null, 'İkon'));
    var ig = el('div', 'sm-icons'); ig.setAttribute('role', 'radiogroup'); ig.setAttribute('aria-label', 'İkon');
    var ia = el('button', 'sm-ic auto'); ia.type = 'button'; ia.setAttribute('data-i', ''); ia.appendChild(svgUse(m.autoIcon)); ia.appendChild(el('span', null, 'Otomatik'));
    ia.addEventListener('click', function () { icon = ''; mark(); }); ig.appendChild(ia);
    ICONS.forEach(function (x) {
      var b = el('button', 'sm-ic'); b.type = 'button'; b.title = x[1]; b.setAttribute('data-i', x[0]); b.appendChild(svgUse(x[0])); b.appendChild(el('span', null, x[1]));
      b.addEventListener('click', function () { icon = x[0]; mark(); }); ig.appendChild(b);
    });
    iw.appendChild(ig); f.appendChild(iw);
    function mark() {
      [].forEach.call(sws.children, function (b, k) { var on = opts[k][0] === tint; b.classList.toggle('on', on); b.setAttribute('aria-checked', on ? 'true' : 'false'); });
      [].forEach.call(ig.children, function (b) { var on = b.getAttribute('data-i') === icon; b.classList.toggle('on', on); });
    }
    mark();
    var acts = el('div', 'sm-acts'), ok = el('button', 'btn-cyan', 'Kaydet'), no = el('button', 'btn-ghost', 'Vazgeç'), del = el('button', 'btn-ghost sm-del', 'Kaldır');
    ok.type = 'submit'; no.type = 'button'; del.type = 'button';
    acts.appendChild(ok); acts.appendChild(no); acts.appendChild(del); f.appendChild(acts);
    no.addEventListener('click', function () { f.remove(); });
    del.addEventListener('click', function () { if (confirm('"' + m.title + '" kaldırılsın mı? Dosya da silinir.')) api('delete', { id: m.id }).then(renderItems); });
    f.addEventListener('submit', function (e) {
      e.preventDefault();
      api('save', { id: m.id, title: t1.value, titleEn: t2.value, group: g.value, summary: sum.value, tint: tint, icon: icon }).then(function (d) { if (d.ok) renderItems(); });
    });
    row.appendChild(f); t1.focus();
  }

  // ---- Ozet onerisi: PDF'in ilk uc sayfasindaki metinden tek cumle (pdf.js, yalniz gerektiginde yuklenir) ----
  var pdfP = null;
  function pdfLib() {
    if (window.pdfjsLib) return Promise.resolve(window.pdfjsLib);
    if (!pdfP) pdfP = new Promise(function (ok, no) {
      var s = document.createElement('script'); s.src = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.min.js';
      s.onload = function () { window.pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js'; ok(window.pdfjsLib); };
      s.onerror = function () { pdfP = null; no(new Error('pdfjs')); }; document.head.appendChild(s);
    });
    return pdfP;
  }
  // metin satir satir gelir: kisa ve noktalamayla bitmeyen satirlar (baslik, iletisim) ayri paragraf sayilir; boylece alttaki cumleye yapismaz
  function paragraphs(lines) {
    var out = [], cur = '';
    lines.forEach(function (l) {
      l = l.replace(/\s+/g, ' ').trim(); if (!l) return;   // bos satir paragraf sonu sayilmaz
      var heading = l.length < 60 && !/[.!?,;:]$/.test(l);
      if (heading) { if (cur) out.push(cur); out.push(l); cur = ''; return; }
      cur = cur ? cur + ' ' + l : l;
    });
    if (cur) out.push(cur);
    return out;
  }
  function pickSentence(lines, title) {
    var parts = [];
    paragraphs(lines).forEach(function (pg) { pg.replace(/ ([.,;:!?])/g, '$1').split(/(?<=[.!?])\s+(?=[A-ZÇĞİÖŞÜ0-9"“])/).forEach(function (x) { parts.push(x); }); });
    var keys = title.toLocaleLowerCase('tr-TR').split(/\s+/).filter(function (w) { return w.length > 3; });
    var best = null, bestScore = -1;
    parts.forEach(function (s) {
      s = s.trim(); var words = s.split(' ').length, letters = (s.match(/[A-Za-zÇĞİÖŞÜçğıöşü]/g) || []).length, upper = (s.match(/[A-ZÇĞİÖŞÜ]/g) || []).length;
      if (s.length < 60 || s.length > 220 || words < 8 || letters < s.length * 0.6 || upper > letters * 0.35) return;
      if (/www\.|http|@|©|tel:|telefon|adres/i.test(s)) return;
      var low = s.toLocaleLowerCase('tr-TR'), sc = 0; keys.forEach(function (k) { if (low.indexOf(k) >= 0) sc += 2; });
      sc += 1 - Math.abs(s.length - 130) / 130;
      if (/[.!?]$/.test(s)) sc += 3;   // gercek cumle sonu: yapay nokta eklenecek parcalarin onunde
      if (sc > bestScore) { bestScore = sc; best = s; }
    });
    if (best && !/[.!?]$/.test(best)) best += '.';
    return best;
  }
  function suggest(m) {
    return pdfLib().then(function (lib) { return lib.getDocument({ url: m.href, withCredentials: true }).promise; }).then(function (doc) {
      var n = Math.min(3, doc.numPages), jobs = [];
      for (var i = 1; i <= n; i++) jobs.push(doc.getPage(i).then(function (pg) { return pg.getTextContent(); }).then(function (tc) {
        // satirlar: dikey konum (transform[5]) degisince ya da hasEOL ile yeni satir
        var lines = [], cur = '', y = null;
        tc.items.forEach(function (it) {
          var iy = it.transform ? it.transform[5] : y;
          if (y !== null && Math.abs(iy - y) > 2 && cur.trim()) { lines.push(cur); cur = ''; }
          cur += it.str; y = iy;
          if (it.hasEOL && cur.trim()) { lines.push(cur); cur = ''; }
        });
        if (cur) lines.push(cur);
        return lines;
      }));
      return Promise.all(jobs);
    }).then(function (pages) { var all = []; pages.forEach(function (p) { all = all.concat(p, ['']); }); return pickSentence(all, m.title); });
  }
  function fillAll() {
    var todo = S.items.filter(function (m) { return !m.summary && m.ext === 'pdf'; }), prog = $('smProg'), done = 0, okN = 0;
    if (!todo.length) { prog.textContent = 'Özeti eksik PDF yok.'; return; }
    var next = function () {
      if (done >= todo.length) {   // liste yenilenince ilerleme alani yeniden olusur: sonuc mesaji yenilemeden sonra yazilir
        var msg = okN + ' / ' + todo.length + ' materyale özet önerildi ve kaydedildi. Gerekirse düzenleyin.';
        return api('list').then(function () { renderItems(); var pr = $('smProg'); if (pr) pr.textContent = msg; });
      }
      var m = todo[done++]; prog.textContent = done + ' / ' + todo.length + ' okunuyor: ' + m.title;
      return suggest(m).then(function (s) {
        if (!s) return;
        okN++;
        return api('save', { id: m.id, title: m.title, titleEn: m.titleEn || '', group: m.group, summary: s, tint: m.tint, icon: m.icon || '' });
      }).catch(function () { }).then(next);
    };
    next();
  }

  // ---- Kategoriler ----
  function renderCats() {
    var p = pane('cats'); p.innerHTML = '';
    S.cats.forEach(function (c, i) {
      var n = S.items.filter(function (m) { return m.group === c.name; }).length;
      var row = el('div', 'sm-row'), line = el('div', 'sm-line'), tx = el('span', 'sm-tx');
      tx.appendChild(el('b', null, c.name)); tx.appendChild(el('small', null, n + ' materyal')); line.appendChild(tx);
      var tools = el('span', 'sm-tools');
      tools.appendChild(tb('↑', 'Yukarı taşı', function () { api('catmove', { id: c.id, dir: 'up' }).then(renderAll); }, i === 0));
      tools.appendChild(tb('↓', 'Aşağı taşı', function () { api('catmove', { id: c.id, dir: 'down' }).then(renderAll); }, i === S.cats.length - 1));
      tools.appendChild(tb('Yeniden adlandır', 'Yeniden adlandır', function () { var v = prompt('Yeni kategori adı', c.name); if (v && v.trim() && v.trim() !== c.name) api('catrename', { id: c.id, name: v.trim() }).then(renderAll); }));
      tools.appendChild(tb('Sil', 'Sil', function () { if (n > 0 || confirm('"' + c.name + '" kategorisi silinsin mi?')) api('catdel', { id: c.id }).then(renderAll); }));
      line.appendChild(tools); row.appendChild(line); p.appendChild(row);
    });
    var f = el('form', 'sm-add'), i = el('input', 'input'); f.setAttribute('data-noload', '1'); i.placeholder = 'Yeni kategori adı'; i.maxLength = 120; i.setAttribute('aria-label', 'Yeni kategori adı');
    var b = el('button', 'btn-cyan', 'Ekle'); b.type = 'submit'; f.appendChild(i); f.appendChild(b); p.appendChild(f);
    f.addEventListener('submit', function (e) { e.preventDefault(); if (i.value.trim()) api('catadd', { name: i.value.trim() }).then(renderAll); });
  }

  // ---- Sayfa metni ----
  function renderIntro() {
    var p = pane('intro'); p.innerHTML = '';
    var f = el('form', 'sm-ed open'), ta = el('textarea', 'input'); f.setAttribute('data-noload', '1'); ta.rows = 3; ta.maxLength = 300; ta.value = S.intro || '';
    var w = el('label', 'sm-f'); w.appendChild(el('span', null, 'Başlığın altındaki tek cümle')); w.appendChild(ta); f.appendChild(w);
    var b = el('button', 'btn-cyan', 'Kaydet'); b.type = 'submit'; var a = el('div', 'sm-acts'); a.appendChild(b); f.appendChild(a); p.appendChild(f);
    f.addEventListener('submit', function (e) { e.preventDefault(); api('intro', { text: ta.value }).then(function (d) { if (d.ok) { var ip = $('smIntro'); if (ip) ip.textContent = S.intro; err.textContent = ''; b.textContent = 'Kaydedildi ✓'; setTimeout(function () { b.textContent = 'Kaydet'; }, 2000); } }); });
  }
})();
