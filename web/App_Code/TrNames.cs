using System;
using System.Collections.Generic;
using System.Globalization;

// Yaygin Turkce adlarin cinsiyeti (hitap icin: "Ad Bey" / "Ad Hanim"). Iki cinsiyette de kullanilan adlar belirsiz sayilir.
// Liste disindaki ya da belirsiz adlarda karar Claude'un tahminine (ve onay kuyrugundaki kontrole) birakilir.
public static class TrNames
{
    static readonly CultureInfo TR = new CultureInfo("tr-TR");
    static readonly HashSet<string> Male = Set(
        "abdullah abdulkadir abdurrahman adem adnan ahmet akif akin alaattin alican ali alp alparslan alper alpay alptekin altan anil arda arif arslan aslan atakan atilla avni aydin ayhan aykut aytac " +
        "bahadir bahattin baki baran baris batuhan battal bayram bedirhan bekir berat berk berkay berkan berke bilal bora bulent burak burhan cafer can caner cagatay cagdas celal celil cem cemal cemil " +
        "cengiz cenk cevat cevdet cihan cuneyt dogan dogukan dursun ebubekir efe egemen ekrem emin emir emirhan emrah emre ender engin enis enver erdal erdem erdinc erdogan eren ergin ergun erhan erkan " +
        "erkut erol ersin ertan ertugrul ertunc esat ethem eyup fahri fatih fazil ferdi ferhat ferit feyzi fikret firat furkan gazi gokay gokhan gokmen guven gurkan haluk hamdi hamza hakan hakki halil " +
        "halit hamit harun hasan haydar hayri hikmet huseyin hulusi ibrahim ihsan ilhan ilker ilkay ilyas irfan ismail ismet izzet kaan kadir kamil kazim kemal kenan kerem kerim koray korkmaz kubilay " +
        "kursat levent lutfi mahir mahmut mehmet melih mert mesut metin mevlut muammer muhammed muhammet muhittin muhsin murat musa mustafa mutlu muzaffer nail nazim necati necdet necip nedim nevzat " +
        "nihat nuri nusret oguz oguzhan okan oktay olcay omer onder onur orhan osman ozan ozcan ozer ozgur ozkan ozturk polat ramazan rasim recep remzi resat ridvan rifat riza ruhi rusen sabri sadi " +
        "sadik salih salim sami samet sarp sedat selahattin selcuk selim sener serdar serhan serhat serkan sertac sezer sezgin sinan sitki suat suleyman sukru tahir talat tamer taner tarik tayfun " +
        "taylan tayyar tekin tevfik tolga tolgahan tufan tuncay tuncer turan turgay turgut tugrul ufuk ugur umit unal utku vahit vedat veli volkan yakup yasar yasin yavuz yigit yilmaz yucel yunus yusuf " +
        "yuksel zafer zekai zeki ziya ahmed mehmed hilmi riza sefa safa ozgun alpaslan tuna mirac berkcan kutay kagan eray alperen semih sefer nadir erkin gunay turker selami fevzi ersan ercan kivanc tunc oytun");
    static readonly HashSet<string> Female = Set(
        "ayse fatma emine hatice zeynep elif meryem sultan hulya merve busra esra ozlem yasemin seda sevgi derya gulsen songul tugba aysel aynur nurten nermin sibel filiz serap leyla canan dilek ebru " +
        "pinar asli asli aylin aysun aysegul ayten bahar banu basak belgin beril berna betul beyza bilge buket burcu cansu ceren cigdem damla defne demet didem dilara dilan dilber duygu dicle ecem " +
        "eda ece ela elcin elvan emel esin esma fadime feride fulya funda gamze gizem gonca gonul gozde gul gulay gulcan guler gulistan gulnur gulsah gulten gunes gulsum hacer hande hanife hatun havva " +
        "hilal irem ipek isil kader kadriye kevser kubra lale latife makbule melek melike melis meltem mine mukaddes muge muazzez nagihan naz nazan nazli nebahat necla nesrin neslihan nevin nilay " +
        "nilgun nilufer nur nuray nurcan nurhan nuriye ozge ozden perihan rabia reyhan rukiye sabahat sadiye saadet safiye sakine selda selin sema semra senay seher sehnaz selma sena serpil sevda sevil " +
        "sevim sevinc seyma sezen simge sinem sule sukran tulay tulin tuba tuna turkan ulku umran yeliz yesim yildiz zehra zeliha zerrin zuhal zubeyde buse cagla dilsad elcin eylul gulbahar hazal " +
        "ilayda irmak nisa oyku ozlem pelin sila tugce yagmur zuleyha asiye aysenur feyza gulizar hasret ilknur mehtap muzeyyen oznur rana saliha semiha sevtap sirin suna tuana yonca zeynepnur esma " +
        "ayca aybike aysu ayla cemile hamiyet necmiye nilsen sermin tulya ulviye vildan arzu alev eylem tansu gokce hayal ruya doga aydan");
    // iki cinsiyette de yaygin: belirsiz
    static readonly HashSet<string> Unisex = Set("deniz evren umut ozgur ilkay gunes eser sezer tuna olcay aytac yagmur bilge ozden cagri");

    static HashSet<string> Set(string s) { HashSet<string> h = new HashSet<string>(); foreach (string w in s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) h.Add(w); return h; }
    public static string Fold(string s)
    {
        s = (s ?? "").Trim().ToLower(TR);
        char[] a = s.ToCharArray();
        for (int i = 0; i < a.Length; i++) { char c = a[i]; a[i] = c == 'ç' ? 'c' : c == 'ğ' ? 'g' : c == 'ı' ? 'i' : c == 'ö' ? 'o' : c == 'ş' ? 's' : c == 'ü' ? 'u' : c == 'â' ? 'a' : c == 'î' ? 'i' : c; }
        return new string(a);
    }
    /// <summary>"E", "K" ya da null (listede yok / iki cinsiyette de kullaniliyor). Bilesik adlarda ilk ad esas alinir (Mehmet Ali -> E).</summary>
    public static string Gender(string firstName)
    {
        string f = Fold((firstName ?? "").Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries).Length > 0 ? firstName.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)[0] : "");
        if (f.Length == 0 || Unisex.Contains(f)) return null;
        bool m = Male.Contains(f), k = Female.Contains(f);
        return m && !k ? "E" : k && !m ? "K" : null;
    }
    /// <summary>Ad(lar)i Turkce kurallarla buyuk harfle baslatir: "AYŞE NUR" -> "Ayşe Nur", "ismail" -> "İsmail"</summary>
    public static string Proper(string s)
    {
        string[] p = (s ?? "").Trim().ToLower(TR).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < p.Length; i++) p[i] = p[i].Substring(0, 1).ToUpper(TR) + p[i].Substring(1);
        return string.Join(" ", p);
    }
}
