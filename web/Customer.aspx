<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Customer.aspx.cs" Inherits="CustomerHomePage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · <%= Culture == "en" ? "Home" : "Ana Sayfa" %></title>
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
    <div class="links"><%= Nav.Links("home") %></div>
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

<div class="cu-top">
  <section class="card ph-hero cu-hero">
    <div class="ph-hero-glow" aria-hidden="true"></div>
    <div class="ph-hero-tx cu-left">
      <svg class="cu-wave" viewBox="0 0 800 300" preserveAspectRatio="none" aria-hidden="true"><path d="M0 220 C120 180 220 260 360 220 S600 160 800 200" fill="none" stroke="currentColor" stroke-width="1.2"/><path d="M0 250 C140 210 240 290 380 250 S620 190 800 230" fill="none" stroke="currentColor" stroke-width="1"/><path d="M0 280 C160 240 260 310 400 280 S640 220 800 260" fill="none" stroke="currentColor" stroke-width=".8"/></svg>
      <div class="pa-row"><%= AvatarHtml %><div class="pa-txt">
      <div class="ph-hero-top">
        <span class="ph-date"><%= Server.HtmlEncode(TodayText) %></span>
        <% if (BalanceText.Length > 0) { %><span class="cu-bal2"><svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" aria-hidden="true"><circle cx="12" cy="12" r="8.5"/><path d="M12 7.5V12l3 2"/></svg><span><%= Culture == "en" ? "Remaining support time" : "Kalan destek süresi" %></span><b><%= Server.HtmlEncode(BalanceText) %></b></span><% } %>
      </div>
      <h1><%= Server.HtmlEncode(Greeting) %></h1>
      <div class="cu-co"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 21V5a1 1 0 0 1 1-1h9a1 1 0 0 1 1 1v16M15 9h4a1 1 0 0 1 1 1v11M8 8h3M8 12h3M8 16h3M3 21h18"/></svg><span><%= Server.HtmlEncode(CompanyName) %></span></div>
      </div></div>
      <div class="cu-todo"><small><%= Culture == "en" ? "FOR TODAY" : "BUGÜN İÇİN" %></small><%= TodoHtml %></div>
      <div class="cu-cta">
        <a class="btn-cyan" href="NewTicket.aspx">+ <%= Culture == "en" ? "New Support Request" : "Destek Talebi Oluştur" %></a>
        <a class="btn-ghost" href="Training.aspx"><%= Culture == "en" ? "Go to River Academy" : "River Akademi'ye git" %></a>
      </div>
    </div>
  </section>
  <section class="card cu-whatsnew" aria-labelledby="cuNewT">
      <div class="cu-wn-h"><h2 id="cuNewT"><%= Culture == "en" ? "What's new?" : "Yeni neler var?" %></h2><% if (NewCount > 0) { %><span class="cu-wn-cnt"><%= NewCount %> <%= Culture == "en" ? "new" : "yeni" %></span><% } %></div>
      <div class="cu-wn-body"><%= NewsHtml %></div>
    </section>
</div>

  <section class="ph-kpis cu-kpis" aria-label="<%= Culture == "en" ? "Overview" : "Göstergeler" %>"><%= KpiHtml %></section>

  <% if (MidHtml.Length > 0) { %><section class="cu-mids" aria-label="<%= Culture == "en" ? "Highlights" : "Öne çıkanlar" %>"><%= MidHtml %></section><% } %>

  <div class="ph-grid cu-grid">
    <section class="card ph-flow">
      <div class="ph-h"><h2><%= Culture == "en" ? "My support requests" : "Destek taleplerim" %></h2><a href="Tickets.aspx"><%= Culture == "en" ? "See all" : "Tümünü gör" %></a></div>
      <div class="ph-list cu-list"><%= TicketsHtml %></div>
    </section>
    <section class="card ph-flow">
      <div class="ph-h"><h2><%= Culture == "en" ? "Your team in River Academy" : "Ekibiniz River Akademi'de" %></h2><a href="Training.aspx"><%= Culture == "en" ? "My teammates" : "Takım arkadaşlarım" %></a></div>
      <div class="ph-list cu-list"><%= TeamHtml %></div>
    </section>
  </div>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
</div>
<script src="js/common.js?v=185"></script>
</body>
</html>
