// Akademi: editor kategori yonetimi (Kategoriler sekmesi) ve formlarda alt kategoriyi ana kategoriye gore suzme.
(function () {
  function el(tag, cls, txt) { var e = document.createElement(tag); if (cls) e.className = cls; if (txt != null) e.textContent = txt; return e; }
  var err = document.getElementById('tcErr');
  var FORMS = { Video: 'form[action="AddTrainingVideo.aspx"]', Document: 'form[action="AddTrainingDocument.aspx"]' };
  var low = function (s) { return String(s || '').toLocaleLowerCase('tr-TR'); };
  // kart temalari: dokulu renk alanlari, css .acg-0 .. .acg-14 ile ayni sira (sunucuda tema numarasi 0..14)
  // hazir kategori ikonlari: assets/caticons.svg (sunucudaki TrainingCats.Icons ile ayni liste)
  var ICONS = [["form", "Form"], ["flow", "Akış"], ["doc", "Belge"], ["template", "Şablon"], ["pdf", "PDF"], ["table", "Tablo"], ["catalog", "Katalog"], ["database", "Veritabanı"], ["chart", "Rapor"], ["dashboard", "Pano"], ["api", "Entegrasyon"], ["settings", "Ayarlar"], ["team", "Ekip"], ["lock", "Yetki"], ["shield", "Güvenlik"], ["bell", "Bildirim"], ["calendar", "Takvim"], ["mail", "E-posta"], ["chat", "Sohbet"], ["ai", "Yapay Zekâ"], ["mobile", "Mobil"], ["globe", "Web"], ["cloud", "Bulut"], ["code", "Kod"], ["upload", "Yükleme"], ["search", "Arama"], ["check", "Onay"], ["sign", "İmza"], ["qr", "Barkod"], ["pin", "Konum"], ["image", "Galeri"], ["play", "Video"], ["news", "Haber"], ["cap", "Eğitim"], ["book", "Kitap"], ["idea", "Fikir"], ["target", "Hedef"], ["rocket", "Başlangıç"], ["cart", "Satış"], ["invoice", "Fatura"], ["puzzle", "Bileşen"], ["layers", "Katman"], ["building", "Intranet"], ["clock", "Zaman"]];
  var svgUse = function (id) { var ns = 'http://www.w3.org/2000/svg', s = document.createElementNS(ns, 'svg'), u = document.createElementNS(ns, 'use'); s.setAttribute('aria-hidden', 'true'); u.setAttribute('href', 'assets/caticons.svg#i-' + id); s.appendChild(u); return s; };
  var THEMES = ['Okyanus', 'Gün batımı', 'Grafit', 'Lavanta', 'Zümrüt', 'Kehribar', 'Gül', 'Gece', 'Güneş', 'Lagün', 'Ametist', 'Mercan', 'Orman', 'Şarap', 'Buz'];

  // ---- formlarda alt kategori suzme ----
  function wire(form) {
    if (!form) return;
    var picks = form.querySelectorAll('.ec-catpick'); if (picks.length < 2) return;
    var mSel = picks[0].querySelector('.ec-cat-sel'), mNew = picks[0].querySelector('.ec-cat-new'), sSel = picks[1].querySelector('.ec-cat-sel');
    function filter() {
      var g = mSel.value === '__new__' ? '' : mSel.value, cur = sSel.value;
      [].forEach.call(sSel.options, function (o) {
        if (o.value === '' || o.value === '__new__') return;
        var vis = g !== '' && low(o.getAttribute('data-g')) === low(g);
        o.hidden = !vis; o.disabled = !vis;
      });
      var so = sSel.options[sSel.selectedIndex];
      if (cur && cur !== '__new__' && so && so.disabled) { sSel.value = ''; sSel.dispatchEvent(new Event('change')); }
    }
    mSel.addEventListener('change', filter); if (mNew) mNew.addEventListener('input', filter);
    form._tcFilter = filter; filter();
  }
  wire(document.querySelector(FORMS.Video)); wire(document.querySelector(FORMS.Document));

  // kategori listesi degisince formlardaki secenekleri yenile
  function syncForm(scope, cats) {
    var form = document.querySelector(FORMS[scope]); if (!form) return;
    var picks = form.querySelectorAll('.ec-catpick'); if (picks.length < 2) return;
    var mSel = picks[0].querySelector('.ec-cat-sel'), sSel = picks[1].querySelector('.ec-cat-sel');
    var mv = mSel.value, sv = sSel.value;
    var rebuild = function (sel, items) {
      var first = sel.options[0], last = sel.options[sel.options.length - 1];
      while (sel.options.length) sel.remove(0);
      sel.add(first); items.forEach(function (it) { var o = el('option', null, it.name); o.value = it.name; if (it.g) o.setAttribute('data-g', it.g); sel.add(o); }); sel.add(last);
    };
    rebuild(mSel, cats.map(function (c) { return { name: c.name }; }));
    var subs = []; cats.forEach(function (c) { c.subs.forEach(function (s) { subs.push({ name: s.name, g: c.name }); }); });
    rebuild(sSel, subs);
    mSel.value = mv; if (mSel.value !== mv) mSel.value = ''; sSel.value = sv;
    mSel.dispatchEvent(new Event('change')); if (form._tcFilter) form._tcFilter();
  }

  // ---- Kategoriler sekmesi ----
  function api(scope, op, data) {
    var url = 'TrainingCategory.ashx?scope=' + scope + (op === 'list' ? '&op=list' : '');
    var opt = { credentials: 'same-origin' };
    if (op !== 'list') { var fd = new URLSearchParams(); fd.append('op', op); fd.append('scope', scope); for (var k in data) fd.append(k, data[k]); opt.method = 'POST'; opt.headers = { 'Content-Type': 'application/x-www-form-urlencoded' }; opt.body = fd.toString(); }
    return fetch(url, opt).then(function (r) { return r.json(); });
  }
  function act(col, op, data, confirmMsg) {
    if (confirmMsg && !window.confirm(confirmMsg)) return;
    err.textContent = '';
    api(col.getAttribute('data-scope'), op, data || {}).then(function (d) { if (!d.ok && d.error) err.textContent = d.error; render(col, d.cats); })
      .catch(function () { err.textContent = 'Bağlantı hatası, tekrar deneyin.'; });
  }
  function btn(txt, title, fn, cls) { var b = el('button', 'tc-b' + (cls ? ' ' + cls : ''), txt); b.type = 'button'; b.title = title; b.setAttribute('aria-label', title); b.addEventListener('click', fn); return b; }
  function inline(row, value, onSave) {   // satir icinde ad duzenleme / ekleme
    var f = el('form', 'tc-inline'), i = el('input', 'input'); i.maxLength = 120; i.value = value || ''; f.setAttribute('data-noload', '1');
    var ok = el('button', 'btn-cyan', 'Kaydet'); ok.type = 'submit';
    var no = el('button', 'btn-ghost', 'Vazgeç'); no.type = 'button';
    f.appendChild(i); f.appendChild(ok); f.appendChild(no); row.appendChild(f); i.focus(); i.select();
    f.addEventListener('submit', function (e) { e.preventDefault(); var v = i.value.trim(); if (v) onSave(v); });
    no.addEventListener('click', function () { f.remove(); });
    i.addEventListener('keydown', function (e) { if (e.key === 'Escape') { e.stopPropagation(); f.remove(); } });
  }
  function item(col, c, isSub, i, n) {
    var row = el('div', 'tc-row' + (isSub ? ' sub' : '')), line = el('div', 'tc-line');
    var st = c.style || {};
    if (st.tint != null || st.card || st.icon) {
      var mk = el('span', 'tc-mark'); mk.title = 'Özel görünüm';
      if (st.tint != null) mk.className = 'tc-mark acg acg-' + st.tint;
      if (st.card) { var im = el('img'); im.src = st.card; im.alt = ''; mk.appendChild(im); }
      else if (st.iconUrl) { mk.classList.add('ico'); var mu = el('i', 'ico-u'); mu.style.setProperty('--iu', "url('" + new URL(st.iconUrl, location.href).href + "')"); mk.appendChild(mu); }
      else if (st.icon) { mk.classList.add('ico'); mk.appendChild(svgUse(st.icon)); }
      line.appendChild(mk);
    }
    line.appendChild(el('span', 'tc-name', c.name));
    line.appendChild(el('span', 'tc-cnt', c.count + ' içerik'));
    var tools = el('span', 'tc-tools');
    tools.appendChild(btn('↑', 'Yukarı taşı', function () { act(col, 'move', { id: c.id, dir: 'up' }); }, i === 0 ? 'dis' : ''));
    tools.appendChild(btn('↓', 'Aşağı taşı', function () { act(col, 'move', { id: c.id, dir: 'down' }); }, i === n - 1 ? 'dis' : ''));
    tools.appendChild(btn('✎', 'Yeniden adlandır', function () { if (!row.querySelector('.tc-inline')) inline(row, c.name, function (v) { act(col, 'rename', { id: c.id, name: v }); }); }));
    tools.appendChild(btn('Görünüm', 'Kart rengi, kart görseli ve kapak görseli', function () { stylePanel(col, row, c, isSub); }, 'sty'));
    if (!isSub) tools.appendChild(btn('+ alt', 'Alt kategori ekle', function () { if (!row.querySelector('.tc-inline')) inline(row, '', function (v) { act(col, 'addSub', { id: c.id, name: v }); }); }, 'add'));
    tools.appendChild(btn('✕', 'Sil', function () {
      act(col, 'delete', { id: c.id }, c.count > 0 ? null : '"' + c.name + '"' + (isSub ? ' alt kategorisi' : ' ana kategorisi' + (c.subs && c.subs.length ? ' ve boş alt kategorileri' : '')) + ' silinsin mi?');
    }, 'del'));
    line.appendChild(tools); row.appendChild(line);
    return row;
  }
  // ---- Gorunum paneli: kart rengi, kart gorseli, kapak gorseli (canli onizleme) ----
  var uid = 0;
  function stylePanel(col, row, c, isSub) {
    var open = row.querySelector('.tc-style'); if (open) { open.remove(); return; }
    var st = c.style || {}, tint = st.tint == null ? '' : String(st.tint), cardUrl = st.card, coverUrl = st.cover;
    var abs = function (u) { try { return new URL(u, location.href).href; } catch (e) { return u; } };   // url() stil dosyasina gore cozulur: mutlak adres
    var custom = /^u:/.test(st.icon || ''), myUrl = custom && st.iconUrl ? abs(st.iconUrl) : null;
    var mode = st.card ? 'upload' : custom ? 'myicon' : st.icon ? 'icon' : 'auto', icon = custom ? '' : (st.icon || '');
    var f = el('form', 'tc-style'); f.setAttribute('data-noload', '1');
    var prev = el('div', 'tc-prev'), pImg = el('div', 'tc-prev-img'), pName = el('div', 'tc-prev-n', c.name), pAuto = el('span', 'tc-prev-auto', 'Otomatik görsel');
    prev.appendChild(pImg); prev.appendChild(pName); f.appendChild(prev);
    var body = el('div', 'tc-style-b'); f.appendChild(body);
    function paint() {
      prev.className = tint === '' ? 'tc-prev auto' : 'tc-prev acg acg-' + tint;
      pImg.innerHTML = ''; pImg.classList.toggle('ico', mode === 'icon' && !!icon);
      pImg.classList.toggle('ico', (mode === 'icon' && !!icon) || (mode === 'myicon' && !!myUrl));
      if (mode === 'icon' && icon) { var gl = el('span', 'ico-g'); gl.appendChild(svgUse(icon)); pImg.appendChild(gl); }
      else if (mode === 'myicon' && myUrl) { var g2 = el('span', 'ico-g'), iu = el('i', 'ico-u'); iu.style.setProperty('--iu', "url('" + myUrl + "')"); g2.appendChild(iu); pImg.appendChild(g2); }
      else if (mode === 'upload' && cardUrl) { var i = el('img'); i.src = cardUrl; i.alt = ''; pImg.appendChild(i); }
      else pImg.appendChild(pAuto);
    }
    // renk
    var g = el('div', 'tc-fs'); g.appendChild(el('span', 'tc-lbl', 'Kart rengi'));
    var sw = el('div', 'tc-sw'); sw.setAttribute('role', 'radiogroup'); sw.setAttribute('aria-label', 'Kart rengi');
    var opts = [['', 'Otomatik']].concat(THEMES.map(function (x, i) { return [String(i), x]; }));
    opts.forEach(function (o) {
      var b = el('button', 'tc-swb' + (o[0] === '' ? ' auto' : '')); b.type = 'button'; b.setAttribute('role', 'radio'); b.title = o[1]; b.setAttribute('aria-label', o[1]);
      if (o[0] === '') b.textContent = 'Otomatik'; else b.className = 'tc-swb acg acg-' + o[0];
      b.addEventListener('click', function () { tint = o[0]; mark(); paint(); });
      sw.appendChild(b);
    });
    function mark() { [].forEach.call(sw.children, function (b, i) { var on = opts[i][0] === tint; b.classList.toggle('on', on); b.setAttribute('aria-checked', on ? 'true' : 'false'); }); }
    var label = el('span', 'tc-swname'); g.appendChild(label);
    var mark0 = mark; mark = function () { mark0(); label.textContent = tint === '' ? 'Otomatik: raftaki sıraya göre' : THEMES[+tint]; };
    g.appendChild(sw); body.appendChild(g);
    // gorseller
    function upload(label, name, cur, onPick, host) {
      var id = 'tcu' + (++uid), w = el('div', 'tc-fs tc-up');
      var l = el('label', 'tc-lbl', label); l.htmlFor = id; w.appendChild(l);
      var rowEl = el('div', 'tc-up-r'), th = el('div', 'tc-thumb');
      var setTh = function (u) { th.innerHTML = ''; if (u) { var i = el('img'); i.src = u; i.alt = ''; th.appendChild(i); } else th.appendChild(el('span', null, 'Yok')); };
      setTh(cur); rowEl.appendChild(th);
      var inp = el('input'); inp.type = 'file'; inp.id = id; inp.name = name; inp.accept = 'image/png,image/jpeg,image/webp'; rowEl.appendChild(inp);
      var clr = null;
      if (cur) { var cl = el('label', 'tc-clr'); clr = el('input'); clr.type = 'checkbox'; cl.appendChild(clr); cl.appendChild(document.createTextNode(' Kaldır')); rowEl.appendChild(cl);
        clr.addEventListener('change', function () { if (clr.checked) { inp.value = ''; setTh(null); onPick(null); } else { setTh(cur); onPick(cur); } }); }
      inp.addEventListener('change', function () { var fl = inp.files && inp.files[0]; if (!fl) return; if (clr) clr.checked = false; var u = URL.createObjectURL(fl); setTh(u); onPick(u); });
      w.appendChild(rowEl); (host || body).appendChild(w);
      return { inp: inp, clr: function () { return clr && clr.checked; } };
    }
    // kart gorseli: Otomatik (ilk video) / Ikon (hazir) / Gorsel yukle
    var cs = el('div', 'tc-fs'); cs.appendChild(el('span', 'tc-lbl', 'Kart görseli (raftaki kart)'));
    var seg = el('div', 'tc-seg'); seg.setAttribute('role', 'radiogroup'); seg.setAttribute('aria-label', 'Kart görseli');
    var MODES = [['auto', 'Otomatik'], ['icon', 'İkon'], ['myicon', 'İkon yükle'], ['upload', 'Görsel yükle']];
    var autoHint = el('p', 'tc-hint', 'Kategorinin ilk videosunun görseli kullanılır.');
    var icoBox = el('div', 'tc-icons'); icoBox.setAttribute('role', 'radiogroup'); icoBox.setAttribute('aria-label', 'Kategori ikonu');
    var upHost = el('div'), myHost = el('div', 'tc-myico');
    ICONS.forEach(function (x) {
      var b = el('button', 'tc-ico'); b.type = 'button'; b.title = x[1]; b.setAttribute('role', 'radio'); b.setAttribute('aria-label', x[1]); b.setAttribute('data-i', x[0]);
      b.appendChild(svgUse(x[0])); b.appendChild(el('span', null, x[1]));
      b.addEventListener('click', function () { icon = x[0]; sync(); paint(); });
      icoBox.appendChild(b);
    });
    MODES.forEach(function (m) {
      var b = el('button', 'tc-segb', m[1]); b.type = 'button'; b.setAttribute('role', 'radio'); b.setAttribute('data-m', m[0]);
      b.addEventListener('click', function () { mode = m[0]; sync(); paint(); });
      seg.appendChild(b);
    });
    function sync() {
      [].forEach.call(seg.children, function (b) { var on = b.getAttribute('data-m') === mode; b.classList.toggle('on', on); b.setAttribute('aria-checked', on ? 'true' : 'false'); });
      [].forEach.call(icoBox.children, function (b) { var on = b.getAttribute('data-i') === icon; b.classList.toggle('on', on); b.setAttribute('aria-checked', on ? 'true' : 'false'); });
      autoHint.hidden = mode !== 'auto'; icoBox.hidden = mode !== 'icon'; upHost.hidden = mode !== 'upload'; myHost.hidden = mode !== 'myicon';
    }
    cs.appendChild(seg); cs.appendChild(autoHint); cs.appendChild(icoBox); cs.appendChild(myHost); cs.appendChild(upHost); body.appendChild(cs);
    // kendi ikonu: saydam zeminli PNG; sunucu kirpar, ortalar ve hazir ikonlarla ayni yere, ayni egimle yerlestirir
    var myId = 'tcmi' + (++uid), myInp = el('input'); myInp.type = 'file'; myInp.id = myId; myInp.accept = 'image/png';
    var myLbl = el('label', 'tc-lbl', 'İkon dosyası (saydam zeminli PNG)'); myLbl.htmlFor = myId;
    myHost.appendChild(myLbl); myHost.appendChild(myInp);
    myHost.appendChild(el('p', 'tc-hint', 'Boyutu önemli değil: boş kenarlar kırpılır, ikon hazır ikonlarla aynı ölçüde ve eğimle yerleştirilir. Yalnız şekli kullanılır; renkler kartın temasına uyar.'));
    // onizleme sunucudaki islemin aynisi: saydam kenarlar kirpilir, 512 kareye %10 payla ortalanir; zemin saydam degilse once burada uyarilir
    function prepIcon(fl, cb) {
      var img = new Image(), src = URL.createObjectURL(fl);
      img.onload = function () {
        var w = img.naturalWidth, h = img.naturalHeight, cv = document.createElement('canvas'); cv.width = w; cv.height = h;
        var cx = cv.getContext('2d'); cx.drawImage(img, 0, 0); var d = cx.getImageData(0, 0, w, h).data;
        var minX = w, minY = h, maxX = -1, maxY = -1, clear = 0;
        for (var y = 0; y < h; y++) for (var x = 0; x < w; x++) { var a = d[(y * w + x) * 4 + 3]; if (a < 250) clear++; if (a > 12) { if (x < minX) minX = x; if (x > maxX) maxX = x; if (y < minY) minY = y; if (y > maxY) maxY = y; } }
        URL.revokeObjectURL(src);
        if (maxX < 0) { cb('İkon tamamen saydam görünüyor.'); return; }
        if (clear < w * h / 20) { cb('İkonun zemini saydam olmalı (saydam zeminli PNG yükleyin). Saydam olmayan zemin kartta dikdörtgen olarak görünür.'); return; }
        var S = 512, pad = 51, cw = maxX - minX + 1, ch = maxY - minY + 1, k = Math.min((S - 2 * pad) / cw, (S - 2 * pad) / ch);
        var dw = Math.max(1, Math.round(cw * k)), dh = Math.max(1, Math.round(ch * k)), o = document.createElement('canvas'); o.width = S; o.height = S;
        var ox = o.getContext('2d'); ox.imageSmoothingQuality = 'high'; ox.drawImage(cv, minX, minY, cw, ch, Math.round((S - dw) / 2), Math.round((S - dh) / 2), dw, dh);
        cb(null, o.toDataURL('image/png'));
      };
      img.onerror = function () { URL.revokeObjectURL(src); cb('İkon okunamadı; geçerli bir PNG seçin.'); };
      img.src = src;
    }
    myInp.addEventListener('change', function () {
      var fl = myInp.files && myInp.files[0]; if (!fl) return; er.textContent = '';
      prepIcon(fl, function (e, u) { if (e) { er.textContent = e; myInp.value = ''; myUrl = custom && st.iconUrl ? abs(st.iconUrl) : null; paint(); return; } myUrl = u; paint(); });
    });
    var uc = upload('Görsel', 'card', st.card, function (u) { cardUrl = u; paint(); }, upHost);
    sync();
    var uv = upload('Kapak görseli (kategori sayfası)', 'cover', coverUrl, function () { });
    body.appendChild(el('p', 'tc-hint', 'PNG, JPG ya da WebP · en fazla 5 MB · önerilen boyut 1280×720. Kapak görseli verilmezse kart görseli ya da seçilen ikon kullanılır; hiçbiri yoksa ilk videonun görseli gösterilir.'));
    var er = el('p', 'tc-err2'); er.setAttribute('role', 'alert'); body.appendChild(er);
    var acts = el('div', 'tc-acts'), ok = el('button', 'btn-cyan', 'Kaydet'), no = el('button', 'btn-ghost', 'Vazgeç'); ok.type = 'submit'; no.type = 'button';
    acts.appendChild(ok); acts.appendChild(no); body.appendChild(acts);
    no.addEventListener('click', function () { f.remove(); });
    f.addEventListener('submit', function (e) {
      e.preventDefault(); er.textContent = ''; ok.disabled = true; ok.textContent = 'Kaydediliyor…';
      if (mode === 'icon' && !icon) { er.textContent = 'Bir ikon seçin.'; ok.disabled = false; ok.textContent = 'Kaydet'; return; }
      if (mode === 'myicon' && !myUrl) { er.textContent = 'Yüklenecek ikonu seçin (saydam zeminli PNG).'; ok.disabled = false; ok.textContent = 'Kaydet'; return; }
      var fd = new FormData(); fd.append('op', 'style'); fd.append('id', c.id); fd.append('tint', tint); fd.append('mode', mode);
      if (mode === 'icon') fd.append('icon', icon);
      if (mode === 'myicon' && myInp.files[0]) fd.append('iconfile', myInp.files[0]);
      if (mode === 'upload' && uc.inp.files[0]) fd.append('card', uc.inp.files[0]); if (uv.inp.files[0]) fd.append('cover', uv.inp.files[0]);
      if (mode === 'upload' && uc.clr()) fd.append('clearCard', '1'); if (uv.clr()) fd.append('clearCover', '1');
      fetch('TrainingCategory.ashx?scope=' + col.getAttribute('data-scope'), { method: 'POST', body: fd, credentials: 'same-origin' })
        .then(function (r) { return r.json(); })
        .then(function (d) { if (!d.ok) { er.textContent = d.error || 'Kaydedilemedi.'; ok.disabled = false; ok.textContent = 'Kaydet'; return; } render(col, d.cats); err.textContent = ''; })
        .catch(function () { er.textContent = 'Bağlantı hatası, tekrar deneyin.'; ok.disabled = false; ok.textContent = 'Kaydet'; });
    });
    mark(); paint(); row.appendChild(f);
    f.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
  }

  function render(col, cats) {
    var list = col.querySelector('.tc-list'); list.innerHTML = '';
    if (!cats.length) list.appendChild(el('p', 'tc-empty', 'Henüz kategori yok.'));
    cats.forEach(function (c, i) {
      var box = el('div', 'tc-main'); box.appendChild(item(col, c, false, i, cats.length));
      c.subs.forEach(function (s, k) { box.appendChild(item(col, s, true, k, c.subs.length)); });
      list.appendChild(box);
    });
    syncForm(col.getAttribute('data-scope'), cats);
  }
  [].forEach.call(document.querySelectorAll('.tc-col'), function (col) {
    var f = col.querySelector('.tc-add'), i = f.querySelector('input');
    f.addEventListener('submit', function (e) { e.preventDefault(); var v = i.value.trim(); if (!v) { i.focus(); return; } act(col, 'addMain', { name: v }); i.value = ''; });
    api(col.getAttribute('data-scope'), 'list').then(function (d) { if (!d.ok && d.error) err.textContent = d.error; render(col, d.cats); }).catch(function () { err.textContent = 'Kategoriler yüklenemedi.'; });
  });
})();
