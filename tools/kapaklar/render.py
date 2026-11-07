import json, sys, glob, os
from playwright.sync_api import sync_playwright
P=json.load(open('products.json',encoding='utf-8'))
os.makedirs('pdf',exist_ok=True); os.makedirs('png',exist_ok=True)
files=sorted(glob.glob('out_*.html'))
with sync_playwright() as p:
    b=p.chromium.launch()
    for f in files:
        i=int(f[4:6]); name=P[i][0]
        pg=b.new_page(viewport={'width':794,'height':1123}, device_scale_factor=2)
        pg.goto('file://'+os.path.abspath(f)); pg.evaluate("document.fonts.ready"); pg.wait_for_timeout(400)
        fam=pg.evaluate("[...document.fonts].filter(f=>f.status=='loaded').map(f=>f.family+' '+f.weight).filter((v,i,a)=>a.indexOf(v)==i).join(', ')")
        pg.pdf(path=f'pdf/{name} - Kapak.pdf', width='210mm', height='297mm', print_background=True, margin={'top':'0','right':'0','bottom':'0','left':'0'})
        pg.screenshot(path=f'png/{name} - Kapak.png', full_page=False)
        print(f'{i:02d} {name} | yazı tipleri: {fam}')
        pg.close()
    b.close()
