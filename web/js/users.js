(function () {
  var dash = document.getElementById('dash');
  var INIT = dash.getAttribute('data-culture') || 'tr';

  var I18N = {
    tr: { brand_sub: 'Asistan Portal', footer_by: '',
      nav_overview: 'Genel Bakış', nav_partners: 'Partnerler', nav_demos: 'Demo Onayları', nav_users: 'Kullanıcılar', nav_materials: 'Satış Materyalleri', nav_support: 'Destek', nav_lms: 'River Akademi',
      user_role: 'Vendor Yöneticisi', logout: 'Çıkış Yap',
      users_h: 'Kullanıcılar', users_sub: 'kullanıcı',
      th_name: 'Ad Soyad', th_email: 'E-posta', th_company: 'Firma', th_role: 'Rol', th_lastlogin: 'Son Giriş' },
    en: { brand_sub: 'Asistan Portal', footer_by: '',
      nav_overview: 'Overview', nav_partners: 'Partners', nav_demos: 'Demo Approvals', nav_users: 'Users', nav_materials: 'Sales Materials', nav_support: 'Support', nav_lms: 'Training Mgmt',
      user_role: 'Vendor Admin', logout: 'Sign Out',
      users_h: 'Users', users_sub: 'users',
      th_name: 'Name', th_email: 'Email', th_company: 'Company', th_role: 'Role', th_lastlogin: 'Last Login' }
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
