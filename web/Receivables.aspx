<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Receivables.aspx.cs" Inherits="ReceivablesPage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Alacaklar</title>
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
  <% if (SetupMsg.Length > 0) { %><div class="flash-err"><%= Server.HtmlEncode(SetupMsg) %></div><% } %>
  <% if (SyncMsg.Length > 0) { %><div class="flash-ok"><%= Server.HtmlEncode(SyncMsg) %></div><% } %>
  <div class="page-head rc-head">
    <div>
      <h1>Alacak Takibi</h1>
      <p>River Teklif'te faturası gönderilen siparişler. Cariyi seçtiğinizde alacak netleşir ve takibe girer.</p>
    </div>
    <div class="rc-acts">
      <button type="button" class="btn-ghost ss-open-btn" id="ssOpenBtn" aria-haspopup="dialog" data-open="<%= SsOpen %>">Satış Önerileri<span class="ss-badge" id="ssBadge"<%= SsPending > 0 ? "" : " hidden" %>><%= SsPending %></span></button>
      <button type="button" class="btn-cyan rc-add-btn" id="rcAddBtn" aria-haspopup="dialog">+ Alacak Ekle</button>
      <a class="btn-ghost" href="Receivables.aspx?sync=1">Teklif'ten güncelle</a>
      <a class="btn-cyan" id="rcExport" href="ReceivableExport.ashx">Excel'e aktar</a>
    </div>
  </div>

  <% if (CntUnassigned > 0) { %><div class="rc-alert" id="rcAlertUn"><b><%= CntUnassigned %></b> alacakta cari seçimi bekleniyor. Cari seçilene kadar bu alacaklar takipte netleşmez. <button type="button" data-go="unassigned">Göster</button></div><% } %>

  <%= TotalHtml %>
  <div class="rc-kpis"><%= KpiHtml %></div>

  <div class="rc-grid2">
    <div class="card rc-card">
      <div class="rc-ch"><h3>Nakit akış beklentisi</h3><span>Gecikmiş + önümüzdeki 12 hafta</span></div>
      <div class="rc-flow"><%= FlowHtml %></div>
    </div>
    <div class="card rc-card">
      <div class="rc-ch"><h3>Gecikme yaşlandırması</h3><span>Vadesi geçmiş tutarlar</span></div>
      <div class="rc-ages"><%= AgingHtml %></div>
    </div>
  </div>

  <div class="card rc-card">
    <div class="rc-tools">
      <div class="rc-tabs" id="rcTabs">
        <button type="button" data-t="active" class="on">Açık takip <em><%= CntOpen + CntUnassigned %></em></button>
        <button type="button" data-t="unassigned">Cari bekleyen <em><%= CntUnassigned %></em></button>
        <button type="button" data-t="overdue">Geciken <em><%= CntOverdue %></em></button>
        <button type="button" data-t="week">Bu hafta <em><%= CntWeek %></em></button>
        <button type="button" data-t="paid">Tahsil edilen <em><%= CntPaid %></em></button>
        <button type="button" data-t="closed">Kapatılan <em><%= CntClosed %></em></button>
        <button type="button" data-t="all">Tümü <em><%= CntAll %></em></button>
      </div>
      <div class="rc-chf" id="rcCh"><button type="button" data-c="" class="on">Tüm sorumlular</button><button type="button" data-c="kanal">Kanal Satış</button><button type="button" data-c="direkt">Direkt Satış</button><button type="button" data-c="none">Belirtilmemiş</button></div>
      <div class="rc-q"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><circle cx="11" cy="11" r="7"/><path d="m20 20-3.5-3.5"/></svg>
        <input type="search" id="rcQ" name="rc_search" aria-label="Alacaklarda ara" placeholder="Sipariş no, cari, bayi, müşteri, ürün" autocomplete="off" readonly="readonly" data-lpignore="true" data-1p-ignore="true" data-bwignore="true" data-form-type="other" /></div>
    </div>
    <div class="rc-split" id="rcSplit"><div class="rc-mainc">
    <div class="tbl-scroll"><table class="tk-table rc-table">
      <thead><tr><th>Sipariş</th><th>Cari</th><th>Sorumlu</th><th class="rc-num">Alacak (KDV dahil)</th><th class="rc-num">Kalan</th><th id="rcDateTh">Sıradaki vade</th><th>Durum</th></tr></thead>
      <tbody id="rcRows"><%= RowsHtml %></tbody>
    </table></div>
    <div class="rc-none" id="rcNone" style="display:none">Bu görünümde alacak yok.</div>
    <nav class="va-pager" id="rcPager" aria-label="Sayfalar"></nav>
    </div>
    <aside class="rc-side" aria-label="Son tahsilatlar">
      <h3>Son tahsilatlar</h3>
      <div class="tbl-scroll"><table class="tk-table rc-stbl">
        <thead><tr><th>Nitelik</th><th>Tarih</th><th>Firma</th></tr></thead>
        <tbody><%= RecentHtml %></tbody>
      </table></div>
    </aside></div>
  </div>
</div>
</div>
<div class="pw-modal ss-modal" id="ssModal" role="dialog" aria-modal="true" aria-labelledby="ssModalT"><div class="pw-box ss-modal-box">
  <div class="ss-modal-h"><h3 id="ssModalT">Satış Önerileri</h3><button type="button" class="pw-x" id="ssModalX" aria-label="Kapat">&times;</button></div>
  <iframe id="ssFrame" title="Satış Önerileri"></iframe>
</div></div>
<script>
// Satis Onerileri penceresi: menuden tasindi; gomulu sayfa (SalesSuggestions.aspx?embed=1) iframe icinde
(function () {
  var md = document.getElementById('ssModal'), fr = document.getElementById('ssFrame'), btn = document.getElementById('ssOpenBtn'), badge = document.getElementById('ssBadge'), last = null;
  if (!md || !btn) return;
  function open(tab) {
    last = document.activeElement;
    fr.src = 'SalesSuggestions.aspx?embed=1' + (tab && tab !== 'pending' ? '&tab=' + tab : '');
    md.classList.add('open'); document.body.classList.add('tv-lock');
    document.getElementById('ssModalX').focus();
  }
  function close() { md.classList.remove('open'); document.body.classList.remove('tv-lock'); fr.src = 'about:blank'; if (last && last.focus) last.focus(); }
  btn.addEventListener('click', function () { open('pending'); });
  document.getElementById('ssModalX').addEventListener('click', close);
  md.addEventListener('click', function (e) { if (e.target === md) close(); });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && md.classList.contains('open')) close(); });
  // pencereden gelen bekleyen sayisi: rozet anlik guncellenir
  window.addEventListener('message', function (e) {
    if (e.origin !== location.origin || !e.data) return;
    if (e.data.type === 'ss-close') { if (md.classList.contains('open')) close(); return; }
    if (e.data.type !== 'ss-pending') return;
    var n = parseInt(e.data.pending, 10) || 0; badge.textContent = n; badge.hidden = n <= 0;
  });
  var auto = btn.getAttribute('data-open');
  if (auto) { open(auto); if (history.replaceState) history.replaceState(null, '', location.pathname); }
})();
</script>
<script src="js/common.js?v=186"></script>
<script>
(function () {
  var rows = [].slice.call(document.querySelectorAll('#rcRows tr[data-tab]')), q = document.getElementById('rcQ'), pager = document.getElementById('rcPager');
  var tab = 'active', ch = '', page = 1, PER = 25, timer = null;
  var free = function () { q.removeAttribute('readonly'); };
  ['focus', 'mousedown', 'touchstart', 'keydown'].forEach(function (ev) { q.addEventListener(ev, free); });
  setTimeout(function () { if (q.value) { q.value = ''; apply(); } }, 600);   // tarayici otomatik doldurduysa temizle
  function match(r) {
    var t = r.getAttribute('data-tab');
    if (tab === 'active') return t === 'open' || t === 'unassigned';
    if (tab === 'overdue') return r.getAttribute('data-od') === '1';
    if (tab === 'week') return r.getAttribute('data-wk') === '1';
    if (tab === 'all') return true;
    return t === tab;
  }
  function apply() {
    var s = (q.value || '').toLowerCase().trim().split(/\s+/).filter(Boolean), hits = [];
    rows.forEach(function (r) { var ok = match(r) && (!ch || r.getAttribute('data-ch') === ch) && s.every(function (w) { return r.getAttribute('data-s').indexOf(w) >= 0; }); r.style.display = 'none'; if (ok) hits.push(r); });
    var pages = Math.max(1, Math.ceil(hits.length / PER)); if (page > pages) page = pages;
    hits.forEach(function (r, k) { if (k >= (page - 1) * PER && k < page * PER) r.style.display = ''; });
    document.getElementById('rcNone').style.display = hits.length ? 'none' : '';
    var exp = document.getElementById('rcExport'); exp.href = 'ReceivableExport.ashx?tab=' + tab + (ch ? '&ch=' + ch : '');
    if (pages <= 1) { pager.innerHTML = ''; return; }
    var h = '<button type="button" data-p="' + (page - 1) + '"' + (page === 1 ? ' disabled' : '') + '>‹ Önceki</button>';
    for (var k = 1; k <= pages; k++) h += '<button type="button" data-p="' + k + '"' + (k === page ? ' class="on"' : '') + '>' + k + '</button>';
    pager.innerHTML = h + '<button type="button" data-p="' + (page + 1) + '"' + (page === pages ? ' disabled' : '') + '>Sonraki ›</button>';
  }
  function setTab(t) {
    tab = t; page = 1;
    // tarih sutunu: tahsil edilende son tahsilat, tumunde ikisi, digerlerinde siradaki vade
    document.getElementById('rcDateTh').textContent = t === 'paid' ? 'Tahsilat tarihi' : t === 'all' ? 'Vade · Tahsilat' : 'Sıradaki vade';
    document.getElementById('rcSplit').setAttribute('data-view', t);   // "Son tahsilatlar" yan tablosu yalniz tahsil edilende
    [].forEach.call(document.querySelectorAll('#rcTabs button'), function (b) { b.classList.toggle('on', b.getAttribute('data-t') === t); });
    apply();
  }
  document.getElementById('rcTabs').addEventListener('click', function (e) { var b = e.target.closest('button'); if (b) setTab(b.getAttribute('data-t')); });
  var al = document.querySelector('#rcAlertUn button'); if (al) al.addEventListener('click', function () { setTab('unassigned'); document.getElementById('rcTabs').scrollIntoView({ behavior: 'smooth', block: 'center' }); });
  pager.addEventListener('click', function (e) { var b = e.target.closest('button[data-p]'); if (!b || b.disabled) return; page = +b.getAttribute('data-p'); apply(); });
  document.getElementById('rcCh').addEventListener('click', function (e) { var b = e.target.closest('button'); if (!b) return; ch = b.getAttribute('data-c'); page = 1; [].forEach.call(this.querySelectorAll('button'), function (x) { x.classList.toggle('on', x === b); }); apply(); });
  q.addEventListener('input', function () { page = 1; clearTimeout(timer); timer = setTimeout(apply, 120); });
  apply();
})();
</script>

  <%-- Manuel alacak kaydi --%>
  <div class="pw-modal" id="rcAddModal" role="dialog" aria-modal="true" aria-labelledby="rcAddT"><div class="pw-box rc-add-box">
    <button type="button" class="pw-x" id="rcAddX" aria-label="Kapat">&times;</button>
    <h3 id="rcAddT">Alacak Ekle</h3>
    <p class="rc-add-sub">Teklif'te siparişi olmayan bir alacağı elle girin. Cari seçerseniz alacak hemen takibe girer.</p>
    <form id="rcAddForm" autocomplete="off" novalidate data-noload="1">
      <div class="field"><label for="rcaCompany">Alacaklı olunan cari</label><select class="input" id="rcaCompany" name="companyId"><%= CompanyOptionsHtml %></select></div>
      <div class="rc-add-2">
        <div class="field"><label for="rcaCustomer">Müşteri / unvan</label><input class="input" id="rcaCustomer" name="customer" maxlength="300" placeholder="Cari seçmediyseniz zorunlu" /></div>
        <div class="field"><label for="rcaPartner">İş ortağı <span class="opt">(opsiyonel)</span></label><input class="input" id="rcaPartner" name="partner" maxlength="300" /></div>
      </div>
      <div class="field"><label for="rcaProduct">Açıklama / ürün</label><input class="input" id="rcaProduct" name="product" maxlength="1000" required placeholder="Ör. River AI Enterprise · 50 kullanıcı · yıllık lisans" /></div>
      <div class="rc-add-2">
        <div class="field"><label for="rcaInv">Fatura tarihi</label><input class="input" type="date" id="rcaInv" name="invoiceDate" required /></div>
        <div class="field"><label for="rcaDue">Vade tarihi</label><input class="input" type="date" id="rcaDue" name="due" required /></div>
      </div>
      <div class="rc-add-3">
        <div class="field"><label for="rcaNet">Tutar (KDV hariç)</label><input class="input" id="rcaNet" name="net" inputmode="decimal" required placeholder="0,00" /></div>
        <div class="field"><label for="rcaVat">KDV</label><select class="input" id="rcaVat" name="vat"><option value="20">%20</option><option value="10">%10</option><option value="1">%1</option><option value="0">%0</option></select></div>
        <div class="field"><label for="rcaCur">Döviz</label><select class="input" id="rcaCur" name="currency"><option>TL</option><option>USD</option><option>EUR</option></select></div>
      </div>
      <div class="rc-add-gross" aria-live="polite">KDV dahil alacak: <b id="rcaGross">0,00 TL</b></div>
      <div class="rc-add-2">
        <div class="field"><label for="rcaCh">Sorumlu</label><select class="input" id="rcaCh" name="channel"><option value="">Belirtilmedi</option><option>Kanal Satış</option><option>Direkt Satış</option></select></div>
        <div class="field"><label for="rcaRep">Satış temsilcisi <span class="opt">(opsiyonel)</span></label><input class="input" id="rcaRep" name="rep" maxlength="150" /></div>
      </div>
      <div class="field"><label for="rcaNote">Not <span class="opt">(opsiyonel)</span></label><textarea class="input" id="rcaNote" name="note" rows="2" maxlength="2000" placeholder="Ödeme bilgisi, cari notu…"></textarea></div>
      <div class="rc-add-err" id="rcaErr" role="alert"></div>
      <div class="rc-add-act"><button type="button" class="btn-ghost" id="rcaCancel">Vazgeç</button><button type="submit" class="btn-cyan" id="rcaSave">Alacağı Kaydet</button></div>
    </form>
  </div></div>
  <script>
  (function () {
    var md = document.getElementById('rcAddModal'), btn = document.getElementById('rcAddBtn'), f = document.getElementById('rcAddForm'); if (!md || !btn) return;
    var $ = function (id) { return document.getElementById(id); }, last = null;
    function iso(d) { return d.getFullYear() + '-' + ('0' + (d.getMonth() + 1)).slice(-2) + '-' + ('0' + d.getDate()).slice(-2); }
    // sunucuyla ayni kural: '1.500' = 1500, '1.500,50' = 1500,5, '1500.5' = 1500,5
    function num(s) { s = (s || '').trim().replace(/\s/g, '').replace(/TL|₺/g, ''); if (s.indexOf(',') >= 0) s = s.replace(/\./g, '').replace(',', '.'); else if (/^\d{1,3}(\.\d{3})+$/.test(s)) s = s.replace(/\./g, ''); var v = parseFloat(s); return isNaN(v) ? 0 : v; }
    function calc() { var g = Math.round(num($('rcaNet').value) * (1 + num($('rcaVat').value) / 100) * 100) / 100; $('rcaGross').textContent = g.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ' + $('rcaCur').value; }
    function open() {
      last = document.activeElement; f.reset(); $('rcaErr').textContent = '';
      var d = new Date(); $('rcaInv').value = iso(d); d.setDate(d.getDate() + 30); $('rcaDue').value = iso(d);
      calc(); md.classList.add('open'); $('rcaCompany').focus();
    }
    function close() { md.classList.remove('open'); if (last && last.focus) last.focus(); }
    btn.addEventListener('click', open); $('rcAddX').addEventListener('click', close); $('rcaCancel').addEventListener('click', close);
    var down = false; md.addEventListener('pointerdown', function (e) { down = e.target === md; }); md.addEventListener('click', function (e) { if (down && e.target === md) close(); down = false; });
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && md.classList.contains('open')) close(); });
    ['rcaNet', 'rcaVat', 'rcaCur'].forEach(function (id) { $(id).addEventListener('input', calc); $(id).addEventListener('change', calc); });
    // cari secilince musteri adi bos ise doldur
    $('rcaCompany').addEventListener('change', function () { var o = this.options[this.selectedIndex]; if (this.value && !$('rcaCustomer').value.trim()) $('rcaCustomer').value = o.text; });
    // kisi bir alani duzeltince eski uyari kalkar
    f.addEventListener('input', function () { $('rcaErr').textContent = ''; }); f.addEventListener('change', function () { $('rcaErr').textContent = ''; });
    f.addEventListener('submit', function (e) {
      e.preventDefault(); var err = $('rcaErr'); err.textContent = '';
      if (num($('rcaNet').value) <= 0) { err.textContent = 'Geçerli bir tutar girin (KDV hariç).'; $('rcaNet').focus(); return; }
      if (!$('rcaProduct').value.trim()) { err.textContent = 'Açıklama / ürün bilgisini girin.'; $('rcaProduct').focus(); return; }
      if (!$('rcaCompany').value && !$('rcaCustomer').value.trim()) { err.textContent = 'Cari seçin ya da müşteri adını yazın.'; $('rcaCustomer').focus(); return; }
      if ($('rcaDue').value < $('rcaInv').value) { err.textContent = 'Vade tarihi fatura tarihinden önce olamaz.'; $('rcaDue').focus(); return; }
      var fd = new FormData(f); fd.append('action', 'create');   // tutar yazildigi gibi gider; sunucu ayni kuralla okur
      var s = $('rcaSave'); s.disabled = true; s.textContent = 'Kaydediliyor…';
      fetch('ReceivableAction.ashx', { method: 'POST', body: fd, credentials: 'same-origin' }).then(function (r) { return r.json(); }).then(function (d) {
        if (d && d.ok) { location.href = 'ReceivableDetail.aspx?id=' + d.id; return; }
        s.disabled = false; s.textContent = 'Alacağı Kaydet'; err.textContent = (d && d.error) || 'Kayıt tamamlanamadı.';
      }).catch(function () { s.disabled = false; s.textContent = 'Alacağı Kaydet'; err.textContent = 'Bağlantı hatası, tekrar deneyin.'; });
    });
  })();
  </script>
</body>
</html>
