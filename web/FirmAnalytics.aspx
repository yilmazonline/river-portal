<%@ Page Language="C#" AutoEventWireup="true" CodeFile="FirmAnalytics.aspx.cs" Inherits="FirmAnalyticsPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Firma Analizi</title>
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
    <div class="links"><%= Nav.Links("support") %></div>
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

  <div class="page-head" style="display:flex;justify-content:space-between;align-items:flex-start;gap:12px;flex-wrap:wrap">
    <div><h1 style="font-size:20px">Firma Analizi</h1>
    <p><b><%= Server.HtmlEncode(FirmName) %></b> · Destek bakiyesi: <%= Server.HtmlEncode(BalanceStr) %></p></div>
    <a class="btn-ghost" href="Vendor.aspx" style="padding:10px 16px">Geri</a>
  </div>

  <div class="fa-kpis">
    <div class="fa-kpi"><div class="fa-kpi-v"><%= TotalT %></div><div class="fa-kpi-k">Toplam Talep</div></div>
    <div class="fa-kpi"><div class="fa-kpi-v" style="color:#12A150"><%= ResolvedT %></div><div class="fa-kpi-k">Çözülen</div></div>
    <div class="fa-kpi"><div class="fa-kpi-v" style="color:#4F46E5"><%= OpenT %></div><div class="fa-kpi-k">Açık</div></div>
    <div class="fa-kpi"><div class="fa-kpi-v"><%= TotalMin %> <span style="font-size:14px">dk</span></div><div class="fa-kpi-k">Toplam Harcanan Süre</div></div>
  </div>

  <div class="cols" style="grid-template-columns:1fr 1fr;margin-top:6px">
    <div class="card"><div class="section-h"><span>Talep Durum Dağılımı</span></div>
      <div style="padding:14px 22px 20px"><%= StatusBars %></div>
    </div>
    <div class="card"><div class="section-h"><span>Öne Çıkanlar</span></div>
      <div style="padding:14px 22px 20px"><%= TopUsers %></div>
    </div>
  </div>

  <div class="card" style="margin-top:14px"><div class="section-h"><span>Kullanıcı Bazında</span></div>
    <div class="tbl-scroll"><table>
      <thead><tr><th>Kullanıcı</th><th>Talep Sayısı</th><th>Harcanan Süre</th></tr></thead>
      <tbody><%= UserTable %></tbody>
    </table></div>
  </div>

  <div class="card" style="margin-top:14px"><div class="section-h"><span>Son Çözülen Talepler</span></div>
    <div class="tbl-scroll"><table>
      <thead><tr><th>No</th><th>Konu</th><th>Tarih</th><th>Süre</th><th>Belge</th></tr></thead>
      <tbody><%= RecentRows %></tbody>
    </table></div>
  </div>
</div>
</div>
<script src="js/common.js?v=186"></script>
</body>
</html>
