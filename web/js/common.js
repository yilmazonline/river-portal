(function () {
  // ---- Tema (koyu/açık) + logo değişimi ----
  var SUN = '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><circle cx="12" cy="12" r="4.2"></circle><path d="M12 2v2M12 20v2M2 12h2M20 12h2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M19.1 4.9l-1.4 1.4M6.3 17.7l-1.4 1.4"/></svg>';
  var MOON = '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8z"/></svg>';

  function iconFor(t) { return (t === 'light') ? MOON : SUN; }
  // Iki ikon birlikte; CSS temaya gore donerek gecis yapar
  function themeIcons() { return '<span class="tb-ic tb-sun">' + SUN + '</span><span class="tb-ic tb-moon">' + MOON + '</span>'; }
  // Tema degisiminde kisa sureli yumusak renk gecisi
  function themeFx() { var h = document.documentElement; h.classList.add('theme-fx'); clearTimeout(themeFx._t); themeFx._t = setTimeout(function () { h.classList.remove('theme-fx'); }, 520); }

  function applyLogos(t) {
    var light = (t === 'light');
    var bl = document.querySelectorAll('.brand-logo');
    for (var i = 0; i < bl.length; i++) bl[i].src = light ? 'assets/RIVER_AI_black.png' : 'assets/RIVER_AI_white.svg';
    var fl = document.querySelectorAll('.app-footer img');
    for (var j = 0; j < fl.length; j++) fl[j].src = light ? 'assets/fol_dark.png' : 'assets/fol_logo.png';
    var lr = document.querySelectorAll('.login-river');
    for (var k = 0; k < lr.length; k++) lr[k].src = light ? 'assets/RIVER_AI_black.png' : 'assets/RIVER_AI_white.png';
    var lf = document.querySelectorAll('.login-fol img');
    for (var m2 = 0; m2 < lf.length; m2++) lf[m2].src = light ? 'assets/fol_dark.png' : 'assets/fol_logo.png';
  }

  var m = document.cookie.match(/(?:^|; )theme=([^;]+)/);
  var theme = (m && m[1]) ? m[1] : 'light';
  document.documentElement.setAttribute('data-theme', theme);
  applyLogos(theme);

  var right = document.querySelector('.nav .right');
  if (right) {
    var tbtn = document.createElement('button');
    tbtn.type = 'button';
    tbtn.className = 'theme-btn';
    tbtn.title = 'Tema (Açık / Koyu)';
    tbtn.innerHTML = themeIcons();
    tbtn.setAttribute('aria-label', 'Tema değiştir');
    right.insertBefore(tbtn, right.firstChild);
    tbtn.addEventListener('click', function () {
      var cur = (document.documentElement.getAttribute('data-theme') === 'light') ? 'dark' : 'light';
      themeFx();
      document.documentElement.setAttribute('data-theme', cur);
      document.cookie = 'theme=' + cur + ';path=/;max-age=31536000';
      applyLogos(cur);
    });
  }

  // ---- Login tema anahtarı (Açık / Koyu) ----
  var lt = document.getElementById('loginTheme');
  if (lt) {
    var ltBtns = lt.querySelectorAll('[data-theme-set]');
    function ltSync() {
      var curT = document.documentElement.getAttribute('data-theme') || 'light';
      for (var i = 0; i < ltBtns.length; i++)
        ltBtns[i].className = (ltBtns[i].getAttribute('data-theme-set') === curT) ? 'active' : '';
    }
    for (var i = 0; i < ltBtns.length; i++) (function (b) {
      b.addEventListener('click', function () {
        var v = b.getAttribute('data-theme-set');
        themeFx();
        document.documentElement.setAttribute('data-theme', v);
        document.cookie = 'theme=' + v + ';path=/;max-age=31536000';
        applyLogos(v); ltSync();
      });
    })(ltBtns[i]);
    ltSync();
  }

  // ---- Logo -> ana sayfa (portal kökü) ----
  var brandLogo = document.querySelector('.nav .brand-logo');
  if (brandLogo) {
    var inLink = brandLogo.parentNode && brandLogo.parentNode.tagName === 'A';
    if (!inLink) {
      brandLogo.style.cursor = 'pointer';
      brandLogo.setAttribute('title', 'Ana Sayfa');
      brandLogo.addEventListener('click', function () { location.href = '/'; });
    }
  }

  // ---- Mobil menü (hamburger) ----
  var navEl = document.querySelector('.nav');
  var linksEl = navEl ? navEl.querySelector('.links') : null;
  if (navEl && linksEl) {
    var burger = document.createElement('button');
    burger.type = 'button';
    burger.className = 'nav-burger';
    burger.setAttribute('aria-label', 'Menü');
    burger.innerHTML = '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"><path d="M3 6h18M3 12h18M3 18h18"/></svg>';
    navEl.appendChild(burger);
    burger.addEventListener('click', function (e) { e.stopPropagation(); navEl.classList.toggle('nav-open'); });
    document.addEventListener('click', function () { navEl.classList.remove('nav-open'); });
    linksEl.addEventListener('click', function (e) { e.stopPropagation(); });
  }

  // ---- Profil dropdown ----
  var user = document.querySelector('.user');
  var menu = document.getElementById('userMenu');
  if (user && menu) {
    user.addEventListener('click', function (e) {
      e.stopPropagation();
      menu.className = (menu.className.indexOf('open') >= 0) ? 'user-menu' : 'user-menu open';
    });
    document.addEventListener('click', function () { menu.className = 'user-menu'; });
    menu.addEventListener('click', function (e) { e.stopPropagation(); });
  }

  // ---- Profil resmi (avatar) gösterimi ----
  function setAvatar(path) {
    if (!path) return;
    var avs = document.querySelectorAll('.avatar');
    for (var i = 0; i < avs.length; i++) { avs[i].className = 'avatar has-img'; avs[i].innerHTML = '<img src="' + path + '" alt="" />'; }
    var hs = document.querySelectorAll('[data-hero-ava] > span');   // ana sayfa karsilamasindaki profil resmi
    for (var j = 0; j < hs.length; j++) { hs[j].className = 'img'; hs[j].innerHTML = '<img src="' + path + '" alt="" />'; }
  }

  // ---- Şifre güncelle (tüm kullanıcılar) ----
  if (menu) {
    var logout = menu.querySelector('a[href="Logout.aspx"]');
    var item = document.createElement('button');
    item.type = 'button'; item.className = 'um-item'; item.id = 'pwOpen'; item.textContent = 'Şifre Güncelle';
    if (logout) menu.insertBefore(item, logout); else menu.appendChild(item);

    // Avatar (profil resmi): yukle veya hazir avatar sec
    var avaItem = document.createElement('button');
    avaItem.type = 'button'; avaItem.className = 'um-item'; avaItem.id = 'avaOpen'; avaItem.textContent = 'Avatar Yükle';
    if (logout) menu.insertBefore(avaItem, logout); else menu.appendChild(avaItem);
    var avaInput = document.createElement('input');
    avaInput.type = 'file'; avaInput.accept = 'image/*'; avaInput.style.display = 'none';
    document.body.appendChild(avaInput);

    var avaModal = document.createElement('div');
    avaModal.className = 'pw-modal'; avaModal.id = 'avaModal';
    // Hazir avatarlar: marka (4 FOL rengi + River R/AI), is hayati karakterleri, klasik
    var AVA_SETS = [
      ['Marka', [['fol-gold', 'FOL · Altın'], ['fol-platinum', 'FOL · Platin'], ['fol-onyx', 'FOL · Oniks'], ['fol-amethyst', 'FOL · Ametist'],
                 ['river-r', 'River R'], ['river-ai', 'River AI']]],
      ['İş Hayatı', [['is-kahve', 'Kahve Bağımlısı'], ['is-deadline', 'Deadline Paniği'], ['is-akis', 'Akış Ustası'], ['is-form', 'Form Uzmanı'],
                     ['is-onay', 'Onay Makamı'], ['is-bug', 'Bug Avcısı'], ['is-robot', 'Yapay Zekâ Asistanı'], ['is-toplanti', 'Toplantı Yorgunu'],
                     ['is-excel', 'Excel Ninjası'], ['is-roket', 'Canlıya Çıkış'], ['is-bende', 'Bende Çalışıyordu'], ['is-destek', 'Destek Kahramanı']]],
      ['Klasik', (function () { var a = []; for (var i = 1; i <= 12; i++) a.push(['av' + i, '']); return a; })()]
    ];
    var presetHtml = '';
    AVA_SETS.forEach(function (set) {
      presetHtml += '<div class="ava-sec">' + set[0] + '</div><div class="ava-grid">';
      set[1].forEach(function (p) {
        var src = 'assets/avatars/presets/' + p[0] + '.png';
        presetHtml += '<button type="button" class="ava-preset" data-src="' + src + '"' + (p[1] ? ' title="' + p[1] + '" aria-label="' + p[1] + '"' : '') + '><img src="' + src + '" alt="" loading="lazy" /></button>';
      });
      presetHtml += '</div>';
    });
    avaModal.innerHTML =
      '<div class="pw-box ava-box"><button type="button" class="pw-x" id="avaClose">&times;</button>' +
      '<h3>Avatar Seç</h3>' +
      '<div class="ava-up"><button type="button" class="btn-cyan" id="avaUploadBtn">Bilgisayardan Yükle</button><span class="ava-msg" id="avaMsg"></span></div>' +
      '<div class="ava-scroll">' + presetHtml + '</div></div>';
    document.body.appendChild(avaModal);
    function avaCloseFn() { avaModal.className = 'pw-modal'; }
    avaItem.addEventListener('click', function (e) { e.stopPropagation(); document.getElementById('avaMsg').textContent = ''; avaModal.className = 'pw-modal open'; });
    document.getElementById('avaClose').addEventListener('click', avaCloseFn);
    avaModal.addEventListener('click', function (e) { if (e.target === avaModal) avaCloseFn(); });
    document.getElementById('avaUploadBtn').addEventListener('click', function () { avaInput.click(); });
    var pbtns = avaModal.querySelectorAll('.ava-preset');
    for (var pk = 0; pk < pbtns.length; pk++) (function (btn) {
      btn.addEventListener('click', function () {
        var src = btn.getAttribute('data-src');
        fetch('UploadAvatar.aspx', { method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: 'preset=' + encodeURIComponent(src) })
          .then(function (r) { return r.text(); })
          .then(function (t) { t = (t || '').trim(); if (t.indexOf('assets/avatars/') === 0) { setAvatar(t); avaCloseFn(); } else document.getElementById('avaMsg').textContent = 'İşlem başarısız.'; });
      });
    })(pbtns[pk]);
    avaInput.addEventListener('change', function () {
      if (!avaInput.files || !avaInput.files[0]) return;
      var fd = new FormData(); fd.append('avatar', avaInput.files[0]);
      document.getElementById('avaMsg').textContent = 'Yükleniyor...';
      fetch('UploadAvatar.aspx', { method: 'POST', credentials: 'same-origin', body: fd })
        .then(function (r) { return r.text(); })
        .then(function (t) {
          t = (t || '').trim();
          if (t.indexOf('assets/avatars/') === 0) { setAvatar(t); avaCloseFn(); }
          else document.getElementById('avaMsg').textContent = 'Bir resim dosyası seçin (en fazla 4MB).';
          avaInput.value = '';
        })
        .catch(function () { document.getElementById('avaMsg').textContent = 'Bağlantı hatası.'; });
    });

    var modal = document.createElement('div');
    modal.className = 'pw-modal'; modal.id = 'pwModal';
    modal.innerHTML =
      '<div class="pw-box">' +
      '<button type="button" class="pw-x" id="pwClose" aria-label="Kapat">&times;</button>' +
      '<h3>Şifre Güncelle</h3>' +
      '<div class="pw-field"><label>Mevcut Şifre</label><input type="password" id="pwOld" autocomplete="current-password" /></div>' +
      '<div class="pw-field"><label>Yeni Şifre</label><input type="password" id="pwNew" autocomplete="new-password" /></div>' +
      '<div class="pw-field"><label>Yeni Şifre (Tekrar)</label><input type="password" id="pwNew2" autocomplete="new-password" /></div>' +
      '<div class="pw-msg" id="pwMsg"></div>' +
      '<button type="button" class="pw-save" id="pwSave">Şifreyi Güncelle</button>' +
      '</div>';
    document.body.appendChild(modal);

    var g = function (id) { return document.getElementById(id); };
    function pwMsg(m, ok) { var el = g('pwMsg'); el.textContent = m; el.className = 'pw-msg' + (ok ? ' ok' : ' err'); }
    function pwOpen() { modal.className = 'pw-modal open'; g('pwOld').value = ''; g('pwNew').value = ''; g('pwNew2').value = ''; g('pwMsg').textContent = ''; g('pwMsg').className = 'pw-msg'; setTimeout(function () { g('pwOld').focus(); }, 50); }
    function pwClose() { modal.className = 'pw-modal'; }
    item.addEventListener('click', function (e) { e.stopPropagation(); pwOpen(); });
    g('pwClose').addEventListener('click', pwClose);
    modal.addEventListener('click', function (e) { if (e.target === modal) pwClose(); });
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') pwClose(); });
    g('pwSave').addEventListener('click', function () {
      var o = g('pwOld').value, n = g('pwNew').value, n2 = g('pwNew2').value;
      if (!o || !n || !n2) { pwMsg('Lütfen tüm alanları doldurun.'); return; }
      if (n !== n2) { pwMsg('Yeni şifreler eşleşmiyor.'); return; }
      if (n.length < 6) { pwMsg('Yeni şifre en az 6 karakter olmalı.'); return; }
      g('pwSave').disabled = true;
      var body = 'old=' + encodeURIComponent(o) + '&new=' + encodeURIComponent(n) + '&new2=' + encodeURIComponent(n2);
      fetch('ChangePassword.aspx', { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, credentials: 'same-origin', body: body })
        .then(function (r) { return r.text(); })
        .then(function (t) {
          g('pwSave').disabled = false; t = (t || '').trim();
          if (t === 'ok') { pwMsg('Şifreniz başarıyla güncellendi.', true); setTimeout(pwClose, 1500); }
          else if (t === 'badold') pwMsg('Mevcut şifreniz hatalı.');
          else if (t === 'mismatch') pwMsg('Yeni şifreler eşleşmiyor.');
          else if (t === 'short') pwMsg('Yeni şifre en az 6 karakter olmalı.');
          else pwMsg('İşlem başarısız. Lütfen tekrar deneyin.');
        })
        .catch(function () { g('pwSave').disabled = false; pwMsg('Bağlantı hatası. Tekrar deneyin.'); });
    });
  }

  // ---- Açılır-kapanır bölümler ([data-toggle]) ----
  var toggles = document.querySelectorAll('[data-toggle]');
  for (var tg = 0; tg < toggles.length; tg++) toggles[tg].addEventListener('click', function () {
    var b = document.getElementById(this.getAttribute('data-toggle'));
    if (!b) return;
    var open = b.style.display !== 'none';
    b.style.display = open ? 'none' : 'block';
    if (open) this.classList.remove('open'); else this.classList.add('open');
  });

  // ---- Dil anahtarı (her sayfada; oturum kültürünü değiştirir + yeniler) ----
  (function () {
    var rt = document.querySelector('.nav .right');
    if (!rt) return;
    if (!rt.querySelector('.lang')) {
      var lg = document.createElement('div'); lg.className = 'lang';
      lg.innerHTML = '<button type="button" data-lang="tr">TR</button><button type="button" data-lang="en">EN</button>';
      var u = rt.querySelector('.user');
      if (u) rt.insertBefore(lg, u); else rt.appendChild(lg);
    }
    var cur = (document.cookie.match(/(?:^|; )ui_culture=([^;]+)/) || [])[1] || 'tr';
    var btns = rt.querySelectorAll('.lang [data-lang]');
    for (var i = 0; i < btns.length; i++) (function (b) {
      if (b.getAttribute('data-lang') === cur) b.className = 'active';
      b.addEventListener('click', function (e) {
        e.preventDefault(); e.stopPropagation();
        location.href = 'SetLang.aspx?lang=' + b.getAttribute('data-lang') + '&r=' + encodeURIComponent(location.pathname + location.search);
      }, true);
    })(btns[i]);
  })();

  // ---- Bildirim Ayarları (profil menüsü) ----
  (function () {
    var m = document.getElementById('userMenu');
    if (!m) return;
    var lo = m.querySelector('a[href="Logout.aspx"]');
    var it = document.createElement('button');
    it.type = 'button'; it.className = 'um-item'; it.textContent = 'Bildirim Ayarları';
    if (lo) m.insertBefore(it, lo); else m.appendChild(it);
    var mo = document.createElement('div'); mo.className = 'pw-modal'; mo.id = 'npModal';
    mo.innerHTML = '<div class="pw-box"><button type="button" class="pw-x" id="npClose">&times;</button>' +
      '<h3>Bildirim Ayarları</h3>' +
      '<p style="color:var(--muted);font-size:12.5px;margin:0 0 14px">Uygulama içi bildirimler her zaman gösterilir. Aşağıdaki tercihler yalnızca e-posta bildirimleri içindir.</p>' +
      '<label class="perm-chk"><input type="checkbox" id="npVersion" /><span>Versiyon Duyuruları <em>yeni sürüm e-postaları</em></span></label>' +
      '<label class="perm-chk"><input type="checkbox" id="npTraining" /><span>Eğitim İçerikleri <em>yeni eğitim e-postaları</em></span></label>' +
      '<div class="pw-msg" id="npMsg"></div>' +
      '<button type="button" class="pw-save" id="npSave">Kaydet</button></div>';
    document.body.appendChild(mo);
    var g = function (id) { return document.getElementById(id); };
    function close() { mo.className = 'pw-modal'; }
    it.addEventListener('click', function (e) {
      e.stopPropagation();
      g('npMsg').textContent = ''; g('npMsg').className = 'pw-msg';
      mo.className = 'pw-modal open';
      fetch('NotifPrefs.aspx', { credentials: 'same-origin' }).then(function (r) { return r.json(); }).then(function (j) {
        g('npVersion').checked = j.version !== false; g('npTraining').checked = j.training !== false;
      }).catch(function () { });
    });
    g('npClose').addEventListener('click', close);
    mo.addEventListener('click', function (e) { if (e.target === mo) close(); });
    g('npSave').addEventListener('click', function () {
      var v = g('npVersion').checked ? '1' : '0', t = g('npTraining').checked ? '1' : '0';
      g('npSave').disabled = true;
      fetch('NotifPrefs.aspx?action=save', { method: 'POST', credentials: 'same-origin', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: 'version=' + v + '&training=' + t })
        .then(function (r) { return r.text(); })
        .then(function () { g('npSave').disabled = false; g('npMsg').textContent = 'Kaydedildi.'; g('npMsg').className = 'pw-msg ok'; setTimeout(close, 900); })
        .catch(function () { g('npSave').disabled = false; g('npMsg').textContent = 'Hata. Tekrar deneyin.'; g('npMsg').className = 'pw-msg err'; });
    });
  })();

  // ---- Bildirim zili (Partner dışındaki tüm sayfalara enjekte) ----
  (function () {
    var rt = document.querySelector('.nav .right');
    if (!rt || rt.querySelector('.bell')) return;
    var bell = document.createElement('div'); bell.className = 'bell';
    bell.innerHTML =
      '<button type="button" class="btn" id="cbBellBtn" aria-label="Bildirimler"><svg class="bell-ic" width="18" height="18" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><path d="M12 2.2c.6 0 1.1.5 1.1 1.1v.6a6.4 6.4 0 0 1 5.3 6.3v3.5l1.5 2.5c.4.7-.1 1.6-.9 1.6H4.9c-.8 0-1.3-.9-.9-1.6l1.5-2.5v-3.5a6.4 6.4 0 0 1 5.3-6.3v-.6c0-.6.5-1.1 1.2-1.1z"/><path d="M9.6 19.4h4.8a2.4 2.4 0 0 1-4.8 0z"/></svg></button>' +
      '<span class="badge" id="cbBellCount" style="display:none">0</span>' +
      '<div class="dropdown" id="cbBellMenu"><h4><span>Bildirimler</span><span class="act" id="cbMarkAll">Tümünü okundu işaretle</span></h4><div id="cbBellList"><div class="cb-nempty">Yükleniyor...</div></div></div>';
    rt.insertBefore(bell, rt.firstChild);
    var menu = document.getElementById('cbBellMenu');
    document.getElementById('cbBellBtn').addEventListener('click', function (e) {
      e.stopPropagation();
      menu.className = (menu.className.indexOf('open') >= 0) ? 'dropdown' : 'dropdown open';
    });
    document.addEventListener('click', function () { menu.className = 'dropdown'; });
    menu.addEventListener('click', function (e) { e.stopPropagation(); });
    document.getElementById('cbMarkAll').addEventListener('click', function () {
      try { fetch('Notif.aspx?action=markall', { method: 'POST', credentials: 'same-origin' }); } catch (e) { }
      var c = document.getElementById('cbBellCount'); c.style.display = 'none'; bell.classList.remove('has-unread');
      var items = menu.querySelectorAll('.cb-notif'); for (var i = 0; i < items.length; i++) items[i].className = 'cb-notif';
    });
    function esc(s) { var d = document.createElement('div'); d.textContent = s || ''; return d.innerHTML; }
    function render(j) {
      var list = document.getElementById('cbBellList');
      if (!j || !j.items || j.items.length === 0) list.innerHTML = '<div class="cb-nempty">Bildirim yok.</div>';
      else {
        var h = '';
        for (var i = 0; i < j.items.length; i++) {
          var it = j.items[i];
          h += '<div class="cb-notif' + (it.u ? ' unread' : '') + (it.l ? ' cb-clickable' : '') + '"' + (it.id ? ' data-id="' + it.id + '"' : '') + (it.l ? ' data-l="' + esc(it.l) + '"' : '') + '><div class="cb-nt">' + esc(it.t) + '</div>' + (it.b ? '<div class="cb-nb">' + esc(it.b) + '</div>' : '') + '</div>';
        }
        list.innerHTML = h;
        var all = list.querySelectorAll('.cb-notif');
        for (var k = 0; k < all.length; k++) all[k].addEventListener('click', function () {
          var id = this.getAttribute('data-id'), l = this.getAttribute('data-l');
          if (id) {
            try { fetch('Notif.aspx?action=markread&id=' + id, { credentials: 'same-origin' }); } catch (e) { }
            if (this.className.indexOf('unread') >= 0) {
              this.className = this.className.replace(' unread', '');
              var badge = document.getElementById('cbBellCount');
              if (badge) { var c = (parseInt(badge.textContent, 10) || 1) - 1; if (c > 0) badge.textContent = c; else { badge.style.display = 'none'; var bw2 = document.querySelector('.bell'); if (bw2) bw2.classList.remove('has-unread'); } }
            }
          }
          if (l) location.href = l;
        });
      }
      var badge = document.getElementById('cbBellCount');
      if (j && j.count > 0) { badge.textContent = j.count; badge.style.display = ''; } else badge.style.display = 'none';
      var bw = document.querySelector('.bell'); if (bw) bw.classList.toggle('has-unread', !!(j && j.count > 0));
    }
    try { fetch('Notif.aspx', { credentials: 'same-origin' }).then(function (r) { return r.json(); }).then(render).catch(function () { render({ count: 0, items: [] }); }); } catch (e) { render({ count: 0, items: [] }); }
  })();

  // ---- Alt bilgi (footer) her sayfada en altta ----
  // 1) Sayfanin en dis kapsayicisinin sonuna tasinir: icerik nerede biterse bitsin (orn. Akademi'de uzun sol menu) hepsinin altinda kalir.
  // 2) Icerik ekrani doldurmuyorsa ust boslugu, alt bilgiyi ekranin dibine itecek kadar buyutulur; uzun sayfalarda hicbir sey degismez.
  //    Yerlesime (display, akis) dokunulmaz; yalniz alt bilginin ust boslugu ayarlanir. Icerik yuksekligi degisince yeniden hesaplanir.
  (function () {
    var f = document.querySelector('.app-footer');
    if (!f || !document.body) return;
    var top = f; while (top.parentNode && top.parentNode !== document.body) top = top.parentNode;
    if (top !== f && f.parentNode !== top) top.appendChild(f);
    var base = parseFloat(getComputedStyle(f).marginTop) || 0, extra = 0, busy = false;
    function scrollY0() { return window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop || 0; }
    function place() {
      if (busy) return; busy = true;
      var cs = getComputedStyle(top), pb = top === f ? 0 : parseFloat(cs.paddingBottom) || 0;
      var bottom = f.getBoundingClientRect().bottom + scrollY0() + pb + (parseFloat(cs.borderBottomWidth) || 0);
      // 24 px pay: alt bilgi ekranin dibinden biraz yukarida; kendi ekledigimiz bosluk dusulerek hesaplanir (dongu olusmaz)
      var need = Math.max(0, Math.floor(window.innerHeight - 24 - (bottom - extra)));
      if (Math.abs(need - extra) > 1) { extra = need; f.style.marginTop = (base + extra) + 'px'; }
      // denetim: eklenen bosluk yuzunden sayfa ekrani asiyorsa (ust menu, kenar bosluklari vb.) asan miktar kadar geri al
      if (extra > 0) {
        var over = Math.max(document.documentElement.scrollHeight, document.body.scrollHeight) - window.innerHeight;
        if (over > 0) { extra = Math.max(0, extra - over - 2); f.style.marginTop = (base + extra) + 'px'; }
      }
      busy = false;
    }
    var t = null, later = function () { clearTimeout(t); t = setTimeout(place, 60); };
    window.addEventListener('resize', later);
    window.addEventListener('load', place);
    if (window.ResizeObserver) { var ro = new ResizeObserver(later); [].forEach.call(top === f ? [document.body] : top.children, function (c) { if (c !== f) ro.observe(c); }); }
    place();
  })();

  // ---- Arama kutulari: tarayici otomatik doldurmasi ve giris (login) onerisi kapali ----
  // type=search ya da ipucu metni "ara" iceren kutular. Benzersiz ad, otomatik doldurma ve sifre yoneticisi isaretleri kapali;
  // kutu dokunulana kadar salt okunur (tarayici salt okunur kutuyu doldurmaz). Fareye basildigi anda yazilabilir: tek tiklamayla odaklanir.
  // Yine de bir e-posta adresi doldurulursa ve kullanici dokunmadiysa geri alinir. Sonradan sayfaya eklenen kutular da korunur.
  (function () {
    var SEL = 'input[type=search], input[data-search]';
    function isSearch(el) {
      if (!el || el.tagName !== 'INPUT') return false;
      if (el.type === 'search' || el.hasAttribute('data-search')) return true;
      return el.type === 'text' && /(^|\s)ara(\b|[:.…])|\bsearch\b/i.test(el.getAttribute('placeholder') || '');
    }
    function guard(el) {
      if (el.__sg || !isSearch(el)) return; el.__sg = 1;
      var at = { autocomplete: 'off', autocorrect: 'off', autocapitalize: 'off', spellcheck: 'false', 'data-lpignore': 'true', 'data-1p-ignore': 'true', 'data-form-type': 'other', role: 'searchbox' };
      for (var k in at) el.setAttribute(k, at[k]);
      if (!el.name || /mail|user|login|kullanici|posta/i.test(el.name)) el.name = 'srch_' + Math.random().toString(36).slice(2, 8);
      var v0 = el.value, touched = false;
      var free = function () { touched = true; if (el.readOnly && !el.hasAttribute('data-keep-readonly')) el.readOnly = false; };
      if (document.activeElement !== el) el.readOnly = true; else touched = true;
      el.addEventListener('mousedown', free); el.addEventListener('touchstart', free, { passive: true }); el.addEventListener('focus', free); el.addEventListener('keydown', free);
      [0, 400, 1200, 2500].forEach(function (ms) {
        setTimeout(function () { if (!touched && el.value !== v0 && /@/.test(el.value)) { el.value = v0; el.dispatchEvent(new Event('input', { bubbles: true })); } }, ms);
      });
    }
    function scan(root) {
      if (!root || !root.querySelectorAll) return;
      if (root.tagName === 'INPUT') guard(root);
      [].forEach.call(root.querySelectorAll('input'), guard);
    }
    scan(document);
    if (window.MutationObserver) new MutationObserver(function (ms) { ms.forEach(function (m) { [].forEach.call(m.addedNodes, function (n) { if (n.nodeType === 1) scan(n); }); }); }).observe(document.documentElement, { childList: true, subtree: true });
  })();

  // ---- Uygulama sürümü (footer) ----
  var APP_VERSION = 'v2.60.1';
  (function () {
    var f = document.querySelector('.app-footer');
    if (f && !f.querySelector('.app-version')) {
      var v = document.createElement('div'); v.className = 'app-version'; v.textContent = 'Sürüm ' + APP_VERSION;
      f.appendChild(v);
    }
  })();

  // ---- Form gönderim yükleme overlay'i (mükerrer gönderimi önler) ----
  var __formLoaderEl = null, __submitting = false;
  function ensureFormLoader() {
    if (__formLoaderEl) return __formLoaderEl;
    var d = document.createElement('div'); d.id = 'formLoader';
    d.innerHTML = '<div class="loader-badge"><img src="assets/RIVER_AI_badge.svg" alt="" style="width:100%;height:100%" /></div>' +
      '<div class="loader-track"><i></i></div>' +
      '<div class="loader-text">River AI tarafından işleniyor. Lütfen sayfayı kapatmayınız.</div>';
    document.body.appendChild(d); __formLoaderEl = d; return d;
  }
  function showFormLoader() { ensureFormLoader().className = 'show'; }
  function hideFormLoader() { if (__formLoaderEl) __formLoaderEl.className = ''; __submitting = false; }
  window.showFormLoader = showFormLoader;
  window.hideFormLoader = hideFormLoader;
  document.addEventListener('submit', function (e) {
    var form = e.target;
    if (!form || form.nodeName !== 'FORM') return;
    if (form.getAttribute('data-noload') === '1') return;
    if (__submitting) { e.preventDefault(); return; }
    __submitting = true;
    showFormLoader();
    // Bu dinleyici formun kendi dogrulamasindan ONCE calisir (capture). Gonderimi bir dogrulama durdurduysa perde kapanir; sayfa kilitli kalmaz.
    setTimeout(function () { if (e.defaultPrevented) hideFormLoader(); }, 0);
  }, true);
  // Geri/ileri (bfcache) veya sayfa yeniden gösteriminde overlay'i kapat
  window.addEventListener('pageshow', function () { hideFormLoader(); });

  // ---- Uygulama Turu (animasyonlu) ----
  (function () {
    function row(y, b) {
      return '<rect x="90" y="' + y + '" width="300" height="44" rx="11" fill="#161C33" stroke="#242A45"><animate attributeName="x" values="-230;90" dur=".55s" begin="' + b + '" fill="freeze"/><animate attributeName="opacity" values="0;1" dur=".55s" begin="' + b + '" fill="freeze"/></rect>' +
             '<rect x="108" y="' + (y + 15) + '" width="120" height="14" rx="7" fill="#2A3358"><animate attributeName="opacity" values="0;1" dur=".5s" begin="' + b + '" fill="freeze"/></rect>' +
             '<rect x="300" y="' + (y + 13) + '" width="74" height="18" rx="9" fill="#18AF6E" opacity=".85"><animate attributeName="opacity" values="0;.85" dur=".5s" begin="' + b + '" fill="freeze"/></rect>';
    }
    function bar(cx, h, col, b, lbl) {
      var y = 220 - h;
      return '<rect x="' + (cx - 26) + '" y="220" width="52" height="0" rx="8" fill="' + col + '"><animate attributeName="height" values="0;' + h + '" dur=".7s" begin="' + b + '" fill="freeze"/><animate attributeName="y" values="220;' + y + '" dur=".7s" begin="' + b + '" fill="freeze"/></rect>' +
             '<text x="' + cx + '" y="244" text-anchor="middle" fill="#93A3C4" font-size="12">' + lbl + '</text>';
    }
    function cbar(cx, h, b) {
      var y = 220 - h;
      return '<rect x="' + (cx - 16) + '" y="220" width="32" height="0" rx="6" fill="#7A3BEA"><animate attributeName="height" values="0;' + h + '" dur=".7s" begin="' + b + '" fill="freeze"/><animate attributeName="y" values="220;' + y + '" dur=".7s" begin="' + b + '" fill="freeze"/></rect>';
    }
    function doc(x, rot, col, b) {
      return '<g transform="translate(' + x + ' 96) rotate(' + rot + ')"><rect x="0" y="0" width="120" height="150" rx="12" fill="#161C33" stroke="' + col + '" stroke-opacity=".6"/><rect x="18" y="26" width="84" height="10" rx="5" fill="#2A3358"/><rect x="18" y="48" width="64" height="10" rx="5" fill="#2A3358"/><rect x="18" y="70" width="84" height="10" rx="5" fill="#2A3358"/><animate attributeName="opacity" values="0;1" dur=".5s" begin="' + b + '" fill="freeze"/></g>';
    }
    function ppl(cx, b, col, tag) {
      return '<g><animate attributeName="opacity" values="0;1" dur=".5s" begin="' + b + '" fill="freeze"/>' +
             '<circle cx="' + cx + '" cy="120" r="26" fill="' + col + '" opacity=".9"/>' +
             '<circle cx="' + cx + '" cy="112" r="9" fill="#fff"/><path d="M' + (cx - 14) + ' 138 a14 12 0 0 1 28 0" fill="#fff"/>' +
             '<rect x="' + (cx - 36) + '" y="152" width="72" height="20" rx="10" fill="#161C33" stroke="#242A45"/>' +
             '<text x="' + cx + '" y="166" text-anchor="middle" fill="#C6D2E6" font-size="11">' + tag + '</text></g>';
    }
    function tseal(cx, cy, r, c1, c2, c3, id, b) {
      var p = [], n = 40;
      for (var i = 0; i < n; i++) { var rr = (i % 2) ? r * 0.88 : r, a = Math.PI * 2 * i / n - Math.PI / 2; p.push((rr * Math.cos(a)).toFixed(1) + ',' + (rr * Math.sin(a)).toFixed(1)); }
      return '<g transform="translate(' + cx + ' ' + cy + ')"><g transform="scale(0)"><animateTransform attributeName="transform" type="scale" values="0;1.14;1" dur=".6s" begin="' + b + '" fill="freeze"/>' +
             '<defs><linearGradient id="' + id + '" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="' + c1 + '"/><stop offset=".45" stop-color="' + c2 + '"/><stop offset="1" stop-color="' + c3 + '"/></linearGradient></defs>' +
             '<polygon fill="url(#' + id + ')" points="' + p.join(' ') + '"/><circle r="' + (r * 0.72).toFixed(1) + '" fill="none" stroke="#FDFBF6" stroke-width="1" opacity=".85"/>' +
             '<image href="assets/fol_mark_light.png" x="' + (-r * 0.4).toFixed(1) + '" y="' + (-r * 0.39).toFixed(1) + '" width="' + (r * 0.8).toFixed(1) + '" height="' + (r * 0.78).toFixed(1) + '"/></g></g>';
    }
    var VIZ = {
      welcome:
        '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial">' +
        '<circle cx="240" cy="138" r="66" fill="#7A3BEA" opacity=".16"><animate attributeName="r" values="54;82;54" dur="3s" repeatCount="indefinite"/></circle>' +
        '<circle cx="240" cy="138" r="44" fill="none" stroke="#7A3BEA" stroke-width="2"><animate attributeName="r" values="44;116;44" dur="3s" repeatCount="indefinite"/><animate attributeName="opacity" values=".7;0;.7" dur="3s" repeatCount="indefinite"/></circle>' +
        '<circle cx="240" cy="138" r="44" fill="none" stroke="#22C1E6" stroke-width="2"><animate attributeName="r" values="44;116;44" dur="3s" begin="1.3s" repeatCount="indefinite"/><animate attributeName="opacity" values=".7;0;.7" dur="3s" begin="1.3s" repeatCount="indefinite"/></circle>' +
        '<image href="assets/river-email-logo.png" x="140" y="116" width="200" height="52" preserveAspectRatio="xMidYMid meet"/>' +
        '<text x="240" y="200" text-anchor="middle" fill="#93A3C4" font-size="12" letter-spacing="5">ASISTAN PORTAL</text></svg>',
      gauge:
        '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial">' +
        '<path d="M110 225 A130 130 0 0 1 370 225" fill="none" stroke="#242A45" stroke-width="18" stroke-linecap="round"/>' +
        '<path d="M110 225 A130 130 0 0 1 370 225" fill="none" stroke="#7A3BEA" stroke-width="18" stroke-linecap="round" stroke-dasharray="408" stroke-dashoffset="408"><animate attributeName="stroke-dashoffset" values="408;150;150;408" keyTimes="0;.5;.85;1" dur="4s" repeatCount="indefinite"/></path>' +
        '<circle r="9" fill="#22C1E6"><animateMotion path="M110 225 A130 130 0 0 1 370 225" keyPoints="0;.63;.63;0" keyTimes="0;.5;.85;1" dur="4s" repeatCount="indefinite"/></circle>' +
        '<text x="240" y="205" text-anchor="middle" fill="#fff" font-weight="800" font-size="26">Ciro &amp; Seviye</text></svg>',
      orders:
        '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial">' +
        row(72, '0s') + row(128, '.35s') + row(184, '.7s') +
        '<rect x="90" y="240" width="300" height="10" rx="5" fill="#161C33"/>' +
        '<rect x="90" y="240" width="0" height="10" rx="5" fill="#7A3BEA"><animate attributeName="width" values="0;300" dur="1.2s" begin=".9s" fill="freeze"/></rect></svg>',
      demo:
        '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial">' +
        '<circle cx="240" cy="140" r="62" fill="none" stroke="#242A45" stroke-width="8"/>' +
        '<circle cx="240" cy="140" r="62" fill="none" stroke="#18AF6E" stroke-width="8" stroke-linecap="round" stroke-dasharray="390" stroke-dashoffset="390" transform="rotate(-90 240 140)"><animate attributeName="stroke-dashoffset" values="390;0" dur="1s" begin=".2s" fill="freeze"/></circle>' +
        '<path d="M212 140 l20 20 l38 -42" fill="none" stroke="#18AF6E" stroke-width="9" stroke-linecap="round" stroke-linejoin="round" stroke-dasharray="90" stroke-dashoffset="90"><animate attributeName="stroke-dashoffset" values="90;0" dur=".5s" begin="1s" fill="freeze"/></path></svg>',
      tiers:
        '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial">' +
        bar(96, 60, '#8C7A5A', '0s', 'Kayitli') + bar(190, 100, '#C0C6D6', '.2s', 'Silver') +
        bar(284, 140, '#E7A438', '.4s', 'Gold') + bar(378, 180, '#C019E6', '.6s', 'Platinum') + '</svg>',
      docs:
        '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial">' +
        doc(146, -8, '#168CFF', '0s') + doc(206, 6, '#7A3BEA', '.25s') + doc(266, 20, '#22C1E6', '.5s') + '</svg>',
      chat:
        '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial">' +
        '<rect x="70" y="74" width="230" height="46" rx="14" fill="#20284A"><animate attributeName="opacity" values="0;1" dur=".4s" fill="freeze"/><animate attributeName="x" values="40;70" dur=".4s" fill="freeze"/></rect>' +
        '<rect x="180" y="134" width="230" height="46" rx="14" fill="#7A3BEA"><animate attributeName="opacity" values="0;1" dur=".4s" begin=".5s" fill="freeze"/><animate attributeName="x" values="410;180" dur=".4s" begin=".5s" fill="freeze"/></rect>' +
        '<g transform="translate(96 210)"><circle cx="0" cy="0" r="6" fill="#22C1E6"><animate attributeName="opacity" values=".3;1;.3" dur="1.2s" repeatCount="indefinite"/></circle><circle cx="20" cy="0" r="6" fill="#22C1E6"><animate attributeName="opacity" values=".3;1;.3" dur="1.2s" begin=".2s" repeatCount="indefinite"/></circle><circle cx="40" cy="0" r="6" fill="#22C1E6"><animate attributeName="opacity" values=".3;1;.3" dur="1.2s" begin=".4s" repeatCount="indefinite"/></circle></g></svg>',
      play:
        '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial">' +
        '<rect x="120" y="68" width="240" height="135" rx="14" fill="#161C33" stroke="#242A45"/>' +
        '<circle cx="240" cy="135" r="34" fill="#C019E6" opacity=".9"><animate attributeName="r" values="30;37;30" dur="1.8s" repeatCount="indefinite"/></circle>' +
        '<path d="M231 118 l26 17 l-26 17 z" fill="#fff"/>' +
        '<rect x="120" y="212" width="240" height="6" rx="3" fill="#242A45"/>' +
        '<rect x="120" y="212" width="0" height="6" rx="3" fill="#22C1E6"><animate attributeName="width" values="0;240" dur="3.4s" repeatCount="indefinite"/></rect></svg>',
      version:
        '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial">' +
        '<rect x="150" y="88" width="180" height="128" rx="16" fill="#161C33" stroke="#18AF6E" stroke-opacity=".5"/>' +
        '<text x="240" y="152" text-anchor="middle" fill="#fff" font-weight="800" font-size="34">v5.2</text>' +
        '<rect x="180" y="170" width="120" height="9" rx="4" fill="#242A45"/><rect x="180" y="186" width="88" height="9" rx="4" fill="#242A45"/>' +
        '<g transform="translate(318 98)"><circle r="15" fill="#18AF6E"><animate attributeName="r" values="13;17;13" dur="1.6s" repeatCount="indefinite"/></circle><text x="0" y="4" text-anchor="middle" fill="#fff" font-size="11" font-weight="800">YENI</text></g></svg>',
      bars:
        '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial">' +
        '<line x1="80" y1="220" x2="400" y2="220" stroke="#242A45" stroke-width="2"/>' +
        cbar(110, 70, '0s') + cbar(170, 120, '.15s') + cbar(230, 95, '.3s') + cbar(290, 150, '.45s') + cbar(350, 175, '.6s') + '</svg>',
      users:
        '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial">' +
        ppl(96, '0s', '#7A3BEA', 'Yonetici') + ppl(152, '.25s', '#168CFF', 'Satis') + ppl(208, '.5s', '#18AF6E', 'Destek') + '</svg>'
    };
    // --- Sinav / sertifika / bildirim adimlari ---
    VIZ.badges = '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial"><defs><linearGradient id="tbg" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#4F46E5"/><stop offset="1" stop-color="#22B8D8"/></linearGradient></defs><rect x="40" y="40" width="200" height="220" rx="16" fill="#161C33" stroke="#B08D4C" stroke-opacity=".45"/><text x="58" y="66" fill="#C9A45C" font-size="10" font-weight="700" text-anchor="start" letter-spacing="2">ROZETLER\u0130M</text>' +
        tseal(74,98,19,'#FBE7AE','#C9A04E','#7A5A22','tb1','.15s') +
        '<g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin=".2s" fill="freeze"/><rect x="102" y="88" width="112" height="9" rx="4" fill="#2A3358"/><text x="102" y="112" fill="#93A3C4" font-size="9.5" font-weight="500" text-anchor="start">Eyl\u00fcl 2026</text></g><circle cx="74" cy="152" r="22" fill="#1CA273" opacity=".0"><animate attributeName="opacity" values="0;.35;0" dur="2s" begin="1s" repeatCount="indefinite"/><animate attributeName="r" values="20;27;20" dur="2s" begin="1s" repeatCount="indefinite"/></circle>' +
        tseal(74,152,19,'#9AF5CE','#1CA273','#08503A','tb2','.45s') +
        '<g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin=".5s" fill="freeze"/><rect x="102" y="142" width="104" height="9" rx="4" fill="#2A3358"/><text x="102" y="166" fill="#18C07A" font-size="9.5" font-weight="700" text-anchor="start">S\u0131nava girebilirsin</text></g>' +
        tseal(74,206,19,'#C9CED8','#9AA2B2','#7B8394','tb3','.75s') +
        '<g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin=".8s" fill="freeze"/><rect x="102" y="196" width="96" height="9" rx="4" fill="#2A3358"/><text x="102" y="220" fill="#93A3C4" font-size="9.5" font-weight="500" text-anchor="start">Videolar: 7/10</text></g><rect x="262" y="40" width="178" height="220" rx="16" fill="#161C33" stroke="#242A45"/><text x="280" y="66" fill="#E7ECF7" font-size="12" font-weight="700" text-anchor="start">Giri\u015f \u015fartlar\u0131</text><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1.1s" fill="freeze"/><circle cx="292" cy="94" r="11" fill="#18AF6E"/><text x="292" y="98" fill="#fff" font-size="11" font-weight="800" text-anchor="middle">\u2713</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1.1s" fill="freeze"/><rect x="312" y="88" width="104" height="8" rx="4" fill="#2A3358"/><rect x="312" y="101" width="70" height="6" rx="3" fill="#242A45"/></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1.5s" fill="freeze"/><circle cx="292" cy="144" r="11" fill="#18AF6E"/><text x="292" y="148" fill="#fff" font-size="11" font-weight="800" text-anchor="middle">\u2713</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1.5s" fill="freeze"/><rect x="312" y="138" width="96" height="8" rx="4" fill="#2A3358"/><rect x="312" y="152" width="104" height="6" rx="3" fill="#242A45"/><rect x="312" y="152" width="0" height="6" rx="3" fill="url(#tbg)"><animate attributeName="width" values="0;104" dur="1s" begin="1.5s" fill="freeze"/></rect></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1.9s" fill="freeze"/><circle cx="292" cy="194" r="11" fill="#18AF6E"/><text x="292" y="198" fill="#fff" font-size="11" font-weight="800" text-anchor="middle">\u2713</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1.9s" fill="freeze"/><rect x="312" y="188" width="88" height="8" rx="4" fill="#2A3358"/><rect x="312" y="201" width="60" height="6" rx="3" fill="#242A45"/></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="2.3s" fill="freeze"/><rect x="284" y="222" width="134" height="26" rx="13" fill="#C21BE3"><animate attributeName="opacity" values="1;.75;1" dur="1.4s" begin="2.6s" repeatCount="indefinite"/></rect><text x="351" y="239" fill="#fff" font-size="11" font-weight="700" text-anchor="middle">S\u0131nava Ba\u015fla</text></g></svg>';
    VIZ.progress = '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial"><defs><linearGradient id="tpg" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#4F46E5"/><stop offset="1" stop-color="#22B8D8"/></linearGradient></defs><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin=".1s" fill="freeze"/><rect x="44" y="32" width="120" height="58" rx="12" fill="#161C33" stroke="#242A45"/><text x="58" y="62" fill="#fff" font-size="20" font-weight="800" text-anchor="start">13/21</text><text x="58" y="80" fill="#93A3C4" font-size="9.5" font-weight="500" text-anchor="start">Tamamlanan video</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin=".3s" fill="freeze"/><rect x="180" y="32" width="120" height="58" rx="12" fill="#161C33" stroke="#242A45"/><text x="194" y="62" fill="#fff" font-size="20" font-weight="800" text-anchor="start">%62</text><text x="194" y="80" fill="#93A3C4" font-size="9.5" font-weight="500" text-anchor="start">\u0130zleme oran\u0131</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin=".5s" fill="freeze"/><rect x="316" y="32" width="120" height="58" rx="12" fill="#161C33" stroke="#242A45"/><text x="330" y="62" fill="#fff" font-size="20" font-weight="800" text-anchor="start">1/3</text><text x="330" y="80" fill="#93A3C4" font-size="9.5" font-weight="500" text-anchor="start">Tamamlanan e\u011fitim</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin=".8s" fill="freeze"/><rect x="44" y="112" width="392" height="44" rx="12" fill="#161C33" stroke="#18AF6E" stroke-opacity=".45"/><circle cx="68" cy="134" r="11" fill="#18AF6E"/><text x="68" y="138" fill="#fff" font-size="10" font-weight="800" text-anchor="middle">\u2713</text><rect x="90" y="122" width="120" height="9" rx="4" fill="#2A3358"/><rect x="90" y="140" width="240" height="6" rx="3" fill="#242A45"/><rect x="90" y="140" width="0" height="6" rx="3" fill="url(#tpg)"><animate attributeName="width" values="0;240" dur="1s" begin=".8s" fill="freeze"/></rect><rect x="346" y="125" width="78" height="18" rx="9" fill="#18C07A" opacity=".18"/><text x="385" y="138" fill="#18C07A" font-size="9.5" font-weight="700" text-anchor="middle">Tamamland\u0131</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1.1s" fill="freeze"/><rect x="44" y="166" width="392" height="44" rx="12" fill="#161C33" stroke="#2E7FE0" stroke-opacity=".45"/><circle cx="68" cy="188" r="11" fill="#2E7FE0"/><text x="68" y="192" fill="#fff" font-size="10" font-weight="800" text-anchor="middle">\u25b6</text><rect x="90" y="176" width="120" height="9" rx="4" fill="#2A3358"/><rect x="90" y="194" width="240" height="6" rx="3" fill="#242A45"/><rect x="90" y="194" width="0" height="6" rx="3" fill="url(#tpg)"><animate attributeName="width" values="0;182" dur="1s" begin="1.1s" fill="freeze"/></rect><rect x="346" y="179" width="78" height="18" rx="9" fill="#4F9BF0" opacity=".18"/><text x="385" y="192" fill="#4F9BF0" font-size="9.5" font-weight="700" text-anchor="middle">Devam ediyor</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1.4s" fill="freeze"/><rect x="44" y="220" width="392" height="44" rx="12" fill="#161C33" stroke="#3A4568" stroke-opacity=".45"/><circle cx="68" cy="242" r="11" fill="#3A4568"/><text x="68" y="246" fill="#fff" font-size="10" font-weight="800" text-anchor="middle">\u25cb</text><rect x="90" y="230" width="120" height="9" rx="4" fill="#2A3358"/><rect x="90" y="248" width="240" height="6" rx="3" fill="#242A45"/><rect x="90" y="248" width="0" height="6" rx="3" fill="url(#tpg)"><animate attributeName="width" values="0;0" dur="1s" begin="1.4s" fill="freeze"/></rect><rect x="346" y="233" width="78" height="18" rx="9" fill="#93A3C4" opacity=".18"/><text x="385" y="246" fill="#93A3C4" font-size="9.5" font-weight="700" text-anchor="middle">Ba\u015flanmad\u0131</text></g></svg>';
    VIZ.cert = '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial"><rect x="60" y="44" width="262" height="190" rx="4" fill="#FDFBF6" stroke="#16213E" stroke-width="1.4"/><rect x="67" y="51" width="248" height="176" rx="2" fill="none" stroke="#B08D4C" stroke-width=".8"/><text x="191" y="68" fill="#B08D4C" font-size="6.5" font-weight="700" text-anchor="middle" letter-spacing="1.6">CERTIFICATE OF ACHIEVEMENT</text><text x="191" y="92" fill="#16213E" font-size="18" font-weight="700" text-anchor="middle" font-family="Georgia,serif">Ba\u015far\u0131 Sertifikas\u0131</text><rect x="186" y="100" width="7" height="7" transform="rotate(45 189.5 103.5)" fill="#B08D4C"/><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin=".3s" fill="freeze"/><text x="191" y="134" fill="#16213E" font-size="21" font-weight="700" text-anchor="middle" font-family="Georgia,serif">Ad Soyad</text><rect x="121" y="142" width="140" height="1.2" fill="#B08D4C"/><rect x="111" y="152" width="160" height="5" rx="2.5" fill="#D8D2C2"/><rect x="131" y="162" width="120" height="5" rx="2.5" fill="#D8D2C2"/></g>' +
        tseal(191,202,20,'#FBE7AE','#C9A04E','#7A5A22','tc1','.7s') +
        '<rect x="85" y="187" width="30" height="30" fill="#fff" stroke="#B08D4C" stroke-width=".6"/><rect x="92" y="190" width="4" height="4" fill="#16213E"/><rect x="96" y="190" width="4" height="4" fill="#16213E"/><rect x="104" y="190" width="4" height="4" fill="#16213E"/><rect x="108" y="190" width="4" height="4" fill="#16213E"/><rect x="88" y="194" width="4" height="4" fill="#16213E"/><rect x="92" y="194" width="4" height="4" fill="#16213E"/><rect x="100" y="194" width="4" height="4" fill="#16213E"/><rect x="104" y="194" width="4" height="4" fill="#16213E"/><rect x="88" y="198" width="4" height="4" fill="#16213E"/><rect x="96" y="198" width="4" height="4" fill="#16213E"/><rect x="100" y="198" width="4" height="4" fill="#16213E"/><rect x="108" y="198" width="4" height="4" fill="#16213E"/><rect x="92" y="202" width="4" height="4" fill="#16213E"/><rect x="96" y="202" width="4" height="4" fill="#16213E"/><rect x="104" y="202" width="4" height="4" fill="#16213E"/><rect x="108" y="202" width="4" height="4" fill="#16213E"/><rect x="88" y="206" width="4" height="4" fill="#16213E"/><rect x="92" y="206" width="4" height="4" fill="#16213E"/><rect x="100" y="206" width="4" height="4" fill="#16213E"/><rect x="104" y="206" width="4" height="4" fill="#16213E"/><rect x="88" y="210" width="4" height="4" fill="#16213E"/><rect x="96" y="210" width="4" height="4" fill="#16213E"/><rect x="100" y="210" width="4" height="4" fill="#16213E"/><rect x="108" y="210" width="4" height="4" fill="#16213E"/><path d="M118 200 C200 250 290 230 350 170" fill="none" stroke="#22B8D8" stroke-width="1.6" stroke-dasharray="4 5" opacity="0"><animate attributeName="opacity" values="0;1" dur=".3s" begin="1.2s" fill="freeze"/><animate attributeName="stroke-dashoffset" values="60;0" dur="1.2s" begin="1.2s" repeatCount="indefinite"/></path><rect x="344" y="64" width="92" height="170" rx="16" fill="#0E1428" stroke="#2A3456" stroke-width="1.4"/><rect x="376" y="72" width="28" height="4" rx="2" fill="#2A3456"/><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1.7s" fill="freeze"/><circle cx="390" cy="128" r="20" fill="#12A150"><animate attributeName="r" values="0;23;20" dur=".5s" begin="1.7s" fill="freeze"/></circle><text x="390" y="134" fill="#fff" font-size="18" font-weight="800" text-anchor="middle">\u2713</text><text x="390" y="168" fill="#18C07A" font-size="12" font-weight="700" text-anchor="middle">Ge\u00e7erli</text><rect x="360" y="180" width="60" height="5" rx="2.5" fill="#2A3358"/><rect x="366" y="191" width="48" height="5" rx="2.5" fill="#2A3358"/><rect x="362" y="202" width="56" height="5" rx="2.5" fill="#2A3358"/></g></svg>';
    VIZ.examedit = '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial"><defs><linearGradient id="teg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#4F46E5"/><stop offset="1" stop-color="#22B8D8"/></linearGradient></defs><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin=".1s" fill="freeze"/><rect x="40" y="38" width="236" height="48" rx="12" fill="#161C33" stroke="#242A45"/><rect x="54" y="51" width="22" height="22" rx="7" fill="#4F46E5" opacity=".3"/><text x="65" y="66" fill="#AFC0FF" font-size="11" font-weight="800" text-anchor="middle">1</text><rect x="86" y="53" width="116" height="8" rx="4" fill="#2A3358"/><rect x="86" y="66" width="80" height="6" rx="3" fill="#242A45"/><rect x="216" y="52" width="48" height="20" rx="10" fill="#22B8D8" opacity=".16"/><text x="240" y="66" fill="#5FD3EC" font-size="9" font-weight="700" text-anchor="middle">25 puan</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin=".4s" fill="freeze"/><rect x="40" y="96" width="236" height="48" rx="12" fill="#161C33" stroke="#242A45"/><rect x="54" y="109" width="22" height="22" rx="7" fill="#4F46E5" opacity=".3"/><text x="65" y="124" fill="#AFC0FF" font-size="11" font-weight="800" text-anchor="middle">2</text><rect x="86" y="111" width="116" height="8" rx="4" fill="#2A3358"/><rect x="86" y="124" width="80" height="6" rx="3" fill="#242A45"/><rect x="216" y="110" width="48" height="20" rx="10" fill="#22B8D8" opacity=".16"/><text x="240" y="124" fill="#5FD3EC" font-size="9" font-weight="700" text-anchor="middle">25 puan</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin=".7s" fill="freeze"/><rect x="40" y="154" width="236" height="48" rx="12" fill="#161C33" stroke="#242A45"/><rect x="54" y="167" width="22" height="22" rx="7" fill="#4F46E5" opacity=".3"/><text x="65" y="182" fill="#AFC0FF" font-size="11" font-weight="800" text-anchor="middle">3</text><rect x="86" y="169" width="116" height="8" rx="4" fill="#2A3358"/><rect x="86" y="182" width="80" height="6" rx="3" fill="#242A45"/><rect x="216" y="168" width="48" height="20" rx="10" fill="#22B8D8" opacity=".16"/><text x="240" y="182" fill="#5FD3EC" font-size="9" font-weight="700" text-anchor="middle">50 puan</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1s" fill="freeze"/><rect x="40" y="212" width="236" height="44" rx="12" fill="none" stroke="#3A4568" stroke-dasharray="5 5"/><text x="158" y="239" fill="#93A3C4" font-size="11" font-weight="700" text-anchor="middle">+ Soru Ekle</text></g><circle cx="370" cy="112" r="48" fill="none" stroke="#242A45" stroke-width="11"/><circle cx="370" cy="112" r="48" fill="none" stroke="url(#teg)" stroke-width="11" stroke-linecap="round" transform="rotate(-90 370 112)" stroke-dasharray="301.6" stroke-dashoffset="301.6"><animate attributeName="stroke-dashoffset" values="301.6;226.2;150.8;0" keyTimes="0;.33;.66;1" dur="1.2s" begin=".2s" fill="freeze"/></circle><text x="370" y="116" fill="#fff" font-size="24" font-weight="800" text-anchor="middle">100</text><text x="370" y="134" fill="#93A3C4" font-size="9.5" font-weight="600" text-anchor="middle">/ 100 puan</text><text x="370" y="190" fill="#93A3C4" font-size="9.5" font-weight="600" text-anchor="middle">Rozet rengi</text>' +
        tseal(318,222,13,'#FBE7AE','#C9A04E','#7A5A22','te1','1.3s') +
        tseal(344,222,13,'#FCDCCD','#D98C77','#8E4E3F','te2','1.4s') +
        tseal(370,222,13,'#9AF5CE','#1CA273','#08503A','te3','1.5s') +
        tseal(396,222,13,'#B0D0FF','#2E6DE0','#122F78','te4','1.6s') +
        tseal(422,222,13,'#FFA3B5','#C8173D','#5E0819','te5','1.7s') +
        '<circle cx="370" cy="222" r="17" fill="none" stroke="#22B8D8" stroke-width="2" opacity="0"><animate attributeName="opacity" values="0;1" dur=".3s" begin="2s" fill="freeze"/></circle><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="2.2s" fill="freeze"/><rect x="318" y="248" width="104" height="24" rx="12" fill="#C21BE3"/><text x="370" y="264" fill="#fff" font-size="10.5" font-weight="700" text-anchor="middle">Yay\u0131na Al</text></g></svg>';
    VIZ.results = '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial"><rect x="40" y="36" width="260" height="228" rx="16" fill="#161C33" stroke="#242A45"/><text x="58" y="62" fill="#E7ECF7" font-size="12" font-weight="700" text-anchor="start">Sonu\u00e7lar</text><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin=".2s" fill="freeze"/><rect x="52" y="76" width="236" height="40" rx="10" fill="#1B2240"/><circle cx="72" cy="96" r="10" fill="#7A3BEA" opacity=".8"/><rect x="90" y="88" width="80" height="8" rx="4" fill="#2A3358"/><rect x="90" y="100" width="54" height="6" rx="3" fill="#242A45"/><text x="196" y="101" fill="#fff" font-size="12" font-weight="800" text-anchor="middle">92</text><rect x="222" y="87" width="54" height="18" rx="9" fill="#18C07A" opacity=".18"/><text x="249" y="100" fill="#18C07A" font-size="9.5" font-weight="700" text-anchor="middle">Ge\u00e7ti</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin=".5s" fill="freeze"/><rect x="52" y="124" width="236" height="40" rx="10" fill="#1B2240"/><circle cx="72" cy="144" r="10" fill="#7A3BEA" opacity=".8"/><rect x="90" y="136" width="80" height="8" rx="4" fill="#2A3358"/><rect x="90" y="148" width="54" height="6" rx="3" fill="#242A45"/><text x="196" y="149" fill="#fff" font-size="12" font-weight="800" text-anchor="middle">64</text><rect x="222" y="135" width="54" height="18" rx="9" fill="#E4467A" opacity=".18"/><text x="249" y="148" fill="#E4467A" font-size="9.5" font-weight="700" text-anchor="middle">Kald\u0131</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin=".8s" fill="freeze"/><rect x="52" y="172" width="236" height="40" rx="10" fill="#1B2240"/><circle cx="72" cy="192" r="10" fill="#7A3BEA" opacity=".8"/><rect x="90" y="184" width="80" height="8" rx="4" fill="#2A3358"/><rect x="90" y="196" width="54" height="6" rx="3" fill="#242A45"/><text x="196" y="197" fill="#fff" font-size="12" font-weight="800" text-anchor="middle">88</text><rect x="222" y="183" width="54" height="18" rx="9" fill="#18C07A" opacity=".18"/><text x="249" y="196" fill="#18C07A" font-size="9.5" font-weight="700" text-anchor="middle">Ge\u00e7ti</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1.1s" fill="freeze"/><rect x="52" y="222" width="100" height="26" rx="9" fill="none" stroke="#3A4568"/><text x="102" y="239" fill="#5FD3EC" font-size="10" font-weight="700" text-anchor="middle">Cevaplar\u0131 G\u00f6r</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1.4s" fill="freeze"/><rect x="316" y="60" width="130" height="182" rx="14" fill="#161C33" stroke="#E4467A" stroke-opacity=".55"/><text x="381" y="84" fill="#E7ECF7" font-size="11.5" font-weight="700" text-anchor="middle">Yeniden giri\u015f</text><text x="381" y="99" fill="#93A3C4" font-size="9.5" font-weight="500" text-anchor="middle">talebi</text><circle cx="381" cy="128" r="16" fill="#D98C77" opacity=".85"/><rect x="345" y="154" width="72" height="7" rx="3.5" fill="#2A3358"/><rect x="355" y="167" width="52" height="6" rx="3" fill="#242A45"/><rect x="332" y="190" width="98" height="26" rx="13" fill="#C21BE3"><animate attributeName="opacity" values="1;.7;1" dur="1.3s" begin="1.8s" repeatCount="3"/></rect><text x="381" y="207" fill="#fff" font-size="11" font-weight="700" text-anchor="middle">Onayla</text></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="3.8s" fill="freeze"/><circle cx="436" cy="70" r="13" fill="#12A150"/><text x="436" y="75" fill="#fff" font-size="13" font-weight="800" text-anchor="middle">\u2713</text></g></svg>';
    VIZ.broadcast = '<svg viewBox="0 0 480 300" font-family="Segoe UI,Arial"><rect x="40" y="56" width="190" height="176" rx="16" fill="#161C33" stroke="#242A45"/><text x="58" y="82" fill="#E7ECF7" font-size="12" font-weight="700" text-anchor="start">Toplu Bildirim</text><rect x="58" y="96" width="150" height="9" rx="4" fill="#2A3358"/><rect x="58" y="112" width="120" height="7" rx="3.5" fill="#242A45"/><rect x="58" y="125" width="136" height="7" rx="3.5" fill="#242A45"/><text x="58" y="176" fill="#93A3C4" font-size="9.5" font-weight="600" text-anchor="start">E-posta temas\u0131</text><rect x="66" y="186" width="20" height="20" rx="6" fill="#4F46E5"/><rect x="94" y="186" width="20" height="20" rx="6" fill="#7A3BEA"/><rect x="122" y="186" width="20" height="20" rx="6" fill="#D99A1E"/><rect x="150" y="186" width="20" height="20" rx="6" fill="#12A150"/><rect x="178" y="186" width="20" height="20" rx="6" fill="#E4467A"/><rect x="66" y="184" width="24" height="24" rx="7" fill="none" stroke="#22B8D8" stroke-width="2"/><g><path d="M0 -9 L24 0 L0 9 L5 0 Z" fill="#22B8D8"><animateMotion path="M230,144 C262,96 296,96 318,122" dur="1s" begin=".5s" fill="freeze" rotate="auto"/></path></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1.3s" fill="freeze"/><rect x="326" y="66" width="118" height="40" rx="12" fill="#161C33" stroke="#242A45"/><rect x="338" y="79" width="18" height="14" rx="3" fill="none" stroke="#5FD3EC" stroke-width="1.4"/><path d="M338 80 l9 7 l9 -7" fill="none" stroke="#5FD3EC" stroke-width="1.4"/><rect x="364" y="78" width="62" height="8" rx="4" fill="#2A3358"/><rect x="364" y="90" width="40" height="6" rx="3" fill="#242A45"/><circle cx="436" cy="74" r="6" fill="#E4467A"><animate attributeName="r" values="0;7;6" dur=".4s" begin="1.3s" fill="freeze"/></circle></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1.55s" fill="freeze"/><rect x="326" y="124" width="118" height="40" rx="12" fill="#161C33" stroke="#242A45"/><rect x="338" y="137" width="18" height="14" rx="3" fill="none" stroke="#5FD3EC" stroke-width="1.4"/><path d="M338 138 l9 7 l9 -7" fill="none" stroke="#5FD3EC" stroke-width="1.4"/><rect x="364" y="136" width="62" height="8" rx="4" fill="#2A3358"/><rect x="364" y="148" width="40" height="6" rx="3" fill="#242A45"/><circle cx="436" cy="132" r="6" fill="#E4467A"><animate attributeName="r" values="0;7;6" dur=".4s" begin="1.55s" fill="freeze"/></circle></g><g opacity="0"><animate attributeName="opacity" values="0;1" dur=".45s" begin="1.8s" fill="freeze"/><rect x="326" y="182" width="118" height="40" rx="12" fill="#161C33" stroke="#242A45"/><rect x="338" y="195" width="18" height="14" rx="3" fill="none" stroke="#5FD3EC" stroke-width="1.4"/><path d="M338 196 l9 7 l9 -7" fill="none" stroke="#5FD3EC" stroke-width="1.4"/><rect x="364" y="194" width="62" height="8" rx="4" fill="#2A3358"/><rect x="364" y="206" width="40" height="6" rx="3" fill="#242A45"/><circle cx="436" cy="190" r="6" fill="#E4467A"><animate attributeName="r" values="0;7;6" dur=".4s" begin="1.8s" fill="freeze"/></circle></g></svg>';

    var welcome = { viz: 'welcome', title: '\u0041sistan Portal\u2019a Ho\u015f Geldiniz', tag: 'Tur', desc: 'River Asistan Portal; i\u015f orta\u011f\u0131, m\u00fc\u015fteri ve ekiplerinizin tek \u00e7at\u0131 alt\u0131nda bulu\u015ftu\u011fu platformdur. Bu k\u0131sa turda rol\u00fcn\u00fcze \u00f6zel b\u00f6l\u00fcmleri ve neler yapabilece\u011finizi tan\u0131yacaks\u0131n\u0131z. \u0130stedi\u011finiz an profil men\u00fcn\u00fczdeki "Portal Turu" ad\u0131m\u0131ndan turu yeniden ba\u015flatabilirsiniz.' };
    function s(viz, title, tag, desc) { return { viz: viz, title: title, tag: tag, desc: desc }; }
    var STEPS = {
      partner: [welcome,
        s('gauge', 'Panel', 'Ana Sayfa', 'Y\u0131ll\u0131k cironuzu, g\u00fcncel partner seviyenizi ve bir \u00fcst seviyeye ula\u015fmak i\u00e7in kalan tutar\u0131 tek bak\u0131\u015fta g\u00f6r\u00fcrs\u00fcn\u00fcz. Seviyeniz son 1 y\u0131ldaki sat\u0131\u015flar\u0131n\u0131za g\u00f6re ayda bir otomatik g\u00fcncellenir; ciro artt\u0131k\u00e7a hak edi\u015f oran\u0131n\u0131z y\u00fckselir.'),
        s('orders', 'Sipari\u015fler', 'Anla\u015fmalar', 'Girdi\u011finiz anla\u015fmalar\u0131, tutarlar\u0131 ve hak edi\u015f oranlar\u0131n\u0131z\u0131 takip edin. Her anla\u015fmada seviyenize g\u00f6re komisyon, b\u00fcy\u00fck anla\u015fmalarda ek prim otomatik hesaplan\u0131r ve cironuza i\u015flenir.'),
        s('demo', 'Demo Talepleri', 'Demo', 'M\u00fc\u015fterileriniz i\u00e7in demo talebi olu\u015fturun. Talebiniz \u00fcretici onay\u0131na d\u00fc\u015fer; onay, planlama ve tamamlanma a\u015famalar\u0131n\u0131 buradan izlersiniz. M\u00fc\u015fteri kayd\u0131n\u0131z 30 g\u00fcn boyunca korunur.'),
        s('tiers', 'Partner Seviyeleri', 'Seviyeler', 'Kay\u0131tl\u0131, Silver, Gold ve Platinum seviyelerinin komisyon oranlar\u0131n\u0131, avantajlar\u0131n\u0131 ve \u015fartlar\u0131n\u0131 kar\u015f\u0131la\u015ft\u0131r\u0131n. Ciro artt\u0131k\u00e7a seviyeniz otomatik y\u00fckselir; \u00f6n \u00f6deme ile seviye y\u00fckseltme de m\u00fcmk\u00fcnd\u00fcr (sat\u0131\u015f temsilcinizle g\u00f6r\u00fc\u015f\u00fcn).'),
        s('docs', 'Sat\u0131\u015f Materyalleri', 'Materyaller', 'Sunumlar, bro\u015f\u00fcrler ve teknik dok\u00fcmanlar burada. \u0130htiyac\u0131n\u0131z olan materyali kategorisinden bulup tek t\u0131kla indirin; her zaman en g\u00fcncel s\u00fcr\u00fcme eri\u015firsiniz.'),
        s('chat', 'Destek', 'Destek', 'Teknik destek taleplerinizi a\u00e7\u0131n: \u00fcr\u00fcn/mod\u00fcl se\u00e7in, dosya ekleyin ve konu\u015fma ge\u00e7mi\u015fini takip edin. Destek ekibi \u00e7\u00f6z\u00fcm\u00fc i\u015faretledikten sonra talebi onaylay\u0131p kapatabilir veya yeniden a\u00e7abilirsiniz.'),
        s('play', 'E\u011fitim Merkezi', 'E\u011fitim', 'Video ve dok\u00fcmanlarla ekibinizi geli\u015ftirin. \u0130\u00e7erikleri kategoriye g\u00f6re ke\u015ffedin, arama yap\u0131n, kald\u0131\u011f\u0131n\u0131z yerden devam edin; ilerlemenizi ve tamamlad\u0131klar\u0131n\u0131z\u0131 E\u011fitim Ge\u00e7mi\u015fim\u2019den g\u00f6r\u00fcn.'),
        s('badges', 'Rozetlerim ve S\u0131navlar', 'Sertifika', 'Akademi\u2019deki Rozetlerim alan\u0131nda kazand\u0131\u011f\u0131n\u0131z ve kazanabilece\u011finiz rozetleri g\u00f6r\u00fcrs\u00fcn\u00fcz. Bir rozete t\u0131klad\u0131\u011f\u0131n\u0131zda s\u0131nav\u0131n giri\u015f \u015fartlar\u0131n\u0131 ad\u0131m ad\u0131m g\u00f6r\u00fcr, eksik videolar\u0131 tek t\u0131kla izler ve \u015fartlar tamamland\u0131\u011f\u0131nda s\u0131nava girersiniz. Deneme hakk\u0131n\u0131z biterse e\u011fitim edit\u00f6r\u00fcnden yeniden giri\u015f onay\u0131 isteyebilirsiniz.'),
        s('progress', 'E\u011fitim Ge\u00e7mi\u015fim', '\u0130lerleme', 'Her e\u011fitimde ka\u00e7 videoyu tamamlad\u0131\u011f\u0131n\u0131z\u0131, izleme oran\u0131n\u0131z\u0131 ve izleme s\u00fcrenizi g\u00f6r\u00fcn. Kalan videolar tek tek listelenir; t\u0131klayarak kald\u0131\u011f\u0131n\u0131z yerden devam edin. E\u011fitime ba\u011fl\u0131 s\u0131nav\u0131n durumu da burada g\u00f6r\u00fcn\u00fcr.'),
        s('cert', 'Sertifikalar\u0131m', 'Sertifika', 'S\u0131nav\u0131 ge\u00e7ti\u011finizde ad\u0131n\u0131za d\u00fczenlenen sertifikay\u0131 PDF olarak indirebilir, Akademi\u2019de rozet olarak sergileyebilirsiniz. Sertifikadaki QR kodu okutan herkes, giri\u015f yapmadan sertifikan\u0131n ge\u00e7erlili\u011fini do\u011frulayabilir.'),
        s('version', 'Versiyon Duyurular\u0131', 'Versiyon', 'River \u00fcr\u00fcnlerindeki yeni s\u00fcr\u00fcmleri, yenilik listelerini, yay\u0131n ve eri\u015fim tarihlerini takip edin. Yeni bir s\u00fcr\u00fcm yay\u0131nland\u0131\u011f\u0131nda portal i\u00e7inde ve e-posta ile bilgilendirilirsiniz.')],
      customer: [welcome,
        s('chat', 'Taleplerim', 'Destek', 'Destek taleplerinizi buradan a\u00e7ar ve takip edersiniz. \u00dcr\u00fcn/mod\u00fcl se\u00e7ip dosya ekleyebilir, ekiple mesajla\u015fabilir, \u00e7\u00f6z\u00fcm sonras\u0131 talebi onaylay\u0131p kapatabilirsiniz.'),
        s('play', 'E\u011fitim Merkezi', 'E\u011fitim', '\u00dcr\u00fcn e\u011fitim videolar\u0131n\u0131 izleyin, dok\u00fcmanlar\u0131 inceleyin. Kategoriler ve arama ile arad\u0131\u011f\u0131n\u0131z i\u00e7eri\u011fe h\u0131zl\u0131ca ula\u015f\u0131r, ilerlemenizi takip edersiniz.'),
        s('badges', 'Rozetlerim ve S\u0131navlar', 'Sertifika', 'Akademi\u2019deki Rozetlerim alan\u0131nda kazand\u0131\u011f\u0131n\u0131z ve kazanabilece\u011finiz rozetleri g\u00f6r\u00fcrs\u00fcn\u00fcz. Bir rozete t\u0131klad\u0131\u011f\u0131n\u0131zda s\u0131nav\u0131n giri\u015f \u015fartlar\u0131n\u0131 ad\u0131m ad\u0131m g\u00f6r\u00fcr, eksik videolar\u0131 tek t\u0131kla izler ve \u015fartlar tamamland\u0131\u011f\u0131nda s\u0131nava girersiniz. Deneme hakk\u0131n\u0131z biterse e\u011fitim edit\u00f6r\u00fcnden yeniden giri\u015f onay\u0131 isteyebilirsiniz.'),
        s('progress', 'E\u011fitim Ge\u00e7mi\u015fim', '\u0130lerleme', 'Her e\u011fitimde ka\u00e7 videoyu tamamlad\u0131\u011f\u0131n\u0131z\u0131, izleme oran\u0131n\u0131z\u0131 ve izleme s\u00fcrenizi g\u00f6r\u00fcn. Kalan videolar tek tek listelenir; t\u0131klayarak kald\u0131\u011f\u0131n\u0131z yerden devam edin. E\u011fitime ba\u011fl\u0131 s\u0131nav\u0131n durumu da burada g\u00f6r\u00fcn\u00fcr.'),
        s('cert', 'Sertifikalar\u0131m', 'Sertifika', 'S\u0131nav\u0131 ge\u00e7ti\u011finizde ad\u0131n\u0131za d\u00fczenlenen sertifikay\u0131 PDF olarak indirebilir, Akademi\u2019de rozet olarak sergileyebilirsiniz. Sertifikadaki QR kodu okutan herkes, giri\u015f yapmadan sertifikan\u0131n ge\u00e7erlili\u011fini do\u011frulayabilir.'),
        s('version', 'Versiyon Duyurular\u0131', 'Versiyon', 'Kulland\u0131\u011f\u0131n\u0131z \u00fcr\u00fcnlerin yeni s\u00fcr\u00fcmlerini ve yeniliklerini buradan \u00f6\u011frenin; her yeni s\u00fcr\u00fcmde bildirim al\u0131rs\u0131n\u0131z.')],
      vendor: [welcome,
        s('bars', 'Genel Bak\u0131\u015f', 'Panel', 'Y\u00f6netim panonuz; partner performans\u0131n\u0131, anla\u015fmalar\u0131 ve \u00f6zet grafikleri tek ekranda g\u00f6r\u00fcrs\u00fcn\u00fcz. \u0130\u015f orta\u011f\u0131 a\u011f\u0131n\u0131z\u0131n genel durumunu buradan izlersiniz.'),
        s('users', 'Partnerler', 'Partnerler', 'Yeni bayi/i\u015f orta\u011f\u0131 olu\u015fturun ve mevcut partnerleri y\u00f6netin. Her partnerin seviyesini, cirosunu ve kullan\u0131c\u0131lar\u0131n\u0131 buradan takip edersiniz.'),
        s('demo', 'Demo Onaylar\u0131', 'Demo', '\u0130\u015f ortaklar\u0131ndan gelen demo taleplerini inceleyip onaylay\u0131n. Onaylad\u0131\u011f\u0131n\u0131z talepler ilgili s\u00fcrece girer ve taraflara bildirim gider.'),
        s('users', 'Kullan\u0131c\u0131lar', 'Kullan\u0131c\u0131lar', '\u00dcretici, bayi ve m\u00fc\u015fteri kullan\u0131c\u0131lar\u0131n\u0131 olu\u015fturun; rollerini ve yetkilerini d\u00fczenleyin. Sat\u0131\u015f temsilcisi, e\u011fitim edit\u00f6r\u00fc, destek personeli gibi rollerle eri\u015fimi hassas y\u00f6netirsiniz.'),
        s('docs', 'Sat\u0131\u015f Materyalleri', 'Materyaller', 'Partnerlerinizle payla\u015f\u0131lacak sunum, bro\u015f\u00fcr ve dok\u00fcmanlar\u0131 y\u00fckleyin ve y\u00f6netin. Y\u00fckledikleriniz ilgili partnerlerde an\u0131nda g\u00f6r\u00fcn\u00fcr.'),
        s('chat', 'Destek Kuyru\u011fu', 'Destek', 'Gelen t\u00fcm destek taleplerini havuzda g\u00f6r\u00fcn; \u201c\u00dczerime Al\u201d ile \u00fcstlenin, yan\u0131tlay\u0131n, dahili not d\u00fc\u015f\u00fcn ve \u00e7\u00f6z\u00fcme kavu\u015fturun. \u00c7\u00f6z\u00fclen talepler belirli s\u00fcre sonunda otomatik kapan\u0131r.'),
        s('play', 'River Akademi', 'E\u011fitim', 'E\u011fitim i\u00e7eriklerini y\u00f6netin: video/dok\u00fcman ekleyin, kategori olu\u015fturun, i\u00e7erikleri d\u00fczenleyip yay\u0131ndan kald\u0131r\u0131n ve banner\u2019da \u00f6ne \u00e7\u0131karacaklar\u0131n\u0131z\u0131 se\u00e7in.'),
        s('examedit', 'S\u0131nav Olu\u015fturma', 'S\u0131nav', 'S\u0131nav olu\u015fturun, sorular\u0131 ekleyin ve her soruya puan verin; toplam 100 puana ula\u015ft\u0131\u011f\u0131nda s\u0131nav yay\u0131na al\u0131nabilir. Sertifika rozet rengini se\u00e7ip \u00f6nizleyin, giri\u015f \u015fart\u0131 olacak e\u011fitimi belirleyin ve haz\u0131r oldu\u011funuzda s\u0131nav\u0131 aktive edin.'),
        s('results', 'Sonu\u00e7lar ve Talepler', 'S\u0131nav', 'S\u0131nava kimlerin girdi\u011fini, puanlar\u0131n\u0131 ve verdikleri cevaplar\u0131 inceleyin. Deneme hakk\u0131n\u0131 dolduran kullan\u0131c\u0131lar\u0131n yeniden giri\u015f taleplerini onaylay\u0131n ya da reddedin; gerekirse verilmi\u015f bir sertifikay\u0131 iptal edin.'),
        s('broadcast', 'Toplu Bildirim', 'Bildirim', 'Se\u00e7ti\u011finiz bayi ve m\u00fc\u015fterilere uygulama i\u00e7i bildirim ve temal\u0131 e-posta g\u00f6nderin. Be\u015f haz\u0131r tema aras\u0131ndan konuya uygun olan\u0131 se\u00e7in ve g\u00f6ndermeden \u00f6nce canl\u0131 \u00f6nizleyin.'),
        s('version', 'Versiyon Duyurular\u0131', 'Versiyon', 'Yeni s\u00fcr\u00fcm duyurusu haz\u0131rlay\u0131p yay\u0131nlay\u0131n. Yay\u0131nlad\u0131\u011f\u0131n\u0131zda sistemdeki herkese hem portal i\u00e7i bildirim hem profesyonel e-posta g\u00f6nderilir.')],
      editor: [welcome,
        s('play', 'River Akademi', 'E\u011fitim', 'E\u011fitim Merkezi\u2019nin i\u00e7eri\u011fini siz y\u00f6netirsiniz: video ve dok\u00fcman ekleyin, kategori olu\u015fturun, ba\u015fl\u0131k/a\u00e7\u0131klama d\u00fczenleyin, yay\u0131ndan kald\u0131r\u0131n ve \u00f6ne \u00e7\u0131kanlar\u0131 belirleyin.'),
        s('examedit', 'S\u0131nav Olu\u015fturma', 'S\u0131nav', 'S\u0131nav olu\u015fturun, sorular\u0131 ekleyin ve her soruya puan verin; toplam 100 puana ula\u015ft\u0131\u011f\u0131nda s\u0131nav yay\u0131na al\u0131nabilir. Sertifika rozet rengini se\u00e7ip \u00f6nizleyin, giri\u015f \u015fart\u0131 olacak e\u011fitimi belirleyin ve haz\u0131r oldu\u011funuzda s\u0131nav\u0131 aktive edin.'),
        s('results', 'Sonu\u00e7lar ve Talepler', 'S\u0131nav', 'S\u0131nava kimlerin girdi\u011fini, puanlar\u0131n\u0131 ve verdikleri cevaplar\u0131 inceleyin. Deneme hakk\u0131n\u0131 dolduran kullan\u0131c\u0131lar\u0131n yeniden giri\u015f taleplerini onaylay\u0131n ya da reddedin; gerekirse verilmi\u015f bir sertifikay\u0131 iptal edin.'),
        s('version', 'Versiyon Duyurular\u0131', 'Versiyon', 'Yeni s\u00fcr\u00fcm duyurular\u0131n\u0131 haz\u0131rlay\u0131p yay\u0131nlayabilirsiniz; yay\u0131nland\u0131\u011f\u0131nda t\u00fcm kullan\u0131c\u0131lara bildirim gider.')],
      support: [welcome,
        s('chat', 'Destek Kuyru\u011fu', 'Destek', 'Havuzdaki destek taleplerini g\u00f6r\u00fcn, \u201c\u00dczerime Al\u201d ile \u00fcstlenin ve m\u00fc\u015fteriyle yaz\u0131\u015farak \u00e7\u00f6z\u00fcme kavu\u015fturun. Dahili notlar yaln\u0131zca \u00fcretici ekibine g\u00f6r\u00fcn\u00fcr.'),
        s('docs', 'Hizmet Formu & Proje Kay\u0131tlar\u0131', 'Hizmet', 'Talep a\u00e7\u0131lmadan verdi\u011finiz destek/proje hizmetlerini \u201cHizmet Formu\u201d ile m\u00fc\u015fteri ki\u015filerine iletebilirsiniz; s\u00fcreyi destek (dk) ya da proje (saat) bakiyesinden d\u00fc\u015fer, ge\u00e7mi\u015f kay\u0131tlar\u0131 Proje Kay\u0131tlar\u0131 sekmesinden firma baz\u0131nda analiz edersiniz.'),
        s('play', 'E\u011fitim Merkezi', 'E\u011fitim', 'E\u011fitim i\u00e7eriklerini izleyerek \u00fcr\u00fcn bilginizi g\u00fcncel tutun; kategoriler ve arama ile h\u0131zl\u0131ca eri\u015fin.'),
        s('version', 'Versiyon Duyurular\u0131', 'Versiyon', 'Yeni s\u00fcr\u00fcmleri ve yenilikleri takip edin; her s\u00fcr\u00fcmde bildirim al\u0131rs\u0131n\u0131z.')],
      partneruser: [welcome,
        s('chat', 'Destek', 'Destek', 'Destek taleplerinizi buradan a\u00e7ar ve takip edersiniz. \u00dcr\u00fcn/mod\u00fcl se\u00e7ip dosya ekleyebilir, ekiple mesajla\u015fabilir, \u00e7\u00f6z\u00fcm sonras\u0131 talebi onaylayabilirsiniz.'),
        s('play', 'E\u011fitim Merkezi', 'E\u011fitim', '\u00dcr\u00fcn e\u011fitim videolar\u0131n\u0131 izleyin, dok\u00fcmanlar\u0131 inceleyin. Kategoriler ve arama ile arad\u0131\u011f\u0131n\u0131z i\u00e7eri\u011fe h\u0131zl\u0131ca ula\u015f\u0131rs\u0131n\u0131z.'),
        s('badges', 'Rozetlerim ve S\u0131navlar', 'Sertifika', 'Akademi\u2019deki Rozetlerim alan\u0131nda kazand\u0131\u011f\u0131n\u0131z ve kazanabilece\u011finiz rozetleri g\u00f6r\u00fcrs\u00fcn\u00fcz. Bir rozete t\u0131klad\u0131\u011f\u0131n\u0131zda s\u0131nav\u0131n giri\u015f \u015fartlar\u0131n\u0131 ad\u0131m ad\u0131m g\u00f6r\u00fcr, eksik videolar\u0131 tek t\u0131kla izler ve \u015fartlar tamamland\u0131\u011f\u0131nda s\u0131nava girersiniz. Deneme hakk\u0131n\u0131z biterse e\u011fitim edit\u00f6r\u00fcnden yeniden giri\u015f onay\u0131 isteyebilirsiniz.'),
        s('progress', 'E\u011fitim Ge\u00e7mi\u015fim', '\u0130lerleme', 'Her e\u011fitimde ka\u00e7 videoyu tamamlad\u0131\u011f\u0131n\u0131z\u0131, izleme oran\u0131n\u0131z\u0131 ve izleme s\u00fcrenizi g\u00f6r\u00fcn. Kalan videolar tek tek listelenir; t\u0131klayarak kald\u0131\u011f\u0131n\u0131z yerden devam edin. E\u011fitime ba\u011fl\u0131 s\u0131nav\u0131n durumu da burada g\u00f6r\u00fcn\u00fcr.'),
        s('cert', 'Sertifikalar\u0131m', 'Sertifika', 'S\u0131nav\u0131 ge\u00e7ti\u011finizde ad\u0131n\u0131za d\u00fczenlenen sertifikay\u0131 PDF olarak indirebilir, Akademi\u2019de rozet olarak sergileyebilirsiniz. Sertifikadaki QR kodu okutan herkes, giri\u015f yapmadan sertifikan\u0131n ge\u00e7erlili\u011fini do\u011frulayabilir.'),
        s('version', 'Versiyon Duyurular\u0131', 'Versiyon', 'Kulland\u0131\u011f\u0131n\u0131z \u00fcr\u00fcnlerin yeni s\u00fcr\u00fcmlerini ve yeniliklerini buradan \u00f6\u011frenirsiniz.')]
    };

    var currentRole = 'partner', steps = [], idx = 0;
    var ov = document.createElement('div');
    ov.className = 'tour-ov'; ov.id = 'tourOv';
    ov.innerHTML =
      '<div class="tour-box tour-lg">' +
      '<button type="button" class="tour-skip" id="tourSkip">Turu Atla</button>' +
      '<div class="tour-main">' +
      '<div class="tour-viz" id="tourViz"></div>' +
      '<div class="tour-side">' +
      '<div class="tour-badge" id="tourBadge"></div>' +
      '<div class="tour-stepno" id="tourStepNo"></div>' +
      '<h3 id="tourTitle"></h3><p id="tourDesc"></p>' +
      '<div class="tour-dots" id="tourDots"></div>' +
      '<div class="tour-nav"><button type="button" class="btn-ghost tour-prev" id="tourPrev">Geri</button>' +
      '<button type="button" class="btn-cyan tour-next" id="tourNext">\u0130leri</button></div>' +
      '</div></div></div>';
    document.body.appendChild(ov);
    var G = function (id) { return document.getElementById(id); };

    function render() {
      var st = steps[idx];
      G('tourViz').innerHTML = VIZ[st.viz] || '';
      var badge = G('tourBadge');
      if (st.tag) { badge.textContent = st.tag; badge.style.display = ''; } else badge.style.display = 'none';
      G('tourStepNo').textContent = (idx + 1) + ' / ' + steps.length;
      G('tourTitle').textContent = st.title || '';
      G('tourDesc').textContent = st.desc || '';
      var d = ''; for (var i = 0; i < steps.length; i++) d += '<span class="' + (i === idx ? 'on' : '') + '"></span>';
      G('tourDots').innerHTML = d;
      G('tourPrev').style.visibility = idx === 0 ? 'hidden' : 'visible';
      G('tourNext').textContent = (idx === steps.length - 1) ? 'Ba\u015fla' : '\u0130leri';
    }
    function openTour(role) { steps = STEPS[role] || STEPS.partner; idx = 0; render(); ov.className = 'tour-ov open'; }
    function closeTour() { ov.className = 'tour-ov'; try { fetch('Tour.aspx?action=seen', { method: 'POST', credentials: 'same-origin' }); } catch (e) { } }
    G('tourSkip').addEventListener('click', closeTour);
    G('tourPrev').addEventListener('click', function () { if (idx > 0) { idx--; render(); } });
    G('tourNext').addEventListener('click', function () { if (idx < steps.length - 1) { idx++; render(); } else closeTour(); });
    document.addEventListener('keydown', function (e) { if (ov.className.indexOf('open') >= 0) { if (e.key === 'Escape') closeTour(); else if (e.key === 'ArrowRight') G('tourNext').click(); else if (e.key === 'ArrowLeft') G('tourPrev').click(); } });

    // Profil menusu: rol videosu olan kullanicilar icin "Tanitim Videosu" (tekrar izleme)
    function addIntroMenu() {
      var tmenu = document.getElementById('userMenu');
      if (!tmenu || document.getElementById('introOpen')) return;
      var tlogout = tmenu.querySelector('a[href="Logout.aspx"]');
      var titem = document.createElement('button');
      titem.type = 'button'; titem.className = 'um-item'; titem.id = 'introOpen'; titem.textContent = 'Tan\u0131t\u0131m Videosu';
      if (tlogout) tmenu.insertBefore(titem, tlogout); else tmenu.appendChild(titem);
      titem.addEventListener('click', function (e) { e.stopPropagation(); tmenu.classList.remove('open'); openIntro(null); });
    }

    // ---- Karsilama videosu (rol bazli): ilk giriste bir kez; profil menusunden tekrar izlenebilir ----
    function openIntro(done) {
      var ov = document.createElement('div');
      ov.className = 'intro-ov';
      ov.innerHTML =
        '<div class="intro-card" role="dialog" aria-modal="true" aria-labelledby="introTitle">' +
          '<button type="button" class="intro-x" aria-label="Kapat">&times;</button>' +
          '<div class="intro-hd"><h2 id="introTitle">Portal \u00f6zelliklerini tan\u0131y\u0131n.</h2></div>' +
          '<div class="intro-vid">' +
            '<video id="introVideo" preload="metadata" playsinline poster="IntroMedia.ashx?f=poster&v=2"><source src="IntroMedia.ashx?f=video&v=2" type="video/mp4" /></video>' +
            '<button type="button" class="intro-play" aria-label="Videoyu oynat"><span class="ip-ic"><svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor"><path d="M8 5.5v13a1 1 0 0 0 1.5.86l10.2-6.5a1 1 0 0 0 0-1.72L9.5 4.64A1 1 0 0 0 8 5.5z"/></svg></span><span class="ip-tx">Tan\u0131t\u0131m\u0131 \u0130zle</span><span class="ip-dur" id="introDur"></span></button>' +
          '</div>' +
          '<div class="intro-ft"><button type="button" class="btn-ghost intro-close">Kapat</button><button type="button" class="btn-cyan intro-go">Portala Ba\u015fla</button></div>' +
        '</div>';
      document.body.appendChild(ov);
      try { fetch('Tour.aspx?action=introseen', { method: 'POST', credentials: 'same-origin' }); } catch (e) { }   // acildigi anda "goruldu"
      var v = ov.querySelector('#introVideo'), play = ov.querySelector('.intro-play'), closed = false;
      function start() { v.controls = true; play.style.display = 'none'; var p = v.play(); if (p && p.catch) p.catch(function () { play.style.display = ''; v.controls = false; }); }
      play.addEventListener('click', start);
      v.addEventListener('play', function () { play.style.display = 'none'; v.controls = true; });
      v.addEventListener('ended', function () { ov.classList.add('ended'); });
      v.addEventListener('loadedmetadata', function () { var d = Math.round(v.duration || 0), el = ov.querySelector('#introDur'); if (el && d) el.textContent = Math.floor(d / 60) + ':' + ('0' + d % 60).slice(-2); });
      function close() {
        if (closed) return; closed = true;
        try { v.pause(); } catch (e) { }
        ov.classList.add('out');
        document.removeEventListener('keydown', onKey);
        setTimeout(function () { if (ov.parentNode) ov.parentNode.removeChild(ov); if (done) done(); }, 280);
      }
      function onKey(e) { if (e.key === 'Escape') close(); }
      document.addEventListener('keydown', onKey);
      ov.querySelector('.intro-x').addEventListener('click', close);
      ov.querySelector('.intro-close').addEventListener('click', close);
      ov.querySelector('.intro-go').addEventListener('click', close);
      requestAnimationFrame(function () { ov.classList.add('in'); });
      // Tarayici izin verirse sesli otomatik baslat; vermezse kapak + oynat butonu kalir
      setTimeout(function () { var p = v.play(); if (p && p.then) p.then(function () { play.style.display = 'none'; v.controls = true; }).catch(function () { }); }, 350);
    }
    window.RiverOpenIntro = openIntro;

    try {
      fetch('Tour.aspx?action=state', { credentials: 'same-origin' })
        .then(function (r) { return r.json(); })
        .then(function (j) {
          if (j && j.role) currentRole = j.role; if (j && j.avatar) setAvatar(j.avatar);
          if (j && j.hasIntro) addIntroMenu();
          if (j && j.intro) openIntro(null);          // otomatik portal turu kaldirildi
        })
        .catch(function () { });
    } catch (e) { }
  })();
})();

// Ana sayfa karsilamasindaki profil resmi: tiklaninca profil resmi penceresi (profil menusundeki "Avatar Yukle")
document.addEventListener('click', function (e) {
  var b = e.target.closest ? e.target.closest('[data-hero-ava]') : null; if (!b) return;
  var o = document.getElementById('avaOpen'); if (o) o.click();
});

