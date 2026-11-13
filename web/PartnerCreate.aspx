<%@ Page Language="C#" AutoEventWireup="true" CodeFile="PartnerCreate.aspx.cs" Inherits="PartnerCreatePage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Bayi Oluştur</title>
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
  </nav>

  <div class="page-head">
    <h1><%= IsCustomerType ? "Müşteri Oluştur" : "Bayi Oluştur" %></h1>
    <p><%= IsCustomerType ? "Bizden doğrudan destek alacak bir müşteri firması ekleyin." : "Yeni bir iş ortağı (bayi) firması ekleyin." %></p>
  </div>

  <% if (FlashErr.Length > 0) { %><div class="flash-err"><%= Server.HtmlEncode(FlashErr) %></div><% } %>

  <div class="card" style="max-width:520px;padding:26px 28px">
    <form method="post" action="PartnerCreate.aspx">
      <input type="hidden" name="ctype" value="<%= IsCustomerType ? "customer" : "partner" %>" />
      <div class="field">
        <label>Firma Adı</label>
        <input class="input" name="name" required="required" placeholder="Örn. Anadolu Yazılım A.Ş." />
      </div>
      <div class="field">
        <label>Vergi No (opsiyonel)</label>
        <input class="input" name="tax" placeholder="1234567890" />
      </div>
      <% if (!IsCustomerType) { %>
      <div class="field">
        <label>Başlangıç Kademesi</label>
        <select class="input st-select" name="tier"><%= TierOptionsHtml %></select>
      </div>
      <% } %>
      <div class="field">
        <label>Destek Süresi (dakika) <span style="color:var(--dim);font-weight:400">firma bu bakiye kadar destek talebi çözümü alır</span></label>
        <input class="input" type="number" name="supportmin" min="0" step="15" value="0" placeholder="Örn. 600" />
      </div>
      <div class="field">
        <label>Proje Süresi (saat) <span style="color:var(--dim);font-weight:400">proje kapsamı hizmetler bu havuzdan düşer (ondalıklı olabilir)</span></label>
        <input class="input" type="number" name="projhours" min="0" step="0.25" value="0" placeholder="Örn. 240" />
      </div>
      <div style="display:flex;gap:10px;margin-top:8px">
        <button class="btn-cyan" type="submit" style="flex:1;padding:12px"><%= IsCustomerType ? "Müşteriyi Oluştur" : "Bayiyi Oluştur" %></button>
        <a class="btn-ghost" href="Vendor.aspx" style="flex:1;text-align:center;padding:12px">Vazgeç</a>
      </div>
    </form>
    <p style="color:var(--muted);font-size:12px;margin-top:14px">Firmayı oluşturduktan sonra Kullanıcılar bölümünden ya da "Kullanıcı Oluştur" ile bu firmaya giriş yapacak kullanıcıyı ekleyin.</p>
  </div>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
<script src="js/common.js?v=186"></script>
</body>
</html>
