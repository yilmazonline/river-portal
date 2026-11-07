// Akademi ana ekrani: kaydirilabilir kategori rafi (her alt kategori ayri kart) + kategori sayfasi (Videolar / Dokumanlar).
// Gorunumler: home (raf + Devam Et), cat (bant + secili kategorinin satirlari), arama (mevcut arama: tum icerik).
// Kategori sayfasi iki kaynaktan acilir: raf karti (alt kategori) ya da sol menudeki ana kategori (tum icerikleri).
// Adres: #kat-<ana> ya da #kat-<ana>--<alt>; geri / ileri calisir. Satirlar data-g / data-s ile suzulur (ac-out sinifi);
// aramanin kendi gizleme sinifi (ec-hide) ile karismaz.
(function () {
  var $ = function (id) { return document.getElementById(id); };
  var shelf = $('acShelf'), band = $('acBand');
  if (!shelf || !band) return;
  var vid = document.querySelector('.ec-videos'), doc = document.querySelector('.ec-docs');
  var fil = $('ecFilters'), search = $('ecSearch'), hero = $('ecHero'), empty = $('acEmpty');
  var tabV = $('acTabV'), tabD = $('acTabD'), catQ = $('acCatQ');
  var by = {};
  [].forEach.call(document.querySelectorAll('#acShelf .ac-cat, .ec-tree .ec-tgroup[data-cat]'), function (c) { by[c.getAttribute('data-cat')] = c; });
  // sunucudaki Training.aspx.cs Fold() ile ayni kural: Turkce kucuk harf, sonra "ı" = "i" (Intranet = intranet = İntranet)
  var fold = function (s) { return String(s || '').trim().toLocaleLowerCase('tr-TR').replace(/ı/g, 'i'); };
  var mode = 'home', cur = null, tab = 'v';
  var items = function (box) { return box ? [].slice.call(box.querySelectorAll('.ec-row, .ec-group')) : []; };
  var docTitle = doc ? doc.querySelector('.ec-section-title') : null;
  var main = document.querySelector('.ec-main');
  // raf basligi ("Kategoriler · N kategori") arama kutusuyla ayni satira, araç çubugunun soluna tasinir (yalniz ana ekranda gorunur)
  var tb = document.querySelector('.ec-toolbar'), head = shelf.querySelector('.ac-shelf-h');
  if (tb && head) { head.classList.add('ac-tb-head'); tb.insertBefore(head, tb.firstChild); tb.classList.add('ac-tb'); }
  var showHead = function (on) { if (head) head.hidden = !on; };
  // video / dokuman satirlarinin oklari: yalniz satir ekrana sigmiyorsa; basta sol, sonda sag ok gizli
  function railArrows(w) {
    var r = w.querySelector('.ec-rail'), l = w.querySelector('.ec-arrow.left'), rt = w.querySelector('.ec-arrow.right');
    if (!r || (!l && !rt)) return;
    var ovf = r.scrollWidth > r.clientWidth + 2;
    w.classList.toggle('ac-fit', !ovf);
    if (l) l.classList.toggle('ac-end', r.scrollLeft <= 2);
    if (rt) rt.classList.toggle('ac-end', r.scrollLeft + r.clientWidth >= r.scrollWidth - 2);
  }
  var wraps = [].slice.call(document.querySelectorAll('.ec-rail-wrap'));
  function allArrows() { wraps.forEach(railArrows); }
  wraps.forEach(function (w) {
    var r = w.querySelector('.ec-rail'); if (!r) return;
    r.addEventListener('scroll', function () { railArrows(w); }, { passive: true });
  });
  if (window.ResizeObserver) { var aro = new ResizeObserver(function (es) { es.forEach(function (e) { var w = e.target.closest('.ec-rail-wrap'); if (w) railArrows(w); }); }); wraps.forEach(function (w) { var r = w.querySelector('.ec-rail'); if (r) aro.observe(r); }); }
  window.addEventListener('resize', allArrows); window.addEventListener('load', allArrows);
  // kategori sayfasinda satirlar yatay ray yerine izgara (asagi dogru) dizilir: .ec-main.ac-incat
  var grid = function (on) { if (main) main.classList.toggle('ac-incat', on); };
  function el(tag, cls, txt) { var e = document.createElement(tag); if (cls) e.className = cls; if (txt != null) e.textContent = txt; return e; }

  function match(r) {   // satir secili kategoride mi? Ana kategori sayfasi: tum alt kategoriler
    if (fold(r.getAttribute('data-g')) !== fold(cur.getAttribute('data-g'))) return false;
    if (!cur.hasAttribute('data-s') || r.classList.contains('ec-group')) return true;
    return fold(r.getAttribute('data-s')) === fold(cur.getAttribute('data-s'));
  }

  function apply() { applyView(); setTimeout(allArrows, 0); }
  function applyView() {
    var searching = !!(search && search.value.trim());
    if (searching) {
      showHead(false);
      clearQ();
      grid(false);
      shelf.hidden = true; band.hidden = true; if (empty) empty.hidden = true;
      if (fil) fil.style.display = '';
      items(vid).concat(items(doc)).forEach(function (r) { r.classList.remove('ac-out'); });
      if (docTitle) docTitle.style.display = '';
      return;
    }
    if (fil) fil.style.display = 'none';
    [].forEach.call(document.querySelectorAll('.ec-fil'), function (b) { b.className = 'ec-fil' + (b.getAttribute('data-f') === 'all' ? ' on' : ''); });
    if (mode === 'home') {
      showHead(true);
      clearQ();
      grid(false);
      shelf.hidden = false; band.hidden = true; if (empty) empty.hidden = true;
      if (hero) hero.style.display = '';
      if (vid) vid.style.display = '';
      if (doc) doc.style.display = 'none';
      items(vid).forEach(function (r) { r.classList.toggle('ac-out', r.id !== 'row-continue'); });
      markTree();
      return;
    }
    grid(true); showHead(false);
    shelf.hidden = true; band.hidden = false;
    if (hero) hero.style.display = 'none';
    items(vid).concat(items(doc)).forEach(function (r) { r.classList.toggle('ac-out', !match(r)); });
    // dokuman grup basligi: alt kategori sayfasinda gizli (bantta zaten var)
    [].forEach.call(doc ? doc.querySelectorAll('.ec-group') : [], function (g) { if (cur.hasAttribute('data-s')) g.classList.add('ac-out'); });
    if (docTitle) docTitle.style.display = 'none';
    var vc = +cur.getAttribute('data-vc'), dc = +cur.getAttribute('data-dc');
    // kategori ici arama: baslik + aciklama; sayilar ve bos mesaji eslesmelere gore
    var term = fold(catQ ? catQ.value : ''), hit = qfilter(term);
    if (term) { vc = hit.v; dc = hit.d; }
    if (tab === 'v' && vc === 0 && dc > 0) tab = 'd';
    if (tab === 'd' && dc === 0 && vc > 0) tab = 'v';
    if (vid) vid.style.display = tab === 'v' ? '' : 'none';
    if (doc) doc.style.display = tab === 'd' ? '' : 'none';
    [[tabV, 'v', vc], [tabD, 'd', dc]].forEach(function (x) {
      var on = tab === x[1];
      x[0].setAttribute('aria-selected', on ? 'true' : 'false'); x[0].classList.toggle('on', on);
      x[0].disabled = x[2] === 0; x[0].querySelector('b').textContent = x[2];
    });
    if (empty) { var none = (tab === 'v' ? vc : dc) === 0; empty.hidden = !none; empty.textContent = none ? (term ? '“' + catQ.value.trim() + '” için bu kategoride sonuç yok.' : 'Bu kategoride henüz içerik yok.') : ''; }
    if (term && vc + dc > 0) { tabV.disabled = vc === 0; tabD.disabled = dc === 0; }
    markTree();
  }

  // kategori ici arama: eslesmeyen kartlar (ac-qout) ve tamamen bos kalan satirlar (ac-qrow) gizlenir
  function qfilter(term) {
    var n = { v: 0, d: 0 };
    [[vid, 'v'], [doc, 'd']].forEach(function (x) {
      items(x[0]).forEach(function (r) {
        if (!r.classList.contains('ec-row')) return;
        var any = false, out = r.classList.contains('ac-out');
        [].forEach.call(r.querySelectorAll('.ec-card, .ec-doc'), function (c) {
          var ok = !term || fold((c.getAttribute('data-title') || '') + ' ' + (c.getAttribute('data-desc') || '')).indexOf(term) >= 0;
          c.classList.toggle('ac-qout', !ok);
          if (ok && !out) { any = true; n[x[1]]++; }
        });
        r.classList.toggle('ac-qrow', !!term && !any);
      });
    });
    return n;
  }
  function clearQ() {
    [].forEach.call(document.querySelectorAll('.ac-qout, .ac-qrow'), function (e) { e.classList.remove('ac-qout', 'ac-qrow'); });
  }

  // sayfa basina (bu duzende kaydirmayi body yapabilir: ikisi de sifirlanir)
  function toTop() {
    try { window.scrollTo({ top: 0, behavior: 'auto' }); } catch (e) { window.scrollTo(0, 0); }
    document.documentElement.scrollTop = 0; document.body.scrollTop = 0;
  }

  function markTree() {
    var g = cur ? fold(cur.getAttribute('data-g')) : null, slug = cur ? cur.getAttribute('data-cat') : null;
    [].forEach.call(document.querySelectorAll('.ec-tree .ec-tgroup[data-cat]'), function (a) {
      var on = g !== null && fold(a.getAttribute('data-g')) === g; a.classList.toggle('ac-on', on);
      if (on && a.getAttribute('data-cat') === slug) a.setAttribute('aria-current', 'page'); else a.removeAttribute('aria-current');
    });
    [].forEach.call(document.querySelectorAll('.ec-tree .ec-tsubs a[data-cat]'), function (a) {
      var on = a.getAttribute('data-cat') === slug; a.classList.toggle('ac-on', on);
      if (on) a.setAttribute('aria-current', 'page'); else a.removeAttribute('aria-current');
    });
  }

  function fillBand(c) {
    band.className = 'ac-band acg acg-' + (parseInt(c.getAttribute('data-tint'), 10) || 0);   // kircilli gradient temasi
    var k = [c.getAttribute('data-num'), c.getAttribute('data-kicker'), c.getAttribute('data-meta')].filter(Boolean);
    $('acBandK').textContent = k.join(' · ');
    $('acBandT').textContent = c.getAttribute('data-name');
    var box = $('acBandS'); box.innerHTML = '';
    (c.getAttribute('data-sibs') || '').split('||').filter(Boolean).forEach(function (p) {
      var i = p.indexOf('|'), slug = p.slice(0, i), name = p.slice(i + 1);
      var b = el('button', 'ac-chip', name); b.type = 'button'; b.addEventListener('click', function () { go(slug); }); box.appendChild(b);
    });
    var img = $('acBandImg'), src = c.getAttribute('data-img'), ico = $('acBandIco'), ic = c.getAttribute('data-ico');
    if (src) { img.src = src; img.hidden = false; } else { img.removeAttribute('src'); img.hidden = true; }
    if (ico) {
      ico.hidden = !(ic && !src);
      var m = /^u:([0-9a-f]{32}\.png)$/.exec(ic || ''), sv = ico.querySelector('svg'), iu = ico.querySelector('.ico-u');
      // yuklenen ikon; mutlak adres: url() stil dosyasinin konumuna gore cozulur
      if (m) { iu.style.setProperty('--iu', "url('" + new URL('assets/catcovers/' + m[1], location.href).href + "')"); iu.hidden = false; sv.style.display = 'none'; }
      else if (ic) { ico.querySelector('use').setAttribute('href', 'assets/caticons.svg#i-' + ic); iu.hidden = true; sv.style.display = ''; }
    }
  }

  function openCat(slug, scroll) {
    var c = by[slug]; if (!c) { home(false); return; }
    if (cur !== c) { tab = 'v'; if (catQ) catQ.value = ''; }
    cur = c; mode = 'cat'; fillBand(c); apply();
    if (scroll) toTop();   // kategori sayfasi en ustten baslar
  }
  function home(scroll) {
    mode = 'home'; cur = null; apply();
    if (scroll) toTop();
  }
  function route(scroll) {
    var m = /^#kat-([a-z0-9-]+)$/.exec(location.hash || '');
    if (m) openCat(m[1], scroll); else home(scroll && mode === 'cat');
  }
  window.addEventListener('hashchange', function () { route(true); });
  window.addEventListener('popstate', function () { route(true); });
  function go(slug) {
    if (location.hash === '#kat-' + slug) { openCat(slug, true); return; }   // ayni kategori: hashchange olmaz
    location.hash = 'kat-' + slug;
  }
  [].forEach.call(shelf.querySelectorAll('.ac-cat'), function (c) { c.addEventListener('click', function () { go(c.getAttribute('data-cat')); }); });
  // sol menu: ayni yapi; baglantilar sayfayi yenilemeden kategori sayfasini acar
  document.addEventListener('click', function (e) {
    var a = e.target.closest && e.target.closest('.ec-tree a[data-cat], .ec-tree a[data-home]');
    if (!a) return;
    e.preventDefault();
    if (a.hasAttribute('data-home')) { $('acBack').click(); return; }
    go(a.getAttribute('data-cat'));
  });
  $('acBack').addEventListener('click', function () {
    if (location.hash) history.pushState(null, document.title, location.pathname + location.search);
    home(true);
  });
  if (catQ) {
    catQ.addEventListener('input', function () { if (mode === 'cat') apply(); });
    catQ.addEventListener('keydown', function (e) { if (e.key === 'Escape' && catQ.value) { e.stopPropagation(); catQ.value = ''; apply(); } });
  }
  [tabV, tabD].forEach(function (b) { b.addEventListener('click', function () { if (b.disabled) return; tab = b.getAttribute('data-tab'); apply(); }); });
  var t = null;
  if (search) search.addEventListener('input', function () { clearTimeout(t); t = setTimeout(apply, 260); });
  if (search) search.addEventListener('keydown', function (e) { if (e.key === 'Enter') setTimeout(apply, 30); });
  var clr = $('ecClear'); if (clr) clr.addEventListener('click', function () { setTimeout(apply, 30); });
  route(false);
  allArrows();
})();
