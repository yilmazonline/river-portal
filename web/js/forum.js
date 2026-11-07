// River Forum (Faz 1): akis, gonderi yazma (gorsel / video / bağlanti), yorum, begeni, kaydetme, sikayet, moderasyon.
// Kullanici metni her zaman kacislanir; yalniz http(s) baglantilari tiklanabilir olur.
(function () {
  var F = window.FORUM || { me: {}, mod: false, focus: 0 };
  var API = 'ForumApi.ashx', FILE = 'ForumFile.ashx?id=';
  var MAX_IMG = 4, IMG_BYTES = 5 * 1024 * 1024, VID_BYTES = 25 * 1024 * 1024;
  var $ = function (id) { return document.getElementById(id); };
  var list = $('foList'), moreBtn = $('foMore');
  var qtag = /[?&]tag=([^&]+)/.exec(location.search);
  var state = { tab: 'feed', before: 0, loading: false, tag: qtag ? decodeURIComponent(qtag[1]).toLocaleLowerCase('tr-TR') : null, tagDisp: null };
  var I = {
    heart: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 20s-7-4.4-7-10a4 4 0 0 1 7-2.6A4 4 0 0 1 19 10c0 5.6-7 10-7 10z"/></svg>',
    chat: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 5h16v11H9l-5 4z"/></svg>',
    save: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M6 3h12v18l-6-4-6 4z"/></svg>',
    dots: '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" aria-hidden="true"><path d="M5 12h.01M12 12h.01M19 12h.01"/></svg>',
    pin: '<svg width="14" height="14" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 3l2 5h5l-4 3.5 1.5 5.5-4.5-3-4.5 3L9 11.5 5 8h5z"/></svg>',
    play: '<svg width="26" height="26" viewBox="0 0 24 24" fill="#fff" aria-hidden="true"><path d="M8 5.5v13l10.5-6.5z"/></svg>',
    tick: '<svg width="12" height="12" viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M12 2l2.4 2.1 3.2-.4.9 3.1 2.8 1.6-1.2 3 1.2 3-2.8 1.6-.9 3.1-3.2-.4L12 22l-2.4-2.1-3.2.4-.9-3.1-2.8-1.6 1.2-3-1.2-3 2.8-1.6.9-3.1 3.2.4z"/><path d="M8.5 12.5l2.3 2.3 4.7-5" stroke="#fff" stroke-width="2" fill="none"/></svg>'
  };

  // ------------------------------------------------------------ yardimcilar
  function esc(s) { return String(s == null ? '' : s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;'); }
  function ago(iso) {
    var d = new Date(iso), s = (Date.now() - d.getTime()) / 1000;
    if (s < 60) return 'Az önce'; if (s < 3600) return Math.floor(s / 60) + ' dk'; if (s < 86400) return Math.floor(s / 3600) + ' sa';
    if (s < 172800) return 'Dün';
    return d.toLocaleDateString('tr-TR', { day: 'numeric', month: 'short' }) + (new Date().getFullYear() !== d.getFullYear() ? ' ' + d.getFullYear() : '');
  }
  function ava(p, size) {
    var st = 'width:' + size + 'px;height:' + size + 'px';
    if (p.ava) return '<span class="fo-ava img" style="' + st + '"><img src="' + esc(p.ava) + '" alt="" /></span>';
    // profil resmi yoksa: kisiye gore sabit renk (ayni kisi her yerde ayni renk)
    var G = ['#4F7BFF,#C23BF0', '#22B8D8,#4F7BFF', '#F07AD8,#A06BFF', '#FFB547,#F07AD8', '#2FD08A,#22B8D8', '#B14AF0,#6C5BFF', '#FF8A5B,#E4467A'];
    return '<span class="fo-ava" style="' + st + ';background:linear-gradient(135deg,' + G[(+p.id || 0) % G.length] + ')">' + esc(p.ini || '?') + '</span>';
  }
  // metin: kacislanir, sonra yalniz http(s) baglantilari ve #etiketler isaretlenir
  function rich(text) {
    var out = esc(text);
    out = out.replace(/https?:\/\/[^\s<]+[^\s<.,;:!?)\]'"]/g, function (u) { return '<a href="' + u + '" target="_blank" rel="noopener noreferrer nofollow">' + (u.length > 60 ? u.slice(0, 57) + '…' : u) + '</a>'; });
    // #etiket: tiklanabilir, akisi suzer (anahtar Turkce kucuk harf; sunucuyla ayni kural)
    out = out.replace(/(^|[\s>(])#([0-9A-Za-zÇĞİÖŞÜçğıöşü_]{2,30})/g, function (m, pre, tg) { var k = tg.toLocaleLowerCase('tr-TR'); return pre + '<a class="fo-tag" href="?tag=' + encodeURIComponent(k) + '" data-tag="' + k + '">#' + tg + '</a>'; });
    return out.replace(/\n/g, '<br>');
  }
  // ilk YouTube / Vimeo baglantisi -> oynatici karti
  function embedOf(text) {
    var m = /(?:youtube\.com\/(?:watch\?(?:[^\s]*&)?v=|shorts\/|embed\/)|youtu\.be\/)([A-Za-z0-9_-]{11})/.exec(text || '');
    if (m) return { kind: 'yt', id: m[1] };
    m = /vimeo\.com\/(?:video\/)?(\d{6,12})/.exec(text || '');
    if (m) return { kind: 'vimeo', id: m[1] };
    return null;
  }
  function embedCard(e) {
    if (!e) return '';
    var bg = e.kind === 'yt' ? '<img src="https://i.ytimg.com/vi/' + e.id + '/hqdefault.jpg" alt="" loading="lazy" />' : '<span class="fo-vimeo-bg"></span>';
    return '<button type="button" class="fo-embed" data-kind="' + e.kind + '" data-vid="' + e.id + '" aria-label="Videoyu oynat">' + bg +
           '<span class="fo-play">' + I.play + '</span><span class="fo-embed-src">' + (e.kind === 'yt' ? 'youtube.com' : 'vimeo.com') + '</span></button>';
  }
  function mediaHtml(ms) {
    if (!ms || !ms.length) return '';
    if (ms[0].kind === 'video') return '<div class="fo-vid"><video controls preload="metadata" playsinline src="' + FILE + ms[0].id + '"></video></div>';
    var h = '<div class="fo-gal n' + ms.length + '">';
    ms.forEach(function (m) { h += '<button type="button" class="fo-img" data-src="' + FILE + m.id + '" aria-label="Görseli büyüt"><img src="' + FILE + m.id + '" alt="" loading="lazy" /></button>'; });
    return h + '</div>';
  }
  function api(op, data, onProgress) {
    if (!data) return fetch(API + '?op=' + op, { credentials: 'same-origin' }).then(function (r) { return r.json(); });
    return new Promise(function (ok, bad) {
      var x = new XMLHttpRequest(); x.open('POST', API + '?op=' + op); x.withCredentials = true;
      if (onProgress && x.upload) x.upload.onprogress = function (e) { if (e.lengthComputable) onProgress(e.loaded / e.total); };
      x.onload = function () { try { ok(JSON.parse(x.responseText)); } catch (e) { bad(e); } };
      x.onerror = bad; x.send(data);
    });
  }
  // gonderi golgesi: akis ve gonderim sirasinda sayfada gorunen yer tutucu
  function ghost(label) {
    return '<div class="fo-ghost" aria-hidden="' + (label ? 'false' : 'true') + '"><div class="fo-row"><span class="fo-gh-ava"></span><div class="fo-main"><span class="fo-gh-line w40"></span><span class="fo-gh-line w90"></span><span class="fo-gh-line w70"></span>' +
           (label ? '<span class="fo-gh-label" role="status">' + esc(label) + '</span>' : '') + '</div></div></div>';
  }
  function fd(o) { var f = new FormData(); for (var k in o) if (o[k] != null) f.append(k, o[k]); return f; }
  function toast(t, bad) { var e = document.createElement('div'); e.className = 'ec-toast' + (bad ? ' bad' : ''); e.setAttribute('role', 'status'); e.textContent = t; document.body.appendChild(e); requestAnimationFrame(function () { e.classList.add('in'); }); setTimeout(function () { e.classList.remove('in'); setTimeout(function () { e.remove(); }, 300); }, 3200); }

  // ------------------------------------------------------------ gonderi
  function who(p, size) { return '<button type="button" class="fo-user" data-uid="' + p.id + '" aria-label="' + esc(p.name) + ' profili">' + ava(p, size) + '</button>'; }
  var VERIFIED = '<svg class="fo-ver" width="16" height="16" viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M12 2l2.4 2.1 3.2-.4.9 3.1 2.8 1.6-1.2 3 1.2 3-2.8 1.6-.9 3.1-3.2-.4L12 22l-2.4-2.1-3.2.4-.9-3.1-2.8-1.6 1.2-3-1.2-3 2.8-1.6.9-3.1 3.2.4z"/><path d="M8.5 12.5l2.3 2.3 4.7-5" stroke="#fff" stroke-width="2.4" fill="none" stroke-linecap="round" stroke-linejoin="round"/></svg>';
  // yalniz ad; Fol personeli: altin onay isareti (uzerine gelince "Uretici firma personeli")
  function whoName(p) {
    return '<button type="button" class="fo-user fo-uname" data-uid="' + p.id + '">' + esc(p.name) + '</button>' +
      (p.fol ? '<span class="fo-verw" title="Üretici firma personeli" aria-label="Üretici firma personeli" tabindex="0">' + VERIFIED + '</span>' : '');
  }
  var HEART_S = '<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.1" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 20s-7-4.4-7-10a4 4 0 0 1 7-2.6A4 4 0 0 1 19 10c0 5.6-7 10-7 10z"/></svg>';
  function commentHtml(c) {
    if (c.gone) return '<div class="fo-com gone" data-cid="' + c.id + '"><span class="fo-gone-ic" aria-hidden="true"></span><div class="fo-com-main"><div class="fo-com-tx">' + (c.gone === 'deleted' ? 'Bu yorum silindi.' : 'Bu yorum moderasyonla gizlendi.') + '</div></div></div>';
    return '<div class="fo-com" data-cid="' + c.id + '" data-uid="' + c.author.id + '" data-uadmin="' + (c.author.admin ? 1 : 0) + '" data-name="' + esc(c.author.name) + '"' + (c.parent ? ' data-parent="' + c.parent + '"' : '') + '>' + who(c.author, c.parent ? 28 : 32) +
      '<div class="fo-com-main"><div class="fo-head">' + whoName(c.author) +
      '<span class="fo-when">· ' + ago(c.at) + '</span>' + (c.hidden ? '<span class="fo-hid">Gizlendi</span>' : '') +
      '<button type="button" class="fo-cmenu" aria-label="Yorum menüsü" data-mine="' + (c.mine ? 1 : 0) + '" data-hidden="' + (c.hidden ? 1 : 0) + '">' + I.dots + '</button></div>' +
      '<div class="fo-com-tx">' + rich(c.body) + '</div>' +
      '<div class="fo-com-acts"><button type="button" class="fo-clike' + (c.liked ? ' on' : '') + '" aria-pressed="' + !!c.liked + '" aria-label="Yorumu beğen">' + HEART_S + '<span>' + (c.likes || '') + '</span></button>' +
      '<button type="button" class="fo-creply">Yanıtla</button></div></div></div>';
  }
  // ust duzey yorumlar ve altlarinda yanit dizileri (3'ten fazla yanitta ilk 2'si + "tumunu gor")
  function threadsHtml(items, open) {
    var tops = [], kids = {};
    items.forEach(function (c) { if (c.parent) (kids[c.parent] = kids[c.parent] || []).push(c); else tops.push(c); });
    return tops.map(function (c) {
      var r = kids[c.id] || [], show = (open && open[c.id]) || r.length <= 3;
      return '<div class="fo-thread" data-top="' + c.id + '">' + commentHtml(c) +
        (r.length ? '<div class="fo-replies">' + r.map(function (x, i) { return (!show && i >= 2) ? commentHtml(x).replace('class="fo-com"', 'class="fo-com fo-more-r"') : commentHtml(x); }).join('') +
          (!show ? '<button type="button" class="fo-rall">' + r.length + ' yanıtın tümünü gör</button>' : '') + '</div>' : '') + '</div>';
    }).join('');
  }
  function postHtml(p) {
    var a = p.author, txt = p.body || '';
    return '<article class="fo-post' + (p.pin ? ' pinned' : '') + (p.hidden ? ' is-hidden' : '') + '" data-id="' + p.id + '" data-mine="' + (p.mine ? 1 : 0) + '" data-pin="' + (p.pin ? 1 : 0) + '" data-hidden="' + (p.hidden ? 1 : 0) + '" data-body="' + esc(txt) + '" data-uid="' + a.id + '" data-uadmin="' + (a.admin ? 1 : 0) + '">' +
      (p.pin ? '<div class="fo-pin">' + I.pin + 'Sabitlenmiş</div>' : '') +
      (p.hidden ? '<div class="fo-hidnote">Bu gönderi moderasyonla gizlendi; yalnız siz ve moderatörler görüyor.</div>' : '') +
      '<div class="fo-row">' + who(a, 44) + '<div class="fo-main">' +
      '<div class="fo-head">' + whoName(a) + '<span class="fo-when" title="' + esc(new Date(p.at).toLocaleString('tr-TR')) + '">· ' + ago(p.at) + (p.edited ? ' · düzenlendi' : '') + '</span>' +
      (p.ann ? '<span class="fo-ann">Duyuru</span>' : '') + '<button type="button" class="fo-pmenu" aria-label="Gönderi menüsü">' + I.dots + '</button></div>' +
      '<div class="fo-body"' + (txt ? '' : ' hidden') + '>' + rich(txt) + '</div>' +
      ((p.media && p.media.length) ? mediaHtml(p.media) : embedCard(embedOf(txt))) +
      '<div class="fo-acts"><button type="button" class="fo-like' + (p.liked ? ' on' : '') + '" aria-pressed="' + p.liked + '" aria-label="Beğen">' + I.heart + '<span>' + p.likes + '</span></button>' +
      '<button type="button" class="fo-cbtn" aria-label="Yorumlar">' + I.chat + '<span>' + p.coms + '</span></button>' +
      '<button type="button" class="fo-save' + (p.saved ? ' on' : '') + '" aria-pressed="' + p.saved + '">' + I.save + '<span>' + (p.saved ? 'Kaydedildi' : 'Kaydet') + '</span></button></div>' +
      '<div class="fo-coms">' + (p.last ? '<div class="fo-thread" data-top="' + p.last.id + '">' + commentHtml(p.last) + '</div>' : '') +
      (p.coms > 1 ? '<button type="button" class="fo-com-all">' + p.coms + ' yorumun tamamını gör</button>' : '') +
      '<form class="fo-com-box" data-noload="1">' + ava(F.me, 32) + '<input type="text" maxlength="2000" placeholder="Yorum yazın…" aria-label="Yorum yazın" /><button type="submit" class="fo-com-send" aria-label="Gönder">Gönder</button></form></div>' +
      '</div></div></article>';
  }

  // ------------------------------------------------------------ akis
  function load(reset) {
    if (state.loading) return; state.loading = true;
    if (reset) { state.before = 0; list.innerHTML = ghost('Akış yükleniyor…') + ghost(); }
    else list.insertAdjacentHTML('beforeend', '<div class="fo-tailskel">' + ghost('Yükleniyor…') + '</div>');
    moreBtn.hidden = true;
    api('feed&tab=' + state.tab + (state.tag ? '&tag=' + encodeURIComponent(state.tag) : '') + (state.before ? '&before=' + state.before : '')).then(function (d) {
      state.loading = false;
      if (reset) list.innerHTML = ''; else { var ts = list.querySelector('.fo-tailskel'); if (ts) ts.remove(); }
      if (!d.ok) { list.innerHTML = '<div class="cu-empty"><b>Akış yüklenemedi</b><span>' + esc(d.error || '') + '</span></div>'; return; }
      // moderasyon: sayfaya gomulu degere degil, canli yanita gore (onbellekteki eski sayfadan etkilenmez)
      F.mod = !!d.mod;
      $('foModBtn').hidden = !F.mod; if (F.mod) $('foModCnt').textContent = d.reports;
      var mh = $('foModHint'); if (mh) mh.hidden = F.mod || !d.vendor;
      if (reset) banner(d);
      if (reset && !d.items.length) {
        var e = state.tag ? ['Bu etiketle gönderi yok', 'Başka bir etiket seçin ya da filtreyi kaldırın.'] : { feed: ['Henüz gönderi yok', 'İlk gönderiyi siz paylaşın: bir soru, bir ipucu ya da bir deneyim.'], announcements: ['Henüz duyuru yok', 'Fol Yazılım duyuruları burada görünecek.'], saved: ['Kaydettiğiniz gönderi yok', 'Gönderilerdeki Kaydet düğmesiyle sonra okumak için ayırabilirsiniz.'] }[state.tab];
        list.innerHTML = '<div class="cu-empty"><b>' + e[0] + '</b><span>' + e[1] + '</span></div>';
        if (F.focus > 0 && !state.focused) { state.focused = true; focusPost(F.focus); }
        return;
      }
      var h = ''; d.items.forEach(function (p) { if (!list.querySelector('[data-id="' + p.id + '"]')) h += postHtml(p); if (!p.pin || state.tab !== 'feed') state.before = p.id; });
      list.insertAdjacentHTML('beforeend', h);
      moreBtn.hidden = !d.more;
      if (reset && F.focus > 0 && !state.focused) { state.focused = true; focusPost(F.focus); }   // akis yuklendikten sonra
    }).catch(function () { state.loading = false; toast('Akış yüklenemedi, bağlantınızı kontrol edin.', true); });
  }
  moreBtn.addEventListener('click', function () { load(false); });
  // asagi indikce yukleme: "Daha fazla" dugmesi ekrana yaklasinca sonraki sayfa kendiliginden gelir
  if ('IntersectionObserver' in window) new IntersectionObserver(function (es) { if (es[0].isIntersecting && !moreBtn.hidden && !state.loading) load(false); }, { rootMargin: '600px 0px' }).observe(moreBtn);
  [].forEach.call(document.querySelectorAll('.fo-tab[data-tab]'), function (b) {
    b.addEventListener('click', function () {
      [].forEach.call(document.querySelectorAll('.fo-tab'), function (x) { x.classList.toggle('on', x === b); });
      state.tab = b.getAttribute('data-tab'); setTag(null, null, true); load(true);
    });
  });

  // ------------------------------------------------------------ etiketler: suzme ve gundemdekiler
  function tagBar() {
    var b = $('foTagBar');
    b.hidden = !state.tag;
    if (state.tag) b.innerHTML = '<span><b>#' + esc(state.tagDisp || state.tag) + '</b> etiketli gönderiler</span><button type="button" class="btn-ghost" data-untag="1">Filtreyi kaldır &times;</button>';
    [].forEach.call(document.querySelectorAll('#foTrend [data-tag]'), function (a) { a.classList.toggle('on', a.getAttribute('data-tag') === state.tag); });
  }
  function setTag(key, disp, silent) {
    state.tag = key; state.tagDisp = disp;
    var url = location.pathname + (key ? '?tag=' + encodeURIComponent(key) : '');
    if (!silent) history.pushState({ tag: key, disp: disp }, '', url); else history.replaceState({ tag: key, disp: disp }, '', url);
    tagBar();
  }
  function goTag(key, disp) {
    if (prof.classList.contains('open')) prof.classList.remove('open');
    [].forEach.call(document.querySelectorAll('.fo-tab[data-tab]'), function (x) { x.classList.toggle('on', x.getAttribute('data-tab') === 'feed'); });
    state.tab = 'feed'; setTag(key, disp); load(true); window.scrollTo({ top: 0, behavior: 'smooth' });
  }
  document.addEventListener('click', function (e) {
    var a = e.target.closest ? e.target.closest('a.fo-tag, #foTrend [data-tag]') : null;
    if (a) { e.preventDefault(); var k = a.getAttribute('data-tag'); goTag(k, a.getAttribute('data-disp') || (a.textContent || '').replace(/^#/, '')); return; }
    if (e.target.closest && e.target.closest('[data-untag]')) { setTag(null, null); load(true); }
  });
  window.addEventListener('popstate', function (e) { var s = e.state || {}; state.tag = s.tag || null; state.tagDisp = s.disp || null; tagBar(); load(true); });
  function trending() {
    api('trending').then(function (d) {
      if (!d.ok) return;
      $('foTrend').innerHTML = d.items.length ? d.items.map(function (x, i) { return '<a href="?tag=' + encodeURIComponent(x.tag) + '" data-tag="' + esc(x.tag) + '" data-disp="' + esc(x.display) + '"><em>' + (i + 1) + '</em><span><b>#' + esc(x.display) + '</b><small>' + x.count + ' gönderi</small></span></a>'; }).join('')
        : '<span class="fo-trend-empty">Son 30 günde etiket kullanılmadı. Gönderinize #etiket ekleyerek gündemi başlatın.</span>';
      tagBar();
    });
  }

  // ------------------------------------------------------------ topluluk kurallari: ilk giriste onay
  var rules = $('foRules'), mustAgree = !!F.consent;
  function openRules(consent) {
    mustAgree = !!consent; rules.classList.add('open'); rules.classList.toggle('consent', mustAgree);
    $('foConsent').hidden = !mustAgree; $('foRulesX').hidden = mustAgree;
    rules.querySelector('.fo-rules-body').scrollTop = 0;
    (mustAgree ? $('foAgree') : $('foRulesX')).focus();
  }
  function closeRules() { if (mustAgree) return; rules.classList.remove('open'); }
  $('foRulesBtn').addEventListener('click', function () { openRules(false); });
  $('foRulesX').addEventListener('click', closeRules);
  rules.addEventListener('click', function (e) { if (e.target === rules) closeRules(); });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && rules.classList.contains('open')) closeRules(); });
  $('foAgree').addEventListener('change', function () { $('foAgreeBtn').disabled = !this.checked; });
  $('foConsent').addEventListener('submit', function (e) {
    e.preventDefault(); if (!$('foAgree').checked) return;
    var b = $('foAgreeBtn'); b.disabled = true;
    api('accept', fd({ agree: 1 })).then(function (d) {
      if (!d.ok) { b.disabled = false; toast(d.error || 'Onay kaydedilemedi.', true); return; }
      mustAgree = false; F.consent = false; rules.classList.remove('open', 'consent'); toast('Teşekkürler, foruma hoş geldiniz.');
    }).catch(function () { b.disabled = false; toast('Bağlantı hatası, tekrar deneyin.', true); });
  });

  // bildirimden gelinen gonderi: en ustte, yorumlari acik
  function focusPost(id) {
    api('post&id=' + id).then(function (d) {
      if (!d.ok) { toast(d.error || 'Gönderi bulunamadı.', true); return; }
      var old = list.querySelector('[data-id="' + id + '"]'); if (old) old.remove();
      list.insertAdjacentHTML('afterbegin', postHtml(d.item));
      var el = list.querySelector('[data-id="' + id + '"]'); el.classList.add('focus');
      renderComments(el, d.comments);
      var cq = /[?&]c=(\d+)/.exec(location.search), ce = cq ? el.querySelector('.fo-com[data-cid="' + cq[1] + '"]') : null;
      if (ce) { var th = ce.closest('.fo-replies'); if (th) { var ra = th.querySelector('.fo-rall'); if (ra) ra.click(); } ce.classList.add('flash'); ce.scrollIntoView({ behavior: 'smooth', block: 'center' }); }
      else el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
  }

  function banner(d) {
    var b = $('foBanner'), ro = false, h = '';
    if (d.closed) { h = '<b>Forum yeni paylaşımlara kapalı.</b> ' + (F.mod ? 'Yalnız forum adminleri paylaşım yapabiliyor.' : 'Gönderileri okumaya devam edebilirsiniz.') + (d.closedNote ? ' <span>' + esc(d.closedNote) + '</span>' : ''); ro = !F.mod; }
    else if (d.restriction) { var r = d.restriction; h = '<b>Forumda paylaşım hakkınız ' + (r.until ? new Date(r.until).toLocaleString('tr-TR', { day: 'numeric', month: 'long', hour: '2-digit', minute: '2-digit' }) + ' tarihine kadar' : 'süresiz olarak') + ' kısıtlandı.</b>' + (r.reason ? ' <span>Açıklama: ' + esc(r.reason) + '</span>' : ''); ro = true; }
    b.hidden = !h; b.innerHTML = h; b.className = 'fo-banner' + (d.closed ? ' closed' : ' restr');
    document.body.classList.toggle('fo-ro', ro);
  }

  // ------------------------------------------------------------ yorumlar
  function renderComments(el, items, keepOpen) {
    var box = el.querySelector('.fo-coms'), form = box.querySelector('.fo-com-box'), open = {};
    [].forEach.call(box.querySelectorAll('.fo-thread'), function (th) { if (!th.querySelector('.fo-rall')) open[th.getAttribute('data-top')] = 1; });
    if (keepOpen) open[keepOpen] = 1;
    [].forEach.call(box.querySelectorAll('.fo-thread, .fo-com-all'), function (n) { n.remove(); });
    form.insertAdjacentHTML('beforebegin', threadsHtml(items, open));
    el.setAttribute('data-full', '1');
    el.querySelector('.fo-cbtn span').textContent = items.filter(function (c) { return !c.hidden && !c.gone; }).length;
  }
  function withComments(el) {
    if (el.getAttribute('data-full') === '1') return Promise.resolve();
    return api('comments&id=' + el.getAttribute('data-id')).then(function (d) { if (d.ok) renderComments(el, d.items); });
  }
  // yanit kutusu: dizinin altinda; yanita yanitta "@Ad " ile baslar
  function openReply(el, com) {
    var top = com.getAttribute('data-parent') || com.getAttribute('data-cid'), name = com.getAttribute('data-name');
    withComments(el).then(function () {
      var th = el.querySelector('.fo-thread[data-top="' + top + '"]'); if (!th) return;
      var old = el.querySelector('.fo-reply-box'); if (old) old.remove();
      var rp = th.querySelector('.fo-replies'); if (!rp) { rp = document.createElement('div'); rp.className = 'fo-replies'; th.appendChild(rp); }
      var all = rp.querySelector('.fo-rall'); if (all) all.click();
      var f = document.createElement('form'); f.className = 'fo-reply-box'; f.setAttribute('data-noload', '1'); f.setAttribute('data-parent', top);
      f.innerHTML = ava(F.me, 28) + '<input type="text" maxlength="2000" placeholder="' + esc(name) + ' adlı kişiye yanıt yazın…" aria-label="Yanıt yazın" /><button type="submit" class="fo-com-send">Yanıtla</button>';
      rp.appendChild(f);
      var inp = f.querySelector('input');
      if (com.getAttribute('data-parent') && name) inp.value = '@' + name + ' ';
      inp.focus(); inp.setSelectionRange(inp.value.length, inp.value.length);
    });
  }

  // ------------------------------------------------------------ olaylar (akis)
  function onClick(e) {
    var t = e.target;
    if (t.closest('.fo-user')) { openProfile(+t.closest('.fo-user').getAttribute('data-uid')); return; }
    var el = t.closest('.fo-post'); if (!el) return;
    var id = el.getAttribute('data-id');
    if (t.closest('.fo-like')) {
      var b = t.closest('.fo-like'), n = b.querySelector('span'), on = !b.classList.contains('on');
      b.classList.toggle('on', on); b.setAttribute('aria-pressed', on); n.textContent = Math.max(0, +n.textContent + (on ? 1 : -1));
      api('like', fd({ id: id })).then(function (d) { if (d.ok) { b.classList.toggle('on', d.on); n.textContent = d.count; } });
      return;
    }
    if (t.closest('.fo-save')) {
      var s = t.closest('.fo-save');
      api('save', fd({ id: id })).then(function (d) { if (!d.ok) return; s.classList.toggle('on', d.on); s.querySelector('span').textContent = d.on ? 'Kaydedildi' : 'Kaydet'; if (!d.on && state.tab === 'saved') el.remove(); });
      return;
    }
    if (t.closest('.fo-clike')) {
      var cb2 = t.closest('.fo-clike'), cn = cb2.querySelector('span'), con = !cb2.classList.contains('on'), cid0 = cb2.closest('.fo-com').getAttribute('data-cid');
      cb2.classList.toggle('on', con); cb2.setAttribute('aria-pressed', con); var nv = Math.max(0, (+cn.textContent || 0) + (con ? 1 : -1)); cn.textContent = nv || '';
      api('clike', fd({ id: cid0 })).then(function (d) { if (d.ok) { cb2.classList.toggle('on', d.on); cn.textContent = d.count || ''; } });
      return;
    }
    if (t.closest('.fo-creply')) { openReply(el, t.closest('.fo-com')); return; }
    if (t.closest('.fo-rall')) { var rp0 = t.closest('.fo-replies'); [].forEach.call(rp0.querySelectorAll('.fo-more-r'), function (n) { n.classList.remove('fo-more-r'); }); t.closest('.fo-rall').remove(); return; }
    if (t.closest('.fo-cbtn') || t.closest('.fo-com-all')) {
      api('comments&id=' + id).then(function (d) { if (d.ok) { renderComments(el, d.items); var i = el.querySelector('.fo-com-box input'); if (i) i.focus(); } });
      return;
    }
    if (t.closest('.fo-img')) { openLb(t.closest('.fo-img').getAttribute('data-src')); return; }
    if (t.closest('.fo-embed')) {
      var em = t.closest('.fo-embed'), k = em.getAttribute('data-kind'), v = em.getAttribute('data-vid');
      var src = k === 'yt' ? 'https://www.youtube-nocookie.com/embed/' + v + '?autoplay=1&rel=0' : 'https://player.vimeo.com/video/' + v + '?autoplay=1&dnt=1';
      var wrap = document.createElement('div'); wrap.className = 'fo-embed-on';
      wrap.innerHTML = '<iframe src="' + src + '" title="Video" allow="autoplay; encrypted-media; picture-in-picture; fullscreen" allowfullscreen referrerpolicy="strict-origin-when-cross-origin"></iframe>';
      em.replaceWith(wrap); return;
    }
    if (t.closest('.fo-pmenu')) { menu(t.closest('.fo-pmenu'), postMenu(el)); return; }
    if (t.closest('.fo-cmenu')) { var cb = t.closest('.fo-cmenu'); menu(cb, commentMenu(el, cb.closest('.fo-com'), cb)); return; }
  }
  function onSubmit(e) {
    if (e.target.closest('.fo-edit')) { e.preventDefault(); saveEdit(e.target.closest('.fo-post')); return; }
    var rb = e.target.closest('.fo-reply-box');
    if (rb) {
      e.preventDefault();
      var pel = rb.closest('.fo-post'), ri = rb.querySelector('input'), rv = ri.value.trim(), top = rb.getAttribute('data-parent'); if (!rv) return;
      ri.disabled = true;
      api('comment', fd({ id: pel.getAttribute('data-id'), body: rv, parent: top })).then(function (d) {
        ri.disabled = false;
        if (!d.ok) { toast(d.error || 'Yanıt gönderilemedi.', true); return; }
        renderComments(pel, d.items, top);
        var last = pel.querySelector('.fo-thread[data-top="' + top + '"] .fo-replies .fo-com:last-of-type'); if (last) { last.classList.add('flash'); }
      }).catch(function () { ri.disabled = false; toast('Bağlantı hatası, tekrar deneyin.', true); });
      return;
    }
    var f = e.target.closest('.fo-com-box'); if (!f) return; e.preventDefault();
    var el = f.closest('.fo-post'), inp = f.querySelector('input'), v = inp.value.trim(); if (!v) return;
    inp.disabled = true;
    api('comment', fd({ id: el.getAttribute('data-id'), body: v })).then(function (d) {
      inp.disabled = false;
      if (!d.ok) { toast(d.error || 'Yorum gönderilemedi.', true); return; }
      inp.value = ''; renderComments(el, d.items); inp.focus();
    }).catch(function () { inp.disabled = false; toast('Bağlantı hatası, tekrar deneyin.', true); });
  }
  list.addEventListener('click', onClick); list.addEventListener('submit', onSubmit);

  // ------------------------------------------------------------ aciklama ve kisitlama penceresi
  function ask(opt) {
    return new Promise(function (resolve) {
      var m = document.createElement('div'); m.className = 'pw-modal open fo-ask'; m.setAttribute('role', 'dialog'); m.setAttribute('aria-modal', 'true');
      m.innerHTML = '<form class="pw-box fo-ask-box" data-noload="1"><button type="button" class="pw-x" data-x="1" aria-label="Kapat">&times;</button><h3>' + esc(opt.title) + '</h3>' +
        (opt.text ? '<p class="fo-ask-sub">' + esc(opt.text) + '</p>' : '') +
        (opt.days ? '<label class="fo-ask-l"><span class="fo-ask-t">Süre</span><select name="days"><option value="1">24 saat</option><option value="7" selected>7 gün</option><option value="30">30 gün</option><option value="0">Süresiz</option></select></label>' : '') +
        '<label class="fo-ask-l"><span class="fo-ask-t">' + esc(opt.label || 'Açıklama') + ' <i>(isteğe bağlı)</i></span><textarea name="reason" rows="3" maxlength="1000" placeholder="' + esc(opt.placeholder || '') + '"></textarea></label>' +
        (opt.hint ? '<p class="fo-ask-hint">' + esc(opt.hint) + '</p>' : '') +
        '<div class="fo-ask-act"><button type="button" class="btn-ghost" data-x="1">Vazgeç</button><button type="submit" class="btn-cyan' + (opt.danger ? ' danger' : '') + '">' + esc(opt.ok) + '</button></div></form>';
      document.body.appendChild(m);
      var fm = m.querySelector('form'); (fm.querySelector('select') || fm.querySelector('textarea')).focus();
      function done(v) { m.remove(); resolve(v); }
      m.addEventListener('click', function (e) { if (e.target === m || e.target.closest('[data-x]')) done(null); });
      m.addEventListener('keydown', function (e) { if (e.key === 'Escape') { e.stopPropagation(); done(null); } });
      fm.addEventListener('submit', function (e) { e.preventDefault(); done({ reason: fm.reason.value.trim(), days: fm.days ? fm.days.value : null }); });
    });
  }
  function restrictUser(uid, name) {
    ask({ title: name + ' · kısıtla', text: 'Kişi forumu okumaya devam eder; süre boyunca gönderi ve yorum yazamaz.', days: true, label: 'Gerekçe', placeholder: 'Örn. topluluk kurallarına aykırı paylaşımlar', hint: 'Gerekçe kişiye uygulama içi bildirim olarak gider.', ok: 'Kısıtla', danger: true })
      .then(function (v) { if (!v) return; api('restrict', fd({ user: uid, days: v.days, reason: v.reason })).then(function (d) { toast(d.ok ? 'Kullanıcı kısıtlandı; kendisine bildirim gönderildi.' : (d.error || 'Kısıtlanamadı.'), !d.ok); }); });
  }

  // ------------------------------------------------------------ menuler
  var pop = null;
  function closeMenu() { if (pop) { pop.remove(); pop = null; } }
  function menu(btn, items) {
    closeMenu(); if (!items.length) return;
    pop = document.createElement('div'); pop.className = 'fo-menu'; pop.setAttribute('role', 'menu');
    items.forEach(function (it) { var b = document.createElement('button'); b.type = 'button'; b.setAttribute('role', 'menuitem'); b.className = it.danger ? 'danger' : ''; b.textContent = it.label; b.addEventListener('click', function () { closeMenu(); it.run(); }); pop.appendChild(b); });
    document.body.appendChild(pop);
    var r = btn.getBoundingClientRect();
    pop.style.top = (r.bottom + window.scrollY + 4) + 'px';
    pop.style.left = Math.max(8 + window.scrollX, Math.min(window.scrollX + document.documentElement.clientWidth - pop.offsetWidth - 8, r.right + window.scrollX - pop.offsetWidth)) + 'px';   // ekran disina tasmaz
    pop.querySelector('button').focus();
  }
  document.addEventListener('click', function (e) { if (pop && !e.target.closest('.fo-menu') && !e.target.closest('.fo-pmenu, .fo-cmenu')) closeMenu(); });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape') { closeMenu(); closeLb(); } });

  function postMenu(el) {
    var id = el.getAttribute('data-id'), mine = el.getAttribute('data-mine') === '1', items = [];
    items.push({ label: 'Bağlantıyı kopyala', run: function () { var u = location.origin + location.pathname + '?post=' + id; (navigator.clipboard ? navigator.clipboard.writeText(u) : Promise.reject()).then(function () { toast('Bağlantı kopyalandı.'); }, function () { prompt('Bağlantı', u); }); } });
    if (F.mod) {
      var pinned = el.getAttribute('data-pin') === '1';
      items.push({ label: pinned ? 'Sabitlemeyi kaldır' : 'En üste sabitle', run: function () { api('pin', fd({ id: id, on: pinned ? 0 : 1 })).then(function (d) { if (d.ok) { toast(pinned ? 'Sabitleme kaldırıldı.' : 'Gönderi sabitlendi.'); if (state.tab === 'feed') load(true); } }); } });
      if (el.getAttribute('data-hidden') === '1') items.push({ label: 'Gizlemeyi kaldır', run: function () { api('unhide', fd({ id: id })).then(function (d) { if (d.ok) { el.classList.remove('is-hidden'); var n = el.querySelector('.fo-hidnote'); if (n) n.remove(); el.setAttribute('data-hidden', '0'); toast('Gönderi yeniden görünür.'); } }); } });
    }
    if (mine) items.push({ label: 'Düzenle', run: function () { startEdit(el); } });
    if (!mine) items.push({ label: 'Şikayet et', run: function () { report(id, null); } });
    if (F.mod && !mine && el.getAttribute('data-uadmin') !== '1') items.push({ label: 'Kullanıcıyı kısıtla', run: function () { restrictUser(el.getAttribute('data-uid'), el.querySelector('.fo-uname').textContent); } });
    if (mine) items.push({ label: 'Gönderiyi sil', danger: true, run: function () { if (!confirm('Gönderi silinsin mi? Bu işlem geri alınamaz.')) return; api('delpost', fd({ id: id })).then(function (d) { if (d.ok) { el.remove(); toast('Gönderi silindi.'); } }); } });
    else if (F.mod) items.push({ label: 'Gönderiyi kaldır', danger: true, run: function () {
      ask({ title: 'Gönderiyi kaldır', text: 'Gönderi akıştan kaldırılır ve moderasyon kaydına girer.', placeholder: 'Örn. kişisel veri içeriyordu', hint: 'Açıklama içerik sahibine uygulama içi bildirim olarak gider.', ok: 'Kaldır', danger: true })
        .then(function (v) { if (!v) return; api('delpost', fd({ id: id, reason: v.reason })).then(function (d) { if (d.ok) { el.remove(); toast('Gönderi kaldırıldı; sahibine bildirim gönderildi.'); } }); });
    } });
    return items;
  }
  function startEdit(el) {
    if (el.querySelector('.fo-edit')) return;
    var body = el.querySelector('.fo-body'); body.hidden = true;
    var f = document.createElement('form'); f.className = 'fo-edit'; f.setAttribute('data-noload', '1');
    f.innerHTML = '<textarea maxlength="4000" aria-label="Gönderiyi düzenle"></textarea><div class="fo-edit-act"><button type="button" class="btn-ghost" data-cancel="1">Vazgeç</button><button type="submit" class="btn-cyan">Kaydet</button></div>';
    var ta = f.querySelector('textarea'); ta.value = el.getAttribute('data-body') || '';
    body.insertAdjacentElement('afterend', f); ta.focus(); ta.style.height = Math.min(ta.scrollHeight + 4, 320) + 'px';
    f.querySelector('[data-cancel]').addEventListener('click', function () { f.remove(); body.hidden = !(el.getAttribute('data-body')); });
  }
  function saveEdit(el) {
    var f = el.querySelector('.fo-edit'), ta = f.querySelector('textarea'), btn = f.querySelector('[type=submit]'); btn.disabled = true;
    api('edit', fd({ id: el.getAttribute('data-id'), body: ta.value })).then(function (d) {
      if (!d.ok) { btn.disabled = false; toast(d.error || 'Gönderi düzenlenemedi.', true); return; }
      var tmp = document.createElement('div'); tmp.innerHTML = postHtml(d.item); var n = tmp.firstChild;
      var coms = el.querySelector('.fo-coms'); n.querySelector('.fo-coms').replaceWith(coms);   // acik yorumlar korunur
      el.replaceWith(n); toast('Gönderi güncellendi.');
    }).catch(function () { btn.disabled = false; toast('Bağlantı hatası, tekrar deneyin.', true); });
  }
  function commentMenu(el, com, btn) {
    var id = el.getAttribute('data-id'), cid = com.getAttribute('data-cid'), mine = btn.getAttribute('data-mine') === '1', items = [];
    if (F.mod && btn.getAttribute('data-hidden') === '1') items.push({ label: 'Gizlemeyi kaldır', run: function () { api('unhide', fd({ id: id, comment: cid })).then(function () { api('comments&id=' + id).then(function (d) { if (d.ok) renderComments(el, d.items); }); }); } });
    if (!mine) items.push({ label: 'Şikayet et', run: function () { report(id, cid); } });
    function after() { api('comments&id=' + id).then(function (x) { if (x.ok) renderComments(el, x.items); }); }
    if (F.mod && !mine && com.getAttribute('data-uadmin') !== '1') items.push({ label: 'Kullanıcıyı kısıtla', run: function () { restrictUser(com.getAttribute('data-uid'), com.querySelector('.fo-uname').textContent); } });
    if (mine) items.push({ label: 'Yorumu sil', danger: true, run: function () { api('delcomment', fd({ id: cid })).then(function (d) { if (d.ok) after(); }); } });
    else if (F.mod) items.push({ label: 'Yorumu kaldır', danger: true, run: function () {
      ask({ title: 'Yorumu kaldır', text: 'Yorum kaldırılır ve moderasyon kaydına girer.', hint: 'Açıklama içerik sahibine uygulama içi bildirim olarak gider.', ok: 'Kaldır', danger: true })
        .then(function (v) { if (!v) return; api('delcomment', fd({ id: cid, reason: v.reason })).then(function (d) { if (d.ok) { after(); toast('Yorum kaldırıldı; sahibine bildirim gönderildi.'); } }); });
    } });
    return items;
  }
  function report(id, cid) {
    var r = prompt('Neden bildiriyorsunuz? (isteğe bağlı)\nÖrn. kişisel veri içeriyor, uygunsuz, reklam');
    if (r === null) return;
    api('report', fd({ id: id, comment: cid, reason: r })).then(function (d) { toast(d.ok ? 'Teşekkürler, moderatörlere iletildi.' : (d.error || 'Bildirim gönderilemedi.'), !d.ok); });
  }

  // ------------------------------------------------------------ gorsel goruntuleyici
  var lb = $('foLb');
  function openLb(src) { lb.querySelector('img').src = src; lb.hidden = false; lb.querySelector('.fo-lb-x').focus(); }
  function closeLb() { if (!lb.hidden) { lb.hidden = true; lb.querySelector('img').src = ''; } }
  lb.addEventListener('click', function (e) { if (e.target === lb || e.target.closest('.fo-lb-x')) closeLb(); });

  // ------------------------------------------------------------ gonderi yazma
  var text = $('foText'), send = $('foSend'), msg = $('foMsg'), files = [], vfile = null;
  $('foMeAva').outerHTML = ava(F.me, 44);
  function msgSet(t, warn) { msg.textContent = t || ''; msg.className = 'fo-c-msg' + (warn ? ' warn' : ''); }
  function refresh() {
    var len = text.value.length;
    $('foCount').textContent = len > 3500 ? (4000 - len) + '' : '';
    send.disabled = !(text.value.trim() || files.length || vfile);
    var e = files.length || vfile ? null : embedOf(text.value), p = $('foEmbedPrev');
    p.hidden = !e; p.innerHTML = e ? embedCard(e).replace('<button', '<div').replace('</button>', '</div>') + '<small>Oynatıcı gönderide kendiliğinden oluşur.</small>' : '';
    var h = '';
    files.forEach(function (f, i) { h += '<span class="fo-thumb"><img src="' + f.url + '" alt="" /><button type="button" data-rm="' + i + '" aria-label="Görseli kaldır">&times;</button></span>'; });
    if (vfile) h += '<span class="fo-thumb vid"><span>' + I.play + '</span><em>' + esc(vfile.file.name) + ' · ' + (vfile.file.size / 1048576).toFixed(1) + ' MB</em><button type="button" data-rm="v" aria-label="Videoyu kaldır">&times;</button></span>';
    $('foFiles').innerHTML = h;
  }
  text.addEventListener('input', function () { text.style.height = 'auto'; text.style.height = Math.min(text.scrollHeight, 320) + 'px'; refresh(); });
  $('foFiles').addEventListener('click', function (e) {
    var b = e.target.closest('[data-rm]'); if (!b) return; var k = b.getAttribute('data-rm');
    if (k === 'v') vfile = null; else { URL.revokeObjectURL(files[+k].url); files.splice(+k, 1); }
    msgSet(''); refresh();
  });
  $('foImg').addEventListener('change', function () {
    msgSet('');
    if (vfile) { msgSet('Bir gönderiye ya görseller ya da tek bir video eklenebilir.', true); this.value = ''; return; }
    [].forEach.call(this.files, function (f) {
      if (files.length >= MAX_IMG) { msgSet('En fazla 4 görsel ekleyebilirsiniz.', true); return; }
      if (f.size > IMG_BYTES) { msgSet(f.name + ' 5 MB sınırını aşıyor.', true); return; }
      files.push({ file: f, url: URL.createObjectURL(f) });
    });
    this.value = ''; refresh();
  });
  $('foVid').addEventListener('change', function () {
    var f = this.files[0]; this.value = ''; msgSet(''); if (!f) return;
    if (files.length) { msgSet('Bir gönderiye ya görseller ya da tek bir video eklenebilir.', true); return; }
    if (f.size > VID_BYTES) { msgSet('Bu video ' + (f.size / 1048576).toFixed(0) + ' MB; sınır 25 MB. Videoyu YouTube ya da Vimeo\'ya yükleyip bağlantısını gönderinize yapıştırabilirsiniz; oynatıcı kendiliğinden oluşur.', true); return; }
    vfile = { file: f }; refresh();
  });
  $('foLink').addEventListener('click', function () { msgSet('YouTube ya da Vimeo bağlantısını metne yapıştırın; oynatıcı gönderide kendiliğinden oluşur.'); text.focus(); });
  $('foCompose').addEventListener('submit', function (e) {
    e.preventDefault(); if (send.disabled) return;
    var data = new FormData(); data.append('body', text.value);
    files.forEach(function (f) { data.append('files', f.file, f.file.name); });
    if (vfile) data.append('files', vfile.file, vfile.file.name);
    if ($('foAnn') && $('foAnn').checked) data.append('ann', '1');
    if ($('foPin') && $('foPin').checked) data.append('pin', '1');
    // akista, sabitlenmislerin altinda gonderinin golgesi ve ilerleme
    var gh = document.createElement('div'); gh.innerHTML = ghost('Gönderiliyor…'); gh = gh.firstChild; gh.classList.add('sending');
    var empty0 = list.querySelector('.cu-empty'); if (empty0) empty0.remove();
    var firstReg0 = list.querySelector('.fo-post:not(.pinned)'); if (firstReg0) list.insertBefore(gh, firstReg0); else list.appendChild(gh);
    var lbl = gh.querySelector('.fo-gh-label'), big = files.length || vfile;
    send.disabled = true; send.textContent = 'Paylaşılıyor…';
    api('create', data, function (p) { if (big) lbl.textContent = 'Gönderiliyor… %' + Math.round(p * 100); }).then(function (d) {
      send.textContent = 'Paylaş';
      if (!d.ok) { gh.remove(); send.disabled = false; msgSet(d.error || 'Gönderi paylaşılamadı.', true); return; }
      files.forEach(function (f) { URL.revokeObjectURL(f.url); }); files = []; vfile = null; text.value = ''; text.style.height = 'auto';
      if ($('foAnn')) $('foAnn').checked = false; if ($('foPin')) $('foPin').checked = false;
      msgSet(''); refresh();
      if (state.tab === 'feed' || (state.tab === 'announcements' && d.item.ann)) {
        var tmp = document.createElement('div'); tmp.innerHTML = postHtml(d.item); var n = tmp.firstChild; n.classList.add('arrive');
        if (d.item.pin) { gh.remove(); list.insertBefore(n, list.firstChild); } else gh.replaceWith(n);   // golgenin yerini gonderi alir
      } else gh.remove();
      toast('Gönderiniz paylaşıldı.'); trending();
    }).catch(function () { gh.remove(); send.textContent = 'Paylaş'; send.disabled = false; msgSet('Bağlantı hatası, tekrar deneyin.', true); });
  });

  // ------------------------------------------------------------ profil
  var prof = $('foProf'), pList = $('foProfList'), pMore = $('foProfMore'), pst = { uid: 0, before: 0, loading: false, more: false };
  function openProfile(uid) {
    if (!uid) return; closeMenu();
    pst = { uid: uid, before: 0, loading: false, more: false };
    $('foProfHead').innerHTML = ghost('Profil yükleniyor…'); $('foProfAch').innerHTML = ''; pList.innerHTML = ''; pMore.hidden = true;
    prof.classList.add('open'); prof.querySelector('.pw-x').focus();
    api('profile&id=' + uid).then(function (d) {
      if (!d.ok) { $('foProfHead').innerHTML = '<div class="cu-empty"><b>' + esc(d.error || 'Profil açılamadı') + '</b></div>'; return; }
      var p = d.profile, u = p.user;
      $('foProfHead').innerHTML = '<div class="fo-ph">' + ava(u, 84) + '<div><h3 id="foProfT">' + esc(u.name) + (p.me ? ' <small>(siz)</small>' : '') + '</h3>' +
        '<div class="fo-ph-stats"><span><b>' + p.posts + '</b> gönderi</span><span><b>' + p.likes + '</b> beğeni</span>' + (p.since ? '<span>İlk gönderi: ' + new Date(p.since).toLocaleDateString('tr-TR', { month: 'long', year: 'numeric' }) + '</span>' : '') + '</div></div></div>';
      var a = '';
      if (!p.shared) a = '<div class="fo-ach-off">Bu kullanıcı başarılarını paylaşmıyor.</div>';
      else {
        a += '<div class="fo-ach"><h4>Rozetler <span>' + p.badges.length + '</span></h4>' + (p.badges.length ? '<div class="fo-badges">' + p.badges.map(function (b) { return '<span class="fo-bdg" title="' + esc(b.title) + ' · ' + new Date(b.at).toLocaleDateString('tr-TR') + '">' + b.svg + '<em>' + esc(b.title) + '</em></span>'; }).join('') + '</div>' : '<p class="fo-ach-none">Henüz rozet yok.</p>') + '</div>';
        a += '<div class="fo-ach"><h4>Sertifikalar <span>' + p.certs.length + '</span></h4>' + (p.certs.length ? p.certs.map(function (c) { return '<div class="fo-cert"><i style="' + (c.color ? 'background:' + esc(c.color) : '') + '">✓</i><div><b>' + esc(c.title) + '</b><small>' + esc(c.no) + ' · ' + new Date(c.at).toLocaleDateString('tr-TR') + '</small></div></div>'; }).join('') : '<p class="fo-ach-none">Henüz sertifika yok.</p>') + '</div>';
      }
      $('foProfAch').innerHTML = a;
      profLoad();
    }).catch(function () { $('foProfHead').innerHTML = '<div class="cu-empty"><b>Bağlantı hatası</b></div>'; });
  }
  function profLoad() {
    if (pst.loading) return; pst.loading = true; var uid = pst.uid;
    pList.insertAdjacentHTML('beforeend', '<div class="fo-tailskel">' + ghost(pst.before ? 'Yükleniyor…' : 'Gönderiler yükleniyor…') + '</div>'); pMore.hidden = true;
    api('feed&tab=feed&author=' + uid + (pst.before ? '&before=' + pst.before : '')).then(function (d) {
      if (uid !== pst.uid) return;   // bu arada baska profil acildi
      pst.loading = false; var ts = pList.querySelector('.fo-tailskel'); if (ts) ts.remove();
      if (!d.ok) return;
      if (!pst.before && !d.items.length) { pList.innerHTML = '<div class="cu-empty"><b>Henüz gönderi yok</b></div>'; return; }
      pList.insertAdjacentHTML('beforeend', d.items.map(postHtml).join(''));
      if (d.items.length) pst.before = d.items[d.items.length - 1].id;
      pst.more = d.more; pMore.hidden = !d.more;
    }).catch(function () { pst.loading = false; });
  }
  pMore.addEventListener('click', profLoad);
  if ('IntersectionObserver' in window) new IntersectionObserver(function (es) { if (es[0].isIntersecting && pst.more && !pst.loading) profLoad(); }, { root: prof.querySelector('.fo-prof-box'), rootMargin: '400px 0px' }).observe(pMore);
  pList.addEventListener('click', onClick); pList.addEventListener('submit', onSubmit);
  prof.addEventListener('click', function (e) { if (e.target === prof || e.target.closest('[data-close]')) prof.classList.remove('open'); });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && prof.classList.contains('open')) prof.classList.remove('open'); });

  // ------------------------------------------------------------ moderasyon
  var modModal = $('foModModal');
  function modLoad() {
    $('foModList').innerHTML = '<div class="fo-skel sm"></div>';
    api('reports').then(function (d) {
      if (!d.ok) return;
      $('foModCnt').textContent = d.items.length;
      if (!d.items.length) { $('foModList').innerHTML = '<div class="cu-empty"><b>Bekleyen bildirim yok</b><span>Topluluk temiz görünüyor.</span></div>'; return; }
      $('foModList').innerHTML = d.items.map(function (r) {
        return '<div class="fo-rep" data-post="' + r.post + '" data-comment="' + (r.comment || '') + '"><div class="fo-rep-h"><b>' + (r.comment ? 'Yorum' : 'Gönderi') + ' · ' + esc(r.author) + '</b><span>' + r.count + ' bildirim · ' + ago(r.at) + '</span></div>' +
               '<div class="fo-rep-tx">' + esc(r.body) + '</div>' + (r.reason ? '<div class="fo-rep-why">Neden: ' + esc(r.reason) + '</div>' : '') +
               '<div class="fo-rep-act"><button type="button" class="btn-ghost" data-view="1" data-post="' + r.post + '" data-comment="' + (r.comment || '') + '">Gör</button><button type="button" class="btn-ghost" data-act="0">Yoksay</button><button type="button" class="btn-cyan" data-act="1">Gizle</button></div></div>';
      }).join('');
    });
  }
  var ACT = { delete_post: 'Gönderi kaldırıldı', delete_comment: 'Yorum kaldırıldı', hide_post: 'Gönderi gizlendi', hide_comment: 'Yorum gizlendi', unhide_post: 'Gönderi gizlemesi kaldırıldı', unhide_comment: 'Yorum gizlemesi kaldırıldı',
              restrict: 'Kullanıcı kısıtlandı', unrestrict: 'Kısıtlama kaldırıldı', close: 'Forum paylaşımlara kapatıldı', open: 'Forum paylaşımlara açıldı', dismiss: 'Bildirim yoksayıldı',
              restore_post: 'Gönderi yeniden yayına alındı', restore_comment: 'Yorum yeniden yayına alındı', purge_post: 'Gönderi kalıcı olarak silindi', purge_comment: 'Yorum kalıcı olarak silindi' };
  var STATE = { live: ['Yayında', 'live'], hidden: ['Gizli', 'hidden'], deleted: ['Silinmiş', 'deleted'], purged: ['Kalıcı olarak silindi', 'purged'] };
  var logBefore = 0;
  function modTab(tab) {
    [].forEach.call(modModal.querySelectorAll('.fo-mtab'), function (b) { b.classList.toggle('on', b.getAttribute('data-mt') === tab); });
    [].forEach.call(modModal.querySelectorAll('.fo-mpane'), function (p) { p.hidden = p.getAttribute('data-mp') !== tab; });
    if (tab === 'reports') modLoad();
    if (tab === 'restr') restrLoad();
    if (tab === 'log') { logBefore = 0; $('foLogList').innerHTML = ''; logLoad(); }
    if (tab === 'set') setLoad();
  }
  function restrLoad() {
    $('foRestrList').innerHTML = ghost('Yükleniyor…');
    api('restrictions').then(function (d) {
      if (!d.ok) return;
      $('foRestrList').innerHTML = d.items.length ? d.items.map(function (r) {
        return '<div class="fo-rep" data-rid="' + r.id + '"><div class="fo-rep-h"><b>' + esc(r.name) + '</b><span>' + (r.until ? new Date(r.until).toLocaleString('tr-TR', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' }) + ' tarihine kadar' : 'Süresiz') + '</span></div>' +
               (r.reason ? '<div class="fo-rep-why">Gerekçe: ' + esc(r.reason) + '</div>' : '') + '<div class="fo-log-meta">' + esc(r.by) + ' · ' + ago(r.at) + '</div>' +
               '<div class="fo-rep-act"><button type="button" class="btn-ghost" data-unr="' + r.id + '">Kısıtlamayı kaldır</button></div></div>';
      }).join('') : '<div class="cu-empty"><b>Etkin kısıtlama yok</b><span>Kısıtlamak için gönderinin ⋯ menüsünden "Kullanıcıyı kısıtla"yı seçin.</span></div>';
    });
  }
  function logLoad() {
    var more = $('foLogMore'); more.hidden = true;
    api('log' + (logBefore ? '&before=' + logBefore : '')).then(function (d) {
      if (!d.ok) return;
      if (!logBefore && !d.items.length) { $('foLogList').innerHTML = '<div class="cu-empty"><b>Kayıt boş</b><span>Silme, gizleme ve kısıtlama işlemleri burada listelenir.</span></div>'; return; }
      $('foLogList').insertAdjacentHTML('beforeend', d.items.map(function (x) {
        return '<div class="fo-log"><div class="fo-log-h"><b>' + esc(ACT[x.action] || x.action) + (x.self ? ' <span class="fo-log-self">sahibi tarafından</span>' : '') + '</b><time>' + new Date(x.at).toLocaleString('tr-TR', { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' }) + '</time></div>' +
          (x.content || x.media ? '<div class="fo-log-tx">' + esc(x.content) + (x.media ? ' <em>· ' + x.media + ' görsel/video</em>' : '') + '</div>' : '') +
          '<div class="fo-log-meta">' + (x.owner ? 'İçerik sahibi: <b>' + esc(x.owner) + '</b> · ' : '') + 'İşlemi yapan: <b>' + esc(x.mod) + '</b>' + (x.post ? ' · Gönderi #' + x.post : '') + '</div>' +
          (x.reason ? '<div class="fo-rep-why">Açıklama: ' + esc(x.reason) + '</div>' : '') + logActs(x) + '</div>';
      }).join(''));
      if (d.items.length) logBefore = d.items[d.items.length - 1].id;
      more.hidden = !d.more;
    });
  }
  // kayit satirindaki dugmeler: icerigin bugunku durumuna gore
  function logActs(x) {
    if (!x.post || !x.state) return '';
    var st = STATE[x.state] || ['', ''], a = 'data-post="' + x.post + '" data-comment="' + (x.comment || '') + '"', h = '<div class="fo-log-acts"><span class="fo-state ' + st[1] + '">' + st[0] + '</span>';
    if (x.state !== 'purged') h += '<button type="button" class="btn-ghost" data-view="1" ' + a + '>Gör</button>';
    if (x.state === 'deleted' || x.state === 'hidden') h += '<button type="button" class="btn-ghost" data-restore="1" ' + a + '>Yeniden yayına al</button>';
    if (x.state === 'deleted' || x.state === 'hidden') h += '<button type="button" class="btn-ghost danger-t" data-purge="1" ' + a + '>Kalıcı olarak sil</button>';
    return h + '</div>';
  }
  // moderator onizlemesi: silinmis / gizli icerik metni ve medyasiyla
  function modView(post, comment) {
    var w = document.createElement('div'); w.className = 'pw-modal open fo-ask'; w.setAttribute('role', 'dialog'); w.setAttribute('aria-modal', 'true');
    w.innerHTML = '<div class="pw-box fo-view-box"><button type="button" class="pw-x" data-x="1" aria-label="Kapat">&times;</button><h3>İçerik önizlemesi</h3><div class="fo-view-body">' + ghost('Yükleniyor…') + '</div></div>';
    document.body.appendChild(w);
    w.addEventListener('click', function (e) { if (e.target === w || e.target.closest('[data-x]')) w.remove(); else if (e.target.closest('.fo-img')) openLb(e.target.closest('.fo-img').getAttribute('data-src')); });
    w.addEventListener('keydown', function (e) { if (e.key === 'Escape') { e.stopPropagation(); w.remove(); } });
    w.querySelector('.pw-x').focus();
    api('modview&id=' + post + (comment ? '&comment=' + comment : '')).then(function (d) {
      var body = w.querySelector('.fo-view-body');
      if (!d.ok) { body.innerHTML = '<div class="cu-empty"><b>' + esc(d.error || 'İçerik açılamadı') + '</b></div>'; return; }
      var p = d.view.post, c = d.view.comment, warn = '';
      if (c) warn = c.deleted ? 'Bu yorum silinmiş.' : c.hidden ? 'Bu yorum moderasyonla gizlenmiş.' : '';
      else warn = p.deleted ? 'Bu gönderi silinmiş; akışta görünmüyor.' : p.hidden ? 'Bu gönderi moderasyonla gizlenmiş.' : '';
      var h = warn ? '<div class="fo-banner restr">' + warn + '</div>' : '';
      if (c) h += '<div class="fo-view-com"><small>YORUM · ' + esc(c.author) + ' · ' + new Date(c.at).toLocaleString('tr-TR') + '</small><div class="fo-com-tx">' + rich(c.body) + '</div></div><h4 class="fo-prof-ph">Yorumun yapıldığı gönderi</h4>';
      h += postHtml(p);
      body.innerHTML = h;
      var art = body.querySelector('.fo-post'); if (art) art.classList.add('fo-preview');
    });
  }
  function setLoad() {
    api('feed&tab=announcements&before=1').then(function (d) { if (!d.ok) return; $('foSetClosed').checked = !!d.closed; $('foSetNote').value = d.closedNote || ''; });
  }
  $('foModBtn').addEventListener('click', function () { modModal.classList.add('open'); modTab('reports'); });
  modModal.addEventListener('click', function (e) {
    var tb = e.target.closest('.fo-mtab'); if (tb) { modTab(tb.getAttribute('data-mt')); return; }
    var un = e.target.closest('[data-unr]'); if (un) { un.disabled = true; api('unrestrict', fd({ id: un.getAttribute('data-unr') })).then(function () { toast('Kısıtlama kaldırıldı; kişiye bildirim gönderildi.'); restrLoad(); }); return; }
    if (e.target.closest('#foLogMore')) { logLoad(); return; }
    var vw = e.target.closest('[data-view]'); if (vw) { modView(vw.getAttribute('data-post'), vw.getAttribute('data-comment')); return; }
    var rs = e.target.closest('[data-restore]');
    if (rs) { rs.disabled = true; api('restore', fd({ id: rs.getAttribute('data-post'), comment: rs.getAttribute('data-comment') })).then(function (d) { toast(d.ok ? 'İçerik yeniden yayına alındı; sahibine bildirim gönderildi.' : (d.error || 'Geri alınamadı.'), !d.ok); if (d.ok) { logBefore = 0; $('foLogList').innerHTML = ''; logLoad(); load(true); } else rs.disabled = false; }); return; }
    var pg = e.target.closest('[data-purge]');
    if (pg) {
      ask({ title: 'Kalıcı olarak sil', text: 'İçerik, görsel ve video dosyaları, yorumları, beğenileri ve şikayetleriyle birlikte tamamen silinir. Bu işlem geri alınamaz. Kayıtta yalnız metin olarak bir satır kalır.', placeholder: 'Örn. KVKK talebi üzerine', ok: 'Kalıcı olarak sil', danger: true })
        .then(function (v) { if (!v) return; pg.disabled = true; api('purge', fd({ id: pg.getAttribute('data-post'), comment: pg.getAttribute('data-comment'), reason: v.reason })).then(function (d) { toast(d.ok ? 'İçerik kalıcı olarak silindi.' : (d.error || 'Silinemedi.'), !d.ok); if (d.ok) { logBefore = 0; $('foLogList').innerHTML = ''; logLoad(); } else pg.disabled = false; }); });
      return;
    }
  });
  $('foSetForm').addEventListener('submit', function (e) {
    e.preventDefault(); var on = $('foSetClosed').checked;
    api('settings', fd({ closed: on ? 1 : 0, note: $('foSetNote').value })).then(function (d) { if (d.ok) { toast(on ? 'Forum yeni paylaşımlara kapatıldı.' : 'Forum yeniden paylaşımlara açıldı.'); load(true); } });
  });
  modModal.addEventListener('click', function (e) {
    if (e.target === modModal || e.target.closest('[data-close]')) { modModal.classList.remove('open'); return; }
    var b = e.target.closest('[data-act]'); if (!b) return;
    var r = b.closest('.fo-rep'), hide = b.getAttribute('data-act') === '1';
    (hide ? ask({ title: 'İçeriği gizle', text: 'İçerik akıştan gizlenir ve moderasyon kaydına girer.', hint: 'Açıklama içerik sahibine uygulama içi bildirim olarak gider.', ok: 'Gizle', danger: true }) : Promise.resolve({ reason: '' })).then(function (v) {
    if (!v) return; b.disabled = true;
    api('resolve', fd({ id: r.getAttribute('data-post'), comment: r.getAttribute('data-comment'), hide: hide ? 1 : 0, reason: v.reason })).then(function (d) {
      if (d.ok) { r.remove(); $('foModCnt').textContent = d.reports; toast(hide ? 'İçerik gizlendi; sahibine bildirim gönderildi.' : 'Bildirim yoksayıldı.'); if (!$('foModList').children.length) modLoad(); }
    });
    });
  });

  // ------------------------------------------------------------ baslat
  refresh();
  if (state.tag) history.replaceState({ tag: state.tag }, '', location.pathname + '?tag=' + encodeURIComponent(state.tag));
  tagBar();
  load(true);
  trending();
  if (F.consent) openRules(true);
})();
