(function () {
  var dash = document.getElementById('dash');
  var INIT = dash.getAttribute('data-culture') || 'tr';

  var I18N = {
    tr: { brand_sub: 'Asistan Portal', footer_by: '',
      nav_dash: 'Panel', nav_orders: 'Siparişler', nav_demos: 'Demo Talepleri',
      nav_tiers: 'Partner Seviyeleri', nav_materials: 'Satış Materyalleri', nav_support: 'Destek', nav_training: 'River Akademi', user_role: 'Partner Yöneticisi',
      logout: 'Çıkış Yap', demos_h: 'Demo Talepleri',
      th_client: 'Müşteri', th_module: 'Modül', th_prefdates: 'Tercih / Alternatifler',
      th_scheduled: 'Planlanan', th_status: 'Durum' },
    en: { brand_sub: 'Asistan Portal', footer_by: '',
      nav_dash: 'Dashboard', nav_orders: 'Orders', nav_demos: 'Demo Requests',
      nav_tiers: 'Partner Tiers', nav_materials: 'Sales Materials', nav_support: 'Support', nav_training: 'Training Hub', user_role: 'Partner Admin',
      logout: 'Sign Out', demos_h: 'Demo Requests',
      th_client: 'Customer', th_module: 'Module', th_prefdates: 'Preferred / Alternatives',
      th_scheduled: 'Scheduled', th_status: 'Status' }
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
