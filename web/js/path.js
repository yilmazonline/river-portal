// "Sifirdan Uzmanliga" ogrenme yolu: menu butonu, rozetler, yol ekrani, kutlama, editor siralamasi.
// Akademi koduyla yalniz iki noktadan konusur: window.pathAfterVideo (video kapaninca/bitince) ve .hist-open (video acma).
(function () {
  var P = window.PATH;
  var edTab = document.querySelector('.ed-tab[data-t="path"]');
  if (!P) { if (edTab) edTab.style.display = 'none'; return; }   // tablo yok (migration calismamis): ozellik gizli

  function $(id) { return document.getElementById(id); }
  function esc(s) { return String(s == null ? '' : s).replace(/[&<>"]/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]; }); }
  var LOCK = '<svg width="14" height="14" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2a5 5 0 0 1 5 5v3h1a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2v-8a2 2 0 0 1 2-2h1V7a5 5 0 0 1 5-5zm0 2a3 3 0 0 0-3 3v3h6V7a3 3 0 0 0-3-3z"/></svg>';
  var modal = $('pathModal');

  // ---------------- cizim ----------------
  function render() {
    var feat = $('lpFeat'), has = P.total > 0;
    if (feat) feat.style.display = has ? '' : 'none';
    if ($('lpBadges')) $('lpBadges').style.display = has ? '' : 'none';
    if (!has) return;
    var all = P.done === P.total;
    // kisisel kart: segmentli halka + baslik
    feat.classList.toggle('done', all);
    $('apRing').innerHTML = segRing(P.done, P.total, 76);
    $('apTitle').textContent = P.done + ' / ' + P.total + ' adım';
    var nx = nextMilestone();
    $('apSub').textContent = all ? 'Yolu tamamladınız' : (nx ? nx.title + ' rozetine ' + nx.left + ' adım' : '');
    // Devam et: siradaki adim (kaldigi saniyeden acilir)
    var go = $('apGo'), st = null;
    P.steps.forEach(function (s) { if (!st && s.state === 'next') st = s; });
    if (st && st.vid) {
      go.style.display = '';
      go.setAttribute('data-vid', st.vid); go.setAttribute('data-mid', st.mid); go.setAttribute('data-title', st.title);
      $('apGoRing').innerHTML = miniRing(st.pct, st.no);
      $('apGoK').textContent = st.skim ? 'BAŞTAN İZLE · ADIM ' + st.no : (st.pct > 0 ? 'DEVAM ET · %' + st.pct : 'SIRADAKİ · ADIM ' + st.no);
      $('apGoT').textContent = st.title;
      $('apGoBar').style.width = (st.pct || 0) + '%';
      go.classList.remove('done');
    } else if (all) {
      go.style.display = ''; go.removeAttribute('data-vid'); go.classList.add('done');
      $('apGoRing').innerHTML = miniRing(100, '✓'); $('apGoK').textContent = 'YOL TAMAMLANDI'; $('apGoT').textContent = 'Tebrikler, tüm adımlar bitti'; $('apGoBar').style.width = '100%';
    } else go.style.display = 'none';
    // siradaki rozet seridi
    $('apNext').innerHTML = nx && !all ? '<span class="ac-next-b">' + nx.svg + '</span><span class="ac-next-tx"><small>SIRADAKİ ROZET</small><b>' + esc(nx.title) + ' · ' + nx.left + ' adım kaldı</b>' +
      '<span class="ac-next-bar"><i style="width:' + Math.round(nx.got * 100 / nx.need) + '%"></i></span></span>' : '';
    $('apNext').style.display = nx && !all ? '' : 'none';
    // rozetler: adim rozetleri + kilometre taslari
    var g = '', earned = 0, totalB = P.steps.length + P.milestones.length;
    P.steps.forEach(function (s) {
      var on = s.state === 'done'; if (on) earned++;
      g += '<span class="lb' + (on ? ' on' : '') + '" title="' + esc(s.no + '. ' + s.title + (on ? ' · Kazanıldı' : ' · Henüz kazanılmadı')) + '">' + s.svg + '</span>';
    });
    P.milestones.forEach(function (m) {
      if (m.earned) earned++;
      g += '<span class="lb lb-ms' + (m.earned ? ' on' : '') + '" title="' + esc(m.title + (m.earned ? ' · Kazanıldı' : ' · ' + m.hint)) + '">' + m.svg + '</span>';
    });
    $('lpBadgeGrid').innerHTML = g; $('lpBadgeCnt').textContent = earned + '/' + totalB;
    if ($('apBadgeN')) $('apBadgeN').textContent = earned;
    renderModal();
  }
  // kisisel kart halkasi: her adim ayri bir segment
  function segRing(done, total, size) {
    var c = 38, r = 31, s = '<svg viewBox="0 0 76 76" width="' + size + '" height="' + size + '" aria-label="' + total + ' adımın ' + done + '\'i tamamlandı"><defs><linearGradient id="apRg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#4F7BFF"/><stop offset=".55" stop-color="#A06BFF"/><stop offset="1" stop-color="#D650FA"/></linearGradient></defs>';
    var n = Math.max(1, total), gap = n > 1 ? 0.16 : 0;
    for (var i = 0; i < n; i++) {
      var a0 = i / n * Math.PI * 2 - Math.PI / 2 + gap / 2, a1 = (i + 1) / n * Math.PI * 2 - Math.PI / 2 - gap / 2;
      var p = function (a) { return (c + r * Math.cos(a)).toFixed(2) + ' ' + (c + r * Math.sin(a)).toFixed(2); };
      s += '<path d="M ' + p(a0) + ' A ' + r + ' ' + r + ' 0 ' + (a1 - a0 > Math.PI ? 1 : 0) + ' 1 ' + p(a1) + '" fill="none" stroke-width="7" stroke-linecap="round" stroke="' + (i < done ? 'url(#apRg)' : 'currentColor') + '"' + (i < done ? '' : ' stroke-opacity=".14"') + '/>';
    }
    return s + '<text x="38" y="43" text-anchor="middle" font-family="Sora,sans-serif" font-size="16" font-weight="800" fill="currentColor">' + done + '/' + total + '</text></svg>';
  }
  // Devam et halkasi: izlenme orani + ortada adim numarasi (oynat ucgeni degil)
  function miniRing(pct, label) {
    var c = 113.1, off = c * (1 - Math.min(100, pct || 0) / 100);
    return '<svg viewBox="0 0 44 44" width="42" height="42" aria-hidden="true"><circle cx="22" cy="22" r="18" fill="rgba(255,255,255,.12)" stroke="rgba(255,255,255,.28)" stroke-width="3.5"/>' +
      '<circle cx="22" cy="22" r="18" fill="none" stroke="#fff" stroke-width="3.5" stroke-linecap="round" stroke-dasharray="' + c + '" stroke-dashoffset="' + off.toFixed(1) + '" transform="rotate(-90 22 22)"/>' +
      '<text x="22" y="27.5" text-anchor="middle" font-family="Sora,sans-serif" font-size="15" font-weight="800" fill="#fff">' + esc(label) + '</text></svg>';
  }
  // henuz kazanilmamis ilk kilometre tasi ve kalan adim
  function nextMilestone() {
    var n = P.total, d = P.done, need = [['m:first', 1], ['m:half', Math.ceil(n / 2)], ['path:done', n]];
    for (var i = 0; i < P.milestones.length; i++) {
      var m = P.milestones[i]; if (m.earned) continue;
      var req = 1; need.forEach(function (x) { if (x[0] === m.key) req = x[1]; });
      if (n < 2 && m.key === 'm:half') continue;
      return { title: m.title, svg: m.svg, need: req, got: Math.min(d, req), left: Math.max(1, req - d) };
    }
    return null;
  }
  function ring(done, total) {
    var r = 30, c = 2 * Math.PI * r, f = total ? done / total : 0;
    return '<svg viewBox="0 0 76 76" width="76" height="76"><defs><linearGradient id="lpRg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#4F7BFF"/><stop offset=".55" stop-color="#A06BFF"/><stop offset="1" stop-color="#D650FA"/></linearGradient></defs>' +
      '<circle cx="38" cy="38" r="' + r + '" fill="none" stroke="currentColor" stroke-opacity=".14" stroke-width="7"/>' +
      '<circle cx="38" cy="38" r="' + r + '" fill="none" stroke="url(#lpRg)" stroke-width="7" stroke-linecap="round" stroke-dasharray="' + c.toFixed(1) + '" stroke-dashoffset="' + (c * (1 - f)).toFixed(1) + '" transform="rotate(-90 38 38)"/>' +
      '<text x="38" y="37" text-anchor="middle" font-size="16" font-weight="800" fill="currentColor">' + done + '/' + total + '</text><text x="38" y="51" text-anchor="middle" font-size="8.5" fill="currentColor" fill-opacity=".6">ADIM</text></svg>';
  }
  function renderModal() {
    if (!modal) return;
    $('lpRing').innerHTML = ring(P.done, P.total);
    var h = '', prevTitle = '';
    P.steps.forEach(function (s, i) {
      var open = ' hist-open" data-vid="' + esc(s.vid) + '" data-mid="' + s.mid + '" data-title="' + esc(s.title) + '"';
      var act = '', chip = '', note = '';
      if (s.state === 'done') { chip = '<span class="lp-chip ok">Tamamlandı</span>'; act = s.vid ? '<button type="button" class="lp-act ghost' + open + '>Tekrar izle</button>' : ''; }
      else if (s.state === 'next') {
        if (s.skim) { chip = '<span class="lp-chip warn">Hızlı geçilmiş</span>'; note = '<div class="lp-note">Bu videonun yarısından azını izlemişsiniz. Adımı tamamlamak için baştan sona izleyin.</div>'; act = '<button type="button" class="lp-act' + open + '>Baştan izle</button>'; }
        else if (s.pct > 0) { chip = '<span class="lp-chip info">%' + s.pct + ' izlendi</span>'; act = '<button type="button" class="lp-act' + open + '>Devam et</button>'; }
        else { chip = '<span class="lp-chip next">Sıradaki</span>'; act = '<button type="button" class="lp-act' + open + '>Başla</button>'; }
      }
      else { chip = '<span class="lp-chip lock">' + LOCK + 'Kilitli</span>'; act = '<span class="lp-lock" title="' + esc('Önce “' + prevTitle + '” adımını tamamlayın') + '">' + LOCK + '</span>'; }
      h += '<div class="lp-step ' + s.state + '"><span class="lp-badge">' + s.svg + '</span><div class="lp-st"><small>ADIM ' + s.no + ' · ' + esc(s.grp) + '</small><b>' + esc(s.title) + '</b>' +
           (s.state === 'locked' ? '<em>Önce “' + esc(prevTitle) + '” adımını tamamlayın</em>' : '') + note + '</div>' + chip + act + '</div>';
      if (i < P.steps.length - 1) h += '<div class="lp-link ' + (s.state === 'done' ? 'done' : '') + '"></div>';
      prevTitle = s.title;
    });
    $('lpSteps').innerHTML = h;
    var m = '<div class="lp-ms-h">Kilometre taşları</div><div class="lp-ms-row">';
    P.milestones.forEach(function (x) { m += '<div class="lp-ms-it' + (x.earned ? ' on' : '') + '">' + x.svg + '<b>' + esc(x.title) + '</b><small>' + (x.earned ? 'Kazanıldı' : esc(x.hint)) + '</small></div>'; });
    $('lpMs').innerHTML = m + '</div>';
  }

  // ---------------- yol ekrani ----------------
  function openModal() { if (!modal) return; renderModal(); modal.className = 'vid-lightbox open'; }
  function closeModal() { if (modal) modal.className = 'vid-lightbox'; }
  if ($('lpFeat')) $('lpFeat').addEventListener('click', openModal);
  if ($('pathClose')) $('pathClose').addEventListener('click', closeModal);
  if (modal) {
    modal.addEventListener('click', function (e) {
      if (e.target === modal) { closeModal(); return; }
      var b = e.target.closest ? e.target.closest('.hist-open') : null;
      if (b) closeModal();          // video acilmadan once yol ekranini kapat (video belge dinleyicisiyle acilir)
    });
  }
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && modal && modal.className.indexOf('open') >= 0) closeModal(); });

  // ---------------- sunucudan tazele + kutlama ----------------
  function refresh() {
    return fetch('PathApi.ashx?op=state', { credentials: 'same-origin', cache: 'no-store' }).then(function (r) { return r.json(); })
      .then(function (d) { if (d && d.ok) { P = d; window.PATH = d; render(); } }).catch(function () { });
  }
  var queue = [], showing = false;
  function celebrate() {
    return fetch('PathApi.ashx?op=celebrate', { credentials: 'same-origin', cache: 'no-store' }).then(function (r) { return r.json(); })
      .then(function (d) { if (d && d.ok && d.items && d.items.length) { queue = queue.concat(d.items); if (!showing) next(); } }).catch(function () { });
  }
  // Steam tarzi: video rozetleri ve kilometre taslari sol altta kucuk kart; yolun tamamlanmasi buyuk kutlama
  var toastBox = null;
  function toast(it) {
    if (!toastBox) { toastBox = document.createElement('div'); toastBox.className = 'lp-toasts'; toastBox.setAttribute('aria-live', 'polite'); document.body.appendChild(toastBox); }
    var ms = it.key.indexOf('m:') === 0, done = it.key === 'path:done';
    var el = document.createElement('div'); el.className = 'lp-toast' + (ms || done ? ' ms' : '');
    el.innerHTML = '<span class="lp-toast-b">' + it.svg + '</span><div class="lp-toast-t"><small>' + (done ? 'YOL TAMAMLANDI' : (ms ? 'KİLOMETRE TAŞI' : 'ROZET KAZANILDI')) + '</small>' +
      '<b>' + esc(it.title) + '</b><em>Sıfırdan Uzmanlığa · ' + (P && P.total ? P.done + '/' + P.total : '') + '</em></div><i class="lp-toast-life"></i>';
    el.addEventListener('click', function () { hide(); openModal(); });
    toastBox.appendChild(el);
    requestAnimationFrame(function () { el.classList.add('in'); });
    var timer = setTimeout(hide, 6000);
    function hide() { clearTimeout(timer); el.classList.remove('in'); el.classList.add('out'); setTimeout(function () { if (el.parentNode) el.parentNode.removeChild(el); }, 450); }
  }
  function next() {
    var it = queue.shift();
    if (!it) { showing = false; return; }
    showing = true;
    toast(it);
    if (it.key === 'path:done') { setTimeout(function () { bigCelebrate(it); }, 900); return; }   // buyuk kutlama "Harika!" ile devam eder
    setTimeout(next, 750);
  }
  function bigCelebrate(it) {
    var ov = $('lpCele'); if (!ov) { next(); return; }
    $('lpCeleBadge').innerHTML = it.svg;
    $('lpCeleK').textContent = 'YOL TAMAMLANDI';
    $('lpCeleT').textContent = 'Tebrikler, Sıfırdan Uzmanlığa yolunu tamamladınız!';
    $('lpCeleP').textContent = 'Tüm adımları önerilen sırayla tamamladınız. Bu rozet artık profilinizde.';
    ov.className = 'lp-cele open';
    confetti(260);
  }
  if ($('lpCeleOk')) $('lpCeleOk').addEventListener('click', function () { $('lpCele').className = 'lp-cele'; setTimeout(next, 250); });

  function confetti(n) {
    var cv = $('lpConf'); if (!cv || (window.matchMedia && matchMedia('(prefers-reduced-motion: reduce)').matches)) return;
    var ctx = cv.getContext('2d'), W = cv.width = innerWidth, H = cv.height = innerHeight, cols = ['#6C8CFF', '#A06BFF', '#D650FA', '#22B8D8', '#F2C14E', '#2FD08A'], ps = [];
    for (var i = 0; i < n; i++) ps.push({ x: W / 2 + (Math.random() - .5) * 120, y: H * .42, vx: (Math.random() - .5) * 14, vy: -Math.random() * 14 - 4, r: Math.random() * 6 + 3, c: cols[i % cols.length], a: Math.random() * 6, va: (Math.random() - .5) * .3 });
    var t0 = performance.now();
    (function frame(t) {
      var k = (t - t0) / 1000; ctx.clearRect(0, 0, W, H);
      ps.forEach(function (p) { p.vy += .35; p.vx *= .99; p.x += p.vx; p.y += p.vy; p.a += p.va; ctx.save(); ctx.translate(p.x, p.y); ctx.rotate(p.a); ctx.globalAlpha = Math.max(0, 1 - k / 2.8); ctx.fillStyle = p.c; ctx.fillRect(-p.r / 2, -p.r / 4, p.r, p.r / 2); ctx.restore(); });
      if (k < 2.8) requestAnimationFrame(frame); else ctx.clearRect(0, 0, W, H);
    })(t0);
  }

  window.pathAfterVideo = function () { refresh().then(celebrate); };

  // ---------------- editor: siralama ----------------
  function initEditor() {
    if (!window.PATH_EDIT || !$('lpEdIn')) return;
    var all = window.PATH_ADMIN || [], inPath = all.filter(function (x) { return x.inPath; }).map(function (x) { return x.mid; }), byId = {};
    all.forEach(function (x) { byId[x.mid] = x; });
    var drag = null;
    function draw() {
      $('lpEdCnt').textContent = '(' + inPath.length + ')';
      $('lpEdIn').innerHTML = inPath.map(function (m, i) { var x = byId[m] || { title: '#' + m, grp: '' };
        return '<li draggable="true" data-m="' + m + '"><span class="lp-grip">⠿</span><span class="lp-no">' + (i + 1) + '</span><div><b>' + esc(x.title) + '</b><small>' + esc(x.grp) + '</small></div><button type="button" class="lp-rm" data-m="' + m + '" aria-label="Çıkar">×</button></li>'; }).join('') ||
        '<li class="lp-empty">Yolda adım yok. Sağdan video ekleyin.</li>';
      var q = ($('lpEdQ').value || '').toLowerCase();
      $('lpEdOut').innerHTML = all.filter(function (x) { return inPath.indexOf(x.mid) < 0 && (!q || (x.title + ' ' + x.grp).toLowerCase().indexOf(q) >= 0); })
        .map(function (x) { return '<li><div><b>' + esc(x.title) + '</b><small>' + esc(x.grp) + '</small></div><button type="button" class="lp-add" data-m="' + x.mid + '">+ Ekle</button></li>'; }).join('') || '<li class="lp-empty">Eklenecek video yok.</li>';
    }
    $('lpEdQ').addEventListener('input', draw);
    $('lpEdOut').addEventListener('click', function (e) { var b = e.target.closest('.lp-add'); if (b) { inPath.push(+b.getAttribute('data-m')); draw(); } });
    $('lpEdIn').addEventListener('click', function (e) { var b = e.target.closest('.lp-rm'); if (b) { inPath.splice(inPath.indexOf(+b.getAttribute('data-m')), 1); draw(); } });
    $('lpEdIn').addEventListener('dragstart', function (e) { var li = e.target.closest('li[data-m]'); if (!li) return; drag = +li.getAttribute('data-m'); li.classList.add('drag'); e.dataTransfer.effectAllowed = 'move'; try { e.dataTransfer.setData('text/plain', String(drag)); } catch (x) { } });
    $('lpEdIn').addEventListener('dragend', function () { drag = null; draw(); });
    $('lpEdIn').addEventListener('dragover', function (e) {
      e.preventDefault(); var li = e.target.closest('li[data-m]'); if (!li || drag === null) return;
      var over = +li.getAttribute('data-m'); if (over === drag) return;
      var r = li.getBoundingClientRect(), after = e.clientY > r.top + r.height / 2;
      inPath.splice(inPath.indexOf(drag), 1); var at = inPath.indexOf(over) + (after ? 1 : 0); inPath.splice(at, 0, drag); draw();
      var nd = $('lpEdIn').querySelector('li[data-m="' + drag + '"]'); if (nd) nd.classList.add('drag');
    });
    $('lpEdSave').addEventListener('click', function () {
      var b = this; b.disabled = true; $('lpEdMsg').textContent = 'Kaydediliyor…';
      var fd = new FormData(); fd.append('mids', inPath.join(','));
      fetch('PathApi.ashx?op=save', { method: 'POST', body: fd, credentials: 'same-origin' }).then(function (r) { return r.json(); })
        .then(function (d) { $('lpEdMsg').textContent = d.ok ? 'Kaydedildi: ' + d.count + ' adım.' : 'Kaydedilemedi.'; if (d.ok) refresh(); })
        .catch(function () { $('lpEdMsg').textContent = 'Bağlantı hatası.'; }).then(function () { b.disabled = false; });
    });
    draw();
  }

  // ---------------- takim arkadaslari ----------------
  var tmBtn = $('btnTeam'), tmModal = $('teamModal'), tmData = null;
  function initials(n) { var p = String(n || '').trim().split(/\s+/); return ((p[0] || '').charAt(0) + (p.length > 1 ? p[p.length - 1].charAt(0) : '')).toUpperCase(); }
  function tmRender() {
    var q = ($('tmQ').value || '').toLocaleLowerCase('tr'), items = (tmData || []).filter(function (x) { return !q || x.name.toLocaleLowerCase('tr').indexOf(q) >= 0; });
    if (!tmData) return;
    if (!tmData.length) { $('tmList').innerHTML = '<div class="tm-empty">Firmanızda kayıtlı başka kişi yok.</div>'; return; }
    if (!items.length) { $('tmList').innerHTML = '<div class="tm-empty">Aramanızla eşleşen kişi yok.</div>'; return; }
    $('tmList').innerHTML = items.map(function (x) {
      var ava = x.ava ? '<span class="tm-ava"><img src="' + esc(x.ava) + '" alt="" /></span>' : '<span class="tm-ava">' + esc(initials(x.name)) + '</span>';
      if (!x.share) return '<div class="tm-card off">' + ava + '<div class="tm-main"><b>' + esc(x.name) + '</b><small>Başarılarını paylaşmıyor</small></div></div>';
      var pct = x.total ? Math.round(x.done * 100 / x.total) : 0, all = x.total && x.done === x.total;
      var badges = x.badges.length ? x.badges.map(function (b) { return '<span class="tm-b" title="' + esc(b.t) + '">' + b.svg + '</span>'; }).join('') : '<span class="tm-none">Henüz rozet yok</span>';
      var certs = x.certs.map(function (c) { return '<a class="tm-cert' + (c.exp ? ' exp' : '') + '" href="' + esc(c.url) + '" target="_blank" rel="noopener" title="Sertifikayı doğrula">' + c.svg + '<span><b>' + esc(c.t) + '</b><small>' + esc(c.d) + (c.exp ? ' · süresi doldu' : '') + '</small></span></a>'; }).join('');
      return '<div class="tm-card">' + ava + '<div class="tm-main"><div class="tm-top"><b>' + esc(x.name) + '</b>' + (all ? '<span class="tm-done">🏆 Yolu tamamladı</span>' : '') + '</div>' +
        (x.total ? '<div class="tm-prog"><span>Sıfırdan Uzmanlığa · ' + x.done + '/' + x.total + ' adım</span><i><u style="width:' + pct + '%"></u></i></div>' : '') +
        '<div class="tm-badges">' + badges + '</div>' + (certs ? '<div class="tm-certs">' + certs + '</div>' : '') + '</div></div>';
    }).join('');
  }
  function tmOpen() {
    tmModal.className = 'vid-lightbox open';
    fetch('PathApi.ashx?op=team', { credentials: 'same-origin', cache: 'no-store' }).then(function (r) { return r.json(); }).then(function (d) {
      if (!d || !d.ok) { $('tmList').innerHTML = '<div class="tm-empty">Liste yüklenemedi.</div>'; return; }
      tmData = d.items.slice().sort(function (a, b) { var pa = a.share ? (a.total ? a.done / a.total : 0) + a.certs.length * .01 : -1, pb = b.share ? (b.total ? b.done / b.total : 0) + b.certs.length * .01 : -1; return pb - pa || a.name.localeCompare(b.name, 'tr'); });
      var fin = tmData.filter(function (x) { return x.share && x.total && x.done === x.total; }).length, certN = tmData.reduce(function (s, x) { return s + (x.share ? x.certs.length : 0); }, 0);
      $('tmSum').textContent = tmData.length + ' takım arkadaşı · ' + fin + ' kişi yolu tamamladı · toplam ' + certN + ' sertifika';
      tmRender();
    }).catch(function () { $('tmList').innerHTML = '<div class="tm-empty">Bağlantı hatası.</div>'; });
  }
  // sol menu karti: yuzler + ozet + ekipteki son basari
  function tmSide() {
    var S = window.TEAM_SUM;
    if (!S || !S.count) return false;
    var shown = (S.faces || []).slice(0, 3);
    var f = shown.map(function (x, i) { return '<span class="tm-face" style="z-index:' + (10 - i) + '">' + (x.a ? '<img src="' + esc(x.a) + '" alt="" />' : esc(initials(x.n))) + '</span>'; }).join('');
    if (S.count > shown.length) f += '<span class="tm-face more">+' + (S.count - shown.length) + '</span>';
    $('tmFaces').innerHTML = f;
    $('tmSideSum').innerHTML = '<b>' + S.count + ' kişi</b><small>' + (S.finished ? S.finished + ' tamamladı' : 'Ekibinizi görün') + '</small>';
    var n = $('tmSideNews');
    if (S.recent) n.innerHTML = '<i class="tm-live"></i><span>' + (S.recent.path ? '🏆 ' : '🎉 ') + '<b>' + esc(S.recent.who) + '</b> ' + esc(S.recent.ago) + ' ' +
      (S.recent.path ? 'yolu tamamladı' : '“' + esc(S.recent.title) + '” rozetini kazandı') + '</span>';
    else n.innerHTML = '<span>Ekibinizin rozetlerini ve sertifikalarını görün</span>';
    return true;
  }
  if (tmBtn && tmModal) {
    if (!tmSide()) { tmBtn = null; } else if ($('acSegTeam')) $('acSegTeam').style.display = '';
  }
  if (tmBtn && tmModal) {
    tmBtn.style.display = '';
    tmBtn.addEventListener('click', tmOpen);
    $('teamClose').addEventListener('click', function () { tmModal.className = 'vid-lightbox'; });
    tmModal.addEventListener('click', function (e) { if (e.target === tmModal) tmModal.className = 'vid-lightbox'; });
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') tmModal.className = 'vid-lightbox'; });
    var tq = $('tmQ'); ['focus', 'mousedown', 'touchstart', 'keydown'].forEach(function (ev) { tq.addEventListener(ev, function () { tq.removeAttribute('readonly'); }); });
    tq.addEventListener('input', tmRender);
  }

  // ---------------- basari paylasimi (sol menu) ----------------
  var shBtn = $('lpShare');
  function shPaint(on) { if (!shBtn) return; shBtn.setAttribute('aria-checked', on ? 'true' : 'false'); shBtn.querySelector('.um-sw').className = 'um-sw' + (on ? ' on' : ''); }
  if (shBtn) {
    shBtn.style.display = '';
    shPaint(P.share !== false);
    shBtn.addEventListener('click', function () {
      var on = shBtn.getAttribute('aria-checked') !== 'true'; shPaint(on);
      var fd = new FormData(); fd.append('on', on ? '1' : '0');
      fetch('PathApi.ashx?op=share', { method: 'POST', body: fd, credentials: 'same-origin' }).then(function (r) { return r.json(); })
        .then(function (d) { if (d && d.ok) { shPaint(d.on); P.share = d.on; } else shPaint(!on); }).catch(function () { shPaint(!on); });
    });
  }

  render(); initEditor();
  setTimeout(celebrate, 600);                                  // uzaktayken kazanilmis rozetler
  if (/[?&]path=1\b/.test(location.search)) setTimeout(openModal, 300);   // bildirim baglantisi
})();
