<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ResetPasswordLink.aspx.cs" Inherits="ResetPasswordLinkPage" %><!DOCTYPE html>
<html lang="tr">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Şifre Sıfırlama</title>
<link href="https://fonts.googleapis.com/css2?family=Sora:wght@400;600;700;800&family=Inter:wght@400;500;600&display=swap" rel="stylesheet" />
<link rel="stylesheet" href="css/site.css?v=170" />
<link rel="icon" type="image/svg+xml" href="assets/RIVER_AI_badge.svg" />
</head>
<body>
<div class="login-wrap">
  <div class="card login-card" style="max-width:440px">
    <div class="login-logo"><img class="login-river" src="assets/RIVER_AI_black.png" alt="River AI" /></div>
    <h1>Şifre Sıfırlama</h1>

    <% if (Done) { %>
      <div class="flash-ok" style="margin-top:10px">Parolanız güncellendi. Artık yeni parolanızla giriş yapabilirsiniz.</div>
      <a class="btn-cyan" href="Login.aspx" style="display:block;text-align:center;padding:12px;margin-top:16px;text-decoration:none">Giriş Yap</a>
    <% } else if (!Valid) { %>
      <div class="flash-err" style="margin-top:10px;line-height:1.6"><%= Server.HtmlEncode(Msg) %></div>
      <a class="btn-ghost" href="Login.aspx" style="display:block;text-align:center;padding:12px;margin-top:16px;text-decoration:none">Giriş Ekranına Dön</a>
    <% } else { %>
      <p class="sub" style="margin:2px 0 16px;color:var(--muted);text-align:center">Merhaba <b><%= Server.HtmlEncode(UserName) %></b>, yeni parolanızı belirleyin.</p>
      <% if (Msg.Length > 0) { %><div class="flash-err" style="margin-bottom:12px"><%= Server.HtmlEncode(Msg) %></div><% } %>
      <form method="post" action="ResetPasswordLink.aspx">
        <input type="hidden" name="token" value="<%= Server.HtmlEncode(Token) %>" />
        <div class="field"><label>Yeni Parola</label>
          <input class="input" type="password" name="pw" required="required" minlength="6" placeholder="En az 6 karakter" /></div>
        <div class="field"><label>Yeni Parola (Tekrar)</label>
          <input class="input" type="password" name="pw2" required="required" minlength="6" /></div>
        <button class="btn-cyan" type="submit" style="width:100%;padding:12px;margin-top:6px">Parolayı Güncelle</button>
      </form>
    <% } %>
  </div>
</div>
<script src="js/common.js?v=186"></script>
</body>
</html>
