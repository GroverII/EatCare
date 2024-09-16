using System.Collections.ObjectModel;
using test_maui.Modules;
using static ApiService;

namespace test_maui
{
    public partial class SearchPage : ContentPage
    {
        private readonly ApiService apiService;
        private readonly string apiKey;
        private readonly string userApiKey;
        private readonly User user;

        public SearchPage(string apiKey, User user, string userApiKey)
        {
            InitializeComponent();

            this.apiService = new ApiService();
            this.apiKey = apiKey;
            this.user = user;
            this.userApiKey = userApiKey;

            Restaurants.ItemTapped += OnItemTapped;

            LoadRestaurantsData();
        }

        private async void LoadRestaurantsData()
        {
            try
            {
                var restaurantList = await apiService.GetAllRestaurantsAsync(apiKey);

                var displayDataList = restaurantList.Select(restaurantData =>
                {
                    var base64Image = restaurantData.ImageContent != null ? ImageSource.FromStream(() => new MemoryStream(restaurantData.ImageContent)) : null;
                    var Id = restaurantData.Restaurant.Id;
                    var foodItems = restaurantData.Restaurant.FoodItems;
                    var ratings = restaurantData.Restaurant.Ratings;
                    if (ratings == 0.0) ratings = double.NaN;
                    var displayData = new RestaurantDisplayData { Id = Id, ImageContent = base64Image, 
                        RestaurantName = restaurantData.Restaurant.RestaurantName, Ratings = ratings, FoodItems = foodItems };
                    return displayData;
                }).ToList();

                Restaurants.ItemsSource = new ObservableCollection<RestaurantDisplayData>(displayDataList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item != null)
            {
                RestaurantDisplayData selectedDisplayData = (RestaurantDisplayData)e.Item;
                Navigation.PushAsync(new RestaurantDetailsPage(selectedDisplayData, user, userApiKey));
            }
        }
    }

}
