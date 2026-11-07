(function () {
  var dash = document.getElementById('dash');
  var GAUGE = parseFloat(dash.getAttribute('data-gauge')) || 0;
  var BAND = parseFloat(dash.getAttribute('data-band')) || 0;
  var INIT = dash.getAttribute('data-culture') || 'tr';

  // ---- Hakediş göstergesi ----
  var arcLen = 628 * (270 / 360);
  var fill = document.getElementById('arcFill');
  if (fill) {
    fill.setAttribute('stroke-dasharray', arcLen + ' 628');
    setTimeout(function () {
      fill.style.transition = 'stroke-dashoffset 1.4s cubic-bezier(.2,.8,.2,1)';
      fill.setAttribute('stroke-dashoffset', arcLen * (1 - GAUGE));
    }, 400);
    var marks = [1.25 / 4, 2.5 / 4, 1.0];
    var g = document.getElementById('ticks');
    for (var i = 0; i < marks.length; i++) {
      var ang = (135 + marks[i] * 270) * Math.PI / 180;
      var x = 120 + Math.cos(ang) * 100, y = 120 + Math.sin(ang) * 100;
      var c = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
      c.setAttribute('cx', x); c.setAttribute('cy', y); c.setAttribute('r', 3.2);
      c.setAttribute('fill', '#EAF2FF'); c.setAttribute('opacity', '.7');
      g.appendChild(c);
    }
  }

  // ---- Sonraki seviye çubuğu ----
  setTimeout(function () {
    var b = document.getElementById('nlBar');
    if (b) b.style.width = (BAND * 100) + '%';
  }, 500);

  // ---- Loading kapat ----
  window.addEventListener('load', function () {
    setTimeout(function () {
      var l = document.getElementById('loader');
      if (l) l.className = (l.className + ' hide');
    }, 1000);
  });

  // ---- Bildirim ----
  var bellBtn = document.getElementById('bellBtn');
  var bellMenu = document.getElementById('bellMenu');
  if (bellBtn) {
    bellBtn.addEventListener('click', function () {
      if (bellMenu.className.indexOf('open') >= 0) bellMenu.className = 'dropdown';
      else bellMenu.className = 'dropdown open';
    });
  }
  var markAll = document.getElementById('markAll');
  if (markAll) {
    markAll.addEventListener('click', function () {
      var items = document.querySelectorAll('.ntf');
      for (var i = 0; i < items.length; i++) {
        if (items[i].className.indexOf('read') < 0) items[i].className = items[i].className + ' read';
      }
      var cnt = document.getElementById('bellCount');
      if (cnt) cnt.style.display = 'none';
      var bw = document.querySelector('.bell'); if (bw) bw.classList.remove('has-unread');
      try {
        var xhr = new XMLHttpRequest();
        xhr.open('POST', 'MarkRead.aspx', true);
        xhr.send();
      } catch (e) {}
    });
  }
  document.addEventListener('click', function (e) {
    var t = e.target;
    var inBell = false;
    while (t) { if (t.className && ('' + t.className).indexOf('bell') >= 0) { inBell = true; break; } t = t.parentNode; }
    if (!inBell && bellMenu) bellMenu.className = 'dropdown';
  });

  // ---- Çok dilli sözlük ----
  var I18N = {
    tr: { loading: 'Portal Yükleniyor', brand_sub: 'Asistan Portal', footer_by: '', nav_dash: 'Panel', nav_deals: 'Anlaşmalar', nav_orders: 'Siparişler',
      nav_demos: 'Demo Talepleri', nav_tiers: 'Partner Seviyeleri', nav_support: 'Destek', nav_materials: 'Satış Materyalleri', nav_training: 'River Akademi', notif_title: 'Bildirimler',
      notif_markall: 'Tümünü okundu işaretle', hero_lbl: 'Yıllık Ciro', sig_title: 'İndirilebilir Mail İmzanız Hazır', sig_p1: 'İndirilebilir mail imzanız hazır.', sig_p2: 'Maillerinizde partner imzanızın yer alması halinde hedef seviyeniz için ek ödül; web sitenizde River AI Partner logosunun bulunması halinde ise ek hedef bakiyesi tanımlanır. İşlemi tamamladıktan sonra doğrulama için portföy yöneticinizle iletişime geçebilirsiniz.', sig_hint: 'Görselin üzerine sağ tıklayıp "Resmi farklı kaydet" diyerek indirebilir, mail imzanıza ekleyebilirsiniz.', sig_dl: 'İmzayı İndir',  user_role: 'Partner Yöneticisi',
      greet: 'Hoş geldin,', m_base: 'Lisans Satışı', m_rental: 'Kiralama Komisyonu', m_renewal: 'Versiyon Yenileme', m_license: 'Lisans/Modül Yükseltme', m_sla: 'SLA Seviyesi', m_act: '12 Ay Aktiflik', active: 'Aktif',
      nl_spark: 'Sıradaki Seviye', nl_almost: 'Bir üst seviyeye çok yakınsınız. Satışlarınızı artırarak 2027 dönemine yüksek hakediş seviyesiyle girin.', nl_remain: 'Kalan ciro:',
      u1_b: 'Her anlaşmada daha yüksek hakediş', u2_t: 'Özel Hesap Yöneticisi', u2_b: 'Stratejik SLA ve birebir destek hattı',
      u3_t: 'Ortak Pazarlama Fonu (MDF)', u3_b: 'Kampanyalarına River bütçe desteği',
      u4_t: 'Beta Özelliklere Erken Erişim', u4_b: 'Yeni modülleri herkesten önce dene',
      nl_cta: 'Anlaşma Simüle Et', nl_hint: '1.5M ₺+ tek anlaşmada ekstra Mega Deal bonusu kazanırsın.',
      ledger_title: 'Sipariş & Anlaşma Defteri', deal_new: 'Yeni Anlaşma', th_customer: 'Müşteri',
      th_amount: 'Tutar', th_rate: 'Oran', th_comm: 'Hakediş', th_status: 'Durum',
      demo_title: 'Demo & Teklif Merkezi', demo_submit: 'Talep Gönder',
      custreg_title: 'Müşteri Kaydet', custreg_submit: 'Müşteriyi Kaydet', myregs_title: 'Kayıtlı Müşterilerim',
      custreg_note: 'Müşteriyi kaydetmezseniz aynı müşteriye başka bir iş ortağı teklif verebilir; müşteri kaybı ve hakediş kaybı yaşanabilir, öncelik hakkınız oluşmaz. Kayıt 30 gün koruma sağlar.',
      ph_custname: 'Müşteri adı', ph_contact: 'Kontak kişisi', ph_contactemail: 'Kontak e-posta', ph_contactphone: 'Kontak telefon', lbl_service: 'Hizmet Türü / Kaynak', lbl_temp: 'Satış Sıcaklığı',
      deal_new_vendor: 'Partner Adına Yeni Anlaşma', deal_book: 'Anlaşmayı Kaydet',
      ph_customer: 'Müşteri adı', ph_dealtitle: 'Anlaşma başlığı (opsiyonel)', ph_amount: 'Tutar (₺)', ph_rate: 'Hakediş %', ph_commission: 'Hakediş (₺)',
      dt_pref: 'Tercih edilen tarih & saat', dt_alt1: 'Alternatif 1 (uygun olunmazsa)', dt_alt2: 'Alternatif 2',
      ph_client: 'Müşteri adı', ph_module: 'Ürün modülü', logout: 'Çıkış Yap' },
    en: { loading: 'Loading Partner Portal', brand_sub: 'Asistan Portal', footer_by: '', nav_dash: 'Dashboard', nav_deals: 'Deals', nav_orders: 'Orders',
      nav_demos: 'Demo Requests', nav_tiers: 'Partner Tiers', nav_support: 'Support', nav_materials: 'Sales Materials', nav_training: 'Training Hub', notif_title: 'Notifications',
      notif_markall: 'Mark all as read', hero_lbl: 'Annual Revenue', sig_title: 'Your Email Signature is Ready', sig_p1: 'Your downloadable email signature is ready.', sig_p2: 'When your partner signature appears in your emails, an additional reward is defined for your target tier; and when the River AI Partner logo is present on your website, an additional target balance is granted. After completing this, you may contact your portfolio manager for verification.', sig_hint: 'Right click the image and choose "Save image as" to download and add it to your email signature.', sig_dl: 'Download Signature',  user_role: 'Partner Admin',
      greet: 'Welcome,', m_base: 'License Sales', m_rental: 'Rental Commission', m_renewal: 'Version Renewal', m_license: 'License/Module Upgrade', m_sla: 'SLA Level', m_act: '12-Month Activity', active: 'Active',
      nl_spark: 'Next Tier', nl_almost: "You're almost at the next tier. Grow your sales to enter the 2027 period at a higher commission level.", nl_remain: 'Remaining revenue:',
      u1_b: 'Higher earnings on every deal', u2_t: 'Dedicated Account Manager', u2_b: 'Strategic SLA and 1-on-1 support line',
      u3_t: 'Market Development Fund (MDF)', u3_b: 'River budget support for your campaigns',
      u4_t: 'Early Access to Beta Features', u4_b: 'Try new modules before anyone else',
      nl_cta: 'Simulate a Deal', nl_hint: 'Earn an extra Mega Deal bonus on any single 1.5M \u20ba+ deal.',
      ledger_title: 'Order & Deal Ledger', deal_new: 'New Deal', th_customer: 'Customer',
      th_amount: 'Amount', th_rate: 'Rate', th_comm: 'Commission', th_status: 'Status',
      demo_title: 'Demo & Quote Center', demo_submit: 'Submit Request',
      custreg_title: 'Register Customer', custreg_submit: 'Register Customer', myregs_title: 'My Registered Customers',
      custreg_note: 'If you do not register a customer, another partner may pitch the same customer; you risk losing the customer and commission, with no priority. Registration protects for 30 days.',
      ph_custname: 'Customer name', ph_contact: 'Contact person', ph_contactemail: 'Contact email', ph_contactphone: 'Contact phone', lbl_service: 'Service Type / Source', lbl_temp: 'Sales Temperature',
      deal_new_vendor: 'New Deal on Behalf of Partner', deal_book: 'Book Deal',
      ph_customer: 'Customer name', ph_dealtitle: 'Deal title (optional)', ph_amount: 'Amount (₺)', ph_rate: 'Commission %', ph_commission: 'Commission (₺)',
      dt_pref: 'Preferred date & time', dt_alt1: 'Alternative 1 (if unavailable)', dt_alt2: 'Alternative 2',
      ph_client: 'Client name', ph_module: 'Product module', logout: 'Sign Out' }
  };

  function setLang(lang) {
    var d = I18N[lang]; if (!d) return;
    document.documentElement.lang = lang;
    var els = document.querySelectorAll('[data-i18n]');
    for (var i = 0; i < els.length; i++) {
      var k = els[i].getAttribute('data-i18n');
      if (d[k] != null) els[i].textContent = d[k];
    }
    var phs = document.querySelectorAll('[data-i18n-ph]');
    for (var j = 0; j < phs.length; j++) {
      var pk = phs[j].getAttribute('data-i18n-ph');
      if (d[pk] != null) phs[j].setAttribute('placeholder', d[pk]);
    }
    var tr = document.getElementById('tr'), en = document.getElementById('en');
    if (tr) tr.className = (lang === 'tr') ? 'on' : '';
    if (en) en.className = (lang === 'en') ? 'on' : '';
    document.cookie = 'ui_culture=' + lang + ';path=/;max-age=31536000';
  }

  var langBtns = document.querySelectorAll('[data-lang]');
  for (var q = 0; q < langBtns.length; q++) {
    (function (btn) {
      btn.addEventListener('click', function () { setLang(btn.getAttribute('data-lang')); });
    })(langBtns[q]);
  }

  // ---- Demo tarih/saat seçici (hafta içi, 09-18, varsayılan sonraki iş günü 10:00) ----
  if (window.flatpickr) {
    var nextBiz = new Date();
    nextBiz.setDate(nextBiz.getDate() + 1);
    while (nextBiz.getDay() === 0 || nextBiz.getDay() === 6) nextBiz.setDate(nextBiz.getDate() + 1);
    nextBiz.setHours(10, 0, 0, 0);

    var fpOpts = {
      enableTime: true, time_24hr: true, dateFormat: 'Y-m-d H:i',
      minTime: '09:00', maxTime: '18:00', minuteIncrement: 30,
      minDate: 'today',
      locale: (INIT === 'tr') ? 'tr' : 'default',
      disable: [function (d) { return d.getDay() === 0 || d.getDay() === 6; }]
    };
    var pref = document.getElementById('preferred');
    if (pref) { var fp = flatpickr(pref, fpOpts); fp.setDate(nextBiz, true); }
    var a1 = document.getElementById('alt1'); if (a1) flatpickr(a1, fpOpts);
    var a2 = document.getElementById('alt2'); if (a2) flatpickr(a2, fpOpts);
  }

  // ---- Satış sıcaklığı kaydırıcı ----
  var tempRange = document.getElementById('tempRange');
  var tempVal = document.getElementById('tempVal');
  if (tempRange && tempVal) {
    var updTemp = function () {
      var v = tempRange.value;
      tempVal.textContent = v;
      tempRange.style.background = 'linear-gradient(90deg, var(--btn) ' + v + '%, rgba(255,255,255,.1) ' + v + '%)';
    };
    tempRange.addEventListener('input', updTemp);
    updTemp();
  }

  // ---- Sıradaki Hedef aç/kapa (ok + başlık tıklanabilir) ----
  var nextlvl = document.getElementById('nextlvl');
  function toggleNl() { if (nextlvl) nextlvl.classList.toggle('collapsed'); }
  var nlToggle = document.getElementById('nlToggle');
  if (nlToggle) nlToggle.addEventListener('click', toggleNl);
  if (nextlvl) {
    var heads = nextlvl.querySelectorAll('.nl-top, .nl-title');
    for (var h = 0; h < heads.length; h++) {
      heads[h].style.cursor = 'pointer';
      heads[h].addEventListener('click', toggleNl);
    }
  }

  setLang(INIT);
})();
