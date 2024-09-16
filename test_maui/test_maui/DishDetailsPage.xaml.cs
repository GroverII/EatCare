using EatCareService.Modules;
using test_maui.Modules;
using static ApiService;

namespace test_maui
{
    public partial class DishDetailsPage : ContentPage
    {
        private readonly User user;
        private readonly ApiService apiService;
        private readonly string userApiKey;
        private readonly FoodItemData dish;
        private double? userRating;
        public List<DishCommentDTO> Comments { get; set; } = new List<DishCommentDTO>();

        public DishDetailsPage(User user, string userApiKey, FoodItemData dish)
        {
            InitializeComponent();
            this.user = user;
            this.userApiKey = userApiKey;
            this.dish = dish;
            apiService = new ApiService();
            BindingContext = this.dish;

            // Загрузка рейтинга пользователя для блюда
            LoadUserRating();
        }

        private async void LoadUserRating()
        {
            // Получаем рейтинг пользователя для блюда
            userRating = await apiService.GetUserDishRatingAsync(user.Id, dish.Id, userApiKey);

            // Если рейтинг существует, устанавливаем его в модель данных
            if (userRating.HasValue)
            {
                dish.Rating = userRating.Value;
            }
        }

        private async void RateButton_Clicked(object sender, EventArgs e)
        {
            double newRating = ratingSlider.Value;

            // Отправляем запрос на сервер для оценки блюда
            string result = await apiService.RateDishAsync(user.Id, dish.Id, newRating, userApiKey);

            if (result != null && result.StartsWith("Rating successfully updated/added"))
            {
                await DisplayAlert("Success", "Rating updated/added successfully", "OK");
            }
            else
            {
                string errorMessage = result ?? "Unknown error occurred";
                await DisplayAlert("Error", errorMessage, "OK");
            }
        }

        private async void AddComment_Clicked(object sender, EventArgs e)
        {
            string commentText = commentEntry.Text;

            if (string.IsNullOrEmpty(commentText))
            {
                await DisplayAlert("Error", "Comment text cannot be empty", "OK");
                return;
            }

            DishCommentDTO newComment = new DishCommentDTO
            {
                DishId = dish.Id,
                UserId = user.Id,
                ParentCommentId = -1, // Assuming -1 means no parent comment
                CommentText = commentText
            };

            var addedComment = await apiService.CreateDishCommentAsync(newComment, userApiKey);

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
            if (sender is Button button && button.BindingContext is DishCommentDTO comment)
            {
                string newCommentText = await DisplayPromptAsync("Edit Comment", "Update your comment:", initialValue: comment.CommentText);

                if (string.IsNullOrEmpty(newCommentText))
                {
                    await DisplayAlert("Error", "Comment text cannot be empty", "OK");
                    return;
                }

                comment.CommentText = newCommentText;

                bool success = await apiService.UpdateDishCommentAsync(comment.Id, comment, userApiKey);

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
            if (sender is Button button && button.BindingContext is DishCommentDTO comment)
            {
                bool confirm = await DisplayAlert("Delete Comment", "Are you sure you want to delete this comment?", "Yes", "No");

                if (!confirm) return;

                bool success = await apiService.DeleteDishCommentAsync(comment.Id, userApiKey);

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
