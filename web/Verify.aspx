<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Verify.aspx.cs" Inherits="VerifyPage" %><!DOCTYPE html>
<html lang="tr">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<meta name="robots" content="noindex" />
<title>Sertifika Doğrulama · River Akademi</title>
<link href="https://fonts.googleapis.com/css2?family=Sora:wght@600;700;800&family=Inter:wght@400;500;600&display=swap" rel="stylesheet" />
<link rel="icon" type="image/svg+xml" href="<%= U("~/assets/RIVER_AI_badge.svg") %>" />
<style>
  *{box-sizing:border-box}
  body{margin:0;min-height:100vh;font-family:Inter,Segoe UI,Arial,sans-serif;color:#1A2338;display:flex;flex-direction:column;align-items:center;padding:40px 16px;
       background:radial-gradient(900px 520px at 15% 0%,rgba(79,70,229,.12),transparent 60%),radial-gradient(900px 520px at 90% 100%,rgba(34,184,216,.12),transparent 60%),#F2F5FA}
  .logo{height:34px;margin-bottom:6px}
  .sub{font-size:11px;letter-spacing:.24em;color:#8A97AE;margin-bottom:26px}
  .card{width:100%;max-width:560px;background:#fff;border:1px solid #E3E8F0;border-radius:20px;box-shadow:0 20px 50px rgba(20,30,60,.10);overflow:hidden}
  .hd{padding:22px 28px;border-bottom:1px solid #EEF1F6}
  .hd h1{font-family:Sora,sans-serif;font-size:20px;margin:0}
  .hd p{margin:6px 0 0;font-size:13px;color:#6B7A99}
  .bd{padding:24px 28px}
  form{display:flex;gap:10px}
  input{flex:1;min-width:0;font-size:15px;padding:12px 14px;border:1px solid #D6DEEC;border-radius:11px;letter-spacing:.08em;text-transform:uppercase;font-family:Consolas,Menlo,monospace;outline:none}
  input:focus{border-color:#4F46E5}
  button{background:#4F46E5;color:#fff;border:0;border-radius:11px;padding:12px 20px;font-size:14px;font-weight:600;cursor:pointer;font-family:inherit}
  .res{margin-top:22px;border-radius:14px;padding:18px 20px}
  .res.valid{background:#E7F6EE;border:1px solid #BFE6CE}
  .res.bad{background:#FDECF2;border:1px solid #F6C4D5}
  .res.warn{background:#FBF3E3;border:1px solid #EAD9AE}
  .st{display:flex;align-items:center;gap:10px;font-family:Sora,sans-serif;font-weight:700;font-size:17px}
  .st i{width:30px;height:30px;border-radius:50%;display:flex;align-items:center;justify-content:center;color:#fff;font-style:normal;font-size:16px;flex:0 0 auto}
  .valid .st{color:#0E7A3E}.valid .st i{background:#12A150}
  .bad .st{color:#9B1F4A}.bad .st i{background:#E4467A}
  .warn .st{color:#8A5A08}.warn .st i{background:#D99A1E}
  .rows{margin-top:14px}
  .row{display:flex;justify-content:space-between;gap:14px;padding:8px 0;border-top:1px solid rgba(20,30,60,.07);font-size:13.5px}
  .row span{color:#6B7A99}.row b{color:#14203A;text-align:right}
  .note{font-size:12px;color:#8A97AE;margin-top:14px;line-height:1.6}
  .ft{margin-top:26px;display:flex;align-items:center;gap:8px;font-size:11.5px;color:#9AA6BC}
  .ft img{height:16px}
</style>
</head>
<body>
  <img class="logo" src="<%= U("~/assets/RIVER_AI_black.png") %>" alt="River AI" />
  <div class="sub">RIVER AKADEMİ</div>
  <div class="card">
    <div class="hd"><h1>Sertifika Doğrulama</h1><p>Sertifika üzerindeki QR kodu okutun veya doğrulama kodunu girin.</p></div>
    <div class="bd">
      <form method="get" action="<%= U("~/verify") %>" id="vf">
        <input name="c" value="<%= V(Code) %>" placeholder="Doğrulama kodu" maxlength="20" autocomplete="off" required="required" />
        <button type="submit">Doğrula</button>
      </form>
      <% if (Searched) { %>
        <% if (State == "notfound") { %>
          <div class="res bad"><div class="st"><i>&#10005;</i>Sertifika bulunamadı</div>
            <div class="note">Girilen kodla eşleşen bir sertifika yok. Kodu sertifika üzerindeki haliyle kontrol edin.</div></div>
        <% } else { %>
          <div class="res <%= State == "valid" ? "valid" : (State == "expired" ? "warn" : "bad") %>">
            <div class="st"><i><%= State == "valid" ? "&#10003;" : "!" %></i><%= State == "valid" ? "Geçerli sertifika" : (State == "expired" ? "Sertifikanın süresi dolmuş" : "Bu sertifika iptal edilmiştir") %></div>
            <div class="rows">
              <div class="row"><span>Sahibi</span><b><%= V(C.HolderName) %></b></div>
              <% if (!string.IsNullOrEmpty(C.CompanyName)) { %><div class="row"><span>Firma</span><b><%= V(C.CompanyName) %></b></div><% } %>
              <div class="row"><span>Sertifika</span><b><%= V(C.ExamTitle) %></b></div>
              <div class="row"><span>Veriliş tarihi</span><b><%= IssuedStr %></b></div>
              <div class="row"><span>Geçerlilik</span><b><%= ValidStr %></b></div>
              <div class="row"><span>Sertifika no</span><b><%= V(C.CertNo) %></b></div>
            </div>
          </div>
        <% } %>
      <% } %>
    </div>
  </div>
  <div class="ft"><img src="<%= U("~/assets/fol_dark.png") %>" alt="Fol Yazılım" /> Fol Yazılım A.Ş. · River Asistan Portal</div>
<script>
document.getElementById('vf').addEventListener('submit', function (e) {
  var c = (this.c.value || '').toUpperCase().replace(/[^A-Z0-9]/g, '');
  if (!c) return;
  e.preventDefault();
  location.href = this.getAttribute('action').replace(/\/$/, '') + '/' + encodeURIComponent(c);
});
</script>
</body>
</html>
