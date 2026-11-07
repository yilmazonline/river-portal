// Destek talebi gorunen durumu: personel listede ve detayda durum etiketine tiklayip hazir ya da ozel bir durum yazar.
// Sistem durumu degismez; musteriye uygulama ici bildirim sunucuda gider (TicketStatusNote.ashx).
(function () {
  var PRESETS = ['Analiz ediliyor', 'Geliştirme aşamasında', 'Güncelleme planlandı', 'Test ediliyor', 'Çözüm hazırlanıyor', 'Üçüncü taraf bekleniyor'];
  var pop = null, cur = null;
  function esc(s) { var d = document.createElement('div'); d.textContent = s || ''; return d.innerHTML; }
  function close() { if (pop) { pop.remove(); pop = null; } if (cur) { cur.setAttribute('aria-expanded', 'false'); cur = null; } }
  function toast(t, bad) { var e = document.createElement('div'); e.className = 'ec-toast' + (bad ? ' bad' : ''); e.setAttribute('role', 'status'); e.textContent = t; document.body.appendChild(e); requestAnimationFrame(function () { e.classList.add('in'); }); setTimeout(function () { e.classList.remove('in'); setTimeout(function () { e.remove(); }, 300); }, 3200); }
  function save(btn, note) {
    var fd = new FormData(); fd.append('id', btn.getAttribute('data-tid')); fd.append('note', note);
    btn.disabled = true;
    fetch('TicketStatusNote.ashx', { method: 'POST', body: fd, credentials: 'same-origin' }).then(function (r) { return r.json(); }).then(function (d) {
      btn.disabled = false;
      if (!d || !d.ok) { toast((d && d.error) || 'Durum güncellenemedi.', true); return; }
      btn.querySelector('span').textContent = d.label; btn.setAttribute('data-note', d.note || '');
      toast(note ? 'Durum güncellendi: ' + d.label + '. Müşteriye bildirim gönderildi.' : 'Varsayılan duruma dönüldü. Müşteriye bildirim gönderildi.');
    }).catch(function () { btn.disabled = false; toast('Bağlantı hatası, tekrar deneyin.', true); });
  }
  function open(btn) {
    close(); cur = btn; btn.setAttribute('aria-expanded', 'true');
    var note = btn.getAttribute('data-note') || '', def = btn.getAttribute('data-def') || '';
    pop = document.createElement('div'); pop.className = 'stn-pop'; pop.setAttribute('role', 'dialog'); pop.setAttribute('aria-label', 'Görünen durum');
    var h = '<div class="stn-h">Görünen durum</div><div class="stn-list" role="listbox">';
    PRESETS.forEach(function (p) { h += '<button type="button" class="stn-it' + (p === note ? ' on' : '') + '" data-v="' + esc(p) + '" role="option" aria-selected="' + (p === note) + '">' + esc(p) + '</button>'; });
    h += '</div><form class="stn-own" data-noload="1"><input type="text" maxlength="40" placeholder="Özel durum yazın" aria-label="Özel durum" value="' + (PRESETS.indexOf(note) < 0 ? esc(note) : '') + '" /><button type="submit" class="btn-cyan">Kaydet</button></form>';
    if (note) h += '<button type="button" class="stn-reset" data-v="">Varsayılan duruma dön · ' + esc(def) + '</button>';
    h += '<div class="stn-foot">Müşteriye uygulama içi bildirim gider.</div>';
    pop.innerHTML = h; document.body.appendChild(pop);
    var r = btn.getBoundingClientRect(), w = pop.offsetWidth;
    pop.style.top = (r.bottom + window.scrollY + 6) + 'px';
    pop.style.left = Math.max(8, Math.min(window.innerWidth - w - 8, r.left + window.scrollX)) + 'px';
    pop.addEventListener('click', function (e) { var b = e.target.closest('[data-v]'); if (!b) return; var v = b.getAttribute('data-v'); var t = cur; close(); save(t, v); });
    pop.querySelector('form').addEventListener('submit', function (e) { e.preventDefault(); var v = this.querySelector('input').value.trim(); if (!v) { this.querySelector('input').focus(); return; } var t = cur; close(); save(t, v); });
    (pop.querySelector('.stn-it.on') || pop.querySelector('.stn-it')).focus();
  }
  document.addEventListener('click', function (e) {
    var b = e.target.closest ? e.target.closest('.st-edit') : null;
    if (b) { e.preventDefault(); e.stopPropagation(); if (cur === b) close(); else open(b); return; }
    if (pop && !(e.target.closest && e.target.closest('.stn-pop'))) close();
  }, true);
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape') close(); });
  window.addEventListener('resize', close);
})();
