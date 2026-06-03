using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snek.Data;

namespace Snek.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty] private string _login = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _errorMessage = "";
    
    [RelayCommand]
    private void SignIn()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Заполните все поля!";
            return;
        }

        using var dbContext = new AppDbContext();
        var user = dbContext.Users.FirstOrDefault(u => u.Login == Login);
        if (user == null || user.Password != Password)
        {
            ErrorMessage = "Невалидные данные";
            return;
        }

        ErrorMessage = "Входите!";
    }

    [RelayCommand]
    private void SignAsGuest()
    {
        ErrorMessage = "Входите!";
    }
}