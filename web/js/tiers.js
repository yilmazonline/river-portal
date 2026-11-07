(function () {
  var dash = document.getElementById('dash');
  var INIT = dash.getAttribute('data-culture') || 'tr';

  var I18N = {
    tr: { nav_dash: 'Panel', nav_orders: 'Siparişler', nav_demos: 'Demo Talepleri',
      nav_tiers: 'Partner Seviyeleri', nav_materials: 'Satış Materyalleri', nav_support: 'Destek', nav_training: 'River Akademi', user_role: 'Partner Yöneticisi',
      brand_sub: 'Asistan Portal', footer_by: '', logout: 'Çıkış Yap',
      tiers_h: 'Partner Seviyeleri',
      tiers_sub: 'Her seviyenin komisyon oranları, avantajları ve şartları tablodadır. Ciro arttıkça seviyeniz otomatik yükselir.',
      back_dash: '← Panele dön' },
    en: { nav_dash: 'Dashboard', nav_orders: 'Orders', nav_demos: 'Demo Requests',
      nav_tiers: 'Partner Tiers', nav_materials: 'Sales Materials', nav_support: 'Support', nav_training: 'Training Hub', user_role: 'Partner Admin',
      brand_sub: 'Asistan Portal', footer_by: '', logout: 'Sign Out',
      tiers_h: 'Partner Tiers',
      tiers_sub: "Each tier's commission rates, benefits and requirements. Your tier rises automatically as revenue grows.",
      back_dash: '← Back to dashboard' }
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
