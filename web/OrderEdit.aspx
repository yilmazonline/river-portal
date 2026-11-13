<%@ Page Language="C#" AutoEventWireup="true" CodeFile="OrderEdit.aspx.cs" Inherits="OrderEditPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Sipariş Düzenle</title>
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
    <h1>Sipariş Düzenle</h1>
    <p><%= Server.HtmlEncode(CompanyName) %></p>
  </div>

  <div class="card" style="max-width:520px;padding:26px 28px">
    <form method="post" action="OrderEdit.aspx">
      <input type="hidden" name="id" value="<%= DealId %>" />
      <div class="field">
        <label>Müşteri Adı</label>
        <input class="input" name="customer" value="<%= Server.HtmlEncode(Customer) %>" required="required" />
      </div>
      <div class="field">
        <label>Başlık</label>
        <input class="input" name="title" value="<%= Server.HtmlEncode(Title) %>" />
      </div>
      <div class="field">
        <label>Tutar (₺)</label>
        <input class="input" name="amount" type="text" inputmode="decimal" value="<%= AmountStr %>" required="required" />
      </div>
      <div class="field">
        <label>Hakediş (%)</label>
        <input class="input" name="rate" type="text" inputmode="decimal" value="<%= RateStr %>" required="required" />
      </div>
      <div class="field">
        <label>Hakediş (₺)</label>
        <input class="input" name="commission" type="text" inputmode="decimal" value="<%= CommissionStr %>" required="required" />
      </div>
      <div class="field">
        <label>Durum</label>
        <select class="input st-select" name="status"><%= StatusOptionsHtml %></select>
      </div>
      <div style="display:flex;gap:10px;margin-top:8px">
        <button class="btn-cyan" type="submit" style="flex:1;padding:12px">Kaydet</button>
        <a class="btn-ghost" href="Orders.aspx" style="flex:1;text-align:center;padding:12px">Vazgeç</a>
      </div>
    </form>
    <p style="color:var(--muted);font-size:12px;margin-top:14px">
      Not: Hakedişi elle girersiniz. Tutar, firmanın kademesini etkileyebilir.
    </p>
  </div>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
<script src="js/common.js?v=186"></script>
</body>
</html>
