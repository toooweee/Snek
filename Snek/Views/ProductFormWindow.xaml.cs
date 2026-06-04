using System.Windows;
using Snek.Models;
using Snek.ViewModels;

namespace Snek.Views;

public partial class ProductFormWindow : Window
{
    public ProductFormWindow(Product? productToEdit)
    {
        InitializeComponent();
        var vm = new ProductFormViewModel(productToEdit);
        vm.RequestClose += () => Close();
        DataContext = vm;   
    }
}