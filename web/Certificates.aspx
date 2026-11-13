<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Certificates.aspx.cs" Inherits="CertificatesPage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Sertifikalarım</title>
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
    <div class="links"><%= Nav.Links("edu") %></div>
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
    <div><h1 style="font-size:20px">Sertifikalarım</h1>
    <p>Kazandığınız sertifikalar QR kod ile herkes tarafından doğrulanabilir.</p></div>
    <div style="display:flex;gap:8px;flex-wrap:wrap">
      <a class="btn-cyan" href="Training.aspx" style="padding:10px 18px;text-decoration:none">Akademi · Rozetlerim</a>
    </div>
  </div>
  <% if (Count == 0) { %>
    <div class="card xt-center">
      <div class="xt-ic"><svg width="30" height="30" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="8" r="6"/><path d="M8.2 13.2 7 22l5-3 5 3-1.2-8.8"/></svg></div>
      <h2>Henüz bir sertifikaya sahip değilsiniz</h2>
      <p class="xt-note">Eğitimleri tamamlayıp sınavı geçtiğinizde sertifikalarınız burada listelenir. Kazanabileceğiniz rozetleri Akademi’deki “Rozetlerim” alanında görebilirsiniz.</p>
    </div>
  <% } else { %>
    <div class="ce-grid"><%= CardsHtml %></div>
  <% } %>
</div>
</div>
<script src="js/common.js?v=186"></script>
</body>
</html>
