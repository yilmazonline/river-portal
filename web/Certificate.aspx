<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Certificate.aspx.cs" Inherits="CertificatePage" %><!DOCTYPE html>
<html lang="tr">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>Sertifika <%= V(C.CertNo) %> · <%= V(C.HolderName) %></title>
<link href="https://fonts.googleapis.com/css2?family=Cormorant+Garamond:ital,wght@0,500;0,600;0,700;1,500;1,600&family=Inter:wght@400;500;600&display=swap" rel="stylesheet" />
<link rel="icon" type="image/svg+xml" href="assets/RIVER_AI_badge.svg" />
<style>
  :root{--ink:#16213E;--ink2:#3C4763;--mut:#7D869C;--gold:<%= P.Accent %>;--gold2:#D6B878;--paper:#FDFBF6}
  *{box-sizing:border-box}
  body{margin:0;background:#E4E7EE;color:var(--ink);font-family:Inter,Segoe UI,Arial,sans-serif;-webkit-print-color-adjust:exact;print-color-adjust:exact}
  .bar{max-width:1123px;margin:18px auto 0;display:flex;gap:10px;justify-content:flex-end;align-items:center;padding:0 10px;flex-wrap:wrap}
  .bar .msg{margin-right:auto;font-size:14px;color:#0E7A3E;font-weight:600}
  .btn{background:var(--ink);color:#fff;border:0;border-radius:10px;padding:11px 22px;font-size:14px;font-weight:600;cursor:pointer;text-decoration:none;font-family:inherit}
  .btn.g{background:#fff;color:#41506E;border:1px solid #D6DEEC}

  .sheet{width:1123px;height:794px;margin:14px auto 30px;position:relative;overflow:hidden;background:var(--paper);box-shadow:0 24px 70px rgba(20,30,60,.22)}
  .sheet::before{content:"";position:absolute;inset:0;background:
      radial-gradient(ellipse at 50% 42%,rgba(255,255,255,.9),transparent 62%),
      radial-gradient(ellipse at 50% 120%,transparent,transparent 55%)}
  /* ince desen */
  .pattern{position:absolute;inset:0;opacity:.35;background-image:repeating-linear-gradient(45deg,rgba(22,33,62,.018) 0 1px,transparent 1px 9px),repeating-linear-gradient(-45deg,rgba(22,33,62,.018) 0 1px,transparent 1px 9px)}
  .b1{position:absolute;inset:26px;border:1.6px solid var(--ink)}
  .b2{position:absolute;inset:34px;border:.8px solid var(--gold)}
  .corner{position:absolute;width:62px;height:62px}
  .corner.tl{top:22px;left:22px}.corner.tr{top:22px;right:22px;transform:scaleX(-1)}
  .corner.bl{bottom:22px;left:22px;transform:scaleY(-1)}.corner.br{bottom:22px;right:22px;transform:scale(-1,-1)}

  .in{position:absolute;inset:74px 110px 60px;display:flex;flex-direction:column;align-items:center;text-align:center}
  .logo{height:30px}
  .acad{margin-top:9px;font-size:10.5px;letter-spacing:.42em;color:var(--mut);font-weight:600;padding-left:.42em}
  .kicker{margin-top:30px;font-size:11px;letter-spacing:.48em;color:var(--gold);font-weight:600;padding-left:.48em}
  h1{font-family:"Cormorant Garamond",Georgia,serif;font-weight:600;font-size:58px;line-height:1;margin:10px 0 0;color:var(--ink);letter-spacing:.01em}
  .orn{display:flex;align-items:center;gap:12px;margin:16px 0 0;color:var(--gold)}
  .orn i{display:block;width:120px;height:1px;background:linear-gradient(90deg,transparent,var(--gold))}
  .orn i:last-child{background:linear-gradient(90deg,var(--gold),transparent)}
  .given{margin-top:20px;font-family:"Cormorant Garamond",Georgia,serif;font-style:italic;font-size:19px;color:var(--ink2)}
  .name{font-family:"Cormorant Garamond",Georgia,serif;font-weight:700;font-size:60px;line-height:1.05;color:var(--ink);margin:30px 0 0;max-width:880px;letter-spacing:.005em}
  .nline{width:520px;max-width:80%;height:1px;margin:12px auto 0;background:linear-gradient(90deg,transparent,var(--gold) 20%,var(--gold) 80%,transparent)}
  .co{margin-top:10px;font-size:11.5px;letter-spacing:.3em;text-transform:uppercase;color:var(--mut);font-weight:600;padding-left:.3em}
  .desc{margin-top:24px;font-family:"Cormorant Garamond",Georgia,serif;font-size:21px;line-height:1.5;color:var(--ink2);max-width:720px}
  .desc b{font-weight:700;color:var(--ink);font-style:normal}

  /* alt blok: tarih | muhur | duzenleyen kurum */
  .foot{position:absolute;left:150px;right:150px;bottom:124px;display:grid;grid-template-columns:1fr 150px 1fr;align-items:end}
  .sig{display:flex;flex-direction:column;align-items:center;text-align:center}
  .sig .v{font-family:"Cormorant Garamond",Georgia,serif;font-size:21px;font-weight:600;color:var(--ink);min-height:34px;display:flex;align-items:flex-end;justify-content:center}
  .sig .v img{height:26px;display:block;margin-bottom:3px}
  .sig .ln{width:230px;height:1px;background:var(--ink);opacity:.55;margin:8px 0 7px}
  .sig .k{font-size:10px;letter-spacing:.3em;text-transform:uppercase;color:var(--mut);font-weight:600;padding-left:.3em}
  .sig .k2{font-size:11px;color:var(--ink2);margin-top:3px;font-weight:500}
  .seal{justify-self:center;width:142px;height:142px;margin-bottom:-26px}

  /* dogrulama seridi */
  .verify{position:absolute;left:62px;right:62px;bottom:46px;display:flex;justify-content:space-between;align-items:flex-end;font-size:10.5px;color:var(--mut)}
  .verify .l{display:flex;align-items:flex-end;gap:10px;text-align:left}
  .verify img{width:62px;height:62px;display:block;background:#fff;padding:3px;border:.8px solid <%= P.Accent %>}
  .verify b{display:block;font-family:Consolas,Menlo,monospace;font-size:12px;color:var(--ink);letter-spacing:.12em;font-weight:600;margin-top:2px}
  .verify .r{text-align:right;line-height:1.6}
  .verify .r b{letter-spacing:.06em}

  .void{position:absolute;inset:0;display:flex;align-items:center;justify-content:center;pointer-events:none;z-index:5}
  .void span{font-family:"Cormorant Garamond",serif;font-size:130px;font-weight:700;color:rgba(190,40,70,.13);transform:rotate(-16deg);letter-spacing:.1em;border:6px solid rgba(190,40,70,.13);padding:0 40px}
  .pv{background:transparent}.pv .bar{display:none}.pv .sheet{margin:0 !important;transform:none !important;box-shadow:none}
  @media print{ body{background:#fff} .bar{display:none} .sheet{margin:0;box-shadow:none} @page{size:A4 landscape;margin:0} }
  @media screen and (max-width:1140px){ .sheet{transform:scale(calc(100vw / 1180));transform-origin:top left;margin-left:10px} }
</style>
</head>
<body class="<%= IsPreview ? "pv" : "" %>">
  <div class="bar">
    <% if (IsNew) { %><span class="msg">Sertifikanız oluşturuldu. Tebrikler!</span><% } %>
    <a class="btn g" href="<%= IsOwner ? "Certificates.aspx" : "Exams.aspx" %>"><%= IsOwner ? "Sertifikalarım" : "Geri" %></a>
    <a class="btn g" href="<%= V(VerifyUrl) %>" target="_blank">Doğrulama Sayfası</a>
    <button class="btn" onclick="window.print()">PDF olarak indir / Yazdır</button>
  </div>
  <div class="sheet">
    <div class="pattern"></div>
    <div class="b1"></div><div class="b2"></div>
    <svg class="corner tl" viewBox="0 0 62 62"><path d="M4 40V4h36" fill="none" stroke="#16213E" stroke-width="1.6"/><path d="M12 30V12h18" fill="none" stroke="<%= P.Accent %>" stroke-width=".9"/><rect x="17" y="17" width="7" height="7" transform="rotate(45 20.5 20.5)" fill="<%= P.Accent %>"/></svg>
    <svg class="corner tr" viewBox="0 0 62 62"><path d="M4 40V4h36" fill="none" stroke="#16213E" stroke-width="1.6"/><path d="M12 30V12h18" fill="none" stroke="<%= P.Accent %>" stroke-width=".9"/><rect x="17" y="17" width="7" height="7" transform="rotate(45 20.5 20.5)" fill="<%= P.Accent %>"/></svg>
    <svg class="corner bl" viewBox="0 0 62 62"><path d="M4 40V4h36" fill="none" stroke="#16213E" stroke-width="1.6"/><path d="M12 30V12h18" fill="none" stroke="<%= P.Accent %>" stroke-width=".9"/><rect x="17" y="17" width="7" height="7" transform="rotate(45 20.5 20.5)" fill="<%= P.Accent %>"/></svg>
    <svg class="corner br" viewBox="0 0 62 62"><path d="M4 40V4h36" fill="none" stroke="#16213E" stroke-width="1.6"/><path d="M12 30V12h18" fill="none" stroke="<%= P.Accent %>" stroke-width=".9"/><rect x="17" y="17" width="7" height="7" transform="rotate(45 20.5 20.5)" fill="<%= P.Accent %>"/></svg>
    <% if (Invalid) { %><div class="void"><span><%= C.IsRevoked ? "GEÇERSİZ" : "SÜRESİ DOLDU" %></span></div><% } %>

    <div class="in">
      <img class="logo" src="assets/RIVER_AI_black.png" alt="River AI" />
      <div class="acad">RIVER AKADEMİ</div>
      <div class="kicker" lang="en">CERTIFICATE OF ACHIEVEMENT</div>
      <h1>Başarı Sertifikası</h1>
      <div class="orn"><i></i><svg width="14" height="14" viewBox="0 0 14 14"><rect x="3" y="3" width="8" height="8" transform="rotate(45 7 7)" fill="<%= P.Accent %>"/></svg><i></i></div>
      <div class="name"><%= V(C.HolderName) %></div>
      <div class="nline"></div>
      <div class="desc"><b>“<%= V(C.ExamTitle) %>”</b> eğitim programı ve sınavını başarıyla tamamlayarak bu belgeyi almaya hak kazanmıştır.</div>
    </div>

    <div class="foot">
      <div class="sig">
        <div class="v"><%= IssuedStr %></div>
        <div class="ln"></div>
        <div class="k">Veriliş Tarihi</div>
        <div class="k2"><%= C.ValidUntil.HasValue ? "Geçerlilik: " + ValidStr : "Süresiz geçerli" %></div>
      </div>
      <svg class="seal" viewBox="0 0 140 140" aria-hidden="true">
        <defs>
          <linearGradient id="gd" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="<%= P.C1 %>"/><stop offset=".42" stop-color="<%= P.C2 %>"/><stop offset=".78" stop-color="<%= P.C2 %>"/><stop offset="1" stop-color="<%= P.C3 %>"/></linearGradient><radialGradient id="gh" cx=".5" cy=".1" r=".85"><stop offset="0" stop-color="#fff" stop-opacity=".16"/><stop offset="1" stop-color="#fff" stop-opacity="0"/></radialGradient><filter id="gf" x="-20%" y="-20%" width="140%" height="140%"><feDropShadow dx="0" dy="1.2" stdDeviation="1.4" flood-color="#1a1206" flood-opacity=".45"/></filter>
          <path id="ring" d="M70 70 m-46 0 a46 46 0 1 1 92 0 a46 46 0 1 1 -92 0"/>
        </defs>
        <polygon fill="url(#gd)" points="<%= SealPoints %>"/><polygon fill="url(#gh)" points="<%= SealPoints %>"/>
        <circle cx="70" cy="70" r="54" fill="none" stroke="#FDFBF6" stroke-width="1.2" opacity=".85"/>
        <circle cx="70" cy="70" r="37" fill="#FDFBF6" opacity=".14"/>
        <circle cx="70" cy="70" r="37" fill="none" stroke="#FDFBF6" stroke-width=".9" opacity=".85"/>
        <text font-family="Inter,Arial,sans-serif" font-size="8" font-weight="600" fill="#FDFBF6"><textPath href="#ring" startOffset="0" textLength="286" lengthAdjust="spacing">RIVER AKADEMİ · ONAYLI SERTİFİKA ·</textPath></text>
        <image href="assets/fol_mark_light.png" x="51" y="46" width="38" height="37.2" filter="url(#gf)" />
        <text x="70" y="96" text-anchor="middle" font-family="Inter,Arial,sans-serif" font-size="5.6" font-weight="600" letter-spacing="1.6" fill="#FDFBF6">AKADEMİ</text>
      </svg>
      <div class="sig">
        <div class="v"><img src="assets/fol_dark.png" alt="Fol Yazılım" /></div>
        <div class="ln"></div>
        <div class="k">Düzenleyen Kurum</div>
        <div class="k2">Fol Yazılım A.Ş.</div>
      </div>
    </div>

    <div class="verify">
      <div class="l"><img id="qrImg" alt="Doğrulama QR kodu" /><div>Doğrulama kodu<b><%= V(C.VerifyCode) %></b><%= V(VerifyHost) %></div></div>
      <div class="r">Sertifika No<br /><b><%= V(C.CertNo) %></b></div>
    </div>
  </div>
<script src="js/qrcode.js"></script>
<script>
(function () {
  try {
    var qr = qrcode(0, 'M');
    qr.addData('<%= VerifyUrl.Replace("'", "") %>');
    qr.make();
    document.getElementById('qrImg').src = qr.createDataURL(4, 1);
  } catch (e) { }
})();
</script>
</body>
</html>
