<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Tiers.aspx.cs" Inherits="TiersPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Partner Seviyeleri</title>
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
    <div class="links"><%= Nav.Links("tiers") %></div>
    <div class="right">
      <div class="lang">
        <button id="tr" data-lang="tr">TR</button>
        <button id="en" data-lang="en">EN</button>
      </div>
      <div class="user">
        <div class="who">
          <div class="nm"><%= Server.HtmlEncode(UserName) %></div>
          <div class="rl" data-i18n="user_role">Partner Yöneticisi</div>
        </div>
        <div class="avatar"><%= Fmt.Initials(UserName) %></div>
        <div class="user-menu" id="userMenu">
          <div class="um-head">
            <div class="um-name"><%= Server.HtmlEncode(UserName) %></div>
          </div>
          <a href="Logout.aspx" class="danger" data-i18n="logout">Çıkış Yap</a>
        </div>
      </div>
    </div>
  </nav>

  <div class="page-head">
    <h1 data-i18n="tiers_h">Partner Seviyeleri</h1>
    <p data-i18n="tiers_sub">Her seviyenin komisyon oranları, avantajları ve şartları tablodadır. Ciro arttıkça seviyeniz otomatik yükselir.</p>
  </div>

  <div class="tier-note-2027">
    Partner seviye ve hakediş sistemi 2027 yılında devreye alınacaktır. Satışlarınızı artırarak yeni döneme yüksek hak ediş seviyesi ile başlayabilirsiniz. Son bir yılda verdiğiniz siparişler listenizde yok veya eksik ise kanal satış temsilcinize bildiriniz.
    <span class="tier-note-sub">Partner seviyeniz ayda bir defa, son 1 yıldaki satışlarınız ölçeğinde güncellenir.</span>
  </div>
  <div class="tier-note-green">
    Ön ödeme ile seviyenizi yükselterek hak edişinizi yükseltebilirsiniz. Ön ödemeli seviye yükseltmelerinde yeni hak ediş seviyesi yükselme anlaşmasını takip eden 60. günden sonra bir yıl boyunca kullanılabilir. Seviye yükseltme şartları için satış temsilciniz ile iletişime geçin.
  </div>

  <div class="tier-grid"><%= TiersHtml %></div>

  <div style="text-align:center;margin-top:28px">
    <a href="Partner.aspx" style="color:var(--muted);font-size:13px;text-decoration:none" data-i18n="back_dash">← Panele dön</a>
  </div>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
</div>

<script src="js/common.js?v=186"></script>
<script src="js/tiers.js"></script>
</body>
</html>
