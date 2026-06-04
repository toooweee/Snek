using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snek.Data;
using Snek.Models;
using Snek.Views;

namespace Snek.ViewModels;

public partial class AdminViewModel : ObservableObject
{
    [ObservableProperty] private string _currentUserFio = "";
    [ObservableProperty] private ObservableCollection<Product> _products = new();
    [ObservableProperty] private ObservableCollection<Supplier> _suppliers = new();
    [ObservableProperty] private Supplier? _selectedSupplier = null!;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private int _selectedSortIndex;

    public AdminViewModel(User user)
    {
        CurrentUserFio = user.FullName;
        LoadSuppliers();
        RefreshProducts();
    }

    partial void OnSearchTextChanged(string value) => RefreshProducts();
    partial void OnSelectedSupplierChanged(Supplier? v) => RefreshProducts();
    partial void OnSelectedSortIndexChanged(int value) => RefreshProducts();

    private void LoadSuppliers()
    {
        Suppliers.Clear();
        Suppliers.Add(new Supplier { Id = 0, Name = "Все поставщики" });
        using var dbContext = new AppDbContext();
        foreach (var s in dbContext.Suppliers.OrderBy(s => s.Name))
        {
            Suppliers.Add(s);
        }

        SelectedSupplier = Suppliers[0];
    }

    private void RefreshProducts()
    {
        using var dbContext = new AppDbContext();
        IQueryable<Product> query = dbContext.Products;

        if (SelectedSupplier is { Id: > 0 })
        {
            query = query.Where(p => p.SupplierId == SelectedSupplier.Id);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.ToLower();
            query = query.Where(p =>
                p.Art.ToLower().Contains(s) ||
                p.Name.ToLower().Contains(s) ||
                p.Description.ToLower().Contains(s) ||
                p.Category.Name.ToLower().Contains(s) ||
                p.Supplier.Name.ToLower().Contains(s) ||
                p.Manufacturer.Name.ToLower().Contains(s));
        }

        query = SelectedSortIndex switch
        {
            1 => query.OrderBy(p => p.Stock),
            2 => query.OrderByDescending(p => p.Stock),
            _ => query.OrderBy(p => p.Id),
        };
        
        Products.Clear();
        foreach (var p in query.ToList())
        {
            Products.Add(p);
        }
    }

    [RelayCommand]
    private void Logout()
    {
        var login = new LoginWindow();
        login.Show();
        Application.Current.Windows[0]?.Close();
    }
}