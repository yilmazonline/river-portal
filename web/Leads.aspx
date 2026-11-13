<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Leads.aspx.cs" Inherits="LeadsPage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Lead Merkezi</title>
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
    <div class="links"><%= Nav.Links("leads") %></div>
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
  <div class="page-head ld-head"><div><h1>Lead Merkezi</h1><p>Potansiyel müşteri ve iş ortağı adaylarını toplayın, eleyin ve tanıtım e-postalarını onaya hazırlayın. Gönderim fol.solutions kutularından yapılır.</p></div></div>
  <div class="card ld" id="ld">
    <div class="lv-tabs" role="tablist" aria-label="Lead Merkezi bölümleri">
      <button role="tab" aria-selected="true" data-tab="overview">Genel bakış</button>
      <button role="tab" aria-selected="false" data-tab="pool">Aday havuzu</button>
      <button role="tab" aria-selected="false" data-tab="import">Aday ekle</button>
      <button role="tab" aria-selected="false" data-tab="queue">Onay kuyruğu <span class="lv-count" id="ldcQueue" hidden></span></button>
      <button role="tab" aria-selected="false" data-tab="replies">Yanıtlar <span class="lv-count" id="ldcRep" hidden></span></button>
      <button role="tab" aria-selected="false" data-tab="tpl">Şablonlar</button>
      <button role="tab" aria-selected="false" data-tab="block">Engelleme</button>
      <button role="tab" aria-selected="false" data-tab="settings">Ayarlar</button>
    </div>
    <div class="lv-view" data-view="overview" id="ldOv"></div>
    <div class="lv-view" data-view="pool" id="ldPool" hidden></div>
    <div class="lv-view" data-view="import" id="ldImport" hidden></div>
    <div class="lv-view" data-view="queue" id="ldQueue" hidden></div>
    <div class="lv-view" data-view="replies" id="ldRep" hidden></div>
    <div class="lv-view" data-view="tpl" id="ldTpl" hidden></div>
    <div class="lv-view" data-view="block" id="ldBlock" hidden></div>
    <div class="lv-view" data-view="settings" id="ldSet" hidden></div>
  </div>
  <div class="lv-toast" id="ldToast" role="status" aria-live="polite" hidden></div>
  <% } %>
</div>
</div>
<script src="js/leads.js?v=9"></script>
<script src="js/common.js?v=186"></script>
</body>
</html>
