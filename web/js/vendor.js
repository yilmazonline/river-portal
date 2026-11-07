(function () {
  var dash = document.getElementById('dash');
  var INIT = dash.getAttribute('data-culture') || 'tr';

  // ---- Impersonation ("Adına Görüntüle") ----
  var impBtns = document.querySelectorAll('[data-imp]');
  for (var i = 0; i < impBtns.length; i++) {
    (function (btn) {
      btn.addEventListener('click', function () {
        var id = btn.getAttribute('data-imp');
        document.cookie = 'imp_company=' + id + ';path=/;max-age=3600';
        window.location.href = 'Partner.aspx';
      });
    })(impBtns[i]);
  }

  var I18N = {
    tr: { nav_overview: 'Genel Bakış', nav_partners: 'Partnerler', nav_demos: 'Demo Onayları', nav_support: 'Destek', nav_materials: 'Satış Materyalleri', nav_lms: 'River Akademi', nav_users: 'Kullanıcılar', alldemos_title: 'Tüm Demolar (geçmiş dahil)',
      brand_sub: 'Asistan Portal', footer_by: '',
      act_new_partner: '+ Bayi Oluştur', act_new_user: '+ Kullanıcı Oluştur',
      user_role: 'Vendor Yöneticisi', kpi_partners: 'Toplam Partner', kpi_active: 'Aktif Partner',
      kpi_revenue: 'Toplam Ciro', kpi_demos: 'Bekleyen Demo', matrix_title: 'Partner Matrisi',
      th_company: 'Firma', th_tier: 'Kademe', th_revenue: 'Ciro', th_state: 'Durum',
      queue_title: 'Demo Onay Kuyruğu', regqueue_title: 'Müşteri Kayıt Onayları', dash_title: 'Dashboard', ch_monthly: 'Aylık Anlaşma & Ciro (M ₺)', ch_tiers: 'Kademe Dağılımı', ch_top: 'En Yüksek Cirolu Bayiler (M ₺)', ch_deal: 'Sipariş Durumları', ch_demo: 'Demo Durumları', logout: 'Çıkış Yap' },
    en: { nav_overview: 'Overview', nav_partners: 'Partners', nav_demos: 'Demo Approvals', nav_support: 'Support', nav_materials: 'Sales Materials', nav_lms: 'Training Mgmt', nav_users: 'Users', alldemos_title: 'All Demos (incl. past)',
      brand_sub: 'Asistan Portal', footer_by: '',
      act_new_partner: '+ New Partner', act_new_user: '+ New User',
      user_role: 'Vendor Admin', kpi_partners: 'Total Partners', kpi_active: 'Active Partners',
      kpi_revenue: 'Total Revenue', kpi_demos: 'Pending Demos', matrix_title: 'Partner Matrix',
      th_company: 'Company', th_tier: 'Tier', th_revenue: 'Revenue', th_state: 'Status',
      queue_title: 'Demo Approval Queue', regqueue_title: 'Customer Registration Approvals', dash_title: 'Dashboard', ch_monthly: 'Monthly Deals & Revenue (M ₺)', ch_tiers: 'Tier Distribution', ch_top: 'Top Partners by Revenue (M ₺)', ch_deal: 'Order Status', ch_demo: 'Demo Status', logout: 'Sign Out' }
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

  // ---- Dashboard grafikleri (Chart.js) · ilk açılışta render edilir ----
  var _charts = [];
  var _charted = false;
  function renderCharts() {
    if (_charted || !window.Chart || !window.VDATA) return;
    _charted = true;
    var V = window.VDATA;
    try { Chart.defaults.color = '#93A3C4'; } catch (ex) {}
    var grid = 'rgba(148,163,196,.15)';
    function mk(id, cfg) { var el = document.getElementById(id); if (el && el.getContext) { try { _charts.push(new Chart(el, cfg)); } catch (ex) {} } }
    var legBottom = { legend: { position: 'bottom', labels: { boxWidth: 12, padding: 12 } } };

    if (V.monthly) mk('chMonthly', {
      data: { labels: V.monthly.labels, datasets: [
        { type: 'bar', label: 'Anlaşma', data: V.monthly.deals, backgroundColor: '#1F56FF', borderRadius: 6, yAxisID: 'y' },
        { type: 'line', label: 'Ciro (M₺)', data: V.monthly.revenue, borderColor: '#C019E6', backgroundColor: '#C019E6', tension: 0.35, yAxisID: 'y1' }
      ] },
      options: { responsive: true, maintainAspectRatio: false,
        scales: { y: { grid: { color: grid }, beginAtZero: true }, y1: { position: 'right', grid: { display: false }, beginAtZero: true }, x: { grid: { display: false } } },
        plugins: { legend: { labels: { boxWidth: 12 } } } }
    });
    if (V.tiers) mk('chTiers', {
      type: 'doughnut',
      data: { labels: V.tiers.labels, datasets: [{ data: V.tiers.data, backgroundColor: ['#8391A8', '#C7CDD6', '#E7A438', '#C019E6'], borderWidth: 0 }] },
      options: { responsive: true, maintainAspectRatio: false, cutout: '62%', plugins: legBottom }
    });
    if (V.top) mk('chTop', {
      type: 'bar',
      data: { labels: V.top.labels, datasets: [{ data: V.top.data, backgroundColor: '#00AFFF', borderRadius: 6 }] },
      options: { indexAxis: 'y', responsive: true, maintainAspectRatio: false,
        scales: { x: { grid: { color: grid }, beginAtZero: true }, y: { grid: { display: false } } }, plugins: { legend: { display: false } } }
    });
    if (V.dealStatus) mk('chDeal', {
      type: 'doughnut',
      data: { labels: V.dealStatus.labels, datasets: [{ data: V.dealStatus.data, backgroundColor: ['#FFB020', '#34E1B5', '#1F56FF', '#FF3D71'], borderWidth: 0 }] },
      options: { responsive: true, maintainAspectRatio: false, cutout: '62%', plugins: legBottom }
    });
    if (V.demoStatus) mk('chDemo', {
      type: 'bar',
      data: { labels: V.demoStatus.labels, datasets: [{ data: V.demoStatus.data, backgroundColor: '#34E1B5', borderRadius: 6 }] },
      options: { responsive: true, maintainAspectRatio: false, scales: { y: { grid: { color: grid }, beginAtZero: true }, x: { grid: { display: false } } }, plugins: { legend: { display: false } } }
    });
  }

  // Dashboard aç/kapa (varsayılan kapalı)
  var dashSection = document.getElementById('dashSection');
  var dashToggle = document.getElementById('dashToggle');
  if (dashToggle && dashSection) {
    dashToggle.addEventListener('click', function () {
      dashSection.classList.toggle('collapsed');
      if (dashSection.className.indexOf('collapsed') < 0) {
        renderCharts();
        setTimeout(function () { for (var i = 0; i < _charts.length; i++) { try { _charts[i].resize(); } catch (e) {} } }, 80);
      }
    });
  }

  setLang(INIT);
})();
