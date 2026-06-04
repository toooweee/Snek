using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Wordprocessing;
using Snek.Data;
using Snek.Views;

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

        switch (user.Role.Name)
        {
            case "Администратор":
                var adminWindow = new AdminWindow(user);
                adminWindow.Show();
                Application.Current.Windows[0]?.Close();
                break;
            case "Менеджер":
                
                Application.Current.Windows[0]?.Close();
                break;
            case "Авторизированный клиент":
                Application.Current.Windows[0]?.Close();
                break;
            default:
                ErrorMessage = "Невалидная роль!";
                return;
        }
    }

    [RelayCommand]
    private void SignAsGuest()
    {
        ErrorMessage = "Входите!";
    }
}