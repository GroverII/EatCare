using CloudinaryDotNet;
using EatCareService.Modules;
using SkiaSharp;

namespace EatCareService.Services
{
    public interface IEatCareServices
    {
        string HashPassword(string password);
        bool VerifyPassword(string enteredPassword, string? storedPasswordHash);
        Task<bool> VerifyAuthentication(AuthenticationData authenticationData, User user);
        Task<bool> VerifyApiKey(string enteredApiKey, int userId);
        string GenerateRandomText();
        float CalculateOptimalTextSize(SKCanvas canvas, SKPaint paint, string text);
        string GenerateApiKey(int userId);
        string verifyGlobalApi(string requestApiKey);
        Task<string> autApi(string autApiKey, int userId);

        Task<string> UploadImageAsync(FileDescription fileDescription);
        byte[] DownloadImage(string assetId);
    }
}
