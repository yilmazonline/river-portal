<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ExamEdit.aspx.cs" Inherits="ExamEditPage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Sınav Düzenle</title>
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
    <div class="links"><%= Nav.Links("edu") %></div>
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
    <div><h1 style="font-size:20px"><%= IsNew ? "Yeni Sınav" : V(X.Title) %></h1>
    <p><% if (!IsNew) { %><span class="st <%= ExamRepo.StatusClass(X.Status) %>"><%= ExamRepo.StatusLabel(X.Status) %></span> &nbsp;<% } %>Soruları ekleyin; puan toplamı 100 olduğunda sınavı yayına alabilirsiniz.</p></div>
    <div style="display:flex;gap:8px;flex-wrap:wrap">
      <a class="btn-ghost" href="Exams.aspx" style="padding:10px 16px">Sınavlar</a>
      <% if (!IsNew) { %><a class="btn-ghost" href="ExamTake.aspx?id=<%= X.ExamId %>" style="padding:10px 16px">Önizle</a><% } %>
    </div>
  </div>
  <% if (FlashMsg.Length > 0) { %><div class="flash-ok" style="margin-bottom:14px"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>
  <% if (FlashErr.Length > 0) { %><div class="flash-err" style="margin-bottom:14px"><%= Server.HtmlEncode(FlashErr) %></div><% } %>

  <% if (!IsNew) { %>
  <div class="card ee-bar">
    <div class="ee-gauge">
      <div class="ee-gauge-h"><span>Puan bütçesi</span><b class="<%= Remaining == 0 ? "ok" : "" %>"><%= 100 - Remaining %> / 100</b></div>
      <div class="ex-bar"><i style="width:<%= Math.Max(0, Math.Min(100, 100 - Remaining)) %>%"></i></div>
      <div class="ee-gauge-s"><%= Remaining == 0 ? "Puan dağılımı tamam, sınav yayına hazır." : (Remaining > 0 ? "Kalan puan: <b>" + Remaining + "</b>" : "Toplam 100'ü aşıyor: <b>" + (-Remaining) + "</b> puan fazla") %></div>
    </div>
    <div class="ee-actions">
      <% if (IsDraft) { %>
        <form method="post" data-noload="1" style="display:inline"><input type="hidden" name="action" value="equalize" />
          <button class="btn-ghost" type="submit" title="100 puanı sorulara eşit dağıtır">Puanları eşit dağıt</button></form>
        <form method="post" style="display:inline" onsubmit="return confirm('Sınav yayına alınacak. Aktive edilene kadar herkes salt okunur görür. Devam edilsin mi?')">
          <input type="hidden" name="action" value="publish" />
          <button class="btn-cyan" type="submit" <%= Remaining == 0 ? "" : "disabled=\"disabled\" title=\"Toplam puan 100 olmalı\"" %>>Yayına Al</button></form>
      <% } else if (X.Status == "Published") { %>
        <form method="post" style="display:inline-flex;align-items:center;gap:10px" onsubmit="return confirm('Sınav aktive edilecek; kullanıcılar giriş yapabilecek ve sorular kilitlenecek. Devam edilsin mi?')">
          <input type="hidden" name="action" value="activate" />
          <label class="ch-mini"><input type="checkbox" name="notify" checked="checked" /> Uygulama bildirimi gönder</label>
          <button class="btn-cyan" type="submit">Aktive Et</button></form>
        <form method="post" data-noload="1" style="display:inline"><input type="hidden" name="action" value="toDraft" />
          <button class="btn-ghost" type="submit">Taslağa Geri Al</button></form>
      <% } else if (X.Status == "Active") { %>
        <form method="post" style="display:inline" onsubmit="return confirm('Sınav arşive alınacak; yeni giriş yapılamaz. Verilen sertifikalar geçerli kalır. Devam edilsin mi?')">
          <input type="hidden" name="action" value="archive" /><button class="btn-ghost" type="submit">Arşive Al</button></form>
      <% } %>
      <form method="post" style="display:inline" onsubmit="return confirm('Sınavın soruları ile birlikte yeni bir taslak kopyası oluşturulsun mu?')">
        <input type="hidden" name="action" value="copy" /><button class="btn-ghost" type="submit">Kopyala</button></form>
      <% if (IsDraft) { %>
        <form method="post" style="display:inline" onsubmit="return confirm('Taslak sınav ve tüm soruları kalıcı olarak silinecek. Emin misiniz?')">
          <input type="hidden" name="action" value="delete" /><button class="btn-ghost eq-del" type="submit">Sil</button></form>
      <% } %>
    </div>
    <% if (!IsDraft && X.Status != "Archived") { %><div class="ee-lock">Sorular kilitli. Değişiklik için <%= X.Status == "Published" ? "\u201cTaslağa Geri Al\u201d" : "\u201cKopyala\u201d ile yeni sürüm" %> kullanın.</div><% } %>
  </div>
  <% } %>

  <div class="card" style="margin-top:14px">
    <div class="section-h"><span>Sınav Bilgileri</span></div>
    <div class="bc-pad">
    <% if (IsDraft) { %>
      <form method="post" id="metaForm">
        <div class="field"><label>Sınav Adı <span style="color:var(--dim);font-weight:400">(sertifikada bu ad yazar)</span></label><input class="input" name="title" id="exTitle" required="required" value="<%= V(X.Title) %>" placeholder="Örn. River AI Designer Sertifika Sınavı" /></div>
        <div class="field"><label>Açıklama <span style="color:var(--dim);font-weight:400">(opsiyonel)</span></label>
          <textarea class="input" name="desc" style="min-height:70px;resize:vertical"><%= V(X.Description) %></textarea></div>
        <div class="field"><label>Giriş Koşulu <span style="color:var(--dim);font-weight:400">(bu eğitimdeki tüm videolar izlenmeli)</span></label>
          <select class="input st-select" name="group"><%= GroupOptions %></select></div>
        <div class="bc-row">
          <div class="field"><label>Geçme Puanı</label><input class="input" type="number" name="pass" min="1" max="100" value="<%= X.PassScore %>" /></div>
          <div class="field"><label>Deneme Hakkı <span style="color:var(--dim);font-weight:400">(bitince editör onayıyla)</span></label><input class="input" type="number" name="max" min="1" max="20" value="<%= X.MaxAttempts %>" /></div>
          <div class="field"><label>Denemeler Arası (saat)</label><input class="input" type="number" name="cool" min="0" max="720" value="<%= X.CooldownHours %>" /></div>
          <div class="field"><label>Süre Sınırı (dk, 0 = yok)</label><input class="input" type="number" name="timelimit" min="0" max="600" value="<%= X.TimeLimitMin.HasValue ? X.TimeLimitMin.Value : 0 %>" /></div>
          <div class="field"><label>Sertifika Geçerliliği (ay, 0 = süresiz)</label><input class="input" type="number" name="valid" id="exValid" min="0" max="120" value="<%= X.ValidMonths.HasValue ? X.ValidMonths.Value : 0 %>" /></div>
        </div>
        <div class="field"><label>Sertifika Rozet Rengi<% if (!IsNew) { %> <a href="#" id="cpOpen" class="cp-open">Sertifikayı önizle &#8599;</a><% } %></label>
          <div class="bg-pick"><%= BadgeOptions %></div></div>
        <div style="display:flex;gap:10px;flex-wrap:wrap">
          <button class="btn-cyan" type="submit" name="action" value="saveMeta" style="padding:11px 22px"><%= IsNew ? "Oluştur ve Soru Eklemeye Geç" : "Kaydet" %></button>
          <% if (!IsNew) { %><button class="btn-ghost" type="submit" name="action" value="saveAndPublish" style="padding:11px 22px" onclick="return confirm('Kaydedilip yayına alınacak. Devam edilsin mi?')">Kaydet ve Yayına Al</button><% } %>
        </div>
      </form>
    <% } else { %>
      <div class="ee-sum">
        <div><span>Giriş koşulu</span><b><%= string.IsNullOrEmpty(X.CategoryGroup) ? "Yok" : V(X.CategoryGroup) + " videoları" %></b></div>
        <div><span>Geçme puanı</span><b><%= X.PassScore %></b></div>
        <div><span>Deneme hakkı</span><b><%= X.MaxAttempts %> · <%= X.CooldownHours %> sa ara</b><span>Sonrası editör onayıyla</span></div>
        <div><span>Süre</span><b><%= X.TimeLimitMin.HasValue ? X.TimeLimitMin.Value + " dk" : "Sınırsız" %></b></div>
        <div><span>Sertifika</span><b><%= X.ValidMonths.HasValue ? X.ValidMonths.Value + " ay geçerli" : "Süresiz" %></b></div>
        <div class="ee-badge"><span>Rozet</span><b><%= ExamRepo.SealSvg(26, false, X.BadgeColor) %> <%= BadgePalette.Get(X.BadgeColor).Name %></b><a href="<%= PreviewUrl %>" target="_blank" class="cp-open">Sertifikayı önizle &#8599;</a></div>
      </div>
      <% if (!string.IsNullOrEmpty(X.Description)) { %><p style="margin:12px 0 0;color:var(--muted);font-size:13px"><%= V(X.Description) %></p><% } %>
    <% } %>
    </div>
  </div>

  <% if (IsNew) { %>
  <div class="card" style="margin-top:14px">
    <div class="section-h"><span>Sertifika Önizlemesi</span><span class="bc-sum" id="cpNote"><%= IsDraft ? "Sınav adı ve renk değiştikçe güncellenir" : "" %></span></div>
    <div class="bc-pad"><div class="cp-wrap" id="cpWrap"><iframe id="cpFrame" src="<%= PreviewUrl %>" title="Sertifika önizlemesi" scrolling="no"></iframe></div></div>
  </div>
  <% } %>

  <% if (!IsNew) { %>
  <div class="card" style="margin-top:14px" id="qlist">
    <div class="section-h"><span>Sorular</span><span class="bc-sum"><%= X.QuestionCount %> soru</span></div>
    <div class="bc-pad"><%= QuestionsHtml %></div>
  </div>

  <% if (IsDraft) { %>
  <div class="card" style="margin-top:14px" id="qform">
    <div class="section-h"><span><%= EditQ != null ? "Soruyu Düzenle" : "+ Soru Ekle" %></span></div>
    <div class="bc-pad">
    <% if (EditQ == null && Remaining <= 0) { %>
      <div class="ex-empty">Puan bütçesi doldu. Yeni soru eklemek için mevcut soruların puanını düşürün veya “Puanları eşit dağıt” kullanın.</div>
    <% } else { %>
      <form method="post" id="qf" data-noload="1">
        <input type="hidden" name="action" value="saveQ" />
        <input type="hidden" name="qid" value="<%= EditQ != null ? EditQ.QuestionId : 0 %>" />
        <div class="field"><label>Soru</label><textarea class="input" name="qtext" id="qtext" required="required" style="min-height:80px;resize:vertical"><%= EditQ != null ? V(EditQ.QText) : "" %></textarea></div>
        <div class="bc-row">
          <div class="field"><label>Soru Tipi</label>
            <select class="input st-select" name="qtype" id="qtype">
              <option value="single" <%= EditQ == null || EditQ.QType == "single" ? "selected=\"selected\"" : "" %>>Tek seçim</option>
              <option value="multi" <%= EditQ != null && EditQ.QType == "multi" ? "selected=\"selected\"" : "" %>>Çoklu seçim</option>
              <option value="tf" <%= EditQ != null && EditQ.QType == "tf" ? "selected=\"selected\"" : "" %>>Doğru / Yanlış</option>
            </select></div>
          <div class="field"><label>Puan <span style="color:var(--dim);font-weight:400">(en fazla <%= EditQMaxPoints %>)</span></label>
            <input class="input" type="number" name="points" id="qpts" min="1" max="<%= EditQMaxPoints %>" value="<%= EditQ != null ? EditQ.Points : Math.Min(EditQMaxPoints, 10) %>" required="required" /></div>
        </div>
        <label class="bc-lbl" id="optLbl">Şıklar <span style="font-weight:400;color:var(--dim)">(doğru olanı işaretleyin)</span></label>
        <div id="optBox" class="eq-optbox"></div>
        <button type="button" class="btn-ghost" id="optAdd" style="margin-top:8px;padding:7px 14px">+ Şık Ekle</button>
        <div style="display:flex;gap:10px;flex-wrap:wrap;margin-top:16px">
          <button class="btn-cyan" type="submit" style="padding:11px 22px"><%= EditQ != null ? "Soruyu Güncelle" : "Soruyu Ekle" %></button>
          <% if (EditQ != null) { %><a class="btn-ghost" href="ExamEdit.aspx?id=<%= X.ExamId %>#qlist" style="padding:11px 18px">Vazgeç</a><% } %>
        </div>
      </form>
    <% } %>
    </div>
  </div>
  <% } %>

  <% if (ResultsHtml.Length > 0) { %>
  <div class="card" style="margin-top:14px" id="results">
    <div class="section-h"><span>Sonuçlar</span></div>
    <div class="bc-pad"><%= ResultsHtml %></div>
  </div>
  <% } %>
  <% } %>
</div>
</div>
<script>
(function () {
  var wrap = document.getElementById('cpWrap'), fr = document.getElementById('cpFrame'), op = document.getElementById('cpOpen');
  var ti = document.getElementById('exTitle'), va = document.getElementById('exValid'), tm = null;
  function color() { var r = document.querySelector('input[name=badge]:checked'); return r ? r.value : 'gold'; }
  function url() { return 'Certificate.aspx?preview=1&title=' + encodeURIComponent(ti ? ti.value : '') + '&color=' + color() + '&valid=' + (va ? (parseInt(va.value, 10) || 0) : 0); }
  if (op) op.addEventListener('click', function (e) { e.preventDefault(); window.open(url(), '_blank'); });
  if (!wrap || !fr) return;
  function fit() { var s = wrap.clientWidth / 1123; fr.style.transform = 'scale(' + s + ')'; wrap.style.height = Math.round(794 * s) + 'px'; }
  window.addEventListener('resize', fit); fit();
  function refresh() { clearTimeout(tm); tm = setTimeout(function () { fr.src = url(); }, 350); }
  if (ti) ti.addEventListener('input', refresh);
  if (va) va.addEventListener('input', refresh);
  var rs = document.querySelectorAll('input[name=badge]');
  for (var i = 0; i < rs.length; i++) rs[i].addEventListener('change', refresh);
})();
</script>
<script>
(function () {
  var box = document.getElementById('optBox'), typeSel = document.getElementById('qtype'), form = document.getElementById('qf');
  if (!box || !typeSel || !form) return;
  var init = <%= EditOptsJson %>;
  var state = init.length ? init : [{t:'',c:true},{t:'',c:false},{t:'',c:false},{t:'',c:false}];
  var prevType = typeSel.value;
  function esc(s){ var d=document.createElement('div'); d.textContent=s||''; return d.innerHTML.replace(/"/g,'&quot;'); }
  function read(){ var rows = box.querySelectorAll('.eq-orow'); if (prevType === 'tf') return;
    state = []; for (var i=0;i<rows.length;i++){ state.push({ t: rows[i].querySelector('input[name=opt]').value, c: rows[i].querySelector('input[name=corr]').checked }); } }
  function render(){
    var t = typeSel.value, h = '';
    document.getElementById('optAdd').style.display = (t === 'tf') ? 'none' : '';
    if (t === 'tf') {
      var tfOk = (init.length === 2 && prevType === 'tf') ? (init[1].c ? 1 : 0) : 0;
      h = '<label class="eq-orow tf"><input type="radio" name="corr" value="0"' + (tfOk===0?' checked':'') + ' /> <span>Doğru</span></label>' +
          '<label class="eq-orow tf"><input type="radio" name="corr" value="1"' + (tfOk===1?' checked':'') + ' /> <span>Yanlış</span></label>';
    } else {
      var kind = (t === 'multi') ? 'checkbox' : 'radio', seen = false;
      for (var i=0;i<state.length;i++){
        var c = state[i].c; if (kind === 'radio') { if (c && seen) c = false; if (c) seen = true; }
        h += '<div class="eq-orow"><input type="' + kind + '" name="corr" value="' + i + '"' + (c?' checked':'') + ' title="Doğru cevap" />' +
             '<input class="input" name="opt" value="' + esc(state[i].t) + '" placeholder="Şık ' + String.fromCharCode(65+i) + '" />' +
             '<button type="button" class="eq-orm" data-i="' + i + '" title="Şıkkı kaldır">&times;</button></div>';
      }
    }
    box.innerHTML = h; prevType = t;
  }
  typeSel.addEventListener('change', function(){ read(); render(); });
  document.getElementById('optAdd').addEventListener('click', function(){ read(); if (state.length >= 8) return; state.push({t:'',c:false}); render(); });
  box.addEventListener('click', function(e){ var b = e.target.closest ? e.target.closest('.eq-orm') : null; if (!b) return; read(); if (state.length <= 2) return; state.splice(parseInt(b.getAttribute('data-i'),10),1); render(); });
  form.addEventListener('submit', function(e){
    var t = typeSel.value, pts = parseInt(document.getElementById('qpts').value,10), max = parseInt(document.getElementById('qpts').max,10);
    var msg = '';
    if (!document.getElementById('qtext').value.trim()) msg = 'Soru metni zorunludur.';
    else if (isNaN(pts) || pts < 1 || pts > max) msg = 'Puan 1 ile ' + max + ' arasında olmalı.';
    else if (t !== 'tf') {
      read(); var filled = 0, ok = 0;
      for (var i=0;i<state.length;i++){ if (state[i].t.trim()) { filled++; if (state[i].c) ok++; } }
      if (filled < 2) msg = 'En az iki şık girin.';
      else if (ok === 0) msg = 'Doğru cevabı işaretleyin (dolu bir şık olmalı).';
    } else if (!form.querySelector('input[name=corr]:checked')) msg = 'Doğru cevabı işaretleyin.';
    if (msg) { e.preventDefault(); alert(msg); }
  });
  render();
})();
</script>
<script src="js/common.js?v=185"></script>
</body>
</html>
