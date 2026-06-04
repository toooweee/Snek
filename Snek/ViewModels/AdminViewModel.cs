using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snek.Data;
using Snek.Models;
using Snek.Views;

namespace Snek.ViewModels;

public partial class AdminViewModel : ObservableObject
{
    // ОДИН контекст на всю VM. С Proxies иначе никак —
    // навигационные свойства (p.Category.Name) ленивые,
    // и если контекст закрыт, при первом обращении из XAML будет ObjectDisposedException.
    private readonly AppDbContext _ctx = new();

    [ObservableProperty] private string _currentUserFio = "";
    [ObservableProperty] private ObservableCollection<Product> _products = new();
    [ObservableProperty] private ObservableCollection<Supplier> _suppliers = new();
    [ObservableProperty] private Supplier? _selectedSupplier;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private int _selectedSortIndex;
    [ObservableProperty] private Product? _selectedProduct;

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
        foreach (var s in _ctx.Suppliers.OrderBy(s => s.Name))
            Suppliers.Add(s);

        SelectedSupplier = Suppliers[0];
    }

    private void RefreshProducts()
    {
        // Сбрасываем трекер, чтобы при повторной загрузке получить свежие данные из БД
        // (например, после редактирования товара в другом контексте).
        _ctx.ChangeTracker.Clear();

        IQueryable<Product> query = _ctx.Products;

        if (SelectedSupplier is { Id: > 0 })
            query = query.Where(p => p.SupplierId == SelectedSupplier.Id);

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
            _ => query.OrderBy(p => p.Id)
        };

        Products.Clear();
        foreach (var p in query.ToList())
            Products.Add(p);
    }

    [RelayCommand]
    private void Add() => OpenForm(productToEdit: null);

    [RelayCommand]
    private void Edit()
    {
        if (SelectedProduct is null)
        {
            MessageBox.Show("Выберите товар для редактирования.",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        OpenForm(SelectedProduct);
    }

    [RelayCommand]
    private void Delete()
    {
        if (SelectedProduct is null)
        {
            MessageBox.Show("Выберите товар для удаления.",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        bool inOrder = _ctx.OrderItems.Any(oi => oi.ProductId == SelectedProduct.Id);
        if (inOrder)
        {
            MessageBox.Show("Нельзя удалить товар, который присутствует в заказе.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var ok = MessageBox.Show($"Удалить товар «{SelectedProduct.Name}»?",
            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (ok != MessageBoxResult.Yes) return;

        try
        {
            DeleteUserImageIfNeeded(SelectedProduct.Image);
            _ctx.Products.Remove(SelectedProduct);
            _ctx.SaveChanges();
            RefreshProducts();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось удалить товар: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Защита от двух открытых форм одновременно.
    private static ProductFormWindow? _openedForm;

    private void OpenForm(Product? productToEdit)
    {
        if (_openedForm is not null)
        {
            _openedForm.Activate();
            return;
        }

        var form = new ProductFormWindow(productToEdit);
        _openedForm = form;
        form.Closed += (_, _) =>
        {
            _openedForm = null;
            RefreshProducts();
        };
        form.Show();
    }

    private static void DeleteUserImageIfNeeded(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath)) return;
        if (!imagePath.StartsWith("UserImages", StringComparison.OrdinalIgnoreCase)) return;

        var full = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
        if (File.Exists(full))
        {
            try
            {
                File.Delete(full);
            }
            catch
            {
                /* не критично */
            }
        }
    }

    [RelayCommand]
    private void Logout()
    {
        var login = new LoginWindow();
        login.Show();

        // Закрываем именно AdminWindow, а не "первое попавшееся".
        foreach (Window w in Application.Current.Windows)
        {
            if (w is AdminWindow)
            {
                w.Close();
                break;
            }
        }
    }
}