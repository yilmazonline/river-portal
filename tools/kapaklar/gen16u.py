import json, re, os, sys
# (dosya adi, baslik, alt baslik, ust etiket (kategori), tur etiketi, ikon, renk1, renk2)
D=json.load(open(sys.argv[1],encoding='utf-8')) if len(sys.argv)>1 else [
 ["river-ai-kurumsal-tanitim","River AI Kurumsal Tanıtım","İş süreçlerinde yapay zekâ çağı","TANITIM","DOKÜMAN","building-2","#C019E6","#6A3CF0"],
 ["river-ai-ekosistem-sunumu","River AI Ekosistem Sunumu","DMS, QMS, EBYS, İntranet ve sözleşme yönetimi","SUNUMLAR","SUNUM","blocks","#3B82F6","#06B6D4"],
 ["dokuman-yonetimi-sistemi","Doküman Yönetimi Sistemi","Doküman yaşam döngüsü, onay akışları ve e-imza","DOKÜMAN YÖNETİMİ","DOKÜMAN","folder-open","#6366F1","#3B82F6"],
]
def icon(n):
    s=re.sub(r'<!--.*?-->','',open(f'node_modules/lucide-static/icons/{n}.svg').read(),flags=re.S)
    return s.replace('stroke-width="2"','stroke-width="1.35"').replace('width="24"','width="100%"').replace('height="24"','height="100%"').replace('stroke="currentColor"','stroke="#fff"')
FONTS=''.join(f'<link rel="stylesheet" href="node_modules/@fontsource/{f}.css">' for f in ['sora/600','sora/700','sora/800','inter/400','inter/500','inter/600'])
CSS='''
*{box-sizing:border-box;margin:0;padding:0}
html,body{width:1920px;height:1080px}
.pg{position:relative;width:1920px;height:1080px;overflow:hidden;background:#fff;font-family:Inter,sans-serif}
.hero{position:absolute;inset:0;background:#090B18;clip-path:polygon(0 0,100% 0,100% 82%,0 92%);overflow:hidden}
.glow{position:absolute;width:1500px;height:1500px;right:-520px;top:-560px;border-radius:50%;background:radial-gradient(circle closest-side,color-mix(in srgb,var(--c1) 50%,transparent) 0%,color-mix(in srgb,var(--c1) 16%,transparent) 45%,transparent 100%)}
.glow2{position:absolute;width:1100px;height:1100px;left:-520px;bottom:-560px;border-radius:50%;background:radial-gradient(circle closest-side,color-mix(in srgb,var(--c2) 38%,transparent) 0%,transparent 100%)}
.grid{position:absolute;inset:0;background-image:radial-gradient(rgba(255,255,255,.10) 1.4px,transparent 1.8px);background-size:30px 30px;-webkit-mask-image:linear-gradient(90deg,rgba(0,0,0,.25),rgba(0,0,0,.9))}
.rings{position:absolute;right:250px;top:170px;width:520px;height:520px}
.rings i{position:absolute;inset:0;border-radius:50%;border:1.5px solid rgba(255,255,255,.08)}
.rings i:nth-child(2){inset:-70px;border-color:rgba(255,255,255,.055)}.rings i:nth-child(3){inset:-150px;border-color:rgba(255,255,255,.035)}
.top{position:absolute;left:96px;right:96px;top:72px;display:flex;justify-content:space-between;align-items:center}
.top img{height:46px}
.chip{font:600 22px Inter;letter-spacing:.18em;color:#fff;border:1.5px solid rgba(255,255,255,.28);border-radius:99px;padding:11px 24px;background:rgba(255,255,255,.06)}
.tile{position:absolute;right:320px;top:250px;width:380px;height:380px;border-radius:92px;background:linear-gradient(140deg,var(--c1),var(--c2));
  box-shadow:0 80px 130px -50px color-mix(in srgb,var(--c1) 70%,transparent),inset 0 3px 0 rgba(255,255,255,.35);display:grid;place-items:center;transform:rotate(-8deg)}
.tile::after{content:"";position:absolute;inset:0;border-radius:92px;background:linear-gradient(160deg,rgba(255,255,255,.28),transparent 46%)}
.tile .ic{width:200px;height:200px;position:relative;z-index:1}
.mini{position:absolute;border-radius:22px;background:#151a2e;border:1.5px solid rgba(255,255,255,.14);padding:20px 24px;box-shadow:0 26px 60px -26px rgba(0,0,0,.6)}
.mini b{display:block;height:9px;border-radius:9px;background:rgba(255,255,255,.55);margin-bottom:11px}.mini b:last-child{margin:0;background:rgba(255,255,255,.25)}
.m1{right:200px;top:560px;width:210px}.m1 b:first-child{width:60%;background:linear-gradient(90deg,var(--c1),var(--c2))}.m1 b:nth-child(2){width:90%}.m1 b:nth-child(3){width:70%}
.m2{right:690px;top:215px;width:160px}.m2 b:first-child{width:80%}.m2 b:nth-child(2){width:55%}
.txt{position:absolute;left:96px;top:300px;width:900px}
.eb{font:700 24px Sora;letter-spacing:.3em;color:var(--c1);margin-bottom:26px}
h1{font:800 var(--fs,92px)/1.04 Sora;color:#fff;letter-spacing:-.02em}
.sub{font:500 34px/1.35 Inter;color:rgba(255,255,255,.72);margin-top:26px}
.bar{width:120px;height:8px;border-radius:8px;background:linear-gradient(90deg,var(--c1),var(--c2));margin-top:40px}
.foot{position:absolute;left:96px;right:96px;bottom:44px;display:flex;justify-content:space-between;align-items:flex-end}
.doc{font:700 38px Sora;color:#0B1020;letter-spacing:-.01em}.doc span{color:var(--c1)}
.sign{display:flex;align-items:center;gap:22px;font:500 22px Inter;color:#7A8296}.sign img{height:40px}
'''
def page(d):
    fn,title,sub,cat,kind,ic,c1,c2=d
    n=len(title); fs='92px' if n<=24 else ('80px' if n<=34 else '68px')
    return f'''<div class="pg" style="--c1:{c1};--c2:{c2};--fs:{fs}"><div class="hero"><div class="glow"></div><div class="glow2"></div><div class="grid"></div><div class="rings"><i></i><i></i><i></i></div>
<div class="top"><img src="RIVER_AI_white.svg" alt=""><span class="chip">{kind}</span></div>
<div class="mini m2"><b></b><b></b></div><div class="tile"><div class="ic">{icon(ic)}</div></div><div class="mini m1"><b></b><b></b><b></b></div>
<div class="txt"><div class="eb">{cat}</div><h1>{title}</h1>{f'<div class="sub">{sub}</div>' if sub else ''}<div class="bar"></div></div></div>
<div class="foot"><div class="doc">Ürün <span>Özellikleri</span></div><div class="sign">river.com.tr<img src="fol_dark.png" alt=""></div></div></div>'''
OUT=os.environ.get('OUT','ak'); os.makedirs(OUT,exist_ok=True)
for i,d in enumerate(D):
    open(f'{OUT}/a_{i:02d}.html','w',encoding='utf-8').write(f'<!doctype html><html lang="tr"><head><meta charset="utf-8">{FONTS.replace("node_modules","../node_modules")}<style>{CSS}</style></head><body>{page(d).replace("src=\"RIVER_AI_white.svg\"","src=\"../RIVER_AI_white.svg\"").replace("src=\"fol_dark.png\"","src=\"../fol_dark.png\"")}</body></html>')
json.dump(D,open(f'{OUT}/docs.json','w',encoding='utf-8'),ensure_ascii=False)
print('ok',len(D))
