using System;
using System.IO;
using System.Web;
using System.Web.Configuration;

// Rol bazli karsilama videolari. Dosyalar: App_Data/intro/intro-{rol}.mp4 + poster-{rol}.jpg
// Rol SUNUCUDA belirlenir (kullanici baska rolun videosunu isteyemez). Dosyasi olmayan rol icin karsilama yok.
public static class IntroVideo
{
    public static string RoleKey()
    {
        if (Auth.IsVendor) return "vendor";            // Portal yetkisi
        if (Auth.IsSupportAgent) return "support";     // yalniz Destek yetkisi
        if (Auth.CanEditTraining) return "editor";     // yalniz Editor yetkisi
        if (Auth.IsCustomerSide) return "customer";
        if (Auth.IsPartnerAdmin) return "partner";     // bayi yoneticisi (tam)
        return "partneruser";                          // kisitli bayi kullanicisi
    }

    public static bool Enabled
    {
        get
        {
            string v = (WebConfigurationManager.AppSettings["Intro.Enabled"] ?? "true").Trim().ToLowerInvariant();
            return v != "false" && v != "0";
        }
    }

    public static string VideoPath(string role) { return HttpContext.Current.Server.MapPath("~/App_Data/intro/intro-" + role + ".mp4"); }
    public static string PosterPath(string role) { return HttpContext.Current.Server.MapPath("~/App_Data/intro/poster-" + role + ".jpg"); }

    public static bool HasVideo(string role)
    {
        try { return Enabled && File.Exists(VideoPath(role)); } catch { return false; }
    }
}
