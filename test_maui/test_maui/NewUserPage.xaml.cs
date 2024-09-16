namespace test_maui;
using test_maui.Modules;
using test_maui.Data;

public partial class NewUserPage : ContentPage
{
    private readonly ApiService apiService;
    private readonly string globalApiKey;
    public NewUserPage(string apiKey)
	{
        globalApiKey = apiKey;

        InitializeComponent();

        apiService = new ApiService(); // Инициализируем экземпляр ApiService
    }

    private async void OnCreateUserClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;
        int type;

        if (int.TryParse(TypeEntry.Text, out type))
        {
            // Хешируйте пароль
            string hashedPassword = HashPassword(password);

            // Вызовите метод API для создания пользователя
            bool createUserResult = await apiService.CreateUserAsync(username, hashedPassword, globalApiKey);

            if (createUserResult)
            {
                // Очистите поля ввода
                UsernameEntry.Text = "";
                PasswordEntry.Text = "";
                TypeEntry.Text = "";

                await DisplayAlert("Success", "User created successfully", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to create user. Please try again.", "OK");
            }
        }
        else
        {
            await DisplayAlert("Error", "Invalid type input. Please enter a valid number.", "OK");
        }
        await Navigation.PopAsync();
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

}