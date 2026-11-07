<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TrainingManage.aspx.cs" Inherits="TrainingManagePage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · İçerik Yönetimi</title>
<link href="https://fonts.googleapis.com/css2?family=Sora:wght@400;600;700;800&family=Inter:wght@400;500;600&display=swap" rel="stylesheet" />
<link rel="stylesheet" href="css/site.css?v=170" />
<link rel="icon" type="image/svg+xml" href="assets/RIVER_AI_badge.svg" />
</head>
<body>
<div class="wrap">
  <nav class="nav">
    <div class="brand">
      <a href="<%= Nav.HomeHref %>" aria-label="Ana Sayfa" style="display:inline-flex"><img class="brand-logo" src="assets/RIVER_AI_white.svg" alt="River AI" /></a>
      <span class="sub">Asistan Portal</span>
    </div>
    <div class="links"><%= Nav.Links("edu") %></div>
  </nav>

  <% if (FlashMsg.Length > 0) { %><div class="flash-ok"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>

  <div class="page-head" style="display:flex;justify-content:space-between;align-items:flex-end">
    <div><h1>Eğitim İçerik Yönetimi</h1><p id="tmCount"><%= Count %> içerik</p></div>
    <a class="btn-ghost" href="Training.aspx" style="padding:10px 16px;text-decoration:none">River Akademi'ye Dön</a>
  </div>

  <div class="card">
    <div class="tm-bar" role="search">
      <input type="search" id="tmQ" class="input" placeholder="Başlık, kategori ya da dosya adında ara" aria-label="İçeriklerde ara" />
      <select id="tmType" class="input" aria-label="Tür"><option value="">Bütün türler</option><option value="video">Video</option><option value="doc">Doküman</option></select>
      <select id="tmGroup" class="input" aria-label="Kategori"><option value="">Bütün kategoriler</option><%= GroupOptions %></select>
      <select id="tmStatus" class="input" aria-label="Durum"><option value="">Bütün durumlar</option><option value="1">Yayında</option><option value="0">Kaldırıldı</option></select>
      <label class="tm-chk"><input type="checkbox" id="tmFeat" /> Yalnız öne çıkanlar</label>
      <button type="button" class="btn-ghost" id="tmClear" hidden>Filtreleri temizle</button>
    </div>
    <div class="tm-wrap">
    <table id="tmTable">
      <thead><tr>
        <th data-sort="title" aria-sort="none"><button type="button">Başlık</button></th><th data-sort="type" aria-sort="none"><button type="button">Tür</button></th>
        <th data-sort="cat" aria-sort="none"><button type="button">Kategori</button></th><th data-sort="active" aria-sort="none"><button type="button">Durum</button></th>
        <th data-sort="feat" aria-sort="none"><button type="button">Öne Çıkan</button></th><th></th>
      </tr></thead>
      <tbody><%= Rows %></tbody>
    </table>
    </div>
    <p class="tm-empty" id="tmEmpty" hidden>Aramanızla eşleşen içerik yok.</p>
  </div>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
<script src="js/training-manage.js?v=1"></script>
<script src="js/common.js?v=185"></script>
</body>
</html>
