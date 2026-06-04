using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;
using Snek.Data;
using Snek.Models;
using Category = Snek.Models.Category;

namespace Snek.ViewModels;

public partial class ProductFormViewModel : ObservableObject
{
    private readonly AppDbContext _ctx = new();
    private readonly Product? _editing; // null = создаём новый

    public event Action? RequestClose;

    public bool IsEditMode => _editing is not null;
    public string WindowTitle => IsEditMode ? "Редактирование товара" : "Добавление товара";

    // ----- Поля формы -----
    [ObservableProperty] private string art = "";
    [ObservableProperty] private string name = "";
    [ObservableProperty] private string description = "";
    [ObservableProperty] private string priceText = "0";
    [ObservableProperty] private string stockText = "0";
    [ObservableProperty] private string discountText = "0";
    [ObservableProperty] private string imagePath = ""; // что показываем И что сохраним в БД

    // ----- Справочники -----
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<Manufacturer> Manufacturers { get; } = new();
    public ObservableCollection<Supplier> Suppliers { get; } = new();
    public ObservableCollection<UnitMeasure> Units { get; } = new();

    [ObservableProperty] private Category? selectedCategory;
    [ObservableProperty] private Manufacturer? selectedManufacturer;
    [ObservableProperty] private Supplier? selectedSupplier;
    [ObservableProperty] private UnitMeasure? selectedUnit;

    // Запоминаем, какая фотка была до изменений, чтобы удалить старую при замене.
    private string? _originalImage;

    // Если выбрали новую фотку, но ещё не сохранили — путь к временно выбранному файлу.
    private string? _newImageFullPath;

    public ProductFormViewModel(Product? productToEdit)
    {
        _editing = productToEdit;

        LoadDictionaries();

        if (_editing is not null)
        {
            Art = _editing.Art;
            Name = _editing.Name;
            Description = _editing.Description;
            PriceText = _editing.Price.ToString("0.##");
            StockText = _editing.Stock.ToString();
            DiscountText = _editing.CurrentDiscount.ToString();
            ImagePath = _editing.Image;
            _originalImage = _editing.Image;

            SelectedCategory = Categories.FirstOrDefault(c => c.Id == _editing.CategoryId);
            SelectedManufacturer = Manufacturers.FirstOrDefault(m => m.Id == _editing.ManufacturerId);
            SelectedSupplier = Suppliers.FirstOrDefault(s => s.Id == _editing.SupplierId);
            SelectedUnit = Units.FirstOrDefault(u => u.Id == _editing.UnitMeasureId);
        }
    }

    private void LoadDictionaries()
    {
        foreach (var c in _ctx.Categories.OrderBy(x => x.Name)) Categories.Add(c);
        foreach (var m in _ctx.Manufacturers.OrderBy(x => x.Name)) Manufacturers.Add(m);
        foreach (var s in _ctx.Suppliers.OrderBy(x => x.Name)) Suppliers.Add(s);
        foreach (var u in _ctx.UnitMeasures.OrderBy(x => x.Name)) Units.Add(u);
    }

    [RelayCommand]
    private void PickImage()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp"
        };
        if (dlg.ShowDialog() != true) return;

        _newImageFullPath = dlg.FileName;
        // Для предпросмотра показываем выбранный файл напрямую с диска.
        // (Конвертер увидит абсолютный путь, который не начинается с "UserImages",
        //  и попробует ресурс — нужно расширить конвертер. См. пояснение ниже.)
        ImagePath = dlg.FileName;
    }

    [RelayCommand]
    private void Save()
    {
        // ----- Валидация -----
        if (string.IsNullOrWhiteSpace(Art))
        {
            Err("Артикул обязателен.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            Err("Наименование обязательно.");
            return;
        }

        if (SelectedCategory is null)
        {
            Err("Выберите категорию.");
            return;
        }

        if (SelectedManufacturer is null)
        {
            Err("Выберите производителя.");
            return;
        }

        if (SelectedSupplier is null)
        {
            Err("Выберите поставщика.");
            return;
        }

        if (SelectedUnit is null)
        {
            Err("Выберите единицу измерения.");
            return;
        }

        if (!decimal.TryParse(PriceText.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var price) || price < 0)
        {
            Err("Цена должна быть неотрицательным числом (например, 1500.50).");
            return;
        }

        if (!int.TryParse(StockText, out var stock) || stock < 0)
        {
            Err("Количество должно быть целым неотрицательным числом.");
            return;
        }

        if (!int.TryParse(DiscountText, out var discount) || discount < 0 || discount > 100)
        {
            Err("Скидка должна быть числом от 0 до 100.");
            return;
        }

        // ----- Уникальность артикула -----
        bool artExists = _editing is null
            ? _ctx.Products.Any(p => p.Art == Art)
            : _ctx.Products.Any(p => p.Art == Art && p.Id != _editing.Id);
        if (artExists)
        {
            Err("Товар с таким артикулом уже существует.");
            return;
        }

        // ----- Обработка фото -----
        string finalImagePath = _originalImage ?? "";
        if (!string.IsNullOrEmpty(_newImageFullPath))
        {
            try
            {
                finalImagePath = SaveResizedImage(_newImageFullPath);
                // Старую загруженную фотку удаляем.
                DeleteOldUserImage(_originalImage);
            }
            catch (Exception ex)
            {
                Err($"Не удалось сохранить изображение: {ex.Message}");
                return;
            }
        }

        // ----- Сохранение в БД -----
        try
        {
            if (_editing is null)
            {
                var p = new Product
                {
                    Art = Art, Name = Name, Description = Description,
                    Price = price, Stock = stock, CurrentDiscount = discount,
                    CategoryId = SelectedCategory.Id,
                    ManufacturerId = SelectedManufacturer.Id,
                    SupplierId = SelectedSupplier.Id,
                    UnitMeasureId = SelectedUnit.Id,
                    Image = finalImagePath
                };
                _ctx.Products.Add(p);
            }
            else
            {
                var p = _ctx.Products.First(x => x.Id == _editing.Id);
                p.Art = Art;
                p.Name = Name;
                p.Description = Description;
                p.Price = price;
                p.Stock = stock;
                p.CurrentDiscount = discount;
                p.CategoryId = SelectedCategory.Id;
                p.ManufacturerId = SelectedManufacturer.Id;
                p.SupplierId = SelectedSupplier.Id;
                p.UnitMeasureId = SelectedUnit.Id;
                p.Image = finalImagePath;
            }

            _ctx.SaveChanges();
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            Err($"Ошибка при сохранении: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke();

    // ----- Хелперы -----

    private static string SaveResizedImage(string sourceFullPath)
    {
        var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserImages");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}.jpg";
        var fullDest = Path.Combine(folder, fileName);

        // Читаем и ресайзим до 300x200.
        var src = new BitmapImage();
        src.BeginInit();
        src.CacheOption = BitmapCacheOption.OnLoad;
        src.UriSource = new Uri(sourceFullPath);
        src.DecodePixelWidth = 300; // WPF пересчитает высоту пропорционально
        src.EndInit();
        src.Freeze();

        var encoder = new JpegBitmapEncoder { QualityLevel = 85 };
        encoder.Frames.Add(BitmapFrame.Create(src));
        using var fs = new FileStream(fullDest, FileMode.Create);
        encoder.Save(fs);

        // Возвращаем относительный путь — то, что ляжет в БД.
        return Path.Combine("UserImages", fileName).Replace('\\', '/');
    }

    private static void DeleteOldUserImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath)) return;
        if (!imagePath.StartsWith("UserImages", StringComparison.OrdinalIgnoreCase)) return;

        var full = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
        if (File.Exists(full))
            try
            {
                File.Delete(full);
            }
            catch
            {
            }
    }

    private static void Err(string msg)
        => MessageBox.Show(msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
}