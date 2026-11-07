// Destek talebi formu: River AI icin talep gonderilmeden once bilgi tabanindan AI on yaniti.
// Kural: kullaniciyi asla engelleme (her zaman "Beklemeden gonder" / "Cozulmedi, gonder"; hata ya da zaman asiminda dogrudan gonder).
(function () {
  var form = document.getElementById('tkForm'); if (!form) return;
  var done = false, answerId = 0, panel = null;
  var hid = document.createElement('input'); hid.type = 'hidden'; hid.name = 'aiAnswerId'; hid.value = ''; form.appendChild(hid);
  function esc(s) { return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; }); }
  function val(n) { var el = form.elements[n]; return el ? (el.value || '') : ''; }
  function send() { done = true; close(); if (form.requestSubmit) form.requestSubmit(); else form.submit(); }
  var stTimer = null, slowTimer = null;
  function show(t) { var el = document.getElementById('kbStage'); if (!el) return false; el.classList.remove('in'); void el.offsetWidth; el.textContent = t; el.classList.add('in'); return true; }
  // Mesajlar 2,4 sn arayla akar, sonuncusu ekranda kalir; uzun surerse "slow" mesaji bir kez gosterilir
  function stage(list, slow) {
    var i = 0; clearInterval(stTimer); clearTimeout(slowTimer);
    stTimer = setInterval(function () { if (!show(list[i]) || ++i >= list.length) clearInterval(stTimer); }, 2400);
    if (slow) slowTimer = setTimeout(function () { clearInterval(stTimer); show(slow); }, 10000);
  }
  function close() { clearInterval(stTimer); clearTimeout(slowTimer); if (panel) { panel.remove(); panel = null; } document.documentElement.classList.remove('kb-open'); }
  function open(html) {
    close(); panel = document.createElement('div'); panel.className = 'kb-overlay'; panel.setAttribute('role', 'dialog'); panel.setAttribute('aria-modal', 'true');
    panel.innerHTML = '<div class="kb-panel">' + html + '</div>'; document.body.appendChild(panel); document.documentElement.classList.add('kb-open');
    panel.addEventListener('click', onClick); var f = panel.querySelector('[data-kb]'); if (f) f.focus();
  }
  function link(l, ts) { return l ? ' <a class="kb-ts" href="' + esc(l) + '" target="_blank" rel="noopener" title="Eğitim videosunu bu saniyeden aç">▶ ' + esc(ts) + '</a>' : ''; }
  function head(t, sub) { return '<div class="kb-head"><div class="kb-badge">✦ River AI destek asistanı</div><h2>' + esc(t) + '</h2>' + (sub ? '<p>' + esc(sub) + '</p>' : '') + '</div>'; }
  function foot() {
    return '<div class="kb-acts"><button type="button" class="kb-btn ok" data-kb="solved">✓ Sorunum çözüldü</button>' +
      '<button type="button" class="kb-btn" data-kb="send">Çözülmedi, talebi gönder</button>' +
      '<button type="button" class="kb-link" data-kb="edit">Talebi düzenle</button></div>' +
      '<p class="kb-note">Bu öneri River Akademi eğitimlerinden yapay zekâ ile hazırlandı. Talebinizi gönderirseniz destek ekibimiz bu öneriyi görerek ilerler.</p>';
  }
  function render(d) {
    var h = '';
    if (d.mode === 'ai') {
      h += head('Talebinizi göndermeden önce şunu deneyin', d.intro || '');
      if (d.steps && d.steps.length) { h += '<ol class="kb-steps">'; d.steps.forEach(function (s) { h += '<li>' + esc(s.text) + link(s.link, s.ts) + '</li>'; }); h += '</ol>'; }
      if (d.followup) h += '<div class="kb-follow"><b>İşe yaramadıysa:</b> ' + esc(d.followup) + ' Bu bilgiyi talebinize eklemeniz çözümü hızlandırır.</div>';
      if (d.refs && d.refs.length) { h += '<div class="kb-refs"><small>Kaynak eğitimler</small>'; d.refs.forEach(function (r) { h += '<a href="' + esc(r.link || '#') + '" target="_blank" rel="noopener"><b>' + esc(r.title) + '</b><span>' + esc(r.video) + ' · ' + esc(r.ts) + '</span></a>'; }); h += '</div>'; }
    } else {
      h += head('Bu konular sorununuzla ilgili olabilir', 'Talebinizi göndermeden önce ilgili eğitim adımlarına göz atın.');
      (d.refs || []).forEach(function (r) {
        h += '<details class="kb-card"' + (h.indexOf('kb-card') < 0 ? ' open' : '') + '><summary><b>' + esc(r.title) + '</b><span>' + esc(r.area) + '</span></summary><p>' + esc(r.summary) + '</p>';
        if (r.steps && r.steps.length) { h += '<ol class="kb-steps">'; r.steps.forEach(function (s) { h += '<li>' + esc(s.text) + link(s.link, s.ts) + '</li>'; }); h += '</ol>'; }
        (r.warnings || []).forEach(function (w) { h += '<div class="kb-warn">⚠️ ' + esc(w) + '</div>'; });
        h += '</details>';
      });
    }
    open(h + foot());
  }
  function onClick(e) {
    var b = e.target.closest('[data-kb]'); if (!b) { if (e.target === panel) close(); return; }
    var k = b.getAttribute('data-kb');
    if (k === 'send') { hid.value = answerId || ''; send(); }
    else if (k === 'skip') { if (ctrl) ctrl.abort(); hid.value = ''; send(); }
    else if (k === 'edit') { close(); var d = form.elements['description']; if (d) d.focus(); }
    else if (k === 'solved') {
      var fd = new FormData(); fd.append('op', 'solved'); fd.append('id', answerId);
      fetch('KbAction.ashx', { method: 'POST', body: fd, credentials: 'same-origin' }).catch(function () { });
      open(head('Sevindik! Sorununuz çözüldü.', 'Talep oluşturulmadı. Benzer bir durumda River Akademi eğitimlerine her zaman göz atabilirsiniz.') +
        '<div class="kb-acts"><a class="kb-btn ok" href="Tickets.aspx">Taleplerime dön</a><button type="button" class="kb-link" data-kb="close">Kapat</button></div>');
    }
    else if (k === 'close') close();
  }
  var ctrl = null;
  form.addEventListener('submit', function (e) {
    if (done || e.defaultPrevented) return;                       // urun secilmediyse onceki kontrol durdurdu
    if (val('product') !== 'River AI' || !window.fetch || !window.FormData) return;
    e.preventDefault();
    open('<div class="kb-conn" role="status" aria-live="polite">' +
      '<div class="kb-orb" aria-hidden="true"><span class="kb-ring"></span><span class="kb-ring r2"></span><img src="assets/RIVER_AI_badge.svg" alt="" /></div>' +
      '<div class="kb-conn-t">River AI\'ya bağlanılıyor<span class="kb-dots" aria-hidden="true"><i>.</i><i>.</i><i>.</i></span></div>' +
      '<div class="kb-conn-s" id="kbStage">Konuyla ilgili beklemeden size yardım edilebilecek hazır bilgiler olabilir. River AI ile kontrol ediyorum.</div>' +
      '<button type="button" class="kb-link" data-kb="skip">Beklemeden gönder</button></div>');
    // Sicak akis: A1 (acilista) -> B1 -> C1 -> D1; 10 sn'yi gecerse bir kez E1
    stage(['River Akademi eğitimlerinde benzer durumları arıyorum.', 'Anlattığınız durumla eşleşen adımları bir araya getiriyorum.', 'Size adım adım bir çözüm hazırlıyorum.'],
      'Biraz daha zaman alıyor; isterseniz beklemeden talebinizi gönderebilirsiniz.');
    var fd = new FormData(); fd.append('op', 'suggest');
    ['product', 'requesttype', 'priority', 'subject', 'description'].forEach(function (n) { fd.append(n, val(n)); });
    ctrl = window.AbortController ? new AbortController() : null;
    var timer = setTimeout(function () { if (ctrl) ctrl.abort(); }, 25000);
    fetch('KbAction.ashx', { method: 'POST', body: fd, credentials: 'same-origin', signal: ctrl ? ctrl.signal : undefined })
      .then(function (r) { return r.json(); })
      .then(function (j) {
        clearTimeout(timer); if (!panel || done) return;
        var d = j && j.ok ? j.data : null;
        if (!d || d.mode === 'skip') { hid.value = ''; send(); return; }
        answerId = d.answerId || 0;
        if (d.mode === 'none' || !(d.refs && d.refs.length)) { hid.value = answerId || ''; send(); return; }   // yanit yok: dogrudan ekibe (eksik icerik olarak kaydedildi)
        render(d);
      })
      .catch(function () { clearTimeout(timer); if (!done) { hid.value = ''; send(); } });
  });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && panel) close(); });
})();
