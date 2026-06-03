using System.Windows;
using Snek.ViewModels;

namespace Snek.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        DataContext = new LoginViewModel();
    }
}