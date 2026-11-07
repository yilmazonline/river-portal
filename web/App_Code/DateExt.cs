using System;

// Depoda UTC (SYSUTCDATETIME) tutulan zamanlari Turkiye saatine cevirir.
// Sunucunun saat diliminden BAGIMSIZ calisir: her zaman UTC -> Istanbul (UTC+3).
public static class DateExt
{
    private static readonly TimeZoneInfo TrZone = ResolveTr();

    private static TimeZoneInfo ResolveTr()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"); }
        catch
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); }
            catch { return null; }
        }
    }

    public static DateTime ToTrLocal(this DateTime dt)
    {
        try
        {
            DateTime utc = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            if (TrZone != null) return TimeZoneInfo.ConvertTimeFromUtc(utc, TrZone);
        }
        catch { }
        return dt.AddHours(3); // guvenli varsayilan: Turkiye UTC+3
    }
}
