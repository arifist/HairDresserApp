using BerberArif.Data;
using BerberArif.Models;

namespace BerberArif.Services;

public class NetgsmSmsService : ISmsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _db;
    private readonly ILogger<NetgsmSmsService> _logger;

    public NetgsmSmsService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        AppDbContext db,
        ILogger<NetgsmSmsService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _db = db;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string phoneNumber, string message)
    {
        var userCode = _configuration["Netgsm:UserCode"];
        var password = _configuration["Netgsm:Password"];
        var header = _configuration["Netgsm:Header"];
        var apiUrl = _configuration["Netgsm:ApiUrl"] ?? "https://api.netgsm.com.tr/sms/send/get";

        var log = new SmsLog
        {
            PhoneNumber = phoneNumber,
            Message = message,
            SentAt = DateTime.UtcNow
        };

        if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(password))
        {
            // Netgsm kimlik bilgileri henüz girilmemiş: gerçek SMS gönderilmez, simüle edilir ve loglanır.
            log.Provider = "Simulated(Netgsm)";
            log.Success = true;
            _logger.LogInformation("[SMS-SIMULASYON] -> {Phone}: {Message}", phoneNumber, message);
            _db.SmsLogs.Add(log);
            await _db.SaveChangesAsync();
            return true;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var normalizedPhone = NormalizePhone(phoneNumber);

            var query = $"usercode={Uri.EscapeDataString(userCode)}" +
                        $"&password={Uri.EscapeDataString(password)}" +
                        $"&gsmno={Uri.EscapeDataString(normalizedPhone)}" +
                        $"&message={Uri.EscapeDataString(message)}" +
                        $"&msgheader={Uri.EscapeDataString(header ?? string.Empty)}";

            var response = await client.GetAsync($"{apiUrl}?{query}");
            var responseBody = await response.Content.ReadAsStringAsync();

            // Netgsm başarı kodları "00" veya "01" ile başlar.
            var success = response.IsSuccessStatusCode &&
                          (responseBody.TrimStart().StartsWith("00") || responseBody.TrimStart().StartsWith("01"));

            log.Provider = "Netgsm";
            log.Success = success;
            if (!success)
            {
                log.ErrorMessage = responseBody;
            }

            _db.SmsLogs.Add(log);
            await _db.SaveChangesAsync();
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Netgsm SMS gönderimi başarısız: {Phone}", phoneNumber);
            log.Provider = "Netgsm";
            log.Success = false;
            log.ErrorMessage = ex.Message;
            _db.SmsLogs.Add(log);
            await _db.SaveChangesAsync();
            return false;
        }
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("90") && digits.Length == 12)
        {
            return digits;
        }
        if (digits.StartsWith("0") && digits.Length == 11)
        {
            return "90" + digits[1..];
        }
        if (digits.Length == 10)
        {
            return "90" + digits;
        }
        return digits;
    }
}
