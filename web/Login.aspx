<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="LoginPage" %><!DOCTYPE html>
<html lang="tr">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>Asistan Portal · Giriş</title>
<link href="https://fonts.googleapis.com/css2?family=Sora:wght@400;600;700;800&family=Inter:wght@400;500;600&display=swap" rel="stylesheet" />
<link rel="stylesheet" href="css/site.css?v=170" />
<link rel="icon" type="image/svg+xml" href="assets/RIVER_AI_badge.svg" />
</head>
<body>
<div class="login-wrap">
  <div class="card login-card">
    <div class="login-logo"><img class="login-river" src="assets/RIVER_AI_black.png" alt="River AI" /></div>
    <h1>Asistan Portal</h1>

    <div class="login-top-toggles">
      <div class="theme-mini" id="loginTheme">
        <button type="button" data-theme-set="light"><span>Açık</span></button>
        <button type="button" data-theme-set="dark"><span>Koyu</span></button>
      </div>
      <div class="lang-mini">
        <label><input type="radio" name="culture" value="tr" checked="checked" /><span>TR</span></label>
        <label><input type="radio" name="culture" value="en" /><span>EN</span></label>
      </div>
    </div>

    <% if (ShowError) { %>
      <div class="login-err">E-posta veya parola hatalı.</div>
    <% } %>
    <% if (ShowForgotOk) { %>
      <div class="flash-ok" style="margin-bottom:14px;line-height:1.6">Eğer girdiğiniz e-posta adresi sistemde kayıtlıysa, şifre sıfırlama bağlantısı bu adrese gönderildi. Lütfen e-posta kutunuzu (ve gerekirse spam klasörünü) kontrol edin. Bağlantı 2 saat geçerlidir.</div>
    <% } %>

    <form method="post" action="Login.aspx">
      <div class="field">
        <label for="email">E-posta</label>
        <input id="email" name="email" type="email" autocomplete="username" required placeholder="ornek@firma.com" />
      </div>
      <div class="field">
        <label for="password">Parola</label>
        <input id="password" name="password" type="password" autocomplete="current-password" required placeholder="********" />
      </div>

      <button class="btn-login" type="submit">Giriş Yap</button>
    </form>

    <div style="text-align:center;margin-top:14px">
      <a href="#" id="forgotToggle" style="color:var(--muted);font-size:12.5px;text-decoration:none">Şifremi unuttum</a>
    </div>

    <div class="login-fol"><img src="assets/fol_dark.png" alt="Fol Yazılım" /></div>

    <form method="post" action="ForgotPassword.aspx" id="forgotForm" style="display:none;margin-top:12px">
      <div class="field">
        <label for="femail">Kayıtlı e-posta adresiniz</label>
        <input id="femail" name="email" type="email" required placeholder="ornek@firma.com" />
      </div>
      <button class="btn-login" type="submit">Şifre Sıfırlama Talebi Gönder</button>
    </form>
  </div>
</div>

<script>
  (function () {
    var t = document.getElementById('forgotToggle');
    var f = document.getElementById('forgotForm');
    if (t && f) t.addEventListener('click', function (e) {
      e.preventDefault();
      f.style.display = (f.style.display === 'none') ? 'block' : 'none';
    });
  })();
</script>
<script src="js/common.js?v=185"></script>
</body>
</html>
