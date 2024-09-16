using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using test_maui.Modules;

namespace test_maui
{
    public partial class RestaurantDetailsPage : ContentPage
    {
        private readonly RestaurantDisplayData selectedRestaurant;
        private readonly ApiService apiService;
        private readonly string userApiKey;
        private readonly User user;
        private double userRating = -1; // Изначально устанавливаем оценку пользователя на -1

        public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();

        public RestaurantDetailsPage(RestaurantDisplayData selectedRestaurant, User user, string userApiKey)
        {
            apiService = new ApiService();

            this.selectedRestaurant = selectedRestaurant;
            this.user = user;
            this.userApiKey = userApiKey;

            InitializeComponent();

            BindingContext = this.selectedRestaurant;

            // Если user есть, показать элементы для оценки
            if (user != null)
            {
                ratingSlider.IsVisible = true;
                rateButton.IsVisible = true;

                // Получить оценку пользователя для этого ресторана
                _ = GetUserRatingAsync(user.Id, userApiKey);
            }

            // Получить комментарии для ресторана
            _ = LoadCommentsAsync();
        }

        private async Task GetUserRatingAsync(int userId, string userApiKey)
        {
            // Получаем оценку пользователя для этого ресторана
            userRating = await apiService.GetUserRating(userId, selectedRestaurant.Id, userApiKey);

            // Если оценка пользователя получена, устанавливаем значение слайдера
            if (userRating >= 0)
            {
                ratingSlider.Value = userRating;
            }
        }

        private async Task LoadCommentsAsync()
        {
            Comments = await apiService.GetCommentsByRestaurantAsync(selectedRestaurant.Id, userApiKey);
            commentsListView.ItemsSource = Comments;
        }

        private async void RateButton_Clicked(object sender, EventArgs e)
        {
            // Получаем новую оценку из Slider
            double newRating = ratingSlider.Value;

            // Отправляем запрос на сервер для обновления оценки
            string result = await apiService.RateRestaurantAsync(user.Id, selectedRestaurant.Id, newRating, userApiKey);

            if (result != null && result.StartsWith("Rating successfully updated/added"))
            {
                await DisplayAlert("Success", "Rating updated/added successfully", "OK");
                userRating = newRating; // Обновляем оценку пользователя
            }
            else
            {
                string errorMessage = result ?? "Unknown error occurred";
                await DisplayAlert("Error", errorMessage, "OK");
            }
        }

        private async void OnFoodItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is ApiService.FoodItemData selectedDish)
            {
                // Переходим на страницу с деталями блюда
                await Navigation.PushAsync(new DishDetailsPage(user, userApiKey, selectedDish));
            }
            // Снимаем выделение элемента
            ((ListView)sender).SelectedItem = null;
        }

        private async void AddComment_Clicked(object sender, EventArgs e)
        {
            string commentText = commentEntry.Text;

            if (string.IsNullOrEmpty(commentText))
            {
                await DisplayAlert("Error", "Comment text cannot be empty", "OK");
                return;
            }

            CommentDTO newComment = new CommentDTO
            {
                RestaurantsId = selectedRestaurant.Id,
                UserId = user.Id,
                ParentCommentId = -1, // Assuming -1 means no parent comment
                CommentText = commentText
            };

            var addedComment = await apiService.CreateCommentAsync(newComment, userApiKey);

            if (addedComment != null)
            {
                Comments.Add(addedComment);
                commentsListView.ItemsSource = null;
                commentsListView.ItemsSource = Comments;
                commentEntry.Text = string.Empty;
            }
            else
            {
                await DisplayAlert("Error", "Failed to add comment", "OK");
            }
        }

        private async void EditComment_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is CommentDTO comment)
            {
                string newCommentText = await DisplayPromptAsync("Edit Comment", "Update your comment:", initialValue: comment.CommentText);

                if (string.IsNullOrEmpty(newCommentText))
                {
                    await DisplayAlert("Error", "Comment text cannot be empty", "OK");
                    return;
                }

                comment.CommentText = newCommentText;

                bool success = await apiService.UpdateCommentAsync(comment.Id, comment, userApiKey);

                if (success)
                {
                    commentsListView.ItemsSource = null;
                    commentsListView.ItemsSource = Comments;
                }
                else
                {
                    await DisplayAlert("Error", "Failed to update comment", "OK");
                }
            }
        }

        private async void DeleteComment_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is CommentDTO comment)
            {
                bool confirm = await DisplayAlert("Delete Comment", "Are you sure you want to delete this comment?", "Yes", "No");

                if (!confirm) return;

                bool success = await apiService.DeleteCommentAsync(comment.Id, userApiKey);

                if (success)
                {
                    Comments.Remove(comment);
                    commentsListView.ItemsSource = null;
                    commentsListView.ItemsSource = Comments;
                }
                else
                {
                    await DisplayAlert("Error", "Failed to delete comment", "OK");
                }
            }
        }
    }
}