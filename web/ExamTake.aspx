<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ExamTake.aspx.cs" Inherits="ExamTakePage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Sınav</title>
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
    <div><h1 style="font-size:20px"><%= V(X.Title) %></h1>
    <p><% if (!IsTaker) { %><span class="st <%= ExamRepo.StatusClass(X.Status) %>"><%= ExamRepo.StatusLabel(X.Status) %></span> &nbsp;<% } %><%= X.QuestionCount %> soru · Geçme puanı <%= X.PassScore %><%= X.TimeLimitMin.HasValue ? " · " + X.TimeLimitMin.Value + " dk" : "" %></p></div>
    <div style="display:flex;gap:8px;flex-wrap:wrap">
      <% if (Mode != "take") { %><a class="btn-ghost" href="<%= BackUrl %>" style="padding:10px 16px"><%= BackLabel %></a><% } %>
      <% if (CanEdit) { %><a class="btn-cyan" href="ExamEdit.aspx?id=<%= X.ExamId %>" style="padding:10px 16px;text-decoration:none">Düzenle</a><% } %>
    </div>
  </div>
  <% if (FlashMsg.Length > 0) { %><div class="flash-ok" style="margin-bottom:14px"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>
  <% if (!string.IsNullOrEmpty(X.Description) && Mode != "take") { %><div class="card" style="padding:16px 22px;margin-bottom:14px;color:var(--muted);font-size:13.5px;line-height:1.6"><%= V(X.Description) %></div><% } %>

  <% if (Mode == "view") { %>
    <div class="xt-banner">
      <% if (CanEdit) { %>Editör önizlemesi · Doğru cevaplar işaretli gösterilir.
      <% } else if (!IsTaker) { %>Salt okunur görünüm.
      <% } else { %>Bu sınav yayında, henüz girişe açılmadı. Soruları salt okunur inceleyebilirsiniz; sınav aktive edildiğinde aşağıdaki şartları karşılayarak giriş yapabilirsiniz.<% } %>
    </div>
    <% if (ChecklistHtml.Length > 0) { %><div class="card gc-card"><div class="section-h"><span>Giriş Şartları</span></div><div class="gc-list"><%= ChecklistHtml %></div></div><% } %>
    <%= QuestionsHtml %>

  <% } else if (Mode == "gate") { %>
    <div class="card gc-card">
      <div class="gc-head">
        <div class="xt-ic <%= G.CanStart ? "" : "muted" %>"><svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="8" r="6"/><path d="M8.2 13.2 7 22l5-3 5 3-1.2-8.8"/></svg></div>
        <div><h2><%= G.CanStart ? "Sınava girmeye hazırsınız" : "Sınava giriş şartları" %></h2>
          <p><%= G.CanStart ? "Tüm şartlar karşılandı." : V(G.Reason) %></p></div>
      </div>
      <div class="gc-list"><%= ChecklistHtml %></div>
      <% if (G.CanStart) { %>
      <div class="gc-start">
        <p class="xt-note">Başladığınızda bir deneme hakkınız kullanılır.<%= X.TimeLimitMin.HasValue ? " Süre dolduğunda cevaplarınız otomatik teslim edilir." : "" %> Sorular ve şıklar karışık sırada gelir. Sınav sırasında kopyalama, yazdırma ve sağ tık kapalıdır; sınavdan sonra sorular tekrar gösterilmez.</p>
        <form method="post" onsubmit="return confirm('Sınav başlatılsın mı? Bir deneme hakkınız kullanılacak.')">
          <input type="hidden" name="action" value="start" />
          <button class="btn-cyan" type="submit" style="padding:13px 34px;font-size:15px">Sınava Başla</button>
        </form>
      </div>
      <% } %>
    </div>

  <% } else if (Mode == "take") { %>
    <form method="post" id="xtForm" data-noload="1" class="xt-guard" style="--wm:<%= WatermarkCss %>">
      <input type="hidden" name="action" value="submit" />
      <input type="hidden" name="attempt" value="<%= A.AttemptId %>" />
      <div class="xt-sticky">
        <span id="xtProg">0 / <%= X.QuestionCount %> cevaplandı</span>
        <% if (DeadlineMs > 0) { %><span class="xt-timer" id="xtTimer">--:--</span><% } %>
      </div>
      <div class="xt-qs"><%= QuestionsHtml %></div>
      <div style="text-align:center;margin:22px 0 10px">
        <button class="btn-cyan" type="submit" style="padding:13px 36px;font-size:15px">Sınavı Teslim Et</button>
      </div>
    </form>

  <% } else if (Mode == "result" || Mode == "name") { %>
    <div class="card xt-center">
      <div class="xt-score <%= A.Passed == true ? "ok" : "no" %>"><b><%= A.Score ?? 0 %></b><span>/ 100</span></div>
      <h2><%= A.Passed == true ? "Tebrikler, sınavı geçtiniz!" : "Bu denemede geçme puanına ulaşılamadı" %></h2>
      <p class="xt-note">Geçme puanı <%= X.PassScore %>. Sınav güvenliği için sorular ve doğru cevaplar gösterilmez.</p>
      <% if (Mode == "name") { %>
        <form method="post" class="xt-name">
          <input type="hidden" name="action" value="issue" />
          <label>Sertifikada görünecek adınız</label>
          <input class="input" name="certName" required="required" minlength="3" maxlength="160" value="<%= V(CertName) %>" />
          <label class="ch-mini" style="justify-content:center;margin-top:10px"><input type="checkbox" name="confirmName" required="required" /> Adımın doğru ve eksiksiz yazıldığını onaylıyorum</label>
          <p class="xt-note" style="margin-top:8px">Sertifika bu adla düzenlenir ve sonradan değiştirilemez.</p>
          <button class="btn-cyan" type="submit" style="padding:13px 32px;font-size:15px">Sertifikamı Oluştur</button>
        </form>
      <% } else { %>
        <% if (G.CanStart) { %>
          <p class="xt-note">Kalan deneme hakkınız: <b><%= G.FreeLeft %></b></p>
          <a class="btn-cyan" href="ExamTake.aspx?id=<%= X.ExamId %>" style="display:inline-block;padding:11px 24px;text-decoration:none">Tekrar Dene</a>
        <% } else if (G.NeedsRequest) { %>
          <p class="xt-note">Deneme hakkınız doldu. Yeniden girmek için eğitim editörüne talep gönderebilirsiniz.</p>
          <a class="btn-cyan" href="ExamTake.aspx?id=<%= X.ExamId %>" style="display:inline-block;padding:11px 24px;text-decoration:none">Yeniden Giriş Talebi Oluştur</a>
        <% } else { %>
          <div class="ex-why" style="max-width:520px;margin:0 auto"><%= V(G.Reason) %></div>
          <a class="btn-ghost" href="ExamTake.aspx?id=<%= X.ExamId %>" style="display:inline-block;margin-top:14px;padding:10px 20px">Giriş Şartlarını Gör</a>
        <% } %>
      <% } %>
    </div>

  <% } else if (Mode == "done") { %>
    <div class="card xt-center">
      <div style="display:inline-block"><%= ExamRepo.SealSvg(84, Cert.IsRevoked || Cert.IsExpired, Cert.BadgeColor) %></div>
      <h2>Bu sınavı başarıyla tamamladınız</h2>
      <p class="xt-note">Sertifika no: <b><%= V(Cert.CertNo) %></b></p>
      <a class="btn-cyan" href="Certificate.aspx?id=<%= Cert.CertificateId %>" style="display:inline-block;padding:11px 24px;text-decoration:none">Sertifikayı Görüntüle</a>
    </div>
  <% } %>
</div>
</div>
<script>
(function () {
  var form = document.getElementById('xtForm'); if (!form) return;
  // Kopyalama / yazdirma / sag tik kapali (caydirici)
  var block = function (e) { e.preventDefault(); return false; };
  ['copy', 'cut', 'contextmenu', 'selectstart', 'dragstart'].forEach(function (ev) { document.addEventListener(ev, block); });
  document.addEventListener('keydown', function (e) {
    var k = (e.key || '').toLowerCase();
    if ((e.ctrlKey || e.metaKey) && (k === 'c' || k === 'x' || k === 'p' || k === 's' || k === 'u' || k === 'a')) { e.preventDefault(); }
    if (k === 'printscreen') { try { navigator.clipboard.writeText(''); } catch (x) { } }
  });
  var total = <%= X.QuestionCount %>, sent = false;
  function answered(){ var qs = form.querySelectorAll('.xt-q'), n = 0;
    for (var i=0;i<qs.length;i++){ var on = qs[i].querySelector('input:checked'); qs[i].classList.toggle('done', !!on); if (on) n++; }
    document.getElementById('xtProg').textContent = n + ' / ' + total + ' cevaplandı'; return n; }
  form.addEventListener('change', answered);
  form.addEventListener('submit', function(e){
    if (sent) return;
    var n = answered();
    if (n < total && !confirm((total - n) + ' soru cevapsız. Yine de teslim edilsin mi? Cevapsız sorular 0 puan alır.')) { e.preventDefault(); return; }
    if (n === total && !confirm('Sınav teslim edilsin mi? Teslimden sonra sorular tekrar gösterilmez.')) { e.preventDefault(); return; }
    sent = true; window.onbeforeunload = null;
  });
  window.onbeforeunload = function(){ return 'Sınav devam ediyor. Sayfadan ayrılırsanız cevaplarınız kaydedilmez.'; };
  var dl = <%= DeadlineMs %>, t = document.getElementById('xtTimer');
  if (dl > 0 && t) {
    var tick = function(){
      var s = Math.max(0, Math.floor((dl - Date.now()) / 1000)), m = Math.floor(s / 60), r = s % 60;
      t.textContent = (m < 10 ? '0' : '') + m + ':' + (r < 10 ? '0' : '') + r;
      t.classList.toggle('warn', s <= 60);
      if (s <= 0 && !sent) { sent = true; window.onbeforeunload = null; form.submit(); return; }
      setTimeout(tick, 500);
    };
    tick();
  }
  answered();
})();
</script>
<script src="js/common.js?v=186"></script>
</body>
</html>
