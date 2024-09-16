using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using test_maui.Modules;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace test_maui
{
    public partial class UserDetailPage : ContentPage
    {
        private readonly User user;
        private readonly Guest guest;
        private readonly string apiKey;
        private readonly string globalApi;
        private ApiService apiService;
        private List<FoodIngredient> dislikedIngredients;
        

        public UserDetailPage(User user, Guest guest, List<FoodIngredient> dislikedIngredients, string apiKey, string globalApi)
        {
            InitializeComponent();

            this.user = user;
            this.guest = guest;
            this.dislikedIngredients = dislikedIngredients;
            this.apiKey = apiKey;
            this.globalApi = globalApi;

            DeleteButton.IsVisible = user != null;

            apiService = new ApiService(); // Create a new instance of ApiService

            RefreshDataAsync();
        }

        private async void RefreshDataAsync()
        {
            // Асинхронные операции могут быть добавлены сюда
            await Task.Yield();


            // Ваш код обновления данных
            var userDetailsList = new List<UserDetailItem>();

            if (user != null)
            {
                dislikedIngredients = await apiService.GetDislikedIngredientsAsync(user.Id, apiKey);
                userDetailsList.Add(new UserDetailItem { PropertyName = "Username:", PropertyValue = user.Username });
                userDetailsList.Add(new UserDetailItem { PropertyName = "Type:", PropertyValue = user.Type.ToString() });
            }

            if (guest != null)
            {
                if(dislikedIngredients == null)
                    dislikedIngredients = new List<FoodIngredient>();
                userDetailsList.Add(new UserDetailItem { PropertyName = "Guest Name:", PropertyValue = guest.GuestName });
                userDetailsList.Add(new UserDetailItem { PropertyName = "IP Address:", PropertyValue = guest.IPAddress });
                userDetailsList.Add(new UserDetailItem { PropertyName = "Last Activity:", PropertyValue = guest.LastActivity.ToString() });
            }

            userDetailsList.Add(new UserDetailItem { PropertyName = "Disliked Ingredients:", PropertyValue = GetDislikedIngredientsText(dislikedIngredients) });

            UserDetailsListView.ItemsSource = new ObservableCollection<UserDetailItem>(userDetailsList);
        }

        private static string GetDislikedIngredientsText(List<FoodIngredient> dislikedIngredients)
        {
            if (dislikedIngredients.Count > 0)
            {
                return string.Join(", ", dislikedIngredients.Select(i => i.IngredientName));
            }
            else
            {
                return "None"; // Если у пользователя нет нелюбимых продуктов
            }
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            bool userConfirmed = await DisplayAlert("Confirmation", "Are you sure you want to leave?", "Yes", "No");

            if (userConfirmed && guest != null)
            {
                bool response = await apiService.DeleteGuestAsync(guest.Id, globalApi);
                if (response)
                {
                    await DisplayAlert("Success", "Guest deleted!", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Invalid server response format", "OK");
                }
            }
            await Navigation.PushAsync(new MainPage(globalApi));
        }


        private async Task<List<FoodIngredient>> GetAllIngredientsAsync()
        {
            try
            {
                return await apiService.GetAllIngredientsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }


        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is UserDetailItem selectedItem)
            {
                string propertyName = selectedItem.PropertyName?.Replace(":", "");
                string oldValue = selectedItem.PropertyValue;

                if (propertyName != "Disliked Ingredients")
                {
                    string newValue = await DisplayPromptAsync("Edit Value", $"Enter a new value for {propertyName}:", initialValue: oldValue);

                    if (newValue != null)
                    {
                        try
                        {
                            ApiService.UserDetailUpdateModel updateModel = new() // Change this line
                            {
                                PropertyName = propertyName,
                                NewValue = newValue
                            };

                            // Send an HTTP PATCH request to update the user property
                            User updatedUser = await apiService.UpdateUserPropertyAsync(user.Id, updateModel, apiKey);

                            if (updatedUser != null)
                            {
                                // Update the local user object with the new value
                                if (propertyName == "Username")
                                {
                                    user.Username = newValue;
                                }
                                else if (propertyName == "Password")
                                {
                                    user.Password = HashPassword(newValue);
                                }
                                else if (propertyName == "Type")
                                {
                                    user.Type = int.Parse(newValue);
                                }

                                // Display an alert
                                await DisplayAlert("Updated " + propertyName, "New data: " + newValue, "OK");

                                // Refresh the UserDetailsListView with the updated data
                                RefreshDataAsync();
                            }
                            else
                            {
                                // Display an error alert
                                await DisplayAlert("Error", "Failed to update user property", "OK");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle exceptions
                            await DisplayAlert("Error", "An error occurred: " + ex.Message, "OK");
                        }
                    }
                }
                else
                {
                    var ingredients = await GetAllIngredientsAsync();

                    await Navigation.PushAsync(new SelectDislikedIngredientsPage(user, guest, ingredients, dislikedIngredients, apiKey, globalApi));
                }

                // Переместите вызов RefreshDataAsync() в этот блок, только если SelectDislikedIngredientsPage отсутствует в стеке
                if (Navigation.NavigationStack.OfType<SelectDislikedIngredientsPage>().FirstOrDefault() == null)
                {
                    RefreshDataAsync();
                }
            }
        }

        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            bool userConfirmed = await DisplayAlert("Confirmation", "Are you sure you want to delete this user?", "Yes", "No");

            if (userConfirmed && user != null)
            {
                // Prompt the user for the password
                string enteredPassword = await DisplayPromptAsync("Password Confirmation", "Please enter your password to confirm:", maxLength: 20, keyboard: Keyboard.Default, placeholder: "");

                if (!string.IsNullOrWhiteSpace(enteredPassword))
                {
                    bool response = await apiService.DeleteUserAsync(user.Id, apiKey, enteredPassword);

                    if (response)
                    {
                        await DisplayAlert("Success", "User deleted!", "OK");
                        await Navigation.PushAsync(new MainPage(globalApi));
                    }
                    else
                    {
                        await DisplayAlert("Error", "Invalid server response format", "OK");
                    }
                }
                else
                {
                    // User canceled or entered an empty password
                    await DisplayAlert("Error", "Password is required to delete the user.", "OK");
                }
            }
        }

        private async void OnSearchButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SearchPage(globalApi, user, apiKey));
        }


        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

    }
    public class UserDetailItem
    {
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        public string NewValue { get; set; } // Add this line
    }
}