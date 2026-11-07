<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ExamAttempt.aspx.cs" Inherits="ExamAttemptPage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Deneme Detayı</title>
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
    <div><h1 style="font-size:20px">Deneme Detayı</h1>
    <p><b><%= V(A.UserName) %></b> · <%= V(A.CompanyName) %> · <%= V(X.Title) %></p></div>
    <a class="btn-ghost" href="ExamEdit.aspx?id=<%= X.ExamId %>#results" style="padding:10px 16px">Sonuçlara Dön</a>
  </div>
  <div class="xt-banner">Bu sayfa yalnız eğitim editörlerine açıktır. Kullanıcı sınav sonrası soruları ve doğru/yanlış bilgisini göremez; gerekirse hatalarını siz iletebilirsiniz.</div>
  <div class="xa-sum">
    <div class="<%= A.Passed == true ? "ok" : "no" %>"><span>Puan</span><b><%= A.Score ?? 0 %> / 100</b><small><%= A.Passed == true ? "Geçti" : "Kaldı" %> · geçme <%= X.PassScore %></small></div>
    <div><span>Doğru</span><b style="color:#12A150"><%= CorrectCount %></b></div>
    <div><span>Yanlış</span><b style="color:#E4467A"><%= WrongCount %></b></div>
    <div><span>Boş</span><b><%= EmptyCount %></b></div>
    <div><span>Teslim</span><b style="font-size:14px"><%= DateStr %></b><small>Süre: <%= DurationStr %></small></div>
  </div>
  <%= QuestionsHtml %>
</div>
</div>
<script src="js/common.js?v=185"></script>
</body>
</html>
