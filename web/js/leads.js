// Lead Merkezi: aday havuzu, aday ekleme (CSV, Apollo), onay kuyrugu, sablonlar, engelleme, ayarlar. Yalniz super admin.
(function () {
  'use strict';
  var root = document.getElementById('ld'); if (!root) return;
  var API = 'LeadAction.ashx', S = null, tab = 'overview', filt = { category: '', status: '', q: '' }, contacts = null, pg = 1, poolTotal = null, qpg = 1, PS = 50;
  var CAT = { musteri: 'Müşteri', danisman: 'Danışman' };
  var PER = { yonetim: 'Yönetim', bt: 'BT', surec: 'Süreç ve kalite', genel: 'Genel', ortak: 'İş ortağı' };
  var CLS = { interested: ['İlgileniyor', 'ok'], later: ['Daha sonra', 'warn'], notinterested: ['İlgilenmiyor', 'mute'], wrongperson: ['Doğru kişi değil', 'info'], ooo: ['Otomatik yanıt', 'mute'],
    bounce: ['Geri döndü', 'bad'], unsubscribe: ['Çıkmak istiyor', 'bad'], other: ['Diğer', 'pend'] };
  var NEED = { interested: 1, later: 1, wrongperson: 1, other: 1 };   // insan ilgisi gerektirenler
  var ST = { 'new': ['Yeni', 'pend'], queued: ['Kuyrukta', 'info'], sequence: ['Dizide', 'info'], replied: ['Yanıtladı', 'ok'], interested: ['İlgileniyor', 'ok'], later: ['Sonra', 'warn'],
    notinterested: ['İlgilenmiyor', 'mute'], bounced: ['Geri döndü', 'bad'], unsubscribed: ['Çıktı', 'mute'], excluded: ['Çıkarıldı', 'mute'], done: ['Tamamlandı', 'mute'] };

  function $(id) { return document.getElementById(id); }
  function esc(s) { return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; }); }
  function n(x) { return (x || 0).toLocaleString('tr-TR'); }
  function api(op, data, file) {
    var o = { credentials: 'same-origin' };
    if (op === 'state' || op === 'contacts') { var qs = new URLSearchParams({ op: op }); if (data) for (var k in data) if (data[k]) qs.append(k, data[k]); return fetch(API + '?' + qs.toString(), o).then(function (r) { return r.json(); }); }
    var b = file ? data : new URLSearchParams(); b.append('op', op); if (!file) for (var k2 in data) if (data[k2] != null) b.append(k2, data[k2]);
    o.method = 'POST'; o.body = b; if (!file) o.headers = { 'Content-Type': 'application/x-www-form-urlencoded' };
    return fetch(API, o).then(function (r) { return r.json(); });
  }
  var tt = null;
  function toast(t) { var e = $('ldToast'); e.textContent = t; e.hidden = false; clearTimeout(tt); tt = setTimeout(function () { e.hidden = true; }, 6000); }
  function load(d, msg) {
    if (!d.ok) { toast(d.error || 'İşlem tamamlanamadı.'); return false; }
    S = d.data; contacts = null; poolTotal = null;
    if (tab === 'pool' && (filt.category || filt.status || filt.q || pg > 1)) { stats(); fetchPool(false); } else { pg = 1; render(); }   // havuzda suzgec ve sayfa korunur
    if (msg) toast(msg); return true;
  }
  function busy(btn, on, txt) { if (!btn) return; if (on) { btn.dataset.t = btn.textContent; btn.textContent = txt || 'Çalışıyor…'; btn.disabled = true; } else { btn.textContent = btn.dataset.t || btn.textContent; btn.disabled = false; } }
  function catSel(id, val, any) { return '<select class="input" id="' + id + '">' + (any ? '<option value="">Tüm kategoriler</option>' : '') + Object.keys(CAT).map(function (k) { return '<option value="' + k + '"' + (k === val ? ' selected' : '') + '>' + CAT[k] + '</option>'; }).join('') + '</select>'; }
  // HTML destegi (sunucudaki Leads.IsHtml / ToHtml ile ayni): etiket varsa HTML, yoksa duz metin paragraflara cevrilir
  function isHtml(s) { return /<\s*\/?\s*[a-z][a-z0-9]*(\s[^>]*)?>/i.test(s || ''); }
  function txt(t) { var e = esc((t || '').replace(/\r\n/g, '\n').trim()); return e ? '<p style="margin:0 0 14px">' + e.replace(/\n{2,}/g, '</p><p style="margin:0 0 14px">').replace(/\n/g, '<br>') + '</p>' : ''; }
  function toHtml(body) {
    var s = S.settings;
    return '<div style="font-family:Segoe UI,Arial,sans-serif;font-size:14.5px;line-height:1.6;color:#1B2440;padding:18px;background:#fff">' + (isHtml(body) ? body : txt(body)) + txt(s.signature) +
      (s.optOut ? '<p style="margin:18px 0 0;font-size:12px;color:#8A90AE">' + esc(s.optOut) + '</p>' : '') + '</div>';
  }
  // canli onizleme: duzenleyicinin yanindaki korumali cerceve (sandbox: betik calismaz), yazdikca guncellenir
  function sample(s) { return s.replace(/\{Hitap\}/g, 'Ayşe Hanım').replace(/\{Ad\}/g, 'Ayşe').replace(/\{Firma\}/g, 'Örnek A.Ş.').replace(/\{Unvan\}/g, 'Bilgi İşlem Müdürü').replace(/\{Giris\}/g, ''); }
  function live(ta) {
    var box = ta.closest('.ld-ed'), f = box && box.querySelector('iframe.ld-pv'); if (!f) return;
    var body = ta.getAttribute('data-sample') ? sample(ta.value) : ta.value;
    f.srcdoc = '<!doctype html><meta charset="utf-8"><body style="margin:0;background:#fff">' + toHtml(body) + '</body>';
    f.onload = function () { try { f.style.height = Math.max(260, Math.min(760, f.contentDocument.body.scrollHeight + 12)) + 'px'; } catch (e) { } };
  }
  function liveAll() { [].forEach.call(root.querySelectorAll('textarea[data-live]'), live); }
  var lt = null;

  function pill(s) { var m = ST[s] || [s, 'mute']; return '<span class="lv-pill ' + m[1] + '">' + m[0] + '</span>'; }

  // ---------- ozet ----------
  function stats() {
    var c = S.counts || {}, g = function (k) { return c[k] || 0; };
    $('ldcQueue').textContent = g('msg.draft'); $('ldcQueue').hidden = !g('msg.draft');
    var open = (S.replies || []).filter(function (r) { return !r.handled && NEED[r.cls]; }).length;
    $('ldcRep').textContent = open; $('ldcRep').hidden = !open;
  }


  // ---------- genel bakis panosu ----------
  var MON = ['Oca', 'Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem', 'Ağu', 'Eyl', 'Eki', 'Kas', 'Ara'];
  function dShort(k) { var a = k.split('-'); return +a[2] + ' ' + MON[+a[1] - 1]; }
  function pct(a, b) { return b ? Math.round(a * 1000 / b) / 10 : 0; }
  function pf(x) { return x.toLocaleString('tr-TR', { maximumFractionDigits: 1 }); }   // Turkce ondalik: %7,1
  function kpi(label, val, sub, tone) { return '<div class="ldx-kpi' + (tone ? ' ' + tone : '') + '"><span>' + label + '</span><b>' + val + '</b><small>' + sub + '</small></div>'; }
  function bars(list, labeler, tone) {
    if (!list || !list.length) return '<div class="ldx-none">Henüz veri yok</div>';
    var max = Math.max.apply(null, list.map(function (x) { return x[1]; })) || 1, tot = list.reduce(function (a, x) { return a + x[1]; }, 0);
    return '<ul class="ldx-bars">' + list.map(function (x) {
      return '<li><span class="l">' + esc(labeler ? labeler(x[0]) : x[0]) + '</span><span class="t"><i class="' + (tone || '') + '" style="width:' + Math.max(2, x[1] * 100 / max) + '%"></i></span><span class="v">' + n(x[1]) + '<small>%' + pf(pct(x[1], tot)) + '</small></span></li>';
    }).join('') + '</ul>';
  }
  function trend(days) {
    var max = Math.max.apply(null, days.map(function (x) { return x[1] + x[2]; })) || 0, mx2 = Math.max.apply(null, days.map(function (x) { return Math.max(x[3], x[4]); })) || 0;
    var top = Math.max(4, max), top2 = Math.max(4, mx2);
    var col = function (x, i) {
      var tip = dShort(x[0]) + ': ' + x[1] + ' müşteri, ' + x[2] + ' danışman adayı; ' + x[3] + ' e-posta, ' + x[4] + ' yanıt';
      return '<div class="c" title="' + esc(tip) + '"><div class="s"><i class="b2" style="height:' + (x[2] * 100 / top) + '%"></i><i class="b1" style="height:' + (x[1] * 100 / top) + '%"></i></div>' +
        '<div class="s2"><i class="b3" style="height:' + (x[3] * 100 / top2) + '%"></i><i class="b4" style="height:' + (x[4] * 100 / top2) + '%"></i></div>' + (i % 5 === 0 || i === days.length - 1 ? '<em>' + dShort(x[0]) + '</em>' : '') + '</div>';
    };
    return '<div class="ldx-leg"><span><i class="b1"></i>Müşteri adayı</span><span><i class="b2"></i>Danışman adayı</span><span><i class="b3"></i>Gönderilen e-posta</span><span><i class="b4"></i>Yanıt</span></div>' +
      '<div class="ldx-trend"><div class="ax"><span>' + top + '</span><span>0</span></div><div class="cols">' + days.map(col).join('') + '</div></div>';
  }
  function funnel(d) {
    var st = [['Aday', d.active], ['Taslak hazırlandı', d.drafted], ['Onaylandı', d.approved], ['Gönderildi', d.sentContacts], ['Yanıtladı', d.replied], ['İlgileniyor', d.interested]], base = st[0][1] || 1;
    return '<ol class="ldx-fun">' + st.map(function (x, i) {
      var conv = i ? pct(x[1], st[i - 1][1]) : 100;
      return '<li><span class="l">' + x[0] + '</span><span class="t"><i style="width:' + Math.max(3, x[1] * 100 / base) + '%"></i></span><span class="v">' + n(x[1]) + (i ? '<small>%' + pf(conv) + '</small>' : '') + '</span></li>';
    }).join('') + '</ol>';
  }
  function overview() {
    var d = S.dash || {}, el = $('ldOv');
    if (!d.total) {
      el.innerHTML = '<div class="ldx-empty"><h2>Lead Merkezi’ne hoş geldiniz</h2><p>Henüz aday yok. İlk adayları CSV ile yükleyebilir ya da Apollo’dan getirebilirsiniz; pano veriler geldikçe dolacak.</p><div class="lv-acts"><button class="btn-cyan" type="button" data-tab="import">Aday ekle</button><button class="btn-ghost" type="button" data-tab="settings">Ayarlar</button></div></div>';
      return;
    }
    var rr = pct(d.replied, d.sentContacts), br = pct(d.bounced, d.sentContacts), en = S.engine || {};
    var h = (S.settings.sendEnabled ? '' : '<div class="lv-alert info ldx-off">Gönderim kapalı. Onaylanan e-postalar kuyrukta bekliyor; ısınma bitince Ayarlar › Gönderim bölümünden açabilirsiniz.</div>') +
      '<div class="ldx-kpis">' +
      kpi('Aktif aday', n(d.active), 'Bu hafta +' + n(d.week)) +
      kpi('Onay bekleyen', n(d.pending), 'taslak', d.pending ? 'hot' : '') +
      kpi('Gönderilen e-posta', n(d.sentMails), n(d.sentContacts) + ' kişiye') +
      kpi('Yanıt oranı', '%' + pf(rr), n(d.replied) + ' yanıt', rr >= 5 ? 'good' : '') +
      kpi('İlgilenen', n(d.interested), d.sentContacts ? '%' + pf(pct(d.interested, d.sentContacts)) + ' dönüşüm' : 'gönderim bekleniyor', d.interested ? 'good' : '') +
      kpi('Apollo bu ay', n(d.apolloMonth), 'eklenen aday') + '</div>';
    if (d.sentContacts >= 20 && br > 3) h += '<div class="lv-alert warn">Geri dönen e-posta oranı %' + pf(br) + '. %3’ün üzerinde teslim edilebilirlik riski var; doğrulama servisi eklemeyi değerlendirelim.</div>';
    h += '<div class="ldx-g2"><section class="ldx-card"><div class="ldx-h"><h3>Son 30 gün</h3><small>Eklenen adaylar ve e-posta trafiği</small></div>' + trend(d.days || []) + '</section>' +
      '<section class="ldx-card"><div class="ldx-h"><h3>Dönüşüm hunisi</h3><small>Bir önceki adıma göre oran</small></div>' + funnel(d) + '</section></div>';
    h += '<div class="ldx-g3"><section class="ldx-card"><div class="ldx-h"><h3>Persona</h3><small>Kategori ve role göre</small></div>' + bars(d.persona, function (k) { var a = k.split('.'); return CAT[a[0]] + ' · ' + (PER[a[1]] || a[1]); }, 'p') + '</section>' +
      '<section class="ldx-card"><div class="ldx-h"><h3>Sektörler</h3><small>İlk 8</small></div>' + bars(d.industry, null, 'c') + '</section>' +
      '<section class="ldx-card"><div class="ldx-h"><h3>Şehirler</h3><small>İlk 8</small></div>' + bars(d.city, null, 'c') + '</section></div>';
    var SZ = ['1-50', '51-200', '201-500', '501-1.000', '1.000+', 'Bilinmiyor'], size = (d.size || []).slice().sort(function (a, b) { return SZ.indexOf(a[0]) - SZ.indexOf(b[0]); });
    h += '<div class="ldx-g3"><section class="ldx-card"><div class="ldx-h"><h3>Firma büyüklüğü</h3><small>Çalışan sayısı</small></div>' + bars(size, null, 'p') + '</section>' +
      '<section class="ldx-card"><div class="ldx-h"><h3>Kaynak</h3><small>Adayların geldiği yer</small></div>' + bars(d.source, function (k) { return { apollo: 'Apollo', instantly: 'Instantly', csv: 'Diğer liste', elle: 'Elle' }[k] || k; }, 'c') + '</section>' +
      '<section class="ldx-card"><div class="ldx-h"><h3>Durum</h3><small>Tüm adaylar</small></div>' + bars(d.status, function (k) { return (ST[k] || [k])[0]; }, 'p') + '</section></div>';
    el.innerHTML = h;
  }

  // ---------- sayfalama (havuz ve kuyruk ortak) ----------
  function pager(kind, page, total) {
    var pages = Math.max(1, Math.ceil(total / PS)); if (total <= PS) return '';
    var a = Math.max(1, Math.min(page - 2, pages - 4)), b = Math.min(pages, a + 4), h = [];
    var btn = function (p, t, dis, cur, lbl) { return '<button type="button" class="ld-pg' + (cur ? ' on' : '') + '" data-pg="' + p + '" data-kind="' + kind + '"' + (dis ? ' disabled' : '') + (cur ? ' aria-current="page"' : '') + ' aria-label="' + (lbl || ('Sayfa ' + p)) + '">' + t + '</button>'; };
    h.push(btn(page - 1, '‹', page <= 1, false, 'Önceki sayfa'));
    if (a > 1) { h.push(btn(1, '1')); if (a > 2) h.push('<span class="ld-pgd">…</span>'); }
    for (var p = a; p <= b; p++) h.push(btn(p, String(p), false, p === page));
    if (b < pages) { if (b < pages - 1) h.push('<span class="ld-pgd">…</span>'); h.push(btn(pages, String(pages))); }
    h.push(btn(page + 1, '›', page >= pages, false, 'Sonraki sayfa'));
    return '<nav class="ld-pager" aria-label="Sayfalar"><span class="ld-pgi">' + n((page - 1) * PS + 1) + '–' + n(Math.min(page * PS, total)) + ' / ' + n(total) + ' kayıt</span><div>' + h.join('') + '</div></nav>';
  }
  function fetchPool(keepFocus) {
    var q = { category: filt.category, status: filt.status, q: filt.q, page: pg };
    return api('contacts', q).then(function (d) {
      if (!d.ok) { toast(d.error || 'Liste alınamadı.'); return; }
      contacts = d.data.contacts; poolTotal = d.data.total; pg = d.data.page;
      var f = keepFocus && document.activeElement && document.activeElement.id; pool();
      if (f && $(f)) { $(f).focus(); if (f === 'ldfQ') { var v = $(f).value; $(f).value = ''; $(f).value = v; } }
    });
  }

  // ---------- havuz ----------
  function pool() {
    var h = '<div class="ld-filters">' + catSel('ldfCat', filt.category, true) +
      '<select class="input" id="ldfSt"><option value="">Tüm durumlar</option>' + Object.keys(ST).map(function (k) { return '<option value="' + k + '"' + (k === filt.status ? ' selected' : '') + '>' + ST[k][0] + '</option>'; }).join('') + '</select>' +
      '<input class="input" type="search" id="ldfQ" placeholder="Kişi, unvan ya da firma ara" value="' + esc(filt.q) + '" aria-label="Havuzda ara"></div>';
    var l = contacts || S.contacts, total = poolTotal != null ? poolTotal : (S.poolTotal || l.length);
    h += l.length ? pager('pool', pg, total) + '<div class="lv-tblw"><table class="lv-tbl ld-tbl"><thead><tr><th>Kişi</th><th>Firma</th><th>E-posta</th><th>Durum</th><th></th></tr></thead><tbody>' + l.map(function (x) {
      return '<tr><td><b>' + esc(x.name) + '</b><small>' + esc(x.title || '·') + '</small><small>' + CAT[x.category] + ' · ' + (PER[x.persona] || '') + '</small></td>' +
        '<td>' + esc(x.company || '·') + '<small>' + esc([x.industry, x.city, x.employees ? n(x.employees) + ' çalışan' : ''].filter(Boolean).join(', ')) + '</small></td>' +
        '<td>' + esc(x.email) + '<small>' + esc(x.emailStatus || 'durum yok') + ' · ' + esc(x.source) + ' · ' + esc(x.at) + '</small></td>' +
        '<td>' + pill(x.status) + (x.note ? '<small>' + esc(x.note) + '</small>' : '') + '</td>' +
        '<td class="ld-acts">' + (x.status === 'new' || x.status === 'queued' ? '<button class="btn-ghost lv-sm" data-do="exclude" data-id="' + x.id + '">Çıkar</button><button class="btn-ghost lv-sm lv-bad" data-do="block" data-id="' + x.id + '">Engelle</button>' : '') +
        (x.linkedin ? '<a class="btn-ghost lv-sm" href="' + esc(x.linkedin) + '" target="_blank" rel="noopener">LinkedIn</a>' : '') + '</td></tr>';
    }).join('') + '</tbody></table></div>' + pager('pool', pg, total)
      : '<div class="lv-empty">Bu süzgeçlere uyan aday yok. “Aday ekle” sekmesinden CSV yükleyebilir ya da Apollo’dan aday getirebilirsiniz.</div>';
    $('ldPool').innerHTML = h;
  }
  var qt = null;
  function refilter() {
    filt.category = $('ldfCat').value; filt.status = $('ldfSt').value; filt.q = $('ldfQ').value.trim();
    pg = 1;   // suzgec degisince ilk sayfa
    clearTimeout(qt); qt = setTimeout(function () { fetchPool(true); }, 250);
  }

  // ---------- aday ekle ----------
  function resultHtml(r) {
    var rows = [['Eklendi', r.added], ['Zaten havuzda', r.duplicate], ['Engelli (iş ortağı, müşteri ya da elle)', r.suppressed], ['Firma başı sınır', r.companyCap], ['Doğrulanmamış e-posta', r.unverified], ['Ücretsiz e-posta servisi', r.freeMail], ['E-postası yok', r.noEmail], ['Geçersiz', r.invalid]];
    return '<div class="ld-res">' + rows.filter(function (x) { return x[1]; }).map(function (x) { return '<span><b>' + n(x[1]) + '</b> ' + x[0] + '</span>'; }).join('') +
      (r.credits ? '<span class="warn"><b>' + n(r.credits) + '</b> Apollo kredisi harcandı</span>' : '') + (r.creditsWasted ? '<span class="warn"><b>' + n(r.creditsWasted) + '</b> kişi açıldıktan sonra engelli çıktı</span>' : '') + '</div>' + (r.notes || []).map(function (t) { return '<div class="lv-alert warn">' + esc(t) + '</div>'; }).join('');
  }
  function importView() {
    var s = S.settings;
    $('ldImport').innerHTML =
      '<div class="ld-cols"><section class="ld-box"><h2 class="lv-h2">CSV ya da Excel’den</h2><p class="lv-hint">Instantly ya da Apollo’nun dışa aktarma dosyası veya başlıkları Ad, Soyad, Ünvan, Firma, E-posta olan bir liste. Excel’de “CSV olarak kaydet” yeterli. Engelli, tekrar eden, doğrulanmamış ve firma başı sınırı aşan kişiler otomatik elenir; varsa firma tanımı da alınır ve kişisel girişte kullanılır.</p>' +
      '<form id="ldCsv" data-noload="1" class="ld-form"><div class="lv-row"><label class="lv-f"><span>Kategori</span>' + catSel('ldcCat', 'musteri') + '</label>' +
      '<label class="lv-f"><span>Kaynak</span><select class="input" id="ldcSrc"><option value="instantly">Instantly</option><option value="apollo">Apollo</option><option value="csv">Diğer liste</option></select></label></div><label class="lv-f"><span>Dosya</span><input type="file" id="ldcFile" accept=".csv,.txt"></label>' +
      '<div class="lv-acts"><button class="btn-cyan" type="submit">Yükle ve ele</button></div></form><div id="ldcRes"></div></section>' +
      '<section class="ld-box"><h2 class="lv-h2">Apollo’dan</h2>' + (S.hasApollo ? '<p class="lv-hint">Ayarlar’daki filtrelerle arama yapılır. Arama ücretsizdir; e-posta açmak kredi harcar ve yalnız elemeden geçenler için yapılır. Her çağrı kaldığı sayfadan devam eder.</p>' :
        '<div class="lv-alert warn">web.config içinde Apollo.ApiKey tanımlı değil.</div>') +
      '<form id="ldAp" data-noload="1" class="ld-form"><label class="lv-f"><span>Kategori</span>' + catSel('ldaCat', 'musteri') + '</label><label class="lv-f"><span>Getirilecek kişi (en fazla 30)</span><input class="input" type="number" id="ldaWant" min="1" max="30" value="' + Math.min(30, Math.ceil(s.dailyNew / 2)) + '"></label>' +
      '<div class="lv-acts"><button class="btn-ghost" type="button" id="ldaTest"' + (S.hasApollo ? '' : ' disabled') + '>Bağlantıyı test et (kredi harcamaz)</button><button class="btn-cyan" type="submit"' + (S.hasApollo ? '' : ' disabled') + '>Apollo’dan getir</button></div></form><div id="ldaRes"></div></section></div>';
  }

  // ---------- onay kuyrugu ----------
  function queueView() {
    var q = S.queue || [], drafts = q.filter(function (m) { return m.status === 'draft'; }), appr = q.filter(function (m) { return m.status === 'approved'; });
    var h = '<div class="ld-bar"><form id="ldDr" data-noload="1" class="lv-inline">' + catSel('lddCat', 'musteri') + '<input class="input ld-num" type="number" id="lddMax" min="1" max="100" value="' + S.settings.dailyNew + '" aria-label="En fazla taslak"><button class="btn-cyan" type="submit">Yeni adaylardan taslak oluştur</button></form>' +
      (drafts.length ? '<button class="btn-ghost" type="button" id="ldAll">Bütün taslakları onayla (' + drafts.length + ')</button>' : '') +
      (drafts.length + appr.length ? '<button class="btn-ghost" type="button" id="ldFixH" title="Ad soyadlı selamlamaları Ad Bey / Ad Hanım yapar">Hitapları düzelt</button>' : '') + '</div>' +
      (function () {   // gonderimin gercek durumu (acik/kapali, saatler, gunluk sinir)
        var st = S.settings, tail = (st.signature ? ' İmza ve ret cümlesi' : ' Ret cümlesi') + ' gönderimde otomatik eklenir' + (st.bcc ? '; her e-postanın gizli kopyası ' + esc(st.bcc) + ' adresine gider' : '') + '.';
        return st.sendEnabled
          ? '<div class="lv-alert info">Gönderim açık: onaylanan e-postalar ' + esc(st.sendFrom) + '–' + esc(st.sendTo) + ' arasında, kutu başına günde en fazla ' + n(st.dailyPerMailbox) + ' e-posta olacak şekilde sırayla gider.' + tail + '</div>'
          : '<div class="lv-alert warn">Gönderim kapalı. Onaylanan e-postalar kuyrukta bekler; Ayarlar › Gönderim bölümünden açtığınızda mesai saatleri içinde sırayla gider.' + tail + '</div>';
      })();
    var qpages = Math.max(1, Math.ceil(drafts.length / PS)); if (qpg > qpages) qpg = qpages;
    var shown = drafts.slice((qpg - 1) * PS, qpg * PS);
    h += drafts.length ? pager('queue', qpg, drafts.length) + shown.map(card).join('') + pager('queue', qpg, drafts.length) : '<div class="lv-empty">Onay bekleyen taslak yok.</div>';
    var fail = q.filter(function (m) { return m.status === 'failed'; }), sent = q.filter(function (m) { return m.status === 'sent'; });
    if (fail.length) h += '<h2 class="lv-h2">Gönderilemeyenler (' + fail.length + ')</h2>' + fail.map(function (m) { return '<div class="ld-appr ld-fail"><div><b>' + esc(m.name) + '</b> · ' + esc(m.company) + '<small>' + esc(m.subject) + '</small><small class="err">' + esc(m.error || 'Bilinmeyen hata') + '</small></div><button class="btn-ghost lv-sm" data-do="retry" data-id="' + m.id + '">Tekrar dene</button></div>'; }).join('');
    if (appr.length) h += '<h2 class="lv-h2">Onaylananlar (' + appr.length + ')</h2>' + appr.map(function (m) { return '<div class="ld-appr"><div><b>' + esc(m.name) + '</b> · ' + esc(m.company) + '<small>' + esc(m.subject) + ' · ' + esc(m.mailbox) + '</small></div><button class="btn-ghost lv-sm" data-do="unapprove" data-id="' + m.id + '">Onayı geri al</button></div>'; }).join('');
    if (sent.length) h += '<h2 class="lv-h2">Son gönderilenler</h2>' + sent.map(function (m) { return '<div class="ld-appr"><div><b>' + esc(m.name) + '</b> · ' + esc(m.company) + '<small>' + (m.step === 9 ? 'Portaldan yanıt' : m.step === 0 ? 'İlk e-posta' : m.step + '. takip') + ' · ' + esc(m.subject) + ' · ' + esc(m.mailbox) + '</small></div><span class="lv-pill ok">' + esc(m.sentAt) + '</span></div>'; }).join('');
    $('ldQueue').innerHTML = h;
  }

  // ---------- yanitlar ----------
  function repliesView() {
    var l = S.replies || [], en = S.engine || {};
    var h = en.graph ? '' : '<div class="lv-alert warn">Yanıtların okunması için web.config içinde Graph ayarları tanımlı olmalı. Tanımlandığında kutular 10 dakikada bir okunur.</div>';
    h += '<p class="lv-hint">Yalnız adaylarımızdan gelen yanıtlar ve geri dönen adres bildirimleri burada görünür. Otomatik yanıtlar, çıkmak isteyenler ve geri dönen adresler kendiliğinden işlenir; ilgilenen ve insan cevabı gerektiren yanıtlar öne çıkar ve bildirim gönderilir.</p>';
    if (!l.length) { $('ldRep').innerHTML = h + '<div class="lv-empty">Henüz yanıt yok.</div>'; return; }
    h += l.map(function (r) {
      var c = CLS[r.cls] || CLS.other;
      return '<article class="lv-item ld-rep' + (r.handled ? ' done' : '') + '" id="ldr-' + r.id + '"><div class="lv-ih"><h3>' + esc(r.name) + '</h3><span class="lv-pill ' + c[1] + '">' + c[0] + '</span>' +
        '<div class="lv-meta">' + esc(r.title) + (r.company ? ', ' + esc(r.company) : '') + ' · ' + esc(r.from) + ' · ' + esc(r.at) + ' · ' + esc(r.mailbox) + (r.answered ? ' · yanıt verildi' : '') + (r.handled ? ' · ele alındı' : '') + '</div></div>' +
        (r.summary ? '<div class="ld-sum"><b>Özet:</b> ' + esc(r.summary) + '</div>' : '') +
        '<div class="ld-rbody">' + esc(r.body) + '</div>' + (r.sentSubject ? '<small class="ld-ref">Yanıtlanan e-posta: ' + esc(r.sentSubject) + '</small>' : '') +
        '<div class="ld-racts"><label class="lv-f ld-cls"><span>Sınıf</span><select class="input" data-do="cls" data-id="' + r.id + '">' + Object.keys(CLS).map(function (k) { return '<option value="' + k + '"' + (k === r.cls ? ' selected' : '') + '>' + CLS[k][0] + '</option>'; }).join('') + '</select></label>' +
        '<button class="btn-ghost lv-sm" data-do="' + (r.handled ? 'unhandled' : 'handled') + '" data-id="' + r.id + '">' + (r.handled ? 'Ele alınmadı olarak işaretle' : 'Ele alındı') + '</button></div>' +
        '<details class="ld-answer"><summary>Portaldan yanıtla</summary><textarea class="input" rows="5" id="ldra-' + r.id + '" placeholder="Yanıtınız aynı e-posta zincirinde ' + esc(r.mailbox) + ' adresinden gider; imza ve gizli kopya eklenir."></textarea>' +
        '<div class="lv-acts"><button class="btn-cyan" data-do="answer" data-id="' + r.id + '">Yanıtı gönder</button></div></details></article>';
    }).join('');
    $('ldRep').innerHTML = h;
  }
  function card(m) {
    var sig = S.settings.signature, opt = S.settings.optOut;
    return '<article class="lv-item ld-msg" id="ldm-' + m.id + '"><div class="lv-ih"><h3>' + esc(m.name) + '</h3><span class="lv-pill info">' + CAT[m.category] + ' · ' + (PER[m.persona] || '') + '</span>' +
      '<div class="lv-meta">' + esc(m.title) + (m.company ? ', ' + esc(m.company) : '') + ' · ' + esc(m.email) + ' · gönderen ' + esc(m.mailbox) + '</div>' +
      '<div class="ld-hitap' + (m.hitapSure ? '' : ' unsure') + '">Hitap: <b>' + esc(m.hitap) + '</b>' + (m.hitapSure ? '' : ' <span class="lv-pill warn">tahmin, kontrol edin</span>') +
      '<span class="ld-gsw" role="group" aria-label="Hitap"><button type="button" data-do="gender" data-g="E" data-cid="' + m.contactId + '"' + (/ Bey$/.test(m.hitap) ? ' class="on"' : '') + '>Bey</button>' +
      '<button type="button" data-do="gender" data-g="K" data-cid="' + m.contactId + '"' + (/ Hanım$/.test(m.hitap) ? ' class="on"' : '') + '>Hanım</button></span></div></div>' +
      '<div class="ld-ed"><div class="ld-ed-l"><label class="lv-f"><span>Konu</span><input class="input" data-k="subject" maxlength="200" value="' + esc(m.subject) + '"></label>' +
      '<label class="lv-f"><span>Metin <em>(düz yazı ya da HTML)</em></span><textarea class="input ld-body" data-k="body" data-live="1" rows="12">' + esc(m.body) + '</textarea></label></div>' +
      '<div class="ld-ed-r"><span class="ld-pvh">Alıcının göreceği hali</span><iframe class="ld-pv" sandbox="" title="E-posta önizlemesi"></iframe></div></div>' +
      '<div class="lv-acts"><button class="btn-cyan lv-okb" data-do="approve" data-id="' + m.id + '">Onayla</button><button class="btn-ghost" data-do="save" data-id="' + m.id + '">Kaydet</button><button class="btn-ghost ld-aib" data-do="aicheck" data-id="' + m.id + '">✓ AI ile kontrol et</button><button class="btn-ghost ld-aib" data-do="aiwrite" data-id="' + m.id + '">✦ AI ile düzenle</button><button class="btn-ghost lv-bad" data-do="cancel" data-id="' + m.id + '">İptal et</button>' +
      (isHtml(m.body) ? '<span class="lv-pill info">HTML</span>' : '') + '</div><div class="ld-ai" id="ldai-' + m.id + '" hidden></div></article>';
  }

  // ---------- AI ile kontrol et / duzenle (taslaga ozel) ----------
  var aiRes = {};
  function cardVals(id) { var c = $('ldm-' + id); return { subject: c.querySelector('[data-k=subject]').value, body: c.querySelector('[data-k=body]').value }; }
  function issueRow(x) {
    return '<li><span class="lv-pill ' + (x.auto ? 'warn' : 'info') + '">' + esc(x.type || 'ifade') + '</span>' +
      (x.quote ? '<del>' + esc(x.quote) + '</del>' : '') + (x.suggestion ? ' <b>→</b> <ins>' + esc(x.suggestion) + '</ins>' : '') + (x.why ? '<small>' + esc(x.why) + '</small>' : '') + '</li>';
  }
  function checkPanel(id, d, cur) {
    var auto = d.auto || [], ai = d.ai, list = auto.concat(ai && ai.issues ? ai.issues : []);
    var changed = ai && ((ai.subject || '') !== cur.subject || (ai.body || '') !== cur.body);
    var h = '<div class="ld-aih"><b>Kontrol sonucu</b>' + (ai && ai.summary ? '<span>' + esc(ai.summary) + '</span>' : '') + '</div>';
    if (d.aiError) h += '<div class="lv-alert warn">' + esc(d.aiError) + '</div>';
    h += list.length ? '<ul class="ld-iss">' + list.map(issueRow).join('') + '</ul>' : '<div class="lv-alert info">Hata bulunmadı.</div>';
    h += '<div class="lv-acts">' + (changed ? '<button class="btn-cyan" data-do="aiapply" data-kind="check" data-id="' + id + '">Düzeltilmiş hali uygula</button>' : '') + '<button class="btn-ghost" data-do="aiclose" data-id="' + id + '">Kapat</button></div>';
    return h;
  }
  function writePanel(id, ai) {
    return '<div class="ld-aih"><b>AI önerisi</b>' + (ai.notes ? '<span>' + esc(ai.notes) + '</span>' : '') + '</div>' +
      '<div class="ld-aisub"><small>Konu</small>' + esc(ai.subject || '') + '</div>' +
      '<iframe class="ld-pv ld-aipv" sandbox="" title="AI önerisinin önizlemesi"></iframe>' +
      '<div class="lv-acts"><button class="btn-cyan" data-do="aiapply" data-kind="write" data-id="' + id + '">Uygula</button><button class="btn-ghost" data-do="aiwrite" data-id="' + id + '">Başka bir versiyon</button><button class="btn-ghost" data-do="aiclose" data-id="' + id + '">Vazgeç</button></div>';
  }
  function runAi(op, id, btn) {
    var cur = cardVals(id), box = $('ldai-' + id);
    busy(btn, true, op === 'aicheck' ? 'Kontrol ediliyor…' : 'Yazılıyor…');
    api(op, { id: id, subject: cur.subject, body: cur.body }).then(function (d) {
      busy(btn, false);
      if (!d.ok) { toast(d.error || 'AI işlemi tamamlanamadı.'); return; }
      aiRes[id] = { kind: op, data: d.data }; box.hidden = false;
      if (op === 'aicheck') box.innerHTML = checkPanel(id, d.data, cur);
      else {
        box.innerHTML = writePanel(id, d.data.ai || {});
        var f = box.querySelector('.ld-aipv'); f.srcdoc = '<!doctype html><meta charset="utf-8"><body style="margin:0;background:#fff">' + toHtml((d.data.ai || {}).body || '') + '</body>';
        f.onload = function () { try { f.style.height = Math.max(220, Math.min(700, f.contentDocument.body.scrollHeight + 12)) + 'px'; } catch (e) { } };
      }
      box.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
    }).catch(function () { busy(btn, false); toast('Bağlantı hatası.'); });
  }
  function applyAi(id) {
    var r = aiRes[id]; if (!r) return; var ai = r.data.ai || {}, c = $('ldm-' + id);
    if (ai.subject) c.querySelector('[data-k=subject]').value = ai.subject;
    if (ai.body) { var ta = c.querySelector('[data-k=body]'); ta.value = ai.body; live(ta); }
    $('ldai-' + id).hidden = true; toast('Düzenleyiciye aktarıldı. Kaydetmek için “Kaydet” ya da “Onayla”.');
  }

  // ---------- sablonu AI ile duzenle ----------
  var tplRes = {};
  function runTpl(id, btn) {
    var f = btn.closest('form'), box = $('ldtai-' + id);
    busy(btn, true, 'Yazılıyor…');
    api('aitpl', { id: id, subject: f.elements.subject.value, body: f.elements.body.value, instruction: f.elements.instruction.value }).then(function (d) {
      busy(btn, false);
      if (!d.ok) { toast(d.error || 'AI işlemi tamamlanamadı.'); return; }
      var ai = (d.data && d.data.ai) || {}; tplRes[id] = ai; box.hidden = false;
      box.innerHTML = '<div class="ld-aih"><b>AI önerisi</b>' + (ai.notes ? '<span>' + esc(ai.notes) + '</span>' : '') + '</div>' +
        '<div class="ld-aisub"><small>Konu</small>' + esc(ai.subject || '') + '</div><iframe class="ld-pv ld-aipv" sandbox="" title="AI önerisinin önizlemesi"></iframe>' +
        '<div class="lv-acts"><button type="button" class="btn-cyan" data-do="tplapply" data-id="' + id + '">Uygula</button><button type="button" class="btn-ghost" data-do="aitpl" data-id="' + id + '">Başka bir versiyon</button><button type="button" class="btn-ghost" data-do="tplclose" data-id="' + id + '">Vazgeç</button></div>';
      var fr = box.querySelector('.ld-aipv'); fr.srcdoc = '<!doctype html><meta charset="utf-8"><body style="margin:0;background:#fff">' + toHtml(sample(ai.body || '')) + '</body>';
      fr.onload = function () { try { fr.style.height = Math.max(220, Math.min(760, fr.contentDocument.body.scrollHeight + 12)) + 'px'; } catch (e) { } };
      box.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
    }).catch(function () { busy(btn, false); toast('Bağlantı hatası.'); });
  }
  function applyTpl(id) {
    var ai = tplRes[id]; if (!ai) return; var box = $('ldtai-' + id), f = box.closest('form');
    if (ai.subject) f.elements.subject.value = ai.subject;
    if (ai.body) { f.elements.body.value = ai.body; live(f.elements.body); }
    box.hidden = true; toast('Şablona aktarıldı. Kalıcı olması için “Şablonu kaydet”.');
  }

  // ---------- sablonlar ----------
  function tplView() {
    var h = '<p class="lv-hint">Yer tutucular: {Hitap} (alıcının adı ve Bey/Hanım, ör. Ayşe Hanım), {Ad}, {Firma}, {Unvan}, {Giris} (Claude’un her kişi için yazdığı tek cümlelik giriş). İmzanızı şablonun sonuna kendiniz yazabilirsiniz; Ayarlar’daki imza alanı boşsa ayrıca imza eklenmez. Adım 0 ilk e-posta, 1 ve 2 takip e-postalarıdır. Takip e-postaları aynı e-posta zincirinde yanıt olarak gider.</p>' +
      '<div class="lv-alert info">Metinler düz yazı ya da HTML olabilir; etiket içeren metin HTML kabul edilir. Soğuk e-postada sade HTML önerilir: kalın, bağlantı ve paragraf yeterli. Görsel, renkli tablo ve çok sayıda bağlantı spam puanını artırır; script, iframe ve form kullanılamaz.</div>';
    h += (S.templates || []).map(function (t) {
      return '<details class="ld-tpl"><summary><b>' + CAT[t.category] + ' · ' + (PER[t.persona] || t.persona) + ' · ' + (t.step === 0 ? 'İlk e-posta' : t.step + '. takip') + '</b>' + (t.active ? '' : ' <span class="lv-pill mute">Kapalı</span>') + '<small>' + esc(t.subject) + '</small></summary>' +
        '<form class="ld-tf" data-noload="1" data-id="' + t.id + '"><label class="lv-f"><span>Konu</span><input class="input" name="subject" maxlength="200" value="' + esc(t.subject) + '"></label>' +
        '<div class="ld-ed"><div class="ld-ed-l"><label class="lv-f"><span>Metin <em>(düz yazı ya da HTML)</em></span><textarea class="input ld-body" name="body" data-live="1" data-sample="1" rows="12">' + esc(t.body) + '</textarea></label></div>' +
        '<div class="ld-ed-r"><span class="ld-pvh">Örnek kişiyle önizleme</span><iframe class="ld-pv" sandbox="" title="Şablon önizlemesi"></iframe></div></div>' +
        '<label class="lv-f ld-ins"><span>AI için talimat <em>(isteğe bağlı: ör. “daha kısa ve samimi yaz; imzam: Yılmaz Güney · İş Geliştirme · River AI”)</em></span><textarea class="input" name="instruction" rows="2"></textarea></label>' +
        '<label class="lv-chk"><input type="checkbox" name="active"' + (t.active ? ' checked' : '') + '> Etkin</label><div class="lv-acts"><button class="btn-cyan" type="submit">Şablonu kaydet</button><button class="btn-ghost ld-aib" type="button" data-do="aitpl" data-id="' + t.id + '">✦ AI ile düzenle</button></div>' +
        '<div class="ld-ai" id="ldtai-' + t.id + '" hidden></div></form></details>';
    }).join('');
    $('ldTpl').innerHTML = h;
  }

  // ---------- engelleme ----------
  function blockView() {
    var l = S.suppression || [];
    $('ldBlock').innerHTML = '<div class="lv-alert info">Otomatik engellenenler (' + n(S.autoBlocked) + ' domain): portaldaki iş ortağı ve müşteri firmalarının domainleri, kendi domainlerimiz. Ücretsiz e-posta servisleri (Gmail, Hotmail vb.) de hiç alınmaz. Bu listeler elle yönetilmez, kendiliğinden güncellenir.</div>' +
      '<form id="ldSup" data-noload="1" class="lv-inline"><select class="input" name="kind" aria-label="Tür"><option value="domain">Domain</option><option value="email">E-posta</option></select><input class="input" name="value" placeholder="firma.com.tr ya da kisi@firma.com.tr" aria-label="Değer" required><input class="input" name="reason" placeholder="Neden (isteğe bağlı)" aria-label="Neden"><button class="btn-cyan" type="submit">Engelle</button></form>' +
      (l.length ? '<div class="lv-tblw"><table class="lv-tbl"><thead><tr><th>Tür</th><th>Değer</th><th>Neden</th><th>Tarih</th><th></th></tr></thead><tbody>' + l.map(function (x) {
        return '<tr><td>' + (x.kind === 'domain' ? 'Domain' : 'E-posta') + '</td><td>' + esc(x.value) + '</td><td>' + esc(x.reason || '·') + '</td><td>' + esc(x.at) + '</td><td><button class="btn-ghost lv-sm" data-do="supdel" data-id="' + x.id + '">Kaldır</button></td></tr>';
      }).join('') + '</tbody></table></div>' : '<div class="lv-empty">Elle eklenmiş engel yok.</div>');
  }

  // ---------- ayarlar ----------
  function setView() {
    var s = S.settings, pretty = function (j) { try { return JSON.stringify(JSON.parse(j), null, 2); } catch (e) { return j; } };
    var en = S.engine || {}, chip = function (ok, t) { return '<span class="lv-pill ' + (ok ? 'ok' : 'warn') + '">' + t + '</span>'; };
    $('ldSet').innerHTML = '<form id="ldSetF" data-noload="1" class="ld-form">' +
      '<section class="ld-box ld-send"><div class="ld-sendh"><h2 class="lv-h2">Gönderim</h2><div class="ld-chips">' + chip(en.graph, en.graph ? 'Graph tanımlı' : 'Graph tanımlı değil') + chip(en.claude, en.claude ? 'Claude tanımlı' : 'Claude tanımlı değil') +
        '<span class="lv-pill ' + (en.inWindow ? 'ok' : 'mute') + '">' + (en.inWindow ? 'Mesai penceresinde' : 'Mesai penceresi dışında') + '</span><span class="lv-pill info">Bugün ' + n(en.sentMusteri) + ' + ' + n(en.sentDanisman) + ' gönderildi</span></div></div>' +
      '<label class="lv-chk ld-big"><input type="checkbox" name="sendEnabled"' + (s.sendEnabled ? ' checked' : '') + '> Gönderim açık <em>(ısınma bitmeden açmayın)</em></label>' +
      '<div class="lv-row3"><label class="lv-f"><span>Kutu başına günlük e-posta</span><input class="input" type="number" name="dailyPerMailbox" min="1" max="50" value="' + s.dailyPerMailbox + '"></label>' +
      '<label class="lv-f"><span>Gönderim başlangıcı</span><input class="input" type="time" name="sendFrom" value="' + esc(s.sendFrom) + '"></label><label class="lv-f"><span>Gönderim bitişi</span><input class="input" type="time" name="sendTo" value="' + esc(s.sendTo) + '"></label></div>' +
      '<div class="lv-row"><label class="lv-chk"><input type="checkbox" name="followupAuto"' + (s.followupAuto ? ' checked' : '') + '> Takip e-postaları onaysız gitsin</label><label class="lv-chk"><input type="checkbox" name="personalize"' + (s.personalize ? ' checked' : '') + '> Taslaklarda Claude ile hitap ve kişisel giriş yaz</label></div>' +
      '<label class="lv-f"><span>Yanıt bildirimi <em>(ilgilenen ve insan cevabı gerektiren yanıtlar)</em></span><input class="input" name="notify" value="' + esc(s.notify) + '"></label>' +
      '<div class="lv-acts"><button class="btn-ghost" type="button" id="ldGt">Graph bağlantısını test et</button><button class="btn-ghost" type="button" id="ldTick">Şimdi çalıştır (yanıt oku, takip, gönderim)</button></div><div id="ldGtRes"></div></section>' +
      '<div class="lv-row3"><label class="lv-f"><span>Günlük yeni aday hedefi</span><input class="input" type="number" name="dailyNew" min="1" max="60" value="' + s.dailyNew + '"></label>' +
      '<label class="lv-f"><span>Firma başına en fazla kişi</span><input class="input" type="number" name="maxPerCompany" min="1" max="5" value="' + s.maxPerCompany + '"></label>' +
      '<label class="lv-chk ld-chk"><input type="checkbox" name="onlyVerified"' + (s.onlyVerified ? ' checked' : '') + '> Yalnız doğrulanmış (verified) e-postalar</label></div>' +
      '<div class="lv-row"><label class="lv-f"><span>Müşteri gönderim kutusu</span><input class="input" name="mailboxMusteri" value="' + esc(s.mailboxMusteri) + '"></label><label class="lv-f"><span>Danışman gönderim kutusu</span><input class="input" name="mailboxDanisman" value="' + esc(s.mailboxDanisman) + '"></label></div>' +
      '<label class="lv-f"><span>Gizli kopya (BCC) <em>(gönderilen her e-postanın kopyası; birden fazlaysa virgülle)</em></span><input class="input" name="bcc" value="' + esc(s.bcc) + '"></label>' +
      '<label class="lv-f"><span>İmza <em>(boş bırakırsanız imza eklenmez; imzanızı şablonların içine yazabilirsiniz)</em></span><textarea class="input" name="signature" rows="4">' + esc(s.signature) + '</textarea></label>' +
      '<label class="lv-f"><span>Ret cümlesi <em>(her e-postanın sonuna eklenir; yasal zorunluluk)</em></span><input class="input" name="optOut" value="' + esc(s.optOut) + '"></label>' +
      '<div class="lv-row"><label class="lv-f"><span>Apollo filtresi: müşteri <em>(JSON)</em></span><textarea class="input ld-json" name="apolloMusteri" rows="12" spellcheck="false">' + esc(pretty(s.apolloMusteri)) + '</textarea></label>' +
      '<label class="lv-f"><span>Apollo filtresi: danışman <em>(JSON)</em></span><textarea class="input ld-json" name="apolloDanisman" rows="12" spellcheck="false">' + esc(pretty(s.apolloDanisman)) + '</textarea></label></div>' +
      '<p class="lv-hint">Filtre alanları Apollo’nun kişi arama API’sindeki adlarla aynıdır (person_locations, person_titles, organization_num_employees_ranges, q_organization_keyword_tags, contact_email_status). Değiştirdikten sonra “Aday ekle” sekmesinden bağlantıyı test edin.</p>' +
      '<div class="lv-acts"><button class="btn-cyan" type="submit">Ayarları kaydet</button></div></form>';
  }

  // ---------- genel ----------
  var views = { overview: overview, replies: repliesView, pool: pool, 'import': importView, queue: queueView, tpl: tplView, block: blockView, settings: setView };
  function render() { if (!S) return; stats(); views[tab](); liveAll(); }
  function showTab(t) {
    tab = t;
    [].forEach.call(root.querySelectorAll('.lv-tabs button'), function (b) { b.setAttribute('aria-selected', b.getAttribute('data-tab') === t ? 'true' : 'false'); });
    [].forEach.call(root.querySelectorAll('.lv-view'), function (v) { v.hidden = v.getAttribute('data-view') !== t; });
    render();
  }
  root.addEventListener('click', function (e) {
    var t = e.target.closest('[data-tab]'); if (t) { showTab(t.getAttribute('data-tab')); return; }
    if (e.target.id === 'ldaTest') {
      var b = e.target; busy(b, true, 'Deneniyor…');
      api('test', { category: $('ldaCat').value }).then(function (d) {
        busy(b, false);
        if (!d.ok) { $('ldaRes').innerHTML = '<div class="lv-alert bad">' + esc(d.error) + '</div>'; return; }
        var x = d.data.test; S = d.data;
        $('ldaRes').innerHTML = '<div class="lv-alert info">Bağlantı çalışıyor. Filtrelere uyan yaklaşık <b>' + n(x.total) + '</b> kişi var. Örnek:</div><ul class="ld-sample">' + x.sample.map(function (p) { return '<li><b>' + esc(p.name) + '</b>, ' + esc(p.title) + ' · ' + esc(p.company) + (p.emailStatus ? ' <small>' + esc(p.emailStatus) + '</small>' : '') + '</li>'; }).join('') + '</ul>';
      }).catch(function () { busy(b, false); toast('Bağlantı hatası.'); });
      return;
    }
    if (e.target.id === 'ldGt' || e.target.id === 'ldTick') {
      var bt = e.target, isT = bt.id === 'ldTick'; busy(bt, true, isT ? 'Çalışıyor…' : 'Deneniyor…');
      api(isT ? 'tick' : 'graphtest', {}).then(function (d) {
        busy(bt, false); if (!d.ok) { $('ldGtRes').innerHTML = '<div class="lv-alert bad">' + esc(d.error) + '</div>'; return; }
        var res = isT ? '<div class="lv-alert info">' + esc(d.data.tick) + '</div>' : d.data.graphTest.map(function (x) { return '<div class="lv-alert ' + (x.ok ? 'info' : 'warn') + '">' + esc(x.text) + '</div>'; }).join('');
        S = d.data; stats(); $('ldGtRes').innerHTML = res;
      }).catch(function () { busy(bt, false); toast('Bağlantı hatası.'); });
      return;
    }
    if (e.target.id === 'ldFixH') { busy(e.target, true, 'Düzeltiliyor…'); api('fixhitap', {}).then(function (d) { load(d, d.data && d.data.msg); }); return; }
    if (e.target.id === 'ldAll') { if (confirm('Bütün taslaklar onaylansın mı?')) api('approveAll', {}).then(function (d) { load(d, 'Taslaklar onaylandı.'); }); return; }
    var pgb = e.target.closest('[data-pg]');
    if (pgb && !pgb.disabled) {
      var to = +pgb.getAttribute('data-pg'), kind = pgb.getAttribute('data-kind');
      if (kind === 'pool') { pg = to; fetchPool(false).then(function () { $('ldPool').scrollIntoView({ block: 'start', behavior: 'smooth' }); }); }
      else { qpg = to; queueView(); liveAll(); $('ldQueue').scrollIntoView({ block: 'start', behavior: 'smooth' }); }
      return;
    }
    var a = e.target.closest('[data-do]'); if (!a) return;
    var op = a.getAttribute('data-do'), id = a.getAttribute('data-id');
    if (op === 'exclude') { api('exclude', { id: id }).then(function (d) { load(d, 'Aday havuzdan çıkarıldı.'); }); return; }
    if (op === 'block') { if (confirm('Bu kişi havuzdan çıkarılıp kalıcı olarak engellensin mi?')) api('exclude', { id: id, block: '1', reason: 'Engellendi' }).then(function (d) { load(d, 'Kişi engellendi.'); }); return; }
    if (op === 'supdel') { api('supdel', { id: id }).then(function (d) { load(d, 'Engel kaldırıldı.'); }); return; }
    if (op === 'retry') { api('retry', { id: id }).then(function (d) { load(d, 'Yeniden kuyruğa alındı.'); }); return; }
    if (op === 'handled' || op === 'unhandled') { api('reply', { id: id, 'do': op }).then(function (d) { load(d, op === 'handled' ? 'Ele alındı.' : 'Geri alındı.'); }); return; }
    if (op === 'answer') {
      var tx = $('ldra-' + id).value.trim(); if (tx.length < 5) { toast('Yanıt metnini yazın.'); return; }
      busy(a, true, 'Gönderiliyor…'); api('reply', { id: id, 'do': 'send', body: tx }).then(function (d) { busy(a, false); load(d, 'Yanıt gönderildi.'); }).catch(function () { busy(a, false); toast('Bağlantı hatası.'); });
      return;
    }
    if (op === 'unapprove') { api('msg', { id: id, 'do': 'unapprove' }).then(function (d) { load(d, 'Onay geri alındı.'); }); return; }
    if (op === 'aicheck' || op === 'aiwrite') { runAi(op, id, a); return; }
    if (op === 'gender') { api('gender', { contactId: a.getAttribute('data-cid'), g: a.getAttribute('data-g') }).then(function (d) { load(d, 'Hitap güncellendi; bu kişinin bekleyen e-postaları düzeltildi.'); }); return; }
    if (op === 'aitpl') { runTpl(id, a); return; }
    if (op === 'tplapply') { applyTpl(id); return; }
    if (op === 'tplclose') { $('ldtai-' + id).hidden = true; return; }
    if (op === 'aiapply') { applyAi(id); return; }
    if (op === 'aiclose') { $('ldai-' + id).hidden = true; return; }
    if (op === 'approve' || op === 'save' || op === 'cancel') {
      if (op === 'cancel' && !confirm('Taslak iptal edilip aday havuzdan çıkarılsın mı?')) return;
      var c = $('ldm-' + id), sub = c.querySelector('[data-k=subject]').value, body = c.querySelector('[data-k=body]').value;
      busy(a, true); api('msg', { id: id, 'do': op, subject: sub, body: body }).then(function (d) { busy(a, false); load(d, op === 'approve' ? 'Onaylandı.' : op === 'save' ? 'Kaydedildi.' : 'İptal edildi.'); });
    }
  });
  root.addEventListener('input', function (e) { if (e.target.id === 'ldfQ') refilter(); if (e.target.hasAttribute && e.target.hasAttribute('data-live')) { var ta = e.target; clearTimeout(lt); lt = setTimeout(function () { live(ta); }, 250); } });
  root.addEventListener('toggle', function (e) { if (e.target.classList && e.target.classList.contains('ld-tpl') && e.target.open) { var ta = e.target.querySelector('textarea[data-live]'); if (ta) live(ta); } }, true);
  root.addEventListener('change', function (e) {
    if (e.target.id === 'ldfCat' || e.target.id === 'ldfSt') refilter();
    if (e.target.getAttribute && e.target.getAttribute('data-do') === 'cls') api('reply', { id: e.target.getAttribute('data-id'), 'do': 'class', cls: e.target.value }).then(function (d) { load(d, 'Sınıf güncellendi; aday durumu buna göre değişti.'); });
  });
  root.addEventListener('submit', function (e) {
    var f = e.target; e.preventDefault(); var b = f.querySelector('button[type=submit]');
    if (f.id === 'ldCsv') {
      var file = $('ldcFile').files[0]; if (!file) { toast('CSV dosyası seçin.'); return; }
      var fd = new FormData(); fd.append('category', $('ldcCat').value); fd.append('source', $('ldcSrc').value); fd.append('file', file); busy(b, true, 'Yükleniyor…');
      api('import', fd, true).then(function (d) { busy(b, false); if (d.ok) { S = d.data; contacts = null; stats(); $('ldcRes').innerHTML = resultHtml(d.data['import']); } else $('ldcRes').innerHTML = '<div class="lv-alert bad">' + esc(d.error) + '</div>'; });
    } else if (f.id === 'ldAp') {
      busy(b, true, 'Apollo’dan getiriliyor…');
      api('apollo', { category: $('ldaCat').value, want: $('ldaWant').value }).then(function (d) { busy(b, false); if (d.ok) { S = d.data; contacts = null; stats(); $('ldaRes').innerHTML = resultHtml(d.data['import']); } else $('ldaRes').innerHTML = '<div class="lv-alert bad">' + esc(d.error) + '</div>'; })
        .catch(function () { busy(b, false); toast('Bağlantı hatası.'); });
    } else if (f.id === 'ldDr') {
      busy(b, true); api('drafts', { category: $('lddCat').value, max: $('lddMax').value }).then(function (d) { busy(b, false); load(d, d.ok ? d.data.made + ' taslak oluşturuldu.' : null); });
    } else if (f.id === 'ldSup') {
      api('sup', { kind: f.elements.kind.value, value: f.elements.value.value, reason: f.elements.reason.value }).then(function (d) { load(d, 'Engellendi.'); });
    } else if (f.id === 'ldSetF') {
      var v = {}; [].forEach.call(f.elements, function (x) { if (x.name) v[x.name] = x.type === 'checkbox' ? (x.checked ? '1' : '0') : x.value; });
      api('settings', v).then(function (d) { load(d, 'Ayarlar kaydedildi.'); });
    } else if (f.classList.contains('ld-tf')) {
      api('template', { id: f.getAttribute('data-id'), subject: f.elements.subject.value, body: f.elements.body.value, active: f.elements.active.checked ? '1' : '0' }).then(function (d) { load(d, 'Şablon kaydedildi.'); });
    }
  });
  api('state').then(function (d) { if (!d.ok) { root.innerHTML = '<div class="flash-err">' + esc(d.error) + '</div>'; return; } S = d.data; var qt = new URLSearchParams(location.search).get('tab'); showTab(views[qt] ? qt : 'overview'); })
    .catch(function () { root.innerHTML = '<div class="flash-err">Lead Merkezi yüklenemedi. Sayfayı yenileyin.</div>'; });
})();
