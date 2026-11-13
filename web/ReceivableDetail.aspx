<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ReceivableDetail.aspx.cs" Inherits="ReceivableDetailPage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Alacak <%= X != null ? (X.IsManual ? "(Manuel)" : "#" + X.SourceOrderId) : "" %></title>
<link href="https://fonts.googleapis.com/css2?family=Sora:wght@400;600;700;800&family=Inter:wght@400;500;600&display=swap" rel="stylesheet" />
<link rel="stylesheet" href="css/site.css?v=170" />
<link rel="icon" type="image/svg+xml" href="assets/RIVER_AI_badge.svg" />
</head>
<body>
<div id="dash" data-culture="<%= Culture %>">
<div class="wrap">
  <nav class="nav">
    <div class="brand">
      <a href="<%= Nav.HomeHref %>" aria-label="Ana Sayfa" style="display:inline-flex"><img class="brand-logo" src="assets/RIVER_AI_white.svg" alt="River AI" /></a>
      <span class="sub" data-i18n="brand_sub">Asistan Portal</span>
    </div>
    <div class="links"><%= Nav.Links("receivables") %></div>
    <div class="right">
      <div class="lang"><button id="tr" data-lang="tr">TR</button><button id="en" data-lang="en">EN</button></div>
      <div class="user">
        <div class="who"><div class="nm"><%= Server.HtmlEncode(UserName) %></div></div>
        <div class="avatar"><%= Fmt.Initials(UserName) %></div>
        <div class="user-menu" id="userMenu">
          <div class="um-head"><div class="um-name"><%= Server.HtmlEncode(UserName) %></div></div>
          <a href="Logout.aspx" class="danger" data-i18n="logout">Çıkış Yap</a>
        </div>
      </div>
    </div>
  </nav>

  <a class="rc-back" href="Receivables.aspx">‹ Alacaklar</a>
  <div class="page-head rc-head">
    <div>
      <div class="rc-kicker"><%= X.IsManual ? "MANUEL ALACAK KAYDI" : "TEKLİF SİPARİŞİ #" + X.SourceOrderId %> <%= X.Tags != null ? "· " + E(X.Tags) : "" %></div>
      <h1><%= X.CompanyName != null ? E(X.CompanyName) : "Cari seçilmedi" %></h1>
      <p><%= E(string.Join(" · ", new[] { X.PartnerName != null ? "Bayi: " + X.PartnerName : null, X.CustomerName != null ? "Müşteri: " + X.CustomerName : null, X.SalesRep != null ? "Satış: " + X.SalesRep : null }.Where2())) %></p>
    </div>
    <div class="rc-acts"><%= StatusChip() %></div>
  </div>

  <% if (X.Warning != null) { %><div class="rc-alert warn"><b>Kontrol edin:</b> <%= E(X.Warning) %></div><% } %>

  <% if (X.Status == "Unassigned") { %>
  <div class="card rc-card rc-assign">
    <h3>Alacaklı olduğumuz cariyi seçin</h3>
    <p>Fatura kime kesildiyse onu seçin. Seçtiğiniz anda alacak netleşir ve takibe girer.</p>
    <% if (SuggestHtml.Length > 0) { %><div class="rc-sugs"><span>Öneriler:</span><%= SuggestHtml %></div><% } %>
    <div class="rc-combo">
      <input type="search" id="rcCo" name="rc_company_search" aria-label="Cari ara" placeholder="Bayi ya da müşteri ara" autocomplete="off" readonly="readonly" data-lpignore="true" data-1p-ignore="true" data-bwignore="true" data-form-type="other" />
      <div class="rc-co-list" id="rcCoList"></div>
    </div>
  </div>
  <% } %>

  <div class="rc-sum">
    <div class="card rc-card rc-amt">
      <div class="rc-amt-row"><div><span>Alacak (KDV dahil)</span><b><%= M(X.GrossAmount) %></b><em><%= M(X.NetAmount) %> + KDV %<%= X.VatRate.ToString("0.##") %></em></div>
        <div><span>Tahsil edilen</span><b class="ok"><%= M(X.Paid) %></b></div><div><span>Kalan</span><b><%= M(X.Remaining) %></b></div></div>
      <div class="rc-prog"><i style="width:<%= X.GrossAmount > 0 ? Math.Min(100m, X.Paid / X.GrossAmount * 100m).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) : "0" %>%"></i></div>
      <div class="rc-meta"><label class="rc-chsel">Sorumlu: <select id="rcChSel" aria-label="Sorumlu"><option value=""<%= X.SalesChannel == null ? " selected" : "" %>>Belirtilmemiş</option><option<%= X.SalesChannel == "Kanal Satış" ? " selected" : "" %>>Kanal Satış</option><option<%= X.SalesChannel == "Direkt Satış" ? " selected" : "" %>>Direkt Satış</option></select></label><span>Sipariş: <b><%= D(X.OrderDate) %></b></span><span>Fatura: <b><%= D(X.InvoiceDate) %></b></span><% if (X.ProductSummary != null) { %><span>Ürün: <b><%= E(X.ProductSummary) %></b></span><% } %></div>
      <% if (X.IsOpen) { %>
      <div class="rc-btns">
        <button type="button" class="btn-cyan" data-open="mPay" <%= X.Status == "Unassigned" ? "disabled title=\"Önce cariyi seçin\"" : "" %>>Tahsilat ekle</button>
        <button type="button" class="btn-ghost" data-open="mSplit">Vadelere böl</button>
        <button type="button" class="btn-ghost" data-open="mRemind" <%= X.Status == "Unassigned" ? "disabled title=\"Önce cariyi seçin\"" : "" %>>Hatırlatma gönder</button>
        <button type="button" class="btn-ghost" data-open="mNote">Not ekle</button>
        <button type="button" class="btn-ghost eq-del" data-open="mClose">Kapat</button>
      </div>
      <% } else if (X.Status == "Closed") { %>
      <div class="rc-btns"><span class="rc-dim">Kapatma gerekçesi: <%= E(X.CloseReason) %></span><button type="button" class="btn-ghost" id="rcReopen">Yeniden aç</button></div>
      <% } %>
    </div>
    <% if (!string.IsNullOrEmpty(X.PaymentNote)) { %>
    <div class="card rc-card rc-note"><h3>Siparişteki ödeme notu</h3><p><%= E(X.PaymentNote).Replace("\n", "<br/>") %></p><small>Notlardaki tutarlar KDV dahil kabul edilir.</small></div>
    <% } %>
  </div>

  <div class="rc-grid2">
    <div class="card rc-card">
      <div class="rc-ch"><h3>Vadeler</h3><span><%= Inst.Count %> vade</span></div>
      <table class="tk-table rc-inst"><thead><tr><th>#</th><th>Vade</th><th class="rc-num">Tutar</th><th class="rc-num">Ödenen</th><th>Durum</th><th></th></tr></thead><tbody>
      <% int no = 0; foreach (RcvInstallment i in Inst) { no++; int dd = (int)(i.DueDate.Date - DateTime.Today).TotalDays; bool done = i.Left <= 0.005m; %>
        <tr><td><%= no %></td><td><%= i.DueDate.ToString("dd.MM.yyyy") %></td><td class="rc-num"><%= M(i.Amount) %></td><td class="rc-num"><%= M(i.PaidAmount) %></td>
          <td><%= done ? "<span class=\"rc-st ok\">Ödendi</span>" : (dd < 0 ? "<span class=\"rc-st bad\">" + (-dd) + " gün geçti</span>" : (dd == 0 ? "<span class=\"rc-st warn\">Bugün</span>" : "<span class=\"rc-st info\">" + dd + " gün kaldı</span>")) %></td>
          <td><% if (!done && X.IsOpen) { %><button type="button" class="rc-link" data-redate="<%= i.InstallmentId %>" data-due="<%= i.DueDate.ToString("yyyy-MM-dd") %>">Tarihi güncelle</button><% } %></td></tr>
      <% } %>
      </tbody></table>
    </div>
    <div class="card rc-card">
      <div class="rc-ch"><h3>Tahsilatlar</h3><span><%= Pays.Count %> kayıt</span></div>
      <% if (Pays.Count == 0) { %><div class="rc-empty">Henüz tahsilat girilmedi.</div><% } %>
      <% foreach (RcvPayment p in Pays) { %>
        <div class="rc-pay"><div><b><%= M(p.Amount) %></b><span><%= p.PayDate.ToString("dd.MM.yyyy") %><%= p.Method != null ? " · " + E(p.Method) : "" %><%= p.Bank != null ? " · " + E(p.Bank) : "" %></span><% if (p.Note != null) { %><em><%= E(p.Note) %></em><% } %></div><small><%= E(p.CreatedBy) %></small></div>
      <% } %>
    </div>
  </div>

  <div class="card rc-card">
    <div class="rc-ch"><h3>İşlem geçmişi</h3><span>Kayıtlar silinemez</span></div>
    <div class="rc-hist">
    <% foreach (RcvHistory h in Hist) { %>
      <div class="rc-h-it"><span class="rc-h-ic"><%= HistIcon(h.Action) %></span><div><p><%= E(h.Detail) %></p><small><%= h.CreatedAt.AddHours(3).ToString("dd.MM.yyyy HH:mm") %><%= h.UserName != null ? " · " + E(h.UserName) : " · Sistem" %></small></div></div>
    <% } %>
    </div>
  </div>
</div>
</div>

<!-- ======================= MODALLAR ======================= -->
<div class="va-modal" id="mPay" aria-hidden="true"><div class="va-mcard rc-m"><div class="va-mhd"><div><h3>Tahsilat ekle</h3><p>Kalan alacak: <%= M(X.Remaining) %></p></div><button type="button" class="va-mx va-mclose" aria-label="Kapat">&times;</button></div>
  <div class="va-mbody"><form data-noload="1" data-act="pay">
    <div class="rc-row2"><div class="field"><label>Tutar</label><input class="input" name="amount" inputmode="decimal" placeholder="Örn. 400.000,00" required="required" /></div>
      <div class="field"><label>Tahsilat tarihi</label><input class="input" type="date" name="date" value="<%= DateTime.Today.ToString("yyyy-MM-dd") %>" max="<%= DateTime.Today.ToString("yyyy-MM-dd") %>" required="required" /></div></div>
    <div class="rc-row2"><div class="field"><label>Ödeme yöntemi</label><select class="input" name="method"><option>Havale / EFT</option><option>Kredi Kartı</option><option>Çek</option><option>Senet</option><option>Nakit</option></select></div>
      <div class="field"><label>Banka (opsiyonel)</label><input class="input" name="bank" /></div></div>
    <div class="field"><label>Açıklama (opsiyonel)</label><input class="input" name="note" placeholder="Örn. 1. taksit, dekont no 4471" /></div>
    <div class="rc-fill"><button type="button" class="rc-link" data-fill="<%= X.Remaining.ToString("0.00", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) %>">Kalanın tamamını gir</button></div>
    <div class="va-mft"><button type="button" class="btn-ghost va-mclose">Vazgeç</button><button class="btn-cyan" type="submit">Tahsilatı kaydet</button></div>
  </form></div></div></div>

<div class="va-modal" id="mSplit" aria-hidden="true"><div class="va-mcard rc-m"><div class="va-mhd"><div><h3>Vadelere böl</h3><p>Kalan <%= M(X.Remaining) %> yeni vadelere bölünür. Ödenmiş kısım korunur.</p></div><button type="button" class="va-mx va-mclose" aria-label="Kapat">&times;</button></div>
  <div class="va-mbody"><form data-noload="1" data-act="split">
    <div class="rc-row3"><div class="field"><label>Vade sayısı</label><input class="input" type="number" name="count" min="1" max="36" value="3" required="required" /></div>
      <div class="field"><label>İlk vade</label><input class="input" type="date" name="first" required="required" /></div>
      <div class="field"><label>Aralık</label><select class="input" name="interval"><option value="month">Aylık</option><option value="week">Haftalık</option></select></div></div>
    <div class="field"><label>Tutarlar <span class="rc-dim">(boş bırakılırsa eşit bölünür; farklıysa her satıra bir tutar)</span></label><textarea class="input" name="amounts" rows="3" placeholder="400.000,00&#10;349.504,00&#10;349.504,00"></textarea></div>
    <div class="rc-preview" id="splitPrev"></div>
    <div class="va-mft"><button type="button" class="btn-ghost va-mclose">Vazgeç</button><button class="btn-cyan" type="submit">Vadeleri oluştur</button></div>
  </form></div></div></div>

<div class="va-modal" id="mRedate" aria-hidden="true"><div class="va-mcard rc-m"><div class="va-mhd"><div><h3>Tahsilat tarihini güncelle</h3><p>Eski tarih işlem geçmişinde saklanır.</p></div><button type="button" class="va-mx va-mclose" aria-label="Kapat">&times;</button></div>
  <div class="va-mbody"><form data-noload="1" data-act="redate"><input type="hidden" name="inst" />
    <div class="field"><label>Yeni tarih</label><input class="input" type="date" name="date" required="required" /></div>
    <div class="field"><label>Gerekçe</label><input class="input" name="reason" placeholder="Örn. Müşteri ay sonunda ödeyeceğini bildirdi" required="required" /></div>
    <div class="va-mft"><button type="button" class="btn-ghost va-mclose">Vazgeç</button><button class="btn-cyan" type="submit">Tarihi güncelle</button></div>
  </form></div></div></div>

<div class="va-modal" id="mNote" aria-hidden="true"><div class="va-mcard rc-m"><div class="va-mhd"><div><h3>Not ekle</h3></div><button type="button" class="va-mx va-mclose" aria-label="Kapat">&times;</button></div>
  <div class="va-mbody"><form data-noload="1" data-act="note"><div class="field"><textarea class="input" name="note" rows="4" placeholder="Örn. Muhasebe ile görüşüldü, ödeme talimatı verildi." required="required"></textarea></div>
    <div class="va-mft"><button type="button" class="btn-ghost va-mclose">Vazgeç</button><button class="btn-cyan" type="submit">Notu kaydet</button></div></form></div></div></div>

<div class="va-modal" id="mClose" aria-hidden="true"><div class="va-mcard rc-m"><div class="va-mhd"><div><h3>Alacağı kapat</h3><p>Kapatılan alacak takipten çıkar; gerekirse yeniden açılabilir.</p></div><button type="button" class="va-mx va-mclose" aria-label="Kapat">&times;</button></div>
  <div class="va-mbody"><form data-noload="1" data-act="close"><div class="field"><label>Gerekçe</label><input class="input" name="reason" placeholder="Örn. İndirim mahsubu, şüpheli alacak" required="required" /></div>
    <div class="va-mft"><button type="button" class="btn-ghost va-mclose">Vazgeç</button><button class="btn-cyan" type="submit">Kapat</button></div></form></div></div></div>

<div class="va-modal" id="mRemind" aria-hidden="true"><div class="va-mcard rc-m rc-m-wide"><div class="va-mhd"><div><h3>Hatırlatma gönder</h3><p id="remScen"></p></div><button type="button" class="va-mx va-mclose" aria-label="Kapat">&times;</button></div>
  <div class="va-mbody"><form data-noload="1" data-act="remind"><input type="hidden" name="inst" /><input type="hidden" name="to" />
    <div class="field"><label>Alıcılar</label><div class="rc-rcp" id="remList"></div>
      <input class="input" name="manual" placeholder="Ek adres yazın (virgülle ayırın)" autocomplete="off" style="margin-top:8px" /></div>
    <div class="field"><label>Konu</label><input class="input" name="subject" required="required" /></div>
    <div class="field"><label>Mesaj</label><textarea class="input" name="body" rows="10" required="required"></textarea></div>
    <div class="va-mft"><button type="button" class="btn-ghost va-mclose">Vazgeç</button><button class="btn-cyan" type="submit">Gönder</button></div>
  </form></div></div></div>

<div class="rc-toast" id="rcToast"></div>
<script src="js/common.js?v=186"></script>
<script>
(function () {
  var RID = <%= X.ReceivableId %>, COMPANIES = <%= CompaniesJson %>, REM = <%= ReminderJson %>, CONTACTS = <%= ContactsJson %>, REMAIN = <%= X.Remaining.ToString(System.Globalization.CultureInfo.InvariantCulture) %>;
  var toast = document.getElementById('rcToast');
  function say(msg, bad) { toast.textContent = msg; toast.className = 'rc-toast on' + (bad ? ' bad' : ''); clearTimeout(say.t); say.t = setTimeout(function () { toast.className = 'rc-toast'; }, 4200); }
  function post(data, done) {
    var fd = new FormData(); fd.append('id', RID); for (var k in data) fd.append(k, data[k]);
    fetch('ReceivableAction.ashx', { method: 'POST', body: fd, credentials: 'same-origin' })
      .then(function (r) { return r.json(); })
      .then(function (j) { if (j.ok) { if (done) done(j); else location.reload(); } else say(j.error || 'İşlem tamamlanamadı.', true); })
      .catch(function () { say('Bağlantı hatası, tekrar deneyin.', true); });
  }
  // ---- modallar
  var last = null;
  function open(id) { var m = document.getElementById(id); if (!m) return; last = document.activeElement; m.classList.add('open'); m.setAttribute('aria-hidden', 'false'); document.body.style.overflow = 'hidden'; setTimeout(function () { var f = m.querySelector('input:not([type=hidden]),textarea,select'); if (f) f.focus(); }, 60); }
  function closeAll() { [].forEach.call(document.querySelectorAll('.va-modal.open'), function (m) { m.classList.remove('open'); m.setAttribute('aria-hidden', 'true'); }); document.body.style.overflow = ''; if (last && last.focus) last.focus(); }
  [].forEach.call(document.querySelectorAll('[data-open]'), function (b) { b.addEventListener('click', function () { if (!b.disabled) open(b.getAttribute('data-open')); }); });
  [].forEach.call(document.querySelectorAll('.va-mclose'), function (b) { b.addEventListener('click', closeAll); });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape') closeAll(); });
  [].forEach.call(document.querySelectorAll('form[data-act]'), function (f) {
    f.addEventListener('submit', function (e) {
      e.preventDefault(); var d = { action: f.getAttribute('data-act') }; [].forEach.call(f.elements, function (el) { if (el.name) d[el.name] = el.value; });
      if (d.action === 'remind') {
        var to = [].slice.call(document.querySelectorAll('#remList input:checked')).map(function (c) { return c.value; });
        if (!to.length && !(d.manual || '').trim()) { say('En az bir alıcı seçin ya da yazın.', true); return; }
        d.to = to.join(','); var btn = f.querySelector('[type=submit]'); btn.disabled = true; btn.textContent = 'Gönderiliyor…';
        post(d, function (j) { say(j.msg); setTimeout(function () { location.reload(); }, 1200); }); setTimeout(function () { btn.disabled = false; btn.textContent = 'Gönder'; }, 4000); return;
      }
      post(d);
    });
  });
  var fill = document.querySelector('[data-fill]'); if (fill) fill.addEventListener('click', function () { document.querySelector('#mPay [name=amount]').value = fill.getAttribute('data-fill'); });
  [].forEach.call(document.querySelectorAll('[data-redate]'), function (b) { b.addEventListener('click', function () { var m = document.getElementById('mRedate'); m.querySelector('[name=inst]').value = b.getAttribute('data-redate'); m.querySelector('[name=date]').value = b.getAttribute('data-due'); open('mRedate'); }); });
  var chs = document.getElementById('rcChSel'); if (chs) chs.addEventListener('change', function () { post({ action: 'channel', channel: chs.value }, function () { say('Sorumlu kaydedildi: ' + (chs.value || 'Belirtilmemiş')); setTimeout(function () { location.reload(); }, 900); }); });
  var ro = document.getElementById('rcReopen'); if (ro) ro.addEventListener('click', function () { if (confirm('Alacak yeniden açılsın mı?')) post({ action: 'reopen' }); });
  // ---- vadelere bol: canli onizleme
  var sf = document.querySelector('form[data-act=split]');
  function fmt(v) { return v.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' TL'; }
  function prev() {
    var n = parseInt(sf.count.value, 10) || 0, f = sf.first.value, iv = sf.interval.value, raw = sf.amounts.value.trim(), out = '';
    if (!n || !f) { document.getElementById('splitPrev').innerHTML = ''; return; }
    var parts = [];
    if (raw) parts = raw.split(/\n|;/).filter(Boolean).map(function (s) { s = s.replace(/\s|TL/g, ''); if (s.indexOf(',') >= 0) s = s.replace(/\./g, '').replace(',', '.'); return parseFloat(s) || 0; });
    else { var each = Math.floor(REMAIN / n * 100) / 100; for (var i = 0; i < n; i++) parts.push(i < n - 1 ? each : Math.round((REMAIN - each * (n - 1)) * 100) / 100); }
    var d0 = new Date(f + 'T00:00:00'), sum = 0;
    for (var k = 0; k < parts.length; k++) { var d = new Date(d0); if (iv === 'week') d.setDate(d.getDate() + 7 * k); else d.setMonth(d.getMonth() + k); sum += parts[k]; out += '<div><span>' + (k + 1) + '. vade · ' + d.toLocaleDateString('tr-TR') + '</span><b>' + fmt(parts[k]) + '</b></div>'; }
    var ok = Math.abs(sum - REMAIN) <= 0.01 && parts.length === n;
    out += '<div class="rc-pv-t ' + (ok ? 'ok' : 'bad') + '"><span>Toplam</span><b>' + fmt(sum) + (ok ? '' : ' · kalan ' + fmt(REMAIN) + ' olmalı') + '</b></div>';
    document.getElementById('splitPrev').innerHTML = out;
  }
  if (sf) { ['input', 'change'].forEach(function (ev) { sf.addEventListener(ev, prev); }); }
  // ---- hatirlatma
  if (REM) {
    var m = document.getElementById('mRemind'), lab = { before: 'Vadesine ' + REM.days + ' gün kaldı', today: 'Vadesi bugün', overdue: 'Vadesi ' + (-REM.days) + ' gün önce doldu' };
    document.getElementById('remScen').textContent = 'Senaryo: ' + lab[REM.scenario] + '. Metni göndermeden önce düzenleyebilirsiniz.';
    m.querySelector('[name=subject]').value = REM.subject; m.querySelector('[name=body]').value = REM.body; m.querySelector('[name=inst]').value = REM.inst;
    var src = { portal: 'Portal kullanıcısı', order: 'Siparişin gönderildiği adres' }, h = '';
    CONTACTS.forEach(function (c, i) { h += '<label class="rc-chk"><input type="checkbox" value="' + c.e.replace(/"/g, '') + '"' + (c.s === 'portal' ? ' checked' : '') + ' /><span><b>' + (c.n || c.e) + '</b>' + (c.n ? ' · ' + c.e : '') + '<em>' + src[c.s] + '</em></span></label>'; });
    document.getElementById('remList').innerHTML = h || '<div class="rc-dim">Kayıtlı alıcı yok; adresi aşağıya yazın.</div>';
  }
  // ---- cari secimi
  var co = document.getElementById('rcCo');
  if (co) {
    var list = document.getElementById('rcCoList');
    ['focus', 'mousedown', 'touchstart', 'keydown'].forEach(function (ev) { co.addEventListener(ev, function () { co.removeAttribute('readonly'); }); });
    function fold(s) { return (s || '').replace(/İ/g, 'i').replace(/I/g, 'ı').toLowerCase().replace(/[çğıöşü]/g, function (c) { return { 'ç': 'c', 'ğ': 'g', 'ı': 'i', 'ö': 'o', 'ş': 's', 'ü': 'u' }[c]; }); }
    function render() {
      var q = fold(co.value.trim()); if (!q) { list.innerHTML = ''; return; }
      var hits = COMPANIES.filter(function (c) { return fold(c.n).indexOf(q) >= 0; }).slice(0, 12);
      list.innerHTML = hits.length ? hits.map(function (c) { return '<button type="button" data-id="' + c.id + '" data-n="' + c.n.replace(/"/g, '&quot;') + '">' + c.n.replace(/</g, '&lt;') + '<em>' + c.t + '</em></button>'; }).join('') : '<div class="rc-dim" style="padding:10px">Eşleşen firma yok.</div>';
    }
    co.addEventListener('input', render);
    function pick(b) { if (confirm('Alacaklı olunan cari: ' + b.getAttribute('data-n') + '\nAlacak bu cariyle netleşip takibe girecek. Onaylıyor musunuz?')) post({ action: 'assign', companyId: b.getAttribute('data-id') }); }
    list.addEventListener('click', function (e) { var b = e.target.closest('button[data-id]'); if (b) pick(b); });
    [].forEach.call(document.querySelectorAll('.rc-sug'), function (b) { b.addEventListener('click', function () { pick(b); }); });
  }
})();
</script>
</body>
</html>
