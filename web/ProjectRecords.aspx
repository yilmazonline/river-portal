<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ProjectRecords.aspx.cs" Inherits="ProjectRecordsPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Proje Kayıtları</title>
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
    <div class="links"><%= Nav.Links("projects") %></div>
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
    <div><h1 style="font-size:20px">Proje Kayıtları</h1>
    <p>Talep açılmadan verilen proje/hizmetlerin firma bazlı ekstresi. Toplam <%= Count %> kayıt.</p></div>
    <a class="btn-cyan" href="ServiceForm.aspx" style="padding:10px 18px;text-decoration:none">+ Hizmet Formu</a>
  </div>

  <div class="tk-filters">
    <form method="get" action="ProjectRecords.aspx" style="display:flex;gap:10px;align-items:center;margin:0">
      <label style="font-size:12.5px;color:var(--muted)">Firma:</label>
      <select class="input st-select" name="cid" onchange="this.form.submit()"><%= CompanyOptions %></select>
    </form>
  </div>

  <div class="card">
    <div class="tbl-scroll">
    <table>
      <thead><tr>
        <th>Tarih</th><th>Firma</th><th>Tür</th><th>Süre</th><th>İçerik</th><th>Gönderilen</th>
      </tr></thead>
      <tbody><%= Rows %></tbody>
    </table>
    </div>
  </div>
</div>
</div>
<script src="js/common.js?v=186"></script>
</body>
</html>
