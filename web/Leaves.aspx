<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Leaves.aspx.cs" Inherits="LeavesPage" ValidateRequest="false" %><!DOCTYPE html>
<html lang="<%= Culture %>">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>River · İzinlerim</title>
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
    <div class="links"><%= Nav.Links("leaves") %></div>
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
  <% if (SetupMsg.Length > 0) { %><div class="flash-err"><%= Server.HtmlEncode(SetupMsg) %></div><% } else { %>
  <div class="page-head lv-head">
    <div><h1>İzinlerim</h1><p>İzin talebi oluşturun, durumunu izleyin ve size gelen talepleri onaylayın. Süre, hafta sonları ve resmi tatiller düşülerek hesaplanır.</p></div>
  </div>
  <div class="lv" id="lv">
    <div class="card lv-main">
      <div class="lv-tabs" role="tablist" aria-label="İzin bölümleri">
        <button role="tab" aria-selected="true" data-tab="new" id="lvt-new">Yeni talep</button>
        <button role="tab" aria-selected="false" data-tab="mine" id="lvt-mine">Taleplerim <span class="lv-count" id="lvcMine" hidden></span></button>
        <button role="tab" aria-selected="false" data-tab="inbox" id="lvt-inbox">Onayımı bekleyenler <span class="lv-count" id="lvcInbox" hidden></span></button>
        <button role="tab" aria-selected="false" data-tab="rules" id="lvt-rules" hidden>Kurallar</button>
      </div>
      <div class="lv-view" data-view="new" role="tabpanel" aria-labelledby="lvt-new">
        <div class="lv-me" id="lvMe"></div>
        <form id="lvForm" novalidate data-noload="1">
          <fieldset><legend>İzin türü</legend><div class="lv-types" id="lvTypes" role="radiogroup" aria-label="İzin türü"></div>
            <label class="lv-f lv-sub" id="lvSubWrap" hidden><span>Mazeret nedeni</span><select class="input" id="lvSub"></select></label></fieldset>
          <fieldset><legend>Tarih ve süre</legend>
            <div id="lvDays"><div class="lv-row"><label class="lv-f"><span>Başlangıç tarihi</span><input class="input" type="date" id="lvStart"></label><label class="lv-f"><span>Bitiş tarihi</span><input class="input" type="date" id="lvEnd"></label></div>
              <div class="lv-row lv-half" id="lvHalf"><label class="lv-chk"><input type="checkbox" id="lvSH"> İlk gün öğleden sonra başlıyorum</label><label class="lv-chk"><input type="checkbox" id="lvEH"> Son gün öğlene kadar izinliyim</label></div></div>
            <div id="lvHours" hidden><div class="lv-row3"><label class="lv-f"><span>Tarih</span><input class="input" type="date" id="lvHD"></label><label class="lv-f"><span>Başlangıç saati</span><input class="input" type="time" id="lvHF" value="09:00" step="900"></label><label class="lv-f"><span>Bitiş saati</span><input class="input" type="time" id="lvHT" value="12:00" step="900"></label></div></div>
            <div class="lv-calc" id="lvCalc"></div><div class="lv-note" id="lvCalcNote"></div><div class="lv-alerts" id="lvWarns" aria-live="polite"></div></fieldset>
          <fieldset id="lvCover"><legend>İş devri</legend><p class="lv-hint">İzinliyken işlerinizi takip edecek kişi. Vekil, talep onaylanınca e-postayla bilgilendirilir.</p>
            <div class="lv-row"><label class="lv-f"><span>Vekil <em id="lvVReq">(zorunlu)</em></span><select class="input" id="lvVekil"></select></label><label class="lv-f" id="lvOcWrap" hidden><span>Nöbet devri <em>(nöbet listesindesiniz)</em></span><select class="input" id="lvOc"></select></label></div>
            <label class="lv-f"><span>Devir notu <em>(açık talepler, bekleyen PR’lar, müşteri görüşmeleri)</em></span><textarea class="input" id="lvHand" maxlength="800"></textarea></label></fieldset>
          <fieldset><legend>İzin süresince iletişim</legend>
            <div class="lv-radios" role="radiogroup" aria-label="Ulaşılabilirlik"><label class="lv-chk"><input type="radio" name="lvReach" value="none" checked> Ulaşılamam</label><label class="lv-chk"><input type="radio" name="lvReach" value="urgent"> Yalnız acil durumda telefonla ulaşılabilirim</label><label class="lv-chk"><input type="radio" name="lvReach" value="mail"> Günde bir kez e-postalarıma bakarım</label></div>
            <div class="lv-row"><label class="lv-f"><span>Acil durum telefonu <em>(isteğe bağlı)</em></span><input class="input" type="tel" id="lvPhone" maxlength="40" autocomplete="off"></label><label class="lv-f"><span>İzin adresi <em>(isteğe bağlı)</em></span><input class="input" type="text" id="lvAddr" maxlength="200" autocomplete="off"></label></div></fieldset>
          <fieldset><legend>Açıklama ve belge</legend>
            <label class="lv-f"><span>Açıklama <em id="lvNReq">(isteğe bağlı)</em></span><textarea class="input" id="lvNote" maxlength="600"></textarea></label>
            <label class="lv-f"><span>Belge <em id="lvFReq">(isteğe bağlı)</em></span><input type="file" id="lvFile" accept=".pdf,.jpg,.jpeg,.png"></label>
            <label class="lv-chk"><input type="checkbox" id="lvOoo" checked> İzin başlamadan bir gün önce bana Outlook otomatik yanıt hatırlatması gönder</label>
            <label class="lv-chk"><input type="checkbox" id="lvOk"> Bilgilerin doğru olduğunu ve izin süresince işlerimin devredildiğini onaylıyorum</label></fieldset>
          <fieldset><legend>Bu talep kime gidecek?</legend><ol class="lv-route" id="lvRoutePrev"></ol></fieldset>
          <div class="lv-alerts" id="lvErrs" aria-live="assertive"></div>
          <div class="lv-acts"><button class="btn-cyan" type="submit" id="lvSubmit">Talebi gönder</button><button class="btn-ghost" type="button" id="lvReset">Formu temizle</button></div>
        </form>
      </div>
      <div class="lv-view" data-view="mine" role="tabpanel" aria-labelledby="lvt-mine" hidden><div class="lv-items" id="lvMine"></div></div>
      <div class="lv-view" data-view="inbox" role="tabpanel" aria-labelledby="lvt-inbox" hidden><div class="lv-items" id="lvInbox"></div></div>
      <div class="lv-view" data-view="rules" role="tabpanel" aria-labelledby="lvt-rules" hidden><div id="lvRules"></div></div>
    </div>
    <aside class="card lv-side" aria-labelledby="lvFlowT"><h2 id="lvFlowT">Talebin akışı</h2><div class="lv-sub2" id="lvFlowSub">Bir talep seçtiğinizde ya da gönderdiğinizde akışı burada izlenir.</div><ol class="lv-route" id="lvRouteLive"></ol><ul class="lv-log" id="lvLog" hidden></ul></aside>
  </div>
  <div class="lv-toast" id="lvToast" role="status" aria-live="polite" hidden></div>
  <% } %>
</div>
</div>
<script src="js/leaves.js?v=2"></script>
<script src="js/common.js?v=185"></script>
</body>
</html>
