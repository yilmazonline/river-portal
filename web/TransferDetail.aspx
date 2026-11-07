<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TransferDetail.aspx.cs" Inherits="TransferDetailPage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Paylaşım Detayı</title>
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
    <div><h1 style="font-size:20px"><%= V(T.Title) %></h1>
    <p><%= T.Status == "Cancelled" ? "<span class=\"st cancelled\">İptal edildi</span>" : T.IsExpired ? "<span class=\"st passive\">Süresi doldu</span>" : "<span class=\"st approved\">Aktif</span>" %> &nbsp;<%= V(T.CreatedByName) %> · <%= SentStr %></p></div>
    <a class="btn-ghost" href="Transfers.aspx" style="padding:10px 16px">Paylaşımlar</a>
  </div>
  <% if (FlashMsg.Length > 0) { %><div class="flash-ok" style="margin-bottom:14px"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>
  <div class="tf-cols">
    <div>
      <div class="card">
        <div class="section-h"><span>Alıcılar</span><span class="bc-sum"><%= T.DownloadedCount %> / <%= T.RecipientCount %> indirdi</span></div>
        <div class="tbl-scroll"><table class="tf-rt"><tbody><%= RecipientsHtml %></tbody></table></div>
      </div>
    </div>
    <div>
      <div class="card">
        <div class="section-h"><span>Paylaşım</span></div>
        <div class="bc-pad">
          <div class="ee-sum" style="grid-template-columns:1fr 1fr">
            <div><span>Toplam</span><b><%= T.FileCount %> dosya · <%= TransferRepo.FormatSize(T.TotalBytes) %></b></div>
            <div><span>Son indirme tarihi</span><b><%= ExpiresStr %></b></div>
            <div><span>İndirme sınırı</span><b><%= T.MaxDownloads.HasValue ? T.MaxDownloads.Value + " / alıcı" : "Sınırsız" %></b></div>
            <div><span>Koruma</span><b><%= T.HasPassword ? "Şifreli" : "Şifresiz" %></b></div>
          </div>
          <% if (!string.IsNullOrEmpty(T.Message)) { %><div class="tf-msg" data-no-i18n="1"><%= V(T.Message) %></div><% } %>
          <div class="tf-files" style="margin-top:12px"><%= FilesHtml %></div>
          <% if (T.Status == "Sent") { %>
          <div style="display:flex;gap:8px;flex-wrap:wrap;margin-top:16px">
            <form method="post" data-noload="1"><input type="hidden" name="action" value="extend" /><button class="btn-ghost" type="submit">Süreyi 7 gün uzat</button></form>
            <form method="post" data-noload="1" onsubmit="return confirm('Paylaşım iptal edilecek; tüm linkler kapanır ve dosyalar sunucudan silinir. Emin misiniz?')">
              <input type="hidden" name="action" value="cancel" /><button class="btn-ghost eq-del" type="submit">Paylaşımı iptal et</button></form>
          </div>
          <% } %>
        </div>
      </div>
    </div>
  </div>
</div>
</div>
<script>
document.addEventListener('click', function (e) {
  var b = e.target.closest ? e.target.closest('.tf-copy') : null; if (!b) return;
  var u = b.getAttribute('data-url'), done = function () { var o = b.textContent; b.textContent = 'Kopyalandı ✓'; setTimeout(function () { b.textContent = o; }, 1600); };
  if (navigator.clipboard && navigator.clipboard.writeText) navigator.clipboard.writeText(u).then(done, function () { prompt('Linki kopyalayın:', u); });
  else prompt('Linki kopyalayın:', u);
});
</script>
<script src="js/common.js?v=185"></script>
</body>
</html>
