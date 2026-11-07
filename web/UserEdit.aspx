<%@ Page Language="C#" AutoEventWireup="true" CodeFile="UserEdit.aspx.cs" Inherits="UserEditPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Kullanıcı Düzenle</title>
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
    <h1>Kullanıcı Düzenle</h1>
    <p><%= Server.HtmlEncode(Email) %> · <%= Server.HtmlEncode(CompanyName) %></p>
  </div>

  <% if (FlashErr.Length > 0) { %><div class="flash-err"><%= Server.HtmlEncode(FlashErr) %></div><% } %>

  <div class="card" style="max-width:520px;padding:26px 28px">
    <form method="post" action="UserEdit.aspx">
      <input type="hidden" name="id" value="<%= UserId %>" />
      <div class="field">
        <label>Ad Soyad</label>
        <input class="input" name="fullname" value="<%= Server.HtmlEncode(FullName) %>" required="required" />
      </div>
      <div class="field">
        <label>E-posta</label>
        <input class="input" value="<%= Server.HtmlEncode(Email) %>" readonly="readonly" style="opacity:.7" />
      </div>
      <div class="field">
        <label>Rol / Yetki</label>
        <% if (IsVendorUser) { %>
        <div id="permWrap">
          <label class="perm-chk"><input type="checkbox" name="perm_portal" value="1" <%= PermPortal ? "checked=\"checked\"" : "" %> /><span>Portal Yönetimi <em>bayi/kullanıcı/anlaşma/demo/materyal</em></span></label>
          <label class="perm-chk"><input type="checkbox" name="perm_editor" value="1" <%= PermEditor ? "checked=\"checked\"" : "" %> /><span>Eğitim Editörü <em>eğitim içeriği + versiyon duyuruları</em></span></label>
          <label class="perm-chk"><input type="checkbox" name="perm_support" value="1" <%= PermSupport ? "checked=\"checked\"" : "" %> /><span>Destek Personeli <em>destek kuyruğu</em></span></label>
          <label class="perm-chk"><input type="checkbox" name="perm_finance" value="1" <%= PermFinance ? "checked=\"checked\"" : "" %> /><span>Finans <em>alacak takibi</em></span></label>
          <label class="perm-chk"><input type="checkbox" name="perm_forum" value="1" <%= PermForum ? "checked=\"checked\"" : "" %> /><span>Forum Admini <em>forum moderasyonu: gizleme, silme, kısıtlama, kayıt</em></span></label>
        </div>
        <% } else { %>
        <select class="input st-select" name="role"><%= RoleOptions %></select>
        <% } %>
      </div>
      <div style="display:flex;gap:10px;margin-top:8px">
        <button class="btn-cyan" type="submit" style="flex:1;padding:12px">Kaydet</button>
        <a class="btn-ghost" href="Users.aspx" style="flex:1;text-align:center;padding:12px">Vazgeç</a>
      </div>
    </form>
    <p style="color:var(--muted);font-size:12px;margin-top:14px">Rol değişikliği kullanıcının erişebileceği bölümleri ve yetkileri belirler.</p>
  </div>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
<script src="js/common.js?v=185"></script>
</body>
</html>
