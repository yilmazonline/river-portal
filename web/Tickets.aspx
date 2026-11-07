<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Tickets.aspx.cs" Inherits="TicketsPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · <%= IsSupport ? "Destek Kuyruğu" : "Taleplerim" %></title>
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
      <span class="sub">Asistan Portal</span>
    </div>
    <div class="links"><%= Nav.Links("support") %></div>
    <div class="right">
      <div class="user">
        <div class="who"><div class="nm"><%= Server.HtmlEncode(UserName) %></div><div class="rl"><%= Server.HtmlEncode(CompanyName) %></div></div>
        <div class="avatar"><%= Fmt.Initials(UserName) %></div>
        <div class="user-menu" id="userMenu">
          <div class="um-head"><div class="um-name"><%= Server.HtmlEncode(UserName) %></div></div>
          <a href="Logout.aspx" class="danger">Çıkış Yap</a>
        </div>
      </div>
    </div>
  </nav>

  <% if (FlashMsg.Length > 0) { %><div class="flash-ok"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>

  <div class="page-head" style="display:flex;justify-content:space-between;align-items:flex-end">
    <div>
      <h1><%= IsSupport ? "Destek Kuyruğu" : "Taleplerim" %></h1>
      <p><%= View == "resolved" ? "Çözülen/kapanan talepler" : "Açık talepler" %> · <%= Count %> kayıt</p>
    </div>
    <% if (!IsSupport) { %><a class="btn-cyan" href="NewTicket.aspx" style="padding:11px 20px;text-decoration:none">+ Yeni Talep</a><% } %>
    <% if (IsSupport) { %><a class="btn-cyan" href="ServiceForm.aspx" style="padding:11px 20px;text-decoration:none">+ Hizmet Formu</a><% } %>
  </div>

  <% if (IsSupport) { %>
  <div class="metrics" style="grid-template-columns:repeat(4,1fr);margin-bottom:18px">
    <div class="metric"><div class="k">Yeni</div><div class="v"><%= CNew %></div></div>
    <div class="metric"><div class="k">İnceleniyor</div><div class="v"><%= CProgress %></div></div>
    <div class="metric"><div class="k">Müşteride Bekliyor</div><div class="v"><%= CWaiting %></div></div>
    <div class="metric"><div class="k">Çözüldü</div><div class="v"><%= CResolved %></div></div>
  </div>
  <% } %>

  <% if (ShowBalance) { %>
  <div class="sup-balance">
    <div class="sb-ic">⏱</div>
    <div class="sb-txt">
      <div class="sb-lbl">Kalan Destek Süreniz</div>
      <div class="sb-val"><%= SupportMinutes %> <span>dk</span></div>
    </div>
    <div class="sb-until">Anlaşma son tarihi<br /><b><%= Server.HtmlEncode(SupportUntilStr) %></b></div>
  </div>
  <% } %>

  <div class="tk-tabs">
    <a href="Tickets.aspx?view=active" class="tk-tab<%= View == "active" ? " on" : "" %>">Aktif (<%= ActiveCount %>)</a>
    <a href="Tickets.aspx?view=resolved" class="tk-tab<%= View == "resolved" ? " on" : "" %>">Çözülenler (<%= ResolvedCount %>)</a>
  </div>

  <div class="tk-filters">
    <input type="search" id="tkSearch" name="tkq_river" placeholder="Ara: no, konu, firma, ürün, atanan…" autocomplete="off" autocorrect="off" autocapitalize="off" spellcheck="false" readonly="readonly" data-lpignore="true" data-form-type="other" data-1p-ignore="true" />
    <select id="tkStatus" class="st-select"><%= StatusFilterOptions %></select>
  </div>

  <div class="card">
    <div class="tbl-scroll">
    <table class="tk-table">
      <thead><tr>
        <th>No</th>
        <th>Konu</th>
        <% if (IsSupport) { %><th>Firma</th><% } %>
        <th>Ürün / Modül</th>
        <th>Tip</th>
        <th>Öncelik</th>
        <th>Durum</th>
        <% if (IsSupport) { %><th class="tk-age-h" title="<%= View == "resolved" ? "Talebin kaç günde çözüldüğü" : "Talebin açılışından bu yana geçen gün" %>"><%= View == "resolved" ? "Çözüm" : "Süre" %></th><% } %>
        <% if (IsSupport) { %><th>Atanan</th><% } %>
        <% if (View == "resolved") { %><th>Harcanan</th><% } %>
      </tr></thead>
      <tbody><%= Rows %></tbody>
    </table>
    </div>
    <div id="tkEmpty" style="display:none;padding:16px;color:var(--muted);font-size:13px">Aramanızla eşleşen talep yok.</div>
  </div>

  <script>
  (function () {
    var q = document.getElementById('tkSearch'), st = document.getElementById('tkStatus');
    if (!q) return;
    var freeSearch = function () { q.removeAttribute('readonly'); };
    q.addEventListener('focus', freeSearch);
    q.addEventListener('mousedown', freeSearch);
    q.addEventListener('touchstart', freeSearch);
    function apply() {
      var term = (q.value || '').toLowerCase().replace(/^\s+|\s+$/g, ''), sv = st ? st.value : '';
      var rows = document.querySelectorAll('tbody .tk-row'), shown = 0;
      for (var i = 0; i < rows.length; i++) {
        var r = rows[i];
        var okT = !term || (r.getAttribute('data-text') || '').indexOf(term) >= 0;
        var okS = !sv || r.getAttribute('data-status') === sv;
        var vis = okT && okS; r.style.display = vis ? '' : 'none'; if (vis) shown++;
      }
      var e = document.getElementById('tkEmpty'); if (e) e.style.display = shown ? 'none' : '';
    }
    q.addEventListener('input', apply);
    if (st) st.addEventListener('change', apply);
  })();
  </script>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
</div>

<script src="js/common.js?v=185"></script>
<script src="js/ticketstatus.js?v=2"></script>
</body>
</html>
