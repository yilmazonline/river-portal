using System.Collections.Generic;
using System.Web.Configuration;

// Yeni eğitim içeriği eklendiğinde bildirim: uygulama içi (her zaman) + e-posta (tercihi açık olanlar).
public static class Notifier
{
    private static string BaseUrl()
    {
        string b = (WebConfigurationManager.AppSettings["Site.BaseUrl"] ?? "").Trim();
        if (b.Length == 0) b = "https://portal.folyazilim.com";
        return b.TrimEnd('/');
    }

    public static void TrainingContentAdded(string title, string category)
    {
        // 1) Uygulama içi bildirim — her zaman (tüm firmalar)
        try
        {
            Repo.NotifyAllCompanies("Yeni eğitim içeriği", "New training content",
                "River Akademi'ye yeni içerik eklendi: \"" + title + "\". Hemen inceleyebilirsiniz.",
                "New training content added: \"" + title + "\". Take a look now.", "Training", "Training.aspx");
        }
        catch { }

        // 2) E-posta — yalnız eğitim e-posta bildirimi AÇIK olan aktif kullanıcılar (best effort)
        try
        {
            string html = Emails.TrainingNew(title, category, BaseUrl(), "tr");
            List<string> emails = Repo.GetActiveEmailsByPref("NotifyTrainingEmail");
            for (int i = 0; i < emails.Count; i++)
            {
                string err;
                try { Mailer.SendTo(emails[i], "Yeni eğitim içeriği · River Akademi", html, out err); }
                catch { }
            }
        }
        catch { }
    }
}
