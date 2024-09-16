using EatCareService.Data;
using EatCareService.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EatCareService.Services
{
    public class DataService : IDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEatCareServices _eatCareServices;
        private readonly string apiKey = "fWK5bASE153KnTH3Cyuyrwjl6AzSVy1ZYAWYhYQPEZ7mL1PRXYaT0lkYeqzDkUmm2ZX3yISkCIupBk7Pl6IahBmSbmDksOxdPdE1IfehVGvBG6yTWYWABq0n6VXAvLkM";

        public DataService(ApplicationDbContext context, IEatCareServices eatCareServices)
        {
            _context = context;
            _eatCareServices = eatCareServices;
        }

        public async Task<IActionResult> GetData(string requestApiKey)
        {
            string response = _eatCareServices.verifyGlobalApi(requestApiKey);

            if (response != "")
            {
                Console.WriteLine(response);
                return new UnauthorizedObjectResult(new { error = response });
            }

            Console.WriteLine("Data request");
            var users = await _context.Users
                .Select(u => new UserDTO { Id = u.Id, Username = u.Username })
                .ToListAsync();

            Console.WriteLine("All OK");
            return new OkObjectResult(users);
        }

        public async Task<IActionResult> VerifyPassword(int userId, string enteredPassword, string requestApiKey)
        {
            string response = _eatCareServices.verifyGlobalApi(requestApiKey);

            if (response != "")
            {
                Console.WriteLine(response);
                return new UnauthorizedObjectResult(new { error = response });
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new NotFoundObjectResult("User not found");
                }

                bool passwordValid = _eatCareServices.VerifyPassword(enteredPassword, user.Password);

                if (passwordValid)
                {
                    var apiKeyRecord = await _context.ApiKeys.FirstOrDefaultAsync(api => api.UserId == userId);

                    if (apiKeyRecord != null)
                    {
                        return new OkObjectResult(apiKeyRecord.ApiKeyString);
                    }
                    else
                    {
                        return new NotFoundObjectResult("API key not found");
                    }
                }
                else
                {
                    return new NotFoundObjectResult("Invalid password");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        public async Task<IActionResult> EnterAsGuest(string ipAddress, string requestApiKey)
        {
            string response = _eatCareServices.verifyGlobalApi(requestApiKey);

            if (response != "")
            {
                Console.WriteLine(response);
                return new UnauthorizedObjectResult(new { error = response });
            }

            try
            {
                var existingGuest = await _context.Guests.FirstOrDefaultAsync(g => g.IPAddress == ipAddress);

                if (existingGuest != null)
                {
                    return new OkObjectResult(existingGuest);
                }
                else
                {
                    var newGuest = new Guest
                    {
                        GuestName = "Guest_" + DateTime.Now.ToString("yyyyMMddHHmmssff"),
                        IPAddress = ipAddress,
                        LastActivity = DateTime.Now
                    };

                    _context.Guests.Add(newGuest);
                    await _context.SaveChangesAsync();

                    return new OkObjectResult(newGuest);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        public async Task<IActionResult> GetDislikedIngredients(int userId, string apiKey)
        {
            try
            {
                string response = await _eatCareServices.autApi(apiKey, userId);

                if (response != "")
                {
                    return new UnauthorizedObjectResult(new { error = response });
                }

                var dislikedIngredients = _context.UserIntolerances
                    .Where(ui => ui.UserId == userId)
                    .Select(ui => ui.Ingredient)
                    .ToList();

                return new OkObjectResult(dislikedIngredients);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
