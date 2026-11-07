// Egitim Icerik Yonetimi: arama, suzme ve siralama (tarayicida; sayfa yenilenmez). Durum oturum boyunca korunur.
(function () {
  var tb = document.querySelector('#tmTable tbody'); if (!tb) return;
  var rows = [].slice.call(tb.querySelectorAll('tr[data-i]')); if (!rows.length) return;
  var $ = function (id) { return document.getElementById(id); };
  var q = $('tmQ'), ty = $('tmType'), gr = $('tmGroup'), st = $('tmStatus'), ft = $('tmFeat'), clr = $('tmClear'), cnt = $('tmCount'), empty = $('tmEmpty');
  var KEY = 'tmState', sort = { key: '', dir: 1 };
  function fold(s) { return String(s || '').toLocaleLowerCase('tr').replace(/[çğıöşüâîû]/g, function (c) { return { 'ç': 'c', 'ğ': 'g', 'ı': 'i', 'ö': 'o', 'ş': 's', 'ü': 'u', 'â': 'a', 'î': 'i', 'û': 'u' }[c]; }); }
  rows.forEach(function (r) { r._hay = fold(r.getAttribute('data-hay') + ' ' + r.getAttribute('data-title')); });
  var coll = window.Intl && Intl.Collator ? new Intl.Collator('tr', { numeric: true, sensitivity: 'base' }) : null;
  function cmp(a, b) { return coll ? coll.compare(a, b) : (a < b ? -1 : a > b ? 1 : 0); }
  function val(r, k) { return k === 'title' ? r.getAttribute('data-title') : k === 'type' ? (r.getAttribute('data-type') === 'doc' ? 'Doküman' : 'Video') : k === 'cat' ? r.getAttribute('data-cat') : r.getAttribute('data-' + k); }
  function apply() {
    var terms = fold(q.value).split(/\s+/).filter(Boolean), n = 0;
    rows.forEach(function (r) {
      var ok = terms.every(function (t) { return r._hay.indexOf(t) >= 0; }) &&
        (!ty.value || r.getAttribute('data-type') === ty.value) && (!gr.value || r.getAttribute('data-group') === gr.value) &&
        (!st.value || r.getAttribute('data-active') === st.value) && (!ft.checked || r.getAttribute('data-feat') === '1');
      r.hidden = !ok; if (ok) n++;
    });
    var list = rows.slice();
    if (sort.key) list.sort(function (a, b) {
      var c = (sort.key === 'active' || sort.key === 'feat') ? (+val(b, sort.key)) - (+val(a, sort.key)) : cmp(val(a, sort.key), val(b, sort.key));
      return (c || cmp(a.getAttribute('data-title'), b.getAttribute('data-title'))) * sort.dir;
    });
    else list.sort(function (a, b) { return a.getAttribute('data-i') - b.getAttribute('data-i'); });
    list.forEach(function (r) { tb.appendChild(r); });
    document.querySelectorAll('#tmTable th[data-sort]').forEach(function (th) {
      var on = th.getAttribute('data-sort') === sort.key; th.setAttribute('aria-sort', on ? (sort.dir > 0 ? 'ascending' : 'descending') : 'none'); th.classList.toggle('on', on);
    });
    var filtered = terms.length || ty.value || gr.value || st.value || ft.checked;
    cnt.textContent = filtered ? n + ' / ' + rows.length + ' içerik' : rows.length + ' içerik';
    empty.hidden = n > 0; clr.hidden = !filtered && !sort.key;
    try { sessionStorage.setItem(KEY, JSON.stringify({ q: q.value, ty: ty.value, gr: gr.value, st: st.value, ft: ft.checked, sk: sort.key, sd: sort.dir })); } catch (e) { }
  }
  // onceki durumu geri yukle (duzenleme, yayinlama, silme sonrasi sayfa yenilendiginde)
  try {
    var s = JSON.parse(sessionStorage.getItem(KEY) || 'null');
    if (s) { q.value = s.q || ''; ty.value = s.ty || ''; st.value = s.st || ''; ft.checked = !!s.ft; sort.key = s.sk || ''; sort.dir = s.sd || 1;
      if (s.gr && [].some.call(gr.options, function (o) { return o.value === s.gr; })) gr.value = s.gr; }
  } catch (e) { }
  var t = null;
  q.addEventListener('input', function () { clearTimeout(t); t = setTimeout(apply, 120); });
  [ty, gr, st, ft].forEach(function (el) { el.addEventListener('change', apply); });
  document.querySelectorAll('#tmTable th[data-sort] button').forEach(function (b) {
    b.addEventListener('click', function () { var k = b.parentNode.getAttribute('data-sort'); if (sort.key === k) sort.dir = -sort.dir; else { sort.key = k; sort.dir = 1; } apply(); });
  });
  clr.addEventListener('click', function () { q.value = ''; ty.value = ''; gr.value = ''; st.value = ''; ft.checked = false; sort.key = ''; sort.dir = 1; apply(); q.focus(); });
  apply();
})();
