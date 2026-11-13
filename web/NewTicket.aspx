<%@ Page Language="C#" AutoEventWireup="true" CodeFile="NewTicket.aspx.cs" Inherits="NewTicketPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Destek Talebi</title>
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
    <div class="links"><%= Nav.Links("support") %></div>
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

  <div class="page-head">
    <h1>Destek Talebi Oluştur</h1>
    <p>Size daha hızlı çözüm üretebilmemiz için lütfen alanları eksiksiz doldurun; mümkünse ekran görüntüsü veya log ekleyin.</p>
  </div>

  <% if (FlashErr.Length > 0) { %><div class="flash-err"><%= Server.HtmlEncode(FlashErr) %></div><% } %>

  <% if (NoBalance) { %>
  <div class="flash-err" style="line-height:1.6">Destek süreniz bulunmamaktadır. Lütfen Fol Yazılım ile iletişime geçiniz.</div>
  <% } else { %>
  <form method="post" action="AddTicket.aspx" enctype="multipart/form-data" id="tkForm">
    <input type="hidden" name="product" id="fProduct" value="" />
    <input type="hidden" name="module" value="Genel" />

    <div class="card" style="margin-bottom:18px">
      <div class="section-h" id="prodSec"><span>Ürün <span style="color:var(--btn)">*</span></span></div>
      <div style="padding:4px 24px 22px">
        <div class="tk-chips" id="prodChips">
          <span class="tk-chip" data-prod="River AI" role="radio" aria-checked="false" tabindex="0">River AI</span>
          <span class="tk-chip" data-prod="River PRO" role="radio" aria-checked="false" tabindex="0">River PRO</span>
        </div>
        <div class="tk-prodnote" id="prodNote">Talebinizin ilgili olduğu ürünü seçin. Farklı ürünlerle ilgili konuları ayrı taleplerle gönderin.</div>
      </div>
    </div>

    <div class="card" style="margin-bottom:18px">
      <div class="section-h"><span>Talep Bilgileri</span></div>
      <div style="padding:4px 24px 22px">
        <div class="tk-grid2">
          <div class="field"><label>Talep Tipi <span style="color:var(--btn)">*</span></label>
            <select class="input st-select" name="requesttype">
              <option>Hata</option><option>Kurulum</option><option>Nasıl Yapılır</option>
              <option>Özellik Talebi</option><option>Performans</option><option>Entegrasyon</option><option>Diğer</option>
            </select></div>
          <div class="field"><label>Öncelik <span style="color:var(--btn)">*</span></label>
            <select class="input st-select" name="priority">
              <option value="Low">Düşük</option><option value="Normal" selected="selected">Normal</option>
              <option value="High">Yüksek</option><option value="Critical">Kritik</option>
            </select></div>
        </div>

        <% if (!IsCustomer) { %>
        <div class="field"><label>Müşteri <span style="color:var(--dim);font-weight:400">(opsiyonel)</span></label>
          <select class="input st-select" name="customer"><%= CustomerOptionsHtml %></select>
          <p class="hint note-amber">Listede adı olmayan müşteriniz varsa, öncelikle müşterinizin versiyon anlaşması bulunduğundan emin olun ve bayi yöneticinizle iletişime geçin.</p>
        </div>
        <% } %>

        <div class="tk-grid2">
          <div class="field"><label>Kullanılan Versiyon <span style="color:var(--btn)">*</span></label>
            <input class="input" name="version" placeholder="Örn. 3.4.2" required="required" /></div>
          <div class="field"><label>Ortam <span style="color:var(--dim);font-weight:400">(opsiyonel)</span></label>
            <select class="input st-select" name="environment">
              <option value="">Belirtilmedi</option><option value="Canlı">Canlı</option><option value="Test">Test</option>
            </select></div>
        </div>

        <div class="field"><label for="tkPhone">Telefon <span style="color:var(--dim);font-weight:400">(opsiyonel)</span></label>
          <input class="input" id="tkPhone" name="phone" type="tel" inputmode="tel" autocomplete="tel" maxlength="30" placeholder="+90 (5xx) xxx xxxx" value="<%= Server.HtmlEncode(SavedPhone) %>" />
          <% if (PhoneFeature) { %>
          <div class="tk-phone-opt">
            <label class="tk-chk"><input type="checkbox" name="rememberPhone" value="1"<%= SavedPhone.Length > 0 ? " checked=\"checked\"" : "" %> /><span>Sonraki taleplerimde hatırla</span></label>
            <button type="button" class="tk-forget" id="tkForget"<%= SavedPhone.Length > 0 ? "" : " hidden=\"hidden\"" %>>Numaramı sistemden sil</button>
          </div>
          <div class="tk-phone-msg" id="tkPhoneMsg" role="status" aria-live="polite"></div>
          <script>
            (function () {
              var b = document.getElementById('tkForget'); if (!b) return;
              b.addEventListener('click', function () {
                b.disabled = true;
                var fd = new FormData(); fd.append('op', 'forget');
                fetch('TicketPhone.ashx', { method: 'POST', body: fd, credentials: 'same-origin' }).then(function (r) { return r.json(); }).then(function (d) {
                  var m = document.getElementById('tkPhoneMsg');
                  if (d && d.ok) {
                    document.getElementById('tkPhone').value = '';
                    var c = document.querySelector('input[name="rememberPhone"]'); if (c) c.checked = false;
                    b.hidden = true; m.className = 'tk-phone-msg ok'; m.textContent = 'Numaranız sistemden silindi. Sonraki taleplerde otomatik doldurulmayacak.';
                  } else { b.disabled = false; m.className = 'tk-phone-msg err'; m.textContent = 'Numara silinemedi, lütfen tekrar deneyin.'; }
                }).catch(function () { b.disabled = false; var m = document.getElementById('tkPhoneMsg'); m.className = 'tk-phone-msg err'; m.textContent = 'Bağlantı hatası, lütfen tekrar deneyin.'; });
              });
            })();
          </script>
          <% } %></div>

        <div class="field"><label>Konu <span style="color:var(--btn)">*</span></label>
          <input class="input" name="subject" placeholder="Konu başlığını girin" required="required" /></div>

        <div class="field"><label>Talep Açıklaması <span style="color:var(--btn)">*</span></label>
          <textarea class="input" name="description" style="min-height:130px;resize:vertical" placeholder="Sorunu veya talebinizi olabildiğince ayrıntılı anlatın." required="required"></textarea></div>

        <div class="field" style="margin-bottom:0"><label>Destekleyici Doküman <span style="color:var(--dim);font-weight:400">(opsiyonel, birden fazla)</span></label>
          <input class="input" type="file" name="files" multiple="multiple" accept=".pdf,.png,.jpg,.jpeg,.gif,.webp,.txt,.log,.csv,.json,.xml,.zip,.doc,.docx,.xls,.xlsx,.ppt,.pptx" />
          <p class="hint" style="color:var(--muted);font-size:11.5px;margin:6px 0 0">Ekran görüntüsü, log veya doküman ekleyebilirsiniz. Dosya başına en fazla 10 MB.</p></div>
      </div>
    </div>

    <div style="display:flex;gap:12px">
      <button class="btn-cyan" type="submit" style="padding:13px 26px;font-weight:700">Destek Talebini Gönder</button>
      <a class="btn-ghost" href="Tickets.aspx" style="padding:13px 22px;text-decoration:none">Vazgeç</a>
    </div>
  </form>
  <% } %>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
</div>

<script src="js/common.js?v=186"></script>
<script>
  (function(){
    // Urun secimi: yalniz River AI / River PRO, onceden secili degil. Secime gore uyari; secilmeden gonderilemez.
    var fProd = document.getElementById('fProduct'), note = document.getElementById('prodNote');
    var NOTES = {
      '': ['', 'Talebinizin ilgili olduğu ürünü seçin. Farklı ürünlerle ilgili konuları ayrı taleplerle gönderin.'],
      'River AI': ['warn', '<b>River AI seçtiniz.</b> Bu talebe verilecek yanıtlar ve yönlendirmeler yalnız River AI içindir. Sorunuz River PRO ile ilgiliyse lütfen <b>River PRO</b>’yu seçin; aksi halde yanlış ürüne ait yönlendirmeler alabilirsiniz.'],
      'River PRO': ['info', '<b>River PRO seçtiniz.</b> Talebiniz River PRO olarak kaydedilip destek ekibimize iletilir.']
    };
    function pick(prod) {
      fProd.value = prod;
      document.querySelectorAll('#prodChips .tk-chip').forEach(function (x) { var on = x.getAttribute('data-prod') === prod; x.classList.toggle('on', on); x.setAttribute('aria-checked', on ? 'true' : 'false'); });
      var n = NOTES[prod] || NOTES['']; note.className = 'tk-prodnote' + (n[0] ? ' ' + n[0] : ''); note.innerHTML = n[1];
    }
    document.querySelectorAll('#prodChips .tk-chip').forEach(function (c) {
      c.addEventListener('click', function () { pick(c.getAttribute('data-prod')); });
      c.addEventListener('keydown', function (e) { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); pick(c.getAttribute('data-prod')); } });
    });
    document.getElementById('tkForm').addEventListener('submit', function (e) {
      if (fProd.value) return;
      e.preventDefault(); note.className = 'tk-prodnote bad'; note.innerHTML = '<b>Lütfen ürünü seçin.</b> Talebiniz River AI ile mi, River PRO ile mi ilgili?';
      document.getElementById('prodSec').scrollIntoView({ behavior: 'smooth', block: 'center' });
    });
  })();
</script>
<script src="js/kb-suggest.js?v=4"></script>
</body>
</html>
