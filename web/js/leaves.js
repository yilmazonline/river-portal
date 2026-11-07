// Izinlerim: yeni talep (anlik sure / donus / onay yolu), taleplerim, onay kutusu, kurallar (yonetici ve IK).
// Kesin hesap ve kurallar sunucuda (LeaveAction.ashx); buradaki hesap yalniz anlik gorsel geri bildirim.
(function () {
  'use strict';
  var root = document.getElementById('lv'); if (!root) return;
  var API = 'LeaveAction.ashx';
  var S = null, tab = 'new', sel = null, editing = null;
  var TYPES = {
    yillik: { n: 'Yıllık izin', d: 'Ücretli, bakiyeden düşer', unit: 'workday', cover: true },
    mazeret: { n: 'Mazeret izni', d: 'Evlilik, eşin doğumu, vefat', unit: 'workday', cover: true },
    hastalik: { n: 'Hastalık ya da rapor', d: 'Rapor yüklenir, onay gerekmez', unit: 'calendar', doc: true },
    saatlik: { n: 'Saatlik izin', d: 'Aynı gün, en fazla 4 saat', unit: 'hour' },
    ucretsiz: { n: 'Ücretsiz izin', d: 'Gerekçe ve Genel Müdür onayı', unit: 'workday', cover: true, reason: true },
    egitim: { n: 'Eğitim ya da etkinlik', d: 'Konferans, sertifika sınavı', unit: 'workday', cover: true },
    dogumgunu: { n: 'Doğum günü izni', d: 'Yılda 1 gün, şirket hediyesi', unit: 'workday' },
    dogum: { n: 'Doğum izni', d: 'Yasal 16 hafta, İK onayı', unit: 'weeks', cover: true, doc: true }
  };
  var SUBS = { evlilik: ['Evlilik', 3], babalik: ['Eşinin doğum yapması', 5], vefat: ['Birinci derece yakının vefatı', 3], tasinma: ['Taşınma (şirket politikası)', 1] };

  // ---------- yardimcilar ----------
  function $(id) { return document.getElementById(id); }
  function esc(s) { return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; }); }
  function pd(k) { var a = k.split('-'); return new Date(+a[0], +a[1] - 1, +a[2]); }
  function key(d) { return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0'); }
  function add(d, n) { var x = new Date(d.getTime()); x.setDate(x.getDate() + n); return x; }
  function today() { var n = new Date(); return new Date(n.getFullYear(), n.getMonth(), n.getDate()); }
  function fDW(d) { return d.toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric', weekday: 'long' }); }
  function fDS(d) { return d.toLocaleDateString('tr-TR', { day: 'numeric', month: 'long' }); }
  function num(x) { return (Math.round(x * 10) / 10).toLocaleString('tr-TR'); }
  function person(id) { if (!S) return null; for (var i = 0; i < S.staff.length; i++) if (S.staff[i].id === id) return S.staff[i]; return null; }
  function pname(id) { var p = person(id); return p ? p.name : ''; }
  function dayVal(d) { var w = d.getDay(); if (w === 0 || w === 6) return 0; var h = S.holidays[key(d)]; return h ? (h[1] ? 0.5 : 0) : 1; }
  function api(op, data, isFile) {
    var opt = { credentials: 'same-origin' };
    if (op === 'state') return fetch(API + '?op=state', opt).then(function (r) { return r.json(); });
    var body = isFile ? data : new URLSearchParams(); if (!isFile) { body.append('op', op); for (var k in data) if (data[k] != null) body.append(k, data[k]); } else body.append('op', op);
    opt.method = 'POST'; opt.body = body; if (!isFile) opt.headers = { 'Content-Type': 'application/x-www-form-urlencoded' };
    return fetch(API, opt).then(function (r) { return r.json(); });
  }
  var tt = null;
  function toast(t) { var e = $('lvToast'); e.textContent = t; e.hidden = false; clearTimeout(tt); tt = setTimeout(function () { e.hidden = true; }, 6000); }
  function load(d) { if (!d.ok) { toast(d.error || 'İşlem tamamlanamadı.'); return false; } S = d.data; if (S.message) toast(S.message); render(); return true; }

  // ---------- hesap (gorsel) ----------
  function form() {
    var t = document.querySelector('input[name=lvType]:checked'); t = t ? t.value : 'yillik';
    var cov = TYPES[t].cover || t === 'dogumgunu', oc = S.me.onCall && cov && t !== 'saatlik';
    return { who: S.me.id, type: t, sub: t === 'mazeret' ? $('lvSub').value : '', start: t === 'saatlik' ? $('lvHD').value : $('lvStart').value, end: t === 'saatlik' ? $('lvHD').value : $('lvEnd').value,
      sh: $('lvSH').checked && t !== 'hastalik' && t !== 'dogum' && t !== 'saatlik', eh: $('lvEH').checked && t !== 'hastalik' && t !== 'dogum' && t !== 'saatlik', hf: $('lvHF').value, ht: $('lvHT').value,
      vekil: cov ? +$('lvVekil').value || null : null, oncall: oc ? +$('lvOc').value || null : null, reach: document.querySelector('input[name=lvReach]:checked').value, phone: $('lvPhone').value.trim(), note: $('lvNote').value.trim(),
      file: $('lvFile').files && $('lvFile').files[0], ok: $('lvOk').checked, cov: cov, oc: oc };
  }
  function calc(f) {
    var o = { ok: false, days: 0, hol: [], ret: '', end: f.end };
    if (f.type === 'saatlik') { if (!f.start || !f.hf || !f.ht) return o; var a = f.hf.split(':'), b = f.ht.split(':'); o.days = ((+b[0] * 60 + +b[1]) - (+a[0] * 60 + +a[1])) / 60; o.ok = true; return o; }
    if (f.type === 'dogum' && f.start) o.end = key(add(pd(f.start), 111));
    if (!f.start || !o.end || o.end < f.start) return o;
    var s = pd(f.start), e = pd(o.end), u = TYPES[f.type].unit; o.ok = true;
    if (u === 'calendar' || u === 'weeks') o.days = Math.round((e - s) / 864e5) + 1;
    else {
      for (var d = new Date(s.getTime()); d <= e; d = add(d, 1)) { o.days += dayVal(d); var h = S.holidays[key(d)]; if (h && d.getDay() % 6 !== 0) o.hol.push(fDS(d) + ' ' + h[0] + (h[1] ? ' (yarım gün)' : '')); }
      if (f.sh && dayVal(s) === 1) o.days -= 0.5; if (f.eh && dayVal(e) === 1) o.days -= 0.5;
    }
    if (f.eh) o.ret = fDW(e) + ', 13.00'; else { var n = add(e, 1), g = 0; while (dayVal(n) === 0 && g++ < 30) n = add(n, 1); o.ret = fDW(n); }
    return o;
  }
  function durTxt(t, days) { return t === 'saatlik' ? num(days) + ' saat' : t === 'hastalik' ? num(days) + ' takvim günü' : t === 'dogum' ? '16 hafta (112 takvim günü)' : num(days) + ' iş günü'; }
  function onLeave(id) { var k = key(today()); return S.cal.some(function (c) { return c.who === id && c.status === 'approved' && c.type !== 'saatlik' && c.s <= k && k <= c.e; }); }
  // sunucudaki Leaves.Chain ile ayni mantik (onizleme)
  function chain(f, days) {
    var p = person(f.who), st = S.settings, steps = [], info = [], err = null;
    var first = p.noApproval ? null : (p.approver && person(p.approver) ? p.approver : st.hr);
    if (f.type === 'hastalik' && st.sickNoApproval) { if (p.approver) info.push(p.approver); }
    else {
      if (f.type === 'dogum') { if (st.hr) steps.push({ who: st.hr, lb: 'İK onayı' }); if (p.approver) info.push(p.approver); }
      else if (first) steps.push({ who: first, lb: p.approver ? 'Yönetici onayı' : 'İK onayı' });
      else if (!p.noApproval) err = 'Onaylayıcınız tanımlı değil. Süper admin yetkisindeki yöneticinizden onaylayıcınızı belirlemesini isteyin.';
      var lng = f.type === 'yillik' && days > st.longDays, unp = f.type === 'ucretsiz' && st.unpaidSecond;
      if ((lng || unp) && st.second && st.second !== p.id && !steps.some(function (s) { return s.who === st.second; })) steps.push({ who: st.second, lb: 'Genel Müdür onayı', why: lng ? st.longDays + ' iş gününü aşan yıllık izin' : 'Ücretsiz izin' });
    }
    steps.forEach(function (s) { var a = person(s.who); if (a && onLeave(s.who) && a.delegateApprover && a.delegateApprover !== p.id) { s.orig = s.who; s.who = a.delegateApprover; } });
    steps = steps.filter(function (s) { return s.who !== p.id; });
    if (f.vekil) info.push(f.vekil); if (f.oncall) info.push(f.oncall); if (st.hr) info.push(st.hr);
    var seen = {}; info = info.filter(function (x) { var d = x === p.id || seen[x] || (steps.some(function (s) { return s.who === x; }) && x !== st.hr); if (!d) seen[x] = 1; return !d; });
    return { steps: steps, info: info, err: err };
  }
  function role(f, id) { var st = S.settings, p = person(f.who); if (id === st.hr) return 'İK ve bordro'; if (p && id === p.approver) return 'Yönetici'; if (id === f.vekil || id === f.delegate) return 'Vekil'; if (id === f.oncall || id === f.onCallTo) return 'Nöbet devri'; return 'Bilgi'; }
  function stop(st, lb, wh, ex, cls) { return '<li class="lv-stop ' + st + '"><span class="lv-dot" aria-hidden="true"></span><div><div class="lv-lb">' + esc(lb) + '</div><div class="lv-wh">' + esc(wh) + '</div>' + (ex ? '<div class="lv-ex ' + (cls || '') + '">' + esc(ex) + '</div>' : '') + '</div></li>'; }

  // ---------- form ----------
  function buildForm() {
    $('lvTypes').innerHTML = Object.keys(TYPES).map(function (k, i) { return '<label class="lv-type"><input type="radio" name="lvType" value="' + k + '"' + (i === 0 ? ' checked' : '') + '><b>' + TYPES[k].n + '</b><small>' + TYPES[k].d + '</small></label>'; }).join('');
    $('lvSub').innerHTML = Object.keys(SUBS).map(function (k) { return '<option value="' + k + '">' + SUBS[k][0] + ' (' + SUBS[k][1] + ' gün)</option>'; }).join('');
    var f = $('lvForm'); f.addEventListener('input', refresh); f.addEventListener('change', refresh);
    f.addEventListener('submit', function (e) { e.preventDefault(); submit(); });
    $('lvReset').addEventListener('click', function () { resetForm(); refresh(); });
  }
  function opts(excl, empty) { return '<option value="">' + empty + '</option>' + S.staff.filter(function (p) { return p.id !== excl; }).map(function (p) { return '<option value="' + p.id + '">' + esc(p.name) + (p.title ? ' · ' + esc(p.title) : '') + '</option>'; }).join(''); }
  function fillSelects() {
    var v = $('lvVekil').value, o = $('lvOc').value;
    $('lvVekil').innerHTML = opts(S.me.id, 'Vekil seçin'); $('lvVekil').value = v;
    $('lvOc').innerHTML = opts(S.me.id, 'Nöbeti devralacak kişi'); $('lvOc').value = o;
  }
  function resetForm() { $('lvForm').reset(); document.querySelector('input[name=lvType][value=yillik]').checked = true; $('lvOoo').checked = true; editing = null; $('lvSubmit').textContent = 'Talebi gönder'; $('lvErrs').innerHTML = ''; }
  function refresh() {
    if (!S) return;
    var f = form(), t = TYPES[f.type], c = calc(f);
    $('lvSubWrap').hidden = f.type !== 'mazeret'; $('lvDays').hidden = f.type === 'saatlik'; $('lvHours').hidden = f.type !== 'saatlik';
    $('lvEnd').disabled = f.type === 'dogum'; if (f.type === 'dogum' && c.end) $('lvEnd').value = c.end;
    $('lvHalf').hidden = f.type === 'hastalik' || f.type === 'dogum' || f.type === 'dogumgunu';
    $('lvCover').hidden = !f.cov; $('lvOcWrap').hidden = !f.oc; $('lvVReq').textContent = t.cover ? '(zorunlu)' : '(isteğe bağlı)';
    $('lvNReq').textContent = t.reason ? '(zorunlu, gerekçe)' : '(isteğe bağlı)';
    $('lvFReq').textContent = t.doc ? '(zorunlu' + (editing && editing.file ? ', mevcut belge korunur' : '') + ')' : '(isteğe bağlı)';
    var h = '';
    if (c.ok && c.days > 0) {
      h += '<div class="lv-c"><b>' + esc(durTxt(f.type, c.days)) + '</b><small>Süre</small></div>';
      if (c.ret && f.type !== 'saatlik') h += '<div class="lv-c"><b class="sm">' + esc(c.ret) + '</b><small>İşe dönüş</small></div>';
      if (f.type === 'yillik') { var left = S.me.balance - S.me.pending + (editing && editing.type === 'yillik' ? editing.days : 0) - c.days; h += '<div class="lv-c' + (left < 0 ? ' neg' : '') + '"><b>' + num(left) + ' gün</b><small>Talep sonrası kalan yıllık izin</small></div>'; }
    }
    $('lvCalc').innerHTML = h;
    $('lvCalcNote').textContent = c.hol.length ? 'Süreye sayılmayan resmi tatiller: ' + c.hol.join(', ') + '.' : t.unit === 'workday' ? 'Hafta sonları ve resmi tatiller süreye sayılmaz.' : t.unit === 'calendar' ? 'Rapor süresi takvim günüyle sayılır.' : '';
    // uyarilar
    var w = [], s0 = f.start, e0 = f.type === 'saatlik' ? f.start : c.end;
    if (s0 && e0 && e0 >= s0) {
      S.events.forEach(function (ev) { if (ev.d >= s0 && ev.d <= e0) w.push(['warn', '<b>Planlı takvim:</b> ' + esc(fDS(pd(ev.d))) + ', ' + esc(ev.t) + '. Onaylayıcı bu çakışmayı görecek.']); });
      S.cal.forEach(function (x) {
        if (x.who === S.me.id || (editing && x.id === editing.id) || x.e < s0 || x.s > e0) return;
        var q = person(x.who); if (!q) return; var when = fDS(pd(x.s)) + ' ile ' + fDS(pd(x.e)) + ' arası';
        if (f.vekil && x.who === f.vekil) w.push(['warn', '<b>Vekil izinde:</b> ' + esc(q.name) + ', ' + esc(when) + ' izinli. Başka bir vekil seçmeniz önerilir.']);
        else if (S.me.dept && q.dept === S.me.dept) w.push(['info', '<b>Ekipten izinli:</b> ' + esc(q.name) + ', ' + esc(when) + (x.status === 'pending' ? ' (onay bekliyor)' : '') + '.']);
      });
    }
    $('lvWarns').innerHTML = w.map(function (x) { return '<div class="lv-alert ' + x[0] + '">' + x[1] + '</div>'; }).join('');
    // onay yolu
    var ch = chain(f, c.days), r = stop('done', 'Talep eden', S.me.name, S.me.title);
    ch.steps.forEach(function (s, i) { r += stop('wait', (i + 1) + '. onay · ' + s.lb, pname(s.who), [s.why, s.orig ? pname(s.orig) + ' izinde, vekil onaylayıcıya gider' : ''].filter(Boolean).join('. '), s.orig ? 'warn' : ''); });
    if (ch.err) r += stop('rejected', 'Onaylayıcı yok', ch.err, '');
    if (f.type === 'hastalik' && S.settings.sickNoApproval) r += stop('info', 'Not', 'Sağlık raporu onay gerektirmez. Yönetici ve İK bilgilendirilir.', '');
    if (ch.info.length) r += stop('info', ch.steps.length ? 'Onaydan sonra bilgilendirilir' : 'Bilgilendirilir', ch.info.map(function (x) { return pname(x) + ' (' + role(f, x) + ')'; }).join(', '), '');
    $('lvRoutePrev').innerHTML = r;
  }
  function submit() {
    var f = form(), errs = [];
    if (!f.start) errs.push(f.type === 'saatlik' ? 'İzin tarihini seçin.' : 'Başlangıç tarihini seçin.');
    if (f.type !== 'saatlik' && f.type !== 'dogum' && !f.end) errs.push('Bitiş tarihini seçin.');
    if (!f.ok) errs.push('Bilgilerin doğruluğunu onaylayın.');
    if (errs.length) { showErrs(errs); return; }
    var fd = new FormData();
    var put = function (k, v) { if (v != null && v !== '') fd.append(k, v); };
    put('type', f.type); put('sub', f.sub); put('start', f.start); put('end', f.type === 'dogum' ? f.start : f.end); put('startHalf', f.sh ? '1' : '0'); put('endHalf', f.eh ? '1' : '0');
    put('hFrom', f.hf); put('hTo', f.ht); put('vekil', f.vekil); put('oncall', f.oncall); put('hand', $('lvHand').value.trim()); put('reach', f.reach); put('phone', f.phone);
    put('addr', $('lvAddr').value.trim()); put('note', f.note); put('ooo', $('lvOoo').checked ? '1' : '0'); if (editing) put('editId', editing.id); if (f.file) fd.append('file', f.file);
    var b = $('lvSubmit'); b.disabled = true; b.textContent = 'Gönderiliyor…';
    api('submit', fd, true).then(function (d) {
      b.disabled = false; b.textContent = editing ? 'Güncelle ve yeniden gönder' : 'Talebi gönder';
      if (!d.ok) { showErrs((d.error || 'Gönderilemedi.').split('\n')); return; }
      S = d.data; sel = S.result ? S.result.id : null; toast(S.message || 'Talep gönderildi.'); resetForm(); showTab('mine');
    }).catch(function () { b.disabled = false; b.textContent = 'Talebi gönder'; showErrs(['Bağlantı hatası, tekrar deneyin.']); });
  }
  function showErrs(l) { $('lvErrs').innerHTML = l.map(function (x) { return '<div class="lv-alert bad">' + esc(x) + '</div>'; }).join(''); $('lvErrs').scrollIntoView({ block: 'center', behavior: 'smooth' }); }
  function editReq(r) {
    resetForm(); editing = r;
    document.querySelector('input[name=lvType][value=' + r.type + ']').checked = true; if (r.sub) $('lvSub').value = r.sub;
    if (r.type === 'saatlik') { $('lvHD').value = r.start; $('lvHF').value = r.hFrom; $('lvHT').value = r.hTo; } else { $('lvStart').value = r.start; $('lvEnd').value = r.end; $('lvSH').checked = r.startHalf; $('lvEH').checked = r.endHalf; }
    $('lvVekil').value = r.delegate || ''; $('lvOc').value = r.onCallTo || ''; $('lvHand').value = r.handover || ''; document.querySelector('input[name=lvReach][value=' + r.reach + ']').checked = true;
    $('lvPhone').value = r.phone || ''; $('lvAddr').value = r.addr || ''; $('lvNote').value = r.note || ''; $('lvOoo').checked = r.ooo;
    $('lvSubmit').textContent = 'Güncelle ve yeniden gönder'; showTab('new'); refresh();
  }

  // ---------- listeler ----------
  function pill(r) {
    var m = { pending: ['pend', r.steps.length > 1 ? (r.step + 1) + '. onay bekleniyor' : 'Onay bekleniyor'], approved: ['ok', 'Onaylandı'], rejected: ['bad', 'Reddedildi'], revise: ['warn', 'Düzeltme istendi'], cancelled: ['mute', 'Geri çekildi'], recorded: ['info', 'Kaydedildi'] }[r.status];
    return '<span class="lv-pill ' + m[0] + '">' + m[1] + '</span>';
  }
  function head(r, who) { return '<div class="lv-ih"><h3>' + esc(who ? r.whoName : r.typeText) + '</h3>' + pill(r) + '<div class="lv-meta">' + esc(who ? r.typeText + ', ' : '') + esc(r.range) + ', ' + esc(r.dur) + ' · ' + esc(r.no) + '</div></div>'; }
  function details(r) {
    var h = '<dl class="lv-kv">' + (r.ret ? '<dt>İşe dönüş</dt><dd>' + esc(r.ret) + '</dd>' : '') +
      (r.balance != null && r.type === 'yillik' ? '<dt>Yıllık izin</dt><dd>Kalan ' + num(r.balance) + ' gün' + (r.status !== 'approved' ? ', onaylanırsa ' + num(r.balanceAfter) + ' gün' : '') + '</dd>' : '') +
      (r.delegateName ? '<dt>Vekil</dt><dd>' + esc(r.delegateName) + '</dd>' : '') + (r.onCallName ? '<dt>Nöbet devri</dt><dd>' + esc(r.onCallName) + '</dd>' : '') +
      '<dt>İzindeyken</dt><dd>' + esc(r.reachText) + '</dd>' + (r.addr ? '<dt>İzin adresi</dt><dd>' + esc(r.addr) + '</dd>' : '') + (r.note ? '<dt>Açıklama</dt><dd>' + esc(r.note) + '</dd>' : '') +
      (r.handover ? '<dt>Devir notu</dt><dd>' + esc(r.handover) + '</dd>' : '') + (r.file ? '<dt>Belge</dt><dd><a href="' + API + '?op=file&amp;id=' + r.id + '" target="_blank" rel="noopener">' + esc(r.fileName || 'Belgeyi aç') + '</a></dd>' : '') + '</dl>';
    return h + (r.warnings || []).map(function (x) { return '<div class="lv-alert ' + esc(x[0]) + '">' + esc(x[1]).replace(/^([^:]+:)/, '<b>$1</b>') + '</div>'; }).join('');
  }
  function renderMine() {
    var l = S.mine;
    $('lvMine').innerHTML = l.length ? l.map(function (r) {
      var a = '';
      if (r.status === 'pending' || r.status === 'revise') a += '<button class="btn-ghost lv-sm lv-bad" data-do="cancel" data-id="' + r.id + '">Talebi geri çek</button>';
      if (r.status === 'revise') a += '<button class="btn-cyan lv-sm" data-do="edit" data-id="' + r.id + '">Güncelle ve yeniden gönder</button>';
      a += '<button class="btn-ghost lv-sm" data-do="sel" data-id="' + r.id + '">Akışı göster</button>';
      return '<article class="lv-item' + (sel === r.id ? ' sel' : '') + '" id="lvr-' + r.id + '">' + head(r, false) + (r.reason ? '<div class="lv-alert ' + (r.status === 'rejected' ? 'bad' : 'warn') + '">“' + esc(r.reason) + '”</div>' : '') + '<div class="lv-acts">' + a + '</div></article>';
    }).join('') : '<div class="lv-empty">Henüz talep göndermediniz. “Yeni talep” sekmesinden başlayın.</div>';
  }
  function renderInbox() {
    var l = S.inbox;
    $('lvInbox').innerHTML = l.length ? l.map(function (r) {
      var s = r.steps[r.step] || {}, me = S.me.id, why = '';
      if (r.escalatedTo === me && s.who !== me) why = '<div class="lv-alert info">Bu talep ' + esc(s.whoName) + ' kişisinin ' + S.settings.escalate + ' saattir yanıtlamaması üzerine size yükseltildi.</div>';
      if (s.orig && s.who === me) why += '<div class="lv-alert info">' + esc(s.origName) + ' izinde; vekil onaylayıcı olarak size geldi.</div>';
      if (s.why) why += '<div class="lv-alert info">İkinci onay gerekçesi: ' + esc(s.why) + '.</div>';
      return '<article class="lv-item' + (sel === r.id ? ' sel' : '') + '" id="lvr-' + r.id + '">' + head(r, true) + why + details(r) +
        '<div class="lv-decide"><label class="lv-f"><span>Not ya da gerekçe <em>(reddetme ve düzeltme için zorunlu)</em></span><textarea class="input" id="lvdn-' + r.id + '" maxlength="400"></textarea></label>' +
        '<div class="lv-acts"><button class="btn-cyan lv-okb" data-do="approve" data-id="' + r.id + '">Onayla</button><button class="btn-ghost" data-do="revise" data-id="' + r.id + '">Düzeltme iste</button><button class="btn-ghost lv-bad" data-do="reject" data-id="' + r.id + '">Reddet</button></div><div class="lv-alerts" id="lvde-' + r.id + '"></div></div></article>';
    }).join('') : '<div class="lv-empty">Onayınızı bekleyen talep yok.</div>';
    var hd = S.handled || [];
    if (hd.length) $('lvInbox').innerHTML += '<h2 class="lv-h2" style="margin-top:18px">Son kararlarınız</h2>' + hd.map(function (r) {
      return '<article class="lv-item' + (sel === r.id ? ' sel' : '') + '" id="lvr-' + r.id + '">' + head(r, true) + '<div class="lv-acts"><button class="btn-ghost lv-sm" data-do="sel" data-id="' + r.id + '">Akışı göster</button></div></article>';
    }).join('');
  }
  function renderFlow() {
    var r = null; S.mine.concat(S.inbox, S.handled || []).forEach(function (x) { if (x.id === sel) r = x; });
    if (!r) { $('lvFlowSub').textContent = 'Bir talep seçtiğinizde ya da gönderdiğinizde akışı burada izlenir.'; $('lvRouteLive').innerHTML = ''; $('lvLog').hidden = true; return; }
    $('lvFlowSub').innerHTML = esc(r.no) + ' · ' + esc(r.whoName) + ', ' + esc(r.typeText) + ' ' + pill(r);
    var h = stop('done', 'Talep eden', r.whoName, 'Gönderildi, ' + r.created);
    r.steps.forEach(function (s, i) {
      var st = 'wait', ex = s.why || '';
      if (s.decision === 'approve') { st = 'done'; ex = 'Onayladı, ' + s.at + (s.byName && s.byName !== s.whoName ? ' (' + s.byName + ')' : ''); }
      else if (s.decision === 'reject') { st = 'rejected'; ex = 'Reddetti, ' + s.at; }
      else if (s.decision === 'revise' && r.status === 'revise') { st = 'active'; ex = 'Düzeltme istedi, ' + s.at; }
      else if (r.status === 'pending' && i === r.step) { st = 'active'; ex = 'Yanıt bekleniyor' + (r.escalatedTo ? '. ' + pname(r.escalatedTo) + ' kişisine yükseltildi' : ''); }
      else if (r.status === 'cancelled') { st = 'skip'; ex = 'Talep geri çekildi'; }
      if (s.orig && st !== 'done') ex = (ex ? ex + '. ' : '') + s.origName + ' izinde, vekil onaylayıcı';
      h += stop(st, (i + 1) + '. onay · ' + s.label, s.whoName, ex, s.orig ? 'warn' : '');
    });
    if (r.status === 'recorded') h += stop('info', 'Not', 'Sağlık raporu onay gerektirmez. Yönetici ve İK bilgilendirildi.', '');
    var info = [], st2 = S.settings, p = person(r.who); if (r.type === 'hastalik' && p && p.approver) info.push(p.approver); if (r.delegate) info.push(r.delegate); if (r.onCallTo) info.push(r.onCallTo); if (st2.hr) info.push(st2.hr);
    var seen = {}; info = info.filter(function (x) { var d = x === r.who || seen[x]; seen[x] = 1; return !d; });
    if (info.length) h += stop('info', r.steps.length ? 'Onaydan sonra bilgilendirilir' : 'Bilgilendirilir', info.map(function (x) { return pname(x) + ' (' + role(r, x) + ')'; }).join(', '), '');
    $('lvRouteLive').innerHTML = h;
    $('lvLog').hidden = !r.log || !r.log.length; $('lvLog').innerHTML = (r.log || []).map(function (l) { return '<li><span>' + esc(l.t) + '</span><b>' + esc(l.x) + '</b></li>'; }).join('');
  }

  // ---------- kurallar (yonetici / IK) ----------
  function sOpts(sel0, excl, empty) { return '<option value="">' + empty + '</option>' + S.staff.filter(function (p) { return p.id !== excl; }).map(function (p) { return '<option value="' + p.id + '"' + (p.id === sel0 ? ' selected' : '') + '>' + esc(p.name) + '</option>'; }).join(''); }
  function renderRules() {
    if (!S.isAdmin) return;
    var st = S.settings, h = '<h2 class="lv-h2">Personel ve onaylayıcılar</h2><p class="lv-hint">Onaylayıcısı boş bırakılan personelin talepleri İK sorumlusuna gider. Yıllık izin hakkı işe giriş tarihine göre hesaplanır (İş Kanunu 53. madde).</p>';
    h += '<div class="lv-tblw"><table class="lv-tbl"><thead><tr><th>Personel</th><th>Unvan ve departman</th><th>Onaylayıcı</th><th>Yıllık izin</th><th></th></tr></thead><tbody>';
    S.profiles.forEach(function (p) {
      h += '<tr><td><b>' + esc(p.name) + '</b><small>' + esc(p.email) + '</small></td><td>' + esc(p.title || '·') + '<small>' + esc(p.dept || 'Departman yok') + '</small></td>' +
        '<td>' + (p.noApproval ? 'Onay gerekmez' : p.approver ? esc(pname(p.approver)) : '<span class="lv-warnt">İK’ya gider</span>') + (p.delegateApprover ? '<small>İzindeyse: ' + esc(pname(p.delegateApprover)) + '</small>' : '') + '</td>' +
        '<td>' + (p.hire ? num(p.balance) + ' / ' + p.entitle + ' gün' : '<span class="lv-warnt">İşe giriş yok</span>') + (p.onCall ? '<small>Nöbet listesinde</small>' : '') + '</td>' +
        '<td><button class="btn-ghost lv-sm" data-do="prof" data-id="' + p.id + '">Düzenle</button></td></tr><tr class="lv-pe" id="lvpe-' + p.id + '" hidden><td colspan="5"></td></tr>';
    });
    h += '</tbody></table></div>';
    h += '<h2 class="lv-h2">Genel kurallar</h2><form class="lv-set" id="lvSet" data-noload="1">' +
      '<div class="lv-rule">Yıllık izin <input class="input" type="number" min="1" max="30" name="longDays" value="' + st.longDays + '" aria-label="İş günü sınırı"> iş gününü aşarsa ikinci onay: <select class="input" name="second" aria-label="İkinci onaylayıcı">' + sOpts(st.second, null, 'İkinci onay yok') + '</select></div>' +
      '<label class="lv-rule lv-chk"><input type="checkbox" name="unpaidSecond"' + (st.unpaidSecond ? ' checked' : '') + '> Ücretsiz izinde de ikinci onay alınsın</label>' +
      '<label class="lv-rule lv-chk"><input type="checkbox" name="sickNoApproval"' + (st.sickNoApproval ? ' checked' : '') + '> Sağlık raporu onay gerektirmesin, yönetici ve İK bilgilendirilsin</label>' +
      '<div class="lv-rule">İK sorumlusu (onaylanan izinler, belgeler, doğum izni onayı, onaylayıcısı olmayanlar): <select class="input" name="hr" aria-label="İK sorumlusu">' + sOpts(st.hr, null, 'Seçilmedi') + '</select></div>' +
      '<div class="lv-rule">Yanıt gelmezse <input class="input" type="number" min="1" max="168" name="remind" value="' + st.remind + '" aria-label="Hatırlatma saati"> saat sonra hatırlat, <input class="input" type="number" min="2" max="336" name="escalate" value="' + st.escalate + '" aria-label="Yükseltme saati"> saat sonra onaylayıcının yöneticisine yükselt</div>' +
      '<div class="lv-acts"><button class="btn-cyan" type="submit">Kuralları kaydet</button></div></form>';
    var hl = (S.holidayList || []).slice().sort(function (a, b) { return a.d < b.d ? -1 : 1; });
    h += '<h2 class="lv-h2">Resmi tatiller</h2><p class="lv-hint">Süre hesabında düşülür. Arifeler yarım gün işaretlenir. Dini bayram tarihleri her yıl açıklandığında ekleyin.</p><ul class="lv-chips">' +
      hl.map(function (x) { return '<li>' + esc(fDS(pd(x.d)) + ' ' + pd(x.d).getFullYear()) + ', ' + esc(x.n) + (x.half ? ' (yarım gün)' : '') + ' <button type="button" class="lv-x" data-do="holdel" data-day="' + x.d + '" aria-label="' + esc(x.n) + ' sil">×</button></li>'; }).join('') + '</ul>' +
      '<form class="lv-inline" id="lvHol" data-noload="1"><input class="input" type="date" name="day" aria-label="Tarih" required><input class="input" type="text" name="name" maxlength="120" placeholder="Tatil adı" aria-label="Tatil adı" required><label class="lv-chk"><input type="checkbox" name="half"> Yarım gün</label><button class="btn-ghost" type="submit">Tatil ekle</button></form>';
    h += '<h2 class="lv-h2">Sürüm ve sprint takvimi</h2><p class="lv-hint">Bu tarihlerle çakışan taleplerde talep sahibi ve onaylayıcı uyarılır.</p><ul class="lv-chips">' +
      S.events.map(function (x) { return '<li>' + esc(fDS(pd(x.d))) + ', ' + esc(x.t) + ' <button type="button" class="lv-x" data-do="evdel" data-ev="' + x.id + '" aria-label="' + esc(x.t) + ' sil">×</button></li>'; }).join('') + '</ul>' +
      '<form class="lv-inline" id="lvEv" data-noload="1"><input class="input" type="date" name="day" aria-label="Tarih" required><input class="input" type="text" name="title" maxlength="160" placeholder="Örn. River AI 5.7 sürümü" aria-label="Açıklama" required><button class="btn-ghost" type="submit">Takvime ekle</button></form>';
    $('lvRules').innerHTML = h;
  }
  function profForm(id) {
    var row = $('lvpe-' + id); if (!row) return; if (!row.hidden) { row.hidden = true; return; }
    var p = S.profiles.filter(function (x) { return x.id === id; })[0];
    row.querySelector('td').innerHTML = '<form class="lv-pf" data-noload="1" data-uid="' + id + '"><div class="lv-row3">' +
      '<label class="lv-f"><span>Unvan</span><input class="input" name="title" maxlength="120" value="' + esc(p.title || '') + '"></label><label class="lv-f"><span>Departman</span><input class="input" name="dept" maxlength="120" value="' + esc(p.dept || '') + '"></label>' +
      '<label class="lv-f"><span>İşe giriş tarihi</span><input class="input" type="date" name="hire" value="' + esc(p.hire) + '"></label>' +
      '<label class="lv-f"><span>Bu yıl sistem dışında kullanılan</span><input class="input" type="number" step="0.5" min="0" max="60" name="used" value="' + p.used + '"></label><label class="lv-f"><span>Devreden ya da ek gün</span><input class="input" type="number" step="0.5" min="0" max="60" name="extra" value="' + p.extra + '"></label>' +
      '<label class="lv-f"><span>Onaylayıcı</span><select class="input" name="approver">' + sOpts(p.approver, id, 'İK sorumlusu') + '</select></label>' +
      '<label class="lv-f"><span>Bu kişi izindeyse onayı veren</span><select class="input" name="delegate">' + sOpts(p.delegateApprover, id, 'Bekletilir') + '</select></label></div>' +
      '<div class="lv-row"><label class="lv-chk"><input type="checkbox" name="onCall"' + (p.onCall ? ' checked' : '') + '> Nöbet listesinde (izinde nöbet devri zorunlu)</label><label class="lv-chk"><input type="checkbox" name="noApproval"' + (p.noApproval ? ' checked' : '') + '> Talepleri onay gerektirmez (yalnız İK bilgilendirilir)</label></div>' +
      '<div class="lv-acts"><button class="btn-cyan" type="submit">Kaydet</button><button class="btn-ghost" type="button" data-do="prof" data-id="' + id + '">Vazgeç</button></div><div class="lv-alerts"></div></form>';
    row.hidden = false;
  }

  // ---------- gorunum ----------
  function showTab(t) {
    if (t === 'rules' && !S.isAdmin) t = 'new';
    tab = t;
    [].forEach.call(root.querySelectorAll('.lv-tabs button'), function (b) { b.setAttribute('aria-selected', b.getAttribute('data-tab') === t ? 'true' : 'false'); });
    [].forEach.call(root.querySelectorAll('.lv-view'), function (v) { v.hidden = v.getAttribute('data-view') !== t; });
    render();
  }
  function render() {
    if (!S) return;
    var me = S.me;
    $('lvMe').innerHTML = '<div class="lv-av">' + esc(me.name.split(' ').map(function (x) { return x.charAt(0); }).join('').slice(0, 2)) + '</div><div><div class="lv-nm">' + esc(me.name) + '</div><div class="lv-tt">' + esc([me.title, me.dept].filter(Boolean).join(', ')) +
      (me.noApproval ? '. Talepleriniz onay gerektirmez' : me.approverName ? '. Onaylayıcı: ' + esc(me.approverName) : '') + '</div></div>' +
      '<div class="lv-bal">' + (me.hasHire ? '<div><b>' + me.entitle + '</b><small>Yıllık hak (' + me.years + ' yıl kıdem)</small></div><div><b>' + num(me.balance) + '</b><small>Kalan</small></div>' + (me.pending ? '<div><b>' + num(me.pending) + '</b><small>Onay bekleyen</small></div>' : '') : '<div><small>İşe giriş tarihiniz tanımlı değil; yıllık izin hakkınızı İK tanımlayacak.</small></div>') + '</div>';
    fillSelects();
    $('lvcMine').textContent = S.mine.length; $('lvcMine').hidden = !S.mine.length; $('lvcInbox').textContent = S.inbox.length; $('lvcInbox').hidden = !S.inbox.length;
    $('lvt-rules').hidden = !S.isAdmin;
    renderMine(); renderInbox(); renderFlow(); if (tab === 'rules') renderRules(); refresh();
  }

  // ---------- olaylar ----------
  root.addEventListener('click', function (e) {
    var t = e.target.closest('[data-tab]'); if (t) { showTab(t.getAttribute('data-tab')); return; }
    var b = e.target.closest('[data-do]'); if (!b) return;
    var a = b.getAttribute('data-do'), id = +b.getAttribute('data-id');
    var find = function () { var r = null; S.mine.concat(S.inbox, S.handled || []).forEach(function (x) { if (x.id === id) r = x; }); return r; };
    if (a === 'sel') { sel = id; render(); $('lvFlowT').scrollIntoView({ block: 'nearest', behavior: 'smooth' }); return; }
    if (a === 'edit') { var r = find(); if (r) editReq(r); return; }
    if (a === 'cancel') { if (!confirm('Talep geri çekilsin mi? Onaylayıcı bilgilendirilir.')) return; api('cancel', { id: id }).then(load); return; }
    if (a === 'prof') { profForm(id); return; }
    if (a === 'holdel') { if (confirm('Bu tatil silinsin mi?')) api('holiday', { day: b.getAttribute('data-day'), del: '1' }).then(function (d) { if (load(d)) renderRules(); }); return; }
    if (a === 'evdel') { api('event', { eventId: b.getAttribute('data-ev'), del: '1' }).then(function (d) { if (load(d)) renderRules(); }); return; }
    if (a === 'approve' || a === 'reject' || a === 'revise') {
      var note = ($('lvdn-' + id) || {}).value || ''; note = note.trim();
      if ((a === 'reject' || a === 'revise') && note.length < 5) { $('lvde-' + id).innerHTML = '<div class="lv-alert bad">' + (a === 'reject' ? 'Reddetme' : 'Düzeltme isteği') + ' için gerekçe yazın; talep sahibine iletilir.</div>'; $('lvdn-' + id).focus(); return; }
      [].forEach.call(b.parentNode.querySelectorAll('button'), function (x) { x.disabled = true; });
      api('decide', { id: id, act: a, note: note }).then(function (d) { sel = id; if (!load(d)) [].forEach.call(b.parentNode.querySelectorAll('button'), function (x) { x.disabled = false; }); });
    }
  });
  root.addEventListener('submit', function (e) {
    var f = e.target; if (f.id === 'lvForm') return; e.preventDefault();
    var v = function (n) { var x = f.elements[n]; return x ? (x.type === 'checkbox' ? (x.checked ? '1' : '0') : x.value) : ''; };
    if (f.classList.contains('lv-pf')) {
      api('profile', { userId: f.getAttribute('data-uid'), title: v('title'), dept: v('dept'), hire: v('hire'), used: v('used'), extra: v('extra'), approver: v('approver'), delegate: v('delegate'), onCall: v('onCall'), noApproval: v('noApproval') })
        .then(function (d) { if (d.ok) { load(d); renderRules(); toast('Profil kaydedildi.'); } else f.querySelector('.lv-alerts').innerHTML = '<div class="lv-alert bad">' + esc(d.error) + '</div>'; });
    } else if (f.id === 'lvSet') {
      api('settings', { longDays: v('longDays'), second: v('second'), unpaidSecond: v('unpaidSecond'), sickNoApproval: v('sickNoApproval'), hr: v('hr'), remind: v('remind'), escalate: v('escalate') }).then(function (d) { if (load(d)) { renderRules(); toast('Kurallar kaydedildi.'); } });
    } else if (f.id === 'lvHol') {
      api('holiday', { day: v('day'), name: v('name'), half: v('half') }).then(function (d) { if (load(d)) renderRules(); });
    } else if (f.id === 'lvEv') {
      api('event', { day: v('day'), title: v('title') }).then(function (d) { if (load(d)) renderRules(); });
    }
  });

  // ---------- baslangic: ?tab=inbox&r=ID&do=approve (e-postadaki dugmeler) ----------
  buildForm();
  api('state').then(function (d) {
    if (!d.ok) { root.innerHTML = '<div class="flash-err">' + esc(d.error || 'İzin bilgileri yüklenemedi.') + '</div>'; return; }
    S = d.data;
    var qs = new URLSearchParams(location.search), qt = qs.get('tab'), qr = +qs.get('r') || null;
    if (qr) sel = qr;
    showTab(qt === 'mine' || qt === 'inbox' || qt === 'rules' ? qt : 'new');
    if (qr) {
      var el = $('lvr-' + qr);
      if (el) el.scrollIntoView({ block: 'center' });
      else if (qt === 'inbox') toast('Bu talep artık onayınızı beklemiyor.');
      if (el && qs.get('do') === 'approve' && confirm('Bu izin talebini onaylıyor musunuz?')) { var bt = el.querySelector('[data-do=approve]'); if (bt) bt.click(); }
    }
  }).catch(function () { root.innerHTML = '<div class="flash-err">İzin bilgileri yüklenemedi. Sayfayı yenileyin.</div>'; });
})();
