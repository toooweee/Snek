using System.Configuration;
using System.Data;
using System.Windows;
using Snek.Data;

namespace Snek;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var dbContext = new AppDbContext();
        dbContext.Database.EnsureCreated();
    }
}