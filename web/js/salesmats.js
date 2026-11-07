// Satis Materyalleri: arama, goruntuleme kaydi, "Bana gonder" (belge giris yapan kisinin e-postasina; BCC sunucuda).
(function () {
  var $ = function (id) { return document.getElementById(id); };
  var q = $('smQ'), none = $('smNone'), toast = $('smToast'), cnt = $('smCount');
  var fold = function (s) { return String(s || '').trim().toLocaleLowerCase('tr-TR').replace(/ı/g, 'i'); };
  var cards = [].slice.call(document.querySelectorAll('.sm-card')), groups = [].slice.call(document.querySelectorAll('.sm-group'));

  // arama: her kelime baslik / ozet / kategori / dosya adinda gecmeli
  function filter() {
    var words = fold(q ? q.value : '').split(/\s+/).filter(Boolean), n = 0;
    cards.forEach(function (c) {
      var d = c.getAttribute('data-q') || '', ok = words.every(function (w) { return d.indexOf(w) >= 0; });
      c.hidden = !ok; if (ok) n++;
    });
    groups.forEach(function (g) { g.hidden = !g.querySelector('.sm-card:not([hidden])'); });
    if (cnt) cnt.textContent = n;
    if (none) none.hidden = n > 0;
  }
  if (q) {
    // tarayicinin kayitli e-postayi doldurmasina karsi: kutu, kullanici dokunana kadar salt okunur (Akademi aramasiyla ayni onlem)
    var touched = false, free = function () { touched = true; q.removeAttribute('readonly'); };
    q.addEventListener('focus', free); q.addEventListener('mousedown', free); q.addEventListener('touchstart', free);
    // ek guvence: yine de doldurulduysa ve kullanici dokunmadiysa ilk saniyelerde temizlenir
    [0, 400, 1200, 2500].forEach(function (ms) { setTimeout(function () { if (!touched && q.value) { q.value = ''; filter(); } }, ms); });
    q.addEventListener('input', filter);
    q.addEventListener('keydown', function (e) { if (e.key === 'Escape' && q.value) { q.value = ''; filter(); } });
  }

  function post(op, data) {
    var fd = new URLSearchParams(); fd.append('op', op);
    for (var k in data) fd.append(k, data[k]);
    return fetch('SalesMaterialAction.ashx', { method: 'POST', credentials: 'same-origin', keepalive: true,
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: fd.toString() }).then(function (r) { return r.json(); });
  }
  var tt = null;
  function show(msg, err) {
    if (!toast) return;
    toast.textContent = msg; toast.className = 'sm-toast' + (err ? ' err' : ''); toast.hidden = false;
    clearTimeout(tt); tt = setTimeout(function () { toast.hidden = true; }, 6000);
  }
  document.addEventListener('click', function (e) {
    var lg = e.target.closest && e.target.closest('.sm-log');
    if (lg) post('view', { id: lg.getAttribute('data-id') }).catch(function () { });   // rağbet kaydi: goruntuleme / indirme
    var sd = e.target.closest && e.target.closest('.sm-send');
    if (!sd) return;
    e.preventDefault();
    if (sd.disabled) return;
    var label = sd.textContent; sd.disabled = true; sd.textContent = 'Gönderiliyor…';
    post('send', { id: sd.getAttribute('data-id') }).then(function (d) {
      if (d.ok) { show(d.data && d.data.message ? d.data.message : 'Doküman e-postanıza gönderildi.'); sd.textContent = 'Gönderildi ✓'; setTimeout(function () { sd.textContent = label; sd.disabled = false; }, 4000); }
      else { show(d.error || 'Gönderilemedi.', true); sd.textContent = label; sd.disabled = false; }
    }).catch(function () { show('Bağlantı hatası, tekrar deneyin.', true); sd.textContent = label; sd.disabled = false; });
  });
})();
