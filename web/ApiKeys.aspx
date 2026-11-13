<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ApiKeys.aspx.cs" Inherits="ApiKeysPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · API Anahtarları</title>
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
    <div class="links"><%= Nav.Links("api") %></div>
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

  <% if (FlashMsg.Length > 0) { %><div class="flash-ok"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>

  <div class="page-head" style="display:flex;justify-content:space-between;align-items:flex-start;gap:12px;flex-wrap:wrap">
    <div>
      <h1>Harici Destek API Anahtarları</h1>
      <p>Her ürün için bir anahtar üretin. Anahtar yalnızca üretildiği anda tam olarak gösterilir; sonra yalnız ön eki görünür.</p>
    </div>
    <a class="btn-ghost" href="ApiTest.aspx" style="padding:10px 16px">API Test Aracı</a>
  </div>

  <% if (NewKey.Length > 0) { %>
  <div class="note-amber" style="margin:0 0 18px">
    <b>Yeni anahtar üretildi — bu değeri şimdi kopyalayın, bir daha gösterilmeyecek:</b>
    <div style="margin-top:8px;font-family:monospace;font-size:14px;background:var(--glass-2);border:1px solid var(--stroke-2);border-radius:8px;padding:10px 12px;word-break:break-all"><%= Server.HtmlEncode(NewKey) %></div>
    <% if (NewClientId.Length > 0) { %><div style="margin-top:8px;font-size:12.5px">Client ID (gizli değil, referans): <b style="font-family:monospace"><%= Server.HtmlEncode(NewClientId) %></b></div><% } %>
  </div>
  <% } %>

  <div class="card" style="margin-bottom:18px">
    <div class="section-h"><span>Yeni Anahtar Üret</span></div>
    <div style="padding:6px 24px 20px">
      <form method="post" action="ApiKeys.aspx" style="display:flex;gap:10px;flex-wrap:wrap;align-items:flex-end">
        <input type="hidden" name="op" value="create" />
        <div class="field" style="flex:1;min-width:200px;margin:0">
          <label>Ürün Adı</label>
          <input class="input" name="name" placeholder="Örn. Bütçe" required="required" />
        </div>
        <div class="field" style="flex:1;min-width:220px;margin:0">
          <label>Firma <span style="color:var(--dim);font-weight:400">(destek süresi bu firmadan düşer)</span></label>
          <select class="input st-select" name="cid"><%= CompanyOptions %></select>
        </div>
        <button class="btn-cyan" type="submit" style="padding:11px 22px">Anahtar Üret</button>
      </form>
      <p style="color:var(--muted);font-size:12px;margin:12px 0 0">Anahtar bir firmaya bağlanır. O anahtarla gelen talepler firmanın destek bakiyesinden düşer; süre/tarih dolduğunda API isteği gerekçesiyle reddedilir. Firma seçilmezse anahtar eski davranışla (bakiyesiz, "Harici" havuz) çalışır.</p>
    </div>
  </div>

  <div class="card">
    <div class="section-h"><span>Tanımlı Anahtarlar</span></div>
    <div style="padding:4px 24px 20px"><%= ClientsHtml %></div>
  </div>

</div>
</div>
<script src="js/common.js?v=186"></script>
</body>
</html>
