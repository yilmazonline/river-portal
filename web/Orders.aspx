<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Orders.aspx.cs" Inherits="OrdersPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Siparişler</title>
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
    <div class="links"><%= Nav.Links("orders") %></div>
    <div class="right">
      <div class="lang">
        <button id="tr" data-lang="tr">TR</button>
        <button id="en" data-lang="en">EN</button>
      </div>
      <div class="user">
        <div class="who">
          <div class="nm"><%= Server.HtmlEncode(UserName) %></div>
          <div class="rl" data-i18n="user_role"><%= IsVendor ? "Vendor Yöneticisi" : "Partner Yöneticisi" %></div>
        </div>
        <div class="avatar"><%= Fmt.Initials(UserName) %></div>
        <div class="user-menu" id="userMenu">
          <div class="um-head"><div class="um-name"><%= Server.HtmlEncode(UserName) %></div></div>
          <a href="Logout.aspx" class="danger" data-i18n="logout">Çıkış Yap</a>
        </div>
      </div>
    </div>
  </nav>

  <% if (IsImpersonating) { %>
  <div class="imp-banner">
    <span><b><%= Server.HtmlEncode(CompanyName) %></b> <%= Culture == "en" ? "· acting on behalf of this partner (Vendor)." : "· bu partner adına işlem yapıyorsunuz (Vendor)." %></span>
    <a href="Vendor.aspx"><%= Culture == "en" ? "Back to Vendor panel" : "Vendor paneline dön" %></a>
  </div>
  <% } %>
  <% if (FlashMsg.Length > 0) { %><div class="flash-ok"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>
  <% if (FlashErr.Length > 0) { %><div class="flash-err"><%= Server.HtmlEncode(FlashErr) %></div><% } %>

  <div class="page-head">
    <h1 data-i18n="orders_h">Siparişler</h1>
    <p><%= Server.HtmlEncode(CompanyName) %> · <%= Culture == "en" ? "all orders" : "tüm siparişler" %> (<%= Count %>)</p>
  </div>

  <div class="card">
    <table>
      <thead><tr>
        <th data-i18n="th_customer">Müşteri Adı</th>
        <th data-i18n="th_title">Başlık</th>
        <th data-i18n="th_amount">Tutar</th>
        <th data-i18n="th_rate">Oran</th>
        <th data-i18n="th_comm">Hakediş</th>
        <th data-i18n="th_date">Tarih</th>
        <th data-i18n="th_status">Durum</th>
        <% if (IsVendor) { %><th data-i18n="th_actions" style="text-align:right">İşlemler</th><% } %>
      </tr></thead>
      <tbody><%= OrdersHtml %></tbody>
    </table>
  </div>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
</div>

<script src="js/common.js?v=186"></script>
<script src="js/orders.js"></script>
</body>
</html>
