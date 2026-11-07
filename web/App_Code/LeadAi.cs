using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

// Onay kuyrugu: taslaga ozel "AI ile kontrol et" ve "AI ile duzenle".
// Her cagri tek bir taslak ve onun alicisi icin: alici profili (unvan, kategori, persona, firma, sektor, buyukluk, sehir, firma tanimi) ile.
// Uydurma yok: River AI hakkinda yalniz mevcut taslaktaki bilgiler; alici firma hakkinda yalniz profil.
public static class LeadAi
{
    /// <summary>Taslak ve alicisi (firma tanimi dahil)</summary>
    public static LdContact Context(int messageId, out int step, out string status)
    {
        step = 0; status = null; LdContact x = null;
        using (SqlConnection c = Db.Open())
        using (SqlCommand q = new SqlCommand("SELECT m.StepNo, m.Status, ct.ContactId, ct.Category, ct.FullName, ct.FirstName, ct.Title, ct.Persona, ct.Email, co.Name, co.Industry, co.City, co.Employees, " +
            (Leads.HasDescription ? "co.Description" : "NULL") + ", " + (Leads.HasGender ? "ct.Gender, ct.GenderSure" : "NULL, CAST(0 AS BIT)") + " FROM dbo.LeadMessages m JOIN dbo.LeadContacts ct ON ct.ContactId = m.ContactId LEFT JOIN dbo.LeadCompanies co ON co.LeadCompanyId = ct.LeadCompanyId WHERE m.MessageId = @i", c))
        {
            q.Parameters.AddWithValue("@i", messageId);
            using (SqlDataReader r = q.ExecuteReader())
                if (r.Read())
                {
                    step = r.GetInt32(0); status = r.GetString(1); x = new LdContact();
                    x.Id = r.GetInt32(2); x.Category = r.GetString(3); x.FullName = r.GetString(4); x.FirstName = r.IsDBNull(5) ? "" : r.GetString(5); x.Title = r.IsDBNull(6) ? "" : r.GetString(6);
                    x.Persona = r.IsDBNull(7) ? "" : r.GetString(7); x.Email = r.GetString(8); x.CompanyName = r.IsDBNull(9) ? "" : r.GetString(9); x.Industry = r.IsDBNull(10) ? "" : r.GetString(10);
                    x.City = r.IsDBNull(11) ? "" : r.GetString(11); x.Employees = r.IsDBNull(12) ? (int?)null : r.GetInt32(12); x.Description = r.IsDBNull(13) ? "" : r.GetString(13);
                    x.Gender = r.IsDBNull(14) ? null : r.GetString(14); x.GenderSure = !r.IsDBNull(15) && r.GetBoolean(15);
                }
        }
        return x;
    }

    static string Profile(LdContact x, int step)
    {
        string per = x.Persona == "yonetim" ? "üst yönetim" : x.Persona == "bt" ? "bilgi teknolojileri" : x.Persona == "surec" ? "süreç ve kalite" : x.Persona == "ortak" ? "iş ortağı adayı" : "genel";
        string d = (x.Description ?? "").Replace("\r", " ").Replace("\n", " ").Trim(); if (d.Length > 700) d = d.Substring(0, 700);
        return "Kategori: " + (x.Category == "danisman" ? "danışmanlık / BT hizmet firması (iş ortağı adayı)" : "potansiyel müşteri") +
               "\nE-posta türü: " + (step == 0 ? "ilk tanıtım e-postası" : step == 9 ? "gelen yanıta cevap" : step + ". takip e-postası (yanıt gelmedi)") +
               "\nAlıcı: " + x.FullName + "\nKullanılacak hitap (birebir): " + Leads.Hitap(x) + "\nUnvan: " + (string.IsNullOrEmpty(x.Title) ? "bilinmiyor" : x.Title) + "\nPersona: " + per +
               "\nFirma: " + (string.IsNullOrEmpty(x.CompanyName) ? "bilinmiyor" : x.CompanyName) + "\nSektör: " + (string.IsNullOrEmpty(x.Industry) ? "bilinmiyor" : x.Industry) +
               "\nŞehir: " + (string.IsNullOrEmpty(x.City) ? "bilinmiyor" : x.City) + "\nÇalışan sayısı (üst sınır): " + (x.Employees.HasValue ? x.Employees.Value.ToString() : "bilinmiyor") +
               (d.Length > 0 ? "\nFirmanın kendi tanımı: " + d : "\nFirma tanımı: yok");
    }

    /// <summary>Ucretsiz on kontrol (Claude'a gitmeden): yer tutucu, tekrar, belirsiz hitap, konu uzunlugu, spam kelimeleri</summary>
    public static List<Dictionary<string, object>> PreCheck(LdContact x, string subject, string body)
    {
        List<Dictionary<string, object>> l = new List<Dictionary<string, object>>();
        Action<string, string, string, string> add = delegate (string type, string quote, string sug, string why)
        { Dictionary<string, object> d = new Dictionary<string, object>(); d["type"] = type; d["quote"] = quote; d["suggestion"] = sug; d["why"] = why; d["auto"] = true; l.Add(d); };
        string all = (subject ?? "") + "\n" + (body ?? "");
        foreach (Match m in Regex.Matches(all, @"\{[^{}\s]{1,20}\}")) add("yer tutucu", m.Value, "Yer tutucuyu gerçek değerle değiştirin ya da silin.", "Doldurulmamış yer tutucu alıcıya olduğu gibi gider.");
        foreach (Match m in Regex.Matches(all, @"\b(\w{2,})\s+\1\b", RegexOptions.IgnoreCase)) add("tekrar", m.Value, m.Groups[1].Value, "Aynı kelime art arda yazılmış.");
        if (Regex.IsMatch(body ?? "", @"Bey\s*/\s*Hanım|Hanım\s*/\s*Bey", RegexOptions.IgnoreCase)) add("hitap", Regex.Match(body, @"\S+\s*Bey\s*/\s*Hanım|\S+\s*Hanım\s*/\s*Bey", RegexOptions.IgnoreCase).Value, "Kişinin cinsiyetine uygun tek hitap (… Bey ya da … Hanım) ya da ad soyad.", "“Bey/Hanım” birlikte kaba ve kalıp görünür.");
        string first = (x.FirstName ?? "").Trim(); string head = Regex.Replace(body ?? "", "<[^>]+>", " ");
        head = head.Length > 160 ? head.Substring(0, 160) : head;
        if (first.Length > 1 && Regex.IsMatch(head, @"^\s*(Merhaba|Sayın|Selam)", RegexOptions.IgnoreCase) && Leads.Fold(head).IndexOf(Leads.Fold(first), StringComparison.Ordinal) < 0)
            add("hitap", head.Split('\n')[0].Trim(), "Merhaba " + first + " Bey/Hanım (uygun olanı)", "Hitapta alıcının adı (" + first + ") geçmiyor; başka birine yazılmış gibi görünebilir.");
        string full = (x.FullName ?? "").Trim();
        if (full.Contains(" ") && Leads.Fold(head).IndexOf(Leads.Fold(full), StringComparison.Ordinal) >= 0)
            add("hitap", full, "Merhaba " + Leads.Hitap(x) + ",", "Hitapta ad soyad yazılması otomatik e-posta izlenimi verir; \"Ad Bey / Ad Hanım\" kullanın.");
        if ((subject ?? "").Length > 70) add("konu", subject, "Konuyu 60 karakterin altına indirin.", "Uzun konu satırları telefonda kesilir.");
        // Turkce kucuk harfle ara ("ÜCRETSİZ" -> "ücretsiz"); butun eslesmeler raporlanir
        foreach (Match sp in Regex.Matches((subject ?? "").ToLower(Leads.TR), @"ücretsiz|bedava|fırsat|kampanya|indirim|acil|kaçırma|!!+|%\s*\d+"))
            add("spam riski", sp.Value, "Bu ifadeyi konudan çıkarın.", "Spam filtrelerinin puan kırdığı bir ifade.");
        if (Regex.IsMatch(body ?? "", @"  +")) add("biçim", "çift boşluk", "Tek boşluk", "Metinde art arda boşluk var.");
        return l;
    }

    /// <summary>AI kontrolu: yalniz gercek hatalar; duzeltilmis hal icerigi yeniden yazmaz</summary>
    public static Dictionary<string, object> Check(LdContact x, int step, string subject, string body, out string err)
    {
        err = null; bool html = Leads.IsHtml(body);
        string sys = "Türkçe B2B tanıtım e-postaları için titiz bir editörsün. Sana bir e-posta taslağı ve alıcı bilgileri veriliyor. Yalnız GERÇEK hataları bul: yazım, noktalama, dilbilgisi, Türkçe karakter (ı/i, ş, ğ, ç, ö, ü) hataları; " +
                     "hitap (selamlama verilen 'Kullanılacak hitap' ile birebir aynı olmalı: Ad Bey ya da Ad Hanım; ad soyad yazılmışsa, Bey/Hanım yanlışsa ya da başka bir kişinin adı geçiyorsa hata say ve düzeltilmiş halde verilen hitabı kullan); firma adının yanlış yazılışı; anlamsız, devrik ya da çeviri kokan ifadeler; " +
                     "gereksiz tekrarlar; doldurulmamış {yer tutucular}; alıcı profiliyle çelişen ifadeler (örneğin danışman firmaya müşteri gibi yazılması). Zevk ve üslup tercihlerini hata sayma. En fazla 8 madde. " +
                     "Düzeltilmiş hali üretirken YALNIZ bulduğun hataları düzelt; içeriği yeniden yazma, cümle ekleme ya da silme, yeni iddia ekleme. Metnin sonundaki imza ya da kapanış bloğunu (Saygılarımla, isim, unvan, iletişim) yazım hatası yoksa aynen koru; ret cümlesi ekleme (gönderimde eklenir). " +
                     (html ? "Metin HTML: düzeltilmiş metni aynı HTML yapısıyla döndür. " : "Metin düz yazı: düzeltilmiş metni düz yazı olarak, paragrafları boş satırla ayırarak döndür. ") +
                     "Sadece şu JSON'u döndür: {\"issues\":[{\"type\":\"yazım|noktalama|dilbilgisi|hitap|ifade|tekrar|yer tutucu|tutarlılık\",\"quote\":\"metindeki sorunlu kısım\",\"suggestion\":\"önerilen hali\",\"why\":\"kısa gerekçe\"}]," +
                     "\"subject\":\"düzeltilmiş konu\",\"body\":\"düzeltilmiş metin\",\"summary\":\"tek cümlelik genel değerlendirme\"}. Hata yoksa issues boş liste olsun ve konu ile metni aynen döndür.";
        string usr = Profile(x, step) + "\n\n--- KONU ---\n" + subject + "\n\n--- METİN ---\n" + body;
        return LeadClaude.Json(sys, usr, 2000, out err);
    }

    /// <summary>Sablonu AI ile duzenle: yer tutucular korunur ({Hitap}, {Firma}, {Giris}...); imza bloku ve talimattaki imza aynen kullanilir; River AI bilgileri yalniz sablon ve talimattan</summary>
    public static Dictionary<string, object> WriteTemplate(string category, string persona, int step, string subject, string body, string instruction, out string err)
    {
        err = null; bool html = Leads.IsHtml(body);
        string per = persona == "yonetim" ? "üst yönetim" : persona == "bt" ? "bilgi teknolojileri" : persona == "surec" ? "süreç ve kalite" : persona == "ortak" ? "iş ortağı adayı" : "genel";
        string kind = step == 0 ? "İlk tanıtım e-postası: 90-150 kelime, 3-4 kısa paragraf." : step + ". takip e-postası (önceki e-postaya yanıt gelmedi): 40-80 kelime, önceki e-postaya kısa atıf, tek yeni değer cümlesi.";
        string sys = "Fol Yazılım'ın River AI platformu için Türkçe B2B soğuk e-posta ŞABLONU yazıyorsun; şablon yüzlerce kişiye kişiselleştirilerek gönderilecek. Kurallar: " +
            "1) Yer tutucuları aynen kullan ve koru: {Hitap} (alıcının adı ve Bey/Hanım, ör. Ayşe Hanım), {Ad}, {Firma}, {Unvan}, {Giris} (sistemin her kişi için yazdığı tek cümlelik kişisel giriş)" + (step > 0 ? ", {Konu} (ilk e-postanın konusu)" : "") + ". Yer tutucunun adını değiştirme, yeni yer tutucu uydurma. " +
            "2) Selamlama: 'Merhaba {Hitap},'. " + (step == 0 ? "Selamlamadan sonraki satıra tek başına {Giris} koy. " : "") +
            "3) River AI hakkında YALNIZ mevcut şablondaki ve kullanıcı talimatındaki bilgileri kullan; yeni rakam, müşteri adı, özellik, sertifika ya da ödül uydurma. " +
            "4) Doğal, saygılı, kısa; kalıp pazarlama dili ve abartı yok; sonda tek ve net bir soru ile kısa görüşme daveti. " + kind + " " +
            "5) Konu en fazla 60 karakter; 'ücretsiz, fırsat, kampanya, acil' gibi kelimeler ve ünlem yok." + (step > 0 ? " Takip e-postasında konu mevcut konu olarak kalsın." : "") + " " +
            "6) İmza: kullanıcı talimatında imza verildiyse onu HARFİ HARFİNE metnin sonuna koy; verilmediyse mevcut şablonun sonundaki imza ya da kapanış bloğunu aynen koru; hiçbiri yoksa imza uydurma. Ret cümlesi ekleme (gönderimde otomatik eklenir). " +
            "7) Kullanıcının talimatı bu kurallarla çelişmedikçe önceliklidir. " +
            "8) " + (html ? "Biçim HTML: yalnız <p>, <b>, <br>, <a> etiketleri." : "Biçim düz yazı: paragrafları boş satırla ayır, HTML kullanma.") + " " +
            "Sadece şu JSON'u döndür: {\"subject\":\"...\",\"body\":\"...\",\"notes\":\"ne değiştirdiğin, tek cümle\"}.";
        string usr = "Kategori: " + (category == "danisman" ? "danışmanlık / BT hizmet firmaları (iş ortağı adayı)" : "potansiyel müşteri") + "\nPersona: " + per + "\nE-posta türü: " + (step == 0 ? "ilk e-posta" : step + ". takip") +
                     "\n\n--- KULLANICI TALİMATI ---\n" + (string.IsNullOrWhiteSpace(instruction) ? "(yok: şablonu kurallara göre iyileştir)" : instruction.Trim()) +
                     "\n\n--- MEVCUT KONU ---\n" + subject + "\n\n--- MEVCUT ŞABLON (River AI bilgilerinin kaynağı) ---\n" + body;
        return LeadClaude.Json(sys, usr, 1600, out err);
    }

    /// <summary>AI ile duzenle: alici profiline ozel yeniden yazim; River AI iddialari yalniz mevcut taslaktan</summary>
    public static Dictionary<string, object> Write(LdContact x, int step, string subject, string body, out string err)
    {
        err = null; bool html = Leads.IsHtml(body);
        string kind = step == 0 ? "İlk tanıtım e-postası: 90-150 kelime, 3-4 kısa paragraf." : step == 9 ? "Gelen bir yanıta cevap: kısa ve doğrudan, 40-100 kelime." : step + ". takip e-postası (önceki e-postaya yanıt gelmedi): 40-80 kelime, önceki e-postaya kısa atıf, tek yeni değer cümlesi.";
        string sys = "Fol Yazılım'ın River AI platformu için Türkçe B2B tanıtım e-postası yazıyorsun. Alıcının firma profiline göre bu kişiye özel, kısa, doğal ve saygılı bir e-posta oluştur. Kurallar: " +
                     "1) River AI hakkında YALNIZ mevcut taslaktaki bilgileri kullan; yeni rakam, müşteri adı, özellik, sertifika ya da ödül ekleme. " +
                     "2) Alıcı firma hakkında YALNIZ verilen profili kullan; uydurma proje, başarı, haber yok. Abartılı iltifat ve kalıp cümle yok ('çalışmalarınızı takdirle izliyoruz' gibi). " +
                     "3) Hitap: 'Merhaba <Kullanılacak hitap>,' biçiminde, profilde verilen hitabı birebir kullan (Ad Bey ya da Ad Hanım). Asla ad soyad yazma. " +
                     "4) Kategori danışman ise iş ortaklığı açısından yaz (firmanın uzmanlığıyla River AI arasında somut bir bağ kur; müşterilerine ne sunabilecekleri). Müşteri ise unvanına uygun faydayı öne çıkar (yönetim: görünürlük ve hız; BT: kontrol ve entegrasyon; süreç/kalite: modelleme ve standartlaştırma). " +
                     "5) " + kind + " Sonda tek ve net bir soru ile kısa bir görüşme daveti. " +
                     "6) Mevcut taslağın sonunda imza ya da kapanış bloğu varsa (Saygılarımla, isim, unvan, telefon, web adresi) onu HARFİ HARFİNE koru ve metnin sonuna koy; yoksa imza uydurma. Ret cümlesi ekleme (gönderimde eklenir). " +
                     "7) Konu en fazla 60 karakter; 'ücretsiz, fırsat, kampanya, acil' gibi kelimeler ve ünlem yok. Takip e-postasında konu mevcut konu olarak kalsın. " +
                     "8) " + (html ? "Biçim HTML: yalnız <p>, <b>, <br>, <a> etiketleri." : "Biçim düz yazı: paragrafları boş satırla ayır, HTML kullanma.") + " " +
                     "Sadece şu JSON'u döndür: {\"subject\":\"...\",\"body\":\"...\",\"notes\":\"bu kişiye özel neyi vurguladığın, tek cümle\"}.";
        string usr = Profile(x, step) + "\n\n--- MEVCUT KONU ---\n" + subject + "\n\n--- MEVCUT TASLAK (River AI hakkındaki bilgilerin tek kaynağı) ---\n" + body;
        return LeadClaude.Json(sys, usr, 1500, out err);
    }
}
