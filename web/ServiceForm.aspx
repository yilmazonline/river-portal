<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ServiceForm.aspx.cs" Inherits="ServiceFormPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Hizmet Formu</title>
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
    <div class="links"><%= Nav.Links("support") %></div>
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
    <div><h1 style="font-size:20px">Hizmet Formu</h1>
    <p>Talep açılmadan verilen destek/proje hizmetini müşteri kişilerine iletin; süre ilgili bakiyeden düşülür.</p></div>
    <a class="btn-ghost" href="Tickets.aspx" style="padding:10px 16px">Destek</a>
  </div>

  <% if (FlashMsg.Length > 0) { %><div class="flash-ok" style="margin-bottom:14px"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>
  <% if (FlashErr.Length > 0) { %><div class="flash-err" style="margin-bottom:14px"><%= Server.HtmlEncode(FlashErr) %></div><% } %>

  <div class="card combo-host">
    <div class="section-h"><span>Yeni Hizmet Bildirimi</span></div>
    <div style="padding:8px 24px 24px">
      <form method="post" action="ServiceForm.aspx" id="svcForm">
        <div class="field">
          <label>Hizmet Türü</label>
          <div class="seg">
            <label><input type="radio" name="type" value="destek" checked="checked" /> <span>Destek</span></label>
            <label><input type="radio" name="type" value="proje" /> <span>Proje</span></label>
          </div>
        </div>

        <div class="field">
          <label>Firma</label>
          <div class="combo" id="firmCombo">
            <input type="hidden" name="cid" id="svcCid" />
            <button type="button" class="input combo-btn" id="firmBtn"><span id="firmLbl" class="combo-ph">Firma seçin...</span><span class="combo-caret">&#9662;</span></button>
            <div class="combo-panel" id="firmPanel">
              <div class="combo-search-wrap"><input type="search" class="combo-search" id="firmSearch" name="sf_firm_search" aria-label="Firma ara" placeholder="Ara..." autocomplete="off" readonly="readonly" data-lpignore="true" data-1p-ignore="true" data-bwignore="true" data-form-type="other" /></div>
              <div class="combo-list" id="firmList"></div>
            </div>
          </div>
          <div id="svcPlan" class="svc-plan" style="display:none"></div>
        </div>

        <div class="field" id="poolWrap" style="display:none">
          <label>Süre hangi bakiyeden düşülsün?</label>
          <div class="seg">
            <label><input type="radio" name="poolsource" value="project" checked="checked" /> <span>Proje süresi (saat)</span></label>
            <label><input type="radio" name="poolsource" value="support" /> <span>Destek süresi (dk)</span></label>
          </div>
        </div>

        <div class="field" id="minWrap">
          <label>Harcanan Süre (dakika)</label>
          <input class="input" type="number" name="minutes" id="svcMin" min="0" step="5" value="15" style="max-width:200px" />
        </div>
        <div class="field" id="hourWrap" style="display:none">
          <label>Harcanan Süre (saat, ondalıklı olabilir)</label>
          <input class="input" type="number" name="hours" id="svcHour" min="0" step="0.25" value="1" style="max-width:200px" />
        </div>

        <div class="field">
          <label>Bildirilecek Kişiler</label>
          <div id="svcUsers" class="svc-users"><span style="color:var(--muted);font-size:13px">Önce firma seçin.</span></div>
        </div>
        <div class="field">
          <label>Ek E-posta <span style="color:var(--dim);font-weight:400">(opsiyonel, firmada tanımlı olmayan biri)</span></label>
          <input class="input" type="email" name="extraEmail" placeholder="ornek@firma.com" style="max-width:320px" />
        </div>

        <div class="field">
          <label>Hizmet İçeriği</label>
          <textarea class="input" name="content" id="svcContent" style="min-height:160px;resize:vertical" placeholder="Verilen hizmeti ayrıntılı yazın..." required="required"></textarea>
        </div>

        <button class="btn-cyan" type="submit" style="padding:12px 26px">Gönder</button>
      </form>
    </div>
  </div>
</div>
</div>
<script>
(function () {
  var cid = document.getElementById('svcCid'), usersBox = document.getElementById('svcUsers'), planBox = document.getElementById('svcPlan');
  var poolWrap = document.getElementById('poolWrap'), minWrap = document.getElementById('minWrap'), hourWrap = document.getElementById('hourWrap');
  var plan = { minutes: 0, hours: 0, expired: false, until: '' };
  function esc(s){var d=document.createElement('div');d.textContent=s||'';return d.innerHTML;}
  function typeVal(){ var r=document.querySelector('input[name=type]:checked'); return r?r.value:'destek'; }
  function poolVal(){ var r=document.querySelector('input[name=poolsource]:checked'); return r?r.value:'project'; }
  function applyMode(){
    var t=typeVal();
    poolWrap.style.display = (t==='proje') ? 'block' : 'none';
    var useHours = (t==='proje' && poolVal()==='project');
    hourWrap.style.display = useHours ? 'block' : 'none';
    minWrap.style.display = useHours ? 'none' : 'block';
    renderPlan();
  }
  function renderPlan(){
    if (!cid.value){ planBox.style.display='none'; return; }
    var useHours = (typeVal()==='proje' && poolVal()==='project');
    var txt, warn=false;
    if (useHours){ txt='Kalan proje süresi: <b>'+plan.hours+' saat</b>'; if(plan.hours<=0) warn=true; }
    else { txt='Kalan destek süresi: <b>'+plan.minutes+' dk</b>' + (plan.until?(' · Geçerlilik: <b>'+esc(plan.until)+'</b>'):''); if(plan.minutes<=0||plan.expired) warn=true; }
    planBox.style.display='block';
    planBox.className='svc-plan'+(warn?' warn':'');
    planBox.innerHTML = txt + (warn ? ' <span class="svc-warn">— yetersiz/dolmuş; göndermeye devam ederseniz bakiye eksiye düşer.</span>' : '');
  }
  function loadFirm(){
    usersBox.innerHTML='<span style="color:var(--muted);font-size:13px">Yükleniyor...</span>';
    if (!cid.value){ usersBox.innerHTML='<span style="color:var(--muted);font-size:13px">Önce firma seçin.</span>'; planBox.style.display='none'; return; }
    fetch('FirmUsers.aspx?cid='+encodeURIComponent(cid.value),{credentials:'same-origin'}).then(function(r){return r.json();}).then(function(j){
      plan.minutes=j.plan.minutes||0; plan.hours=parseFloat(j.plan.hours)||0; plan.expired=!!j.plan.expired; plan.until=j.plan.until||'';
      if (!j.users || j.users.length===0){ usersBox.innerHTML='<span style="color:var(--muted);font-size:13px">Bu firmada tanımlı aktif kullanıcı yok. Aşağıdan ek e-posta girebilirsiniz.</span>'; }
      else {
        var h='';
        for (var i=0;i<j.users.length;i++){ var u=j.users[i];
          h+='<label class="svc-chk"><input type="checkbox" name="targets" value="'+u.id+'" /><span><b>'+esc(u.name)+'</b> <em>'+esc(u.email)+'</em></span></label>';
        }
        usersBox.innerHTML=h;
      }
      renderPlan();
    }).catch(function(){ usersBox.innerHTML='<span style="color:#E4467A;font-size:13px">Kullanıcılar yüklenemedi.</span>'; });
  }
  cid.addEventListener('change', loadFirm);
  // --- Aramalı firma dropdown ---
  var FIRMS = <%= CompanyJson %>;
  var combo = document.getElementById('firmCombo'), fbtn = document.getElementById('firmBtn'),
      fpanel = document.getElementById('firmPanel'), fsearch = document.getElementById('firmSearch'),
      flist = document.getElementById('firmList'), flbl = document.getElementById('firmLbl');
  // Tarayici otomatik doldurmasini engelle: salt okunur gelir, kullanici etkilesiminde acilir
  if (fsearch) ['focus', 'mousedown', 'touchstart', 'keydown'].forEach(function (ev) { fsearch.addEventListener(ev, function () { fsearch.removeAttribute('readonly'); }); });
  function renderList(q){
    q = (q||'').toLowerCase().replace(/^\s+|\s+$/g,'');
    var h='', lastG='';
    for (var i=0;i<FIRMS.length;i++){ var f=FIRMS[i];
      if (q && f.name.toLowerCase().indexOf(q)<0) continue;
      if (f.g!==lastG){ h+='<div class="combo-group">'+esc(f.g==='Bayi'?'Bayiler':'Müşteriler')+'</div>'; lastG=f.g; }
      h+='<div class="combo-opt" data-id="'+f.id+'" data-name="'+esc(f.name)+'">'+esc(f.name)+'</div>';
    }
    flist.innerHTML = h || '<div class="combo-empty">Sonuç yok</div>';
  }
  function openCombo(){ renderList(''); fpanel.classList.add('open'); fsearch.value=''; setTimeout(function(){fsearch.focus();},10); }
  function closeCombo(){ fpanel.classList.remove('open'); }
  fbtn.addEventListener('click', function(e){ e.stopPropagation(); if(fpanel.classList.contains('open')) closeCombo(); else openCombo(); });
  fsearch.addEventListener('input', function(){ renderList(this.value); });
  fsearch.addEventListener('click', function(e){ e.stopPropagation(); });
  flist.addEventListener('click', function(e){
    var o=e.target.closest ? e.target.closest('.combo-opt') : null; if(!o) return;
    cid.value=o.getAttribute('data-id'); flbl.textContent=o.getAttribute('data-name'); flbl.className='';
    closeCombo(); loadFirm();
  });
  document.addEventListener('click', function(){ closeCombo(); });
  fpanel.addEventListener('click', function(e){ e.stopPropagation(); });
  var rs=document.querySelectorAll('input[name=type],input[name=poolsource]');
  for (var i=0;i<rs.length;i++) rs[i].addEventListener('change', applyMode);
  applyMode();
  document.getElementById('svcForm').addEventListener('submit', function(e){
    var useHours = (typeVal()==='proje' && poolVal()==='project');
    var need = useHours ? (parseFloat(document.getElementById('svcHour').value)||0) : (parseInt(document.getElementById('svcMin').value,10)||0);
    var avail = useHours ? plan.hours : plan.minutes;
    var expired = useHours ? false : plan.expired;
    if (cid.value && (avail<=0 || expired || need>avail)){
      if (!confirm('Bu firmanın ilgili bakiyesi yetersiz veya süresi dolmuş. Yine de göndermek istiyor musunuz? Bakiye eksiye düşecektir.')) { e.preventDefault(); }
    }
  });
})();
</script>
<script src="js/common.js?v=185"></script>
</body>
</html>
