// Destek talebi ekleri: uygulama icinde onizleme (resim / PDF / metin). Onizlenemeyenler dogrudan indirilir.
// Dosyalar yetki kontrollu TicketFile.ashx uzerinden gelir; metin icerigi DOM'a textContent ile basilir (betik calismaz).
(function () {
  var links = [].slice.call(document.querySelectorAll('a.tk-att[data-att]'));
  if (!links.length) return;
  var MAX_TEXT = 512 * 1024;
  var seen = {}, items = [];
  links.forEach(function (a) {                      // ayni ek ust listede ve mesaj altinda olabilir: tek kayit
    var id = a.getAttribute('data-att');
    if (a.getAttribute('data-kind') && !seen[id]) { seen[id] = 1; items.push({ id: id, kind: a.getAttribute('data-kind'), name: a.getAttribute('data-name'), size: a.getAttribute('data-size') }); }
  });

  var ov = null, cur = -1, lastFocus = null;
  function el(tag, cls, html) { var e = document.createElement(tag); if (cls) e.className = cls; if (html != null) e.innerHTML = html; return e; }
  var ICON = {
    prev: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 6l-6 6 6 6"/></svg>',
    next: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 6l6 6-6 6"/></svg>',
    open: '<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 4h6v6M20 4l-9 9M18 14v5a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V7a1 1 0 0 1 1-1h5"/></svg>',
    down: '<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 4v11M7 10l5 5 5-5M5 20h14"/></svg>'
  };

  function build() {
    ov = el('div', 'tv-ov'); ov.setAttribute('role', 'dialog'); ov.setAttribute('aria-modal', 'true'); ov.setAttribute('aria-label', 'Ek önizleme');
    ov.innerHTML =
      '<div class="tv-bar"><div class="tv-tt"><b class="tv-name"></b><small class="tv-meta"></small></div>' +
      '<div class="tv-act"><a class="tv-btn tv-open" target="_blank" rel="noopener" title="Yeni sekmede aç" aria-label="Yeni sekmede aç">' + ICON.open + '</a>' +
      '<a class="tv-btn tv-down" title="İndir" aria-label="İndir">' + ICON.down + '<span>İndir</span></a>' +
      '<button type="button" class="tv-btn tv-x" title="Kapat (Esc)" aria-label="Kapat">&times;</button></div></div>' +
      '<div class="tv-stage"><button type="button" class="tv-nav prev" aria-label="Önceki ek">' + ICON.prev + '</button><div class="tv-body"></div>' +
      '<button type="button" class="tv-nav next" aria-label="Sonraki ek">' + ICON.next + '</button></div>';
    document.body.appendChild(ov);
    ov.querySelector('.tv-x').addEventListener('click', close);
    ov.querySelector('.tv-nav.prev').addEventListener('click', function () { go(-1); });
    ov.querySelector('.tv-nav.next').addEventListener('click', function () { go(1); });
    // Bosluga tiklayinca kapat: yalniz tiklama BOSLUKTA baslayip boslukta bitiyorsa
    // (metin secerken disari surukleme kapatmaz; resme tik = yakinlastirma; kaydirma cubugu kapatmaz)
    var downOnBack = false;
    function isBack(e) {
      var t = e.target;
      if (!(t === ov || t.classList.contains('tv-stage') || t.classList.contains('tv-body'))) return false;
      if (t.classList.contains('tv-body') && (e.offsetX > t.clientWidth || e.offsetY > t.clientHeight)) return false;   // kaydirma cubugu
      return true;
    }
    ov.addEventListener('pointerdown', function (e) { downOnBack = e.button === 0 && isBack(e); });
    ov.addEventListener('click', function (e) { var ok = downOnBack && isBack(e); downOnBack = false; if (ok) close(); });
  }

  function show(i) {
    if (!ov) build();
    cur = (i + items.length) % items.length;
    var it = items[cur], url = 'TicketFile.ashx?id=' + encodeURIComponent(it.id), body = ov.querySelector('.tv-body');
    ov.querySelector('.tv-name').textContent = it.name;
    ov.querySelector('.tv-meta').textContent = it.size + (items.length > 1 ? ' · ' + (cur + 1) + ' / ' + items.length : '');
    ov.querySelector('.tv-open').href = url + '&inline=1';
    ov.querySelector('.tv-down').href = url;
    ov.classList.toggle('multi', items.length > 1);
    body.className = 'tv-body ' + it.kind; body.innerHTML = '';
    if (it.kind === 'image') {
      var img = el('img'); img.alt = it.name; img.src = url + '&inline=1';
      img.addEventListener('click', function () { body.classList.toggle('zoom'); });
      img.addEventListener('error', function () { fail(body, url); });
      body.appendChild(img);
    } else if (it.kind === 'pdf') {
      var fr = el('iframe'); fr.title = it.name; fr.src = url + '&inline=1';
      body.appendChild(fr);
    } else if (it.kind === 'text') {
      var pre = el('pre', 'tv-pre'); pre.textContent = 'Yükleniyor…'; body.appendChild(pre);
      fetch(url + '&inline=1', { credentials: 'same-origin' }).then(function (r) { if (!r.ok) throw 0; return r.text(); }).then(function (s) {
        if (items[cur] !== it) return;
        var cut = s.length > MAX_TEXT; pre.textContent = cut ? s.slice(0, MAX_TEXT) : s;
        if (cut) body.appendChild(el('div', 'tv-note', 'Dosyanın ilk 512 KB’ı gösteriliyor. Tamamı için indirin.'));
      }).catch(function () { if (items[cur] === it) fail(body, url); });
    }
    if (ov.className.indexOf(' open') < 0) { lastFocus = document.activeElement; ov.className += ' open'; document.body.classList.add('tv-lock'); ov.querySelector('.tv-x').focus(); }
  }
  function fail(body, url) { body.className = 'tv-body'; body.innerHTML = '<div class="tv-fail"><b>Önizleme açılamadı</b><span>Dosyayı indirerek görüntüleyebilirsiniz.</span><a class="btn-cyan" href="' + url + '">İndir</a></div>'; }
  function go(d) { if (items.length > 1) show(cur + d); }
  function close() {
    if (!ov) return;
    ov.className = 'tv-ov'; document.body.classList.remove('tv-lock'); ov.querySelector('.tv-body').innerHTML = '';   // PDF/resim bellekten cikar
    if (lastFocus && lastFocus.focus) lastFocus.focus();
  }

  document.addEventListener('click', function (e) {
    var a = e.target.closest ? e.target.closest('a.tk-att[data-att]') : null;
    if (!a || e.ctrlKey || e.metaKey || e.shiftKey || e.button !== 0) return;   // Ctrl/Cmd tik: dogrudan indir
    var kind = a.getAttribute('data-kind'); if (!kind) return;                    // onizlenemeyen: varsayilan (indir)
    e.preventDefault();
    for (var i = 0; i < items.length; i++) if (items[i].id === a.getAttribute('data-att')) { show(i); return; }
  });
  document.addEventListener('keydown', function (e) {
    if (!ov || ov.className.indexOf(' open') < 0) return;
    if (e.key === 'Escape') { e.preventDefault(); close(); }
    else if (e.key === 'ArrowLeft') go(-1);
    else if (e.key === 'ArrowRight') go(1);
  });
})();
