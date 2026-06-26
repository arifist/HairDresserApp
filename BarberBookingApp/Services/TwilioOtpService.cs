using BarberBookingApp.Data;
using BarberBookingApp.Models;
using Twilio.Exceptions;
using Twilio.Rest.Verify.V2.Service;

namespace BarberBookingApp.Services;

public class TwilioOtpService : IOtpService
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);
    private const string DemoCode = "0000";

    private readonly JsonDataStore _store;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioOtpService> _logger;

    public TwilioOtpService(JsonDataStore store, IConfiguration configuration, ILogger<TwilioOtpService> logger)
    {
        _store = store;
        _configuration = configuration;
        _logger = logger;
    }

    private string? VerifyServiceSid => _configuration["Twilio:VerifyServiceSid"];
    private bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_configuration["Twilio:AccountSid"]) &&
        !string.IsNullOrWhiteSpace(_configuration["Twilio:AuthToken"]) &&
        !string.IsNullOrWhiteSpace(VerifyServiceSid);
    private bool IsDemoOtpEnabled => _configuration.GetValue("AppSettings:DemoOtpEnabled", true);

    public async Task<OtpRequestResult> RequestCodeAsync(string phoneNumber)
    {
        var now = DateTime.UtcNow;
        phoneNumber = phoneNumber.Trim();

        if (IsDemoOtpEnabled)
            return await RequestDemoCodeAsync(phoneNumber, now);

        var lastLog = _store.SmsLogs
            .Where(l => l.PhoneNumber == phoneNumber)
            .OrderByDescending(l => l.SentAt)
            .FirstOrDefault();

        if (lastLog != null && now - lastLog.SentAt < ResendCooldown)
        {
            var waitSeconds = (int)(ResendCooldown - (now - lastLog.SentAt)).TotalSeconds;
            return new OtpRequestResult(false, $"Lütfen yeni kod istemeden önce {waitSeconds} saniye bekleyin.");
        }

        if (!IsConfigured)
            return await RequestSimulatedCodeAsync(phoneNumber, now);

        var log = new SmsLog { PhoneNumber = phoneNumber, Message = "Twilio Verify doğrulama kodu", SentAt = now, Provider = "Twilio Verify" };

        try
        {
            var verification = await VerificationResource.CreateAsync(to: NormalizePhone(phoneNumber), channel: "sms", pathServiceSid: VerifyServiceSid);
            log.Success = verification.Status == "pending";
            await _store.AddSmsLogAsync(log);
            return log.Success ? new OtpRequestResult(true, null) : new OtpRequestResult(false, "SMS gönderilemedi, lütfen daha sonra tekrar deneyin.");
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Twilio Verify SMS gönderimi başarısız: {Phone}", phoneNumber);
            log.Success = false;
            log.ErrorMessage = ex.Message;
            await _store.AddSmsLogAsync(log);
            return new OtpRequestResult(false, "SMS gönderilemedi, lütfen daha sonra tekrar deneyin.");
        }
    }

    public async Task<bool> VerifyCodeAsync(string phoneNumber, string code)
    {
        phoneNumber = phoneNumber.Trim();

        if (IsDemoOtpEnabled)
            return await VerifyDemoCodeAsync(phoneNumber, code);

        if (!IsConfigured)
            return await VerifySimulatedCodeAsync(phoneNumber, code);

        try
        {
            var check = await VerificationCheckResource.CreateAsync(to: NormalizePhone(phoneNumber), code: code, pathServiceSid: VerifyServiceSid);
            return check.Status == "approved";
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Twilio Verify kod doğrulaması başarısız: {Phone}", phoneNumber);
            return false;
        }
    }

    private async Task<OtpRequestResult> RequestDemoCodeAsync(string phoneNumber, DateTime now)
    {
        await _store.AddOtpVerificationAsync(new OtpVerification { PhoneNumber = phoneNumber, Code = DemoCode, ExpiresAt = now.Add(CodeLifetime), CreatedAt = now });
        _logger.LogInformation("[DEMO-OTP] -> {Phone}: sabit kod {Code}", phoneNumber, DemoCode);
        return new OtpRequestResult(true, null);
    }

    private async Task<bool> VerifyDemoCodeAsync(string phoneNumber, string code)
    {
        if (code.Trim() != DemoCode) return false;
        var now = DateTime.UtcNow;
        var otp = _store.OtpVerifications
            .Where(o => o.PhoneNumber == phoneNumber && !o.IsUsed && o.ExpiresAt > now)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefault();
        if (otp is null) return false;
        otp.IsUsed = true;
        await _store.UpdateOtpVerificationsAsync();
        return true;
    }

    private async Task<OtpRequestResult> RequestSimulatedCodeAsync(string phoneNumber, DateTime now)
    {
        var code = Random.Shared.Next(100000, 999999).ToString();
        await _store.AddOtpVerificationAsync(new OtpVerification { PhoneNumber = phoneNumber, Code = code, ExpiresAt = now.Add(CodeLifetime), CreatedAt = now });
        await _store.AddSmsLogAsync(new SmsLog { PhoneNumber = phoneNumber, Message = $"Kuaför Arif dogrulama kodunuz: {code}.", SentAt = now, Provider = "Simulated(Twilio)", Success = true });
        _logger.LogInformation("[SMS-SIMULASYON] -> {Phone}: kod {Code}", phoneNumber, code);
        return new OtpRequestResult(true, null);
    }

    private async Task<bool> VerifySimulatedCodeAsync(string phoneNumber, string code)
    {
        var now = DateTime.UtcNow;
        var otp = _store.OtpVerifications
            .Where(o => o.PhoneNumber == phoneNumber && !o.IsUsed && o.ExpiresAt > now)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefault();
        if (otp == null || otp.Code != code) return false;
        otp.IsUsed = true;
        await _store.UpdateOtpVerificationsAsync();
        return true;
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("90") && digits.Length == 12) return "+" + digits;
        if (digits.StartsWith("0") && digits.Length == 11) return "+90" + digits[1..];
        if (digits.Length == 10) return "+90" + digits;
        return "+" + digits;
    }
}
