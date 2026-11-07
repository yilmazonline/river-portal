(function () {
  var dash = document.getElementById('dash');
  var INIT = dash.getAttribute('data-culture') || 'tr';

  // Durum select degisince otomatik gonder
  var selects = document.querySelectorAll('select[data-auto]');
  for (var i = 0; i < selects.length; i++) {
    selects[i].addEventListener('change', function () { this.form.submit(); });
  }

  // Silme onayi
  var delBtns = document.querySelectorAll('.del-btn');
  var confirmMsg = (INIT === 'en') ? 'Delete this order? This cannot be undone.'
                                    : 'Bu sipariş silinsin mi? Bu işlem geri alınamaz.';
  for (var j = 0; j < delBtns.length; j++) {
    delBtns[j].addEventListener('click', function (e) {
      if (!window.confirm(confirmMsg)) e.preventDefault();
    });
  }

  var I18N = {
    tr: { brand_sub: 'Asistan Portal', footer_by: '',
      nav_dash: 'Panel', nav_orders: 'Siparişler', nav_demos: 'Demo Talepleri',
      nav_tiers: 'Partner Seviyeleri', nav_materials: 'Satış Materyalleri', nav_support: 'Destek', nav_training: 'River Akademi', user_role: 'Partner Yöneticisi',
      logout: 'Çıkış Yap', orders_h: 'Siparişler',
      th_customer: 'Müşteri Adı', th_title: 'Başlık', th_amount: 'Tutar', th_rate: 'Oran',
      th_comm: 'Hakediş', th_date: 'Tarih', th_status: 'Durum', th_actions: 'İşlemler' },
    en: { brand_sub: 'Asistan Portal', footer_by: '',
      nav_dash: 'Dashboard', nav_orders: 'Orders', nav_demos: 'Demo Requests',
      nav_tiers: 'Partner Tiers', nav_materials: 'Sales Materials', nav_support: 'Support', nav_training: 'Training Hub', user_role: 'Partner Admin',
      logout: 'Sign Out', orders_h: 'Orders',
      th_customer: 'Customer', th_title: 'Title', th_amount: 'Amount', th_rate: 'Rate',
      th_comm: 'Commission', th_date: 'Date', th_status: 'Status', th_actions: 'Actions' }
  };

  function setLang(lang) {
    var d = I18N[lang]; if (!d) return;
    document.documentElement.lang = lang;
    var els = document.querySelectorAll('[data-i18n]');
    for (var i = 0; i < els.length; i++) {
      var k = els[i].getAttribute('data-i18n');
      if (d[k] != null) els[i].textContent = d[k];
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

  setLang(INIT);
})();
