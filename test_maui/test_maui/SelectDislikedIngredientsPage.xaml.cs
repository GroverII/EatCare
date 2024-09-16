using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using test_maui.Modules;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace test_maui
{
    public partial class SelectDislikedIngredientsPage : ContentPage
    {
        private readonly User user;
        private readonly Guest guest;
        private readonly ApiService apiService;
        private readonly string apiKey;
        private readonly string globalApiKey;

        private List<FoodIngredient> selectedIngredients = null;

        public SelectDislikedIngredientsPage(User user, Guest guest, List<FoodIngredient> allIngredients, List<FoodIngredient> dislikedIngredients, string apiKey, string globalApiKey)
        {
            InitializeComponent();

            this.user = user;
            this.guest = guest;
            this.apiKey = apiKey;
            this.globalApiKey = globalApiKey;
            apiService = new ApiService(); // Инициализируем экземпляр ApiService

            // Создаем объекты IngredientViewModel для всех ингредиентов
            var ingredientViewModels = allIngredients.Select(ingredient => new IngredientViewModel
            {
                Id = ingredient.Id,
                Name = ingredient.IngredientName,
                IsSelected = dislikedIngredients.Any(d => d.Id == ingredient.Id)
            }).ToList();

            // Устанавливаем источник данных для ListView
            listView.ItemsSource = ingredientViewModels;
        }

        async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                if (user != null)
                {
                    // Получаем выбранные ингредиенты
                    var selectedIntolerances = ((List<IngredientViewModel>)listView.ItemsSource)
                        .Where(ingredient => ingredient.IsSelected)
                        .Select(ingredient => ingredient.Id)
                        .ToList();

                    // Создаем модель для отправки на сервер
                    var updateModel = new List<int>(selectedIntolerances);

                    // Вызываем метод ApiService для обновления интолерантностей пользователя
                    bool success = await apiService.UpdateUserIntolerancesAsync(user.Id, updateModel, apiKey);

                    if (success)
                    {
                        // Обработка успешного ответа
                        await DisplayAlert("Success", "User intolerances updated successfully", "OK");
                    }
                    else
                    {
                        // Обработка ошибочного ответа
                        await DisplayAlert("Error", "An error occurred while updating intolerances", "OK");
                    }
                }
                else
                {
                    selectedIngredients = ((List<IngredientViewModel>)listView.ItemsSource)
                        .Where(ingredient => ingredient.IsSelected)
                        .Select(ingredient => new FoodIngredient { IngredientName = ingredient.Name })
                        .ToList();

                    

                    await Navigation.PushAsync(new UserDetailPage(null, guest, selectedIngredients, apiKey, globalApiKey));
                }

            }
            catch (Exception ex)
            {
                // Обработка ошибок при выполнении запроса
                await DisplayAlert("Error", "An error occurred while updating intolerances: " + ex.Message, "OK");
            }
        }

        async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UserDetailPage(user, guest, selectedIngredients, apiKey, globalApiKey));
        }

        void OnIngredientToggled(object sender, ToggledEventArgs e)
        {
            // Здесь можно обрабатывать переключение для сохранения состояния переключателя, если это необходимо
        }
    }

    // ViewModel для ингредиента
    public class IngredientViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
