<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Transfer.aspx.cs" Inherits="TransferPublicPage" %><!DOCTYPE html>
<html lang="tr">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<meta name="robots" content="noindex" />
<title><%= State == "ok" || State == "password" ? V(T.Title) + " · " : "" %>River Dosya Paylaşımı</title>
<link href="https://fonts.googleapis.com/css2?family=Sora:wght@600;700;800&family=Inter:wght@400;500;600&display=swap" rel="stylesheet" />
<link rel="icon" type="image/svg+xml" href="<%= U("~/assets/RIVER_AI_badge.svg") %>" />
<style>
  *{box-sizing:border-box}
  body{margin:0;min-height:100vh;font-family:Inter,Segoe UI,Arial,sans-serif;color:#1A2338;display:flex;flex-direction:column;align-items:center;padding:40px 16px;
       background:radial-gradient(900px 520px at 15% 0%,rgba(79,70,229,.12),transparent 60%),radial-gradient(900px 520px at 90% 100%,rgba(34,184,216,.12),transparent 60%),#F2F5FA}
  .logo{height:34px;margin-bottom:6px}
  .sub{font-size:11px;letter-spacing:.24em;color:#8A97AE;margin-bottom:26px}
  .card{width:100%;max-width:600px;background:#fff;border:1px solid #E3E8F0;border-radius:20px;box-shadow:0 20px 50px rgba(20,30,60,.10);overflow:hidden}
  .hd{padding:24px 28px 18px;border-bottom:1px solid #EEF1F6}
  .from{font-size:13px;color:#6B7A99}.from b{color:#1B2440}
  .hd h1{font-family:Sora,sans-serif;font-size:21px;margin:6px 0 0;line-height:1.3}
  .bd{padding:20px 28px 26px}
  .msg{background:#F6F8FC;border:1px solid #E6EBF3;border-left:3px solid #4F46E5;border-radius:10px;padding:12px 15px;margin-bottom:18px;font-size:13.5px;line-height:1.65;color:#2A3550;white-space:pre-wrap}
  .meta{display:flex;gap:16px;flex-wrap:wrap;font-size:12.5px;color:#6B7A99;margin-bottom:14px}
  .meta b{color:#1B2440}
  .fl{display:flex;align-items:center;gap:12px;padding:12px 0;border-top:1px solid #EEF1F6}
  .fi{width:44px;height:44px;flex:0 0 auto;border-radius:11px;display:flex;align-items:center;justify-content:center;font-size:10.5px;font-weight:800;letter-spacing:.04em;
      color:#4F46E5;background:rgba(79,70,229,.08);border:1px solid rgba(79,70,229,.2)}
  .fm{flex:1;min-width:0;display:flex;flex-direction:column;gap:2px}
  .fm b{font-size:13.5px;font-weight:600;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
  .fm span{font-size:12px;color:#8A97AE}
  .dl{flex:0 0 auto;text-decoration:none;font-size:13px;font-weight:600;color:#4F46E5;border:1px solid #D5D8FB;border-radius:10px;padding:8px 16px}
  .dl:hover{background:#EEF0FE}
  .all{display:block;width:100%;margin-top:18px;background:#4F46E5;color:#fff;border:0;border-radius:12px;padding:14px;font-size:15px;font-weight:700;cursor:pointer;font-family:inherit}
  .note{font-size:11.5px;color:#9AA6BC;margin-top:14px;line-height:1.6;text-align:center}
  .st{text-align:center;padding:34px 28px}
  .st i{width:54px;height:54px;border-radius:50%;display:inline-flex;align-items:center;justify-content:center;font-style:normal;font-size:24px;color:#fff;margin-bottom:12px}
  .st h2{font-family:Sora,sans-serif;font-size:19px;margin:0 0 8px}
  .st p{font-size:13.5px;color:#6B7A99;margin:0;line-height:1.6}
  form.pw{display:flex;flex-direction:column;gap:10px;margin-top:18px}
  form.pw input{font-size:15px;padding:12px 14px;border:1px solid #D6DEEC;border-radius:11px;outline:none;text-align:center}
  form.pw input:focus{border-color:#4F46E5}
  form.pw button{background:#4F46E5;color:#fff;border:0;border-radius:11px;padding:12px;font-size:14px;font-weight:600;cursor:pointer;font-family:inherit}
  .err{color:#C21D55;font-size:12.5px}
  .ft{margin-top:26px;display:flex;align-items:center;gap:8px;font-size:11.5px;color:#9AA6BC}
  .ft img{height:16px}
</style>
</head>
<body>
  <img class="logo" src="<%= U("~/assets/RIVER_AI_black.png") %>" alt="River AI" />
  <div class="sub">DOSYA PAYLAŞIMI</div>
  <div class="card">
  <% if (State == "ok") { %>
    <div class="hd"><div class="from"><b><%= V(T.CreatedByName) %></b> · Fol Yazılım A.Ş. sizinle dosya paylaştı</div><h1><%= V(T.Title) %></h1></div>
    <div class="bd">
      <% if (!string.IsNullOrEmpty(T.Message)) { %><div class="msg" data-no-i18n="1"><%= V(T.Message) %></div><% } %>
      <div class="meta"><span><b><%= FileCount %></b> dosya · <b><%= TransferRepo.FormatSize(T.TotalBytes) %></b></span><span>Son indirme: <b><%= ExpiresStr %></b></span>
        <% if (T.MaxDownloads.HasValue) { %><span>Kalan indirme hakkı: <b><%= Math.Max(0, T.MaxDownloads.Value - R.DownloadCount) %></b></span><% } %></div>
      <%= FilesHtml %>
      <% if (FileCount > 1) { %><button class="all" type="button" id="dlAll">Tümünü İndir</button><% } else { %><a class="all" style="text-align:center;text-decoration:none" href="#" id="dlAll">İndir</a><% } %>
      <div class="note">Bu bağlantı size özeldir. Yarıda kalan indirmeler aynı bağlantıdan kaldığı yerden sürdürülebilir.</div>
    </div>
  <% } else if (State == "password") { %>
    <div class="st"><i style="background:#4F46E5">&#128274;</i><h2><%= V(T.Title) %></h2><p>Bu paylaşım şifre ile korunuyor. Size ayrıca iletilen şifreyi girin.</p>
      <form method="post" class="pw"><input type="password" name="pwd" placeholder="Şifre" autofocus="autofocus" required="required" />
        <% if (PwdError.Length > 0) { %><span class="err"><%= V(PwdError) %></span><% } %><button type="submit">Devam Et</button></form></div>
  <% } else if (State == "limit") { %>
    <div class="st"><i style="background:#D99A1E">!</i><h2>İndirme sınırına ulaşıldı</h2><p>Bu paylaşım için tanımlı indirme hakkınız doldu. Dosyalara yeniden ihtiyacınız varsa gönderen kişiyle iletişime geçin.</p></div>
  <% } else if (State == "expired") { %>
    <div class="st"><i style="background:#8A97AE">&#8987;</i><h2>Bu paylaşımın süresi doldu</h2><p>Dosyalar artık indirilemiyor. Yeniden ihtiyacınız varsa gönderen kişiyle iletişime geçin.</p></div>
  <% } else { %>
    <div class="st"><i style="background:#E4467A">&#10005;</i><h2>Paylaşım bulunamadı</h2><p>Bağlantı hatalı ya da kaldırılmış olabilir. E-postadaki bağlantıyı eksiksiz açtığınızdan emin olun.</p></div>
  <% } %>
  </div>
  <div class="ft"><img src="<%= U("~/assets/fol_dark.png") %>" alt="Fol Yazılım" /> Fol Yazılım A.Ş. · River Asistan Portal</div>
<script>
(function () {
  var b = document.getElementById('dlAll'); if (!b) return;
  b.addEventListener('click', function (e) {
    e.preventDefault();
    var links = document.querySelectorAll('a[data-dl]');
    // Dosyalari sirayla gizli cercevelerde baslat (tarayicinin coklu indirme engeline takilmadan)
    for (var i = 0; i < links.length; i++) (function (h, d) {
      setTimeout(function () { var f = document.createElement('iframe'); f.style.display = 'none'; f.src = h; document.body.appendChild(f); }, d);
    })(links[i].href, i * 1500);
  });
})();
</script>
</body>
</html>
