namespace BerberArif.Services;

public interface IOtpService
{
    Task<OtpRequestResult> RequestCodeAsync(string phoneNumber);
    Task<bool> VerifyCodeAsync(string phoneNumber, string code);
}

public record OtpRequestResult(bool Success, string? ErrorMessage);
