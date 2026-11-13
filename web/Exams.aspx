<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Exams.aspx.cs" Inherits="ExamsPage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Sınavlar</title>
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
    <div><h1 style="font-size:20px">Sınavlar</h1>
    <p>Eğitimleri tamamlayıp sınavı geçerek doğrulanabilir River sertifikası alın.</p></div>
    <div style="display:flex;gap:8px;flex-wrap:wrap">
      <a class="btn-ghost" href="Training.aspx" style="padding:10px 16px">Akademi</a>
      <% if (CanEdit) { %><a class="btn-cyan" href="ExamEdit.aspx" style="padding:10px 18px;text-decoration:none">+ Sınav Oluştur</a><% } %>
    </div>
  </div>
  <% if (FlashMsg.Length > 0) { %><div class="flash-ok" style="margin-bottom:14px"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>
  <% if (RequestsHtml.Length > 0) { %>
  <div class="card rq-card" id="requests">
    <div class="section-h"><span>Yeniden Giriş Talepleri</span><span class="rq-cnt"><%= PendingCount %> bekleyen</span></div>
    <p class="rq-hint">Deneme hakkını dolduran kullanıcılar yeniden girmek için onay bekliyor. Onaylarsanız kullanıcıya bir deneme hakkı daha tanımlanır; karar e-posta ile iletilir.</p>
    <div class="rq-list"><%= RequestsHtml %></div>
  </div>
  <% } %>
  <div class="ex-grid"><%= CardsHtml %></div>
</div>
</div>
<script src="js/common.js?v=186"></script>
</body>
</html>
