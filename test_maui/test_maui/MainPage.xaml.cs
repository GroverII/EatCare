using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using test_maui.Modules;
using SkiaSharp;
using Newtonsoft.Json;
using System.Net;
using static ApiService;

namespace test_maui
{
    public partial class MainPage : ContentPage
    {
        private List<User> allUsers;
        private ApiService apiService;
        private string globalApiKey = "";
        private string metadataValue;

        private ScrollView scrollView;
        private VerticalStackLayout stackLayout;
        private Image userImage;
        private Entry searchEntry;
        private Button createNewUserButton;
        private Button enterAsGuestButton;
        private ListView UsersListView;

        private Entry codeEntry;
        private Button confirmButton;
        private CheckBox yesNoCheckBox;

        public MainPage(string apiKey) {
            globalApiKey = apiKey;
            LoadStackLayout();
        }
        public MainPage()
        {
            LoadStackLayout();
        }

        private void LoadStackLayout() {
            // Инициализация элементов UI в коде
            scrollView = new ScrollView();
            stackLayout = new VerticalStackLayout
            {
                Spacing = 25,
                Padding = new Thickness(30, 0),
                VerticalOptions = LayoutOptions.Center
            };

            userImage = new Image
            {
                WidthRequest = 100,
                HeightRequest = 100,
                Aspect = Aspect.AspectFit
            };

            searchEntry = new Entry
            {
                Placeholder = "Search Users"
            };
            searchEntry.TextChanged += OnSearchTextChanged;

            createNewUserButton = new Button
            {
                Text = "Create New User"
            };
            createNewUserButton.Clicked += OnCreateNewUserClicked;

            enterAsGuestButton = new Button
            {
                Text = "Enter as Guest"
            };
            enterAsGuestButton.Clicked += OnEnterAsGuestClicked;

            codeEntry = new Entry
            {
                Placeholder = "Enter Code with Photo"
            };

            confirmButton = new Button
            {
                Text = "Confirm"
            };
            confirmButton.Clicked += OnConfirmButtonClicked;

            yesNoCheckBox = new CheckBox
            {
                IsChecked = false // Измените на true или false в зависимости от вашего состояния по умолчанию
            };

            UsersListView = new ListView();
            UsersListView.ItemTapped += OnUserItemTapped;
            UsersListView.ItemTemplate = new DataTemplate(() =>
            {
                var cell = new ViewCell();
                var stack = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Margin = new Thickness(0, 10, 0, 10),
                    BackgroundColor = Color.FromArgb("#E6E6E6")
                };
                var label = new Label
                {
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                };
                label.SetBinding(Label.TextProperty, "Username");
                stack.Children.Add(label);
                cell.View = stack;
                return cell;
            });

            // Определите условие, при котором нужно добавить элементы в stackLayout
            if (!globalApiKey.Equals(""))
            {
                // Добавляем элементы в stackLayout, включая userImage, codeEntry и confirmButton
                stackLayout.Children.Add(searchEntry);
                stackLayout.Children.Add(createNewUserButton);
                stackLayout.Children.Add(enterAsGuestButton);
                stackLayout.Children.Add(UsersListView);
            }
            else
            {
                // Если фото не получено, добавляем только userImage
                stackLayout.Children.Add(userImage);
                stackLayout.Children.Add(codeEntry);
                stackLayout.Children.Add(confirmButton);
            }

            // Добавляем stackLayout в scrollView
            scrollView.Content = stackLayout;

            // Добавляем scrollView в ContentPage
            Content = scrollView;

            apiService = new ApiService();
            GetGlobalAPI();
        }

        private async void GetGlobalAPI()
        {
            ImageData imageData = await apiService.GetRandomTextImageAsync();

            if (imageData != null && imageData.ImageBytes != null && imageData.Metadata.Count > 0)
            {
                var imageSource = ImageSource.FromStream(() => new MemoryStream(imageData.ImageBytes));
                userImage.Source = imageSource;

                // Получаем первое значение метаданных
                metadataValue = imageData.Metadata.Values.First();
            }
            else
            {
                // Если imageData, imageBytes или metadata пусты, устанавливаем цвет фона как чёрный
                userImage.BackgroundColor = Colors.Black;
            }
        }



        protected override void OnAppearing()
        {
            base.OnAppearing();

            if(!globalApiKey.Equals(""))
                LoadAllUsers();
        }

        private async void LoadAllUsers()
        {
            try
            {
                List<UserDTO> allUsersDTO = await apiService.GetAllUsersAsync(globalApiKey);

                if (allUsersDTO != null)
                {
                    // Преобразуйте UserDTO в ваш тип User, если необходимо
                    allUsers = allUsersDTO.Select(dto => new User { Id = dto.Id, Username = dto.Username }).ToList();

                    UsersListView.ItemsSource = allUsers;
                }
                else
                {
                    await DisplayAlert("Error", "No Data!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }


        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue.Trim().ToLower();

            if (allUsers != null)
            {
                var filteredUsers = allUsers
                    .Where(user => searchText.All(charSearch => user.Username.ToLower().Contains(charSearch.ToString().ToLower())))
                    .ToList();

                UsersListView.ItemsSource = filteredUsers;
            }
        }

        private async void OnUserItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is User selectedUser)
            {
                string enteredPassword = await DisplayPromptAsync("Password Required", "Please enter the password for this user:");

                int userId = selectedUser.Id;

                if (string.IsNullOrEmpty(enteredPassword))
                {
                    return;
                }

                string apiKey = await apiService.VerifyPasswordAsync(userId, enteredPassword, globalApiKey);

                if (!string.IsNullOrEmpty(apiKey))
                {
                    await Navigation.PushAsync(new UserDetailPage(selectedUser, null, null, apiKey, globalApiKey));
                }
                else
                {
                    await DisplayAlert("Error", "Incorrect password. Access denied.", "OK");
                }
            }
        }


        private async void OnEnterAsGuestClicked(object sender, EventArgs e)
        {
            try
            {
                string ipAddress = GetDeviceIPAddress();

                var guest = await apiService.EnterAsGuestAsync(ipAddress, globalApiKey);

                if (guest != null)
                {
                    await DisplayAlert("Guest ID", $"Guest ID: {guest.Id}", "OK");
                    await Navigation.PushAsync(new UserDetailPage(null, guest, null, null, globalApiKey));
                }
                else
                {
                    await DisplayAlert("Error", "Failed to enter as guest.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private string GetDeviceIPAddress()
        {
            try
            {
                var currentNetwork = Connectivity.NetworkAccess;

                if (currentNetwork == NetworkAccess.Internet)
                {
                    var currentIpAddresses = Dns.GetHostAddresses(Dns.GetHostName());
                    var ipv4Address = currentIpAddresses.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                    if (ipv4Address != null)
                    {
                        return ipv4Address.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting IP address: {ex.Message}");
                DisplayAlert("Error", ex.Message, "OK");
            }

            return "127.0.0.1";
        }

        private async void OnConfirmButtonClicked(object sender, EventArgs e)
        {
            string RandomText = codeEntry.Text;

            if (RandomText == null) { return; }

            //await Navigation.PushAsync(new ConfigurationPage());

            Ticket verificationModel = new Ticket
            {
                Id = new Guid(metadataValue),
                RandomText = RandomText
            };

            globalApiKey = await apiService.VerifyTicketAndSendApiKeyAsync(verificationModel);
            await Navigation.PushAsync(new MainPage(globalApiKey));
        }

        private async void OnCreateNewUserClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NewUserPage(globalApiKey));
        }
    }
}
