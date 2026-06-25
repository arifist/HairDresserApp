namespace BarberBookingApp.Helpers;

public static class PhoneNumberHelper
{
    // Kanonik depolama/karşılaştırma formatı: 10 hane, başında 0/90/+ olmadan (örn. "5551234567").
    public static string Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return string.Empty;
        }

        var digits = new string(phone.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("90") && digits.Length == 12)
        {
            digits = digits[2..];
        }
        else if (digits.StartsWith("0") && digits.Length == 11)
        {
            digits = digits[1..];
        }

        return digits;
    }

    public static bool IsValidTurkishMobile(string? phone)
    {
        var normalized = Normalize(phone);
        return normalized.Length == 10 && normalized[0] == '5';
    }

    // Twilio Verify E.164 formatı.
    public static string ToE164(string? phone) => "+90" + Normalize(phone);

    // Netgsm formatı (artı işareti olmadan).
    public static string ToNetgsmFormat(string? phone) => "90" + Normalize(phone);

    // Ekranda gösterim için: "555 123 45 67".
    public static string FormatForDisplay(string? phone)
    {
        var normalized = Normalize(phone);
        return normalized.Length == 10
            ? $"{normalized[..3]} {normalized.Substring(3, 3)} {normalized.Substring(6, 2)} {normalized.Substring(8, 2)}"
            : normalized;
    }
}
