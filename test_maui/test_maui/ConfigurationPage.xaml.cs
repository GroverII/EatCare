namespace test_maui
{
    public partial class ConfigurationPage : ContentPage
    {
        public List<RestaurantMenu> MenuData { get; set; }
        private ListView listView;
        private Picker picker;
        private string selectedParameter = "Restaurant Name";

        public ConfigurationPage()
        {
            InitializeComponent();

            MenuData = new List<RestaurantMenu>
            {
                new RestaurantMenu
                {
                    RestaurantName = "GoEat!",
                    FoodItems = new List<FoodItem>
                    {
                        new FoodItem
                        {
                            ItemName = "Milkshake",
                            Price = 5.99,
                            FoodType = "Dessert",
                            Ingredients = new List<string> { "Milk", "Ice Cream", "Sugar" }
                        },
                        new FoodItem
                        {
                            ItemName = "Non-Dairy Milkshake",
                            Price = 6.99,
                            FoodType = "Dessert",
                            Ingredients = new List<string> { "Milk Substitute", "Ice Cream", "Sugar", "Flavorings" }
                        }
                    }
                }
            };

            picker = new Picker
            {
                Title = "Select Grouping Parameter",
                ItemsSource = new List<string> { "Restaurant Name", "Price", "Item Name", "Food Type" }
            };

            picker.SelectedIndexChanged += (sender, args) =>
            {
                if (picker.SelectedIndex == -1)
                    return;

                selectedParameter = picker.SelectedItem.ToString();
                UpdateDataBasedOnParameter();

                picker.SelectedIndex = -1; // Сбросить выбор, чтобы можно было выбрать снова
            };

            listView = new ListView
            {
                ItemsSource = MenuData,
                ItemTemplate = new DataTemplate(() =>
                {
                    var dataLabel = new Label();
                    // Здесь устанавливаем привязку к соответствующему свойству объекта в зависимости от выбранного параметра
                    dataLabel.SetBinding(Label.TextProperty, new Binding(selectedParameter));

                    return new ViewCell
                    {
                        View = new StackLayout
                        {
                            Children = { dataLabel }
                        }
                    };
                })
            };

            Content = new StackLayout
            {
                Children = { picker, listView }
            };
        }

        private void UpdateDataBasedOnParameter()
        {
            listView.BeginRefresh();

            switch (selectedParameter)
            {
                case "Restaurant Name":
                    listView.ItemTemplate = new DataTemplate(() =>
                    {
                        var dataLabel = new Label();
                        dataLabel.SetBinding(Label.TextProperty, "RestaurantName");
                        return new ViewCell { View = new StackLayout { Children = { dataLabel } } };
                    });
                    break;
                case "Price":
                    var foodItemsCollectionView = new CollectionView
                    {
                        ItemTemplate = new DataTemplate(() =>
                        {
                            var priceLabel = new Label();
                            priceLabel.SetBinding(Label.TextProperty, "Price");
                            return new Grid { Children = { priceLabel } };
                        })
                    };

                    foodItemsCollectionView.SetBinding(ItemsView.ItemsSourceProperty, "FoodItems");

                    listView.ItemTemplate = new DataTemplate(() =>
                    {
                        var dataLabel = new Label();
                        dataLabel.SetBinding(Label.TextProperty, "RestaurantName");

                        return new ViewCell
                        {
                            View = new StackLayout
                            {
                                Children = { dataLabel, foodItemsCollectionView }
                            }
                        };
                    });
                    break;
                case "Item Name":
                    // Добавьте привязку к свойству "ItemName" внутри FoodItem
                    listView.ItemTemplate = new DataTemplate(() =>
                    {
                        var dataLabel = new Label();
                        dataLabel.SetBinding(Label.TextProperty, "FoodItems[0].ItemName"); // Пример: первый элемент FoodItems
                        return new ViewCell { View = new StackLayout { Children = { dataLabel } } };
                    });
                    break;
                case "Food Type":
                    // Добавьте привязку к свойству "FoodType" внутри FoodItem
                    listView.ItemTemplate = new DataTemplate(() =>
                    {
                        var dataLabel = new Label();
                        dataLabel.SetBinding(Label.TextProperty, "FoodItems[0].FoodType"); // Пример: первый элемент FoodItems
                        return new ViewCell { View = new StackLayout { Children = { dataLabel } } };
                    });
                    break;
                default:
                    break;
            }

            listView.EndRefresh();
        }




        public class Ingredient
        {
            public string IngredientName { get; set; }
        }

        public class FoodItem
        {
            public string ItemName { get; set; }
            public double Price { get; set; }
            public string FoodType { get; set; }
            public List<string> Ingredients { get; set; }
        }

        public class RestaurantMenu
        {
            public string RestaurantName { get; set; }
            public List<FoodItem> FoodItems { get; set; }
        }
    }
}
