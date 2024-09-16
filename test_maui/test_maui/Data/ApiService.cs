using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using test_maui.Modules;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using test_maui.Data;
using EatCareService.Modules;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<List<UserDTO>> GetAllUsersAsync(string apiKey)
    {
        try
        {
            string apiUrl = "https://localhost:7024/api/Data";

            // Assuming apiKey is the API key you want to pass
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            return await HandleApiResponse<List<UserDTO>>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }


    public async Task<string> VerifyPasswordAsync(int userId, string enteredPassword, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/VerifyPassword?userId={userId}&enteredPassword={enteredPassword}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                // Используйте ReadAsStringAsync для чтения содержимого как строки
                string userApiKey = await response.Content.ReadAsStringAsync();
                return userApiKey;
            }
            else
            {
                // В случае неудачи возвращаем null или другое значение, чтобы обозначить ошибку
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public async Task<Guest> EnterAsGuestAsync(string ipAddress, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/EnterAsGuest?ipAddress={ipAddress}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, null);
            return await HandleApiResponse<Guest>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<FoodIngredient>> GetDislikedIngredientsAsync(int userId, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/DislikedIngredients/{userId}?apiKey={apiKey}";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            return await HandleApiResponse<List<FoodIngredient>>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }


    public async Task<bool> DeleteGuestAsync(int guestId, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/DeleteGuest/{guestId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            HttpResponseMessage response = await _httpClient.DeleteAsync(apiUrl);
            return await HandleApiResponse<bool>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(int userId, string apiKey, string password)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/DeleteUser/{userId}";

            // Prepare data for the request
            var authenticationData = new AuthenticationData
            {
                UserId = userId,
                ApiKey = apiKey,
                Password = password
            };

            // Convert data to JSON
            var jsonContent = new StringContent(JsonConvert.SerializeObject(authenticationData), Encoding.UTF8, "application/json");

            // Create a custom request with DELETE method
            var request = new HttpRequestMessage(HttpMethod.Delete, apiUrl)
            {
                Content = jsonContent
            };

            // Send the request
            HttpResponseMessage response = await _httpClient.SendAsync(request);

            return await HandleApiResponse<bool>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public async Task<User> UpdateUserPropertyAsync(int userId, UserDetailUpdateModel updateModel, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/UpdateUserProperty/{userId}?apiKey={apiKey}";
            string json = JsonConvert.SerializeObject(updateModel);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PatchAsync(apiUrl, content);
            return await HandleApiResponse<User>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }


    private async Task<T> HandleApiResponse<T>(HttpResponseMessage response)
    {
        try
        {
            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();

                if (typeof(T) == typeof(bool) && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created))
                {
                    return (T)(object)true;
                }
                return JsonConvert.DeserializeObject<T>(responseData);
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return default; // или выбросьте исключение, в зависимости от вашей логики обработки ошибок
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during JSON deserialization: {ex.Message}");
            throw; // Прокиньте исключение дальше для дальнейшей обработки
        }
    }


    public async Task<bool> UpdateUserIntolerancesAsync(int userId, List<int> intolerances, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/UpdateUserIntolerances/{userId}?apiKey={apiKey}";
            string json = JsonConvert.SerializeObject(intolerances);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PatchAsync(apiUrl, content);
            
            return await HandleApiResponse<bool>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public async Task<List<FoodIngredient>> GetAllIngredientsAsync()
    {
        try
        {
            string apiUrl = "https://localhost:7024/api/Data/AllIngredients";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            return await HandleApiResponse<List<FoodIngredient>>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> CreateUserAsync(string username, string password, string apiKey)
    {
        try
        {
            string apiUrl = "https://localhost:7024/api/Data/CreateUser";
            var user = new
            {
                Username = username,
                Password = password
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            string json = JsonConvert.SerializeObject(user);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            return await HandleApiResponse<bool>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }


    public async Task<ImageData> GetRandomTextImageAsync()
    {
        try
        {
            string apiUrl = "https://localhost:7024/api/Data/GenerateRandomTextImage";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                // Получение данных изображения из ответа API в виде строки JSON
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Десериализация JSON в объект ImageResponse
                ImageResponse imageResponse = JsonConvert.DeserializeObject<ImageResponse>(jsonResponse);

                // Преобразование строки Base64 в байты изображения
                byte[] imageBytes = Convert.FromBase64String(imageResponse.Image);

                // Вывод метаданных (пример)
                foreach (var metadata in imageResponse.Metadata)
                {
                    Console.WriteLine($"{metadata.Key}: {metadata.Value}");
                }

                // Возвращение объекта, содержащего байтовый массив и другие данные
                return new ImageData
                {
                    ImageBytes = imageBytes,
                    Metadata = imageResponse.Metadata
                };
            }
            else
            {
                Console.WriteLine($"Error: {response.ReasonPhrase}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public async Task<string> VerifyTicketAndSendApiKeyAsync(Ticket verificationModel)
    {
        try
        {
            string apiUrl = "https://localhost:7024/api/Data/VerifyTicketAndSendApiKey";
            string json = JsonConvert.SerializeObject(verificationModel);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string apiKey = await response.Content.ReadAsStringAsync();
                return apiKey;
            }
            else
            {
                // В случае неудачи возвращаем null или другое значение, чтобы обозначить ошибку
                Console.WriteLine($"Error: {response.StatusCode}");
                return "";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return "";
        }
    }

    public async Task<List<RestaurantData>> GetAllRestaurantsAsync(string apiKey)
    {
        try
        {
            string apiUrl = "https://localhost:7024/api/Data/GetAllRestaurants";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            return await HandleApiResponse<List<RestaurantData>>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public async Task<double> GetUserRating(int userId, int restaurantId, string userApiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/GetUserRating/{userId}/{restaurantId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", userApiKey);

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                if (double.TryParse(result, out double rating))
                {
                    return rating; // Возвращаем оценку пользователя
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return -1; // Если что-то пошло не так, возвращаем -1
    }


    public async Task<string> RateRestaurantAsync(int userId, int restaurantId, double ratingValue, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/RateRestaurant";
            var ratingInputModel = new RatingInputModel
            {
                UserId = userId,
                ItemId = restaurantId,
                RatingValue = ratingValue
            };

            // Assuming apiKey is the API key you want to pass
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            string json = JsonConvert.SerializeObject(ratingInputModel);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
            else
            {
                string errorMessage = $"Error: {response.StatusCode}";
                Console.WriteLine(errorMessage);
                return errorMessage;
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Error: {ex.Message}";
            Console.WriteLine(errorMessage);
            return errorMessage;
        }
    }

    public async Task<string> RateDishAsync(int userId, int dishId, double ratingValue, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/RateDish";
            var ratingInputModel = new RatingInputModel
            {
                UserId = userId,
                ItemId = dishId,
                RatingValue = ratingValue
            };

            // Assuming apiKey is the API key you want to pass
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            string json = JsonConvert.SerializeObject(ratingInputModel);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public async Task<double?> GetUserDishRatingAsync(int userId, int dishId, string userApiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/GetUserDishRating/{userId}/{dishId}";

            // Assuming userApiKey is the API key you want to pass
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", userApiKey);

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<double?>(result);
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public async Task<RatingResult> GetAverageRatingForRestaurant(int restaurantId, int userId, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/AverageRatingForRestaurant/{restaurantId}?userId={userId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                // Используйте ReadAsStringAsync для чтения содержимого как строки JSON
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Десериализация JSON в объект RatingResult
                RatingResult result = JsonConvert.DeserializeObject<RatingResult>(jsonResponse);

                return result;
            }
            else
            {
                Console.WriteLine($"Error: {response.ReasonPhrase}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public async Task<RatingResult> GetAverageRatingForDish(int dishId, int userId, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/AverageRatingForDish/{dishId}?userId={userId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                // Используйте ReadAsStringAsync для чтения содержимого как строки JSON
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Десериализация JSON в объект RatingResult
                RatingResult result = JsonConvert.DeserializeObject<RatingResult>(jsonResponse);

                return result;
            }
            else
            {
                Console.WriteLine($"Error: {response.ReasonPhrase}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public async Task<CommentDTO> CreateCommentAsync(CommentDTO commentDTO, string apiKey)
    {
        try
        {
            string apiUrl = "https://localhost:7024/api/Data/CreateComment";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            string json = JsonConvert.SerializeObject(commentDTO);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            return await HandleApiResponse<CommentDTO>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateCommentAsync(int id, CommentDTO commentDTO, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/UpdateComment/{id}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            string json = JsonConvert.SerializeObject(commentDTO);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PutAsync(apiUrl, content);
            return await HandleApiResponse<bool>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteCommentAsync(int id, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/DeleteComment/{id}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            HttpResponseMessage response = await _httpClient.DeleteAsync(apiUrl);
            return await HandleApiResponse<bool>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public async Task<List<CommentDTO>> GetCommentsByRestaurantAsync(int restaurantId, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/GetCommentsByRestaurant/{restaurantId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            return await HandleApiResponse<List<CommentDTO>>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public async Task<DishCommentDTO> CreateDishCommentAsync(DishCommentDTO commentDTO, string apiKey)
    {
        try
        {
            string apiUrl = "https://localhost:7024/api/Data/CreateDishComment";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            string json = JsonConvert.SerializeObject(commentDTO);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
            return await HandleApiResponse<DishCommentDTO>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateDishCommentAsync(int id, DishCommentDTO commentDTO, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/UpdateDishComment/{id}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            string json = JsonConvert.SerializeObject(commentDTO);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PutAsync(apiUrl, content);
            return await HandleApiResponse<bool>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteDishCommentAsync(int id, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/DeleteDishComment/{id}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            HttpResponseMessage response = await _httpClient.DeleteAsync(apiUrl);
            return await HandleApiResponse<bool>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }

    public async Task<List<DishCommentDTO>> GetCommentsByDishAsync(int dishId, string apiKey)
    {
        try
        {
            string apiUrl = $"https://localhost:7024/api/Data/GetCommentsByDish/{dishId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("requestApiKey", apiKey);

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            return await HandleApiResponse<List<DishCommentDTO>>(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }


    public class ImageResponse
    {
        public string Image { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class RestaurantData
    {
        public RestaurantDetails Restaurant { get; set; }
        public byte[] ImageContent { get; set; }
    }

    public class RestaurantDetails
    {
        public int Id { get; set; }
        public string RestaurantName { get; set; }
        public List<FoodItemData> FoodItems { get; set; }
        public double Ratings { get; set; }
    }

    public class FoodItemData
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public double Price { get; set; }
        public string FoodType { get; set; }
        public double Rating { get; set; }
        public List<string> Ingredients { get; set; }
    }



    // Добавьте класс ImageData для объединения байтового массива и других данных
    public class ImageData
    {
        public byte[] ImageBytes { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class UserDetailUpdateModel
    {
        public string PropertyName { get; set; }
        public string NewValue { get; set; }
    }

    public class Ticket
    {
        public Guid Id { get; set; }
        public string RandomText { get; set; }
    }
}
