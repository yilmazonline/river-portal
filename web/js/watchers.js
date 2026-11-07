// Akademi editor paneli: En Cok Izleyenler. Liste sekme ilk acildiginda yuklenir; satira tiklaninca egitim gecmisi pencerede acilir.
(function () {
  var tab = document.querySelector('.ed-tab[data-t="watchers"]'); if (!tab) return;
  var list = document.getElementById('twList'), sum = document.getElementById('twSum'), qEl = document.getElementById('twQ'), typeEl = document.getElementById('twType');
  var seg = document.querySelector('.tw-seg'), period = 'all', cache = {}, items = [], loaded = false;
  var TYPE = { fol: 'Fol Yazılım', partner: 'İş Ortağı', customer: 'Müşteri' };
  function esc(s) { return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; }); }
  function fold(s) { return String(s || '').toLocaleLowerCase('tr-TR').normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/ı/g, 'i'); }
  function dur(sec) { var m = Math.round(sec / 60), h = Math.floor(m / 60); m = m % 60; return h > 0 ? h + ' sa' + (m ? ' ' + m + ' dk' : '') : (m || (sec > 0 ? 1 : 0)) + ' dk'; }
  function ago(iso) {
    if (!iso) return '-'; var s = (Date.now() - new Date(iso).getTime()) / 1000;
    if (s < 3600) return Math.max(1, Math.floor(s / 60)) + ' dk önce'; if (s < 86400) return Math.floor(s / 3600) + ' sa önce';
    if (s < 172800) return 'dün'; if (s < 2592000) return Math.floor(s / 86400) + ' gün önce';
    return new Date(iso).toLocaleDateString('tr-TR', { day: 'numeric', month: 'short', year: 'numeric' });
  }
  function ini(n) { return String(n || '?').trim().split(/\s+/).slice(0, 2).map(function (w) { return w.charAt(0); }).join('').toLocaleUpperCase('tr-TR'); }
  function render() {
    var q = fold(qEl.value.trim()), ty = typeEl.value;
    var rows = items.filter(function (x) { return (!ty || x.type === ty) && (!q || fold(x.name + ' ' + x.company + ' ' + x.email).indexOf(q) >= 0); });
    var total = 0; rows.forEach(function (x) { total += x.sec; });
    sum.textContent = rows.length ? rows.length + ' kişi · toplam ' + dur(total) + ' izleme' : '';
    if (!rows.length) { list.innerHTML = '<div class="tw-empty"><b>' + (items.length ? 'Aramaya uyan kişi yok' : 'Bu dönemde izleme kaydı yok') + '</b><span>' + (items.length ? 'Süzgeçleri değiştirin.' : 'Başka bir dönem seçin.') + '</span></div>'; return; }
    var max = rows[0].sec || 1;
    list.innerHTML = rows.map(function (x, i) {
      var rank = items.indexOf(x) + 1;   // genel sira (suzgecten bagimsiz)
      var av = x.ava ? '<span class="tw-ava"><img src="' + esc(x.ava) + '" alt="" /></span>' : '<span class="tw-ava">' + esc(ini(x.name)) + '</span>';
      return '<button type="button" class="tw-row" data-id="' + x.id + '" data-name="' + esc(x.name) + '">' +
        '<span class="tw-rank' + (rank <= 3 ? ' r' + rank : '') + '">' + rank + '</span>' + av +
        '<span class="tw-who"><b>' + esc(x.name) + '</b><small>' + esc(x.company || '-') + (x.type ? ' <em class="tw-t ' + x.type + '">' + TYPE[x.type] + '</em>' : '') + '</small></span>' +
        '<span class="tw-time"><b>' + dur(x.sec) + '</b><i><s style="width:' + Math.max(3, Math.round(x.sec / max * 100)) + '%"></s></i></span>' +
        '<span class="tw-meta"><span>' + x.videos + ' video</span><span>' + x.done + ' tamamlandı</span><span>' + x.days + ' gün</span></span>' +
        '<span class="tw-last">' + ago(x.last) + '</span><span class="tw-go" aria-hidden="true">›</span></button>';
    }).join('');
  }
  function load() {
    if (cache[period]) { items = cache[period]; render(); return; }
    list.innerHTML = '<div class="tw-skel"></div><div class="tw-skel"></div><div class="tw-skel"></div>'; sum.textContent = 'Yükleniyor…';
    var p = period;
    fetch('TrainingWatchers.ashx?period=' + p, { credentials: 'same-origin' }).then(function (r) { return r.json(); }).then(function (d) {
      if (p !== period) return;
      if (!d.ok) { sum.textContent = ''; list.innerHTML = '<div class="tw-empty"><b>Liste okunamadı</b><span>' + esc(d.error || '') + '</span></div>'; return; }
      cache[p] = d.items; items = d.items; render();
    }).catch(function () { sum.textContent = ''; list.innerHTML = '<div class="tw-empty"><b>Bağlantı hatası</b><span>Sekmeyi yeniden açın.</span></div>'; });
  }
  tab.addEventListener('click', function () { if (!loaded) { loaded = true; load(); } });
  seg.addEventListener('click', function (e) {
    var b = e.target.closest('button[data-p]'); if (!b || b.getAttribute('data-p') === period) return;
    [].forEach.call(seg.querySelectorAll('button'), function (x) { x.classList.toggle('on', x === b); });
    period = b.getAttribute('data-p'); load();
  });
  qEl.addEventListener('input', render); typeEl.addEventListener('change', render);

  // detay: egitim gecmisi pencerede (UserTraining.aspx, gomulu mod)
  var dlg = document.getElementById('twDetail'), frame = document.getElementById('twFrame'), last = null;
  function open(id, name) {
    last = document.activeElement;
    document.getElementById('twDetailT').textContent = name;
    document.getElementById('twDetailOpen').href = 'UserTraining.aspx?id=' + id;
    frame.src = 'UserTraining.aspx?id=' + id + '&embed=1';
    dlg.hidden = false; document.getElementById('twDetailX').focus();
  }
  function close() { dlg.hidden = true; frame.src = 'about:blank'; if (last && last.focus) last.focus(); }
  list.addEventListener('click', function (e) { var r = e.target.closest('.tw-row'); if (r) open(r.getAttribute('data-id'), r.getAttribute('data-name')); });
  document.getElementById('twDetailX').addEventListener('click', close);
  dlg.addEventListener('click', function (e) { if (e.target === dlg) close(); });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && !dlg.hidden) { e.stopPropagation(); close(); } }, true);
})();
