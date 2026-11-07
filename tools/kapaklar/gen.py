import json, re, os, sys
P=[ # dosya adi, baslik, aciklama (kisaltmalarda acilim), ikon, renk1, renk2
 ("River AI Bakım Ürün Özellikleri","Bakım","Bakım Yönetimi","wrench","#F59E0B","#EF4444"),
 ("River AI Banka Ürün Özellikleri","Banka","Banka Yönetimi","landmark","#0EA5E9","#2563EB"),
 ("River AI BudgetAI Ürün Özellikleri","BudgetAI","Bütçe Yönetimi","chart-pie","#22C55E","#0EA5E9"),
 ("River AI CRM Ürün Özellikleri","CRM","Müşteri İlişkileri Yönetimi","handshake","#EC4899","#8B5CF6"),
 ("River AI EBYS Ürün Özellikleri","EBYS","Elektronik Belge Yönetim Sistemi","folder-open","#6366F1","#3B82F6"),
 ("River AI E-Fatura Ürün Özellikleri","E-Fatura","E-Fatura Yönetimi","receipt","#14B8A6","#22C55E"),
 ("River AI ITSM Ürün Özellikleri","ITSM","BT Hizmet Yönetimi","server-cog","#3B82F6","#06B6D4"),
 ("River AI KYS Ürün Özellikleri","KYS","Kalite Yönetim Sistemi","badge-check","#10B981","#84CC16"),
 ("River AI Robotlar Ürün Özellikleri","Robotlar","Yazılım Robotları","bot","#A855F7","#EC4899"),
 ("River AI Sandık Ürün Özellikleri","Sandık","Sandık Yönetimi","piggy-bank","#EAB308","#F97316"),
 ("River AI Satın Alma Ürün Özellikleri","Satın Alma","Satın Alma Yönetimi","shopping-cart","#F97316","#EC4899"),
 ("River AI Sözleşme Yönetimi Ürün Özellikleri","Sözleşme Yönetimi","","file-pen-line","#8B5CF6","#6366F1"),
 ("River AI SSH Ürün Özellikleri","SSH","Satış Sonrası Hizmetler","headset","#06B6D4","#10B981"),
 ("River AI Süreç Modelleme Ürün Özellikleri","Süreç Modelleme","","workflow","#C019E6","#6A3CF0"),
 ("River AI Ticket Yönetimi Ürün Özellikleri","Ticket Yönetimi","","ticket","#EF4444","#F97316"),
 ("River AI Üretim Ürün Özellikleri","Üretim","Üretim Yönetimi","factory","#84CC16","#14B8A6"),
 ("River AI WMS (Depo Yönetimi) Ürün Özellikleri","WMS","Depo Yönetim Sistemi","warehouse","#2563EB","#7C3AED"),
]
def icon(n):
    s=open(f'node_modules/lucide-static/icons/{n}.svg').read()
    s=re.sub(r'<!--.*?-->','',s,flags=re.S)
    return s.replace('stroke-width="2"','stroke-width="1.35"').replace('width="24"','width="100%"').replace('height="24"','height="100%"').replace('stroke="currentColor"','stroke="#fff"')
FONTS=''.join(f'<link rel="stylesheet" href="node_modules/@fontsource/{f}.css">' for f in ['sora/600','sora/700','sora/800','inter/400','inter/500','inter/600'])
CSS='''
@page{size:A4;margin:0}
*{box-sizing:border-box;margin:0;padding:0}
html,body{width:210mm;height:297mm}
body{font-family:Inter,sans-serif;-webkit-print-color-adjust:exact;print-color-adjust:exact}
.pg{position:relative;width:210mm;height:297mm;overflow:hidden;background:#fff}
/* ust koyu bolum (capraz kesimli) */
.hero{position:absolute;inset:0 0 auto 0;height:226mm;background:#090B18;clip-path:polygon(0 0,100% 0,100% 88%,0 100%);overflow:hidden}
.glow{position:absolute;width:230mm;height:230mm;right:-80mm;top:-20mm;border-radius:50%;background:radial-gradient(circle closest-side,color-mix(in srgb,var(--c1) 55%,transparent) 0%,color-mix(in srgb,var(--c1) 18%,transparent) 45%,transparent 100%)}
.glow2{position:absolute;width:200mm;height:200mm;left:-90mm;bottom:-70mm;border-radius:50%;background:radial-gradient(circle closest-side,color-mix(in srgb,var(--c2) 40%,transparent) 0%,transparent 100%)}
.grid{position:absolute;inset:0;background-image:radial-gradient(rgba(255,255,255,.10) .32mm,transparent .4mm);background-size:7mm 7mm;mask-image:linear-gradient(180deg,rgba(0,0,0,.9),rgba(0,0,0,.15) 80%);-webkit-mask-image:linear-gradient(180deg,rgba(0,0,0,.9),rgba(0,0,0,.15) 80%)}
.rings{position:absolute;right:14mm;top:30mm;width:126mm;height:126mm}
.rings i{position:absolute;inset:0;border-radius:50%;border:.3mm solid rgba(255,255,255,.08)}
.rings i:nth-child(2){inset:-16mm;border-color:rgba(255,255,255,.055)}
.rings i:nth-child(3){inset:-34mm;border-color:rgba(255,255,255,.035)}
.top{position:absolute;left:18mm;right:18mm;top:16mm;display:flex;justify-content:space-between;align-items:center}
.top img{height:9mm}
.chip{font:600 8.4pt Inter;letter-spacing:.18em;color:#fff;border:.3mm solid rgba(255,255,255,.28);border-radius:99mm;padding:2.2mm 4.6mm;background:rgba(255,255,255,.06)}
.tile{position:absolute;right:36mm;top:52mm;width:84mm;height:84mm;border-radius:20mm;background:linear-gradient(140deg,var(--c1),var(--c2));
  box-shadow:0 18mm 30mm -12mm color-mix(in srgb,var(--c1) 70%,transparent),inset 0 .6mm 0 rgba(255,255,255,.35);display:grid;place-items:center;transform:rotate(-8deg)}
.tile::after{content:"";position:absolute;inset:0;border-radius:20mm;background:linear-gradient(160deg,rgba(255,255,255,.28),transparent 46%)}
.tile .ic{width:44mm;height:44mm;position:relative;z-index:1}
.mini{position:absolute;border-radius:5mm;background:#151a2e;border:.3mm solid rgba(255,255,255,.14);padding:4mm 5mm;box-shadow:0 6mm 14mm -6mm rgba(0,0,0,.6)}
.mini b{display:block;height:2mm;border-radius:2mm;background:rgba(255,255,255,.55);margin-bottom:2.2mm}
.mini b:last-child{margin:0;background:rgba(255,255,255,.25)}
.m1{right:14mm;top:126mm;width:46mm}.m1 b:first-child{width:60%;background:linear-gradient(90deg,var(--c1),var(--c2))}.m1 b:nth-child(2){width:90%}.m1 b:nth-child(3){width:70%}
.m2{right:116mm;top:46mm;width:34mm}.m2 b:first-child{width:80%}.m2 b:nth-child(2){width:55%}
.txt{position:absolute;left:18mm;right:18mm;bottom:26mm}
.eb{font:700 9.5pt Sora;letter-spacing:.32em;color:var(--c1);margin-bottom:5mm}
h1{font:800 var(--fs,46pt)/1.02 Sora;color:#fff;letter-spacing:-.02em;max-width:150mm}
.sub{font:500 15pt/1.35 Inter;color:rgba(255,255,255,.72);margin-top:5mm;max-width:150mm}
.bar{width:26mm;height:1.6mm;border-radius:2mm;background:linear-gradient(90deg,var(--c1),var(--c2));margin-top:8mm}
/* alt acik bolum */
.foot{position:absolute;left:18mm;right:18mm;bottom:16mm}
.doc{font:700 20pt Sora;color:#0B1020;letter-spacing:-.01em}
.doc span{color:var(--c1)}
.desc{font:400 10.5pt/1.55 Inter;color:#5A6478;margin-top:2.5mm;max-width:150mm}
.sign{display:flex;justify-content:space-between;align-items:flex-end;margin-top:13mm;padding-top:6mm;border-top:.3mm solid #E3E6EE}
.sign img{height:8mm}
.sign div{text-align:right;font:500 9pt/1.5 Inter;color:#7A8296}
.sign div b{display:block;font:600 10pt Sora;color:#0B1020}
'''
def page(p):
    fn,title,sub,ic,c1,c2=p
    fs = '46pt' if len(title)<=10 else ('40pt' if len(title)<=16 else '36pt')
    return f'''<div class="pg" style="--c1:{c1};--c2:{c2};--fs:{fs}">
<div class="hero"><div class="glow"></div><div class="glow2"></div><div class="grid"></div><div class="rings"><i></i><i></i><i></i></div>
<div class="top"><img src="RIVER_AI_white.svg" alt=""><span class="chip">ÜRÜN ÖZELLİKLERİ</span></div>
<div class="mini m2"><b></b><b></b></div>
<div class="tile"><div class="ic">{icon(ic)}</div></div>
<div class="mini m1"><b></b><b></b><b></b></div>
<div class="txt"><div class="eb">ÜRÜN</div><h1>{title}</h1>{f'<div class="sub">{sub}</div>' if sub else ''}<div class="bar"></div></div></div>
<div class="foot"><div class="doc">Ürün <span>Özellikleri</span></div>
<div class="desc">River AI Platform üzerinde çalışan {sub or title} ürününün temel özelliklerini ve iş süreçlerinize sağladığı katkıyı özetler.</div>
<div class="sign"><img src="fol_dark.png" alt=""><div><b>Fol Yazılım A.Ş.</b>river.com.tr</div></div></div></div>'''
which = sys.argv[1:] or [p[0] for p in P]
for p in P:
    if p[0] not in which and p[1] not in which: continue
    open(f'out_{P.index(p):02d}.html','w',encoding='utf-8').write(f'<!doctype html><html lang="tr"><head><meta charset="utf-8">{FONTS}<style>{CSS}</style></head><body>{page(p)}</body></html>')
json.dump(P,open('products.json','w',encoding='utf-8'),ensure_ascii=False)
print('ok', len(which))
