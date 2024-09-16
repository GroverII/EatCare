using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EatCareService.Data;
using EatCareService.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Security.Cryptography;
using System.Text.Json;

namespace EatCareService.Services
{
    public class EatCareServices : IEatCareServices
    {
        private readonly ApplicationDbContext _context;
        private readonly string apiKey = "fWK5bASE153KnTH3Cyuyrwjl6AzSVy1ZYAWYhYQPEZ7mL1PRXYaT0lkYeqzDkUmm2ZX3yISkCIupBk7Pl6IahBmSbmDksOxdPdE1IfehVGvBG6yTWYWABq0n6VXAvLkM";
        private static readonly List<Ticket> Tickets = new List<Ticket>();
        private readonly Cloudinary _cloudinary;

        public EatCareServices(ApplicationDbContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string enteredPassword, string? storedPasswordHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPasswordHash);
        }

        public async Task<bool> VerifyAuthentication(AuthenticationData authenticationData, User user)
        {
            // Ваш код проверки аутентификации
            bool isPasswordValid = VerifyPassword(authenticationData.Password, user.Password);
            bool isApiKeyValid = await VerifyApiKey(authenticationData.ApiKey, user.Id);

            return isPasswordValid && isApiKeyValid;
        }

        public async Task<bool> VerifyApiKey(string enteredApiKey, int userId)
        {
            try
            {
                ApiKey apiKeyEntity = await _context.ApiKeys.FirstOrDefaultAsync(a => a.UserId == userId);

                if (apiKeyEntity == null)
                {
                    return false;
                }

                // Сравниваем строки с использованием Equals для надежности
                return string.Equals(enteredApiKey, apiKeyEntity.ApiKeyString, StringComparison.Ordinal);
            }
            catch (Exception ex)
            {
                // Желательно логировать исключение для последующего анализа
                Console.WriteLine($"Error verifying API key: {ex.Message}");
                return false;
            }
        }

        public string GenerateRandomText()
        {
            // Генерируем случайный текст
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 10)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public float CalculateOptimalTextSize(SKCanvas canvas, SKPaint paint, string text)
        {
            // Находим оптимальный размер текста, чтобы он занимал всю ширину изображения
            float textWidth = paint.MeasureText(text);
            float canvasWidth = canvas.LocalClipBounds.Width;
            float textSize = paint.TextSize;

            while (textWidth < canvasWidth)
            {
                textSize += 1;
                paint.TextSize = textSize;
                textWidth = paint.MeasureText(text);
            }

            return textSize;
        }

        public string GenerateApiKey(int userId)
        {
            try
            {
                // Проверяем существование пользователя с заданным ID
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);

                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }

                // Генерируем уникальный API-ключ
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    byte[] keyData = new byte[32]; // 32 байта для генерации 256-битного ключа
                    rng.GetBytes(keyData);

                    // Преобразование байтов в строку в шестнадцатеричном формате без разделителей
                    string apiKey = BitConverter.ToString(keyData).Replace("-", "").ToLower();

                    return apiKey;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating API key: {ex.Message}");
                throw; // Пробрасываем исключение выше для обработки в вызывающем коде
            }
        }

        public string verifyGlobalApi(string requestApiKey) {
            if (!string.IsNullOrEmpty(requestApiKey))
            {
                try
                {
                    // Parse the JSON string
                    JsonDocument jsonDocument = JsonDocument.Parse(requestApiKey);

                    // Access the root object
                    JsonElement root = jsonDocument.RootElement;

                    // Check if the "apiKey" property exists
                    if (root.TryGetProperty("apiKey", out JsonElement apiKeyElement))
                    {
                        // Get the value of the "apiKey" property
                        requestApiKey = apiKeyElement.GetString();

                        // Now you have the apiKey variable containing the extracted API key
                        Console.WriteLine($"Extracted API Key: {apiKey}");
                    }
                    else
                    {
                        Console.WriteLine("No 'apiKey' property found in the JSON string.");
                        return "API key is null or empty";
                    }
                    if (requestApiKey != apiKey)
                    {
                        Console.WriteLine("Denied");
                        return "Invalid API key";
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error parsing JSON: {ex.Message}");
                    return "Error with JSON";
                }
            }
            else
            {
                Console.WriteLine("requestApiKey is null or empty.");
                return "API key is null or empty";
            }
            return "";
        }

        public async Task<string> autApi(string autApiKey, int userId) {
            var apiKeyEntity = await _context.ApiKeys.FirstOrDefaultAsync(a => a.UserId == userId);

            if (apiKeyEntity == null)
            {
                return "API key not found for the user";
            }

            // Проверка наличия API-ключа
            if (string.IsNullOrEmpty(autApiKey) || autApiKey != apiKeyEntity.ApiKeyString)
            {
                return "Invalid API key";
            }
            return "";
        }

        public async Task<string> UploadImageAsync(FileDescription fileDescription)
        {
            var uploadParams = new ImageUploadParams
            {
                File = fileDescription
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            return uploadResult.Url.ToString();
        }

        public byte[] DownloadImage(string assetId)
        {
            var result = _cloudinary.GetResourceByAssetId(assetId);

            if (result == null)
            {
                return null;
            }

            var imageUrl = result.Url;

            using (var httpClient = new HttpClient())
            {
                return httpClient.GetByteArrayAsync(imageUrl).Result;
            }
        }
    }
}
