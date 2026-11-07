<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Training.aspx.cs" Inherits="TrainingPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River Akademi</title>
<link href="https://fonts.googleapis.com/css2?family=Sora:wght@400;600;700;800&family=Inter:wght@400;500;600&display=swap" rel="stylesheet" />
<link rel="stylesheet" href="css/site.css?v=170" />
<link rel="icon" type="image/svg+xml" href="assets/RIVER_AI_badge.svg" />
</head>
<body class="ec-fullpage">
<div id="dash" data-culture="<%= Culture %>">
<div class="ec-page">

  <nav class="nav ec-nav">
    <div class="brand">
      <a href="<%= IsVendor ? "Vendor.aspx" : (IsEditorOnly ? "Training.aspx" : (IsCustomer ? "Tickets.aspx" : "Partner.aspx")) %>" style="display:inline-flex" aria-label="Ana Sayfa">
        <a href="<%= Nav.HomeHref %>" aria-label="Ana Sayfa" style="display:inline-flex"><img class="brand-logo" src="assets/RIVER_AI_white.svg" alt="River AI" /></a>
      </a>
      <span class="sub" data-i18n="brand_sub">Asistan Portal</span>
    </div>
    <div class="links"><%= Nav.Links("edu") %></div>
    <div class="right">
      <div class="lang"><button id="tr" data-lang="tr">TR</button><button id="en" data-lang="en">EN</button></div>
      <div class="user">
        <div class="who"><div class="nm"><%= Server.HtmlEncode(UserName) %></div><div class="rl"><%= Server.HtmlEncode(CompanyName) %></div></div>
        <div class="avatar"><%= Fmt.Initials(UserName) %></div>
        <div class="user-menu" id="userMenu">
          <div class="um-head"><div class="um-name"><%= Server.HtmlEncode(UserName) %></div></div>
          <a href="Logout.aspx" class="danger" data-i18n="logout">Çıkış Yap</a>
        </div>
      </div>
    </div>
  </nav>

  <div id="ec-home"></div>
  <% if (FlashMsg.Length > 0) { %><div class="flash-ok" style="margin:16px 40px 0"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>
  <% if (FlashErr.Length > 0) { %><div class="flash-err" style="margin:16px 40px 0"><%= Server.HtmlEncode(FlashErr) %></div><% } %>
  <%= HeroHtml %>

  <div class="ec-body">
    <aside class="ec-tree ac-side">
      <nav class="ac-card ac-tools" aria-label="Akademi araçları">
        <button type="button" class="ac-tool" id="btnHist"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="8.5"/><path d="M12 7.5V12l3 2"/></svg><span>Geçmişim</span></button>
        <% if (!(Auth.IsPartnerSide || Auth.IsCustomerSide)) { %><a class="ac-tool" href="Exams.aspx" title="<%= CanEdit ? "Sınav oluştur ve yönet" : "Sınavları görüntüle" %>"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M9 4h6l1 3h3v13H5V7h3z"/><path d="M9 13l2 2 4-4"/></svg><span>Sınavlar</span><% if (PendingRequests > 0) { %><i class="ac-cnt alert" title="Bekleyen yeniden giriş talebi"><%= PendingRequests %></i><% } %></a><% } %>
        <% if (Auth.IsPartnerSide || Auth.IsCustomerSide) { %><a class="ac-tool" href="Certificates.aspx"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="9" r="5"/><path d="M9 13.5L8 21l4-2 4 2-1-7.5"/></svg><span>Sertifika</span><% if (CertCount > 0) { %><i class="ac-cnt"><%= CertCount %></i><% } %></a><% } %>
        <button type="button" class="ac-tool" id="btnSuggest"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 3a6 6 0 0 0-3.5 10.9V17h7v-3.1A6 6 0 0 0 12 3z"/><path d="M9.5 21h5"/></svg><span>Öner</span></button>
        <% if (CanEdit) { %><button type="button" class="ac-tool" id="btnEditor"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 20h4L19 9l-4-4L4 16z"/><path d="M13.5 6.5l4 4"/></svg><span>Editör</span></button><% } %>
      </nav>

      <section class="ac-card ac-me">
        <div class="ac-me-h">
          <span class="ac-ava"><% if (MyAvatar.Length > 0) { %><img src="<%= Server.HtmlEncode(MyAvatar) %>" alt="" /><% } else { %><%= Fmt.Initials(UserName) %><% } %></span>
          <span class="ac-me-tx"><b><%= Server.HtmlEncode(UserName) %></b>
            <small class="ac-streak<%= MyWeekDays > 0 ? " on" : "" %>"><svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 3c1 3.5 5 5.5 5 10a5 5 0 0 1-10 0c0-2.4 1.3-3.6 2.4-4.8C10.3 9.4 11.6 7 12 3z"/></svg><%= MyWeekDays > 0 ? "Bu hafta " + MyWeekDays + " gün eğitim izlediniz" : "Bu hafta ilk videonuzu izleyin" %></small></span>
        </div>
        <button type="button" class="ac-path" id="lpFeat" style="display:none" aria-label="Sıfırdan Uzmanlığa yolunu aç">
          <span class="ac-ring" id="apRing"></span>
          <span class="ac-path-tx"><small>SIFIRDAN UZMANLIĞA</small><b id="apTitle"></b><em id="apSub"></em></span>
        </button>
        <button type="button" class="ac-go hist-open" id="apGo" style="display:none">
          <span class="ac-go-ring" id="apGoRing"></span>
          <span class="ac-go-tx"><small id="apGoK">DEVAM ET</small><b id="apGoT"></b><span class="ac-go-bar"><i id="apGoBar"></i></span></span>
        </button>
        <div class="ac-stats">
          <span><b id="apBadgeN">0</b><small>rozet</small></span>
          <span><b><%= CertCount %></b><small>sertifika</small></span>
          <span><b><%= MyWatch %></b><small>izleme</small></span>
        </div>
      </section>

      <section class="ac-card ac-badges" id="lpBadges" style="display:none">
        <div class="ac-h"><b>Rozetlerim</b><span id="lpBadgeCnt"></span></div>
        <div class="ac-next" id="apNext"></div>
        <div class="lb-grid" id="lpBadgeGrid"></div>
        <% if ((Auth.IsPartnerSide || Auth.IsCustomerSide) && CertBadgesHtml.Length > 0) { %>
        <div class="ac-sub">Sertifika rozetleri</div>
        <div class="ec-badges ac-certb"><%= CertBadgesHtml %></div>
        <% } %>
        <button type="button" class="lp-share" id="lpShare" role="switch" aria-checked="true" style="display:none" title="Açıkken, kazandığınız rozetler firmanızdaki kişilere bildirim olarak duyurulur ve Takım Arkadaşlarım listesinde görünür">
          <span>Başarılarımı firmamla paylaş</span><i class="um-sw on"></i>
        </button>
      </section>

      <section class="ac-card ac-tabs">
        <div class="ac-seg" id="acSeg" role="tablist" aria-label="Sol menü">
          <button type="button" role="tab" aria-selected="true" data-t="cat" class="on">Kategoriler</button>
          <button type="button" role="tab" aria-selected="false" data-t="team" id="acSegTeam" style="display:none">Takım</button>
        </div>
        <div class="ac-pane on" data-p="cat">
          <div class="wl-side" id="wlSide"></div>
          <%= TreeHtml %>
          <% if (DocTreeHtml.Length > 0) { %>
          <div class="ec-tree-sep">Dokümanlar</div>
          <%= DocTreeHtml %>
          <% } %>
        </div>
        <div class="ac-pane" data-p="team">
          <button type="button" class="tm-side" id="btnTeam" style="display:none" aria-label="Takım Arkadaşlarım">
            <span class="tm-side-in">
              <span class="tm-side-h"><b>Takım Arkadaşlarım</b><span class="tm-side-go" aria-hidden="true">›</span></span>
              <span class="tm-side-row"><span class="tm-faces" id="tmFaces"></span><span class="tm-side-sum" id="tmSideSum"></span></span>
              <span class="tm-side-news" id="tmSideNews"></span>
            </span>
          </button>
        </div>
      </section>
      <script>
        (function () {
          var seg = document.getElementById('acSeg'); if (!seg) return;
          seg.addEventListener('click', function (e) {
            var b = e.target.closest ? e.target.closest('button[data-t]') : null; if (!b) return;
            [].forEach.call(seg.querySelectorAll('button[data-t]'), function (x) { var on = x === b; x.classList.toggle('on', on); x.setAttribute('aria-selected', on ? 'true' : 'false'); });
            [].forEach.call(seg.parentNode.querySelectorAll('.ac-pane'), function (p) { p.classList.toggle('on', p.getAttribute('data-p') === b.getAttribute('data-t')); });
          });
        })();
      </script>
    </aside>
    <main class="ec-main" id="ecMain">
      <section class="wl-view" id="wlView" style="display:none"></section>


      <div class="ec-toolbar">
        <div class="ec-search">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><circle cx="11" cy="11" r="7"/><path d="M21 21l-4.3-4.3"/></svg>
          <input type="search" id="ecSearch" name="ecq_river" placeholder="Eğitim içeriklerinde ara (başlık, açıklama)..." autocomplete="off" autocorrect="off" autocapitalize="off" spellcheck="false" aria-label="Eğitim içeriklerinde ara" readonly="readonly" data-lpignore="true" data-form-type="other" data-1p-ignore="true" />
        </div>
        <div class="ec-filters" id="ecFilters">
          <button type="button" class="ec-fil on" data-f="all">Tümü</button>
          <button type="button" class="ec-fil" data-f="video">Videolar</button>
          <button type="button" class="ec-fil" data-f="doc">Dokümanlar</button>
        </div>
      </div>
      <div class="ec-noresults" id="ecNoRes" style="display:none">
        Aramanızla eşleşen içerik bulunamadı.
        <button type="button" class="btn-ghost" id="ecClear" style="margin-left:10px;padding:7px 14px">Aramayı Temizle</button>
      </div>

      <%= ShelfHtml %>
      <section class="ac-band" id="acBand" hidden aria-labelledby="acBandT">
        <div class="ac-band-top"><button type="button" class="ac-back" id="acBack"><svg width="16" height="16" viewBox="0 0 16 16" aria-hidden="true"><path d="M10 3 L5 8 L10 13" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>Tüm kategoriler</button></div>
        <div class="ac-band-in">
          <div class="ac-band-tx"><span class="ac-band-k" id="acBandK"></span><h1 class="ac-band-t" id="acBandT"></h1><div class="ac-band-subs" id="acBandS"></div></div>
          <img class="ac-band-img" id="acBandImg" alt="" hidden />
          <span class="ac-band-ico" id="acBandIco" hidden><span class="ico-g"><svg aria-hidden="true"><use href=""></use></svg><i class="ico-u" aria-hidden="true" hidden></i></span></span>
        </div>
        <div class="ac-band-bar">
          <div class="ac-band-tabs" role="tablist" aria-label="İçerik türü">
            <button type="button" role="tab" id="acTabV" data-tab="v" aria-selected="true">Videolar <b></b></button>
            <button type="button" role="tab" id="acTabD" data-tab="d" aria-selected="false">Dokümanlar <b></b></button>
          </div>
          <label class="ac-bsearch"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" aria-hidden="true"><circle cx="11" cy="11" r="7"/><path d="M20 20l-3.5-3.5"/></svg>
            <span class="sr-only">Bu kategoride ara</span><input type="search" id="acCatQ" placeholder="Bu kategoride ara" autocomplete="off" /></label>
        </div>
      </section>
      <div class="ac-empty" id="acEmpty" hidden></div>

      <div class="ec-videos">
        <%= RowsHtml %>
      </div>

      <div class="ec-docs">
        <h2 class="ec-section-title">Dokümanlar</h2>
        <%= DocsHtml %>
      </div>

      <div class="app-footer" style="margin-top:30px">
        <img src="assets/fol_logo.png" alt="Fol Yazılım" />
        <div class="fname">Fol Yazılım A.Ş.</div>
      </div>
    </main>
  </div>
</div>
</div>

<div class="vid-lightbox" id="vidLightbox">
  <div class="vlb-inner">
    <button type="button" class="vlb-close" id="vlbClose" aria-label="Kapat">&times;</button>
    <div class="vlb-player" id="vlbPlayer">
    <div class="vlb-frame" id="vlbFrame"></div>
    <div class="vlb-ctl" id="vlbCtl">
      <div class="vc-prog" id="vcProg"><div class="vc-buf" id="vcBuf"></div><div class="vc-played" id="vcPlayed"></div><div class="vc-thumb" id="vcThumb"></div><div class="vc-tip" id="vcTip">0:00</div></div>
      <div class="vc-row">
        <button type="button" class="vc-btn vc-play" id="vcPlay" aria-label="Oynat / Duraklat" title="Oynat / Duraklat (Boşluk)"><span class="i-play"><svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M8 5.14v13.72a1 1 0 0 0 1.52.85l10.6-6.86a1 1 0 0 0 0-1.7L9.52 4.29A1 1 0 0 0 8 5.14z"/></svg></span><span class="i-pause"><svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><rect x="6" y="5" width="4.2" height="14" rx="1.2"/><rect x="13.8" y="5" width="4.2" height="14" rx="1.2"/></svg></span></button>
        <button type="button" class="vc-btn" id="vcBack" aria-label="10 saniye geri" title="10 saniye geri (←)"><svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M12 5V2L7 6l5 4V7a6 6 0 1 1-6 6H4a8 8 0 1 0 8-8z"/><text x="12" y="16.2" font-size="6.4" font-weight="700" text-anchor="middle" font-family="sans-serif">10</text></svg></button>
        <button type="button" class="vc-btn" id="vcFwd" aria-label="10 saniye ileri" title="10 saniye ileri (→)"><svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M12 5V2l5 4-5 4V7a6 6 0 1 0 6 6h2a8 8 0 1 1-8-8z"/><text x="12" y="16.2" font-size="6.4" font-weight="700" text-anchor="middle" font-family="sans-serif">10</text></svg></button>
        <div class="vc-vol"><button type="button" class="vc-btn" id="vcMute" aria-label="Sesi kapat / aç" title="Ses (M)"><span class="i-vol"><svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M4 9v6h4l5 4V5L8 9H4z"/><path d="M16 8.5a5 5 0 0 1 0 7" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"/><path d="M18.5 6a8.5 8.5 0 0 1 0 12" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg></span><span class="i-mute"><svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M4 9v6h4l5 4V5L8 9H4z"/><path d="M16.5 9.5l5 5m0-5l-5 5" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg></span></button><input type="range" class="vc-volr" id="vcVol" min="0" max="100" value="100" aria-label="Ses düzeyi" autocomplete="off" /></div>
        <span class="vc-time" id="vcTime">0:00 / 0:00</span>
        <span class="vc-sp"></span>
        <button type="button" class="vc-btn" id="vcFull" aria-label="Tam ekran" title="Tam ekran (F)"><span class="i-full"><svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M4 9V4h5M20 9V4h-5M4 15v5h5M20 15v5h-5" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"/></svg></span><span class="i-exit"><svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor"><path d="M9 4v5H4M15 4v5h5M9 20v-5H4M15 20v-5h5" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"/></svg></span></button>
      </div>
    </div>
    </div>
    <div class="vlb-next" id="vlbNext" style="display:none"></div>
    <div class="vlb-title" id="vlbTitle"></div>
    <div class="vlb-tools">
      <button type="button" class="vlb-wl" id="vlbWl"><svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z"/></svg><span>İzleme listeme ekle</span></button>
      <span class="vlb-resume" id="vlbResume" style="display:none"></span>
      <div class="vlb-size" id="vlbSize" title="Pencere boyutu"><span>Boyut</span><button type="button" data-z="normal" class="on" title="Normal"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="6" y="8" width="12" height="8" rx="1.5"/></svg>Normal</button><button type="button" data-z="large" title="Bir boy büyük"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="6" width="18" height="12" rx="1.5"/></svg>Büyük</button><button type="button" data-z="fill" title="Tarayıcı penceresini doldur"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M4 9V4h5M20 9V4h-5M4 15v5h5M20 15v5h-5"/></svg>Tarayıcı</button></div>
      <div class="vlb-speed" id="vlbSpeed" title="Oynatma hızı"><span>Hız</span><button type="button" data-r="0.75">0,75×</button><button type="button" data-r="1" class="on">1×</button><button type="button" data-r="1.25">1,25×</button></div>
    </div>
    <div class="vlb-rate" id="vlbRate">
      <span class="vlb-rate-lbl">Bu içeriği puanla:</span>
      <span class="vlb-stars" id="vlbStars">
        <i data-s="1">★</i><i data-s="2">★</i><i data-s="3">★</i><i data-s="4">★</i><i data-s="5">★</i>
      </span>
      <input type="text" class="vlb-comment" id="vlbComment" placeholder="Yorumunuz (opsiyonel)" />
      <button type="button" class="vlb-rate-save" id="vlbSaveRate" disabled="disabled" title="Yorumu gönder">Gönder</button>
      <span class="vlb-rate-msg" id="vlbRateMsg" role="status" aria-live="polite"></span>
    </div>
  </div>
</div>

<div class="vid-lightbox" id="docModal">
  <div class="vlb-inner">
    <button type="button" class="vlb-close" id="docClose" aria-label="Kapat">&times;</button>
    <div class="doc-frame" id="docFrame"></div>
    <div class="doc-bar"><span class="vlb-title" id="docTitle"></span><span class="doc-actions" id="docActions"></span></div>
  </div>
</div>

<div class="vid-lightbox" id="histModal">
  <div class="vlb-inner ec-modal">
    <button type="button" class="vlb-close" id="histClose" aria-label="Kapat">&times;</button>
    <h2 class="ec-modal-h">Eğitim Geçmişim</h2>
    <p class="ec-modal-sub">Eğitimlerdeki ilerlemeniz ve izleme istatistikleriniz</p>
    <div class="hp-stats">
      <div class="hp-stat"><div class="v"><%= StatVideosDone %><span>/<%= StatVideosTotal %></span></div><div class="k">Tamamlanan video</div></div>
      <div class="hp-stat"><div class="v">%<%= StatRate %></div><div class="k">Genel izleme oranı</div></div>
      <div class="hp-stat"><div class="v"><%= StatCoursesDone %><span>/<%= StatCoursesTotal %></span></div><div class="k">Tamamlanan eğitim</div></div>
      <div class="hp-stat"><div class="v"><%= StatMinutes %><span> dk</span></div><div class="k">Son 30 gün izleme</div></div>
    </div>
    <% if (ProgressHtml.Length > 0) { %>
    <div class="hist-sec">Eğitimlerim</div>
    <div class="gc-list hp-list"><%= ProgressHtml %></div>
    <% } %>
    <div class="hist-sec">Son 30 gün · <%= StatContent %> içerik</div>
    <div class="hist-list"><%= HistHtml %></div>
  </div>
</div>

<div class="vid-lightbox" id="suggestModal">
  <div class="vlb-inner ec-modal" style="max-width:560px">
    <button type="button" class="vlb-close" id="suggestClose" aria-label="Kapat">&times;</button>
    <h2 class="ec-modal-h">İçerik Öner</h2>
    <p class="ec-modal-sub">Görmek istediğiniz bir eğitim konusu mu var? Önerinizi iletin, ekibimize ulaşsın.</p>
    <form method="post" action="SuggestContent.aspx">
      <input class="input" name="title" placeholder="Konu başlığı (opsiyonel)" />
      <textarea class="input" name="body" style="min-height:100px;resize:vertical;margin-top:10px" placeholder="Hangi konuda eğitim içeriği istersiniz?" required="required"></textarea>
      <button class="btn-cyan" type="submit" style="padding:11px 24px;margin-top:14px">Öneriyi Gönder</button>
    </form>
  </div>
</div>

<% if (CanEdit) { %>
<div class="tw-detail" id="twDetail" role="dialog" aria-modal="true" aria-labelledby="twDetailT" hidden>
  <div class="tw-dbox">
    <div class="tw-dhd"><div><div class="tw-dk">EĞİTİM GEÇMİŞİ</div><h3 id="twDetailT"></h3></div>
      <a class="btn-ghost tw-dopen" id="twDetailOpen" href="#" target="_blank" rel="noopener">Yeni sekmede aç</a>
      <button type="button" class="tw-dx" id="twDetailX" aria-label="Kapat">&times;</button></div>
    <iframe id="twFrame" title="Eğitim geçmişi"></iframe>
  </div>
</div>
<div class="vid-lightbox" id="editorModal">
  <div class="vlb-inner ec-modal ed-modal">
    <button type="button" class="vlb-close" id="editorClose" aria-label="Kapat">&times;</button>
    <h2 class="ec-modal-h">Editör Paneli</h2>
    <p class="ec-modal-sub">İçerikleri buradan yönetin; sayfayı son kullanıcı gibi görmeye devam edin.</p>
    <div class="ed-tabs">
      <button type="button" class="ed-tab on" data-t="video">Video Ekle</button>
      <button type="button" class="ed-tab" data-t="doc">Doküman Ekle</button>
      <button type="button" class="ed-tab" data-t="manage">İçerik Yönetimi</button>
      <button type="button" class="ed-tab" data-t="cats">Kategoriler</button>
      <button type="button" class="ed-tab" data-t="stats">İstatistikler</button>
      <button type="button" class="ed-tab" data-t="watchers">En Çok İzleyenler</button>
      <button type="button" class="ed-tab" data-t="path">Sıfırdan Uzmanlığa</button>
    </div>

    <div class="ed-pane on" data-p="video">
      <form method="post" action="AddTrainingVideo.aspx">
        <input class="input" name="titletr" placeholder="Başlık (Türkçe)" required="required" />
        <input class="input" name="titleen" placeholder="Title (English) opsiyonel" />
        <div style="display:flex;gap:10px;flex-wrap:wrap">
          <div class="ec-catpick" style="flex:1;min-width:180px">
            <select class="input st-select ec-cat-sel"><option value="">Ana kategori seçin</option><%= VidGroupOptions %><option value="__new__">+ Yeni kategori oluştur</option></select>
            <input class="input ec-cat-new" placeholder="Yeni ana kategori adı" style="display:none;margin-top:8px" />
            <input type="hidden" name="categorygroup" class="ec-cat-hidden" />
          </div>
          <div class="ec-catpick" style="flex:1;min-width:180px">
            <select class="input st-select ec-cat-sel"><option value="">Alt kategori seçin</option><%= VidSubOptions %><option value="__new__">+ Yeni alt kategori oluştur</option></select>
            <input class="input ec-cat-new" placeholder="Yeni alt kategori adı" style="display:none;margin-top:8px" />
            <input type="hidden" name="subcategory" class="ec-cat-hidden" />
          </div>
        </div>
        <input class="input" name="url" placeholder="YouTube bağlantısı veya embed linki" required="required" />
        <label class="perm-chk" style="margin-top:10px"><input type="checkbox" name="notify" value="1" /><span>Kullanıcılara bildirim gönder <em>uygulama içi + e-posta (tercihi kapalı olanlara e-posta gitmez)</em></span></label>
        <button class="btn-cyan" type="submit" style="width:100%;padding:11px;margin-top:12px">Videoyu Yayınla</button>
      </form>
    </div>

    <div class="ed-pane" data-p="cats">
      <p class="tc-intro">Ana ve alt kategorileri buradan yönetin. Ad değişikliği o kategorideki tüm içeriklere yansır; içeriği olan bir kategori silinemez. Sayfadaki sıra, sayfa yenilendiğinde güncellenir.</p>
      <div class="tc-cols">
        <section class="tc-col" data-scope="Video"><h4>Video kategorileri</h4><div class="tc-list" aria-live="polite"></div>
          <form class="tc-add" data-noload="1"><input class="input" maxlength="120" placeholder="Yeni ana kategori adı" aria-label="Yeni video ana kategorisi" /><button class="btn-cyan" type="submit">Ekle</button></form></section>
        <section class="tc-col" data-scope="Document"><h4>Doküman kategorileri</h4><div class="tc-list" aria-live="polite"></div>
          <form class="tc-add" data-noload="1"><input class="input" maxlength="120" placeholder="Yeni ana kategori adı" aria-label="Yeni doküman ana kategorisi" /><button class="btn-cyan" type="submit">Ekle</button></form></section>
      </div>
      <p class="tc-err" id="tcErr" role="alert"></p>
    </div>

    <div class="ed-pane" data-p="doc">
      <form method="post" action="AddTrainingDocument.aspx" enctype="multipart/form-data">
        <input class="input" name="titletr" placeholder="Başlık (Türkçe)" required="required" />
        <input class="input" name="titleen" placeholder="Title (English) opsiyonel" />
        <div style="display:flex;gap:10px;flex-wrap:wrap">
          <div class="ec-catpick" style="flex:1;min-width:180px">
            <select class="input st-select ec-cat-sel"><option value="">Ana kategori seçin</option><%= DocGroupOptions %><option value="__new__">+ Yeni kategori oluştur</option></select>
            <input class="input ec-cat-new" placeholder="Yeni ana kategori adı" style="display:none;margin-top:8px" />
            <input type="hidden" name="categorygroup" class="ec-cat-hidden" />
          </div>
          <div class="ec-catpick" style="flex:1;min-width:180px">
            <select class="input st-select ec-cat-sel"><option value="">Alt kategori seçin</option><%= DocSubOptions %><option value="__new__">+ Yeni alt kategori oluştur</option></select>
            <input class="input ec-cat-new" placeholder="Yeni alt kategori adı" style="display:none;margin-top:8px" />
            <input type="hidden" name="subcategory" class="ec-cat-hidden" />
          </div>
        </div>
        <textarea class="input" name="description" style="min-height:70px;resize:vertical" placeholder="Açıklama (opsiyonel)"></textarea>
        <div class="field" style="margin-top:2px"><label>Dosya (her tür, en fazla 50 MB)</label>
          <input class="input" type="file" name="file" required="required" />
          <p class="ed-hint">PDF, görseller, video, ses, metin, CSV, Word (.docx) ve Excel tarayıcıda açılır; diğer türler indirilir. Program ve betik dosyaları (.exe, .bat, .js gibi) yüklenemez.</p></div>
        <div class="field" style="margin:0"><label>Kapak görseli (opsiyonel)</label>
          <input class="input" type="file" name="cover" accept=".png,.jpg,.jpeg,.webp" /></div>
        <label class="perm-chk" style="margin-top:10px"><input type="checkbox" name="notify" value="1" /><span>Kullanıcılara bildirim gönder <em>uygulama içi + e-posta (tercihi kapalı olanlara e-posta gitmez)</em></span></label>
        <button class="btn-cyan" type="submit" style="width:100%;padding:11px;margin-top:12px">Dokümanı Yayınla</button>
      </form>
    </div>

    <div class="ed-pane" data-p="manage">
      <p style="color:var(--muted);font-size:13.5px;line-height:1.6;margin:0 0 14px">Mevcut içerikleri düzenleyin, başlık/açıklama değiştirin, yayından kaldırın veya banner'da öne çıkarın.</p>
      <a class="btn-cyan" href="TrainingManage.aspx" style="padding:11px 20px;text-decoration:none">İçerik Yönetimine Git</a>
    </div>

    <div class="ed-pane" data-p="stats"><%= EditorStatsHtml %></div>
    <div class="ed-pane" data-p="watchers">
      <div class="tw-bar">
        <div class="tw-seg" role="group" aria-label="Dönem"><button type="button" class="on" data-p="all">Tüm zamanlar</button><button type="button" data-p="30">Son 30 gün</button><button type="button" data-p="month">Bu ay</button></div>
        <select class="input st-select tw-type" id="twType" aria-label="Firma türü"><option value="">Tüm firmalar</option><option value="partner">İş ortakları</option><option value="customer">Müşteriler</option><option value="fol">Fol Yazılım</option></select>
        <input class="input tw-q" id="twQ" type="search" placeholder="Kişi ya da firma ara…" aria-label="Kişi ya da firma ara" autocomplete="off" />
      </div>
      <div class="tw-sum" id="twSum" aria-live="polite"></div>
      <div class="tw-list" id="twList"></div>
    </div>
    <div class="ed-pane" data-p="path">
      <p class="lp-ed-help">Yoldaki adımların sırasını sürükleyerek değiştirin. Sağdaki listeden video ekleyin, × ile çıkarın. Kazanılmış rozetler silinmez.</p>
      <div class="lp-ed"><div><div class="lp-ed-h">Yoldaki adımlar <span id="lpEdCnt"></span></div><ol class="lp-ed-list" id="lpEdIn"></ol></div>
        <div><div class="lp-ed-h">Eklenebilecek videolar</div><input class="input lp-ed-q" id="lpEdQ" placeholder="Video ara" autocomplete="off" /><ul class="lp-ed-list lp-ed-out" id="lpEdOut"></ul></div></div>
      <div class="lp-ed-ft"><span class="lp-ed-msg" id="lpEdMsg"></span><button type="button" class="btn-cyan" id="lpEdSave">Sırayı kaydet</button></div>
    </div>
  </div>
</div>
<% } %>

<div class="wl-pop" id="wlPop" style="display:none"></div>
<div class="vid-lightbox" id="wlShareModal">
  <div class="vlb-inner ec-modal" style="max-width:520px">
    <button type="button" class="vlb-close" id="wlShareClose" aria-label="Kapat">&times;</button>
    <h2 class="ec-modal-h">Listeyi Paylaş</h2>
    <p class="ec-modal-sub" id="wlShareName"></p>
    <div class="wl-scopes" id="wlScopes">
      <label><input type="radio" name="wlscope" value="private" /> <span><b>Yalnız ben</b><small>Liste kişiye özel kalır</small></span></label>
      <label><input type="radio" name="wlscope" value="company" /> <span><b>Firmamdaki herkes</b><small>Firmanızdaki tüm kullanıcılar görür</small></span></label>
      <label><input type="radio" name="wlscope" value="users" /> <span><b>Seçtiğim kişiler</b><small>Seçilen iş arkadaşlarınıza bildirim gider</small></span></label>
    </div>
    <div class="wl-people" id="wlPeople" style="display:none"></div>
    <% if (CanEdit) { %><label class="wl-rec"><input type="checkbox" id="wlRec" /> <span><b>Tüm kullanıcılara öner</b><small>Liste herkesin Akademi’sinde “Önerilen Listeler” altında görünür</small></span></label><% } %>
    <div style="display:flex;gap:10px;margin-top:18px"><button type="button" class="btn-cyan" id="wlShareSave" style="padding:11px 22px">Kaydet</button><button type="button" class="btn-ghost" id="wlShareCancel" style="padding:11px 18px">Vazgeç</button></div>
  </div>
</div>
<script>window.MYRATINGS = <%= MyRatingsJson %>; window.WL = <%= WatchJson %>; window.VIDS = <%= VideosJson %>; window.WL_ME = <%= Auth.UserId %>; window.WL_CANREC = <%= CanEdit ? "true" : "false" %>;</script>
<script src="js/common.js?v=185"></script>
<script src="js/training.js?v=25"></script>
<script src="js/acshelf.js?v=9"></script>
<% if (CanEdit) { %><script src="js/trcats.js?v=7"></script><% } %>
<div class="vid-lightbox" id="pathModal">
  <div class="vlb-inner ec-modal lp-modal">
    <button type="button" class="vlb-close" id="pathClose" aria-label="Kapat">&times;</button>
    <div class="lp-head"><div><div class="lp-kick">ÖNERİLEN EĞİTİM YOLU</div><h2 class="ec-modal-h">Sıfırdan Uzmanlığa</h2>
      <p class="ec-modal-sub">Eğitimleri önerdiğimiz sırayla izleyin. Her adım bir öncekini tamamlayınca açılır; her video bir rozet kazandırır.</p></div>
      <div class="lp-ring" id="lpRing"></div></div>
    <div class="lp-steps" id="lpSteps"></div>
    <div class="lp-ms" id="lpMs"></div>
  </div>
</div>
<div class="vid-lightbox" id="teamModal">
  <div class="vlb-inner ec-modal tm-modal">
    <button type="button" class="vlb-close" id="teamClose" aria-label="Kapat">&times;</button>
    <h2 class="ec-modal-h">Takım Arkadaşlarım</h2>
    <p class="ec-modal-sub" id="tmSum">Firmanızdaki kişilerin River Akademi rozetleri ve sertifikaları</p>
    <div class="tm-q"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><circle cx="11" cy="11" r="7"/><path d="m20 20-3.5-3.5"/></svg>
      <input type="search" id="tmQ" name="tm_search" aria-label="Takım arkadaşı ara" placeholder="İsimle ara" autocomplete="off" readonly="readonly" data-lpignore="true" data-1p-ignore="true" data-bwignore="true" data-form-type="other" /></div>
    <div class="tm-list" id="tmList"><div class="tm-empty">Yükleniyor…</div></div>
  </div>
</div>
<div class="lp-cele" id="lpCele" aria-live="polite"><canvas id="lpConf"></canvas><div class="lp-cele-card"><div class="lp-cele-badge" id="lpCeleBadge"></div><div class="lp-cele-k" id="lpCeleK">YENİ ROZET</div><h3 id="lpCeleT"></h3><p id="lpCeleP"></p><button type="button" class="btn-cyan" id="lpCeleOk">Harika!</button></div></div>
<script>window.TEAM_SUM = <%= TeamJson %>; window.PATH = <%= PathJson %>; window.PATH_ADMIN = <%= PathAdminJson %>; window.PATH_EDIT = <%= CanEdit ? "true" : "false" %>;</script>
<script src="<%= ResolveUrl("~/js/path.js") %>?v=6"></script>
<% if (CanEdit) { %><script src="<%= ResolveUrl("~/js/watchers.js") %>?v=1"></script><% } %>
</body>
</html>
