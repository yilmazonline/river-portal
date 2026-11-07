<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Partner.aspx.cs" Inherits="PartnerPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>Portal</title>
<link href="https://fonts.googleapis.com/css2?family=Sora:wght@400;600;700;800&family=Inter:wght@400;500;600&display=swap" rel="stylesheet" />
<link rel="stylesheet" href="css/site.css?v=170" />
<link rel="icon" type="image/svg+xml" href="assets/RIVER_AI_badge.svg" />
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.css" />
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/themes/dark.css" />
<script src="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/flatpickr.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/flatpickr@4.6.13/dist/l10n/tr.js"></script>
</head>
<body>

<div id="loader">
  <div class="loader-badge"><img src="assets/RIVER_AI_badge.svg" alt="River AI" style="width:100%;height:100%" /></div>
  <div class="loader-track"><i></i></div>
  <div class="loader-text" data-i18n="loading">Portal Yükleniyor</div>
</div>

<div id="dash" data-gauge="<%= Gauge.ToString(System.Globalization.CultureInfo.InvariantCulture) %>" data-band="<%= Band.ToString(System.Globalization.CultureInfo.InvariantCulture) %>" data-culture="<%= Culture %>">
<div class="wrap">

  <nav class="nav">
    <div class="brand">
      <a href="<%= Nav.HomeHref %>" aria-label="Ana Sayfa" style="display:inline-flex"><img class="brand-logo" src="assets/RIVER_AI_white.svg" alt="River AI" /></a>
      <span class="sub" data-i18n="brand_sub">Asistan Portal</span>
    </div>
    <div class="links"><%= Nav.Links("home") %></div>
    <div class="right">
      <div class="lang">
        <button id="tr" data-lang="tr">TR</button>
        <button id="en" data-lang="en">EN</button>
      </div>
      <div class="bell<%= UnreadCount > 0 ? " has-unread" : "" %>">
        <button class="btn" id="bellBtn" type="button" aria-label="Bildirimler"><svg class="bell-ic" width="18" height="18" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2.2c.6 0 1.1.5 1.1 1.1v.6a6.4 6.4 0 0 1 5.3 6.3v3.5l1.5 2.5c.4.7-.1 1.6-.9 1.6H4.9c-.8 0-1.3-.9-.9-1.6l1.5-2.5v-3.5a6.4 6.4 0 0 1 5.3-6.3v-.6c0-.6.5-1.1 1.2-1.1z"/><path d="M9.6 19.4h4.8a2.4 2.4 0 0 1-4.8 0z"/></svg></button>
        <% if (UnreadCount > 0) { %><span class="badge" id="bellCount"><%= UnreadCount %></span><% } %>
        <div class="dropdown" id="bellMenu">
          <h4><span data-i18n="notif_title">Bildirimler</span><span class="act" id="markAll" data-i18n="notif_markall">Tümünü okundu işaretle</span></h4>
          <%= NotifHtml %>
        </div>
      </div>
      <div class="user">
        <div class="who">
          <div class="nm"><%= Server.HtmlEncode(UserName) %></div>
          <div class="rl" data-i18n="user_role">Partner Yöneticisi</div>
        </div>
        <div class="avatar"><%= Fmt.Initials(UserName) %></div>
        <div class="user-menu" id="userMenu">
          <div class="um-head">
            <div class="um-name"><%= Server.HtmlEncode(UserName) %></div>
            <div class="um-role"><%= Server.HtmlEncode(CompanyName) %></div>
          </div>
          <a href="Logout.aspx" class="danger" data-i18n="logout">Çıkış Yap</a>
        </div>
      </div>
    </div>
  </nav>

  <% if (IsImpersonating) { %>
  <div class="imp-banner">
    <span><b><%= Server.HtmlEncode(CompanyName) %></b> <%= Culture == "en" ? "· you are acting on behalf of this partner (Vendor)." : "· bu partner adına işlem yapıyorsunuz (Vendor)." %></span>
    <a href="Vendor.aspx"><%= Culture == "en" ? "Back to Vendor panel" : "Vendor paneline dön" %></a>
  </div>
  <% } %>
  <% if (FlashMsg.Length > 0) { %>
  <div class="flash-ok"><%= Server.HtmlEncode(FlashMsg) %></div>
  <% } %>
  <% if (FlashErr.Length > 0) { %>
  <div class="flash-err"><%= Server.HtmlEncode(FlashErr) %></div>
  <% } %>

  <%-- ===== Yeni ana sayfa (Komuta Merkezi) ===== --%>
  <a class="ph-2027 top lk" href="Tiers.aspx">
        <span class="ph-2027-h"><small><%= Culture == "en" ? "COMING · 2027" : "YAKINDA · 2027" %></small>
        <b><%= Culture == "en" ? "Partner Program" : "Partner Programı" %></b></span>
        <span class="ph-2027-body"><span class="ph-2027-p">Partner seviye ve hakediş sistemi 2027 yılında devreye alınacaktır. Satışlarınızı artırarak yeni döneme yüksek hak ediş seviyesi ile başlayabilirsiniz. Son bir yılda verdiğiniz siparişler listenizde yok veya eksik ise kanal satış temsilcinize bildiriniz.</span>
        <span class="ph-2027-p dim">Partner seviyeniz ayda bir defa, son 1 yıldaki satışlarınız ölçeğinde güncellenir. Ön ödeme ile seviyenizi yükselterek hak edişinizi yükseltebilirsiniz; ön ödemeli yükseltmelerde yeni seviye, anlaşmayı takip eden 60. günden sonra bir yıl boyunca geçerlidir. Şartlar için satış temsilcinizle iletişime geçin.</span>
        </span><span class="ph-2027-go"><%= Culture == "en" ? "Explore the program" : "Programı incele" %> &rarr;</span>
      </a>

  <section class="card ph-hero">
    <div class="ph-hero-glow" aria-hidden="true"></div>
    <div class="ph-hero-tx">
      <div class="pa-row"><%= AvatarHtml %><div class="pa-txt">
      <div class="ph-hero-top"><span class="ph-badge"><svg width="13" height="13" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2l2.9 6.6 7.1.6-5.4 4.7 1.6 7L12 17.3 5.8 21l1.6-7L2 9.2l7.1-.6z"/></svg><%= Server.HtmlEncode(BadgeLabel) %></span><span class="ph-date"><%= Server.HtmlEncode(TodayText) %></span></div>
      <h1><%= Server.HtmlEncode(Greeting) %></h1>
      <div class="ph-company"><%= Server.HtmlEncode(CompanyName) %></div>
      </div></div>
      <p class="ph-sum"><%= SummaryHtml %></p>
    </div>
    <nav class="ph-qa" aria-label="<%= Culture == "en" ? "Quick actions" : "Hızlı işlemler" %>">
      <button type="button" class="ph-qa-it primary lk" data-open="mdCust"><svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="9" cy="8" r="3.5"/><path d="M3 20c.7-3.6 3.2-5.6 6-5.6s5.3 2 6 5.6"/><path d="M18 8v6M15 11h6"/></svg><span><b data-i18n="custreg_title">Müşteri Kaydet</b><small><%= Culture == "en" ? "Protect the opportunity" : "Fırsatı koruma altına al" %></small></span></button>
      <button type="button" class="ph-qa-it lk" data-open="mdDemo"><svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="3"/><path d="M3 10h18M8 3v4M16 3v4"/></svg><span><b><%= Culture == "en" ? "Request a Demo" : "Demo Talep Et" %></b><small><%= Culture == "en" ? "Suggest a time, we schedule" : "Tarih öner, ekip planlasın" %></small></span></button>
      <a class="ph-qa-it lk" href="NewTicket.aspx"><svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 13a8 8 0 0 1 16 0v4a2 2 0 0 1-2 2h-1v-6h3M4 13v4a2 2 0 0 0 2 2h1v-6H4"/></svg><span><b><%= Culture == "en" ? "Support Request" : "Destek Talebi" %></b><small><%= Culture == "en" ? "Reach our experts" : "Uzmanlara ulaş" %></small></span></a>
      <a class="ph-qa-it lk" href="Training.aspx"><svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M2 9l10-5 10 5-10 5z"/><path d="M6 11v5c2 2 10 2 12 0v-5"/><path d="M22 9v6"/></svg><span><b>River Akademi</b><small><%= Culture == "en" ? "Trainings and certificates" : "Eğitimler ve sertifikalar" %></small></span></a>
    </nav>
  </section>

  <section class="ph-kpis" aria-label="<%= Culture == "en" ? "Overview" : "Göstergeler" %>"><%= KpiHtml %></section>

  <div class="ph-grid">
    <section class="card ph-flow">
      <div class="ph-h"><h2><%= Culture == "en" ? "Customer and demo pipeline" : "Müşteri ve demo akışı" %></h2><a href="#musteriler"><%= Culture == "en" ? "See all" : "Tümünü gör" %></a></div>
      <div class="ph-list ph-list-flow"><%= PipelineHtml %></div>
    </section>
    <aside class="ph-side">
      <section class="card ph-news"><h2><%= Culture == "en" ? "Latest updates" : "Son gelişmeler" %></h2><div class="ph-list ph-list-news"><%= FeedHtml %></div></section>
    </aside>
  </div>

  <%-- === 2027'de devreye alinacak: ciro/seviye/hak edis bloklari gizlendi. Geri almak icin bu yorumu (--%%> ve <%%--) kaldirin. ===
  <div class="card hero">
    <div class="gauge-wrap">
      <div class="gauge">
        <svg viewBox="0 0 240 240" width="240" height="240">
          <defs>
            <linearGradient id="arc" x1="0" y1="0" x2="1" y2="1">
              <stop offset="0%" stop-color="#E7A438" /><stop offset="100%" stop-color="#FFD879" />
            </linearGradient>
            <filter id="glow"><feGaussianBlur stdDeviation="4" result="b" />
              <feMerge><feMergeNode in="b" /><feMergeNode in="SourceGraphic" /></feMerge></filter>
          </defs>
          <circle cx="120" cy="120" r="100" fill="none" stroke="rgba(255,255,255,.07)" stroke-width="14"
            stroke-dasharray="471" stroke-dashoffset="0" transform="rotate(135 120 120)"
            stroke-linecap="round" pathLength="628" />
          <circle id="arcFill" cx="120" cy="120" r="100" fill="none" stroke="url(#arc)" stroke-width="14"
            transform="rotate(135 120 120)" stroke-linecap="round" filter="url(#glow)"
            pathLength="628" stroke-dasharray="628" stroke-dashoffset="628" />
          <g id="ticks"></g>
        </svg>
        <div class="center">
          <div class="amount"><%= RevM %><small>M ₺</small></div>
          <div class="lbl" data-i18n="hero_lbl">Yıllık Ciro</div>
        </div>
      </div>
    </div>

    <div class="hero-right">
      <div class="greet"><span data-i18n="greet">Hoş geldin,</span> <b><%= Server.HtmlEncode(UserName) %></b></div>
      <div class="company"><%= Server.HtmlEncode(CompanyName) %></div>
      <div class="tier-badge"><span class="ring"></span><span><%= Server.HtmlEncode(TierNameStr) %></span></div>
      <div class="metrics">
        <div class="metric">
          <div class="k" data-i18n="m_base">Lisans Satışı</div>
          <div class="v rate"><%= BaseCommStr %>%</div>
        </div>
        <div class="metric">
          <div class="k" data-i18n="m_rental">Kiralama Komisyonu</div>
          <div class="v rate"><%= RentalStr %>%</div>
        </div>
        <div class="metric">
          <div class="k" data-i18n="m_renewal">Versiyon Yenileme</div>
          <div class="v rate"><%= RenewalStr %>%</div>
        </div>
        <div class="metric">
          <div class="k" data-i18n="m_license">Lisans/Modül Yükseltme</div>
          <div class="v rate"><%= LicenseStr %>%</div>
        </div>
        <div class="metric wide">
          <div class="k" data-i18n="m_sla">SLA Seviyesi</div>
          <div class="v" style="font-size:16px"><%= Server.HtmlEncode(SlaStr) %></div>
        </div>
        <div class="metric wide">
          <div class="k" data-i18n="m_act">12 Ay Aktiflik</div>
          <div class="v"><span class="pill ok">● <span data-i18n="active">Aktif</span></span></div>
        </div>
      </div>
    </div>
  </div>

  <% if (HasNext) { %>
  <div class="card nextlvl collapsed" id="nextlvl">
    <div class="inner">
      <button type="button" class="nl-toggle" id="nlToggle" aria-label="Genişlet">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"></polyline></svg>
      </button>
      <div class="nl-top"><span class="nl-spark" data-i18n="nl_spark">◆ Sıradaki Hedef</span></div>
      <div class="nl-title"><span data-i18n="nl_almost">Bir üst seviyeye çok yakınsın</span> <span class="goal">· <%= Server.HtmlEncode(NextName) %></span></div>
      <div class="nl-sub">
        <% if (Culture == "en") { %>
          Unlock the next tier's privileges with your next big deal. Your commission rises to <%= NextBaseStr %>% and you get a dedicated account manager.
        <% } else { %>
          Bir sonraki büyük anlaşmanla üst seviyenin ayrıcalıklarını aç. Komisyonun %<%= NextBaseStr %>'ye çıkar, sana özel bir hesap yöneticisi atanır.
        <% } %>
      </div>

      <div class="nl-body">
      <div class="nl-bar-wrap">
        <div class="nl-bar"><i id="nlBar"></i></div>
        <div class="nl-bar-info">
          <span><b><%= CurMinM %>M ₺</b> · <%= Server.HtmlEncode(TierNameStr) %></span>
          <span class="nl-remain"><span data-i18n="nl_remain">Kalan:</span> <%= RemainingStr %> ₺</span>
          <span><b><%= NextMinM %>M ₺</b> · <%= Server.HtmlEncode(NextName) %></span>
        </div>
      </div>

      <div class="unlocks">
        <div class="unlock"><div class="ic">%</div><div>
          <div class="u-t"><%= Culture == "en" ? "Base commission" : "Taban komisyon" %> <%= BaseCommStr %>% &rarr; <%= NextBaseStr %>%</div>
          <div class="u-b" data-i18n="u1_b">Her anlaşmada daha yüksek hakediş</div>
        </div></div>
        <div class="unlock"><div class="ic">★</div><div>
          <div class="u-t" data-i18n="u2_t">Özel Hesap Yöneticisi</div>
          <div class="u-b" data-i18n="u2_b">Stratejik SLA ve birebir destek hattı</div>
        </div></div>
        <div class="unlock"><div class="ic">◈</div><div>
          <div class="u-t" data-i18n="u3_t">Ortak Pazarlama Fonu (MDF)</div>
          <div class="u-b" data-i18n="u3_b">Kampanyalarına River bütçe desteği</div>
        </div></div>
        <div class="unlock"><div class="ic">⚡</div><div>
          <div class="u-t" data-i18n="u4_t">Beta Özelliklere Erken Erişim</div>
          <div class="u-b" data-i18n="u4_b">Yeni modülleri herkesten önce dene</div>
        </div></div>
      </div>

      <div class="nl-cta">
        <button class="btn-brand" data-i18n="nl_cta">Anlaşma Simüle Et</button>
        <span class="nl-hint" data-i18n="nl_hint">1.5M ₺+ tek anlaşmada ekstra Mega Deal bonusu kazanırsın.</span>
      </div>
      </div>
    </div>
  </div>
  <% } %>
  --%>

  <section class="card ph-sec" id="musteriler">
    <div class="section-h ph-sec-h"><span data-i18n="myregs_title">Kayıtlı Müşterilerim</span><button type="button" class="btn-cyan ph-sec-btn" data-open="mdCust">+ <span data-i18n="custreg_title">Müşteri Kaydet</span></button></div>
    <div class="ph-regs"><%= CustomerRegsHtml %></div>
  </section>

  <div class="cols">
    <section class="card ph-sec" id="satislar">
      <div class="section-h"><span data-i18n="ledger_title">Sipariş & Anlaşma Defteri</span></div>
      <div class="tbl-scroll"><table>
        <thead><tr>
          <th data-i18n="th_customer">Müşteri</th>
          <th data-i18n="th_amount">Tutar</th>
          <th data-i18n="th_rate">Oran</th>
          <th data-i18n="th_comm">Hakediş</th>
          <th data-i18n="th_status">Durum</th>
        </tr></thead>
        <tbody><%= DealsHtml %></tbody>
      </table></div>
      <% if (IsVendorActing) { %>
      <div class="foot">
        <div class="foot-title" data-i18n="deal_new_vendor">Partner Adına Yeni Anlaşma</div>
        <form method="post" action="BookDeal.aspx">
          <input class="input" name="customer" data-i18n-ph="ph_customer" placeholder="Müşteri adı" required="required" />
          <input class="input" name="title" data-i18n-ph="ph_dealtitle" placeholder="Anlaşma başlığı (opsiyonel)" />
          <input class="input" name="amount" type="text" inputmode="decimal" data-i18n-ph="ph_amount" placeholder="Tutar (₺)" required="required" />
          <input class="input" name="rate" type="text" inputmode="decimal" data-i18n-ph="ph_rate" placeholder="Hakediş %" required="required" />
          <input class="input" name="commission" type="text" inputmode="decimal" data-i18n-ph="ph_commission" placeholder="Hakediş (₺)" required="required" />
          <button class="btn-cyan" style="width:100%;padding:11px" type="submit" data-i18n="deal_book">Anlaşmayı Kaydet</button>
        </form>
      </div>
      <% } %>
    </section>

    <section class="card ph-sec" id="demolar">
      <div class="section-h ph-sec-h"><span data-i18n="demo_title">Demo & Teklif Merkezi</span><button type="button" class="btn-cyan ph-sec-btn" data-open="mdDemo"><%= Culture == "en" ? "+ Request a Demo" : "+ Demo Talep Et" %></button></div>
      <%= DemosHtml %>
    </section>
  </div>

  <%-- Formlar: dugmelerle acilan pencereler (alanlar ve hedefler degismedi) --%>
  <div class="pw-modal ph-modal" id="mdCust" role="dialog" aria-modal="true" aria-labelledby="mdCustT"><div class="pw-box ph-box">
    <button type="button" class="pw-x" data-close="1" aria-label="Kapat">&times;</button>
    <h3 id="mdCustT" data-i18n="custreg_title">Müşteri Kaydet</h3>
    <div class="reg-note-box" data-i18n="custreg_note">Müşteriyi kaydetmezseniz aynı müşteriye başka bir iş ortağı teklif verebilir; müşteri kaybı ve hakediş kaybı yaşanabilir, öncelik hakkınız oluşmaz. Kayıt 30 gün koruma sağlar.</div>
      <form method="post" action="RegisterCustomer.aspx" class="ph-form">
        <input class="input" name="customer" data-i18n-ph="ph_custname" placeholder="Müşteri adı" required="required" />
        <input class="input" name="contact" data-i18n-ph="ph_contact" placeholder="Kontak kişisi" />
        <input class="input" name="contactemail" type="email" data-i18n-ph="ph_contactemail" placeholder="Kontak e-posta" />
        <input class="input" name="contactphone" type="tel" data-i18n-ph="ph_contactphone" placeholder="Kontak telefon" />
        <label class="dt-label" data-i18n="lbl_service">Hizmet Türü / Kaynak</label>
        <select class="input st-select" name="service">
          <option>ERP Danışmanlığı</option>
          <option>Dış Kaynak</option>
          <option>Referans</option>
          <option>Fuar / Etkinlik</option>
          <option>Web / Dijital Pazarlama</option>
          <option>Soğuk Arama</option>
          <option>Mevcut Müşteri</option>
          <option>Diğer</option>
        </select>
        <label class="dt-label"><span data-i18n="lbl_temp">Satış Sıcaklığı</span>: <b id="tempVal">50</b>/100</label>
        <input type="range" min="1" max="100" value="50" name="temperature" id="tempRange" class="temp-range" />
        <button class="btn-cyan" style="width:100%;padding:11px;margin-top:12px" type="submit" data-i18n="custreg_submit">Müşteriyi Kaydet</button>
      </form>
  </div></div>
  <div class="pw-modal ph-modal" id="mdDemo" role="dialog" aria-modal="true" aria-labelledby="mdDemoT"><div class="pw-box ph-box">
    <button type="button" class="pw-x" data-close="1" aria-label="Kapat">&times;</button>
    <h3 id="mdDemoT"><%= Culture == "en" ? "Request a Demo" : "Demo Talep Et" %></h3>
    <form method="post" action="SubmitDemo.aspx">
          <input class="input" name="client" data-i18n-ph="ph_client" placeholder="Müşteri adı" required="required" />
          <input class="input" name="module" data-i18n-ph="ph_module" placeholder="Ürün modülü" required="required" />
          <label class="dt-label" data-i18n="dt_pref">Tercih edilen tarih &amp; saat</label>
          <input class="input dtpick" id="preferred" name="preferred" type="text" readonly="readonly" required="required" />
          <label class="dt-label" data-i18n="dt_alt1">Alternatif 1 (uygun olunmazsa)</label>
          <input class="input dtpick" id="alt1" name="alt1" type="text" readonly="readonly" />
          <label class="dt-label" data-i18n="dt_alt2">Alternatif 2</label>
          <input class="input dtpick" id="alt2" name="alt2" type="text" readonly="readonly" />
          <button class="btn-cyan" style="width:100%;padding:11px" type="submit" data-i18n="demo_submit">Talep Gönder</button>
        </form>
  </div></div>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
</div>

<script src="js/common.js?v=185"></script>
<script src="js/partner.js?v=6"></script>
<script>
(function () {
  var last = null;
  function open(id) { var m = document.getElementById(id); if (!m) return; last = document.activeElement; m.classList.add('open'); var f = m.querySelector('input:not([type=hidden]):not([readonly]), select'); if (f) f.focus(); }
  function close(m) { m.classList.remove('open'); if (last && last.focus) last.focus(); }
  document.addEventListener('click', function (e) {
    var o = e.target.closest ? e.target.closest('[data-open]') : null; if (o) { open(o.getAttribute('data-open')); return; }
    var x = e.target.closest ? e.target.closest('[data-close]') : null; if (x) close(x.closest('.ph-modal'));
  });
  [].forEach.call(document.querySelectorAll('.ph-modal'), function (m) {
    var down = false;
    m.addEventListener('pointerdown', function (e) { down = e.target === m; });
    m.addEventListener('click', function (e) { if (down && e.target === m) close(m); down = false; });
  });
  document.addEventListener('keydown', function (e) { if (e.key !== 'Escape') return; var m = document.querySelector('.ph-modal.open'); if (m) close(m); });
  if (/[?&](err|ok)=/.test(location.search) && /err=(customer|demo)/.test(location.search)) open(/demo/.test(location.search) ? 'mdDemo' : 'mdCust');
})();
</script>
</body>
</html>
