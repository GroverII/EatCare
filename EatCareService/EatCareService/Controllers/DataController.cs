using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;
using EatCareService.Data;
using EatCareService.Modules;
using EatCareService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System.Security.Cryptography;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly string apiKey = "fWK5bASE153KnTH3Cyuyrwjl6AzSVy1ZYAWYhYQPEZ7mL1PRXYaT0lkYeqzDkUmm2ZX3yISkCIupBk7Pl6IahBmSbmDksOxdPdE1IfehVGvBG6yTWYWABq0n6VXAvLkM";
    private static readonly List<Ticket> Tickets = new List<Ticket>();
    private readonly Cloudinary _cloudinary;
    private readonly IEatCareServices _eatCareServices;
    private readonly IDataService _dataService;

    public DataController(ApplicationDbContext context, Cloudinary cloudinary, IEatCareServices eatCareServices, IDataService dataService)
    {
        _context = context;
        _cloudinary = cloudinary;
        _eatCareServices = eatCareServices;
        _dataService = dataService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Invalid file");
            }

            var fileDescription = new FileDescription(file.FileName, file.OpenReadStream());
            var imageUrl = await _eatCareServices.UploadImageAsync(fileDescription);

            return Ok(new { imageUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }


    // Метод для скачивания изображения
    [HttpGet("download")]
    public IActionResult DownloadImage(string assetId)
    {
        try
        {
            var imageBytes = _eatCareServices.DownloadImage(assetId);

            if (imageBytes == null)
            {
                return NotFound($"Image with Asset ID '{assetId}' not found.");
            }

            return File(imageBytes, "image/jpeg");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        string? requestApiKey = HttpContext.Request.Headers["requestApiKey"];

        return await _dataService.GetData(requestApiKey);
    }

    [HttpGet("VerifyPassword")]
    public async Task<IActionResult> VerifyPassword(int userId, string enteredPassword)
    {
        string requestApiKey = HttpContext.Request.Headers["requestApiKey"];

        return await _dataService.VerifyPassword(userId, enteredPassword, requestApiKey);
    }

    [HttpPost("EnterAsGuest")]
    public async Task<IActionResult> EnterAsGuest(string ipAddress)
    {
        string requestApiKey = HttpContext.Request.Headers["requestApiKey"];

        return await _dataService.EnterAsGuest(ipAddress, requestApiKey);
    }

    [HttpGet("DislikedIngredients/{userId}")]
    public async Task<IActionResult> GetDislikedIngredients(int userId, [FromQuery] string apiKey)
    {
        return await _dataService.GetDislikedIngredients(userId, apiKey);
    }

    [HttpPatch("UpdateUserProperty/{userId}")]
    public async Task<IActionResult> UpdateUserProperty(int userId, [FromBody] UserDetailUpdateModel updateModel, [FromQuery] string apiKey)
    {
        try
        {
            string response = await _eatCareServices.autApi(apiKey, userId);

            if (response != "") {
                return Unauthorized(new { error = response });
            }

            var userInDatabase = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (userInDatabase != null)
            {
                // Обновление значения свойства в объекте пользователя
                switch (updateModel.PropertyName)
                {
                    case "Username":
                        userInDatabase.Username = updateModel.NewValue ?? throw new ArgumentNullException(nameof(updateModel.NewValue));
                        break;

                    case "Password":
                        if (updateModel.NewValue != null)
                        {
                            userInDatabase.Password = _eatCareServices.HashPassword(updateModel.NewValue);
                        }
                        else
                        {
                            return BadRequest(new { error = "Invalid Password value" });
                        }
                        break;

                    case "Type":
                        if (int.TryParse(updateModel.NewValue, out int newValue))
                        {
                            userInDatabase.Type = newValue;
                        }
                        else
                        {
                            return BadRequest(new { error = "Invalid Type value" });
                        }
                        break;

                    // Добавьте другие свойства, которые вы хотите обновить

                    default:
                        return BadRequest(new { error = "Invalid property name" });
                }

                await _context.SaveChangesAsync();

                return Ok(userInDatabase);
            }
            else
            {
                return NotFound(new { error = "User not found" });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Internal Server Error: {ex.Message}" });
        }
    }


    [HttpDelete("DeleteGuest/{guestId}")]
    public async Task<IActionResult> DeleteGuest(int guestId)
    {
        string? requestApiKey = HttpContext.Request.Headers["requestApiKey"]; // Assuming the hashed API key is in the request headers

        string response = _eatCareServices.verifyGlobalApi(requestApiKey);
        if (response != "")
        {
            Console.WriteLine(response);
            return Unauthorized(new { error = response });
        }

        try
        {
            var guestInDatabase = await _context.Guests.FirstOrDefaultAsync(g => g.Id == guestId);

            if (guestInDatabase != null)
            {
                _context.Guests.Remove(guestInDatabase);
                await _context.SaveChangesAsync();

                return Ok(new { message = true });
            }
            else
            {
                return NotFound(new { error = false });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = false });
        }
    }

    [HttpPatch("UpdateUserIntolerances/{userId}")]
    public async Task<IActionResult> UpdateUserIntolerances(int userId, [FromBody] List<int> selectedIngredientIds, string apiKey)
    {
        try
        {
            string response = await _eatCareServices.autApi(apiKey, userId);

            if (response != "")
            {
                return Unauthorized(new { error = response });
            }
            // Получаем существующие записи об ингредиентах для данного пользователя
            var existingIntolerances = await _context.UserIntolerances
                .Where(ui => ui.UserId == userId)
                .ToListAsync();

            // Получаем записи, которые нужно удалить
            var intolerancesToRemove = existingIntolerances
                .Where(ui => !selectedIngredientIds.Contains(ui.IngredientId))
                .ToList();

            // Получаем записи, которые нужно добавить
            var intolerancesToAdd = selectedIngredientIds
                .Where(id => !existingIntolerances.Any(ui => ui.IngredientId == id))
                .Select(id => new UserIntolerance
                {
                    UserId = userId,
                    IngredientId = id
                })
                .ToList();

            // Удаляем записи, которые не выбраны
            _context.UserIntolerances.RemoveRange(intolerancesToRemove);

            // Добавляем новые записи в контекст базы данных
            _context.UserIntolerances.AddRange(intolerancesToAdd);

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            return Ok(new { message = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = false });
        }
    }

    [HttpGet("AllIngredients")]
    public IActionResult GetAllIngredients()
    {
        try
        {
            var ingredients = _context.FoodIngredients.ToList();
            return Ok(ingredients);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("CreateUser")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserModel createUserModel)
    {
        string? requestApiKey = HttpContext.Request.Headers["requestApiKey"]; // Assuming the hashed API key is in the request headers

        string response = _eatCareServices.verifyGlobalApi(requestApiKey);
        if (response != "")
        {
            Console.WriteLine(response);
            return Unauthorized(new { error = response });
        }

        try
        {
            // Проверка наличия пользователя с таким именем
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == createUserModel.Username);

            if (existingUser != null)
            {
                return BadRequest(new { error = false });
            }

            // Создание нового пользователя
            var newUser = new User
            {
                Username = createUserModel.Username!,
                Password = createUserModel.Password!, // Хешированный пароль
                Type = 0
            };

            // Добавление нового пользователя в контекст данных и сохранение изменений
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Генерация и сохранение API-ключа для нового пользователя
            string apiKey = _eatCareServices.GenerateApiKey(newUser.Id);

            var apiKeyRecord = new ApiKey
            {
                UserId = newUser.Id,
                ApiKeyString = apiKey
            };

            _context.ApiKeys.Add(apiKeyRecord);
            await _context.SaveChangesAsync();

            return Ok(new { message = true, apiKey });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, new { error = false });
        }
    }

    [HttpGet("GenerateRandomTextImage")]
    public IActionResult GenerateRandomTextImage()
    {
        try
        {
            // Генерируем случайный текст
            string randomText = _eatCareServices.GenerateRandomText();

            // Создаем изображение с текстом
            using (var surface = SKSurface.Create(new SKImageInfo(300, 100)))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                // Устанавливаем параметры для текста
                var paint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    TextAlign = SKTextAlign.Center
                };

                paint.TextSize = _eatCareServices.CalculateOptimalTextSize(canvas, paint, randomText);

                // Рисуем текст на изображении в центре
                var textPosition = new SKPoint(canvas.LocalClipBounds.MidX, canvas.LocalClipBounds.MidY);
                canvas.DrawText(randomText, textPosition, paint);

                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(), // Используем GUID как уникальный идентификатор тикета
                    RandomText = randomText,
                };

                Tickets.Add(ticket);

                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 100))
                {
                    // Получаем массив байт изображения
                    var imageData = data.ToArray();

                    // Добавляем Id в метаданные
                    var metadata = new Dictionary<string, string>
                    {
                        { "Id", ticket.Id.ToString() }
                    };

                    // Добавляем метаданные к изображению
                    var result = new
                    {
                        Image = Convert.ToBase64String(imageData),
                        Metadata = metadata
                    };

                    // Преобразуем результат в JSON и возвращаем его
                    return Ok(result);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }


    [HttpPost("VerifyTicketAndSendApiKey")]
    [AllowAnonymous]
    public IActionResult VerifyTicketAndSendApiKey([FromBody] Ticket verificationModel)
    {
        try
        {
            // Поиск тикета в массиве по Id
            var ticket = Tickets.FirstOrDefault(t => t.Id == verificationModel.Id);

            if (ticket == null)
            {
                return NotFound(new { error = "Ticket not found" });
            }

            // Проверка совпадения тикета
            if (ticket.RandomText != verificationModel.RandomText)
            {
                return BadRequest(new { error = "Ticket verification failed" });
            }

            return Ok(new { apiKey });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpDelete("DeleteUser/{userId}")]
    public async Task<IActionResult> DeleteUser(int userId, [FromBody] AuthenticationData authenticationData)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound($"User with ID {userId} not found");
        }

        // Проверьте аутентификацию
        bool isAuthenticationValid = await _eatCareServices.VerifyAuthentication(authenticationData, user);

        if (!isAuthenticationValid)
        {
            return Unauthorized("Invalid authentication");
        }

        // Manually delete associated records in user_intolerances
        var intolerances = await _context.UserIntolerances
            .Where(ui => ui.UserId == userId)
            .ToListAsync();

        _context.UserIntolerances.RemoveRange(intolerances);

        // Proceed with user deletion
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok($"User with ID {userId} deleted successfully");
    }

    [HttpGet("GetMenu/{restaurantId}")]
    public async Task<IActionResult> GetMenu(int restaurantId)
    {
        try
        {
            var restaurantInfo = await _context.Restaurants
                .Where(r => r.Id == restaurantId)
                .Include(r => r.FoodItems)
                    .ThenInclude(fi => fi.FoodType)
                .Select(r => new
                {
                    RestaurantName = r.RestaurantName,
                    AssetId = r.AssetId,
                    FoodItems = r.FoodItems.Select(fi => new
                    {
                        ItemName = fi.ItemName,
                        Price = fi.Price,
                        FoodType = fi.FoodType.TypeName,
                        Ingredients = _context.FoodItemIngredients
                            .Where(fii => fii.FoodItemId == fi.Id)
                            .Select(fii => fii.Ingredient.IngredientName)
                            .ToList(),
                        Ratings = _context.DishRatings
                            .Where(rating => rating.DishId == fi.Id)
                            .Select(rating => rating.RatingValue)
                            .ToList()
                    })
                })
                .FirstOrDefaultAsync();

            if (restaurantInfo == null)
            {
                return NotFound();
            }

            // Get resource information by Asset ID
            var imageResult = _cloudinary.GetResourceByAssetId(restaurantInfo.AssetId);

            // Download the image content
            byte[]? imageBytes = null;
            using (var httpClient = new HttpClient())
            {
                imageBytes = await httpClient.GetByteArrayAsync(imageResult.Url);
            }

            // Return the image content with the appropriate content type
            var imageContentResult = new FileContentResult(imageBytes, $"image/{imageResult.Format.ToLower()}")
            {
                FileDownloadName = $"{restaurantInfo.RestaurantName}_Menu.{imageResult.Format.ToLower()}"
            };

            // Combine other information with the image content
            var menu = new
            {
                restaurantInfo.RestaurantName,
                restaurantInfo.FoodItems
            };

            return Ok(new { Menu = menu, ImageContent = imageContentResult });
        }
        catch (Exception ex)
        {
            // Handle exceptions and return an error response
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }


    [HttpGet("GetAllRestaurants")]
    public async Task<IActionResult> GetAllRestaurants()
    {
        try
        {
            string? requestApiKey = HttpContext.Request.Headers["requestApiKey"]; // Assuming the hashed API key is in the request headers

            string response = _eatCareServices.verifyGlobalApi(requestApiKey);
            if (response != "")
            {
                Console.WriteLine(response);
                return Unauthorized(new { error = response });
            }

            var result = await _context.Restaurants
                .Include(r => r.FoodItems)
                    .ThenInclude(fi => fi.FoodType)
                .Select(r => new
                {
                    r.Id, // Добавляем Id ресторана
                    r.RestaurantName,
                    r.AssetId,
                    FoodItems = r.FoodItems.Select(fi => new
                    {
                        fi.Id,
                        fi.ItemName,
                        fi.Price,
                        FoodType = fi.FoodType.TypeName,
                        Ingredients = _context.FoodItemIngredients
                            .Where(fii => fii.FoodItemId == fi.Id)
                            .Select(fii => fii.Ingredient.IngredientName)
                            .ToList()
                    }),
                    Ratings = _context.RestaurantRatings
                        .Where(rating => rating.RestaurantId == r.Id)
                        .Select(rating => rating.RatingValue)
                        .DefaultIfEmpty()
                        .Average()
                })
                .ToListAsync();

            // For each restaurant, get image information and download image content
            var restaurantsWithImages = new List<object>();
            foreach (var restaurantInfo in result)
            {
                var imageResult = _cloudinary.GetResourceByAssetId(restaurantInfo.AssetId);

                using (var httpClient = new HttpClient())
                {
                    var imageBytes = await httpClient.GetByteArrayAsync(imageResult.Url);

                    var restaurantWithImage = new
                    {
                        restaurantInfo.Id, // Добавляем Id ресторана
                        restaurantInfo.RestaurantName,
                        restaurantInfo.FoodItems,
                        restaurantInfo.Ratings
                    };

                    restaurantsWithImages.Add(new { Restaurant = restaurantWithImage, ImageContent = imageBytes });
                }
            }

            return Ok(restaurantsWithImages);
        }
        catch (Exception ex)
        {
            // Handle exceptions and return an error response
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("GetUserRating/{userId}/{restaurantId}")]
    public async Task<IActionResult> GetUserRating(int userId, int restaurantId)
    {
        try
        {
            string? requestApiKey = HttpContext.Request.Headers["requestApiKey"];

            string response = await _eatCareServices.autApi(requestApiKey, userId);

            if (response != "")
            {
                return Unauthorized(new { error = response });
            }

            var rating = await _context.RestaurantRatings
                .Where(r => r.UserId == userId && r.RestaurantId == restaurantId)
                .Select(r => r.RatingValue)
                .FirstOrDefaultAsync();

            return Ok(rating);
        }
        catch (Exception ex)
        {
            // Handle exceptions and return an error response
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("GetUserDishRating/{userId}/{DishId}")]
    public async Task<IActionResult> GetUserDishRating(int userId, int DishId)
    {
        try
        {
            string? requestApiKey = HttpContext.Request.Headers["requestApiKey"];

            string response = await _eatCareServices.autApi(requestApiKey, userId);

            if (response != "")
            {
                return Unauthorized(new { error = response });
            }

            var rating = await _context.DishRatings
                .Where(r => r.UserId == userId && r.DishId == DishId)
                .Select(r => r.RatingValue)
                .FirstOrDefaultAsync();

            return Ok(rating);
        }
        catch (Exception ex)
        {
            // Handle exceptions and return an error response
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("GetFoodItem/{foodItemId}")]
    public async Task<IActionResult> GetFoodItem(int foodItemId)
    {
        try
        {
            var foodItem = await _context.FoodItems
                .Include(fi => fi.FoodType)
                .Where(fi => fi.Id == foodItemId)
                .Select(fi => new
                {
                    fi.ItemName,
                    fi.Price,
                    FoodType = fi.FoodType.TypeName,
                    Ingredients = _context.FoodItemIngredients
                        .Where(fii => fii.FoodItemId == fi.Id)
                        .Select(fii => fii.Ingredient.IngredientName)
                        .ToList(),
                    Ratings = _context.DishRatings
                        .Where(rating => rating.DishId == foodItemId)
                        .Select(rating => rating.RatingValue)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (foodItem == null)
            {
                return NotFound();
            }

            return Ok(foodItem);
        }
        catch (Exception ex)
        {
            // Handle exceptions and return an error response
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }


    // POST: api/RestaurantRatings/RateRestaurant
    [HttpPost("RateRestaurant")]
    public async Task<IActionResult> RateRestaurant([FromBody] RatingInputModel inputModel)
    {
        try
        {
            var user = await _context.Users.FindAsync(inputModel.UserId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var restaurant = await _context.Restaurants.FindAsync(inputModel.ItemId);

            if (restaurant == null)
            {
                return NotFound("Restaurant not found");
            }

            // Check if the user has already rated this restaurant
            var existingRating = await _context.RestaurantRatings
                .FirstOrDefaultAsync(r => r.UserId == inputModel.UserId && r.RestaurantId == inputModel.ItemId);

            if (existingRating != null)
            {
                // Update existing rating
                existingRating.RatingValue = inputModel.RatingValue;
                existingRating.Date = DateTime.Now;
                _context.RestaurantRatings.Update(existingRating);
            }
            else
            {
                // Create a new rating
                var newRating = new RestaurantRating
                {
                    UserId = inputModel.UserId,
                    RestaurantId = inputModel.ItemId,
                    RatingValue = inputModel.RatingValue,
                    Date = DateTime.Now
                };

                _context.RestaurantRatings.Add(newRating);
            }

            await _context.SaveChangesAsync();

            return Ok("Rating successfully updated/added");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
        }
    }

    [HttpPost("RateDish")]
    public async Task<IActionResult> RateDish([FromBody] RatingInputModel inputModel)
    {
        try
        {
            var user = await _context.Users.FindAsync(inputModel.UserId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var foodItem = await _context.FoodItems.FindAsync(inputModel.ItemId);

            if (foodItem == null)
            {
                return NotFound("Dish not found");
            }

            // Check if the user has already rated this dish
            var existingRating = await _context.DishRatings
                .FirstOrDefaultAsync(r => r.UserId == inputModel.UserId && r.DishId == inputModel.ItemId);

            if (existingRating != null)
            {
                // Update existing rating
                existingRating.RatingValue = inputModel.RatingValue;
                existingRating.Date = DateTime.Now;
                _context.DishRatings.Update(existingRating);
            }
            else
            {
                // Create a new rating
                var newRating = new DishRating
                {
                    UserId = inputModel.UserId,
                    DishId = inputModel.ItemId,
                    RatingValue = inputModel.RatingValue,
                    Date = DateTime.Now
                };

                _context.DishRatings.Add(newRating);
            }

            await _context.SaveChangesAsync();

            return Ok("Rating successfully updated/added");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("AverageRatingForRestaurant/{restaurantId}")]
    public async Task<ActionResult<RatingResult>> GetAverageRatingForRestaurant(int restaurantId, int userId)
    {
        var ratings = await _context.RestaurantRatings
            .Where(r => r.RestaurantId == restaurantId)
            .ToListAsync();

        if (ratings.Any())
        {
            var averageRating = ratings.Average(r => r.RatingValue);
            var userRating = ratings.FirstOrDefault(r => r.UserId == userId)?.RatingValue;
            var numberOfRatings = ratings.Count;

            var result = new RatingResult
            {
                AverageRating = averageRating,
                UserRating = userRating,
                NumberOfRatings = numberOfRatings
            };

            return Ok(result);
        }
        else
        {
            return NotFound($"No ratings found for restaurant with ID {restaurantId}");
        }
    }

    [HttpGet("AverageRatingForDish/{dishId}")]
    public async Task<ActionResult<RatingResult>> GetAverageRatingForDish(int dishId, int userId)
    {
        var ratings = await _context.DishRatings
            .Where(d => d.DishId == dishId)
            .ToListAsync();

        if (ratings.Any())
        {
            var averageRating = ratings.Average(d => d.RatingValue);
            var userRating = ratings.FirstOrDefault(d => d.UserId == userId)?.RatingValue;
            var numberOfRatings = ratings.Count;

            var result = new RatingResult
            {
                AverageRating = averageRating,
                UserRating = userRating,
                NumberOfRatings = numberOfRatings
            };

            return Ok(result);
        }
        else
        {
            return NotFound($"No ratings found for dish with ID {dishId}");
        }
    }

    [HttpPost("CreateComment")]
    public async Task<IActionResult> CreateComment(CommentDTO commentDTO)
    {
        if (commentDTO.ParentCommentId != -1)
        {
            // Проверяем существование родительского комментария
            var parentCommentExists = await _context.RestaurantsComments.AnyAsync(c => c.Id == commentDTO.ParentCommentId);

            if (!parentCommentExists)
            {
                return BadRequest("Родительский комментарий не найден");
            }
        }

        var comment = new RestaurantsComments
        {
            RestaurantsId = commentDTO.RestaurantsId,
            UserId = commentDTO.UserId,
            ParentCommentId = commentDTO.ParentCommentId,
            CommentText = commentDTO.CommentText
        };

        _context.RestaurantsComments.Add(comment);
        await _context.SaveChangesAsync();


        return Ok(comment);
    }

    [HttpPut("UpdateComment/{id}")]
    public async Task<IActionResult> UpdateComment(int id, CommentDTO commentDTO)
    {
        var comment = await _context.RestaurantsComments.FindAsync(id);

        if (comment == null)
        {
            return NotFound();
        }

        // Устанавливаем новый текст комментария
        comment.CommentText = commentDTO.CommentText;

        await _context.SaveChangesAsync();

        return Ok(true);
    }

    [HttpDelete("DeleteComment/{id}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await _context.RestaurantsComments.FindAsync(id);

        if (comment == null)
        {
            return NotFound();
        }

        // Проверяем, есть ли зависимые комментарии
        var dependentCommentsExist = await _context.RestaurantsComments.AnyAsync(c => c.ParentCommentId == id);

        if (!dependentCommentsExist)
        {
            // Удалить комментарий из контекста
            _context.RestaurantsComments.Remove(comment);
        }
        else
        {
            // Заменяем текст комментария на 'User deleted comment' и user_id на 12
            comment.CommentText = "User deleted comment";
            comment.UserId = 12;
        }

        await _context.SaveChangesAsync();

        return Ok(comment);
    }

    [HttpGet("GetCommentsByRestaurant/{restaurantId}")]
    public async Task<IActionResult> GetCommentsByRestaurant(int restaurantId)
    {
        var comments = await _context.RestaurantsComments
            .Where(c => c.RestaurantsId == restaurantId)
            .Select(c => new
            {
                c.Id,
                c.RestaurantsId,
                c.UserId,
                c.ParentCommentId,
                c.CommentText
            })
            .ToListAsync();

        if (comments == null || comments.Count == 0)
        {
            return NotFound("Комментарии для данного ресторана не найдены");
        }

        return Ok(comments);
    }

    [HttpPost("CreateDishComment")]
    public async Task<IActionResult> CreateDishComment(DishCommentDTO commentDTO)
    {
        if (commentDTO.ParentCommentId != -1)
        {
            // Проверяем существование родительского комментария
            var parentCommentExists = await _context.DishComments.AnyAsync(c => c.Id == commentDTO.ParentCommentId);

            if (!parentCommentExists)
            {
                return BadRequest("Родительский комментарий не найден");
            }
        }

        var comment = new DishComments
        {
            DishId = commentDTO.DishId,
            UserId = commentDTO.UserId,
            ParentCommentId = commentDTO.ParentCommentId,
            CommentText = commentDTO.CommentText
        };

        _context.DishComments.Add(comment);
        await _context.SaveChangesAsync();

        return Ok(comment);
    }

    [HttpPut("UpdateDishComment/{id}")]
    public async Task<IActionResult> UpdateDishComment(int id, DishCommentDTO commentDTO)
    {
        var comment = await _context.DishComments.FindAsync(id);

        if (comment == null)
        {
            return NotFound();
        }

        // Устанавливаем новый текст комментария
        comment.CommentText = commentDTO.CommentText;

        await _context.SaveChangesAsync();

        return Ok(true);
    }

    [HttpDelete("DeleteDishComment/{id}")]
    public async Task<IActionResult> DeleteDishComment(int id)
    {
        var comment = await _context.DishComments.FindAsync(id);

        if (comment == null)
        {
            return NotFound();
        }

        // Проверяем, есть ли зависимые комментарии
        var dependentCommentsExist = await _context.DishComments.AnyAsync(c => c.ParentCommentId == id);

        if (!dependentCommentsExist)
        {
            // Удалить комментарий из контекста
            _context.DishComments.Remove(comment);
        }
        else
        {
            // Заменяем текст комментария на 'User deleted comment' и user_id на 12
            comment.CommentText = "User deleted comment";
            comment.UserId = 12;
        }

        await _context.SaveChangesAsync();

        return Ok(comment);
    }

    [HttpGet("GetCommentsByDish/{dishId}")]
    public async Task<IActionResult> GetCommentsByDish(int dishId)
    {
        var comments = await _context.DishComments
            .Where(c => c.DishId == dishId)
            .Select(c => new
            {
                c.Id,
                c.DishId,
                c.UserId,
                c.ParentCommentId,
                c.CommentText
            })
            .ToListAsync();

        if (comments == null || comments.Count == 0)
        {
            return NotFound("Комментарии для данного блюда не найдены");
        }

        return Ok(comments);
    }


    public class RatingResult
    {
        public double AverageRating { get; set; }
        public double? UserRating { get; set; }
        public int NumberOfRatings { get; set; }
    }

}
