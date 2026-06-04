using System.Configuration;
using System.Data;
using System.IO;
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
    public static string UserImagesFolder { get; } =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserImages");
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Directory.CreateDirectory(UserImagesFolder);
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        using var dbContext = new AppDbContext();
        dbContext.Database.EnsureCreated();
        Seeder.SeedAll(dbContext);
        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }
}