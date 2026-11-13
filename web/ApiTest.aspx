<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ApiTest.aspx.cs" Inherits="ApiTestPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · API Test</title>
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

  <div class="page-head" style="display:flex;justify-content:space-between;align-items:flex-start;gap:12px;flex-wrap:wrap">
    <div>
      <h1 style="font-size:20px">API Test Aracı</h1>
      <p>Gerçek uç noktaya (<code>https://portal.folyazilim.com/api/v1/tickets.ashx</code>) sunucudan, Cloudflare üzerinden bir destek talebi POST eder ve yanıtı gösterir — yani ürünlere vereceğiniz anahtarı <b>gerçek dış yolu</b> ile test edersiniz. Sunucu kendi genel adresine ulaşamazsa (hairpin) Hedef URL'i <code>http://localhost:88/api/v1/tickets.ashx</code> yapıp origin'i doğrudan sınayabilirsiniz.</p>
    </div>
    <a class="btn-ghost" href="ApiKeys.aspx" style="padding:10px 16px">API Anahtarları</a>
  </div>

  <div class="card">
    <div class="section-h"><span>Test Talebi Gönder</span></div>
    <div style="padding:6px 24px 24px">
      <form method="post" action="ApiTest.aspx" enctype="multipart/form-data">
        <div class="field">
          <label>API Anahtarı <span style="color:var(--dim);font-weight:400">(ApiKeys ekranında ürettiğiniz anahtarı yapıştırın)</span></label>
          <input class="input" name="key" required="required" placeholder="rvk_..." />
        </div>
        <div class="field">
          <label>Hedef URL</label>
          <input class="input" name="target" value="<%= Server.HtmlEncode(DefaultTarget) %>" />
        </div>
        <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px">
          <div class="field"><label>Ürün (product)</label><input class="input" name="product" value="Butce" /></div>
          <div class="field"><label>externalRef</label><input class="input" name="externalRef" value="<%= Server.HtmlEncode(DefaultRef) %>" /></div>
        </div>
        <div class="field"><label>Konu (subject)</label><input class="input" name="subject" value="TEST · Rapor ekranında toplam hatası" required="required" /></div>
        <div class="field"><label>Açıklama (description)</label><textarea class="input" name="description" style="min-height:90px;resize:vertical" required="required">TEST talebidir. Aylık rapor ekranında genel toplam yanlış hesaplanıyor.</textarea></div>
        <div class="field">
          <label>Öncelik</label>
          <select class="input st-select" name="priority"><option value="normal">Normal</option><option value="high">Yüksek</option><option value="low">Düşük</option></select>
        </div>
        <div class="section-h" style="margin-top:6px"><span>Talep Eden (requester)</span></div>
        <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px">
          <div class="field"><label>Firma</label><input class="input" name="rc" value="Anadolu Yazılım A.Ş." required="required" /></div>
          <div class="field"><label>Kişi</label><input class="input" name="rn" value="Ali Veli" required="required" /></div>
          <div class="field"><label>E-posta</label><input class="input" name="re" type="email" value="ali.veli@ornek.com" required="required" /></div>
          <div class="field"><label>Telefon <span style="color:var(--dim);font-weight:400">(opsiyonel)</span></label><input class="input" name="rp" value="" /></div>
        </div>
        <div class="field">
          <label>Ek Dosya <span style="color:var(--dim);font-weight:400">(opsiyonel, ≤10 MB)</span></label>
          <input class="input" type="file" name="file" />
        </div>
        <button class="btn-cyan" type="submit" style="padding:12px 26px;margin-top:6px">Test Talebini Gönder</button>
      </form>
    </div>
  </div>

  <%= ResultHtml %>

</div>
</div>
<script src="js/common.js?v=186"></script>
</body>
</html>
