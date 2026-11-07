using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

// Toplu bildirim e-postasi: hazir tema ya da ozel HTML. Onizleme, test gonderimi ve asil gonderim ayni yoldan gecer.
public static class BroadcastHtml
{
    public const int MaxBytes = 1024 * 1024;      // ust sinir (1 MB)
    public const int GmailClip = 102 * 1024;      // Gmail bu boyutun ustunu kirpar

    /// <summary>Formdan e-posta HTML'i: theme=custom ise ozel HTML temizlenir, degilse hazir tema. error doluysa gonderilmez.</summary>
    public static string Build(NameValueCollection f, bool preview, out string error, out List<string> notes)
    {
        error = null; notes = new List<string>();
        string theme = f["theme"] ?? "duyuru";
        if (theme != "custom")
            return Emails.Broadcast(theme, f["title"] ?? "", f["body"] ?? "", f["btnText"] ?? "", f["btnUrl"] ?? "", f["f1"] ?? "", f["f2"] ?? "", f["f3"] ?? "", preview);
        string raw = f["html"] ?? "";
        if (raw.Trim().Length == 0) { error = "Özel HTML alanı boş."; return null; }
        if (Encoding.UTF8.GetByteCount(raw) > MaxBytes) { error = "HTML 1 MB sınırını aşıyor. Görselleri gömmek yerine bağlantıyla verin."; return null; }
        string html = Clean(raw, notes);
        if (Regex.Replace(Regex.Replace(html, @"<(style|head)[\s\S]*?</\1>", "", RegexOptions.IgnoreCase), @"<[^>]+>|&nbsp;|\s", "").Length == 0 && html.IndexOf("<img", StringComparison.OrdinalIgnoreCase) < 0)
        { error = "HTML'de görünür bir içerik bulunamadı."; return null; }
        int bytes = Encoding.UTF8.GetByteCount(html);
        if (bytes > GmailClip) notes.Add("E-posta " + (bytes / 1024) + " KB. Gmail 102 KB'den büyük iletileri kırpar ve \"İletinin tamamını görüntüle\" bağlantısı gösterir.");
        return html;
    }

    /// <summary>Calismayan / riskli icerigi cikarir; parca ise tam belgeye sarar. Cikarilanlar notes listesine yazilir.</summary>
    public static string Clean(string raw, List<string> notes)
    {
        string h = raw.Replace("\0", "");
        int n;
        h = Count(h, @"<script\b[\s\S]*?</script\s*>|<script\b[^>]*/?>", "", out n); if (n > 0) notes.Add(n + " betik (script) çıkarıldı.");
        h = Count(h, @"<(iframe|object|embed|applet|frameset|frame)\b[\s\S]*?</\1\s*>|<(iframe|object|embed|applet|frame)\b[^>]*/?>", "", out n); if (n > 0) notes.Add(n + " gömülü çerçeve/nesne çıkarıldı.");
        h = Count(h, @"</?form\b[^>]*>", "", out n); if (n > 0) notes.Add("Form etiketleri çıkarıldı (e-postada çalışmaz).");
        h = Count(h, @"<meta\b[^>]*http-equiv\s*=\s*[""']?refresh[^>]*>|<base\b[^>]*>", "", out n); if (n > 0) notes.Add("Yönlendirme / base etiketi çıkarıldı.");
        // olay isleyicileri ve javascript: baglantilari yalniz etiketlerin ICINDE aranir (duz metindeki "oneri = iyi" gibi ifadelere dokunulmaz)
        int ev = 0, js = 0;
        h = Regex.Replace(h, @"<[a-zA-Z][^>]*>", delegate (Match tag)
        {
            int a1, a2;
            string x = Count(tag.Value, @"\s+on[a-z]+\s*=\s*(""[^""]*""|'[^']*'|[^\s>]+)", "", out a1);
            x = Count(x, @"(\s(?:href|src|action|background|formaction|xlink:href)\s*=\s*)([""']?)\s*(?:javascript|vbscript|data:text/html)[^""'>\s]*\2", "$1$2#$2", out a2);
            ev += a1; js += a2; return x;
        });
        if (ev > 0) notes.Add(ev + " olay işleyicisi (onclick vb.) çıkarıldı.");
        if (js > 0) notes.Add(js + " javascript: bağlantısı etkisizleştirildi.");
        if (Regex.IsMatch(h, @"<html\b", RegexOptions.IgnoreCase))
        {
            if (!Regex.IsMatch(h, @"<meta\b[^>]*charset", RegexOptions.IgnoreCase))
                h = Regex.IsMatch(h, @"<head\b[^>]*>", RegexOptions.IgnoreCase)
                    ? Regex.Replace(h, @"(<head\b[^>]*>)", "$1<meta charset=\"utf-8\">", RegexOptions.IgnoreCase)
                    : Regex.Replace(h, @"(<html\b[^>]*>)", "$1<head><meta charset=\"utf-8\"></head>", RegexOptions.IgnoreCase);
            if (!h.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase)) h = "<!DOCTYPE html>\n" + h.TrimStart();
        }
        else
            h = "<!DOCTYPE html>\n<html lang=\"tr\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"></head><body style=\"margin:0;padding:0\">\n" + h.Trim() + "\n</body></html>";
        return h;
    }

    private static string Count(string s, string pattern, string repl, out int n)
    {
        int c = 0;
        string r = Regex.Replace(s, pattern, delegate (Match m) { c++; return m.Result(repl); }, RegexOptions.IgnoreCase);
        n = c; return r;
    }
}
