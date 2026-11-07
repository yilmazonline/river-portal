<%@ Page Language="C#" AutoEventWireup="true" CodeFile="UserTraining.aspx.cs" Inherits="UserTrainingPage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Eğitim Geçmişi · <%= E(U != null ? U.FullName : "") %></title>
<link href="https://fonts.googleapis.com/css2?family=Sora:wght@400;600;700;800&family=Inter:wght@400;500;600&display=swap" rel="stylesheet" />
<link rel="stylesheet" href="css/site.css?v=170" />
<link rel="icon" type="image/svg+xml" href="assets/RIVER_AI_badge.svg" />
</head>
<body<%= Embed ? " class=\"ut-embed\"" : "" %>>
<div id="dash" data-culture="<%= Culture %>">
<div class="wrap">
  <% if (!Embed) { %>
  <nav class="nav">
    <div class="brand">
      <a href="<%= Nav.HomeHref %>" aria-label="Ana Sayfa" style="display:inline-flex"><img class="brand-logo" src="assets/RIVER_AI_white.svg" alt="River AI" /></a>
      <span class="sub" data-i18n="brand_sub">Asistan Portal</span>
    </div>
    <div class="links"><%= Nav.Links("users") %></div>
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
  <% } %>

  <% if (!Embed && Auth.IsVendorAdmin) { %><a class="rc-back" href="Users.aspx">‹ Kullanıcılar</a><% } %>
  <div class="ut-head card">
    <span class="uava ut-ava"><% if (!string.IsNullOrEmpty(U.AvatarImage)) { %><img src="<%= E(U.AvatarImage) %>" alt="" /><% } else { %><%= E(Fmt.Initials(U.FullName)) %><% } %></span>
    <div class="ut-who"><div class="rc-kicker">EĞİTİM GEÇMİŞİ</div><h1><%= E(U.FullName) %></h1><p><%= E(U.Email) %> · <%= E(U.CompanyName) %></p></div>
    <% if (!Embed && Auth.IsVendorAdmin) { %><a class="btn-ghost" href="UserEdit.aspx?id=<%= U.UserId %>" style="padding:10px 16px;text-decoration:none">Kullanıcıyı düzenle</a><% } %>
  </div>

  <div class="ut-stats">
    <div class="rc-kpi"><div class="rc-k">Tamamlanan eğitim</div><div class="rc-v"><%= StatCoursesDone %> / <%= StatCoursesTotal %></div></div>
    <div class="rc-kpi"><div class="rc-k">Tamamlanan video</div><div class="rc-v"><%= StatVideosDone %> / <%= StatVideosTotal %></div></div>
    <div class="rc-kpi"><div class="rc-k">İzleme oranı</div><div class="rc-v">%<%= StatRate %></div></div>
    <div class="rc-kpi ok"><div class="rc-k">Geçerli sertifika</div><div class="rc-v"><%= StatCerts %></div></div>
    <div class="rc-kpi"><div class="rc-k">Toplam gerçek izleme</div><div class="rc-v"><%= StatWatch %></div><div class="rc-h">Oynatırken ölçülen süre</div></div>
    <div class="rc-kpi"><div class="rc-k">Aktif gün</div><div class="rc-v"><%= StatActiveDays %></div><div class="rc-h">Video izlenen farklı gün</div></div>
    <div class="rc-kpi"><div class="rc-k">İlk etkinlik</div><div class="rc-v ut-sm"><%= StatFirst %></div></div>
    <div class="rc-kpi"><div class="rc-k">Son etkinlik</div><div class="rc-v ut-sm"><%= StatLast %></div></div>
  </div>

  <% if (RecentHtml.Length > 0) { %><div class="card rc-card"><div class="rc-ch"><h3>Son etkinlikler</h3><span>En son izlenen videolar</span></div><div class="ut-recent"><%= RecentHtml %></div></div><% } %>

  <div class="card rc-card"><div class="rc-ch"><h3>Eğitimler</h3><span>Kişinin kendi "Eğitim Geçmişim" ekranındaki bilgiler</span></div>
    <div class="gc-list ut-prog"><%= ProgressHtml %></div></div>

  <% if (PathHtml.Length > 0) { %><div class="card rc-card"><div class="rc-ch"><h3>Sıfırdan Uzmanlığa</h3><span>Önerilen eğitim yolu ve rozetler</span></div><%= PathHtml %></div><% } %>
  <div class="card rc-card"><div class="rc-ch"><h3>Sertifikalar</h3><span>Belgeyi açmak için tıklayın</span></div><%= CertHtml %></div>
  <div class="card rc-card"><div class="rc-ch"><h3>Sınav sonuçları</h3><span>Tüm denemeler, en yeni üstte</span></div><%= AttemptHtml %></div>
</div>
</div>
<script src="js/common.js?v=185"></script>
</body>
</html>
