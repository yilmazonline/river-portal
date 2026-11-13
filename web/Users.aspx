<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Users.aspx.cs" Inherits="UsersPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Kullanıcılar</title>
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
    <div class="links"><%= Nav.Links("users") %></div>
    <div class="right">
      <div class="lang">
        <button id="tr" data-lang="tr">TR</button>
        <button id="en" data-lang="en">EN</button>
      </div>
      <div class="user">
        <div class="who">
          <div class="nm"><%= Server.HtmlEncode(UserName) %></div>
          <div class="rl" data-i18n="user_role">Vendor Yöneticisi</div>
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

  <div class="page-head" style="display:flex;justify-content:space-between;align-items:flex-end;gap:16px;flex-wrap:wrap">
    <div>
      <h1 data-i18n="users_h">Kullanıcılar</h1>
      <p><%= Count %> <span data-i18n="users_sub">kullanıcı</span></p>
    </div>
    <div class="mgmt-bar" style="margin:0">
      <a class="btn-cyan" href="PartnerCreate.aspx">+ Bayi Oluştur</a>
      <a class="btn-cyan" href="PartnerCreate.aspx?type=customer">+ Müşteri Oluştur</a>
      <a class="btn-cyan" href="UserCreate.aspx">+ Kullanıcı Oluştur</a>
    </div>
  </div>

  <div class="tk-filters" style="margin-bottom:14px;flex-wrap:wrap;gap:10px">
    <input type="search" id="uSearch" name="uq_river" class="input" placeholder="Ara: ad, firma, rol…" autocomplete="off" autocorrect="off" autocapitalize="off" spellcheck="false" readonly="readonly" data-lpignore="true" data-1p-ignore="true" data-form-type="other" style="max-width:360px" />
    <div class="u-chips">
      <button type="button" class="u-chip active" data-ct="all">Tümü</button>
      <button type="button" class="u-chip" data-ct="customer">Müşteriler</button>
      <button type="button" class="u-chip" data-ct="partner">Bayiler</button>
      <button type="button" class="u-chip" data-ct="vendor">Fol Yazılım</button>
    </div>
    <span id="uCount" style="font-size:12.5px;color:var(--muted)"></span>
  </div>

  <div class="card">
    <div class="tbl-scroll">
    <table>
      <thead><tr>
        <th></th>
        <th data-i18n="th_name">Ad Soyad</th>
        <th data-i18n="th_email">E-posta</th>
        <th data-i18n="th_company">Firma</th>
        <th data-i18n="th_role">Rol</th>
        <th data-i18n="th_lastlogin">Son Giriş</th>
        <th></th>
      </tr></thead>
      <tbody><%= UsersHtml %></tbody>
    </table>
    </div>
  </div>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
</div>

<script>
(function () {
  var q = document.getElementById('uSearch'), cnt = document.getElementById('uCount');
  if (!q) return;
  var free = function () { q.removeAttribute('readonly'); };
  q.addEventListener('focus', free); q.addEventListener('mousedown', free); q.addEventListener('touchstart', free);
  var rows = document.querySelectorAll('.card tbody tr');
  var chips = document.querySelectorAll('.u-chip'), activeCt = 'all';
  function apply() {
    var v = (q.value || '').toLowerCase().replace(/^\s+|\s+$/g, ''), shown = 0;
    for (var i = 0; i < rows.length; i++) {
      var ct = rows[i].getAttribute('data-ctype') || 'other';
      var okCt = (activeCt === 'all') || (ct === activeCt);
      var okQ = !v || rows[i].textContent.toLowerCase().indexOf(v) >= 0;
      var m = okCt && okQ;
      rows[i].style.display = m ? '' : 'none'; if (m) shown++;
    }
    cnt.textContent = (v || activeCt !== 'all') ? (shown + ' sonuç') : '';
  }
  for (var c = 0; c < chips.length; c++) chips[c].addEventListener('click', function () {
    activeCt = this.getAttribute('data-ct');
    for (var k = 0; k < chips.length; k++) chips[k].classList.remove('active');
    this.classList.add('active'); apply();
  });
  q.addEventListener('input', apply);
})();
</script>
<script src="js/common.js?v=186"></script>
<script src="js/users.js?v=3"></script>
</body>
</html>
