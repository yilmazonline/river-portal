<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Materials.aspx.cs" Inherits="MaterialsPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Satış Materyalleri</title>
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
    <div class="links"><%= Nav.Links("materials") %></div>
    <div class="right">
      <div class="lang">
        <button id="tr" data-lang="tr">TR</button>
        <button id="en" data-lang="en">EN</button>
      </div>
      <div class="user">
        <div class="who">
          <div class="nm"><%= Server.HtmlEncode(UserName) %></div>
          <div class="rl" data-i18n="user_role"><%= IsVendor ? "Vendor Yöneticisi" : "Partner Yöneticisi" %></div>
        </div>
        <div class="avatar"><%= Fmt.Initials(UserName) %></div>
        <div class="user-menu" id="userMenu">
          <div class="um-head"><div class="um-name"><%= Server.HtmlEncode(UserName) %></div></div>
          <a href="Logout.aspx" class="danger" data-i18n="logout">Çıkış Yap</a>
        </div>
      </div>
    </div>
  </nav>

  <% if (FlashMsg.Length > 0) { %><div class="flash-ok"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>

  <div class="page-head mat-head">
    <div>
      <h1 data-i18n="mat_h">Satış Materyal ve İçerikleri</h1>
      <% if (NewLayout) { %><p id="smIntro"><%= Server.HtmlEncode(Intro) %></p><% } else { %><p data-i18n="mat_sub">River AI sunumları, tanıtım dokümanları ve satış içerikleri. Tüm iş ortakları için.</p><% } %>
    </div>
    <% if (IsVendor && NewLayout) { %><button type="button" class="btn-ghost sm-admin-btn" id="smAdminBtn" aria-haspopup="dialog">İçerik Yönetimi</button><% } %>
    <% if (IsVendor) { %><button type="button" class="btn-cyan mat-add-btn" id="matAddBtn" aria-haspopup="dialog"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" aria-hidden="true"><path d="M12 5v14M5 12h14"/></svg><span data-i18n="mat_add_btn">Yeni Materyal</span></button><% } %>
  </div>

  <% if (IsVendor) { %>
  <div class="pw-modal" id="matModal" role="dialog" aria-modal="true" aria-labelledby="matModalT"><div class="pw-box mat-box"><button type="button" class="pw-x" id="matModalX" aria-label="Kapat">&times;</button>
    <h3 id="matModalT" data-i18n="mat_add">Yeni Materyal Yayınla</h3>
    <form method="post" action="AddSalesMaterial.aspx" enctype="multipart/form-data">
      <input class="input" name="titletr" data-i18n-ph="mat_ph_title" placeholder="Başlık (Türkçe)" required="required" />
      <input class="input" name="titleen" data-i18n-ph="mat_ph_titleen" placeholder="Title (English) opsiyonel" />
      <% if (NewLayout) { %><select class="input" name="categorygroup" required="required" aria-label="Kategori"><%= CatOptions %></select>
      <p class="sm-form-hint">Yeni kategori için İçerik Yönetimi › Kategoriler.</p>
      <% } else { %><input class="input" name="categorygroup" list="grpList" data-i18n-ph="mat_ph_group" placeholder="Kategori (ör. Sunumlar, Broşürler)" required="required" /><% } %>
      <div class="field" style="margin-top:2px">
        <label data-i18n="mat_file">Dosya (PDF, sunum, doküman)</label>
        <input class="input" type="file" name="file" accept=".pdf,.ppt,.pptx,.doc,.docx,.xls,.xlsx,.zip,.png,.jpg,.jpeg" />
      </div>
      <input class="input" name="url" data-i18n-ph="mat_ph_url" placeholder="veya harici bağlantı (https://...)" />
      <button class="btn-cyan" type="submit" style="width:100%;padding:11px;margin-top:12px" data-i18n="mat_publish">Yayınla</button>
    </form>
    <datalist id="grpList"><%= GroupSuggestions %></datalist>
    <p style="color:var(--muted);font-size:12px;margin-top:12px" data-i18n="mat_hint">Dosya yükleyin ya da harici bağlantı girin. Yüklenen dosyalar sunucudaki materials klasörüne kaydedilir.</p>
  </div></div>
  <% } %>

  <% if (NewLayout) { %>
  <div class="sm-bar">
    <span class="sm-count"><b id="smCount"><%= Count %></b> materyal</span>
    <label class="ec-search sm-search"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" aria-hidden="true"><circle cx="11" cy="11" r="7"/><path d="M20 20l-3.5-3.5"/></svg>
      <span class="sr-only">Materyallerde ara</span><input type="search" id="smQ" name="smq_river" placeholder="Materyallerde ara (başlık, özet, kategori)" autocomplete="off" autocorrect="off" autocapitalize="off" spellcheck="false" readonly="readonly" data-lpignore="true" data-form-type="other" data-1p-ignore="true" /></label>
  </div>
  <div class="sm-none" id="smNone" hidden>Aramanızla eşleşen materyal yok.</div>
  <div class="sm-toast" id="smToast" role="status" aria-live="polite" hidden></div>
  <% if (IsVendor) { %><div class="pw-modal sm-admin" id="smAdmin" role="dialog" aria-modal="true" aria-labelledby="smAdminT"><div class="pw-box sm-admin-box"><button type="button" class="pw-x" id="smAdminX" aria-label="Kapat">&times;</button>
    <h3 id="smAdminT">İçerik Yönetimi</h3>
    <div class="sm-tabs" role="tablist"><button type="button" role="tab" class="on" data-t="items">Materyaller</button><button type="button" role="tab" data-t="cats">Kategoriler</button><button type="button" role="tab" data-t="intro">Sayfa metni</button></div>
    <div class="sm-pane" data-p="items"></div><div class="sm-pane" data-p="cats" hidden></div><div class="sm-pane" data-p="intro" hidden></div>
    <p class="sm-err" id="smErr" role="alert"></p>
  </div></div><% } %>
  <% } %>

  <%= GroupsHtml %>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
</div>

<script src="js/common.js?v=186"></script>
<script src="js/materials.js?v=5"></script>
<% if (NewLayout) { %><script src="js/salesmats.js?v=2"></script><% if (IsVendor) { %><script src="js/smadmin.js?v=1"></script><% } %><% } %>
</body>
</html>
