// Bilgi tabani yonetimi: pano, kayitlar, ice aktarma (KbAction.ashx)
(function () {
  var root = document.getElementById('kb'); if (!root) return;
  var $ = function (id) { return document.getElementById(id); }, ENT = null, DAYS = 30;
  function esc(s) { return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; }); }
  function api(op, data) { var fd = data instanceof FormData ? data : new FormData(); fd.append('op', op); if (data && !(data instanceof FormData)) Object.keys(data).forEach(function (k) { fd.append(k, data[k]); }); return fetch('KbAction.ashx', { method: 'POST', body: fd, credentials: 'same-origin' }).then(function (r) { return r.json(); }); }
  function tab(t) { root.querySelectorAll('.lv-tabs button').forEach(function (b) { b.setAttribute('aria-selected', b.getAttribute('data-tab') === t ? 'true' : 'false'); }); root.querySelectorAll('.lv-view').forEach(function (v) { v.hidden = v.getAttribute('data-view') !== t; }); if (t === 'entries' && !ENT) loadEntries(); if (t === 'stats') loadStats(); }
  root.querySelector('.lv-tabs').addEventListener('click', function (e) { var b = e.target.closest('button[data-tab]'); if (b) tab(b.getAttribute('data-tab')); });
  function pct(a, b) { return b ? '%' + Math.round(a * 100 / b) : '—'; }
  function loadStats() {
    var el = $('kbStats'); el.innerHTML = '<p class="kb-muted">Yükleniyor…</p>';
    api('stats', { days: DAYS }).then(function (j) {
      if (!j.ok) { el.innerHTML = '<div class="lv-alert warn">' + esc(j.error) + '</div>'; return; }
      var d = j.data, shown = d.total - d.none;
      var h = '<div class="kb-range">Son <select id="kbDays">' + [7, 30, 90, 365].map(function (n) { return '<option value="' + n + '"' + (n === DAYS ? ' selected' : '') + '>' + n + ' gün</option>'; }).join('') + '</select></div>' +
        '<div class="kb-kpis">' +
        '<div class="kb-kpi"><small>Ön yanıt aranan talep</small><b>' + d.total + '</b><span>River AI seçilen talep formları</span></div>' +
        '<div class="kb-kpi ok"><small>Talep açılmadan çözüldü</small><b>' + d.solved + '</b><span>Çözülme oranı ' + pct(d.solved, shown) + '</span></div>' +
        '<div class="kb-kpi"><small>Yanıta rağmen talebe dönüştü</small><b>' + d.escalated + '</b><span>AI notuyla ekibe geldi</span></div>' +
        '<div class="kb-kpi warn"><small>Bilgi tabanında karşılığı yok</small><b>' + d.none + '</b><span>Eksik içerik adayları</span></div></div>';
      h += '<h3 class="kb-h3">Yanıt bulunamayan ya da talebe dönüşen sorular</h3><p class="kb-muted">Bu sorular, bilgi tabanında ya da River Akademi\'de eksik olan konuları gösterir. Sık tekrar edenler için yeni eğitim içeriği hazırlamayı değerlendirin.</p>';
      if (!d.gaps.length) h += '<div class="lv-alert info">Bu dönemde kayıt yok.</div>';
      else h += '<div class="kb-gaps">' + d.gaps.map(function (g) {
        return '<div class="kb-gap"><div class="kb-gap-h"><span class="lv-pill ' + (g.mode === 'none' ? 'warn' : 'info') + '">' + (g.mode === 'none' ? 'karşılığı yok' : 'talebe dönüştü') + '</span>' +
          '<span>' + esc(g.at) + (g.company ? ' · ' + esc(g.company) : '') + '</span>' + (g.ticketId ? '<a href="TicketDetail.aspx?id=' + g.ticketId + '">' + esc(g.ticketNo || ('#' + g.ticketId)) + '</a>' : '') + '</div><p>' + esc(g.question) + '</p></div>';
      }).join('') + '</div>';
      el.innerHTML = h;
      $('kbDays').onchange = function () { DAYS = parseInt(this.value, 10); loadStats(); };
    }).catch(function () { el.innerHTML = '<div class="lv-alert warn">Bağlantı hatası.</div>'; });
  }
  function loadEntries() {
    var el = $('kbEntries'); el.innerHTML = '<p class="kb-muted">Yükleniyor…</p>';
    api('list').then(function (j) {
      if (!j.ok) { el.innerHTML = '<div class="lv-alert warn">' + esc(j.error) + '</div>'; return; }
      ENT = j.data; var c = $('kbCount'); c.textContent = ENT.length; c.hidden = false;
      var areas = []; ENT.forEach(function (e) { if (areas.indexOf(e.area) < 0) areas.push(e.area); });
      el.innerHTML = '<div class="kb-filter"><input type="search" id="kbQ" placeholder="Başlık, anahtar kelime ya da kod ara" autocomplete="off" /><select id="kbArea"><option value="">Bütün alanlar</option>' +
        areas.map(function (a) { return '<option>' + esc(a) + '</option>'; }).join('') + '</select><select id="kbAct"><option value="">Hepsi</option><option value="1">Açık</option><option value="0">Kapalı</option></select></div><div id="kbList"></div>';
      ['kbQ', 'kbArea', 'kbAct'].forEach(function (id) { $(id).addEventListener('input', renderList); });
      renderList();
      if (!ENT.length) $('kbList').innerHTML = '<div class="lv-alert info">Henüz kayıt yok. İçe aktar sekmesinden bilgi tabanı dosyasını yükleyin.</div>';
    });
  }
  function fold(s) { return String(s || '').toLocaleLowerCase('tr').replace(/[çğıöşü]/g, function (c) { return { 'ç': 'c', 'ğ': 'g', 'ı': 'i', 'ö': 'o', 'ş': 's', 'ü': 'u' }[c]; }); }
  function renderList() {
    var q = fold($('kbQ').value.trim()), a = $('kbArea').value, ac = $('kbAct').value;
    var l = ENT.filter(function (e) { return (!a || e.area === a) && (ac === '' || (ac === '1') === e.active) && (!q || fold(e.title + ' ' + e.keywords + ' ' + e.code + ' ' + e.summary).indexOf(q) >= 0); });
    $('kbList').innerHTML = '<p class="kb-muted">' + l.length + ' kayıt</p>' + l.map(function (e) {
      return '<details class="kb-row' + (e.active ? '' : ' off') + '"><summary><span class="kb-code">' + esc(e.code) + '</span><b>' + esc(e.title) + '</b><span class="kb-meta">' + esc(e.area) + ' · ' + e.hits + ' kez kullanıldı</span>' +
        '<label class="kb-sw" title="Ön yanıtlarda kullanılsın"><input type="checkbox" data-id="' + e.id + '"' + (e.active ? ' checked' : '') + ' /> açık</label></summary>' +
        '<p>' + esc(e.summary) + '</p><ol class="kb-steps">' + e.steps.map(function (s) { return '<li>' + esc(s.text) + (s.link ? ' <a class="kb-ts" href="' + esc(s.link) + '" target="_blank" rel="noopener">▶ ' + esc(s.ts) + '</a>' : '') + '</li>'; }).join('') + '</ol>' +
        e.warnings.map(function (w) { return '<div class="kb-warn">⚠️ ' + esc(w) + '</div>'; }).join('') +
        '<p class="kb-muted">Kaynak: ' + esc(e.video) + ' · ' + esc(e.ts) + ' · Anahtar kelimeler: ' + esc(e.keywords) + '</p></details>';
    }).join('');
  }
  document.addEventListener('change', function (e) {
    var cb = e.target.closest('.kb-sw input'); if (!cb) return;
    var id = cb.getAttribute('data-id'), on = cb.checked;
    api('toggle', { id: id, on: on ? '1' : '0' }).then(function (j) { if (!j.ok) { cb.checked = !on; alert(j.error); return; } ENT.forEach(function (x) { if (String(x.id) === id) x.active = on; }); cb.closest('.kb-row').classList.toggle('off', !on); });
  });
  document.addEventListener('click', function (e) { if (e.target.closest('.kb-sw')) e.stopPropagation(); }, true);
  $('kbImpForm') && $('kbImpForm').addEventListener('submit', function (e) {
    e.preventDefault(); var msg = $('kbImpMsg'), btn = this.querySelector('button'); btn.disabled = true; msg.innerHTML = '<p class="kb-muted">Aktarılıyor…</p>';
    api('import', new FormData(this)).then(function (j) {
      btn.disabled = false;
      msg.innerHTML = j.ok ? '<div class="lv-alert info">Tamamlandı: ' + j.data.added + ' yeni, ' + j.data.updated + ' güncellendi' + (j.data.skipped ? ', ' + j.data.skipped + ' atlandı' : '') + '.</div>' : '<div class="lv-alert warn">' + esc(j.error) + '</div>';
      if (j.ok) ENT = null;
    }).catch(function () { btn.disabled = false; msg.innerHTML = '<div class="lv-alert warn">Bağlantı hatası.</div>'; });
  });
  loadStats();
})();
