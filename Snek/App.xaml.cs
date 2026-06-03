using System.Configuration;
using System.Data;
using System.Windows;
using Snek.Data;
using Snek.ViewModels;
using Snek.Views;

namespace Snek;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        using var dbContext = new AppDbContext();
        dbContext.Database.EnsureCreated();
        Seeder.SeedAll(dbContext);
        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }
}