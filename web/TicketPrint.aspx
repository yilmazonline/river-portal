<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TicketPrint.aspx.cs" Inherits="TicketPrintPage" %><!DOCTYPE html>
<html lang="tr">
<head>
<meta charset="utf-8" />
<script src="<%= ResolveUrl("~/js/i18n.js") %>?v=51"></script>
<meta name="viewport" content="width=device-width, initial-scale=1.0" />
<title>Talep <%= Server.HtmlEncode(T.TicketNo) %></title>
<style>
  *{box-sizing:border-box}
  body{margin:0;background:#F2F5FA;color:#1A2338;font-family:Segoe UI,Arial,sans-serif;-webkit-print-color-adjust:exact;print-color-adjust:exact}
  .sheet{max-width:820px;margin:24px auto;background:#fff;border:1px solid #E2E8F2;border-radius:14px;overflow:hidden}
  .top{display:flex;justify-content:space-between;align-items:flex-start;padding:26px 32px;border-bottom:3px solid #4F46E5;gap:16px}
  .top h1{margin:0;font-size:22px}.top .no{color:#6B7A99;font-size:13px;margin-top:4px}
  .brand{text-align:right}
  .brand small{display:block;font-size:11px;color:#8A97AE;font-weight:600;letter-spacing:.14em;text-transform:uppercase;margin-top:5px}
  .meta{display:grid;grid-template-columns:1fr 1fr;gap:2px 26px;padding:20px 32px;background:#F7F9FD}
  .meta .r{display:flex;justify-content:space-between;gap:12px;padding:7px 0;border-bottom:1px solid #EDF1F7;font-size:13px}
  .meta .r .k{color:#6B7A99}.meta .r .v{font-weight:600;text-align:right}
  .sec{padding:18px 32px}
  .sec h2{font-size:12px;text-transform:uppercase;letter-spacing:.08em;color:#8A97AE;margin:0 0 12px}
  .msg{border:1px solid #E7ECF3;border-radius:10px;padding:12px 14px;margin-bottom:10px;background:#FBFCFE}
  .msg .mh{font-size:12px;color:#6B7A99;margin-bottom:6px}
  .msg .mb{font-size:13.5px;line-height:1.6}
  .foot{padding:16px 32px;border-top:1px solid #EDF1F7;color:#9AA6BC;font-size:11px;text-align:center}
  .bar{max-width:820px;margin:16px auto 0;display:flex;gap:10px;justify-content:flex-end;padding:0 6px}
  .btn{background:#4F46E5;color:#fff;border:0;border-radius:10px;padding:11px 22px;font-size:14px;font-weight:600;cursor:pointer;text-decoration:none}
  .btn.g{background:#fff;color:#41506E;border:1px solid #D6DEEC}
  @media print{ body{background:#fff} .bar{display:none} .sheet{border:0;margin:0;max-width:none} @page{margin:14mm} }
</style>
</head>
<body>
  <div class="bar">
    <a class="btn g" href="TicketDetail.aspx?id=<%= T.TicketId %>">Kapat</a>
    <button class="btn" onclick="window.print()">PDF olarak indir / Yazdır</button>
  </div>
  <div class="sheet">
    <div class="top">
      <div><h1>Destek Talebi</h1><div class="no"><%= Server.HtmlEncode(T.TicketNo) %></div></div>
      <div class="brand"><img src="assets/RIVER_AI_black.png" alt="River AI" style="height:34px;display:block" /><small>Asistan Portal</small></div>
    </div>
    <div class="meta">
      <div class="r"><span class="k">Firma</span><span class="v"><%= Fld(T.CompanyName) %></span></div>
      <div class="r"><span class="k">Durum</span><span class="v"><%= Fld(T.Status) %></span></div>
      <div class="r"><span class="k">Konu</span><span class="v"><%= Fld(T.Subject) %></span></div>
      <div class="r"><span class="k">Ürün / Modül</span><span class="v"><%= Fld(T.Product) %> · <%= Fld(T.Module) %></span></div>
      <div class="r"><span class="k">Tip</span><span class="v"><%= Fld(T.RequestType) %></span></div>
      <div class="r"><span class="k">Öncelik</span><span class="v"><%= Fld(T.Priority) %></span></div>
      <div class="r"><span class="k">Açan</span><span class="v"><%= Fld(T.OpenedByName) %></span></div>
      <div class="r"><span class="k">Atanan</span><span class="v"><%= Fld(T.AssignedToName) %></span></div>
      <div class="r"><span class="k">Oluşturulma</span><span class="v"><%= Dt(T.CreatedAt) %></span></div>
      <div class="r"><span class="k">Harcanan Süre</span><span class="v"><%= T.ResolutionMinutes.HasValue ? T.ResolutionMinutes.Value + " dk" : "—" %></span></div>
    </div>
    <div class="sec">
      <h2>Yazışma</h2>
      <%= Body.Length > 0 ? Body : "<div style='color:#9AA6BC;font-size:13px'>Görüntülenecek mesaj yok.</div>" %>
    </div>
    <div class="foot">Bu belge River Asistan Portal üzerinden oluşturulmuştur · <%= DateTime.Now.ToTrLocal().ToString("dd.MM.yyyy HH:mm") %></div>
  </div>
  <script>window.addEventListener('load', function(){ setTimeout(function(){ try{ window.print(); }catch(e){} }, 350); });</script>
</body>
</html>
