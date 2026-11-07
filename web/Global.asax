<%@ Application Language="C#" %>
<%@ Import Namespace="System.Web.Routing" %>
<script runat="server">
    // Uzantisiz, profesyonel adresler (ASP.NET Routing - ek IIS modulu gerektirmez)
    //   /verify            -> Verify.aspx (kod giris formu)
    //   /verify/{kod}      -> Verify.aspx (QR ile dogrudan sonuc)
    //   /t/{link}          -> Transfer.aspx (dosya paylasimi, kisiye ozel link)
    void Application_Start(object sender, EventArgs e)
    {
        RouteTable.Routes.MapPageRoute("verify-code", "verify/{code}", "~/Verify.aspx");
        RouteTable.Routes.MapPageRoute("verify", "verify", "~/Verify.aspx");
        RouteTable.Routes.MapPageRoute("transfer", "t/{token}", "~/Transfer.aspx");   // dosya paylasimi indirme sayfasi
    }

    // Kalici oturum: sunucu oturumu dusmusse (bosta kalma, uygulama yeniden baslamasi, guncelleme) imzali cerezle kullaniciyi geri yukler
    void Application_AcquireRequestState(object sender, EventArgs e)
    {
        Auth.TryRestore(HttpContext.Current);
    }
</script>
