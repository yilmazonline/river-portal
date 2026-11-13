<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Vendor.aspx.cs" Inherits="VendorPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River Vendor Kontrol Merkezi</title>
<link href="https://fonts.googleapis.com/css2?family=Sora:wght@400;600;700;800&family=Inter:wght@400;500;600&display=swap" rel="stylesheet" />
<link rel="stylesheet" href="css/site.css?v=170" />
<link rel="icon" type="image/svg+xml" href="assets/RIVER_AI_badge.svg" />
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
</head>
<body>
<div id="dash" data-culture="<%= Culture %>">
<div class="wrap">

  <nav class="nav">
    <div class="brand">
      <a href="<%= Nav.HomeHref %>" aria-label="Ana Sayfa" style="display:inline-flex"><img class="brand-logo" src="assets/RIVER_AI_white.svg" alt="River AI" /></a>
      <span class="sub" data-i18n="brand_sub">Asistan Portal</span>
    </div>
    <div class="links"><%= Nav.Links("overview") %></div>
    <div class="right">
      <div class="lang">
        <button id="tr" data-lang="tr">TR</button>
        <button id="en" data-lang="en">EN</button>
      </div>
      <div class="user">
        <div class="who">
          <div class="nm"><%= Server.HtmlEncode(UserName) %></div>
          <div class="rl" data-i18n="user_role">Vendor Yöneticisi</div>
        </div>
        <div class="avatar"><%= Fmt.Initials(UserName) %></div>
        <div class="user-menu" id="userMenu">
          <div class="um-head">
            <div class="um-name"><%= Server.HtmlEncode(UserName) %></div>
            <div class="um-role"><%= Server.HtmlEncode(CompanyName) %></div>
          </div>
          <a href="Logout.aspx" class="danger" data-i18n="logout">Çıkış Yap</a>
        </div>
      </div>
    </div>
  </nav>

  <% if (FlashMsg.Length > 0) { %><div class="flash-ok"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>

  <div class="tym-row">
  <% if (Auth.IsVendorAdmin) { %>
  <%-- Teklif Yonetim Merkezi: Toplu Bildirim ile ayni kosul (Portal yetkisi ya da VendorAdmin rolu). Onceden yalniz rol adina bakiyordu. --%>
  <a class="tym-btn" href="https://teklif.folyazilim.com" target="_blank" rel="noopener">
    <span class="tym-ic"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><path d="M14 2v6h6"/><path d="M9 13h6M9 17h4"/></svg></span>
    <span class="tym-tx"><b>Teklif Yönetim Merkezi</b><small>teklif.folyazilim.com · yeni sekmede açılır</small></span>
    <span class="tym-arw">→</span>
  </a>
  <a class="tym-btn" href="Broadcast.aspx">
    <span class="tym-ic"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 11l18-8-8 18-2-8-8-2z"/></svg></span>
    <span class="tym-tx"><b>Toplu Bildirim</b><small>Bayi ve müşterilere uygulama bildirimi / e-posta</small></span>
    <span class="tym-arw">→</span>
  </a>
  <% } %>
  <%-- Ust menuden tasindi: Dosyalar ve API --%>
  <a class="tym-btn" href="Transfers.aspx">
    <span class="tym-ic"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><path d="M12 11v5M9.5 13.5L12 11l2.5 2.5"/></svg></span>
    <span class="tym-tx"><b>Dosyalar</b><small>Dosya paylaşımı ve transfer bağlantıları</small></span>
    <span class="tym-arw">→</span>
  </a>
  <a class="tym-btn" href="ApiKeys.aspx">
    <span class="tym-ic"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M8 9l-4 3 4 3M16 9l4 3-4 3M13.5 6l-3 12"/></svg></span>
    <span class="tym-tx"><b>API</b><small>API anahtarları ve entegrasyon</small></span>
    <span class="tym-arw">→</span>
  </a>
  </div>

  <div class="kpis">
    <div class="card kpi">
      <div class="k" data-i18n="kpi_partners">Toplam Partner</div>
      <div class="v"><%= TotalPartners %></div>
    </div>
    <div class="card kpi">
      <div class="k" data-i18n="kpi_active">Aktif Partner</div>
      <div class="v" style="background:linear-gradient(90deg,var(--ok),#7CF0D2);-webkit-background-clip:text;background-clip:text;color:transparent"><%= ActivePartners %></div>
    </div>
    <div class="card kpi">
      <div class="k" data-i18n="kpi_revenue">Toplam Ciro</div>
      <div class="v"><%= TotalRevenueM %><span style="font-size:15px;color:var(--muted)">M ₺</span></div>
    </div>
    <div class="card kpi">
      <div class="k" data-i18n="kpi_demos">Bekleyen Demo</div>
      <div class="v" style="background:var(--brand-grad);-webkit-background-clip:text;background-clip:text;color:transparent"><%= PendingDemos %></div>
    </div>
  </div>

  <div class="dash-section collapsed" id="dashSection" style="margin-top:26px">
    <div class="dash-head" id="dashToggle">
      <h2 style="font-family:var(--sora);font-weight:800;font-size:18px" data-i18n="dash_title">Dashboard</h2>
      <button type="button" class="nl-toggle" aria-label="Aç / Kapat">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"></polyline></svg>
      </button>
    </div>
    <div class="dash-body">
    <div class="charts-grid">
      <div class="card chart-card wide2">
        <div class="ch-title" data-i18n="ch_monthly">Aylık Anlaşma &amp; Ciro (M ₺)</div>
        <div class="ch-wrap"><canvas id="chMonthly"></canvas></div>
      </div>
      <div class="card chart-card">
        <div class="ch-title" data-i18n="ch_tiers">Kademe Dağılımı</div>
        <div class="ch-wrap"><canvas id="chTiers"></canvas></div>
      </div>
      <div class="card chart-card wide2">
        <div class="ch-title" data-i18n="ch_top">En Yüksek Cirolu Bayiler (M ₺)</div>
        <div class="ch-wrap"><canvas id="chTop"></canvas></div>
      </div>
      <div class="card chart-card">
        <div class="ch-title" data-i18n="ch_deal">Sipariş Durumları</div>
        <div class="ch-wrap"><canvas id="chDeal"></canvas></div>
      </div>
      <div class="card chart-card">
        <div class="ch-title" data-i18n="ch_demo">Demo Durumları</div>
        <div class="ch-wrap"><canvas id="chDemo"></canvas></div>
      </div>
    </div>
    </div>
  </div>

  <div class="card ig-panel" id="sec-partners">
    <div class="section-h" style="display:flex;justify-content:space-between;align-items:center;gap:12px;flex-wrap:wrap"><span>İş Ortakları (Bayiler)</span>
      <input type="search" class="input fc-search" data-grid="fc-grid-p" placeholder="Bayi ara..." autocomplete="off" readonly="readonly" data-lpignore="true" data-1p-ignore="true" data-form-type="other" style="max-width:240px" />
    </div>
    <p class="ig-hint">Bir bayiye tıklayın; ciro/kademe/durum ve o firmaya bağlı kullanıcılar görünür.</p>
    <div class="fc-grid" id="fc-grid-p"><%= PartnerTiles %></div>
    <div class="fc-details"><%= PartnerDetails %></div>
  </div>

  <div class="card ig-panel" style="margin-top:22px" id="sec-customers">
    <div class="section-h" style="display:flex;justify-content:space-between;align-items:center;gap:12px;flex-wrap:wrap"><span>Müşteriler</span>
      <input type="search" class="input fc-search" data-grid="fc-grid-c" placeholder="Müşteri ara..." autocomplete="off" readonly="readonly" data-lpignore="true" data-1p-ignore="true" data-form-type="other" style="max-width:240px" />
    </div>
    <p class="ig-hint">Bir müşteri firmaya tıklayın; kullanıcıları, rolleri ve destek planı görünür.</p>
    <div class="fc-grid" id="fc-grid-c"><%= CustomerTiles %></div>
    <div class="fc-details"><%= CustomerDetails %></div>
  </div>

  <div class="card" style="margin-top:22px" id="sec-demos">
    <div class="section-h"><span data-i18n="queue_title">Demo Onay Kuyruğu</span></div>
    <%= QueueHtml %>
  </div>

  <div class="card" style="margin-top:22px">
    <div class="section-h"><span data-i18n="regqueue_title">Müşteri Kayıt Onayları</span></div>
    <%= RegQueueHtml %>
  </div>

  <div class="card" id="sec-alldemos" style="margin-top:22px">
    <div class="section-h"><span data-i18n="alldemos_title">Tüm Demolar (geçmiş dahil)</span></div>
    <table>
      <thead><tr>
        <th data-i18n="th_company">Firma</th>
        <th data-i18n="th_client">Müşteri</th>
        <th data-i18n="th_module">Modül</th>
        <th data-i18n="th_dates">Tercih / Alternatifler</th>
        <th data-i18n="th_status">Durum</th>
      </tr></thead>
      <tbody><%= AllDemosHtml %></tbody>
    </table>
  </div>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
</div>

<script>
(function(){
  document.addEventListener('click', function(e){
    var close = e.target.closest ? e.target.closest('.fc-close') : null;
    if(close){ var dd=close.closest('.fc-detail'); if(dd){ dd.style.display='none'; var tt=document.querySelector('.fc-tile[data-det="'+dd.id+'"]'); if(tt)tt.classList.remove('active'); } return; }
    var tile = e.target.closest ? e.target.closest('.fc-tile') : null;
    if(!tile) return;
    var det=document.getElementById(tile.getAttribute('data-det'));
    var panel=tile.closest('.ig-panel');
    var wasActive=tile.classList.contains('active');
    if(panel){ var ds=panel.querySelectorAll('.fc-detail'); for(var i=0;i<ds.length;i++)ds[i].style.display='none';
      var ts=panel.querySelectorAll('.fc-tile'); for(var j=0;j<ts.length;j++)ts[j].classList.remove('active'); }
    if(!wasActive && det){ det.style.display='block'; tile.classList.add('active'); }
  });
  var ss=document.querySelectorAll('.fc-search');
  for(var k=0;k<ss.length;k++){ (function(s){
    var free=function(){ s.removeAttribute('readonly'); };
    s.addEventListener('focus',free); s.addEventListener('mousedown',free); s.addEventListener('touchstart',free);
    s.addEventListener('input', function(){
      var grid=document.getElementById(s.getAttribute('data-grid')); if(!grid)return;
      var q=(s.value||'').toLowerCase().replace(/^\s+|\s+$/g,'');
      var tiles=grid.querySelectorAll('.fc-tile');
      for(var m=0;m<tiles.length;m++){ var n=(tiles[m].getAttribute('data-name')||'').toLowerCase(); tiles[m].style.display=(!q||n.indexOf(q)>=0)?'':'none'; }
    });
  })(ss[k]); }
})();
</script>
<script src="js/common.js?v=186"></script>
<script>window.VDATA = <%= ChartsJson %>;</script>
<script src="js/vendor.js?v=3"></script>
</body>
</html>
