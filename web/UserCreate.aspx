<%@ Page Language="C#" AutoEventWireup="true" CodeFile="UserCreate.aspx.cs" Inherits="UserCreatePage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Kullanıcı Oluştur</title>
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
    <h1>Kullanıcı Oluştur</h1>
    <p>Bayi, müşteri veya destek personeli kullanıcısı ekleyin. Giriş bilgileri e-posta ile gönderilir.</p>
  </div>

  <% if (FlashErr.Length > 0) { %><div class="flash-err"><%= Server.HtmlEncode(FlashErr) %></div><% } %>

  <div class="card" style="max-width:520px;padding:26px 28px">
    <form method="post" action="UserCreate.aspx">
      <div class="field">
        <label>Kullanıcı Tipi</label>
        <select class="input st-select" name="utype" id="utype">
          <option value="bayi">Bayi Kullanıcısı</option>
          <option value="musteri">Müşteri Kullanıcısı</option>
          <option value="uretici">Üretici Personeli</option>
        </select>
      </div>
      <div class="field" id="companyWrap">
        <label id="companyLabel">Bayi</label>
        <select class="input st-select" name="company" id="company"></select>
      </div>
      <div class="field">
        <label>Ad Soyad</label>
        <input class="input" name="fullname" required="required" placeholder="Örn. Ayşe Yılmaz" />
      </div>
      <div class="field">
        <label>E-posta</label>
        <input class="input" name="email" type="email" required="required" placeholder="kullanici@firma.com" />
      </div>
      <div class="field" id="roleWrap">
        <label>Rol</label>
        <select class="input st-select" name="role" id="role"></select>
      </div>
      <div class="field" id="permWrap" style="display:none">
        <label>Yetkiler (birden fazla seçilebilir)</label>
        <label class="perm-chk"><input type="checkbox" name="perm_portal" value="1" /><span>Portal Yönetimi <em>bayi/kullanıcı/anlaşma/demo/materyal</em></span></label>
        <label class="perm-chk"><input type="checkbox" name="perm_editor" value="1" /><span>Eğitim Editörü <em>eğitim içeriği + versiyon duyuruları</em></span></label>
        <label class="perm-chk"><input type="checkbox" name="perm_support" value="1" /><span>Destek Personeli <em>destek kuyruğu</em></span></label>
        <label class="perm-chk"><input type="checkbox" name="perm_finance" value="1" /><span>Finans <em>alacak takibi</em></span></label>
        <label class="perm-chk"><input type="checkbox" name="perm_forum" value="1" /><span>Forum Admini <em>forum moderasyonu: gizleme, silme, kısıtlama, kayıt</em></span></label>
      </div>
      <div class="field">
        <label>Geçici Parola</label>
        <input class="input" name="password" value="<%= Server.HtmlEncode(GenPassword) %>" required="required" />
      </div>
      <div class="field">
        <label>Kullanıcı Dili</label>
        <select class="input st-select" name="culture">
          <option value="tr">Türkçe</option>
          <option value="en">English</option>
        </select>
      </div>
      <div style="display:flex;gap:10px;margin-top:8px">
        <button class="btn-cyan" type="submit" style="flex:1;padding:12px">Kullanıcıyı Oluştur</button>
        <a class="btn-ghost" href="Vendor.aspx" style="flex:1;text-align:center;padding:12px">Vazgeç</a>
      </div>
    </form>
    <p style="color:var(--muted);font-size:12px;margin-top:14px" id="typeHint"></p>
  </div>

  <script>
    var PARTNERS = <%= PartnersJson %>;
    var CUSTOMERS = <%= CustomersJson %>;
    var ut = document.getElementById('utype');
    var companyWrap = document.getElementById('companyWrap');
    var companyLabel = document.getElementById('companyLabel');
    var company = document.getElementById('company');
    var role = document.getElementById('role');
    var hint = document.getElementById('typeHint');
    function fillCompany(list, empty){
      company.innerHTML = '';
      var o0 = document.createElement('option'); o0.value=''; o0.textContent = empty; company.appendChild(o0);
      list.forEach(function(c){ var o=document.createElement('option'); o.value=c.id; o.textContent=c.name; company.appendChild(o); });
    }
    function fillRole(pairs){
      role.innerHTML='';
      pairs.forEach(function(p){ var o=document.createElement('option'); o.value=p[0]; o.textContent=p[1]; role.appendChild(o); });
    }
    function apply(){
      var v = ut.value;
      document.getElementById('permWrap').style.display='none';
      document.getElementById('roleWrap').style.display='';
      if(v==='musteri'){
        companyWrap.style.display=''; companyLabel.textContent='Müşteri';
        fillCompany(CUSTOMERS,'Müşteri seçin');
        fillRole([['CustomerAdmin','Müşteri Yöneticisi'],['CustomerUser','Müşteri Kullanıcısı']]);
        hint.textContent='Müşteri kullanıcısı yalnızca Destek ve River Akademi görür.';
      } else if(v==='uretici' || v==='destek'){
        companyWrap.style.display='none';
        document.getElementById('roleWrap').style.display='none';
        document.getElementById('permWrap').style.display='';
        hint.textContent='Birden fazla yetki seçebilirsiniz. Portal Yönetimi: iş ortağı/bayi portalı vendor admin yetkileri. Eğitim Editörü: eğitim içeriği + versiyon duyuruları. Destek Personeli: destek kuyruğu.';
      } else {
        companyWrap.style.display=''; companyLabel.textContent='Bayi';
        fillCompany(PARTNERS,'Bayi seçin');
        fillRole([['PartnerAdmin','Bayi Yöneticisi'],['PartnerUser','Bayi Kullanıcısı']]);
        hint.textContent='';
      }
    }
    ut.addEventListener('change', apply);
    apply();
  </script>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
<script src="js/common.js?v=185"></script>
</body>
</html>
