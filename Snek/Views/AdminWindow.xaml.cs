using System.Windows;
using Snek.Models;
using Snek.ViewModels;

namespace Snek.Views;

public partial class AdminWindow : Window
{
    public AdminWindow(User user)
    {
        InitializeComponent();
        DataContext = new AdminViewModel(user);
    }
}