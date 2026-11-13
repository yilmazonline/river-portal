<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ProjectRecordDetail.aspx.cs" Inherits="ProjectRecordDetailPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Kayıt Detayı</title>
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
    <div><h1 style="font-size:20px">Kayıt Detayı</h1>
    <p><b><%= Server.HtmlEncode(R.CompanyName) %></b> · <%= DtStr %></p></div>
    <a class="btn-ghost" href="ProjectRecords.aspx" style="padding:10px 16px">Listeye Dön</a>
  </div>

  <div class="cols" style="grid-template-columns:1fr 300px">
    <div class="card">
      <div class="section-h"><span>Hizmet İçeriği</span></div>
      <div style="padding:16px 24px 24px;white-space:pre-wrap;line-height:1.7;font-size:14px;color:var(--text)"><%= Server.HtmlEncode(R.Content) %></div>
    </div>
    <div>
      <div class="card">
        <div class="section-h"><span>Bilgiler</span></div>
        <div style="padding:6px 22px 18px">
          <div class="k">Firma</div><div style="margin:2px 0 12px;font-weight:600"><%= Server.HtmlEncode(R.CompanyName) %></div>
          <div class="k">Tür</div><div style="margin:2px 0 12px"><span class="st <%= R.Pool == "project" ? "invoiced" : "approved" %>"><%= R.Pool == "project" ? "Proje" : "Destek" %></span></div>
          <div class="k">Harcanan Süre</div><div style="margin:2px 0 12px;font-weight:700"><%= Server.HtmlEncode(SureStr) %></div>
          <div class="k">Düşüm</div><div style="margin:2px 0 12px;font-size:12.5px;color:var(--muted)"><%= Server.HtmlEncode(PoolStr) %></div>
          <div class="k">Gönderilen Kişiler</div><div style="margin:2px 0 12px;font-size:13px"><%= Server.HtmlEncode(string.IsNullOrEmpty(R.Recipients) ? "-" : R.Recipients) %></div>
          <div class="k">Oluşturan</div><div style="margin:2px 0 0;font-size:13px"><%= Server.HtmlEncode(R.CreatedByName) %></div>
        </div>
      </div>
    </div>
  </div>
</div>
</div>
<script src="js/common.js?v=186"></script>
</body>
</html>
