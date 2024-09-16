using Microsoft.AspNetCore.Mvc;

namespace EatCareService.Services
{
    public interface IDataService
    {
        Task<IActionResult> GetData(string requestApiKey);
        Task<IActionResult> VerifyPassword(int userId, string enteredPassword, string requestApiKey);
        Task<IActionResult> EnterAsGuest(string ipAddress, string requestApiKey);
        Task<IActionResult> GetDislikedIngredients(int userId, string apiKey);
    }
}
