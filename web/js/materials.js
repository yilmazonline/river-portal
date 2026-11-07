(function () {
  var dash = document.getElementById('dash');
  var INIT = dash.getAttribute('data-culture') || 'tr';

  var I18N = {
    tr: {
      brand_sub: 'Asistan Portal', footer_by: '',
      nav_dash: 'Panel', nav_orders: 'Siparişler', nav_demos: 'Demo Talepleri', nav_tiers: 'Partner Seviyeleri',
      nav_support: 'Destek', nav_training: 'River Akademi', nav_materials: 'Satış Materyalleri',
      nav_overview: 'Genel Bakış', nav_partners: 'Partnerler', nav_vdemos: 'Demo Onayları',
      nav_users: 'Kullanıcılar', nav_lms: 'River Akademi',
      user_role: 'Partner Yöneticisi', logout: 'Çıkış Yap',
      mat_h: 'Satış Materyal ve İçerikleri',
      mat_sub: 'River AI sunumları, tanıtım dokümanları ve satış içerikleri. Tüm iş ortakları için.',
      mat_add: 'Yeni Materyal Yayınla', mat_add_btn: 'Yeni Materyal', mat_publish: 'Yayınla', mat_file: 'Dosya (PDF, sunum, doküman)',
      mat_hint: 'Dosya yükleyin ya da harici bağlantı girin. Yüklenen dosyalar sunucudaki materials klasörüne kaydedilir.',
      mat_ph_title: 'Başlık (Türkçe)', mat_ph_titleen: 'Title (English) opsiyonel',
      mat_ph_group: 'Kategori (ör. Sunumlar, Broşürler)', mat_ph_url: 'veya harici bağlantı (https://...)'
    },
    en: {
      brand_sub: 'Asistan Portal', footer_by: '',
      nav_dash: 'Dashboard', nav_orders: 'Orders', nav_demos: 'Demo Requests', nav_tiers: 'Partner Tiers',
      nav_support: 'Support', nav_training: 'Training Hub', nav_materials: 'Sales Materials',
      nav_overview: 'Overview', nav_partners: 'Partners', nav_vdemos: 'Demo Approvals',
      nav_users: 'Users', nav_support: 'Support', nav_lms: 'Training Mgmt',
      user_role: 'Partner Admin', logout: 'Sign Out',
      mat_h: 'Sales Materials and Content',
      mat_sub: 'River AI presentations, brochures and sales content. For all partners.',
      mat_add: 'Publish New Material', mat_add_btn: 'New Material', mat_publish: 'Publish', mat_file: 'File (PDF, presentation, document)',
      mat_hint: 'Upload a file or enter an external link. Uploaded files are saved to the server materials folder.',
      mat_ph_title: 'Title (Turkish)', mat_ph_titleen: 'Title (English) optional',
      mat_ph_group: 'Category (e.g. Presentations, Brochures)', mat_ph_url: 'or external link (https://...)'
    }
  };

  function setLang(lang) {
    var d = I18N[lang]; if (!d) return;
    document.documentElement.lang = lang;
    var els = document.querySelectorAll('[data-i18n]');
    for (var i = 0; i < els.length; i++) {
      var k = els[i].getAttribute('data-i18n');
      if (d[k] != null) els[i].textContent = d[k];
    }
    var ph = document.querySelectorAll('[data-i18n-ph]');
    for (var j = 0; j < ph.length; j++) {
      var pk = ph[j].getAttribute('data-i18n-ph');
      if (d[pk] != null) ph[j].setAttribute('placeholder', d[pk]);
    }
    var tr = document.getElementById('tr'), en = document.getElementById('en');
    if (tr) tr.className = (lang === 'tr') ? 'on' : '';
    if (en) en.className = (lang === 'en') ? 'on' : '';
    document.cookie = 'ui_culture=' + lang + ';path=/;max-age=31536000';
  }

  var langBtns = document.querySelectorAll('[data-lang]');
  for (var q = 0; q < langBtns.length; q++) {
    (function (btn) { btn.addEventListener('click', function () { setLang(btn.getAttribute('data-lang')); }); })(langBtns[q]);
  }

  setLang(INIT);
})();

// ===================== Yeni materyal penceresi + PDF/resim goruntuleyici =====================
(function () {
  // --- Yeni Materyal formu: dugmeyle acilan pencere
  var md = document.getElementById('matModal'), mb = document.getElementById('matAddBtn'), last = null;
  function mOpen() { last = document.activeElement; md.classList.add('open'); var f = md.querySelector('input[name="titletr"]'); if (f) f.focus(); }
  function mClose() { md.classList.remove('open'); if (last && last.focus) last.focus(); }
  if (md && mb) {
    mb.addEventListener('click', mOpen);
    document.getElementById('matModalX').addEventListener('click', mClose);
    var downBack = false;
    md.addEventListener('pointerdown', function (e) { downBack = e.target === md; });
    md.addEventListener('click', function (e) { if (downBack && e.target === md) mClose(); downBack = false; });
  }

  // --- Goruntuleyici (destek ekleriyle ayni gorunum: .tv-*)
  var btns = [].slice.call(document.querySelectorAll('.mat-view[data-src]'));
  var items = btns.map(function (b) { return { src: b.getAttribute('data-src'), kind: b.getAttribute('data-kind'), name: b.getAttribute('data-name'), file: b.getAttribute('data-file'), size: b.getAttribute('data-size') }; });
  var ov = null, cur = -1, lastV = null;
  var I = {
    prev: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 6l-6 6 6 6"/></svg>',
    next: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 6l6 6-6 6"/></svg>',
    open: '<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 4h6v6M20 4l-9 9M18 14v5a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V7a1 1 0 0 1 1-1h5"/></svg>',
    down: '<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 4v11M7 10l5 5 5-5M5 20h14"/></svg>'
  };
  function build() {
    ov = document.createElement('div'); ov.className = 'tv-ov'; ov.setAttribute('role', 'dialog'); ov.setAttribute('aria-modal', 'true'); ov.setAttribute('aria-label', 'Materyal görüntüleme');
    ov.innerHTML = '<div class="tv-bar"><div class="tv-tt"><b class="tv-name"></b><small class="tv-meta"></small></div><div class="tv-act">' +
      '<span class="tv-zoom" hidden><button type="button" class="tv-btn tv-zo" title="Uzaklaştır" aria-label="Uzaklaştır">&minus;</button>' +
      '<span class="tv-zv" aria-live="polite">100%</span><button type="button" class="tv-btn tv-zi" title="Yakınlaştır" aria-label="Yakınlaştır">+</button></span>' +
      '<button type="button" class="tv-btn tv-x" title="Kapat (Esc)" aria-label="Kapat">&times;</button></div></div>' +
      '<div class="tv-stage"><button type="button" class="tv-nav prev" aria-label="Önceki">' + I.prev + '</button><div class="tv-body"></div>' +
      '<button type="button" class="tv-nav next" aria-label="Sonraki">' + I.next + '</button></div>';
    document.body.appendChild(ov);
    ov.querySelector('.tv-x').addEventListener('click', vClose);
    ov.querySelector('.tv-zo').addEventListener('click', function () { zoom(-1); });
    ov.querySelector('.tv-zi').addEventListener('click', function () { zoom(1); });
    // salt okunur: sag tik menusu ve gorsel surukleme kapali
    ov.addEventListener('contextmenu', function (e) { e.preventDefault(); });
    ov.addEventListener('dragstart', function (e) { e.preventDefault(); });
    ov.querySelector('.tv-nav.prev').addEventListener('click', function () { go(-1); });
    ov.querySelector('.tv-nav.next').addEventListener('click', function () { go(1); });
    // bosluga tiklayinca kapat: tiklama boslukta baslayip boslukta bitmeli
    var downBack = false;
    function isBack(e) { var t = e.target; if (!(t === ov || t.classList.contains('tv-stage') || t.classList.contains('tv-body'))) return false;
      return !(t.classList.contains('tv-body') && (e.offsetX > t.clientWidth || e.offsetY > t.clientHeight)); }
    ov.addEventListener('pointerdown', function (e) { downBack = e.button === 0 && isBack(e); });
    ov.addEventListener('click', function (e) { var ok = downBack && isBack(e); downBack = false; if (ok) vClose(); });
  }
  function show(i) {
    if (!ov) build();
    cur = (i + items.length) % items.length;
    var it = items[cur], body = ov.querySelector('.tv-body');
    ov.querySelector('.tv-name').textContent = it.name;
    ov.querySelector('.tv-meta').textContent = (it.file || '') + (it.size ? ' · ' + it.size : '') + (items.length > 1 ? ' · ' + (cur + 1) + ' / ' + items.length : '');
    ov.classList.toggle('multi', items.length > 1);
    body.className = 'tv-body ' + it.kind; body.innerHTML = '';
    ov.querySelector('.tv-zoom').hidden = it.kind !== 'pdf';
    pdfToken++;
    if (it.kind === 'image') { var img = document.createElement('img'); img.alt = it.name; img.src = it.src; img.draggable = false; img.addEventListener('click', function () { body.classList.toggle('zoom'); }); body.appendChild(img); }
    else renderPdf(it, body);
    if (ov.className.indexOf(' open') < 0) { lastV = document.activeElement; ov.className += ' open'; document.body.classList.add('tv-lock'); ov.querySelector('.tv-x').focus(); }
  }
  function go(d) { if (items.length > 1) show(cur + d); }

  // ---- Salt okunur PDF: tarayicinin PDF goruntuleyicisi (indir / yazdir cubugu) yerine pdf.js ile sayfa sayfa cizim ----
  var pdfP = null, pdfToken = 0, pdfDoc = null, pdfScale = 1, pdfFit = 1, pdfIO = null;
  function pdfLib() {
    if (window.pdfjsLib) return Promise.resolve(window.pdfjsLib);
    if (!pdfP) pdfP = new Promise(function (ok, no) {
      var sc = document.createElement('script'); sc.src = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.min.js';
      sc.onload = function () { window.pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js'; ok(window.pdfjsLib); };
      sc.onerror = function () { pdfP = null; no(new Error('pdfjs')); }; document.head.appendChild(sc);
    });
    return pdfP;
  }
  function msg(body, t) { body.innerHTML = ''; var m = document.createElement('div'); m.className = 'tv-msg'; m.textContent = t; body.appendChild(m); }
  function renderPdf(it, body) {
    var tok = pdfToken; msg(body, 'Doküman yükleniyor…');
    pdfLib().then(function (lib) { return lib.getDocument({ url: it.src }).promise; }).then(function (doc) {
      if (tok !== pdfToken) return;
      pdfDoc = doc;
      return doc.getPage(1).then(function (p1) {
        if (tok !== pdfToken) return;
        var w = Math.min(body.clientWidth - 32, 900), v = p1.getViewport({ scale: 1 });   // %100: rahat okuma genisligi (en fazla 900 px)
        pdfFit = Math.max(0.3, Math.min(2.5, w / v.width)); pdfScale = pdfFit;
        layout(body, tok);
        var meta = ov.querySelector('.tv-meta'); meta.textContent = meta.textContent + ' · ' + doc.numPages + ' sayfa';
      });
    }).catch(function () { if (tok === pdfToken) msg(body, 'Doküman önizlenemedi. "Bana gönder" ile e-postanıza alabilirsiniz.'); });
  }
  // sayfalar once yer tutucu olarak dizilir, ekrana yaklasinca cizilir (uzun dokumanlar hizli acilir)
  function layout(body, tok) {
    body.innerHTML = ''; ov.querySelector('.tv-zv').textContent = Math.round(pdfScale / pdfFit * 100) + '%';
    var box = document.createElement('div'); box.className = 'tv-pdf'; body.appendChild(box);
    if (pdfIO) pdfIO.disconnect();
    pdfIO = new IntersectionObserver(function (es) { es.forEach(function (e) { if (e.isIntersecting && !e.target._done) { e.target._done = true; drawPage(e.target, tok); } }); }, { root: body, rootMargin: '600px 0px' });
    pdfDoc.getPage(1).then(function (p1) {
      var v = p1.getViewport({ scale: pdfScale });
      for (var i = 1; i <= pdfDoc.numPages; i++) {
        var pg = document.createElement('div'); pg.className = 'tv-page'; pg._n = i; pg.style.width = Math.floor(v.width) + 'px'; pg.style.height = Math.floor(v.height) + 'px';
        box.appendChild(pg); pdfIO.observe(pg);
      }
    });
  }
  function drawPage(holder, tok) {
    pdfDoc.getPage(holder._n).then(function (pg) {
      if (tok !== pdfToken) return;
      var v = pg.getViewport({ scale: pdfScale }), dpr = Math.min(window.devicePixelRatio || 1, 2);
      var c = document.createElement('canvas'); c.width = Math.floor(v.width * dpr); c.height = Math.floor(v.height * dpr);
      c.style.width = Math.floor(v.width) + 'px'; c.style.height = Math.floor(v.height) + 'px';
      holder.style.width = c.style.width; holder.style.height = c.style.height; holder.appendChild(c);
      pg.render({ canvasContext: c.getContext('2d'), viewport: v, transform: dpr !== 1 ? [dpr, 0, 0, dpr, 0, 0] : null });
    });
  }
  function zoom(d) {
    if (!pdfDoc || !ov) return;
    var steps = [0.5, 0.75, 1, 1.25, 1.5, 2], r = pdfScale / pdfFit, k = 0;
    for (var i = 0; i < steps.length; i++) if (Math.abs(steps[i] - r) < Math.abs(steps[k] - r)) k = i;
    k = Math.max(0, Math.min(steps.length - 1, k + d));
    pdfScale = pdfFit * steps[k]; pdfToken++;
    layout(ov.querySelector('.tv-body'), pdfToken);
  }
  function vClose() { if (!ov) return; pdfToken++; if (pdfIO) pdfIO.disconnect(); pdfDoc = null; ov.className = 'tv-ov'; document.body.classList.remove('tv-lock'); ov.querySelector('.tv-body').innerHTML = ''; if (lastV && lastV.focus) lastV.focus(); }
  btns.forEach(function (b, i) { b.addEventListener('click', function () { show(i); }); });

  document.addEventListener('keydown', function (e) {
    if (ov && ov.className.indexOf(' open') >= 0) {
      // salt okunur: kaydet ve yazdir kisayollari kapali
      if ((e.ctrlKey || e.metaKey) && /^[sp]$/i.test(e.key)) { e.preventDefault(); return; }
      if (e.key === 'Escape') { e.preventDefault(); vClose(); } else if (e.key === 'ArrowLeft') go(-1); else if (e.key === 'ArrowRight') go(1);
      else if (e.key === '+' || e.key === '=') zoom(1); else if (e.key === '-') zoom(-1);
      return;
    }
    if (e.key === 'Escape' && md && md.classList.contains('open')) mClose();
  });
})();

