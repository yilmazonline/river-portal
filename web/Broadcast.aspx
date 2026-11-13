<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Broadcast.aspx.cs" Inherits="BroadcastPage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · Toplu Bildirim</title>
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
    <div class="links"><%= Nav.Links("overview") %></div>
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
    <div><h1 style="font-size:20px">Toplu Bildirim</h1>
    <p>Seçtiğiniz bayi ve müşterilere uygulama içi bildirim ve/veya temalı e-posta gönderin.</p></div>
    <a class="btn-ghost" href="Vendor.aspx" style="padding:10px 16px">Genel Bakış</a>
  </div>

  <% if (FlashMsg.Length > 0) { %><div class="flash-ok" style="margin-bottom:14px"><%= Server.HtmlEncode(FlashMsg) %></div><% } %>
  <% if (FlashErr.Length > 0) { %><div class="flash-err" style="margin-bottom:14px"><%= Server.HtmlEncode(FlashErr) %></div><% } %>

  <form method="post" action="Broadcast.aspx" id="bcForm">
  <div class="bc-cols">
    <div class="bc-left">

      <!-- 1. Alicilar -->
      <div class="card combo-host">
        <div class="section-h"><span>1. Alıcılar</span><span class="bc-sum" id="bcSum">Seçim yok</span></div>
        <div class="bc-pad">
          <input type="search" class="input" id="bcSearch" placeholder="Firma ara..." autocomplete="off" readonly="readonly" data-lpignore="true" data-1p-ignore="true" data-form-type="other" />
          <div class="bc-grp">
            <div class="bc-gh"><b>Bayiler</b> <span>(<%= PartnerCount %>)</span><label class="bc-all"><input type="checkbox" data-all="p" /> Tümünü seç</label></div>
            <div class="bc-list" data-list="p"><%= PartnerRows %></div>
          </div>
          <div class="bc-grp">
            <div class="bc-gh"><b>Müşteriler</b> <span>(<%= CustomerCount %>)</span><label class="bc-all"><input type="checkbox" data-all="c" /> Tümünü seç</label></div>
            <div class="bc-list" data-list="c"><%= CustomerRows %></div>
          </div>
          <p class="bc-note">Seçilen firmaların tüm aktif kullanıcılarına gönderilir.</p>
          <div class="bc-extra" id="extraBox">
            <div class="bc-gh"><b>Ek e-posta adresleri</b> <span id="bcExCount"></span>
              <label class="btn-ghost bc-file bc-file-sm">Dosyadan yükle<input type="file" id="bcExFile" accept=".txt,.csv,text/plain,text/csv" /></label></div>
            <textarea class="input bc-extra-in" name="extraEmails" id="bcExtra" rows="4" spellcheck="false" autocomplete="off" placeholder="Adresleri yapıştırın: virgül, noktalı virgül ya da satır satır. Outlook'tan kopyalanan &quot;Ad Soyad &lt;ad@firma.com&gt;&quot; biçimi ve Excel sütunu da olur."></textarea>
            <div class="bc-ana" id="bcAna" aria-live="polite"></div>
            <p class="bc-note">Ek adreslere yalnız e-posta gider; uygulama içi bildirim yalnız portal firmalarına iletilir. Hatalı, tekrar eden ve seçili firmalarda zaten alıcı olan adresler gönderimden kendiliğinden çıkarılır.</p>
          </div>
        </div>
      </div>

      <!-- 2. Kanal -->
      <div class="card" style="margin-top:14px">
        <div class="section-h"><span>2. Gönderim Kanalı</span></div>
        <div class="bc-pad">
          <div class="seg">
            <label><input type="checkbox" name="ch_app" id="chApp" checked="checked" /> <span>Uygulama içi bildirim</span></label>
            <label><input type="checkbox" name="ch_mail" id="chMail" checked="checked" /> <span>E-posta</span></label>
          </div>
          <div class="bc-to" id="mailNote">
            <label for="bcTo">Kime (To)</label>
            <input class="input" type="email" name="toAddr" id="bcTo" value="<%= Server.HtmlEncode(ToAddress) %>" data-def="<%= Server.HtmlEncode(ToAddress) %>" maxlength="254" autocomplete="off" spellcheck="false" />
            <p class="bc-note">Bu adres e-postanın görünen alıcısıdır. Seçilen firmaların kullanıcıları ve ek adresler gizli alıcı (BCC) olarak eklenir; birbirlerinin adresini görmezler. <span id="bcToErr" class="bc-to-err" role="alert"></span></p>
          </div>
        </div>
      </div>

      <!-- 3. Icerik -->
      <div class="card" style="margin-top:14px">
        <div class="section-h"><span>3. İçerik</span></div>
        <div class="bc-pad">
          <div id="themeWrap">
            <label class="bc-lbl">E-posta Teması</label>
            <div class="bc-themes">
              <label class="bc-th"><input type="radio" name="theme" value="duyuru" checked="checked" /><span class="bc-sw" style="background:#4F46E5"></span><b>Genel Duyuru</b></label>
              <label class="bc-th"><input type="radio" name="theme" value="yenilik" /><span class="bc-sw" style="background:linear-gradient(135deg,#4F46E5,#7A3BEA,#22B8D8)"></span><b>Ürün Yeniliği</b></label>
              <label class="bc-th"><input type="radio" name="theme" value="bakim" /><span class="bc-sw" style="background:#D99A1E"></span><b>Planlı Bakım</b></label>
              <label class="bc-th"><input type="radio" name="theme" value="etkinlik" /><span class="bc-sw" style="background:#12A150"></span><b>Etkinlik / Webinar</b></label>
              <label class="bc-th"><input type="radio" name="theme" value="kritik" /><span class="bc-sw" style="background:#E4467A"></span><b>Kritik / Acil</b></label>
              <label class="bc-th"><input type="radio" name="theme" value="custom" /><span class="bc-sw bc-sw-code" aria-hidden="true">&lt;/&gt;</span><b>Özel HTML</b></label>
            </div>
            <div class="field" id="subjWrap" style="margin-top:14px">
              <label>E-posta Konusu <span style="color:var(--dim);font-weight:400">(boş bırakılırsa başlık kullanılır)</span></label>
              <input class="input" name="subject" id="bcSubject" placeholder="Örn. River AI 5.2 yayında" />
            </div>
          </div>

          <div class="field"><label id="bcTitleL">Başlık</label>
            <input class="input bc-in" name="title" id="bcTitle" required="required" placeholder="Bildirim başlığı" /></div>
          <div class="field" id="bodyWrap"><label id="bcBodyL">Metin <span style="color:var(--dim);font-weight:400">(boş satır = yeni paragraf, "- " ile başlayan satır = madde)</span></label>
            <textarea class="input bc-in" name="body" id="bcBody" required="required" style="min-height:150px;resize:vertical" placeholder="Bildirim metni"></textarea></div>

          <div class="field bc-custom" id="customWrap" hidden>
            <label for="bcHtml">HTML Kodu <span style="color:var(--dim);font-weight:400">(tasarımınızı yapıştırın ya da dosyadan yükleyin)</span></label>
            <div class="bc-htools">
              <label class="btn-ghost bc-file">.html dosyası yükle<input type="file" id="bcHtmlFile" accept=".html,.htm,text/html" /></label>
              <span class="bc-hsize" id="bcHsize" aria-live="polite"></span>
            </div>
            <textarea class="input bc-in bc-code" name="html" id="bcHtml" spellcheck="false" autocomplete="off" placeholder="&lt;table width=&quot;100%&quot; ...&gt;"></textarea>
            <ul class="bc-hnotes" id="bcHnotes" aria-live="polite"></ul>
            <p class="bc-note">E-posta programları harici CSS dosyalarını ve betikleri desteklemez: stilleri satır içi (<code>style="..."</code>) verin, görselleri tam adresle (<code>https://...</code>) ekleyin. Betik ve olay işleyicileri gönderimden önce çıkarılır.</p>
          </div>

          <div id="extraWrap">
            <div class="bc-row">
              <div class="field" id="w1"><label id="l1">Alan 1</label><input class="input bc-in" name="f1" id="bcF1" /></div>
              <div class="field" id="w2"><label id="l2">Alan 2</label><input class="input bc-in" name="f2" id="bcF2" /></div>
              <div class="field" id="w3"><label id="l3">Alan 3</label><input class="input bc-in" name="f3" id="bcF3" /></div>
            </div>
          </div>

          <div id="btnWrap" class="bc-row">
            <div class="field"><label>Buton Metni <span style="color:var(--dim);font-weight:400">(opsiyonel)</span></label>
              <input class="input bc-in" name="btnText" id="bcBtnT" placeholder="Örn. Portala Git" /></div>
            <div class="field"><label>Buton Linki</label>
              <input class="input bc-in" name="btnUrl" id="bcBtnU" placeholder="https://portal.folyazilim.com" /></div>
          </div>

          <div class="bc-acts">
            <button class="btn-cyan" type="submit" style="padding:12px 26px">Gönder</button>
            <button class="btn-ghost bc-testbtn" type="button" id="bcTest">Bana test gönder</button>
            <span class="bc-testmsg" id="bcTestMsg" role="status"></span>
          </div>
        </div>
      </div>
    </div>

    <!-- Onizleme -->
    <div class="bc-right">
      <div class="bc-sticky">
        <div class="card" id="appPrevCard">
          <div class="section-h"><span>Uygulama Bildirimi Önizleme</span></div>
          <div class="bc-pad">
            <div class="bc-notif"><div class="bc-nt" id="pvT">Bildirim başlığı</div><div class="bc-nb" id="pvB">Bildirim metni burada görünür.</div></div>
          </div>
        </div>
        <div class="card" id="mailPrevCard" style="margin-top:14px">
          <div class="section-h"><span>E-posta Önizleme</span></div>
          <iframe id="bcFrame" class="bc-frame" title="E-posta önizleme" sandbox="allow-popups allow-popups-to-escape-sandbox"></iframe>
        </div>
      </div>
    </div>
  </div>
  </form>
</div>
</div>
<script>
(function () {
  var $ = function (id) { return document.getElementById(id); };
  var form = $('bcForm'), chApp = $('chApp'), chMail = $('chMail');
  var DEF = {
    duyuru:  null,
    yenilik: [['Alt Başlık (renkli bantta)', 'text', 'Örn. Daha hızlı form tasarımı'], null, null],
    bakim:   [['Tarih', 'text', 'Örn. 28 Eylül 2026, Pazar'], ['Saat', 'text', 'Örn. 02:00 – 04:00'], ['Etkilenen Sistem', 'text', 'Örn. Asistan Portal, API']],
    etkinlik:[['Tarih', 'date', ''], ['Saat', 'text', 'Örn. 14:00 – 15:00'], ['Yer', 'text', 'Örn. Online · Microsoft Teams']],
    kritik:  [['Uyarı Bandı Metni', 'text', 'Bu bildirim acil işlem gerektirmektedir.'], null, null]
  };
  function theme() { var r = document.querySelector('input[name=theme]:checked'); return r ? r.value : 'duyuru'; }

  function applyCustom() {   // Ozel HTML: tema alanlari yerine HTML kodu; metin yalniz uygulama bildirimi icin
    var c = theme() === 'custom' && chMail.checked;
    $('customWrap').hidden = !c;
    $('bodyWrap').style.display = (c && !chApp.checked) ? 'none' : '';
    $('bcBody').required = !c || chApp.checked;
    $('bcBodyL').firstChild.nodeValue = c ? 'Uygulama bildirimi metni ' : 'Metin ';
    $('bcTitleL').textContent = c ? 'Başlık (konu boşsa e-posta konusu olur)' : 'Başlık';
  }
  function applyTheme() {
    var d = DEF[theme()];
    applyCustom();
    $('extraWrap').style.display = d ? '' : 'none';
    for (var i = 0; i < 3; i++) {
      var w = $('w' + (i + 1)), inp = $('bcF' + (i + 1));
      if (d && d[i]) { w.style.display = ''; $('l' + (i + 1)).textContent = d[i][0]; if (inp.type !== d[i][1]) { inp.value = ''; inp.type = d[i][1]; } inp.placeholder = d[i][2]; inp.disabled = false; }
      else { w.style.display = 'none'; inp.disabled = true; }
    }
    $('btnWrap').style.display = (theme() === 'bakim' || theme() === 'custom') ? 'none' : '';
    if (theme() === 'bakim') { $('bcBtnT').value = ''; $('bcBtnU').value = ''; }
    schedule();
  }
  function applyChannels() {
    var m = chMail.checked, a = chApp.checked;
    $('themeWrap').style.display = m ? '' : 'none';
    $('mailNote').style.display = m ? '' : 'none';
    $('mailPrevCard').style.display = m ? '' : 'none';
    $('extraBox').style.display = m ? '' : 'none';
    $('appPrevCard').style.display = a ? '' : 'none';
    if (!m) { $('extraWrap').style.display = 'none'; $('btnWrap').style.display = 'none'; applyCustom(); } else applyTheme();
    refreshSum();   // ek adresler yalniz e-posta kanalinda sayilir
  }

  // --- Onizleme ---
  var timer = null;
  function schedule() { clearTimeout(timer); timer = setTimeout(preview, 350); }
  function preview() {
    $('pvT').textContent = $('bcTitle').value || 'Bildirim başlığı';
    var b = ($('bcBody').value || 'Bildirim metni burada görünür.').replace(/\s+/g, ' ');
    $('pvB').textContent = b.length > 280 ? b.substring(0, 277) + '...' : b;
    if (!chMail.checked) return;
    var p = new URLSearchParams();
    p.append('theme', theme()); p.append('title', $('bcTitle').value || 'Bildirim başlığı');
    p.append('body', $('bcBody').value || 'Bildirim metni burada görünür.');
    p.append('btnText', $('bcBtnT').value); p.append('btnUrl', $('bcBtnU').value || (($('bcBtnT').value) ? '#' : ''));
    for (var i = 1; i <= 3; i++) p.append('f' + i, $('bcF' + i).disabled ? '' : $('bcF' + i).value);
    var custom = theme() === 'custom';
    if (custom) p.append('html', $('bcHtml').value);
    fetch('BroadcastPreview.aspx', { method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: p.toString() })
      .then(function (r) {
        var dec = function (k) { var v = r.headers.get(k); try { return v ? decodeURIComponent(v) : ''; } catch (e) { return ''; } };
        var notes = dec('X-Broadcast-Notes'), er = dec('X-Broadcast-Error');
        showNotes(custom ? (er ? [er] : []).concat(notes ? notes.split('|') : []) : [], !!er);
        return r.text();
      })
      .then(function (h) {
        if (custom && !h) { $('bcFrame').srcdoc = '<p style="font:14px sans-serif;color:#6b7499;padding:24px">Önizleme için HTML kodu yapıştırın.</p>'; return; }
        // ozel HTML kendi <base>'i olmadan, tam belge olarak; cerceve sandbox'ta (betik calismaz)
        $('bcFrame').srcdoc = custom ? h : '<base href="' + location.href.replace(/[^\/]*$/, '') + '"><style>body{margin:0}</style>' + h;
      })
      .catch(function () { });
  }

  // --- Ozel HTML: boyut, notlar, dosyadan yukleme ---
  function showNotes(list, isErr) {
    var ul = $('bcHnotes'); ul.innerHTML = '';
    list.forEach(function (t) { if (!t) return; var li = document.createElement('li'); li.textContent = t; if (isErr) li.className = 'err'; ul.appendChild(li); });
  }
  function sizeInfo() {
    var b = new Blob([$('bcHtml').value]).size, el = $('bcHsize');
    el.textContent = b ? (b < 1024 ? b + ' B' : (b / 1024).toFixed(b < 10240 ? 1 : 0).replace('.', ',') + ' KB') : '';
    el.className = 'bc-hsize' + (b > 1048576 ? ' bad' : b > 104448 ? ' warn' : '');
  }
  $('bcHtml').addEventListener('input', sizeInfo);
  $('bcHtmlFile').addEventListener('change', function () {
    var fl = this.files && this.files[0]; if (!fl) return;
    if (fl.size > 1048576) { showNotes(['Dosya 1 MB sınırını aşıyor.'], true); this.value = ''; return; }
    var rd = new FileReader();
    rd.onload = function () { $('bcHtml').value = String(rd.result || ''); sizeInfo(); schedule(); };
    rd.readAsText(fl, 'utf-8'); this.value = '';
  });

  // --- Test gonderimi: yalniz oturumdaki yoneticiye ---
  $('bcTest').addEventListener('click', function () {
    var b = this, msg = $('bcTestMsg');
    if (!chMail.checked) { msg.className = 'bc-testmsg err'; msg.textContent = 'Test için e-posta kanalını seçin.'; return; }
    if (theme() === 'custom' ? !$('bcHtml').value.trim() : (!$('bcTitle').value.trim() || !$('bcBody').value.trim())) { msg.className = 'bc-testmsg err'; msg.textContent = theme() === 'custom' ? 'Önce HTML kodunu girin.' : 'Önce başlık ve metni girin.'; return; }
    var fd = new FormData(form); fd.delete('cid');
    b.disabled = true; msg.className = 'bc-testmsg'; msg.textContent = 'Gönderiliyor…';
    fetch('BroadcastTest.ashx', { method: 'POST', body: fd, credentials: 'same-origin' }).then(function (r) { return r.json(); }).then(function (d) {
      b.disabled = false; msg.className = 'bc-testmsg ' + (d.ok ? 'ok' : 'err'); msg.textContent = d.message || '';
      if (d.notes && d.notes.length && theme() === 'custom') showNotes(d.notes, false);
    }).catch(function () { b.disabled = false; msg.className = 'bc-testmsg err'; msg.textContent = 'Bağlantı hatası.'; });
  });

  // --- Kime (To) ---
  function toOk() {
    var v = $('bcTo').value.trim(), ok = /^[^\s@,;<>]+@([a-z0-9]([a-z0-9-]*[a-z0-9])?\.)+[a-z]{2,24}$/i.test(v);
    $('bcToErr').textContent = ok ? '' : (v ? 'Geçerli bir e-posta adresi girin.' : 'Kime adresi boş olamaz.');
    $('bcTo').classList.toggle('bad', !ok); return ok;
  }
  $('bcTo').addEventListener('input', function () { if ($('bcTo').classList.contains('bad')) toOk(); });
  // alandan cikista denetim yok: hata satiri sayfayi kaydirip Gonder tiklamasini bosa dusurur. Bos birakilirsa varsayilana doner; hata Gonder'de gosterilir.
  $('bcTo').addEventListener('blur', function () { if (!$('bcTo').value.trim()) { $('bcTo').value = $('bcTo').getAttribute('data-def'); toOk(); } });

  // --- Alici secimi ---
  var EX = 0;   // analizden gelen gonderilecek ek adres sayisi
  function refreshSum() {
    var boxes = document.querySelectorAll('input[name=cid]'), p = 0, c = 0, u = 0;
    for (var i = 0; i < boxes.length; i++) if (boxes[i].checked) { if (boxes[i].getAttribute('data-grp') === 'p') p++; else c++; u += parseInt(boxes[i].getAttribute('data-users'), 10) || 0; }
    var ex = chMail.checked ? EX : 0, parts = [];
    if (p + c > 0) parts.push(p + ' bayi, ' + c + ' müşteri · ~' + u + ' kullanıcı');
    if (ex > 0) parts.push(ex + ' ek adres');
    $('bcSum').textContent = parts.length ? parts.join(' + ') : 'Seçim yok';
    return { p: p, c: c, u: u, ex: ex };
  }

  // --- Ek e-posta adresleri: sunucuda analiz (gonderimle ayni ayristirici) ---
  var exTimer = null, exSeq = 0, LAST = null;
  function esc(x) { return String(x == null ? '' : x).replace(/[&<>"']/g, function (ch) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[ch]; }); }
  function exSchedule() { clearTimeout(exTimer); exTimer = setTimeout(analyze, 450); }
  function analyze() {
    var txt = $('bcExtra').value;
    if (!txt.trim()) { EX = 0; LAST = null; $('bcAna').innerHTML = ''; $('bcExCount').textContent = ''; refreshSum(); return; }
    var fd = new URLSearchParams(); fd.append('text', txt);
    [].forEach.call(document.querySelectorAll('input[name=cid]:checked'), function (b) { fd.append('cid', b.value); });
    var seq = ++exSeq;
    fetch('BroadcastEmails.ashx', { method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: fd.toString() })
      .then(function (r) { return r.json(); }).then(function (d) { if (seq !== exSeq || !d.ok) return; LAST = d; EX = d.send.length; renderAna(d); refreshSum(); })
      .catch(function () { $('bcAna').innerHTML = '<p class="bc-ana-err">Adresler analiz edilemedi. Bağlantıyı kontrol edin.</p>'; });
  }
  function grp(key, cls, label, n, open, inner) {
    if (!n) return '';
    return '<details class="bc-g ' + cls + '" data-g="' + key + '"' + (open ? ' open' : '') + '><summary><b>' + n + '</b> ' + label + '</summary><div class="bc-gb">' + inner + '</div></details>';
  }
  function renderAna(d) {
    $('bcExCount').textContent = '(' + d.send.length + ' gönderilecek)';
    var h = '';
    if (d.truncated) h += '<p class="bc-ana-err">Liste ' + d.max + ' adres sınırını aşıyor; fazlası dikkate alınmadı.</p>';
    if (d.domains.length) h += '<div class="bc-doms">' + d.domains.slice(0, 8).map(function (x) { return '<span>' + esc(x.domain) + ' <i>' + x.count + '</i></span>'; }).join('') + (d.domains.length > 8 ? '<span class="more">+' + (d.domains.length - 8) + ' alan adı</span>' : '') + '</div>';
    h += grp('valid', 'ok', 'gönderilecek', d.valid.length, false, d.valid.map(function (x) {
      return '<span class="bc-em">' + esc(x.email) + (x.user ? '<small>Portal kullanıcısı · ' + esc(x.user) + ' · ' + esc(x.company) + '</small>' : '') + (x.role ? '<small>Kurumsal adres</small>' : '') + '</span>';
    }).join(''));
    h += grp('typos', 'warn', 'yazım hatası olabilir (gönderilmez)', d.typos.length, true, d.typos.map(function (x) {
      return '<div class="bc-fix"><span><s>' + esc(x.email) + '</s> → <b>' + esc(x.suggest) + '</b></span><button type="button" class="bc-fixbtn" data-from="' + esc(x.email) + '" data-to="' + esc(x.suggest) + '">Düzelt</button></div>';
    }).join('') + (d.typos.length > 1 ? '<button type="button" class="bc-act" data-act="fixall">Tüm önerileri uygula</button>' : ''));
    h += grp('invalid', 'bad', 'hatalı (gönderilmez)', d.invalid.length, true, d.invalid.map(function (x) { return '<div class="bc-bad"><code>' + esc(x.raw) + '</code><span>' + esc(x.reason) + '</span></div>'; }).join(''));
    h += grp('covered', 'info', 'seçili firmalarda zaten alıcı', d.covered.length, false, d.covered.map(function (x) { return '<span class="bc-em">' + esc(x.email) + '<small>' + esc(x.company) + '</small></span>'; }).join(''));
    h += grp('dup', 'info', 'tekrar eden (bir kez gönderilir)', d.duplicates.length, false, d.duplicates.map(function (x) { return '<span class="bc-em">' + esc(x.email) + '<small>' + x.count + ' kez yazılmış</small></span>'; }).join(''));
    h += grp('blocked', 'bad', 'gönderilmeyecek sistem adresi', d.blocked.length, false, d.blocked.map(function (x) { return '<span class="bc-em">' + esc(x.email) + '<small>' + esc(x.reason) + '</small></span>'; }).join(''));
    var dirty = d.typos.length + d.invalid.length + d.duplicates.length + d.blocked.length + d.covered.length;
    if (dirty && d.send.length) h += '<button type="button" class="bc-act" data-act="clean">Yalnız gönderilecekleri bırak</button>';
    $('bcAna').innerHTML = h;
  }
  function reEsc(x) { return x.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'); }
  function fixOne(from, to) {   // yalniz adresin tamami ("kayse@..." icindeki "ayse@..." degismez)
    var ta = $('bcExtra'); ta.value = ta.value.replace(new RegExp('(^|[^a-z0-9._%+\\-])' + reEsc(from) + '(?![a-z0-9.\\-])', 'gi'), function (m, pre) { return pre + to; });
  }
  $('bcAna').addEventListener('click', function (e) {
    var b = e.target.closest('.bc-fixbtn'); if (b) { fixOne(b.getAttribute('data-from'), b.getAttribute('data-to')); analyze(); return; }
    var a = e.target.closest('.bc-act'); if (!a || !LAST) return;
    if (a.getAttribute('data-act') === 'fixall') { LAST.typos.forEach(function (x) { fixOne(x.email, x.suggest); }); analyze(); }
    if (a.getAttribute('data-act') === 'clean') { $('bcExtra').value = LAST.send.join('\n'); analyze(); }
  });
  $('bcExtra').addEventListener('input', exSchedule);
  $('bcExFile').addEventListener('change', function () {
    var fl = this.files && this.files[0]; if (!fl) return;
    var rd = new FileReader(); rd.onload = function () { var ta = $('bcExtra'); ta.value = (ta.value.trim() ? ta.value.trim() + '\n' : '') + String(rd.result || ''); analyze(); };
    rd.readAsText(fl, 'utf-8'); this.value = '';
  });
  document.addEventListener('change', function (e) {
    var t = e.target;
    if (t.name === 'cid') { refreshSum(); if ($('bcExtra').value.trim()) exSchedule(); }   // "zaten alici" ayrimi secime bagli
    if (t.getAttribute && t.getAttribute('data-all')) {
      var g = t.getAttribute('data-all'), bs = document.querySelectorAll('input[name=cid][data-grp=' + g + ']');
      for (var i = 0; i < bs.length; i++) if (bs[i].closest('.bc-firm').style.display !== 'none') bs[i].checked = t.checked;
      refreshSum(); if ($('bcExtra').value.trim()) exSchedule();
    }
    if (t.name === 'theme') applyTheme();
    if (t === chApp || t === chMail) applyChannels();
  });
  var s = $('bcSearch'), free = function () { s.removeAttribute('readonly'); };
  s.addEventListener('focus', free); s.addEventListener('mousedown', free); s.addEventListener('touchstart', free);
  s.addEventListener('input', function () {
    var q = (s.value || '').toLowerCase().replace(/^\s+|\s+$/g, ''), rows = document.querySelectorAll('.bc-firm');
    for (var i = 0; i < rows.length; i++) rows[i].style.display = (!q || (rows[i].getAttribute('data-name') || '').indexOf(q) >= 0) ? '' : 'none';
  });

  var ins = document.querySelectorAll('.bc-in');
  for (var i = 0; i < ins.length; i++) ins[i].addEventListener('input', schedule);

  // window capture: yükleme overlay'inden (document capture) önce çalışır; iptalde yayılımı durdurur
  window.addEventListener('submit', function (e) {
    if (e.target !== form) return;
    var stop = function (msg) { e.preventDefault(); e.stopPropagation(); if (msg) alert(msg); };
    var r = refreshSum();
    if (r.p + r.c === 0 && !r.ex) return stop(chMail.checked ? 'Lütfen en az bir bayi / müşteri seçin ya da geçerli bir ek e-posta adresi girin.' : 'Lütfen en az bir bayi veya müşteri seçin.');
    if (!chApp.checked && !chMail.checked) return stop('Lütfen en az bir gönderim kanalı seçin.');
    var cst = theme() === 'custom' && chMail.checked;
    if (!$('bcTitle').value.trim()) return stop('Başlık zorunludur.');
    if ((!cst || chApp.checked) && !$('bcBody').value.trim()) return stop(cst ? 'Uygulama içi bildirim için metin zorunludur.' : 'Başlık ve metin zorunludur.');
    if (cst && !$('bcHtml').value.trim()) return stop('Özel HTML kodunu girin.');
    if (chMail.checked && !toOk()) { $('bcTo').focus(); return stop('Kime (To) adresi geçerli değil.'); }
    if (cst && new Blob([$('bcHtml').value]).size > 1048576) return stop('HTML 1 MB sınırını aşıyor.');
    var ch = [chApp.checked ? 'uygulama içi bildirim' : '', chMail.checked ? 'e-posta' : ''].filter(Boolean).join(' ve ');
    var who = [];
    if (r.p + r.c > 0) who.push(r.p + ' bayi ve ' + r.c + ' müşteriye (~' + r.u + ' kullanıcı) ' + ch);
    if (r.ex > 0) who.push(r.ex + ' ek adrese e-posta');
    if (!confirm(who.join(', ') + ' gönderilecek. Onaylıyor musunuz?')) stop();
  }, true);

  applyChannels(); refreshSum(); preview();
})();
</script>
<script src="js/common.js?v=186"></script>
</body>
</html>
