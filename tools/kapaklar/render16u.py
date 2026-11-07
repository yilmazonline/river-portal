import json, os, glob
OUT=os.environ.get('OUT','ak')
from playwright.sync_api import sync_playwright
from PIL import Image
D=json.load(open(OUT+'/docs.json',encoding='utf-8')); os.makedirs(OUT+'/png',exist_ok=True)
with sync_playwright() as p:
    b=p.chromium.launch()
    for f in sorted(glob.glob(OUT+'/a_*.html')):
        i=int(f.split('_')[1][:2]); pg=b.new_page(viewport={'width':1920,'height':1080})
        pg.goto('file://'+os.path.abspath(f)); pg.evaluate("document.fonts.ready"); pg.wait_for_timeout(300)
        out=f'{OUT}/png/{D[i][0]}.png'; pg.screenshot(path=out); pg.close()
        im=Image.open(out).convert('RGB'); im.save(out.replace('.png','.webp'),'WEBP',quality=88,method=6)
    b.close()
print('ok')
