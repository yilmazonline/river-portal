<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Forum.aspx.cs" Inherits="ForumPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Forum</title>
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
      <span class="sub">Asistan Portal</span>
    </div>
    <div class="links"><%= Nav.Links("forum") %></div>
    <div class="right">
      <div class="user">
        <div class="who"><div class="nm"><%= Server.HtmlEncode(UserName) %></div><div class="rl"><%= Server.HtmlEncode(CompanyName) %></div></div>
        <div class="avatar"><%= Fmt.Initials(UserName) %></div>
        <div class="user-menu" id="userMenu">
          <div class="um-head"><div class="um-name"><%= Server.HtmlEncode(UserName) %></div></div>
          <a href="Logout.aspx" class="danger">Çıkış Yap</a>
        </div>
      </div>
    </div>
  </nav>

  <div class="fo-wrap">
    <nav class="fo-left" aria-label="Forum">
      <button type="button" class="fo-tab on" data-tab="feed"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 12l8-8 8 8M6 10v10h12V10"/></svg>Akış</button>
      <button type="button" class="fo-tab" data-tab="announcements"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 11v2a1 1 0 0 0 1 1h2l5 4V6L6 10H4a1 1 0 0 0-1 1zM16 8a5 5 0 0 1 0 8"/></svg>Duyurular</button>
      <button type="button" class="fo-tab" data-tab="saved"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M6 3h12v18l-6-4-6 4z"/></svg>Kaydettiklerim</button>
      <span class="fo-tab soon" aria-disabled="true"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M9.1 9a3 3 0 1 1 4.2 2.7c-.8.4-1.3 1.2-1.3 2v.3M12 17h.01"/></svg>Sorular<i>Yakında</i></span>
      <button type="button" class="fo-tab" id="foRulesBtn"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 3l8 3v6c0 4.5-3.4 8.3-8 9-4.6-.7-8-4.5-8-9V6z"/><path d="M9 12l2 2 4-4"/></svg>Topluluk kuralları</button>
      <section class="fo-trend" aria-labelledby="foTrendT"><h4 id="foTrendT">Gündemdekiler</h4><div id="foTrend"><span class="fo-trend-empty">Yükleniyor…</span></div></section>
      <span class="fo-modhint" id="foModHint" hidden title="Kullanıcılar ekranından hesabınıza Forum Admini yetkisi verilmeli"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 21V4h11l-2 4 2 4H5"/></svg><span>Moderasyon<small>Forum Admini yetkisi gerekli</small></span></span>
      <button type="button" class="fo-modbtn" id="foModBtn"<%= IsMod ? "" : " hidden" %>><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 21V4h11l-2 4 2 4H5"/></svg>Moderasyon <b id="foModCnt">0</b></button>
    </nav>

    <main class="fo-feed" id="foFeed" aria-live="polite">
      <div class="fo-banner" id="foBanner" role="status" hidden></div>
      <form class="fo-compose" id="foCompose" autocomplete="off" data-noload="1">
        <span class="fo-ava" id="foMeAva" aria-hidden="true"></span>
        <div class="fo-c-main">
          <textarea id="foText" rows="2" maxlength="4000" placeholder="Bir soru sorun, deneyiminizi paylaşın…" aria-label="Gönderi metni"></textarea>
          <div class="fo-c-embed" id="foEmbedPrev" hidden></div>
          <div class="fo-c-files" id="foFiles"></div>
          <div class="fo-c-msg" id="foMsg" role="alert"></div>
          <div class="fo-c-bar">
            <div class="fo-c-tools">
              <label class="fo-tool" title="Görsel ekle (4 adede kadar, her biri 5 MB)"><input type="file" id="foImg" accept="image/jpeg,image/png,image/gif,image/webp" multiple hidden /><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 5h16v14H4zM4 15l4-4 4 4 3-3 5 5"/></svg>Görsel</label>
              <label class="fo-tool" title="Video ekle (25 MB'a kadar MP4 / WEBM)"><input type="file" id="foVid" accept="video/mp4,video/webm" hidden /><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 6h13v12H3zM16 10l5-3v10l-5-3"/></svg>Video</label>
              <button type="button" class="fo-tool" id="foLink" title="YouTube / Vimeo bağlantısı"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M10 14a4 4 0 0 0 5.7 0l3-3a4 4 0 0 0-5.7-5.7l-1 1M14 10a4 4 0 0 0-5.7 0l-3 3a4 4 0 0 0 5.7 5.7l1-1"/></svg>Bağlantı</button>
              <% if (IsMod) { %><label class="fo-chk"><input type="checkbox" id="foAnn" /> Duyuru</label><label class="fo-chk"><input type="checkbox" id="foPin" /> Sabitle</label><% } %>
            </div>
            <span class="fo-count" id="foCount" aria-hidden="true"></span>
            <button type="submit" class="btn-cyan fo-send" id="foSend" disabled>Paylaş</button>
          </div>
          <div class="fo-c-hint">Video 25 MB'a kadar · daha büyük videolar için YouTube ya da Vimeo bağlantısı ekleyin</div>
        </div>
      </form>
      <div class="fo-tagbar" id="foTagBar" hidden></div>
      <div id="foList"></div>
      <button type="button" class="btn-ghost fo-more-btn" id="foMore" hidden>Daha fazla göster</button>
    </main>

  </div>

  <%-- Topluluk kurallari: ilk giriste onay zorunlu; sonra menuden okunur --%>
  <div class="pw-modal fo-rules-modal" id="foRules" role="dialog" aria-modal="true" aria-labelledby="foRulesT"><div class="pw-box fo-rules-box">
    <button type="button" class="pw-x" id="foRulesX" aria-label="Kapat">&times;</button>
    <h3 id="foRulesT">River Forum · Topluluk Kuralları</h3>
    <p class="fo-rules-lead">River Forum, River AI kullanıcılarının ve iş ortaklarının soru sorduğu, deneyim ve iyi uygulama paylaştığı bir bilgi topluluğudur. Paylaşımlarınızı müşteriler, iş ortakları ve Fol Yazılım ekibi görebilir. Lütfen aşağıdaki kurallara uyun.</p>
    <div class="fo-rules-body">
      <section><h4>1. Kişisel veriler (KVKK)</h4><ul>
        <li>Kendinize ya da başkalarına ait <b>T.C. kimlik numarası, telefon, e-posta, adres, IBAN, sağlık bilgisi</b> gibi kişisel verileri paylaşmayın.</li>
        <li>Ekran görüntüsü ve videolarda <b>müşteri adları, çalışan bilgileri, kayıt içerikleri ve rapor verileri</b> görünmemeli; paylaşmadan önce bu alanları kapatın ya da bulanıklaştırın.</li>
        <li>Üçüncü kişilerin açık rızası olmadan onlara ait bilgi, fotoğraf ya da yazışma paylaşmayın.</li></ul></section>
      <section><h4>2. Gizli ve ticari bilgiler</h4><ul>
        <li><b>Şifre, API anahtarı, bağlantı cümlesi, sunucu adresi</b> ve lisans bilgisi paylaşmayın.</li>
        <li>Kurumunuza ya da müşterinize ait fiyat, sözleşme ve iç süreç bilgilerini yalnız yetkiniz varsa ve anonimleştirerek paylaşın.</li></ul></section>
      <section><h4>3. Uygunsuz içerik</h4><ul>
        <li>Hakaret, aşağılama, taciz, tehdit ve ayrımcılık içeren paylaşımlar yasaktır.</li>
        <li>Müstehcen, şiddet içeren ya da yasa dışı içerik paylaşılamaz.</li>
        <li>İzinsiz reklam, toplu mesaj (spam) ve konu dışı tanıtım yapılmaz.</li>
        <li>Telif hakkı size ait olmayan içerikleri kaynak ve izin olmadan paylaşmayın.</li></ul></section>
      <section><h4>4. Saygılı ve yapıcı iletişim</h4><ul>
        <li>Farklı görüşlere saygı gösterin; kişileri değil fikirleri tartışın.</li>
        <li>Doğruluğundan emin olmadığınız bilgileri kesinmiş gibi sunmayın.</li></ul></section>
      <section><h4>5. Destek talepleri</h4><ul>
        <li>Forum bir destek kanalı değildir. Acil ya da size özel sorunlar için <b>destek talebi</b> oluşturun; talebinizin ayrıntılarını foruma yazmayın.</li></ul></section>
      <section><h4>6. Moderasyon</h4><ul>
        <li>Fol Yazılım forum yöneticileri kurallara aykırı içerikleri gizleyebilir ya da kaldırabilir, paylaşım hakkını süreli ya da süresiz kısıtlayabilir. Bu işlemler kayıt altına alınır ve içerik sahibine bildirilir.</li>
        <li>Kurallara aykırı bir içerik gördüğünüzde gönderinin <b>⋯</b> menüsünden “Şikayet et” seçeneğini kullanın.</li>
        <li>Paylaşılan içeriğin hukuki sorumluluğu paylaşan kişiye aittir.</li></ul></section>
      <section class="fo-rules-tips"><h4>İpuçları</h4><ul>
        <li><b>#etiket</b> ekleyin (#Form, #Akış, #Katalog): gönderiniz ilgili konuda bulunur ve gündemde görünür.</li>
        <li>YouTube ya da Vimeo bağlantısını yapıştırın; oynatıcı kendiliğinden oluşur. 25 MB'ı aşan videolar için bu yolu kullanın.</li>
        <li>Soru sorarken ürün sürümünü, ne yapmak istediğinizi ve ne olduğunu kısaca yazın; cevap daha hızlı gelir.</li>
        <li>Kişilerin adına ya da profil resmine tıklayarak profillerini, rozetlerini ve sertifikalarını görebilirsiniz.</li></ul></section>
    </div>
    <form class="fo-consent" id="foConsent" data-noload="1" hidden>
      <label class="fo-consent-chk"><input type="checkbox" id="foAgree" /><span>Topluluk kurallarını <b>okudum, anladım</b>. Paylaşımlarımda KVKK kapsamında kişisel veri, gizli bilgi ve uygunsuz içerik bulunmayacağını kabul ediyorum.</span></label>
      <div class="fo-consent-act"><a class="btn-ghost" href="<%= Nav.HomeHref %>">Şimdi değil</a><button type="submit" class="btn-cyan" id="foAgreeBtn" disabled>Kabul ediyorum, devam et</button></div>
    </form>
  </div></div>
  <div class="fo-lb" id="foLb" hidden role="dialog" aria-modal="true" aria-label="Görsel"><button type="button" class="fo-lb-x" aria-label="Kapat">&times;</button><img alt="" /></div>
  <div class="pw-modal fo-prof-modal" id="foProf" role="dialog" aria-modal="true" aria-labelledby="foProfT"><div class="pw-box fo-prof-box">
    <button type="button" class="pw-x" data-close="1" aria-label="Kapat">&times;</button>
    <div id="foProfHead"></div><div id="foProfAch"></div>
    <h4 class="fo-prof-ph">Gönderiler</h4><div id="foProfList" class="fo-prof-list"></div>
    <button type="button" class="btn-ghost fo-more-btn" id="foProfMore" hidden>Daha fazla göster</button>
  </div></div>
  <div class="pw-modal" id="foModModal" role="dialog" aria-modal="true" aria-labelledby="foModT"><div class="pw-box fo-mod-box">
    <button type="button" class="pw-x" data-close="1" aria-label="Kapat">&times;</button>
    <h3 id="foModT">Moderasyon</h3>
    <nav class="fo-mtabs" aria-label="Moderasyon"><button type="button" class="fo-mtab on" data-mt="reports">Bildirimler</button><button type="button" class="fo-mtab" data-mt="restr">Kısıtlamalar</button><button type="button" class="fo-mtab" data-mt="log">Kayıt</button><button type="button" class="fo-mtab" data-mt="set">Ayarlar</button></nav>
    <div class="fo-mpane" data-mp="reports"><p class="fo-mod-sub">Bildirilen içerikler. Gizlerken yazdığınız açıklama içerik sahibine bildirim olarak gider.</p><div id="foModList"></div></div>
    <div class="fo-mpane" data-mp="restr" hidden><p class="fo-mod-sub">Paylaşım hakkı kısıtlanan kişiler. Kısıtlamayı kaldırdığınızda kişiye bildirim gider.</p><div id="foRestrList"></div></div>
    <div class="fo-mpane" data-mp="log" hidden><p class="fo-mod-sub">Silinen ve gizlenen içerikler, kısıtlamalar ve forum ayarı değişiklikleri. Kayıt kalıcıdır; içerik silinse de okunur.</p><div id="foLogList"></div><button type="button" class="btn-ghost fo-more-btn" id="foLogMore" hidden>Daha fazla göster</button></div>
    <div class="fo-mpane" data-mp="set" hidden><form id="foSetForm" data-noload="1">
      <label class="fo-switch"><input type="checkbox" id="foSetClosed" /><span><b>Forumu yeni paylaşımlara kapat</b><small>Herkes okumaya devam eder; yalnız forum adminleri gönderi ve yorum yazabilir.</small></span></label>
      <label class="fo-ask-l"><span class="fo-ask-t">Bilgi notu <i>(isteğe bağlı, şeritte görünür)</i></span><input type="text" id="foSetNote" maxlength="500" placeholder="Örn. Bakım çalışması nedeniyle 18:00'e kadar" /></label>
      <div class="fo-ask-act"><button type="submit" class="btn-cyan">Kaydet</button></div>
    </form></div>
  </div></div>
</div>
</div>
<script>window.FORUM = { me: <%= MeJson %>, mod: <%= IsMod ? "true" : "false" %>, focus: <%= FocusPost %>, consent: <%= NeedsConsent ? "true" : "false" %> };</script>
<script src="js/common.js?v=186"></script>
<script src="js/forum.js?v=8"></script>
</body>
</html>
