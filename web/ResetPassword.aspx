<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ResetPassword.aspx.cs" Inherits="ResetPasswordPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Şifreyi Yenile</title>
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
    <h1>Şifreyi Yenile</h1>
    <p><%= Server.HtmlEncode(TargetName) %> · <%= Server.HtmlEncode(TargetEmail) %> · <%= Server.HtmlEncode(TargetCompany) %></p>
  </div>

  <% if (FlashErr.Length > 0) { %><div class="flash-err"><%= Server.HtmlEncode(FlashErr) %></div><% } %>

  <div class="card" style="max-width:520px;padding:26px 28px">
    <form method="post" action="ResetPassword.aspx">
      <input type="hidden" name="id" value="<%= UserId %>" />
      <div class="field">
        <label>Yeni Parola</label>
        <input class="input" name="password" value="<%= Server.HtmlEncode(GenPassword) %>" required="required" />
      </div>
      <label style="display:flex;align-items:center;gap:10px;font-size:13.5px;color:var(--text);margin:6px 0 4px;cursor:pointer">
        <input type="checkbox" name="sendmail" value="1" />
        <span>Şifreyi ilgili mail adresine gönder</span>
      </label>
      <div style="display:flex;gap:10px;margin-top:10px">
        <button class="btn-cyan" type="submit" style="flex:1;padding:12px">Şifreyi Yenile</button>
        <a class="btn-ghost" href="Users.aspx" style="flex:1;text-align:center;padding:12px">Vazgeç</a>
      </div>
    </form>
    <p style="color:var(--muted);font-size:12px;margin-top:14px">
      Mail gönderimi için SMTP ayarları (web.config) dolu olmalıdır. Boşsa şifre yine güncellenir, mail atlanır.
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
