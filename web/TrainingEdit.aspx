<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TrainingEdit.aspx.cs" Inherits="TrainingEditPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · İçerik Düzenle</title>
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

  <div class="page-head"><h1>İçerik Düzenle</h1><p><%= TypeLabel %></p></div>

  <% if (FlashErr.Length > 0) { %><div class="flash-err"><%= Server.HtmlEncode(FlashErr) %></div><% } %>

  <div class="card" style="max-width:640px;padding:26px 28px">
    <form method="post" action="TrainingEdit.aspx" enctype="multipart/form-data">
      <input type="hidden" name="id" value="<%= MaterialId %>" />
      <div class="field"><label>Başlık (Türkçe)</label>
        <input class="input" name="titletr" value="<%= Server.HtmlEncode(TitleTr) %>" required="required" /></div>
      <div class="field"><label>Title (English) opsiyonel</label>
        <input class="input" name="titleen" value="<%= Server.HtmlEncode(TitleEn) %>" /></div>
      <div class="field"><label>Açıklama</label>
        <textarea class="input" name="description" style="min-height:90px;resize:vertical"><%= Server.HtmlEncode(DescriptionTr) %></textarea></div>
      <% if (!IsVideo) { %>
      <div class="field"><label for="teCover">Kapak görseli</label>
        <div class="te-vid">
          <img id="teCovImg" src="<%= CoverCur.Length > 0 ? "assets/traincovers/" + Server.HtmlEncode(CoverCur) : "" %>" alt="Kapak görseli" <%= CoverCur.Length > 0 ? "" : "hidden=\"hidden\"" %> />
          <div class="te-vid-in">
            <input class="input" type="file" id="teCover" name="cover" accept=".png,.jpg,.jpeg,.webp,image/png,image/jpeg,image/webp" />
            <small class="te-hint" id="teCovInfo"><%= CoverCur.Length > 0 ? "Yeni bir görsel seçerseniz mevcut kapağın yerine geçer." : "Bu dokümanın kapağı yok." %> 16:9 oran önerilir (ör. 1920×1080); PNG, JPG ya da WebP, en fazla 5 MB.</small>
            <% if (CoverCur.Length > 0) { %><label class="te-hint te-cov-rm"><input type="checkbox" name="coverremove" value="1" id="teCovRm" /><span>Kapağı kaldır</span></label><% } %>
          </div>
        </div>
      </div>
      <% } %>
      <% if (IsVideo) { %>
      <div class="field"><label for="teUrl">Video bağlantısı</label>
        <div class="te-vid">
          <img id="teThumb" src="<%= YoutubeIdCur.Length == 11 ? "https://i.ytimg.com/vi/" + Server.HtmlEncode(YoutubeIdCur) + "/mqdefault.jpg" : "" %>" alt="Video küçük resmi" <%= YoutubeIdCur.Length == 11 ? "" : "hidden=\"hidden\"" %> />
          <div class="te-vid-in">
            <textarea class="input" id="teUrl" name="url" rows="2" required="required" spellcheck="false" style="resize:vertical;font-size:13px" data-cur="<%= Server.HtmlEncode(YoutubeIdCur) %>"><%= Server.HtmlEncode(VideoUrl) %></textarea>
            <small id="teUrlInfo" class="te-hint">YouTube bağlantısı, kısa bağlantı (youtu.be) ya da embed kodunun (iframe) tamamını yapıştırabilirsiniz.</small>
          </div>
        </div>
        <% if (KbCount > 0) { %><small class="te-hint" id="teKbNote" hidden="hidden">⚠️ Bu video bilgi tabanında <%= KbCount %> kayıtta kaynak olarak kullanılıyor. Yeni videoda anlatım sırası değiştiyse destek yanıtlarındaki zaman damgaları kayabilir; gerekirse bilgi tabanını yeniden çıkarın.</small><% } %>
      </div>
      <% } %>
      <% if (UseSelects) { %>
      <div class="te-cats">
        <div class="field"><label for="teGroup">Ana Kategori</label>
          <select class="input" id="teGroup" name="categorygroup" required="required"><%= GroupOptions %></select></div>
        <div class="field"><label for="teSub">Alt Kategori</label>
          <select class="input" id="teSub" name="subcategory"><option value="Genel" data-any="1">Alt kategori yok (Genel)</option><%= SubOptions %></select></div>
      </div>
      <p class="te-hint">Listede olmayan bir kategori için önce <a href="Training.aspx">Editör Paneli › Kategoriler</a> sekmesinden ekleyin.</p>
      <script>
        // alt kategori listesi secilen ana kategoriye gore suzulur (Turkce buyuk-kucuk harf ve ı/i farki gozetilmez)
        (function () {
          var g = document.getElementById('teGroup'), s = document.getElementById('teSub'); if (!g || !s) return;
          var fold = function (x) { return String(x || '').trim().toLocaleLowerCase('tr-TR').replace(/ı/g, 'i'); };
          function sync() {
            var k = fold(g.value), cur = s.options[s.selectedIndex];
            [].forEach.call(s.options, function (o) { if (o.getAttribute('data-any')) return; var ok = fold(o.getAttribute('data-g')) === k; o.hidden = !ok; o.disabled = !ok; });
            if (cur && cur.disabled) s.value = 'Genel';
          }
          g.addEventListener('change', sync); sync();
        })();
      </script>
      <% } else { %>
      <div style="display:flex;gap:10px">
        <div class="field" style="flex:1"><label>Ana Kategori</label>
          <input class="input" name="categorygroup" value="<%= Server.HtmlEncode(CategoryGroup) %>" /></div>
        <div class="field" style="flex:1"><label>Alt Kategori</label>
          <input class="input" name="subcategory" value="<%= Server.HtmlEncode(SubCategory) %>" /></div>
      </div>
      <% } %>
      <label class="chk" style="display:flex;align-items:center;gap:10px;font-size:13.5px;margin:6px 0 10px;cursor:pointer">
        <input type="checkbox" name="featured" value="1" <%= IsFeatured ? "checked=\"checked\"" : "" %> style="width:16px;height:16px;accent-color:var(--btn)" />
        <span>Banner'da öne çıkar</span>
      </label>
      <div class="field"><label>Öne Çıkarma Metni (banner etiketi, opsiyonel)</label>
        <input class="input" name="featuretext" value="<%= Server.HtmlEncode(FeatureText) %>" placeholder="Örn. Öne Çıkan · Yeni Sürüm · En Çok İzlenen" /></div>
      <div style="display:flex;gap:10px;margin-top:8px">
        <button class="btn-cyan" type="submit" style="flex:1;padding:12px">Kaydet</button>
        <a class="btn-ghost" href="TrainingManage.aspx" style="flex:1;text-align:center;padding:12px">Vazgeç</a>
      </div>
    </form>
    <p style="color:var(--muted);font-size:12px;margin-top:14px">"Öne çıkar" işaretli içerikler River Akademi banner'ında slider olarak döner; etiket olarak yukarıdaki metni gösterir.</p>
  </div>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
<script>
  // Dokuman kapagi: secilen gorseli kaydetmeden once onizle
  (function () {
    var inp = document.getElementById('teCover'); if (!inp) return;
    var img = document.getElementById('teCovImg'), info = document.getElementById('teCovInfo'), rm = document.getElementById('teCovRm'), orig = img.getAttribute('src');
    inp.addEventListener('change', function () {
      var f = inp.files && inp.files[0];
      if (!f) { img.src = orig; img.hidden = !orig; return; }
      if (!/\.(png|jpe?g|webp)$/i.test(f.name)) { info.className = 'te-hint bad'; info.textContent = 'Kapak görseli PNG, JPG ya da WebP olmalı.'; inp.value = ''; return; }
      if (f.size > 5 * 1024 * 1024) { info.className = 'te-hint bad'; info.textContent = 'Kapak görseli en fazla 5 MB olabilir.'; inp.value = ''; return; }
      img.src = URL.createObjectURL(f); img.hidden = false; if (rm) rm.checked = false;
      info.className = 'te-hint ok'; info.textContent = 'Yeni kapak: ' + f.name + ' · Kaydettiğinizde mevcut kapağın yerine geçer.';
    });
    if (rm) rm.addEventListener('change', function () { img.style.opacity = rm.checked ? '.3' : ''; if (rm.checked) inp.value = ''; });
  })();
  // Video baglantisi: yapistirilan baglanti / embed kodundan YouTube kimligini cikar, kucuk resmi ve durumu canli guncelle (sunucudaki Fmt.YouTubeId ile ayni kurallar)
  (function () {
    var ta = document.getElementById('teUrl'); if (!ta) return;
    var img = document.getElementById('teThumb'), info = document.getElementById('teUrlInfo'), kb = document.getElementById('teKbNote'), cur = ta.getAttribute('data-cur') || '';
    function yid(s) {
      s = (s || '').trim(); var m = ['embed/', 'watch?v=', 'youtu.be/', 'shorts/', 'v='], low = s.toLowerCase(), rest = s;
      for (var i = 0; i < m.length; i++) { var p = low.indexOf(m[i]); if (p >= 0) { rest = s.substring(p + m[i].length); break; } }
      var id = (rest.match(/^[A-Za-z0-9_-]+/) || [''])[0].substring(0, 11);
      return id.length === 11 ? id : '';
    }
    function upd() {
      var id = yid(ta.value);
      if (id) { img.src = 'https://i.ytimg.com/vi/' + id + '/mqdefault.jpg'; img.hidden = false; }
      else img.hidden = true;
      info.className = 'te-hint' + (id ? (id !== cur ? ' ok' : '') : (ta.value.trim() ? ' bad' : ''));
      info.textContent = !ta.value.trim() ? 'YouTube bağlantısı, kısa bağlantı (youtu.be) ya da embed kodunun (iframe) tamamını yapıştırabilirsiniz.'
        : !id ? 'Bu metinde YouTube videosu bulunamadı. Bağlantıyı ya da embed kodunun tamamını yapıştırın.'
        : id === cur ? 'Mevcut video: ' + id : 'Yeni video algılandı: ' + id + ' · Kaydettiğinizde video değişecek; süre ilk izlemede yeniden hesaplanır.';
      if (kb) kb.hidden = !(id && id !== cur);
      ta.setCustomValidity(id ? '' : 'Geçerli bir YouTube bağlantısı ya da embed kodu girin.');
    }
    ta.addEventListener('input', upd); upd();
  })();
</script>
<script src="js/common.js?v=186"></script>
</body>
</html>
