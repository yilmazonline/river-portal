<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TicketDetail.aspx.cs" Inherits="TicketDetailPage" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · <%= Server.HtmlEncode(T.TicketNo) %></title>
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
        <div class="who"><div class="nm"><%= Server.HtmlEncode(UserName) %></div></div>
        <div class="avatar"><%= Fmt.Initials(UserName) %></div>
        <div class="user-menu" id="userMenu">
          <div class="um-head"><div class="um-name"><%= Server.HtmlEncode(UserName) %></div></div>
          <a href="Logout.aspx" class="danger">Çıkış Yap</a>
        </div>
      </div>
    </div>
  </nav>

  <% if (FlashMsg.Length > 0) { %><div class="flash-ok"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>

  <% if (CanConfirm) { %>
  <div class="note-amber" style="margin:0 0 16px">Bu talep <b>çözüldü</b> olarak işaretlendi ve şu an <b>sizin onayınızı bekliyor</b>. 5 gün içinde onaylamaz ya da yeniden açmazsanız talep <b>otomatik olarak kapanacaktır</b>.</div>
  <% } %>

  <div class="page-head" style="display:flex;justify-content:space-between;align-items:flex-start;gap:16px">
    <div>
      <h1 style="font-size:20px"><%= Server.HtmlEncode(T.Subject) %></h1>
      <p><%= Server.HtmlEncode(T.TicketNo) %> · <% if (CanEditStatus) { %><button type="button" class="st st-edit <%= StatusClass %>" data-tid="<%= T.TicketId %>" data-note="<%= Server.HtmlEncode(T.StatusNote ?? "") %>" data-def="<%= Server.HtmlEncode(StatusDefault) %>" title="Görünen durumu değiştir"><span><%= Server.HtmlEncode(StatusLabel) %></span><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 20h4L19 9l-4-4L4 16z"/></svg></button><% } else { %><span class="st <%= StatusClass %>"><%= Server.HtmlEncode(StatusLabel) %></span><% } %></p>
    </div>
    <div style="display:flex;gap:8px;flex-wrap:wrap">
      <a class="btn-ghost" href="TicketPrint.aspx?id=<%= T.TicketId %>" target="_blank" style="padding:10px 16px;text-decoration:none;white-space:nowrap">PDF / Yazdır</a>
      <a class="btn-ghost" href="Tickets.aspx" style="padding:10px 16px;text-decoration:none;white-space:nowrap">Listeye Dön</a>
    </div>
  </div>

  <div class="cols" style="grid-template-columns:1.6fr 1fr">
    <div>
      <div class="card" style="margin-bottom:18px">
        <div class="section-h"><span>Konuşma</span></div>
        <div style="padding:4px 24px 22px"><%= ThreadHtml %></div>
      </div>

      <% if (CanReply) { %>
      <div class="card">
        <div class="section-h"><span>Yanıt Yaz</span></div>
        <div style="padding:4px 24px 22px">
          <form method="post" action="TicketAction.aspx" enctype="multipart/form-data">
            <input type="hidden" name="id" value="<%= T.TicketId %>" />
            <input type="hidden" name="action" value="reply" />
            <textarea class="input" name="body" style="min-height:110px;resize:vertical" placeholder="Yanıtınızı yazın..." required="required"></textarea>
            <div class="tk-reply-actions">
              <button class="btn-cyan" type="submit" style="padding:11px 20px">Yanıtı Gönder</button>
              <% if (IsSupport) { %>
              <button id="noteBtn" data-me="<%= Server.HtmlEncode(UserName) %>" class="btn-ghost" type="submit" formaction="TicketAction.aspx" name="action" value="note" style="padding:11px 20px">Dahili Not Olarak Ekle</button>
              <% } %>
              <span class="tk-clip-wrap">
                <span class="tk-clip-name"></span>
                <label class="btn-ghost tk-clip" title="Dosya ekle (maks. 5 MB)">
                  <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21.44 11.05l-9.19 9.19a5 5 0 0 1-7.07-7.07l9.19-9.19a3.5 3.5 0 0 1 4.95 4.95l-9.2 9.19a2 2 0 0 1-2.83-2.83l8.49-8.48"/></svg>
                  <input type="file" name="attach" accept=".pdf,.png,.jpg,.jpeg,.gif,.webp,.txt,.log,.csv,.json,.xml,.zip,.doc,.docx,.xls,.xlsx,.ppt,.pptx" onchange="tkClip(this)" />
                </label>
              </span>
            </div>
          </form>
          <% if (IsSupport) { %><p style="color:var(--muted);font-size:11.5px;margin:10px 0 0">Dahili notlar yalnız üretici tarafında görünür, bayiye/müşteriye iletilmez.</p><% } %>
        </div>
      </div>
      <% } %>

      <% if (!IsSupport && (CanClose || CanReopen)) { %>
      <div class="card" style="margin-top:18px">
        <div class="section-h"><span><%= T.Status == "Resolved" ? "Çözüm Onayı" : "Talep İşlemleri" %></span></div>
        <div style="padding:4px 24px 22px;display:flex;flex-direction:column;gap:12px">
          <% if (CanConfirm) { %><p style="margin:0;color:var(--muted);font-size:12.5px">Sorun giderildiyse çözümü onaylayıp talebi kapatın; devam ediyorsa "Sorun Çözülmedi" ile yeniden açın.</p><% } %>
          <% if (CanResolveReq) { %>
          <form method="post" action="TicketAction.aspx" style="margin:0">
            <input type="hidden" name="id" value="<%= T.TicketId %>" />
            <button class="btn-ghost" type="submit" name="action" value="resolve" style="width:100%;padding:11px">Çözüldü Olarak İşaretle</button>
          </form>
          <% } %>
          <% if (CanClose) { %>
          <form method="post" action="TicketAction.aspx" style="margin:0">
            <input type="hidden" name="id" value="<%= T.TicketId %>" />
            <input type="hidden" name="action" value="close" />
            <button class="btn-cyan" type="submit" style="width:100%;padding:11px"><%= T.Status == "Resolved" ? "Çözümü Onayla ve Kapat" : "Talebi Kapat" %></button>
          </form>
          <% } %>
          <% if (CanReopen) { %>
          <form method="post" action="TicketAction.aspx" enctype="multipart/form-data" style="margin:0">
            <input type="hidden" name="id" value="<%= T.TicketId %>" />
            <input type="hidden" name="action" value="reopen" />
            <textarea class="input" name="body" style="min-height:80px;resize:vertical" placeholder="Çözülmeyen noktaları yazın; talebi yeniden açalım." required="required"></textarea>
            <div class="tk-reply-actions">
              <button class="btn-ghost" type="submit" style="flex:1;padding:11px;border-color:rgba(255,61,113,.4);color:#FF9DB5">Sorun Çözülmedi, Yeniden Aç</button>
              <span class="tk-clip-wrap" style="margin-left:0">
                <span class="tk-clip-name"></span>
                <label class="btn-ghost tk-clip" title="Dosya ekle (maks. 5 MB)">
                  <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21.44 11.05l-9.19 9.19a5 5 0 0 1-7.07-7.07l9.19-9.19a3.5 3.5 0 0 1 4.95 4.95l-9.2 9.19a2 2 0 0 1-2.83-2.83l8.49-8.48"/></svg>
                  <input type="file" name="attach" accept=".pdf,.png,.jpg,.jpeg,.gif,.webp,.txt,.log,.csv,.json,.xml,.zip,.doc,.docx,.xls,.xlsx,.ppt,.pptx" onchange="tkClip(this)" />
                </label>
              </span>
            </div>
          </form>
          <% } %>
        </div>
      </div>
      <% } %>
    </div>

    <div>
      <div class="card" style="margin-bottom:18px">
        <div class="section-h"><span>Talep Bilgileri</span></div>
        <div style="padding:4px 24px 20px"><%= MetaHtml %></div>
        <%= AttachHtml %>
      </div>

      <% if (!IsSupport && T.Status != "Closed" && T.Status != "Resolved") { %>
      <div class="card" style="margin-bottom:18px">
        <div class="section-h"><span>Hatırlatma</span></div>
        <div style="padding:8px 24px 20px">
          <% if (CanRemind) { %>
          <p style="margin:0 0 10px;color:var(--muted);font-size:12.5px">Talebiniz beklerken destek ekibine nazik bir hatırlatma gönderebilirsiniz. Hafta içi, 24 saatte bir gönderilebilir.</p>
          <form method="post" action="TicketAction.aspx" style="margin:0">
            <input type="hidden" name="id" value="<%= T.TicketId %>" />
            <button class="btn-cyan" type="submit" name="action" value="remind" style="width:100%;padding:11px">Hatırlatma Gönder</button>
          </form>
          <% } else { %>
          <button type="button" disabled="disabled" class="btn-ghost" style="width:100%;padding:11px;opacity:.6;cursor:not-allowed"><%= Server.HtmlEncode(RemindBtnText) %></button>
          <p style="margin:8px 0 0;color:var(--muted);font-size:12px"><%= Server.HtmlEncode(RemindInfo) %></p>
          <% } %>
        </div>
      </div>
      <% } %>

      <% if (IsSupport && T.Status != "Closed") { %>
      <div class="card">
        <div class="section-h"><span>Destek İşlemleri</span></div>
        <div style="padding:4px 24px 22px;display:flex;flex-direction:column;gap:10px">
          <% if (CanTake) { %>
          <form method="post" action="TicketAction.aspx" style="margin:0">
            <input type="hidden" name="id" value="<%= T.TicketId %>" />
            <button class="btn-cyan" type="submit" name="action" value="assign" style="width:100%;padding:11px">Üzerime Al</button>
          </form>
          <% } %>
          <% if (CanAssign && SupportUserOptions.Length > 0) { %>
          <form method="post" action="TicketAction.aspx" style="margin:0">
            <input type="hidden" name="id" value="<%= T.TicketId %>" />
            <input type="hidden" name="action" value="assignto" />
            <label style="display:block;font-size:12px;color:var(--muted);margin-bottom:6px">Başka bir destek personeline ata</label>
            <select class="input st-select" name="target" required="required" style="margin-bottom:8px">
              <option value="">Kişi seçin...</option>
              <%= SupportUserOptions %>
            </select>
            <button class="btn-ghost" type="submit" style="width:100%;padding:11px">Ata</button>
          </form>
          <% } %>
          <% if (T.Status != "Resolved") { %>
          <form method="post" action="TicketAction.aspx" style="margin:0" id="resolveForm">
            <input type="hidden" name="id" value="<%= T.TicketId %>" />
            <button type="button" class="btn-ghost" id="resolveOpen" style="width:100%;padding:11px">Çözüldü Olarak İşaretle</button>
            <div id="resolvePanel" style="display:none;margin-top:10px">
              <label style="display:block;font-size:12.5px;color:var(--muted);margin-bottom:6px">Çözüm süresini <b>dakika bazında</b> girin (en az 15):</label>
              <input class="input" type="number" name="minutes" id="resMinutes" min="0" step="5" value="15" style="margin-bottom:8px" />
              <button class="btn-cyan" type="submit" name="action" value="resolve" style="width:100%;padding:11px">Çözümü Kaydet</button>
            </div>
          </form>
          <% } %>
        </div>
      </div>
      <% } %>

      <% if (IsSupport && (T.Status == "Resolved" || T.Status == "Closed")) { %>
      <div class="card" style="margin-top:18px">
        <div class="section-h"><span>Çözüm Süresi</span></div>
        <div style="padding:6px 24px 20px">
          <form method="post" action="TicketAction.aspx" style="margin:0">
            <input type="hidden" name="id" value="<%= T.TicketId %>" />
            <label style="display:block;font-size:12.5px;color:var(--muted);margin-bottom:6px">Bu talep için çözüm süresini <b>dakika</b> girin/güncelleyin (en az 15). Girilen süre firmanın destek bakiyesinden düşülür.</label>
            <input class="input" type="number" name="minutes" min="0" step="5" value="<%= T.ResolutionMinutes.HasValue ? T.ResolutionMinutes.Value : 15 %>" style="margin-bottom:8px" />
            <button class="btn-cyan" type="submit" name="action" value="logminutes" style="width:100%;padding:11px"><%= T.ResolutionMinutes.HasValue ? "Süreyi Güncelle" : "Süreyi Kaydet" %></button>
          </form>
          <% if (T.ResolutionMinutes.HasValue) { %><p style="color:var(--muted);font-size:12px;margin:8px 0 0">Kayıtlı süre: <b><%= T.ResolutionMinutes.Value %> dk</b></p><% } %>
        </div>
      </div>
      <% } %>

      <div class="card" style="margin-top:18px">
        <div class="section-h"><span>Form Hareketleri</span></div>
        <div style="padding:8px 24px 18px" class="tk-acts"><%= ActivitiesHtml %></div>
      </div>
    </div>
  </div>

  <div class="app-footer">
    <img src="assets/fol_logo.png" alt="Fol Yazılım" />
    <div class="fname">Fol Yazılım A.Ş.</div>
  </div>
</div>
</div>

<script>
(function () {
  var nb = document.getElementById('noteBtn');
  if (!nb) return;
  var form = nb.form || (nb.closest ? nb.closest('form') : null);
  if (!form) return;
  nb.addEventListener('click', function (e) {
    e.preventDefault();
    var ta = form.querySelector('textarea[name=body]');
    var idEl = form.querySelector('input[name=id]');
    var txt = (ta && ta.value ? ta.value : '').replace(/^\s+|\s+$/g, '');
    if (!txt) { if (ta) ta.focus(); return; }
    var orig = nb.textContent; nb.disabled = true;
    var body = 'id=' + encodeURIComponent(idEl.value) + '&action=note&ajax=1&body=' + encodeURIComponent(txt);
    fetch('TicketAction.aspx', { method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: body })
      .then(function (r) { return r.text(); })
      .then(function () {
        nb.disabled = false;
        var esc = function (s) { var x = document.createElement('div'); x.textContent = s; return x.innerHTML; };
        var thread = document.querySelector('.tk-thread');
        if (thread) {
          var p = function (n) { return (n < 10 ? '0' : '') + n; }; var d = new Date();
          var ts = p(d.getDate()) + '.' + p(d.getMonth() + 1) + '.' + d.getFullYear() + ' ' + p(d.getHours()) + ':' + p(d.getMinutes());
          var el = document.createElement('div'); el.className = 'tk-msg internal';
          el.innerHTML = '<div class="who">' + esc(nb.getAttribute('data-me') || '') + '<span style="font-weight:400;color:var(--muted)"> · Dahili Not</span></div>' +
            '<div class="when">' + ts + '</div><div class="body">' + esc(txt) + '</div>';
          thread.appendChild(el); el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }
        if (ta) ta.value = '';
        nb.textContent = 'Not eklendi ✓';
        setTimeout(function () { nb.textContent = orig; }, 1600);
      })
      .catch(function () { nb.disabled = false; nb.textContent = orig; alert('Not eklenemedi, tekrar deneyin.'); });
  });
})();
(function () {
  var ro = document.getElementById('resolveOpen');
  if (ro) ro.addEventListener('click', function () {
    var p = document.getElementById('resolvePanel'); if (p) p.style.display = 'block';
    ro.style.display = 'none';
    var m = document.getElementById('resMinutes'); if (m) m.focus();
  });
  var rf = document.getElementById('resolveForm');
  if (rf) rf.addEventListener('submit', function () {
    var m = document.getElementById('resMinutes');
    if (m) { var v = parseInt(m.value, 10); if (isNaN(v) || v < 0) v = 0; if (v > 0 && v < 15) v = 15; else if (v >= 15) v = Math.round(v/5)*5; m.value = v; }
  });
})();
</script>
<script>
function tkClip(inp){
  try{
    if(inp.files[0] && inp.files[0].size > 5*1024*1024){ alert('Dosya boyutu en fazla 5 MB olabilir.'); inp.value=''; }
    var w = inp.closest ? inp.closest('.tk-clip-wrap') : inp.parentNode.parentNode;
    if(w){ var n=w.querySelector('.tk-clip-name'); if(n) n.textContent = inp.files[0]?inp.files[0].name:''; if(inp.files[0]) w.className+=' has'; else w.className=w.className.replace(' has',''); }
  }catch(e){}
}
</script>
<script src="js/common.js?v=186"></script>
<script src="js/ticketstatus.js?v=2"></script>
<script src="js/ticketview.js?v=2"></script>
</body>
</html>
