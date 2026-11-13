<%@ Page Language="C#" AutoEventWireup="true" CodeFile="KnowledgeBase.aspx.cs" Inherits="KnowledgeBasePage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Bilgi Tabanı</title>
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
    <div class="links"><%= Nav.Links("kb") %></div>
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
  <% if (SetupMsg.Length > 0) { %><div class="flash-err"><%= Server.HtmlEncode(SetupMsg) %></div><% } else { %>
  <div class="page-head"><div><h1>Bilgi Tabanı</h1><p>River Akademi eğitimlerinden çıkarılan konu kayıtları. Destek talebi formunda River AI seçen iş ortaklarına talep göndermeden önce bu kayıtlardan yapay zekâ ile ön yanıt verilir.</p></div></div>
  <div class="card" id="kb">
    <div class="lv-tabs" role="tablist" aria-label="Bilgi tabanı bölümleri">
      <button role="tab" aria-selected="true" data-tab="stats">Pano</button>
      <button role="tab" aria-selected="false" data-tab="entries">Kayıtlar <span class="lv-count" id="kbCount" hidden></span></button>
      <button role="tab" aria-selected="false" data-tab="import">İçe aktar</button>
    </div>
    <div class="lv-view" data-view="stats" id="kbStats"></div>
    <div class="lv-view" data-view="entries" id="kbEntries" hidden></div>
    <div class="lv-view" data-view="import" id="kbImport" hidden>
      <form id="kbImpForm" data-noload="1" class="kb-imp">
        <p>Bilgi tabanı dosyasını (<code>river-ai-bilgi-tabani.json</code>) seçin. Kayıtlar koduna göre eşleşir: yeni kodlar eklenir, var olanlar güncellenir; kapattığınız kayıtlar kapalı kalır.</p>
        <input type="file" name="file" accept=".json,application/json" required />
        <button type="submit" class="kb-btn ok">İçe aktar</button>
        <div id="kbImpMsg" role="status"></div>
      </form>
    </div>
  </div>
  <% } %>
</div>
</div>
<script src="js/kb-admin.js?v=1"></script>
<script src="js/common.js?v=186"></script>
</body>
</html>
