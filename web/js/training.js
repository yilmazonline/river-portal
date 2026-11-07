(function () {
  var dash = document.getElementById('dash');
  var INIT = dash ? (dash.getAttribute('data-culture') || 'tr') : 'tr';

  var I18N = {
    tr: { brand_sub: 'Asistan Portal', footer_by: '',
      nav_dash: 'Panel', nav_orders: 'Siparişler', nav_demos: 'Demo Talepleri', nav_tiers: 'Partner Seviyeleri', nav_partners: 'Partnerler', nav_vdemos: 'Demo Onayları', nav_overview: 'Genel Bakış', nav_users: 'Kullanıcılar', nav_materials: 'Satış Materyalleri',
      nav_support: 'Destek', nav_lms: 'River Akademi', nav_training: 'River Akademi', nav_tickets: 'Taleplerim',
      logout: 'Çıkış Yap' },
    en: { brand_sub: 'Asistan Portal', footer_by: '',
      nav_dash: 'Dashboard', nav_orders: 'Orders', nav_demos: 'Demo Requests', nav_tiers: 'Partner Tiers', nav_partners: 'Partners', nav_vdemos: 'Demo Approvals', nav_overview: 'Overview', nav_users: 'Users', nav_materials: 'Sales Materials',
      nav_support: 'Support', nav_lms: 'Training Mgmt', nav_training: 'Training Hub', nav_tickets: 'My Tickets',
      logout: 'Sign Out' }
  };
  function setLang(lang) {
    var d = I18N[lang]; if (!d) return;
    document.documentElement.lang = lang;
    var els = document.querySelectorAll('[data-i18n]');
    for (var i = 0; i < els.length; i++) { var k = els[i].getAttribute('data-i18n'); if (d[k] != null) els[i].textContent = d[k]; }
    var tr = document.getElementById('tr'), en = document.getElementById('en');
    if (tr) tr.className = (lang === 'tr') ? 'on' : '';
    if (en) en.className = (lang === 'en') ? 'on' : '';
    document.cookie = 'ui_culture=' + lang + ';path=/;max-age=31536000';
  }
  var langBtns = document.querySelectorAll('[data-lang]');
  for (var q = 0; q < langBtns.length; q++) (function (b) { b.addEventListener('click', function () { setLang(b.getAttribute('data-lang')); }); })(langBtns[q]);

  // ---- yardımcı POST ----
  function post(url, data) {
    try {
      var body = Object.keys(data).map(function (k) { return encodeURIComponent(k) + '=' + encodeURIComponent(data[k]); }).join('&');
      fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: body, credentials: 'same-origin' });
    } catch (e) {}
  }

  // ---- YouTube API + heartbeat izleme takibi ----
  var ytApiReady = false, ytPlayer = null, pingTimer = null, curMid = 0, pendingVid = null;
  (function loadApi() {
    var s = document.createElement('script'); s.src = 'https://www.youtube.com/iframe_api';
    document.head.appendChild(s);
  })();
  window.onYouTubeIframeAPIReady = function () { ytApiReady = true; if (pendingVid) { createPlayer(); pendingVid = null; } };

  function createPlayer() {
    if (!ytApiReady || !window.YT || !YT.Player) return;
    try { ytPlayer = new YT.Player('ytplayer', { events: {
      onReady: function () { applyRate(); vcReady(); },
      onStateChange: function (ev) { if (ev.data === 1) { applyRate(); hideTabNote(); } if (ev.data === 0) { pingProgress(); showNext(); } }
    } }); } catch (e) {}
  }
  // 15 sn'de bir çalışır (modal açıkken). API varsa gerçek konum/süre/durum; yoksa yine süreyi kaydeder.
  function heartbeat() {
    if (!curMid) return;
    var pos = 0, dur = 0, playing = true;
    if (ytPlayer && ytPlayer.getCurrentTime) {
      try {
        pos = Math.floor(ytPlayer.getCurrentTime() || 0);
        dur = Math.floor(ytPlayer.getDuration() || 0);
        var st = ytPlayer.getPlayerState ? ytPlayer.getPlayerState() : 1;
        playing = (st === 1 || st === 3); // playing veya buffering
      } catch (e) {}
    }
    if (window.VIDS && window.VIDS[curMid] && pos > 0) { window.VIDS[curMid].s = pos; if (dur > 0) { window.VIDS[curMid].dur = dur; var pc = Math.round(pos * 100 / dur); if (pc > window.VIDS[curMid].p) window.VIDS[curMid].p = pc; if (pc >= 90) window.VIDS[curMid].d = 1; } }
    post('TrainingTrack.aspx', { mid: curMid, action: 'progress', pos: pos, dur: dur, delta: playing ? 15 : 0 });
  }
  function startPing() { stopPing(); pingTimer = setInterval(heartbeat, 15000); }
  function stopPing() { if (pingTimer) { clearInterval(pingTimer); pingTimer = null; } }
  function pingProgress() { heartbeat(); }

  // ---- Lightbox ----
  var lb = document.getElementById('vidLightbox');
  var lbFrame = document.getElementById('vlbFrame');
  var lbTitle = document.getElementById('vlbTitle');
  var lbClose = document.getElementById('vlbClose');
  var starsEl = document.getElementById('vlbStars');
  var commentEl = document.getElementById('vlbComment');
  var saveRateBtn = document.getElementById('vlbSaveRate');
  var rateMsg = document.getElementById('vlbRateMsg');
  var selStars = 0;

  function paintStars(n) {
    if (!starsEl) return;
    var it = starsEl.querySelectorAll('i');
    for (var i = 0; i < it.length; i++) it[i].className = (i < n) ? 'on' : '';
  }
  if (starsEl) {
    var its = starsEl.querySelectorAll('i');
    for (var s = 0; s < its.length; s++) (function (el) {
      el.addEventListener('mouseover', function () { paintStars(parseInt(el.getAttribute('data-s'), 10)); });
      el.addEventListener('click', function () { saveStars(parseInt(el.getAttribute('data-s'), 10)); });
    })(its[s]);
    starsEl.addEventListener('mouseleave', function () { paintStars(selStars); });
  }
  // Puan: yildiza tiklandigi anda kaydedilir (yorum korunur). Gonder: yalniz yorum.
  var savedComment = '';
  function rateSend(data) {
    var body = Object.keys(data).map(function (k) { return encodeURIComponent(k) + '=' + encodeURIComponent(data[k]); }).join('&');
    return fetch('SaveRating.aspx', { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: body, credentials: 'same-origin' })
      .then(function (r) { return r.text(); }).then(function (s) { if ((s || '').trim() !== 'ok') throw 0; });
  }
  function rateNote(txt, kind) { if (rateMsg) { rateMsg.textContent = txt; rateMsg.className = 'vlb-rate-msg' + (kind ? ' ' + kind : ''); } }
  function syncSendBtn() { if (saveRateBtn && commentEl) saveRateBtn.disabled = commentEl.value.trim() === savedComment.trim(); }
  function saveStars(n) {
    if (!curMid || n < 1) return;
    var prev = selStars, mid = curMid;
    selStars = n; paintStars(n); rateNote('Kaydediliyor…', '');
    rateSend({ mid: mid, stars: n }).then(function () {
      if (window.MYRATINGS) { var o = window.MYRATINGS[mid] || {}; window.MYRATINGS[mid] = { s: n, c: o.c || '' }; }
      if (mid === curMid) rateNote('Puanınız kaydedildi, teşekkürler.', 'ok');
    }).catch(function () {
      if (mid !== curMid) return;
      selStars = prev; paintStars(prev); rateNote('Puan kaydedilemedi, lütfen tekrar deneyin.', 'err');
    });
  }
  if (commentEl) commentEl.addEventListener('input', syncSendBtn);
  if (commentEl) commentEl.addEventListener('keydown', function (e) { if (e.key === 'Enter' && saveRateBtn && !saveRateBtn.disabled) { e.preventDefault(); saveRateBtn.click(); } });
  if (saveRateBtn) saveRateBtn.addEventListener('click', function () {
    if (!curMid) return;
    if (selStars < 1) { rateNote('Yorum göndermek için önce yıldız verin.', 'err'); return; }
    var mid = curMid, c = commentEl ? commentEl.value.trim() : '';
    saveRateBtn.disabled = true; rateNote('Gönderiliyor…', '');
    rateSend({ mid: mid, stars: selStars, comment: c }).then(function () {
      if (window.MYRATINGS) window.MYRATINGS[mid] = { s: selStars, c: c };
      if (mid === curMid) { savedComment = c; rateNote(c ? 'Yorumunuz gönderildi, teşekkürler.' : 'Yorumunuz kaldırıldı.', 'ok'); syncSendBtn(); }
    }).catch(function () { if (mid === curMid) { rateNote('Yorum gönderilemedi, lütfen tekrar deneyin.', 'err'); syncSendBtn(); } });
  });

  function openLb(vid, title, mid) {
    if (!lb || !vid) return;
    toggleModal('histModal', false); toggleModal('suggestModal', false);
    curMid = parseInt(mid, 10) || 0;
    var origin = encodeURIComponent(location.protocol + '//' + location.host);
    var startAt = forceStart != null ? forceStart : resumeAt(curMid); forceStart = null;   // bilgi tabani baglantisi belirli saniyeden acar
    hideNext(); closePop();
    lbFrame.innerHTML = '<iframe id="ytplayer" src="https://www.youtube.com/embed/' + vid +
      '?autoplay=1&rel=0&enablejsapi=1&controls=0&disablekb=1&fs=0&iv_load_policy=3&playsinline=1' + (startAt ? '&start=' + startAt : '') + '&origin=' + origin + '" frameborder="0" allow="autoplay; encrypted-media; picture-in-picture" allowfullscreen="allowfullscreen"></iframe>';
    showResume(startAt); paintWlBtn(); paintRate(getRate());
    if (lbTitle) {
      lbTitle.textContent = title || '';
      var dc = document.querySelector('.ec-card[data-mid="' + curMid + '"]'), dt = dc ? dc.getAttribute('data-date') : '';
      if (dt) { var sp = document.createElement('span'); sp.className = 'vlb-date'; sp.textContent = (document.documentElement.lang === 'en' ? 'Uploaded ' : 'Yüklenme: ') + dt; lbTitle.appendChild(sp); }
    }
    // puan widget
    selStars = 0; if (commentEl) commentEl.value = ''; if (rateMsg) rateMsg.textContent = '';
    if (window.MYRATINGS && window.MYRATINGS[curMid]) { selStars = parseInt(window.MYRATINGS[curMid].s, 10) || 0; if (commentEl) commentEl.value = window.MYRATINGS[curMid].c || ''; }
    savedComment = commentEl ? commentEl.value : ''; syncSendBtn(); if (rateMsg) rateMsg.className = 'vlb-rate-msg';
    paintStars(selStars);
    lb.className = 'vid-lightbox open';
    // izlenme + oynatıcı
    if (curMid) post('TrainingTrack.aspx', { mid: curMid, action: 'view' });
    ytPlayer = null;
    if (ytApiReady) createPlayer(); else pendingVid = vid;
    startPing();
    vcStart(vid, startAt, origin);
  }
  // Baglantiyla acilis: Training.aspx?play=<MaterialId>&v=<YoutubeId>&t=<saniye> (destek talebi on yanitindaki video baglantilari)
  var forceStart = null;
  (function () {
    try {
      var p = new URLSearchParams(location.search), m = p.get('play'); if (!m || !/^\d+$/.test(m)) return;
      var t = Math.max(0, parseInt(p.get('t'), 10) || 0), el = document.querySelector('[data-mid="' + m + '"][data-vid]');
      var vid = el ? el.getAttribute('data-vid') : (p.get('v') || ''); if (!/^[\w-]{6,20}$/.test(vid)) return;
      forceStart = t; setTimeout(function () { openLb(vid, el ? el.getAttribute('data-title') : '', m); }, 250);
    } catch (e) { }
  })();
  // ===================== Kendi kontrol cubugumuz =====================
  // YouTube kontrolleri kapali (controls=0). Cubuk yalniz oynatici API'sinden OKUR ve komut GONDERIR;
  // izleme takibi (heartbeat), hiz, devam etme ve sonraki video kodlari degismedi.
  var vcEnded = false, vcEl = document.getElementById('vlbPlayer'), vcTimer = null, vcFallback = null, vcIsReady = false, vcDrag = false, vcVid = '', vcStartAt = 0, vcOrigin = '';
  function $v(id) { return document.getElementById(id); }
  function vcFmt(s) { s = Math.max(0, Math.floor(s || 0)); var h = Math.floor(s / 3600), m = Math.floor((s % 3600) / 60), x = s % 60; return (h ? h + ':' + (m < 10 ? '0' : '') : '') + m + ':' + (x < 10 ? '0' : '') + x; }
  function vcOk() { return vcIsReady && ytPlayer && ytPlayer.getCurrentTime; }
  function vcReady() { vcIsReady = true; if (vcFallback) { clearTimeout(vcFallback); vcFallback = null; } if (vcEl) vcEl.classList.remove('vc-native'); vcTick(); }
  function vcStart(vid, startAt, origin) {
    if (!vcEl) return;
    vcIsReady = false; vcDrag = false; vcVid = vid; vcStartAt = startAt || 0; vcOrigin = origin;
    vcEl.classList.remove('vc-native', 'vc-playing'); $v('vcPlayed').style.width = '0%'; $v('vcBuf').style.width = '0%'; $v('vcThumb').style.left = '0%'; $v('vcTime').textContent = '0:00 / 0:00';
    if (vcTimer) clearInterval(vcTimer); vcTimer = setInterval(vcTick, 250);
    // Guvenlik agi: API 8 sn icinde hazir olmazsa (or. kurumsal agda engelli) YouTube'un kendi kontrolleriyle yeniden ac
    if (vcFallback) clearTimeout(vcFallback);
    vcFallback = setTimeout(function () {
      if (vcIsReady || !curMid) return;
      vcEl.classList.add('vc-native');
      var fr = document.getElementById('ytplayer');
      if (fr) fr.src = fr.src.replace('&controls=0&disablekb=1&fs=0', '&controls=1');
    }, 8000);
  }
  function vcStop() {
    if (vcTimer) { clearInterval(vcTimer); vcTimer = null; }
    if (vcFallback) { clearTimeout(vcFallback); vcFallback = null; }
    vcIsReady = false;
    if (document.fullscreenElement === vcEl && document.exitFullscreen) { try { document.exitFullscreen(); } catch (e) { } }
  }
  function vcTick() {
    if (!vcOk() || !vcEl) return;
    try {
      var cur = ytPlayer.getCurrentTime() || 0, dur = ytPlayer.getDuration() || 0, st = ytPlayer.getPlayerState(), buf = ytPlayer.getVideoLoadedFraction ? ytPlayer.getVideoLoadedFraction() : 0;
      vcEl.classList.toggle('vc-playing', st === 1 || st === 3);
      if (st === 0 && !vcEnded) { vcEnded = true; if (window.pathAfterVideo) setTimeout(window.pathAfterVideo, 2500); }   // video bitti: son kayit islendikten sonra
      if (st !== 0) vcEnded = false;
      vcEl.classList.toggle('vc-muted', ytPlayer.isMuted ? ytPlayer.isMuted() : false);
      $v('vcBuf').style.width = Math.min(100, buf * 100) + '%';
      if (!vcDrag && dur > 0) { var pc = Math.min(100, cur * 100 / dur); $v('vcPlayed').style.width = pc + '%'; $v('vcThumb').style.left = pc + '%'; }
      $v('vcTime').textContent = vcFmt(cur) + ' / ' + vcFmt(dur);
      var vr = $v('vcVol'); if (document.activeElement !== vr && ytPlayer.getVolume) vr.value = ytPlayer.isMuted() ? 0 : ytPlayer.getVolume();
    } catch (e) { }
  }
  function vcToggle() { if (!vcOk()) return; try { var st = ytPlayer.getPlayerState(); if (st === 1 || st === 3) ytPlayer.pauseVideo(); else ytPlayer.playVideo(); } catch (e) { } setTimeout(vcTick, 60); }
  function vcSeekBy(d) { if (!vcOk()) return; try { var dur = ytPlayer.getDuration() || 0; ytPlayer.seekTo(Math.max(0, Math.min(dur - 1, (ytPlayer.getCurrentTime() || 0) + d)), true); } catch (e) { } setTimeout(vcTick, 60); }
  function vcMute() { if (!vcOk()) return; try { if (ytPlayer.isMuted()) { ytPlayer.unMute(); if (!ytPlayer.getVolume()) ytPlayer.setVolume(60); } else ytPlayer.mute(); } catch (e) { } setTimeout(vcTick, 60); }
  function vcFull() {
    if (!vcEl) return;
    var fe = document.fullscreenElement || document.webkitFullscreenElement;
    if (fe) { (document.exitFullscreen || document.webkitExitFullscreen).call(document); return; }
    var rq = vcEl.requestFullscreen || vcEl.webkitRequestFullscreen; if (rq) rq.call(vcEl);
  }
  if (vcEl) {
    $v('vcPlay').addEventListener('click', vcToggle);
    $v('vcBack').addEventListener('click', function () { vcSeekBy(-10); });
    $v('vcFwd').addEventListener('click', function () { vcSeekBy(10); });
    $v('vcMute').addEventListener('click', vcMute);
    $v('vcFull').addEventListener('click', vcFull);
    $v('vcVol').addEventListener('input', function () { if (!vcOk()) return; var v = parseInt(this.value, 10) || 0; try { ytPlayer.setVolume(v); if (v > 0 && ytPlayer.isMuted()) ytPlayer.unMute(); if (v === 0) ytPlayer.mute(); } catch (e) { } });
    ['fullscreenchange', 'webkitfullscreenchange'].forEach(function (ev) { document.addEventListener(ev, function () { vcEl.classList.toggle('vc-fs', (document.fullscreenElement || document.webkitFullscreenElement) === vcEl); }); });
    // ilerleme cubugu: tikla / surukle ile atla; uzerinde zaman ipucu
    var pg = $v('vcProg'), tip = $v('vcTip');
    function frac(e) { var r = pg.getBoundingClientRect(); return Math.max(0, Math.min(1, (e.clientX - r.left) / r.width)); }
    function dur() { try { return vcOk() ? (ytPlayer.getDuration() || 0) : 0; } catch (e) { return 0; } }
    pg.addEventListener('pointermove', function (e) { var f = frac(e), d = dur(); tip.textContent = vcFmt(f * d); tip.style.left = (f * 100) + '%'; if (vcDrag) { $v('vcPlayed').style.width = (f * 100) + '%'; $v('vcThumb').style.left = (f * 100) + '%'; } });
    pg.addEventListener('pointerdown', function (e) { if (!vcOk()) return; vcDrag = true; pg.setPointerCapture(e.pointerId); pg.classList.add('drag'); var f = frac(e); $v('vcPlayed').style.width = (f * 100) + '%'; $v('vcThumb').style.left = (f * 100) + '%'; });
    pg.addEventListener('pointerup', function (e) { if (!vcDrag) return; vcDrag = false; pg.classList.remove('drag'); try { ytPlayer.seekTo(frac(e) * dur(), true); } catch (x) { } setTimeout(vcTick, 80); });
    pg.addEventListener('pointercancel', function () { vcDrag = false; pg.classList.remove('drag'); });
    // klavye: Bosluk/K oynat-duraklat, oklar 5 sn, M ses, F tam ekran (yalniz video penceresi acikken; yazi alanlarinda degil)
    document.addEventListener('keydown', function (e) {
      if (!lb || lb.className.indexOf('open') < 0 || vcEl.classList.contains('vc-native')) return;
      var tg = e.target, tn = (tg && tg.tagName) || '';
      if (tn === 'INPUT' || tn === 'TEXTAREA' || tn === 'SELECT' || (tg && tg.isContentEditable)) return;
      var k = e.key;
      if (k === ' ' || k === 'k' || k === 'K') { e.preventDefault(); vcToggle(); }
      else if (k === 'ArrowLeft') { e.preventDefault(); vcSeekBy(-5); }
      else if (k === 'ArrowRight') { e.preventDefault(); vcSeekBy(5); }
      else if (k === 'm' || k === 'M') vcMute();
      else if (k === 'f' || k === 'F') vcFull();
    });
  }

  function closeLb() {
    if (!lb) return;
    pingProgress();               // son ilerleme
    stopPing();
    vcStop();
    if (window.pathAfterVideo) setTimeout(window.pathAfterVideo, 900);   // Sifirdan Uzmanliga: durum + kutlama
    try { if (ytPlayer && ytPlayer.destroy) ytPlayer.destroy(); } catch (e) {}
    hideNext(); closePop(); pausedByTab = false; tabNoteOn = false;
    ytPlayer = null; curMid = 0;
    lb.className = 'vid-lightbox';
    lbFrame.innerHTML = '';
  }

  document.addEventListener('click', function (e) {
    var el = e.target;
    if (el.closest && el.closest('.ec-more, .ec-menu')) return;   // kart ustundeki uc nokta menusu videoyu acmaz
    while (el && el !== document) {
      if (el.classList && el.classList.contains('hist-open') && el.getAttribute('data-vid')) {
        openLb(el.getAttribute('data-vid'), el.getAttribute('data-title'), el.getAttribute('data-mid'));
        return;
      }
      if (el.classList && el.classList.contains('ec-card')) {
        openLb(el.getAttribute('data-vid'), el.getAttribute('data-title'), el.getAttribute('data-mid'));
        return;
      }
      el = el.parentNode;
    }
  });
  if (lbClose) lbClose.addEventListener('click', closeLb);
  if (lb) lb.addEventListener('click', function (e) { if (e.target === lb) closeLb(); });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape') closeLb(); });

  // ---- Rail okları ----
  var arrows = document.querySelectorAll('.ec-arrow');
  for (var a = 0; a < arrows.length; a++) arrows[a].addEventListener('click', function (e) {
    e.stopPropagation();
    var rail = this.parentNode.querySelector('.ec-rail');
    if (!rail) return;
    var dx = Math.round(rail.clientWidth * 0.85);
    rail.scrollBy({ left: this.className.indexOf('left') >= 0 ? -dx : dx, behavior: 'smooth' });
  });

  // ---- Editör Paneli modalı ----
  var edModal = document.getElementById('editorModal');
  var btnEditor = document.getElementById('btnEditor');
  if (btnEditor && edModal) btnEditor.addEventListener('click', function () { edModal.className = 'vid-lightbox open'; });
  var edClose = document.getElementById('editorClose');
  if (edClose) edClose.addEventListener('click', function () { edModal.className = 'vid-lightbox'; });
  if (edModal) edModal.addEventListener('click', function (e) { if (e.target === edModal) edModal.className = 'vid-lightbox'; });
  var edTabs = document.querySelectorAll('.ed-tab');
  for (var et = 0; et < edTabs.length; et++) edTabs[et].addEventListener('click', function () {
    var key = this.getAttribute('data-t');
    for (var k = 0; k < edTabs.length; k++) edTabs[k].className = 'ed-tab';
    this.className = 'ed-tab on';
    var panes = document.querySelectorAll('.ed-pane');
    for (var p = 0; p < panes.length; p++) panes[p].className = 'ed-pane' + (panes[p].getAttribute('data-p') === key ? ' on' : '');
  });

  // ---- Doküman modalı ----
  var docModal = document.getElementById('docModal');
  var docFrame = document.getElementById('docFrame');
  var docTitle = document.getElementById('docTitle');
  var docActions = document.getElementById('docActions');
  var docClose = document.getElementById('docClose');
  // Tarayici goruntuleyici: pdf, image, video, audio, text, csv, docx, xlsx; digerleri indirilir.
  // Word / Excel icin kutuphane yalniz gerektiginde yuklenir; cevrilen icerik kisitli cercevede (betik calismaz) gosterilir.
  var DOC_LIBS = { docx: 'https://cdn.jsdelivr.net/npm/mammoth@1.8.0/mammoth.browser.min.js', xlsx: 'https://cdn.jsdelivr.net/npm/xlsx@0.18.5/dist/xlsx.full.min.js' };
  var docLibP = {}, docSeq = 0, DOC_MAX = 15 * 1024 * 1024;
  function docLib(k) {
    if (!docLibP[k]) docLibP[k] = new Promise(function (ok, no) {
      var sc = document.createElement('script'); sc.src = DOC_LIBS[k]; sc.async = true;
      sc.onload = ok; sc.onerror = function () { delete docLibP[k]; no(new Error('lib')); };
      document.head.appendChild(sc);
    });
    return docLibP[k];
  }
  function el(tag, cls, txt) { var e = document.createElement(tag); if (cls) e.className = cls; if (txt != null) e.textContent = txt; return e; }
  function noPrev(ext, msg) {
    var w = el('div', 'doc-noprev'); w.appendChild(el('div', 'doc-noprev-ic', ext || 'DOSYA')); w.appendChild(el('p', null, msg));
    docFrame.innerHTML = ''; docFrame.appendChild(w);
  }
  function loading() { noPrev('…', 'Önizleme hazırlanıyor…'); docFrame.firstChild.classList.add('doc-loading'); }
  function sandboxed(html) {
    var f = el('iframe'); f.setAttribute('sandbox', ''); f.title = 'Doküman önizleme';
    f.srcdoc = '<!DOCTYPE html><html><head><meta charset="utf-8"><style>body{font:15px/1.6 system-ui,Segoe UI,Arial,sans-serif;color:#1b2440;margin:28px 34px;background:#fff}img{max-width:100%;height:auto}' +
      'table{border-collapse:collapse;font-size:13px}td,th{border:1px solid #d8dce8;padding:5px 9px;vertical-align:top;white-space:nowrap}tr:first-child td{background:#f3f5fb;font-weight:600}h1,h2,h3{line-height:1.25}</style></head><body>' + html + '</body></html>';
    return f;
  }
  function parseCsv(t) {
    var first = t.split(/\r?\n/)[0] || '', d = ',';
    var c = { ',': (first.match(/,/g) || []).length, ';': (first.match(/;/g) || []).length, '\t': (first.match(/\t/g) || []).length };
    if (c[';'] > c[d]) d = ';'; if (c['\t'] > c[d]) d = '\t';
    var rows = [], row = [], cur = '', q = false;
    for (var i = 0; i < t.length && rows.length <= 500; i++) {
      var ch = t[i];
      if (q) { if (ch === '"') { if (t[i + 1] === '"') { cur += '"'; i++; } else q = false; } else cur += ch; }
      else if (ch === '"') q = true;
      else if (ch === d) { row.push(cur); cur = ''; }
      else if (ch === '\n' || ch === '\r') { if (ch === '\r' && t[i + 1] === '\n') i++; row.push(cur); rows.push(row); row = []; cur = ''; }
      else cur += ch;
    }
    if (cur.length || row.length) { row.push(cur); rows.push(row); }
    return rows;
  }
  function csvTable(rows, total) {
    var w = el('div', 'doc-table'), tb = el('table'), more = rows.length > 500;
    rows.slice(0, 500).forEach(function (r, i) { var tr = el('tr'); r.forEach(function (v) { tr.appendChild(el(i ? 'td' : 'th', null, v)); }); tb.appendChild(tr); });
    w.appendChild(tb);
    if (more) w.appendChild(el('p', 'doc-more', 'İlk 500 satır gösteriliyor. Tamamı için dosyayı indirin.'));
    return w;
  }
  function openDoc(o) {
    if (!docModal) return;
    var seq = ++docSeq, kind = o.kind || 'none', ext = (o.ext || '').toUpperCase();
    docFrame.className = 'doc-frame k-' + kind; docFrame.innerHTML = '';
    if (docTitle) docTitle.textContent = o.title || '';
    if (docActions) { docActions.innerHTML = ''; var a = el('a', 'btn-cyan', 'İndir'); a.href = o.dl || o.href; docActions.appendChild(a); }
    docModal.className = 'vid-lightbox open';
    var stale = function () { return seq !== docSeq || docModal.className.indexOf('open') < 0; };
    var fail = function () { if (!stale()) noPrev(ext, 'Önizleme açılamadı. Dosyayı indirerek açabilirsiniz.'); };
    if (kind === 'pdf') { var f = el('iframe'); f.title = o.title || 'PDF'; f.src = o.href + '#view=FitH'; docFrame.appendChild(f); return; }
    if (kind === 'image') { var iw = el('div', 'doc-img'), im = el('img'); im.alt = o.title || ''; im.src = o.href; im.onerror = fail; iw.appendChild(im); docFrame.appendChild(iw); return; }
    if (kind === 'video') { var v = el('video'); v.controls = true; v.preload = 'metadata'; v.setAttribute('playsinline', ''); v.src = o.href; v.onerror = fail; docFrame.appendChild(v); return; }
    if (kind === 'audio') { var aw = el('div', 'doc-audio'); aw.appendChild(el('div', 'doc-noprev-ic', ext)); var au = el('audio'); au.controls = true; au.preload = 'metadata'; au.src = o.href; au.onerror = fail; aw.appendChild(au); docFrame.appendChild(aw); return; }
    if (kind === 'none') { noPrev(ext, 'Bu dosya türü tarayıcıda önizlenemiyor. Aşağıdaki düğmeyle indirebilirsiniz.'); return; }
    if (o.size > DOC_MAX) { noPrev(ext, 'Dosya önizleme için büyük (' + Math.round(o.size / 1048576) + ' MB). Aşağıdaki düğmeyle indirebilirsiniz.'); return; }
    loading();
    var get = fetch(o.href, { credentials: 'same-origin' }).then(function (r) { if (!r.ok) throw new Error('http'); return r; });
    if (kind === 'text' || kind === 'csv') {
      get.then(function (r) { return r.text(); }).then(function (t) {
        if (stale()) return;
        docFrame.innerHTML = '';
        if (kind === 'csv') { docFrame.appendChild(csvTable(parseCsv(t))); return; }
        var pre = el('pre', 'doc-text'); pre.textContent = t.length > 1500000 ? t.slice(0, 1500000) + '\n\n… (devamı için dosyayı indirin)' : t; docFrame.appendChild(pre);
      }).catch(fail);
      return;
    }
    if (kind === 'docx') {
      Promise.all([get.then(function (r) { return r.arrayBuffer(); }), docLib('docx')]).then(function (x) {
        return window.mammoth.convertToHtml({ arrayBuffer: x[0] });
      }).then(function (res) { if (stale()) return; docFrame.innerHTML = ''; docFrame.appendChild(sandboxed(res.value || '<p>(Boş belge)</p>')); }).catch(fail);
      return;
    }
    if (kind === 'xlsx') {
      Promise.all([get.then(function (r) { return r.arrayBuffer(); }), docLib('xlsx')]).then(function (x) {
        if (stale()) return;
        var wb = window.XLSX.read(new Uint8Array(x[0]), { type: 'array' }), names = wb.SheetNames || [];
        docFrame.innerHTML = '';
        var box = el('div', 'doc-xl'), tabs = el('div', 'doc-xl-tabs'), host = el('div', 'doc-xl-host');
        var show = function (i) {
          [].forEach.call(tabs.children, function (b, k) { b.classList.toggle('on', k === i); });
          host.innerHTML = ''; host.appendChild(sandboxed(window.XLSX.utils.sheet_to_html(wb.Sheets[names[i]], { header: '', footer: '' })));
        };
        names.forEach(function (n, i) { var b = el('button', null, n); b.type = 'button'; b.addEventListener('click', function () { show(i); }); tabs.appendChild(b); });
        if (names.length > 1) box.appendChild(tabs);
        box.appendChild(host); docFrame.appendChild(box);
        if (names.length) show(0); else noPrev(ext, 'Çalışma kitabında sayfa yok.');
      }).catch(fail);
      return;
    }
    noPrev(ext, 'Bu dosya türü tarayıcıda önizlenemiyor. Aşağıdaki düğmeyle indirebilirsiniz.');
  }
  function closeDoc() { if (!docModal) return; docSeq++; docModal.className = 'vid-lightbox'; docFrame.innerHTML = ''; }
  document.addEventListener('click', function (e) {
    var t = e.target;
    while (t && t !== document) {
      if (t.classList && t.classList.contains('ec-doc')) {
        openDoc({ href: t.getAttribute('data-href'), dl: t.getAttribute('data-dl'), kind: t.getAttribute('data-kind') || (t.getAttribute('data-pdf') === '1' ? 'pdf' : 'none'),
          title: t.getAttribute('data-title'), ext: t.getAttribute('data-type'), size: parseInt(t.getAttribute('data-size') || '0', 10) });
        return;
      }
      t = t.parentNode;
    }
  });
  if (docClose) docClose.addEventListener('click', closeDoc);
  if (docModal) docModal.addEventListener('click', function (e) { if (e.target === docModal) closeDoc(); });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape') closeDoc(); });

  // ---- Tür filtresi ----
  var fils = document.querySelectorAll('.ec-fil');
  var vidBox = document.querySelector('.ec-videos');
  var docBox = document.querySelector('.ec-docs');
  for (var f = 0; f < fils.length; f++) fils[f].addEventListener('click', function () {
    for (var k = 0; k < fils.length; k++) fils[k].className = 'ec-fil';
    this.className = 'ec-fil on';
    var v = this.getAttribute('data-f');
    if (vidBox) vidBox.style.display = (v === 'doc') ? 'none' : '';
    if (docBox) docBox.style.display = (v === 'video') ? 'none' : '';
  });


  // ---- Kategori seç/oluştur pickerları ----
  var picks = document.querySelectorAll('.ec-catpick');
  for (var p2 = 0; p2 < picks.length; p2++) (function (pick) {
    var sel = pick.querySelector('.ec-cat-sel');
    var neu = pick.querySelector('.ec-cat-new');
    var hid = pick.querySelector('.ec-cat-hidden');
    function sync() {
      if (sel.value === '__new__') { neu.style.display = ''; hid.value = neu.value.trim(); }
      else { neu.style.display = 'none'; hid.value = sel.value; }
    }
    if (sel) sel.addEventListener('change', sync);
    if (neu) neu.addEventListener('input', function () { hid.value = neu.value.trim(); });
    sync();
  })(picks[p2]);

  // ---- Hero slider ----
  (function () {
    var track = document.getElementById('ecHeroTrack');
    if (!track || track.children.length < 2) return;
    var slides = track.children.length, idx = 0, timer = null;
    var dots = document.querySelectorAll('.ec-hdot');
    function go(i) {
      idx = (i + slides) % slides;
      track.style.transform = 'translateX(-' + (idx * 100) + '%)';
      for (var d = 0; d < dots.length; d++) dots[d].className = 'ec-hdot' + (d === idx ? ' on' : '');
    }
    function start() { stop(); timer = setInterval(function () { go(idx + 1); }, 6000); }
    function stop() { if (timer) { clearInterval(timer); timer = null; } }
    var pv = document.getElementById('ecHeroPrev'), nx = document.getElementById('ecHeroNext');
    if (pv) pv.addEventListener('click', function () { go(idx - 1); start(); });
    if (nx) nx.addEventListener('click', function () { go(idx + 1); start(); });
    for (var d = 0; d < dots.length; d++) (function (el) { el.addEventListener('click', function () { go(parseInt(el.getAttribute('data-i'), 10)); start(); }); })(dots[d]);
    var hero = document.getElementById('ecHero');
    if (hero) { hero.addEventListener('mouseenter', stop); hero.addEventListener('mouseleave', start); }
    start();
  })();

  // ---- Ağaç tıklaması: filtreyi Tümü yap + kaydır (one-page) ----
  var treeLinks = document.querySelectorAll('.ec-tree a[href^="#"]');
  for (var t2 = 0; t2 < treeLinks.length; t2++) treeLinks[t2].addEventListener('click', function (e) {
    var id = this.getAttribute('href').substring(1);
    var tgt = document.getElementById(id);
    if (vidBox) vidBox.style.display = '';
    if (docBox) docBox.style.display = '';
    for (var k = 0; k < fils.length; k++) fils[k].className = 'ec-fil' + (fils[k].getAttribute('data-f') === 'all' ? ' on' : '');
    if (tgt) { e.preventDefault(); tgt.scrollIntoView({ behavior: 'smooth', block: 'start' }); }
  });

  // ---- Eğitim Geçmişim + İçerik Öner modalları ----
  function toggleModal(id, show) { var m = document.getElementById(id); if (m) m.className = 'vid-lightbox' + (show ? ' open' : ''); }
  var bh = document.getElementById('btnHist');
  if (bh) bh.addEventListener('click', function () { toggleModal('histModal', true); });
  var hc = document.getElementById('histClose');
  if (hc) hc.addEventListener('click', function () { toggleModal('histModal', false); });
  var histM = document.getElementById('histModal');
  if (histM) histM.addEventListener('click', function (e) { if (e.target === histM) toggleModal('histModal', false); });
  var bs = document.getElementById('btnSuggest');
  if (bs) bs.addEventListener('click', function () { toggleModal('suggestModal', true); });
  var sc = document.getElementById('suggestClose');
  if (sc) sc.addEventListener('click', function () { toggleModal('suggestModal', false); });
  var sugM = document.getElementById('suggestModal');
  if (sugM) sugM.addEventListener('click', function (e) { if (e.target === sugM) toggleModal('suggestModal', false); });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape') { toggleModal('histModal', false); toggleModal('suggestModal', false); } });

  // ---- Eğitim arama (full-text: başlık + açıklama) ----
  var searchEl = document.getElementById('ecSearch');
  var heroEl = document.getElementById('ecHero');
  var noRes = document.getElementById('ecNoRes');
  var clearBtn = document.getElementById('ecClear');
  var searchT = null;
  function runSearch() {
    if (!searchEl) return;
    var q = searchEl.value.toLowerCase().trim();
    if (q === '') {
      if (heroEl) heroEl.style.display = '';
      var hid = document.querySelectorAll('.ec-hide');
      for (var h = 0; h < hid.length; h++) hid[h].classList.remove('ec-hide');
      if (noRes) noRes.style.display = 'none';
      return;
    }
    if (vidBox) vidBox.style.display = '';
    if (docBox) docBox.style.display = '';
    if (heroEl) heroEl.style.display = 'none';
    var cards = document.querySelectorAll('.ec-rail .ec-card, .ec-rail .ec-doc');
    for (var i = 0; i < cards.length; i++) {
      var c = cards[i];
      var txt = ((c.getAttribute('data-title') || '') + ' ' + (c.getAttribute('data-desc') || '')).toLowerCase();
      if (txt.indexOf(q) >= 0) c.classList.remove('ec-hide'); else c.classList.add('ec-hide');
    }
    var anyRow = false;
    var rows = document.querySelectorAll('.ec-row');
    for (var r = 0; r < rows.length; r++) {
      var vis = rows[r].querySelector('.ec-card:not(.ec-hide), .ec-doc:not(.ec-hide)');
      if (vis) { rows[r].classList.remove('ec-hide'); anyRow = true; } else rows[r].classList.add('ec-hide');
    }
    var grps = document.querySelectorAll('.ec-group');
    for (var g = 0; g < grps.length; g++) {
      var gv = false, n = grps[g].nextElementSibling;
      while (n && !n.classList.contains('ec-group')) {
        if (n.classList.contains('ec-row') && !n.classList.contains('ec-hide')) gv = true;
        n = n.nextElementSibling;
      }
      grps[g].classList.toggle('ec-hide', !gv);
    }
    if (noRes) noRes.style.display = anyRow ? 'none' : '';
  }
  if (searchEl) {
    var freeSearch = function () { searchEl.removeAttribute('readonly'); };
    searchEl.addEventListener('focus', freeSearch);
    searchEl.addEventListener('mousedown', freeSearch);
    searchEl.addEventListener('touchstart', freeSearch);
  }
  if (searchEl) searchEl.addEventListener('input', function () { clearTimeout(searchT); searchT = setTimeout(runSearch, 200); });
  if (searchEl) searchEl.addEventListener('keydown', function (e) { if (e.key === 'Enter') { e.preventDefault(); runSearch(); } });
  if (clearBtn) clearBtn.addEventListener('click', function () { if (searchEl) { searchEl.value = ''; runSearch(); searchEl.focus(); } });

  // ===================== OYNATMA HIZI =====================
  var RATES = [0.75, 1, 1.25];   // 1,25x ustu kaldirildi; eski kayitli 1,5x/2x tercihleri 1x'e doner
  function getRate() { var r = 1; try { r = parseFloat(localStorage.getItem('rv_rate')) || 1; } catch (e) { } return RATES.indexOf(r) >= 0 ? r : 1; }
  function paintRate(r) { var bs = document.querySelectorAll('#vlbSpeed button'); for (var i = 0; i < bs.length; i++) bs[i].className = parseFloat(bs[i].getAttribute('data-r')) === r ? 'on' : ''; }
  function applyRate() { var r = getRate(); paintRate(r); try { if (ytPlayer && ytPlayer.setPlaybackRate) ytPlayer.setPlaybackRate(r); } catch (e) { } }
  // ---- Pencere boyutu: normal / large (bir boy buyuk) / fill (tarayici penceresini doldur); tercih hatirlanir
  var szEl = document.getElementById('vlbSize');
  function applySize(z) {
    if (['normal', 'large', 'fill'].indexOf(z) < 0) z = 'normal';
    lb.setAttribute('data-size', z);                 // sinif degil oznitelik: pencere acilirken className yeniden yaziliyor
    if (szEl) [].forEach.call(szEl.querySelectorAll('button'), function (b) { b.classList.toggle('on', b.getAttribute('data-z') === z); });
    try { localStorage.setItem('vlbSize', z); } catch (e) { }
  }
  if (szEl) {
    var z0 = 'normal'; try { z0 = localStorage.getItem('vlbSize') || 'normal'; } catch (e) { }
    applySize(z0);
    szEl.addEventListener('click', function (e) { var b = e.target.closest ? e.target.closest('button[data-z]') : null; if (b) applySize(b.getAttribute('data-z')); });
  }
  var spEl = document.getElementById('vlbSpeed');
  if (spEl) spEl.addEventListener('click', function (e) {
    var b = e.target.closest ? e.target.closest('button[data-r]') : null; if (!b) return;
    try { localStorage.setItem('rv_rate', b.getAttribute('data-r')); } catch (x) { }
    applyRate();
  });

  // ===================== KALDIGI YERDEN DEVAM =====================
  function mmss(s) { s = Math.max(0, Math.floor(s)); var m = Math.floor(s / 60), r = s % 60; return m + ':' + (r < 10 ? '0' : '') + r; }
  // Son konum 10 sn'den fazlaysa, video bitmemisse ve sonuna 15 sn'den fazla varsa oradan (3 sn geriden) baslat
  function resumeAt(mid) {
    var v = window.VIDS && window.VIDS[mid]; if (!v) return 0;
    var s = v.s || 0, dur = v.dur || 0;
    if (s < 10 || v.d === 1 || (dur > 0 && s > dur - 15)) return 0;
    return Math.max(0, s - 3);
  }
  function showResume(at) {
    var el = document.getElementById('vlbResume'); if (!el) return;
    if (!at) { el.style.display = 'none'; return; }
    el.innerHTML = 'Kaldığınız yerden devam ediliyor (' + mmss(at) + ') · <a href="#" id="vlbRestart">Baştan başla</a>';
    el.style.display = '';
    var a = document.getElementById('vlbRestart');
    if (a) a.onclick = function (e) { e.preventDefault(); try { if (ytPlayer && ytPlayer.seekTo) ytPlayer.seekTo(0, true); } catch (x) { } el.style.display = 'none'; };
  }

  // ===================== SIRADAKI VIDEO (otomatik gecis) =====================
  var queue = [], qIdx = -1, nextTimer = null;
  function setQueueFromCard(card) {
    queue = []; qIdx = -1;
    var rail = card.closest ? card.closest('.ec-rail') : null; if (!rail) return;
    var cs = rail.querySelectorAll('.ec-card[data-mid]');
    for (var i = 0; i < cs.length; i++) { queue.push(parseInt(cs[i].getAttribute('data-mid'), 10)); if (cs[i] === card) qIdx = i; }
  }
  function hideNext() { if (nextTimer) { clearInterval(nextTimer); nextTimer = null; } var el = document.getElementById('vlbNext'); if (el) el.style.display = 'none'; }
  function playMid(mid) { var v = window.VIDS && window.VIDS[mid]; if (v && v.v) openLb(v.v, v.t, mid); }
  function showNext() {
    if (qIdx < 0 || qIdx + 1 >= queue.length) return;
    var nmid = queue[qIdx + 1], nv = window.VIDS && window.VIDS[nmid]; if (!nv) return;
    var el = document.getElementById('vlbNext'); if (!el) return;
    var left = 5;
    function paint() {
      el.innerHTML = '<span class="vn-l">Sıradaki</span><b class="vn-t"></b><span class="vn-c">' + left + ' sn</span>' +
        '<button type="button" class="vn-go">Şimdi oynat</button><button type="button" class="vn-no">İptal</button>';
      el.querySelector('.vn-t').textContent = nv.t;
      el.querySelector('.vn-go').onclick = go; el.querySelector('.vn-no').onclick = hideNext;
    }
    function go() { hideNext(); qIdx++; playMid(nmid); }
    paint(); el.style.display = '';
    nextTimer = setInterval(function () { left--; if (left <= 0) go(); else { var c = el.querySelector('.vn-c'); if (c) c.textContent = left + ' sn'; } }, 1000);
  }
  // Kart tiklamasi: egitim.js'in acilis dinleyicisinden once siralamayi belirle (capture)
  document.addEventListener('click', function (e) {
    var bm = e.target.closest ? e.target.closest('.ec-bm') : null;
    if (bm) { e.preventDefault(); e.stopPropagation(); openPop(bm, parseInt(bm.getAttribute('data-mid'), 10)); return; }
    var card = e.target.closest ? e.target.closest('.ec-card') : null;
    if (card) setQueueFromCard(card);
    else if (e.target.closest && e.target.closest('.hist-open')) { queue = []; qIdx = -1; }
  }, true);

  // ===================== IZLEME LISTELERI =====================
  var WL = window.WL;                         // null -> ozellik kapali (migration 34 calismamis)
  var wlSide = document.getElementById('wlSide'), wlView = document.getElementById('wlView'), ecMain = document.getElementById('ecMain');
  var pop = document.getElementById('wlPop');
  if (!WL) {
    var hide = document.querySelectorAll('.ec-bm, #vlbWl'); for (var h = 0; h < hide.length; h++) hide[h].style.display = 'none';
  }
  function esc(s) { var d = document.createElement('div'); d.textContent = s == null ? '' : s; return d.innerHTML; }
  function api(action, data) {
    var body = Object.keys(data || {}).map(function (k) { return encodeURIComponent(k) + '=' + encodeURIComponent(data[k]); }).join('&');
    return fetch('Watchlist.ashx?action=' + action, { method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: body })
      .then(function (r) { return r.json(); }).then(function (j) { if (!j.ok) throw j; return j; });
  }
  function toast(msg) {
    var t = document.getElementById('wlToast');
    if (!t) { t = document.createElement('div'); t.id = 'wlToast'; t.className = 'wl-toast'; document.body.appendChild(t); }
    t.textContent = msg; t.className = 'wl-toast on';
    clearTimeout(t._h); t._h = setTimeout(function () { t.className = 'wl-toast'; }, 2400);
  }
  function byId(id) { for (var i = 0; i < WL.length; i++) if (WL[i].id === id) return WL[i]; return null; }
  function own() { return WL.filter(function (l) { return l.kind === 'own'; }); }
  var ICON_BM = '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z"/></svg>';
  function scopeLabel(l) { return l.rec ? 'Önerilen' : l.scope === 'company' ? 'Firmayla paylaşıldı' : l.scope === 'users' ? (l.shared.length + ' kişiyle paylaşıldı') : ''; }

  function renderSide() {
    if (!wlSide || !WL) return;
    var o = own(), sh = WL.filter(function (l) { return l.kind === 'shared'; }), rc = WL.filter(function (l) { return l.kind === 'rec'; });
    function item(l) {
      return '<a href="#wl-' + l.id + '" class="wl-it' + (curList === l.id ? ' on' : '') + '" data-l="' + l.id + '">' + ICON_BM +
        '<span class="wl-n">' + esc(l.n) + '</span>' + (l.kind === 'own' && l.scope !== 'private' ? '<i class="wl-sh" title="Paylaşıldı"></i>' : '') + '<span class="wl-c">' + l.items.length + '</span></a>';
    }
    var h = '<div class="wl-h"><span>İzleme Listelerim</span><button type="button" class="wl-add" id="wlNew" title="Yeni liste">+</button></div>';
    h += o.map(item).join('');
    if (sh.length) h += '<div class="wl-h2">Benimle paylaşılanlar</div>' + sh.map(item).join('');
    if (rc.length) h += '<div class="wl-h2">Önerilen listeler</div>' + rc.map(item).join('');
    wlSide.innerHTML = h;
  }
  if (wlSide) wlSide.addEventListener('click', function (e) {
    if (e.target.closest && e.target.closest('#wlNew')) { e.preventDefault(); var n = prompt('Yeni izleme listesinin adı:'); if (n && n.trim()) createList(n.trim(), 0).then(function (l) { openList(l.id); }); return; }
    var a = e.target.closest ? e.target.closest('.wl-it') : null; if (!a) return;
    e.preventDefault(); openList(parseInt(a.getAttribute('data-l'), 10));
  });

  function createList(name, mid) {
    return api('create', { name: name, m: mid || '' }).then(function (j) {
      var l = { id: j.id, n: j.name, def: 0, scope: 'private', rec: 0, kind: 'own', owner: '', items: mid ? [mid] : [], shared: [] };
      WL.splice(own().length, 0, l); renderSide(); if (curList) renderView(); toast('“' + j.name + '” oluşturuldu'); return l;
    }).catch(function (x) { alert((x && x.error) || 'Liste oluşturulamadı.'); throw x; });
  }
  function toggle(l, mid, on) {
    var i = l.items.indexOf(mid);
    if (on && i < 0) l.items.push(mid); if (!on && i >= 0) l.items.splice(i, 1);
    renderSide(); if (curList === l.id) renderView(); paintWlBtn();
    return api('toggle', { l: l.id, m: mid, on: on ? 1 : 0 }).then(function () { toast(on ? '“' + l.n + '” listesine eklendi' : '“' + l.n + '” listesinden çıkarıldı'); })
      .catch(function () { toast('İşlem kaydedilemedi'); });
  }

  // ---- Listeye ekle penceresi (kart / oynatici) ----
  function openPop(anchor, mid) {
    if (!pop || !WL || !mid) return;
    var o = own(), h = '<div class="wl-pop-h">İzleme listesine ekle</div>';
    for (var i = 0; i < o.length; i++) h += '<label class="wl-pop-it"><input type="checkbox" data-l="' + o[i].id + '"' + (o[i].items.indexOf(mid) >= 0 ? ' checked' : '') + ' /><span>' + esc(o[i].n) + '</span><em>' + o[i].items.length + '</em></label>';
    h += '<div class="wl-pop-new"><input type="text" maxlength="120" placeholder="Yeni liste adı" /><button type="button">Oluştur</button></div>';
    pop.innerHTML = h; pop.style.display = 'block';
    var r = anchor.getBoundingClientRect(), w = 270;
    var left = Math.min(window.innerWidth - w - 10, Math.max(10, r.left + r.width / 2 - w / 2));
    var top = r.bottom + 8; if (top + pop.offsetHeight > window.innerHeight - 10) top = Math.max(10, r.top - pop.offsetHeight - 8);
    pop.style.left = left + 'px'; pop.style.top = top + 'px';
    pop.onchange = function (e) { var cb = e.target; if (cb.getAttribute('data-l')) { var l = byId(parseInt(cb.getAttribute('data-l'), 10)); if (l) toggle(l, mid, cb.checked); pop.querySelector('em') && openPop(anchor, mid); } };
    var inp = pop.querySelector('.wl-pop-new input'), btn = pop.querySelector('.wl-pop-new button');
    function mk() { var n = (inp.value || '').trim(); if (!n) { inp.focus(); return; } createList(n, mid).then(function () { openPop(anchor, mid); paintWlBtn(); }); }
    btn.onclick = mk; inp.onkeydown = function (e) { if (e.key === 'Enter') { e.preventDefault(); mk(); } };
  }
  function closePop() { if (pop) pop.style.display = 'none'; }
  document.addEventListener('click', function (e) {
    if (!pop || pop.style.display === 'none') return;
    if (pop.contains(e.target) || (e.target.closest && (e.target.closest('.ec-bm') || e.target.closest('#vlbWl')))) return;
    closePop();
  });
  window.addEventListener('scroll', closePop);   // yalniz sayfa kaydirmasi (kart seridinin ic kaydirmasi pencereyi kapatmasin)
  var wlBtn = document.getElementById('vlbWl');
  function paintWlBtn() {
    if (!wlBtn || !WL) return;
    var n = own().filter(function (l) { return l.items.indexOf(curMid) >= 0; }).length;
    wlBtn.className = 'vlb-wl' + (n ? ' in' : '');
    wlBtn.querySelector('span').textContent = n ? (n === 1 ? 'İzleme listende' : n + ' listede') : 'İzleme listeme ekle';
  }
  if (wlBtn) wlBtn.addEventListener('click', function (e) { e.stopPropagation(); if (pop && pop.style.display === 'block') { closePop(); return; } openPop(wlBtn, curMid); });

  // ---- Liste gorunumu ----
  var curList = 0, dragMid = 0;
  function openList(id) {
    var l = byId(id); if (!l || !ecMain) return;
    curList = id; ecMain.classList.add('wl-mode'); document.body.classList.add('wl-open'); wlView.style.display = '';
    if (location.hash !== '#wl-' + id) history.replaceState(null, '', '#wl-' + id);
    renderView(); renderSide(); window.scrollTo(0, 0);
  }
  function closeList() {
    curList = 0; if (ecMain) ecMain.classList.remove('wl-mode'); document.body.classList.remove('wl-open'); if (wlView) wlView.style.display = 'none';
    history.replaceState(null, '', location.pathname + location.search); renderSide();
  }
  function renderView() {
    var l = byId(curList); if (!l) { closeList(); return; }
    var mine = l.kind === 'own', items = l.items.filter(function (m) { return window.VIDS && window.VIDS[m]; });
    var meta = items.length + ' video' + (mine ? '' : ' · ' + esc(l.owner || '')) + (scopeLabel(l) ? ' · ' + scopeLabel(l) : '');
    var h = '<div class="wl-top"><button type="button" class="btn-ghost wl-back">&#8592; Tüm eğitimler</button></div>' +
      '<div class="wl-head"><div><h2>' + esc(l.n) + '</h2><p>' + meta + '</p></div><div class="wl-acts">' +
      (items.length ? '<button type="button" class="btn-cyan" data-a="play">&#9654; Oynat</button>' : '') +
      (mine ? '<button type="button" class="btn-ghost" data-a="share">Paylaş</button>' + (l.def ? '' : '<button type="button" class="btn-ghost" data-a="rename">Yeniden adlandır</button><button type="button" class="btn-ghost eq-del" data-a="delete">Sil</button>')
            : '<button type="button" class="btn-ghost" data-a="copy">Kendi listeme kopyala</button>') + '</div></div>';
    if (!items.length) h += '<div class="wl-empty">' + (mine ? 'Bu liste henüz boş. Video kartlarındaki ' + ICON_BM + ' simgesine ya da oynatıcıdaki “İzleme listeme ekle” butonuna tıklayarak video ekleyebilirsiniz.' : 'Bu listede görüntüleyebileceğiniz video yok.') + '</div>';
    else {
      h += '<div class="wl-items">';
      for (var i = 0; i < items.length; i++) {
        var m = items[i], v = window.VIDS[m];
        h += '<div class="wl-row"' + (mine ? ' draggable="true"' : '') + ' data-m="' + m + '" data-i="' + i + '">' + (mine ? '<span class="wl-drag" title="Sürükleyerek sırala">&#8942;&#8942;</span>' : '<span class="wl-no">' + (i + 1) + '</span>') +
          '<span class="wl-th"><img src="https://img.youtube.com/vi/' + esc(v.v) + '/mqdefault.jpg" alt="" loading="lazy" /></span>' +
          '<span class="wl-tx"><b>' + esc(v.t) + '</b><small>' + esc(v.g) + '</small>' +
          (v.d ? '<span class="wl-done">&#10003; Tamamlandı</span>' : v.p > 0 ? '<span class="wl-pbar"><i style="width:' + v.p + '%"></i></span>' : '') + '</span>' +
          (mine ? '<button type="button" class="wl-rm" title="Listeden çıkar">&times;</button>' : '') + '</div>';
      }
      h += '</div>';
    }
    wlView.innerHTML = h;
  }
  if (wlView) {
    wlView.addEventListener('click', function (e) {
      var l = byId(curList); if (!l) return;
      if (e.target.closest('.wl-back')) { closeList(); return; }
      var rm = e.target.closest('.wl-rm');
      if (rm) { toggle(l, parseInt(rm.closest('.wl-row').getAttribute('data-m'), 10), false); return; }
      var act = e.target.closest('[data-a]');
      if (act) {
        var a = act.getAttribute('data-a'), items = l.items.filter(function (m) { return window.VIDS && window.VIDS[m]; });
        if (a === 'play' && items.length) { queue = items.slice(); qIdx = 0; playMid(items[0]); }
        else if (a === 'share') openShare(l);
        else if (a === 'rename') { var n = prompt('Yeni ad:', l.n); if (n && n.trim()) { l.n = n.trim().substring(0, 120); api('rename', { l: l.id, name: l.n }); renderView(); renderSide(); } }
        else if (a === 'delete') { if (confirm('“' + l.n + '” listesi silinsin mi?')) { api('delete', { l: l.id }); WL.splice(WL.indexOf(l), 1); closeList(); toast('Liste silindi'); } }
        else if (a === 'copy') api('copy', { l: l.id }).then(function (j) { var c = { id: j.id, n: l.n, def: 0, scope: 'private', rec: 0, kind: 'own', owner: '', items: l.items.slice(), shared: [] }; WL.splice(own().length, 0, c); openList(c.id); toast('Listeniz oluşturuldu'); });
        return;
      }
      var row = e.target.closest('.wl-row');
      if (row && !e.target.closest('.wl-drag')) {
        var its = l.items.filter(function (m) { return window.VIDS && window.VIDS[m]; });
        queue = its.slice(); qIdx = parseInt(row.getAttribute('data-i'), 10); playMid(its[qIdx]);
      }
    });
    wlView.addEventListener('dragstart', function (e) { var r = e.target.closest && e.target.closest('.wl-row'); if (!r) return; dragMid = parseInt(r.getAttribute('data-m'), 10); r.classList.add('drag'); try { e.dataTransfer.setData('text/plain', dragMid); } catch (x) { } });
    wlView.addEventListener('dragend', function (e) { var r = e.target.closest && e.target.closest('.wl-row'); if (r) r.classList.remove('drag'); });
    wlView.addEventListener('dragover', function (e) { if (dragMid && e.target.closest && e.target.closest('.wl-row')) e.preventDefault(); });
    wlView.addEventListener('drop', function (e) {
      var r = e.target.closest && e.target.closest('.wl-row'), l = byId(curList); if (!r || !l || !dragMid) return;
      e.preventDefault();
      var to = parseInt(r.getAttribute('data-m'), 10); if (to === dragMid) return;
      var from = l.items.indexOf(dragMid); l.items.splice(from, 1);
      l.items.splice(l.items.indexOf(to) + (from <= l.items.indexOf(to) ? 1 : 0), 0, dragMid);
      dragMid = 0; renderView(); api('reorder', { l: l.id, mids: l.items.join(',') });
    });
  }

  // ---- Paylasim penceresi ----
  var shareModal = document.getElementById('wlShareModal'), shareList = null, colleagues = null;
  function scopeVal() { var r = document.querySelector('input[name=wlscope]:checked'); return r ? r.value : 'private'; }
  function renderPeople() {
    var box = document.getElementById('wlPeople'); if (!box) return;
    box.style.display = scopeVal() === 'users' ? '' : 'none';
    if (scopeVal() !== 'users') return;
    if (!colleagues) {
      box.innerHTML = '<div class="wl-empty">Yükleniyor…</div>';
      fetch('Watchlist.ashx?action=colleagues', { credentials: 'same-origin' }).then(function (r) { return r.json(); }).then(function (j) { colleagues = j.users || []; renderPeople(); });
      return;
    }
    if (!colleagues.length) { box.innerHTML = '<div class="wl-empty">Firmanızda paylaşabileceğiniz başka kullanıcı yok.</div>'; return; }
    box.innerHTML = colleagues.map(function (u) { return '<label class="wl-person"><input type="checkbox" value="' + u.id + '"' + (shareList.shared.indexOf(u.id) >= 0 ? ' checked' : '') + ' /> ' + esc(u.name) + '</label>'; }).join('');
  }
  function openShare(l) {
    if (!shareModal) return;
    shareList = l;
    document.getElementById('wlShareName').textContent = '“' + l.n + '” · ' + l.items.length + ' video';
    var rs = document.querySelectorAll('input[name=wlscope]'); for (var i = 0; i < rs.length; i++) rs[i].checked = rs[i].value === l.scope;
    var rec = document.getElementById('wlRec'); if (rec) rec.checked = !!l.rec;
    renderPeople(); shareModal.className = 'vid-lightbox open';
  }
  function closeShare() { if (shareModal) shareModal.className = 'vid-lightbox'; }
  if (shareModal) {
    shareModal.addEventListener('change', function (e) { if (e.target.name === 'wlscope') renderPeople(); });
    document.getElementById('wlShareClose').onclick = closeShare;
    document.getElementById('wlShareCancel').onclick = closeShare;
    shareModal.addEventListener('click', function (e) { if (e.target === shareModal) closeShare(); });
    document.getElementById('wlShareSave').onclick = function () {
      var l = shareList, scope = scopeVal(), uids = [];
      var cbs = document.querySelectorAll('#wlPeople input:checked'); for (var i = 0; i < cbs.length; i++) uids.push(parseInt(cbs[i].value, 10));
      if (scope === 'users' && !uids.length) { alert('Lütfen en az bir kişi seçin.'); return; }
      var rec = document.getElementById('wlRec'), recOn = rec ? rec.checked : !!l.rec;
      api('share', { l: l.id, scope: scope, uids: uids.join(','), rec: recOn ? 1 : 0 }).then(function (j) {
        l.scope = scope; l.shared = scope === 'users' ? uids : []; l.rec = recOn ? 1 : 0;
        closeShare(); renderView(); renderSide();
        toast(scope === 'private' && !recOn ? 'Liste artık yalnız size görünür' : 'Paylaşım kaydedildi' + (j.notified ? ' · ' + j.notified + ' kişiye bildirim gitti' : ''));
      }).catch(function (x) { alert((x && x.error) || 'Paylaşım kaydedilemedi.'); });
    };
  }

  if (WL) {
    renderSide();
    var hm = /^#wl-(\d+)$/.exec(location.hash || '');
    if (hm) openList(parseInt(hm[1], 10));
  }

  // ===================== SEKME DEGISINCE DURAKLAT =====================
  // Kullanici baska sekmeye gecince / tarayiciyi kucultunce oynayan video duraklatilir; donuste bilgi gosterilir.
  // (window blur kullanilmaz: oynaticinin kendi kontrollerine tiklamak da blur tetikler.)
  var pausedByTab = false, tabNoteOn = false;
  function hideTabNote() { if (!tabNoteOn) return; tabNoteOn = false; var el = document.getElementById('vlbResume'); if (el) el.style.display = 'none'; }
  function showTabNote() {
    var el = document.getElementById('vlbResume'); if (!el) return;
    el.innerHTML = 'Başka sekmeye geçtiğiniz için video duraklatıldı · <a href="#" id="vlbTabResume">Devam et</a>';
    el.style.display = ''; tabNoteOn = true;
    var a = document.getElementById('vlbTabResume');
    if (a) a.onclick = function (e) { e.preventDefault(); try { if (ytPlayer && ytPlayer.playVideo) ytPlayer.playVideo(); } catch (x) { } hideTabNote(); };
  }
  document.addEventListener('visibilitychange', function () {
    if (!curMid || !ytPlayer || !ytPlayer.getPlayerState) return;
    try {
      if (document.hidden) {
        var st = ytPlayer.getPlayerState();
        if (st === 1 || st === 3) { ytPlayer.pauseVideo(); pausedByTab = true; pingProgress(); }   // oynuyor / arabellekte
      } else if (pausedByTab) { pausedByTab = false; showTabNote(); }
    } catch (e) { }
  });

  setLang(INIT);

  // Derin link: Training.aspx#play-{MaterialId} -> ilgili videoyu ac (sinav sartlarindaki izlenmemis video linkleri)
  (function () {
    var m = /^#play-(\d+)$/.exec(location.hash || '');
    if (!m) return;
    var el = document.querySelector('[data-mid="' + m[1] + '"][data-vid]');
    if (el) { try { el.scrollIntoView({ block: 'center' }); } catch (e) { } openLb(el.getAttribute('data-vid'), el.getAttribute('data-title'), el.getAttribute('data-mid')); }
  })();
})();

// ===================== Devam Et: uc nokta menusu → "Devam Et'ten kaldir" (ilerleme sifirlanir) =====================
(function () {
  var menu = null, menuMid = 0, menuBtn = null, dlg = null;
  function closeMenu() { if (menu) { menu.remove(); menu = null; } if (menuBtn) { menuBtn.setAttribute('aria-expanded', 'false'); menuBtn = null; } }
  function openMenu(btn) {
    closeMenu();
    menuBtn = btn; menuMid = +btn.getAttribute('data-mid'); btn.setAttribute('aria-expanded', 'true');
    menu = document.createElement('div'); menu.className = 'ec-menu'; menu.setAttribute('role', 'menu');
    menu.innerHTML = '<button type="button" role="menuitem" class="ec-menu-it danger"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 12a9 9 0 1 0 3-6.7"/><path d="M3 4v5h5"/></svg><span>Devam Et’ten kaldır</span></button>';
    document.body.appendChild(menu);
    var r = btn.getBoundingClientRect(), w = menu.offsetWidth;
    menu.style.top = (r.bottom + window.scrollY + 6) + 'px';
    menu.style.left = Math.max(8, Math.min(window.innerWidth - w - 8, r.right + window.scrollX - w)) + 'px';
    menu.querySelector('button').addEventListener('click', function () { var m = menuMid; closeMenu(); confirmReset(m); });
    menu.querySelector('button').focus();
  }
  function confirmReset(mid) {
    var card = document.querySelector('#row-continue .ec-card[data-mid="' + mid + '"]'), title = card ? card.getAttribute('data-title') : '';
    dlg = document.createElement('div'); dlg.className = 'ec-cfm'; dlg.setAttribute('role', 'alertdialog'); dlg.setAttribute('aria-modal', 'true'); dlg.setAttribute('aria-labelledby', 'ecCfmT');
    dlg.innerHTML = '<div class="ec-cfm-box"><div class="ec-cfm-ic"><svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 12a9 9 0 1 0 3-6.7"/><path d="M3 4v5h5"/></svg></div>' +
      '<h3 id="ecCfmT">Devam Et’ten kaldırılsın mı?</h3><p class="ec-cfm-v"></p>' +
      '<p>Bu videodaki <b>izleme ilerlemeniz sıfırlanacak</b>: izlenme oranı, kaldığınız yer ve izleme süreniz silinir. Video bir dahaki açılışta baştan başlar. Kazandığınız rozetler ve sertifikalar etkilenmez.</p>' +
      '<p class="ec-cfm-warn">Bu işlem geri alınamaz.</p><div class="ec-cfm-msg" role="status"></div>' +
      '<div class="ec-cfm-act"><button type="button" class="btn-ghost ec-cfm-no">Vazgeç</button><button type="button" class="ec-cfm-yes">Sıfırla ve kaldır</button></div></div>';
    dlg.querySelector('.ec-cfm-v').textContent = title;
    document.body.appendChild(dlg);
    var yes = dlg.querySelector('.ec-cfm-yes'), no = dlg.querySelector('.ec-cfm-no');
    no.focus();
    no.addEventListener('click', closeDlg);
    dlg.addEventListener('click', function (e) { if (e.target === dlg) closeDlg(); });
    yes.addEventListener('click', function () {
      yes.disabled = no.disabled = true; yes.textContent = 'Sıfırlanıyor…';
      var fd = new FormData(); fd.append('action', 'reset'); fd.append('mid', mid);
      fetch('TrainingTrack.aspx', { method: 'POST', body: fd, credentials: 'same-origin' }).then(function (r) { return r.text(); }).then(function (s) {
        if ((s || '').trim() !== 'ok') throw 0;
        closeDlg(); applyReset(mid);
      }).catch(function () {
        yes.disabled = no.disabled = false; yes.textContent = 'Sıfırla ve kaldır';
        dlg.querySelector('.ec-cfm-msg').textContent = 'İşlem tamamlanamadı, lütfen tekrar deneyin.';
      });
    });
  }
  function closeDlg() { if (dlg) { dlg.remove(); dlg = null; } }
  function applyReset(mid) {
    if (window.VIDS && window.VIDS[mid]) { window.VIDS[mid].s = 0; window.VIDS[mid].d = 0; }   // bir dahaki acilista bastan
    [].forEach.call(document.querySelectorAll('.ec-card[data-mid="' + mid + '"] .ec-prog'), function (p) { p.remove(); });
    [].forEach.call(document.querySelectorAll('.ec-card[data-mid="' + mid + '"] .ec-dur.up'), function (d) { d.classList.remove('up'); });
    var card = document.querySelector('#row-continue .ec-card[data-mid="' + mid + '"]');
    if (card) {
      card.classList.add('ec-leaving');
      setTimeout(function () {
        var row = document.getElementById('row-continue'); card.remove();
        if (row && !row.querySelector('.ec-card')) row.remove();
      }, 320);
    }
    var t = document.createElement('div'); t.className = 'ec-toast'; t.setAttribute('role', 'status'); t.textContent = 'Video Devam Et’ten kaldırıldı, ilerleme sıfırlandı.';
    document.body.appendChild(t); requestAnimationFrame(function () { t.classList.add('in'); });
    setTimeout(function () { t.classList.remove('in'); setTimeout(function () { t.remove(); }, 300); }, 3200);
    if (window.pathAfterVideo) window.pathAfterVideo();   // sol menudeki yol ilerlemesi tazelenir
  }
  document.addEventListener('click', function (e) {
    var b = e.target.closest ? e.target.closest('.ec-more') : null;
    if (b) { e.preventDefault(); e.stopPropagation(); if (menuBtn === b) closeMenu(); else openMenu(b); return; }
    if (menu && !(e.target.closest && e.target.closest('.ec-menu'))) closeMenu();
  }, true);
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape') { if (dlg) closeDlg(); else closeMenu(); } });
  window.addEventListener('resize', closeMenu);
  document.addEventListener('scroll', closeMenu, true);
})();

