<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Versions.aspx.cs" Inherits="VersionsPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Versiyon Duyuruları</title>
<link href="https://fonts.googleapis.com/css2?family=Sora:wght@400;600;700;800&family=Inter:wght@400;500;600&display=swap" rel="stylesheet" />
<link rel="stylesheet" href="css/site.css?v=170" />
<link rel="icon" type="image/svg+xml" href="assets/RIVER_AI_badge.svg" />
</head>
<body>
<div class="wrap">
  <nav class="nav">
    <div class="brand">
      <a href="<%= Nav.HomeHref %>" aria-label="Ana Sayfa" style="display:inline-flex"><img class="brand-logo" src="assets/RIVER_AI_white.svg" alt="River AI" /></a>
      <span class="sub">Asistan Portal</span>
    </div>
    <div class="links"><%= Nav.Links("versions") %></div>
    <div class="right">
      <div class="user">
        <div class="who"><div class="nm"><%= Server.HtmlEncode(UserName) %></div><div class="rl"><%= Server.HtmlEncode(CompanyName) %></div></div>
        <div class="avatar"><%= Fmt.Initials(UserName) %></div>
        <div class="user-menu" id="userMenu">
          <div class="um-head"><div class="um-name"><%= Server.HtmlEncode(UserName) %></div></div>
          <a href="Logout.aspx" class="danger">Çıkış Yap</a>
        </div>
      </div>
    </div>
  </nav>

  <% if (FlashMsg.Length > 0) { %><div class="flash-ok"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>

  <div class="page-head va-head">
    <div>
    <h1><%= Culture == "en" ? "Version Announcements" : "Versiyon Duyuruları" %></h1>
    <p><%= Culture == "en" ? "New versions, highlights and access dates for River products." : "River ürünlerindeki yeni sürümler, yenilikler ve erişim tarihleri." %></p>
    </div>
    <% if (CanEdit) { %><div class="va-acts">
      <button type="button" class="btn-ghost va-listbtn" id="vaListBtn" aria-haspopup="dialog"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M8 6h13M8 12h13M8 18h13M3.5 6h.01M3.5 12h.01M3.5 18h.01"/></svg>Versiyon Listesi</button>
      <button type="button" class="btn-cyan va-new" id="vaNewBtn" aria-haspopup="dialog"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" aria-hidden="true"><path d="M12 5v14M5 12h14"/></svg>Yeni Versiyon</button>
    </div><% } %>
  </div>

  <div class="va-search" id="vaSearch">
    <div class="va-q">
      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><circle cx="11" cy="11" r="7"/><path d="m20 20-3.5-3.5"/></svg>
      <input type="search" id="vaQ" name="va_note_search" aria-label="Sürüm notlarında ara" placeholder="Sürüm notlarında ara: özellik, modül, hata ya da sürüm no (ör. 1.1.2)" autocomplete="off" readonly="readonly" data-lpignore="true" data-1p-ignore="true" data-bwignore="true" data-form-type="other" />
      <button type="button" class="va-clear" id="vaClear" aria-label="Temizle" style="display:none">&times;</button>
    </div>
    <div class="va-cats" id="vaCats">
      <button type="button" class="on" data-c="">Tümü</button><button type="button" data-c="Yenilikler">Yenilikler</button><button type="button" data-c="İyileştirmeler">İyileştirmeler</button><button type="button" data-c="Değişiklikler">Değişiklikler</button><button type="button" data-c="Hata Düzeltmeleri">Hata Düzeltmeleri</button><button type="button" data-c="Güvenlik">Güvenlik</button><button type="button" data-c="Kaldırılanlar">Kaldırılanlar</button>
    </div>
    <div class="va-info" id="vaInfo"></div>
  </div>
  <div class="va-grid" id="vaGrid"><%= PublicHtml %></div>
  <div class="va-none" id="vaNone" style="display:none">Aramanızla eşleşen bir sürüm notu bulunamadı.</div>
  <nav class="va-pager" id="vaPager" aria-label="Sayfalar"></nav>

  <% if (CanEdit) { %>

  <div class="va-modal" id="vaModal" role="dialog" aria-modal="true" aria-labelledby="vaModalTitle" aria-hidden="true">
    <div class="va-mcard">
      <div class="va-mhd"><div><h3 id="vaModalTitle">Yeni Versiyon Duyurusu</h3><p>Taslak olarak kaydedilir; yayınladığınızda tüm kullanıcılara bildirim gider.</p></div>
        <button type="button" class="va-mx va-mclose" aria-label="Kapat">&times;</button></div>
      <div class="va-mbody">
    <form method="post" action="VersionAction.aspx" id="verForm">
      <input type="hidden" name="action" value="save" />
      <div style="display:flex;gap:12px;flex-wrap:wrap">
        <div class="field" style="flex:1;min-width:220px"><label>Ürün Adı</label>
          <input class="input" name="product" placeholder="Örn. River AI" required="required" /></div>
        <div class="field" style="flex:1;min-width:160px"><label>Versiyon No</label>
          <input class="input" name="versionno" placeholder="Örn. 5.2" required="required" /></div>
      </div>
      <div style="display:flex;gap:12px;flex-wrap:wrap">
        <div class="field" style="flex:1;min-width:200px"><label>Yayın Tarihi</label>
          <input class="input" type="date" name="release" /></div>
        <div class="field" style="flex:1;min-width:200px"><label>Erişime Açılma Tarihi</label>
          <input class="input" type="date" name="access" /></div>
      </div>
      <div class="field"><label>Açıklama (opsiyonel)</label>
        <textarea class="input" name="body" style="min-height:70px;resize:vertical" placeholder="Sürüm hakkında kısa açıklama"></textarea></div>
      <div class="field">
        <label>İçerik Bölümleri <span style="color:var(--dim);font-weight:400">(kategori seçin; her satıra bir madde yazın)</span></label>
        <div id="secList"></div>
        <button type="button" class="btn-ghost" id="secAdd" style="padding:8px 16px;margin-top:8px">+ Bölüm Ekle</button>
      </div>
      <input type="hidden" name="highlights" id="secHidden" />
      <div class="va-mft"><button type="button" class="btn-ghost va-mclose" style="padding:11px 20px">Vazgeç</button><button class="btn-cyan" type="submit" style="padding:11px 22px">Taslağı Kaydet</button></div>
    </form>
      </div>
    </div>
  </div>

  <div class="va-modal" id="vaListModal" role="dialog" aria-modal="true" aria-labelledby="vaListTitle" aria-hidden="true">
    <div class="va-mcard va-lcard">
      <div class="va-mhd"><div><h3 id="vaListTitle">Versiyon Listesi</h3><p>Taslakları yayınlayın ya da silin. Yayınladığınızda seçtiğiniz kanallardan bildirim gider.</p></div>
        <button type="button" class="va-mx va-mclose" aria-label="Kapat">&times;</button></div>
      <div class="va-mbody va-lbody">
        <table class="va-ltable">
          <thead><tr><th>Ürün</th><th>Versiyon</th><th>Modüller</th><th>Yayın Tarihi</th><th>Durum</th><th></th></tr></thead>
          <tbody><%= ManageHtml %></tbody>
        </table>
      </div>
    </div>
  </div>
  <% } %>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
<script>
(function () {
  var CATS = ['Yenilikler', 'İyileştirmeler', 'Değişiklikler', 'Hata Düzeltmeleri', 'Güvenlik', 'Kaldırılanlar'];   // arama filtreleriyle ayni
  var MODS = <%= ModulesJson %>;   // modul listesi (VersionModules)
  var NEW_MOD = '__new__';
  function esc(x) { return String(x).replace(/[&<>"]/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]; }); }
  function modOpts(sel) {
    var h = '<option value="">Modül (opsiyonel)</option>';
    for (var i = 0; i < MODS.length; i++) h += '<option' + (MODS[i] === sel ? ' selected' : '') + ' value="' + esc(MODS[i]) + '">' + esc(MODS[i]) + '</option>';
    return h + '<option value="' + NEW_MOD + '">+ Yeni modül ekle…</option>';
  }
  function refreshMods() {   // yeni modul eklenince butun bolumlerin listesi guncellenir, secimler korunur
    [].forEach.call(document.querySelectorAll('.sec-mod'), function (s) { var v = s.getAttribute('data-v') || ''; s.innerHTML = modOpts(v); s.value = v; });
  }
  var list = document.getElementById('secList'), addBtn = document.getElementById('secAdd'),
      hidden = document.getElementById('secHidden'), form = document.getElementById('verForm');
  if (!list) return;
  function opts(sel) { var h = ''; for (var i = 0; i < CATS.length; i++) h += '<option' + (CATS[i] === sel ? ' selected' : '') + '>' + CATS[i] + '</option>'; return h; }
  function addSection(cat) {
    var row = document.createElement('div'); row.className = 'sec-row';
    row.innerHTML = '<div class="sec-head"><select class="input st-select sec-cat" aria-label="Kategori">' + opts(cat) + '</select>' +
      '<select class="input st-select sec-mod" aria-label="Modül" data-v="">' + modOpts('') + '</select>' +
      '<button type="button" class="sec-del" title="Bölümü kaldır" aria-label="Bölümü kaldır">&times;</button></div>' +
      '<div class="sec-newmod" hidden><input class="input" type="text" maxlength="80" placeholder="Yeni modül adı, örn. E-Fatura" aria-label="Yeni modül adı" />' +
      '<button type="button" class="btn-cyan sec-newmod-ok">Ekle</button><button type="button" class="btn-ghost sec-newmod-no">Vazgeç</button><span class="sec-newmod-err" role="alert"></span></div>' +
      '<textarea class="input sec-text" style="min-height:90px;resize:vertical" placeholder="Her satıra bir madde..."></textarea>';
    list.appendChild(row);
    var ms = row.querySelector('.sec-mod'), box = row.querySelector('.sec-newmod'), inp = box.querySelector('input'), err = box.querySelector('.sec-newmod-err');
    ms.addEventListener('change', function () {
      if (ms.value === NEW_MOD) { ms.value = ms.getAttribute('data-v') || ''; box.hidden = false; err.textContent = ''; inp.value = ''; inp.focus(); return; }
      ms.setAttribute('data-v', ms.value);
    });
    function closeBox() { box.hidden = true; ms.focus(); }
    function addMod() {
      var n = inp.value.replace(/\s+/g, ' ').trim(); if (n.length < 2) { err.textContent = 'Modül adı en az 2 karakter olmalı.'; inp.focus(); return; }
      var ok = box.querySelector('.sec-newmod-ok'); ok.disabled = true; err.textContent = '';
      var fd = new FormData(); fd.append('name', n);
      fetch('VersionModule.ashx', { method: 'POST', body: fd, credentials: 'same-origin' }).then(function (r) { return r.json(); }).then(function (d) {
        ok.disabled = false;
        if (!d.ok) { err.textContent = d.error || 'Modül eklenemedi.'; return; }
        if (MODS.indexOf(d.name) < 0) { MODS.push(d.name); MODS.sort(function (a, b) { return a.localeCompare(b, 'tr'); }); }
        ms.setAttribute('data-v', d.name); refreshMods(); closeBox();
      }).catch(function () { ok.disabled = false; err.textContent = 'Bağlantı hatası, tekrar deneyin.'; });
    }
    box.querySelector('.sec-newmod-ok').addEventListener('click', addMod);
    box.querySelector('.sec-newmod-no').addEventListener('click', closeBox);
    inp.addEventListener('keydown', function (e) { if (e.key === 'Enter') { e.preventDefault(); addMod(); } else if (e.key === 'Escape') { e.preventDefault(); e.stopPropagation(); closeBox(); } });
    row.querySelector('.sec-del').addEventListener('click', function () {
      if (list.children.length > 1) list.removeChild(row); else row.querySelector('.sec-text').value = '';
    });
  }
  addBtn.addEventListener('click', function () { addSection('Yenilikler'); });
  addSection('Yenilikler');
  form.addEventListener('submit', function () {
    var rows = list.querySelectorAll('.sec-row'), out = [];
    for (var i = 0; i < rows.length; i++) {
      var cat = rows[i].querySelector('.sec-cat').value, mod = rows[i].querySelector('.sec-mod').getAttribute('data-v') || '';
      var txt = (rows[i].querySelector('.sec-text').value || '').replace(/^\s+|\s+$/g, '');
      if (txt.length === 0) continue;
      out.push('## ' + cat + (mod ? ' | ' + mod : '') + '\n' + txt);   // "## Kategori | Modul"
    }
    hidden.value = out.join('\n');
  });
})();
</script>
<script src="js/common.js?v=185"></script>
<script>
(function () {
  var grid = document.getElementById('vaGrid'); if (!grid) return;
  var q = document.getElementById('vaQ'), clr = document.getElementById('vaClear'), info = document.getElementById('vaInfo'), none = document.getElementById('vaNone');
  var cards = [].slice.call(grid.querySelectorAll('.va-card')), cat = '', timer = null, PER = 10, page = 1, pager = document.getElementById('vaPager');
  // Turkce duyarsiz katlama: harf sayisi korunur (vurgulama icin indeksler birebir eslesir)
  var MAP = { 'ç': 'c', 'ğ': 'g', 'ı': 'i', 'ö': 'o', 'ş': 's', 'ü': 'u', 'â': 'a', 'î': 'i', 'û': 'u' };
  function fold(s) { return s.replace(/İ/g, 'i').replace(/I/g, 'ı').toLowerCase().replace(/[çğıöşüâîû]/g, function (c) { return MAP[c]; }); }
  // Eski adlarla yayinlanmis duyurular da dogru filtrelensin
  var ALIAS = { 'Yeni Özellikler': 'Yenilikler', 'Hata Giderme': 'Hata Düzeltmeleri', 'Performans İyileştirmeleri': 'İyileştirmeler' };
  var ITEMS = [];
  cards.forEach(function (c) {
    [].slice.call(c.querySelectorAll('.va-hl li')).forEach(function (li) {
      var sec = li.closest('.va-sec'); var t = li.textContent, cn = sec ? sec.getAttribute('data-cat') : '', md = sec ? (sec.getAttribute('data-mod') || '') : '';
      ITEMS.push({ li: li, card: c, t: t, f: fold(t), fm: fold(md), cat: ALIAS[cn] || cn });
    });
  });
  function esc(s) { return s.replace(/[&<>"]/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]; }); }
  function mark(it, terms) {
    if (!terms.length) { it.li.textContent = it.t; return; }
    var hits = [];
    terms.forEach(function (w) { var p = 0, k; while ((k = it.f.indexOf(w, p)) >= 0) { hits.push([k, k + w.length]); p = k + w.length; } });
    hits.sort(function (a, b) { return a[0] - b[0]; });
    var out = '', pos = 0;
    hits.forEach(function (h) { if (h[0] < pos) return; out += esc(it.t.slice(pos, h[0])) + '<mark>' + esc(it.t.slice(h[0], h[1])) + '</mark>'; pos = h[1]; });
    it.li.innerHTML = out + esc(it.t.slice(pos));
  }
  function apply() {
    var raw = (q.value || '').trim(), qf = fold(raw);
    var verQ = /^v?\d+(\.\d+)*\.?$/.test(raw) ? raw.replace(/^v/, '').replace(/\.$/, '') : null;
    var terms = verQ ? [] : qf.split(/\s+/).filter(function (w) { return w.length >= 2; });
    var active = !!(terms.length || cat || verQ);
    clr.style.display = raw ? '' : 'none';
    var hitCards = {}, nItems = 0;
    ITEMS.forEach(function (it) {
      var ver = it.card.getAttribute('data-ver') || '';
      var ok = (!cat || it.cat === cat) && (!verQ || ver === verQ || ver.indexOf(verQ + '.') === 0) &&
               terms.every(function (w) { return it.f.indexOf(w) >= 0 || it.fm.indexOf(w) >= 0; });   // madde metni ya da modul adi
      it.li.classList.toggle('va-hide', active && !ok);
      if (active && ok) { hitCards[ver] = 1; nItems++; }
      mark(it, ok ? terms : []);
    });
    cards.forEach(function (c) {
      var show = !active || hitCards[c.getAttribute('data-ver')];
      c.style.display = show ? '' : 'none';
      c.classList.toggle('va-search-on', active);
      [].slice.call(c.querySelectorAll('.va-sec')).forEach(function (s) {
        var vis = [].slice.call(s.querySelectorAll('li')).some(function (li) {
          return !li.classList.contains('va-hide') && (active || c.classList.contains('open') || !li.classList.contains('va-x'));
        });
        s.style.display = vis ? '' : 'none';
      });
    });
    // ---- sayfalama: eslesen kartlar 10'arli sayfalara bolunur ----
    var matched = cards.filter(function (c) { return c.style.display !== 'none'; });
    var pages = Math.max(1, Math.ceil(matched.length / PER));
    if (page > pages) page = pages;
    matched.forEach(function (c, k) { if (k < (page - 1) * PER || k >= page * PER) c.style.display = 'none'; });
    renderPager(pages);
    var nCards = Object.keys(hitCards).length;
    info.textContent = (active ? (nItems ? nItems + ' not · ' + nCards + ' sürüm' : '') : cards.length + ' sürüm') + (pages > 1 ? ' · Sayfa ' + page + ' / ' + pages : '');
    none.style.display = active && !nItems ? '' : 'none';
    try { var u = new URL(location.href); if (raw) u.searchParams.set('q', raw); else u.searchParams.delete('q'); if (page > 1) u.searchParams.set('p', page); else u.searchParams.delete('p'); history.replaceState(null, '', u.toString()); } catch (e) { }
  }
  // Tarayici otomatik doldurmasini engelle: kutu salt okunur gelir, kullanici etkilesiminde acilir (TransferNew firma aramasiyla ayni yontem)
  var freeQ = function () { if (q.hasAttribute('readonly')) q.removeAttribute('readonly'); };
  ['focus', 'mousedown', 'touchstart', 'keydown'].forEach(function (ev) { q.addEventListener(ev, freeQ); });
  // Sayfa acilisinda tarayici yine de bir deger yazdiysa (adres cubugundaki ?q= disinda) temizle
  var urlQ = ''; try { urlQ = new URL(location.href).searchParams.get('q') || ''; } catch (e) { }
  setTimeout(function () { if (q.value && q.value !== urlQ) { q.value = urlQ; apply(); } }, 600);
  function renderPager(pages) {
    if (pages <= 1) { pager.innerHTML = ''; return; }
    var h = '<button type="button" data-p="' + (page - 1) + '"' + (page === 1 ? ' disabled' : '') + ' aria-label="Önceki sayfa">‹ Önceki</button>', last = 0;
    for (var k = 1; k <= pages; k++) {
      if (k === 1 || k === pages || Math.abs(k - page) <= 1) {
        if (last && k - last > 1) h += '<span class="va-gap">…</span>';
        h += '<button type="button" data-p="' + k + '"' + (k === page ? ' class="on" aria-current="page"' : '') + '>' + k + '</button>'; last = k;
      }
    }
    pager.innerHTML = h + '<button type="button" data-p="' + (page + 1) + '"' + (page === pages ? ' disabled' : '') + ' aria-label="Sonraki sayfa">Sonraki ›</button>';
  }
  pager.addEventListener('click', function (e) {
    var b = e.target.closest ? e.target.closest('button[data-p]') : null; if (!b || b.disabled) return;
    page = +b.getAttribute('data-p'); apply();
    var top = document.getElementById('vaSearch').getBoundingClientRect().top + window.pageYOffset - 12;
    window.scrollTo({ top: top, behavior: 'smooth' });
  });
  q.addEventListener('input', function () { page = 1; clearTimeout(timer); timer = setTimeout(apply, 140); });
  q.addEventListener('keydown', function (e) { if (e.key === 'Escape') { q.value = ''; page = 1; apply(); } });
  clr.addEventListener('click', function () { q.value = ''; page = 1; apply(); q.focus(); });
  document.getElementById('vaCats').addEventListener('click', function (e) {
    var b = e.target.closest ? e.target.closest('button[data-c]') : null; if (!b) return;
    cat = b.getAttribute('data-c'); page = 1;
    [].slice.call(this.querySelectorAll('button')).forEach(function (x) { x.classList.toggle('on', x === b); });
    apply();
  });
  grid.addEventListener('click', function (e) {
    var b = e.target.closest ? e.target.closest('.va-more') : null; if (!b) return;
    var c = b.closest('.va-card'), open = c.classList.toggle('open');
    b.textContent = open ? 'Daha az göster' : 'Tüm notları göster (' + b.getAttribute('data-n') + ')';
    apply();
  });
  try { var sp = new URL(location.href).searchParams, p = sp.get('q'); if (p) q.value = p; page = Math.max(1, parseInt(sp.get('p'), 10) || 1); } catch (e) { }
  apply();
})();
</script>
<script>
(function () {
  // Yeni Versiyon ve Versiyon Listesi pencereleri
  var last = null, openM = null;
  function open(m, focusSel) {
    last = document.activeElement; openM = m; m.classList.add('open'); m.setAttribute('aria-hidden', 'false'); document.body.style.overflow = 'hidden';
    setTimeout(function () { var f = m.querySelector(focusSel); if (f) f.focus(); }, 60);
  }
  function close() { if (!openM) return; openM.classList.remove('open'); openM.setAttribute('aria-hidden', 'true'); document.body.style.overflow = ''; openM = null; if (last && last.focus) last.focus(); }
  [['vaModal', 'vaNewBtn', 'input[name="product"]'], ['vaListModal', 'vaListBtn', '.va-mclose']].forEach(function (x) {
    var m = document.getElementById(x[0]), b = document.getElementById(x[1]); if (!m || !b) return;
    b.addEventListener('click', function () { open(m, x[2]); });
    [].forEach.call(m.querySelectorAll('.va-mclose'), function (c) { c.addEventListener('click', close); });
  });
  // Liste penceresinde disari tiklamak kapatir; formda kapatmaz (yazilanlar kaybolmasin)
  var lm = document.getElementById('vaListModal');
  if (lm) lm.addEventListener('mousedown', function (e) { if (e.target === lm) close(); });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && openM) close(); });
  // Taslak kaydedildiyse liste acik gelir (yayinlamak bir tik uzakta)
  if (<%= OpenList ? "true" : "false" %> && lm) open(lm, '.va-mclose');
})();
</script>
</body>
</html>
