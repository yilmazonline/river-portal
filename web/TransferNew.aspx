<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TransferNew.aspx.cs" Inherits="TransferNewPage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Yeni Dosya Paylaşımı</title>
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
    <div class="links"><%= Nav.Links("files") %></div>
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
    <div><h1 style="font-size:20px">Yeni Dosya Paylaşımı</h1>
    <p>Dosyaları yükleyin, alıcıları seçin; her alıcıya kişiye özel indirme linki gönderilir.</p></div>
    <a class="btn-ghost" href="Transfers.aspx" style="padding:10px 16px">Paylaşımlarım</a>
  </div>

  <div id="tfCompose">
  <div class="tf-cols">
    <div>
      <div class="card">
        <div class="section-h"><span>1. Dosyalar</span><span class="bc-sum" id="tfSum">Toplam 0 B / <%= MaxLabel %></span></div>
        <div class="bc-pad">
          <label class="tf-drop" id="tfDrop">
            <input type="file" id="tfInput" multiple="multiple" />
            <svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"><path d="M12 16V4"/><path d="m6 10 6-6 6 6"/><path d="M4 20h16"/></svg>
            <b>Dosyaları buraya sürükleyin</b><span>ya da seçmek için tıklayın · en fazla <%= MaxLabel %></span>
          </label>
          <div id="tfFiles" class="tf-files"></div>
        </div>
      </div>

      <div class="card" style="margin-top:14px">
        <div class="section-h"><span>3. Mesaj ve Ayarlar</span></div>
        <div class="bc-pad">
          <div class="field"><label>Başlık</label><input class="input" id="tfTitle" maxlength="200" placeholder="Örn. River AI 5.2 kurulum paketi" /></div>
          <div class="field"><label>Mesaj <span style="color:var(--dim);font-weight:400">(opsiyonel)</span></label>
            <textarea class="input" id="tfMsg" maxlength="2000" style="min-height:90px;resize:vertical" placeholder="Alıcılara kısa bir not"></textarea></div>
          <div class="bc-row">
            <div class="field"><label>Geçerlilik</label>
              <select class="input st-select" id="tfDays"><option value="1">1 gün</option><option value="3">3 gün</option><option value="7" selected="selected">7 gün</option><option value="14">14 gün</option><option value="30">30 gün</option></select></div>
            <div class="field"><label>İndirme sınırı <span style="color:var(--dim);font-weight:400">(alıcı başına)</span></label>
              <input class="input" id="tfMax" type="number" min="1" max="1000" placeholder="Sınırsız" /></div>
            <div class="field"><label>Şifre <span style="color:var(--dim);font-weight:400">(opsiyonel)</span></label>
              <input class="input" id="tfPwd" type="text" autocomplete="off" data-lpignore="true" data-1p-ignore="true" placeholder="Şifresiz" /></div>
          </div>
          <p class="bc-note" style="margin-top:4px">Şifre e-postada yazılmaz; alıcılara ayrıca iletmeniz gerekir. Süre dolduğunda linkler kapanır ve dosyalar sunucudan silinir.</p>
        </div>
      </div>
    </div>

    <div>
      <div class="card">
        <div class="section-h"><span>2. Alıcılar</span><span class="bc-sum" id="tfRcSum">Seçim yok</span></div>
        <div class="bc-pad">
          <div id="tfChips" class="tf-chips"></div>
          <input type="search" class="input" id="tfFirmQ" placeholder="Bayi veya müşteri ara..." autocomplete="off" readonly="readonly" data-lpignore="true" data-1p-ignore="true" data-form-type="other" />
          <div id="tfFirms" class="tf-firms"></div>
          <div class="field" style="margin-top:14px"><label>Harici e-posta adresleri <span style="color:var(--dim);font-weight:400">(virgül ya da satır ile ayırın)</span></label>
            <textarea class="input" id="tfEmails" style="min-height:70px;resize:vertical" placeholder="ornek@firma.com, diger@firma.com"></textarea></div>
          <p class="bc-note">Seçilen portal kullanıcıları e-postaya ek olarak zil menülerinde de bildirim alır. Harici adresler yalnız e-posta alır.</p>
        </div>
      </div>
      <button class="btn-cyan tf-send" type="button" id="tfSend">Yükle ve Gönder</button>
    </div>
  </div>
  </div>

  <div id="tfProgress" class="card tf-prog" style="display:none">
    <div class="bc-pad" style="padding:26px">
      <h2 id="tfPH">Dosyalar yükleniyor…</h2>
      <p class="tf-ps" id="tfPS">Lütfen sayfayı kapatmayın.</p>
      <div class="tf-bar"><i id="tfPBar"></i></div>
      <div class="tf-pm"><span id="tfPPct">%0</span><span id="tfPSpeed"></span></div>
      <div id="tfPFiles" class="tf-pfiles"></div>
      <div id="tfDone" style="display:none;text-align:center;margin-top:18px">
        <a class="btn-cyan" id="tfDetail" href="Transfers.aspx" style="padding:11px 24px;text-decoration:none;display:inline-block">Paylaşımı Görüntüle</a>
        <a class="btn-ghost" href="TransferNew.aspx" style="padding:11px 20px;text-decoration:none;display:inline-block;margin-left:6px">Yeni Paylaşım</a>
      </div>
    </div>
  </div>
</div>
</div>
<script>
(function () {
  var FIRMS = <%= FirmsJson %>, MAX = <%= MaxBytes %>;
  var $ = function (id) { return document.getElementById(id); };
  var files = [], picked = {};            // picked[userId] = {name, firm}
  function esc(s){ var d=document.createElement('div'); d.textContent=s||''; return d.innerHTML; }
  function fmt(b){ if (b>=1073741824) return (b/1073741824).toFixed(2).replace('.',',')+' GB'; if (b>=1048576) return (b/1048576).toFixed(1).replace('.',',')+' MB'; if (b>=1024) return Math.round(b/1024)+' KB'; return b+' B'; }
  function total(){ var t=0; for (var i=0;i<files.length;i++) t+=files[i].size; return t; }

  // ---- Dosyalar ----
  function renderFiles(){
    var h=''; for (var i=0;i<files.length;i++) h+='<div class="tf-file"><span class="tf-fn">'+esc(files[i].name)+'</span><span class="tf-fs">'+fmt(files[i].size)+'</span><button type="button" class="tf-x" data-i="'+i+'" title="Kaldır">&times;</button></div>';
    $('tfFiles').innerHTML=h;
    var t=total(); $('tfSum').textContent='Toplam '+fmt(t)+' / <%= MaxLabel %>'; $('tfSum').classList.toggle('over', t>MAX);
    if (!$('tfTitle').value && files.length===1) $('tfTitle').placeholder=files[0].name;
  }
  function add(list){ for (var i=0;i<list.length;i++) if (list[i].size>0) files.push(list[i]); renderFiles(); }
  $('tfInput').addEventListener('change', function(){ add(this.files); this.value=''; });
  var drop=$('tfDrop');
  ['dragenter','dragover'].forEach(function(ev){ drop.addEventListener(ev,function(e){ e.preventDefault(); drop.classList.add('on'); }); });
  ['dragleave','drop'].forEach(function(ev){ drop.addEventListener(ev,function(e){ e.preventDefault(); drop.classList.remove('on'); }); });
  drop.addEventListener('drop', function(e){ if (e.dataTransfer && e.dataTransfer.files) add(e.dataTransfer.files); });
  $('tfFiles').addEventListener('click', function(e){ var b=e.target.closest?e.target.closest('.tf-x'):null; if(!b)return; files.splice(+b.getAttribute('data-i'),1); renderFiles(); });

  // ---- Alicilar ----
  function renderChips(){
    var ks=Object.keys(picked), h='';
    for (var i=0;i<ks.length;i++) h+='<span class="tf-chip">'+esc(picked[ks[i]].name)+' <em>'+esc(picked[ks[i]].firm)+'</em><button type="button" data-u="'+ks[i]+'">&times;</button></span>';
    $('tfChips').innerHTML=h;
    var ext=parseEmails().length;
    $('tfRcSum').textContent=(ks.length+ext)===0?'Seçim yok':(ks.length+' kullanıcı'+(ext?' · '+ext+' harici':''));
    var cbs=document.querySelectorAll('.tf-u input'); for (var j=0;j<cbs.length;j++) cbs[j].checked=!!picked[cbs[j].value];
  }
  function parseEmails(){ return ($('tfEmails').value||'').split(/[\s,;]+/).filter(function(x){ return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(x); }); }
  $('tfEmails').addEventListener('input', renderChips);
  $('tfChips').addEventListener('click', function(e){ var b=e.target.closest?e.target.closest('button[data-u]'):null; if(!b)return; delete picked[b.getAttribute('data-u')]; renderChips(); });
  function renderFirms(q){
    q=(q||'').toLowerCase(); var h='', last='';
    for (var i=0;i<FIRMS.length;i++){ var f=FIRMS[i]; if (q && f.n.toLowerCase().indexOf(q)<0) continue;
      if (f.g!==last){ h+='<div class="tf-g">'+(f.g==='Bayi'?'Bayiler':'Müşteriler')+'</div>'; last=f.g; }
      h+='<div class="tf-firm" data-c="'+f.id+'" data-n="'+esc(f.n)+'"><div class="tf-fh"><span>'+esc(f.n)+'</span><span class="tf-cv">&#9662;</span></div><div class="tf-us" style="display:none"></div></div>'; }
    $('tfFirms').innerHTML=h||'<div class="tf-empty">Sonuç yok</div>';
  }
  var fq=$('tfFirmQ'), free=function(){ fq.removeAttribute('readonly'); };
  fq.addEventListener('focus',free); fq.addEventListener('mousedown',free); fq.addEventListener('touchstart',free);
  fq.addEventListener('input', function(){ renderFirms(this.value); });
  $('tfFirms').addEventListener('click', function(e){
    var cb=e.target.closest?e.target.closest('.tf-u input, .tf-all input'):null;
    if (cb) return; // checkbox degisimi 'change' ile islenir
    var head=e.target.closest?e.target.closest('.tf-fh'):null; if(!head)return;
    var box=head.parentNode, us=box.querySelector('.tf-us'), cid=box.getAttribute('data-c'), fname=box.getAttribute('data-n');
    if (us.style.display!=='none'){ us.style.display='none'; box.classList.remove('open'); return; }
    box.classList.add('open'); us.style.display='';
    if (us.getAttribute('data-l')) return;
    us.innerHTML='<div class="tf-empty">Yükleniyor…</div>';
    fetch('FirmUsers.aspx?cid='+cid,{credentials:'same-origin'}).then(function(r){return r.json();}).then(function(j){
      us.setAttribute('data-l','1');
      if (!j.users||!j.users.length){ us.innerHTML='<div class="tf-empty">Bu firmada aktif kullanıcı yok.</div>'; return; }
      var h='<label class="tf-all"><input type="checkbox" /> Tümünü seç</label>';
      for (var i=0;i<j.users.length;i++){ var u=j.users[i]; h+='<label class="tf-u"><input type="checkbox" value="'+u.id+'" data-n="'+esc(u.name)+'" data-f="'+esc(fname)+'"'+(picked[u.id]?' checked':'')+' /><span><b>'+esc(u.name)+'</b> <em>'+esc(u.email)+'</em></span></label>'; }
      us.innerHTML=h;
    }).catch(function(){ us.innerHTML='<div class="tf-empty">Kullanıcılar yüklenemedi.</div>'; });
  });
  $('tfFirms').addEventListener('change', function(e){
    var t=e.target;
    if (t.closest && t.closest('.tf-all')){ var bs=t.closest('.tf-us').querySelectorAll('.tf-u input'); for (var i=0;i<bs.length;i++){ bs[i].checked=t.checked; tog(bs[i]); } renderChips(); return; }
    if (t.closest && t.closest('.tf-u')){ tog(t); renderChips(); }
  });
  function tog(cb){ if (cb.checked) picked[cb.value]={ name:cb.getAttribute('data-n'), firm:cb.getAttribute('data-f') }; else delete picked[cb.value]; }
  renderFirms('');

  // ---- Yukleme ----
  function post(url, body, onprog){
    return new Promise(function(res, rej){
      var x=new XMLHttpRequest(); x.open('POST', url, true); x.withCredentials=true;
      if (onprog && x.upload) x.upload.onprogress=function(ev){ if (ev.lengthComputable) onprog(ev.loaded); };
      x.onload=function(){ var j=null; try{ j=JSON.parse(x.responseText); }catch(e){} if (x.status>=200&&x.status<300&&j&&j.ok) res(j); else rej({ status:x.status, j:j }); };
      x.onerror=function(){ rej({ status:0 }); };
      x.send(body);
    });
  }
  var sending=false;
  window.addEventListener('beforeunload', function(e){ if (sending){ e.preventDefault(); e.returnValue='Yükleme devam ediyor.'; return e.returnValue; } });

  $('tfSend').addEventListener('click', function(){
    var t=total(), title=($('tfTitle').value||'').trim()||(files.length===1?files[0].name:'');
    var uids=Object.keys(picked), emails=parseEmails();
    if (!files.length) return alert('Lütfen en az bir dosya ekleyin.');
    if (t>MAX) return alert('Toplam boyut <%= MaxLabel %> sınırını aşıyor.');
    if (!title) return alert('Lütfen bir başlık girin.');
    if (!uids.length && !emails.length) return alert('Lütfen en az bir alıcı seçin veya e-posta adresi girin.');
    if (!confirm(files.length+' dosya ('+fmt(t)+') '+(uids.length+emails.length)+' alıcıya gönderilecek. Devam edilsin mi?')) return;

    var fd=new FormData();
    fd.append('title', title); fd.append('message', $('tfMsg').value||''); fd.append('days', $('tfDays').value);
    fd.append('maxdl', $('tfMax').value||''); fd.append('password', $('tfPwd').value||'');
    for (var i=0;i<files.length;i++){ fd.append('fname', files[i].name); fd.append('fsize', files[i].size); fd.append('ftype', files[i].type||''); }

    $('tfCompose').style.display='none'; $('tfProgress').style.display=''; sending=true;
    var ph=''; for (var i2=0;i2<files.length;i2++) ph+='<div class="tf-pf" id="pf'+i2+'"><span>'+esc(files[i2].name)+'</span><em>Bekliyor</em></div>'; $('tfPFiles').innerHTML=ph;
    var doneBytes=0, t0=Date.now();
    function show(extra){
      var b=doneBytes+(extra||0), p=t?Math.min(100,Math.floor(b*100/t)):100, sec=(Date.now()-t0)/1000, sp=sec>0?b/sec:0;
      $('tfPBar').style.width=p+'%'; $('tfPPct').textContent='%'+p+' · '+fmt(b)+' / '+fmt(t);
      $('tfPSpeed').textContent=sp>0?(fmt(sp)+'/sn · kalan ~'+Math.max(1,Math.ceil((t-b)/sp/60))+' dk'):'';
    }
    function fail(msg){ sending=false; $('tfPH').textContent='Gönderim tamamlanamadı'; $('tfPS').textContent=msg||'Bağlantı hatası. Lütfen tekrar deneyin.'; $('tfPS').classList.add('err'); }

    post('TransferApi.ashx?action=create', fd).then(function(c){
      var chunk=c.chunk, tid=c.transferId, list=c.files, idx=0;
      function nextFile(){
        if (idx>=list.length) return finalize();
        var f=files[idx], meta=list[idx], off=0, tries=0, el=$('pf'+idx);
        el.className='tf-pf on'; el.querySelector('em').textContent='%0';
        function nextChunk(){
          if (off>=f.size){ el.className='tf-pf ok'; el.querySelector('em').textContent='Tamamlandı'; idx++; return nextFile(); }
          var end=Math.min(f.size, off+chunk), blob=f.slice(off,end);
          post('TransferApi.ashx?action=chunk&t='+tid+'&f='+meta.id+'&o='+off, blob, function(l){ show(l); el.querySelector('em').textContent='%'+Math.floor((off+l)*100/f.size); })
            .then(function(r){ doneBytes+=(r.uploaded-off); off=r.uploaded; tries=0; show(); nextChunk(); })
            .catch(function(e){
              if (e.status===409 && e.j && typeof e.j.expected==='number'){ doneBytes+=(e.j.expected-off); off=e.j.expected; return nextChunk(); }
              if (++tries<=4){ el.querySelector('em').textContent='Yeniden deneniyor…'; return setTimeout(nextChunk, 1500*tries); }
              fail((e.j&&e.j.error)||('“'+f.name+'” yüklenemedi.'));
            });
        }
        nextChunk();
      }
      function finalize(){
        $('tfPH').textContent='Alıcılara gönderiliyor…'; $('tfPSpeed').textContent='';
        var ff=new FormData(); ff.append('t', tid); ff.append('uids', uids.join(',')); ff.append('emails', emails.join('\n'));
        post('TransferApi.ashx?action=finalize', ff).then(function(r){
          sending=false;
          $('tfPH').textContent='Paylaşım gönderildi';
          $('tfPS').textContent=r.sent+' alıcıya kişiye özel link gönderildi'+(r.errors?(' · '+r.errors+' e-posta gönderilemedi'):'')+'.';
          $('tfDetail').href='TransferDetail.aspx?id='+tid; $('tfDone').style.display='';
        }).catch(function(e){ fail((e.j&&e.j.error)||'Gönderim sırasında hata oluştu.'); });
      }
      nextFile();
    }).catch(function(e){ fail((e.j&&e.j.error)||'Paylaşım oluşturulamadı.'); $('tfCompose').style.display=''; $('tfProgress').style.display='none'; alert((e.j&&e.j.error)||'Paylaşım oluşturulamadı.'); });
  });
})();
</script>
<script src="js/common.js?v=186"></script>
</body>
</html>
