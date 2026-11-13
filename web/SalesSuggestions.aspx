<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SalesSuggestions.aspx.cs" Inherits="SalesSuggestionsPage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Satış Önerileri</title>
<link href="https://fonts.googleapis.com/css2?family=Sora:wght@400;600;700;800&family=Inter:wght@400;500;600&display=swap" rel="stylesheet" />
<link rel="stylesheet" href="css/site.css?v=170" />
<link rel="icon" type="image/svg+xml" href="assets/RIVER_AI_badge.svg" />
</head>
<body<%= Embed ? " class=\"ss-embed\"" : "" %>>
<div id="dash" data-culture="<%= Culture %>">
<div class="wrap">
  <nav class="nav">
    <div class="brand">
      <a href="<%= Nav.HomeHref %>" aria-label="Ana Sayfa" style="display:inline-flex"><img class="brand-logo" src="assets/RIVER_AI_white.svg" alt="River AI" /></a>
      <span class="sub" data-i18n="brand_sub">Asistan Portal</span>
    </div>
    <div class="links"><%= Nav.Links("sales") %></div>
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
  <% if (SetupMsg.Length > 0) { %><div class="flash-err"><%= E(SetupMsg) %></div><% } else { %>
  <div class="page-head" style="display:flex;justify-content:space-between;align-items:flex-end;gap:16px;flex-wrap:wrap">
    <div><div class="rc-kicker">FİNANS</div><h1>Satış Önerileri</h1>
      <p>Teklif'e girilen kanal siparişlerini iş ortağıyla eşleştirip satış kaydı olarak ekleyin. Tutarlar KDV hariçtir.</p></div>
  </div>
  <% if (SyncMsg.Length > 0) { %><div class="flash-ok"><%= E(SyncMsg) %></div><% } %>
  <nav class="ss-tabs" aria-label="Öneri durumu">
    <a href="SalesSuggestions.aspx?embed=1" class="<%= Tab == "pending" ? "on" : "" %>">Bekleyen <span id="ssCnt"><%= CntPending %></span></a>
    <a href="SalesSuggestions.aspx?embed=1&amp;tab=booked" class="<%= Tab == "booked" ? "on" : "" %>">Kaydedilen <span><%= CntBooked %></span></a>
    <a href="SalesSuggestions.aspx?embed=1&amp;tab=dismissed" class="<%= Tab == "dismissed" ? "on" : "" %>">Yoksayılan <span><%= CntDismissed %></span></a>
  </nav>
  <%= ListHtml %>
  <% } %>
</div>
</div>
<script>
(function () {
  // Alacaklar sayfasindaki pencereye bekleyen sayisini bildir (rozet guncellenir)
  function tell(n) { try { if (window.parent !== window) window.parent.postMessage({ type: 'ss-pending', pending: n }, location.origin); } catch (e) { } }
  var c0 = document.getElementById('ssCnt'); if (c0) tell(parseInt(c0.textContent, 10) || 0);
  // odak pencerenin icindeyken Esc ust sayfaya ulasmaz: kapatma istegi ust sayfaya iletilir
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && window.parent !== window) { try { window.parent.postMessage({ type: 'ss-close' }, location.origin); } catch (x) { } } });
  function send(data) {
    var fd = new FormData(); for (var k in data) fd.append(k, data[k]);
    return fetch('SalesSuggestAction.ashx', { method: 'POST', body: fd, credentials: 'same-origin' }).then(function (r) { return r.json(); });
  }
  function done(card, txt, cls) {
    card.classList.add('ss-done'); card.querySelector('.ss-act').remove();
    var m = card.querySelector('.ss-msg'); m.className = 'ss-msg ' + cls; m.textContent = txt;
  }
  document.addEventListener('click', function (e) {
    var b = e.target.closest ? e.target.closest('.ss-book, .ss-dis, .ss-restore') : null; if (!b) return;
    if (b.classList.contains('ss-restore')) {
      var tr = b.closest('tr'); b.disabled = true;
      send({ op: 'restore', id: tr.getAttribute('data-id') }).then(function (d) { if (d.ok) { tr.remove(); tell(d.pending); } else { b.disabled = false; alert(d.error || 'İşlem tamamlanamadı.'); } });
      return;
    }
    var card = b.closest('.ss-card'), id = card.getAttribute('data-id'), sel = card.querySelector('select'), msg = card.querySelector('.ss-msg');
    if (b.classList.contains('ss-book') && !sel.value) { msg.className = 'ss-msg err'; msg.textContent = 'Önce iş ortağını seçin.'; sel.focus(); return; }
    [].forEach.call(card.querySelectorAll('button'), function (x) { x.disabled = true; });
    var book = b.classList.contains('ss-book'), name = book ? sel.options[sel.selectedIndex].text.replace(' (önerilen)', '') : '';
    send(book ? { op: 'book', id: id, company: sel.value } : { op: 'dismiss', id: id }).then(function (d) {
      if (!d.ok) { [].forEach.call(card.querySelectorAll('button'), function (x) { x.disabled = false; }); msg.className = 'ss-msg err'; msg.textContent = d.error || 'İşlem tamamlanamadı.'; return; }
      document.getElementById('ssCnt').textContent = d.pending; tell(d.pending);
      done(card, book ? '✓ ' + name + ' iş ortağına satış olarak kaydedildi. Bayi yöneticilerine bildirim gönderildi.' : 'Kanal satışı değil olarak yoksayıldı. Yoksayılan sekmesinden geri alabilirsiniz.', book ? 'ok' : 'mute');
    }).catch(function () { [].forEach.call(card.querySelectorAll('button'), function (x) { x.disabled = false; }); msg.className = 'ss-msg err'; msg.textContent = 'Bağlantı hatası, tekrar deneyin.'; });
  });
})();
</script>
<script src="js/common.js?v=186"></script>
</body>
</html>
