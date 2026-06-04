using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Snek.Models;

namespace Snek.Data;

public class Seeder
{
    public static void SeedAll(AppDbContext dbContext)
    {
        SeedRolesAndUsers(dbContext, "Resources/user_import.xlsx");
        SeedProducts(dbContext, "Resources/Tovar.xlsx");
        SeedPickupPoints(dbContext, "Resources/pvz.xlsx");
        SeedOrders(dbContext, "Resources/order.xlsx");
    }

    private static void SeedRolesAndUsers(AppDbContext dbContext, string excelPath)
    {
        if (dbContext.Users.Any()) return;

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheet(1);
        var dataRows = sheet.RowsUsed().Skip(1).ToList();

        var roleNames = dataRows
            .Select(row => row.Cell(1).GetString().Trim())
            .Distinct()
            .ToList();

        var roleByName = dbContext.Roles.ToDictionary(r => r.Name);

        foreach (var name in roleNames)
        {
            if (!roleByName.ContainsKey(name))
            {
                var newRole = new Role { Name = name };
                dbContext.Roles.Add(newRole);
                roleByName[name] = newRole;
            }
        }

        dbContext.SaveChanges();

        foreach (var row in dataRows)
        {
            var roleName = row.Cell(1).GetString().Trim();
            var fullname = row.Cell(2).GetString().Trim();
            var login = row.Cell(3).GetString().Trim();
            var password = row.Cell(4).GetString().Trim();

            var user = new User
                { Login = login, Password = password, FullName = fullname, RoleId = roleByName[roleName].Id };

            dbContext.Users.Add(user);
        }

        dbContext.SaveChanges();
    }

    private static void SeedProducts(AppDbContext dbContext, string excelPath)
    {
        if (dbContext.Products.Any()) return;

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheet(1);

        var rows = sheet.RowsUsed().Skip(1).ToList();

        var unitNames = rows
            .Select(row => row.Cell(3).GetString().Trim())
            .Distinct()
            .ToList();

        var unitByName = dbContext.UnitMeasures.ToDictionary(u => u.Name);

        foreach (var name in unitNames)
        {
            if (!unitByName.ContainsKey(name))
            {
                var unit = new UnitMeasure { Name = name };
                dbContext.UnitMeasures.Add(unit);
                unitByName[name] = unit;
            }
        }

        var supplierNames = rows
            .Select(row => row.Cell(5).GetString().Trim())
            .Distinct()
            .ToList();

        var supplierByName = dbContext.Suppliers.ToDictionary(s => s.Name);

        foreach (var name in supplierNames)
        {
            if (!supplierByName.ContainsKey(name))
            {
                var supplier = new Supplier { Name = name };
                dbContext.Suppliers.Add(supplier);
                supplierByName[name] = supplier;
            }
        }

        var manufacturerNames = rows
            .Select(row => row.Cell(6).GetString().Trim())
            .Distinct()
            .ToList();

        var manufacturerByName = dbContext.Manufacturers.ToDictionary(m => m.Name);

        foreach (var name in manufacturerNames)
        {
            if (!manufacturerByName.ContainsKey(name))
            {
                var manufacturer = new Manufacturer { Name = name };
                dbContext.Manufacturers.Add(manufacturer);
                manufacturerByName[name] = manufacturer;
            }
        }


        var categoryNames = rows
            .Select(row => row.Cell(7).GetString().Trim())
            .Distinct()
            .ToList();

        var categoryByName = dbContext.Categories.ToDictionary(c => c.Name);

        foreach (var name in categoryNames)
        {
            if (!categoryByName.ContainsKey(name))
            {
                var category = new Category { Name = name };
                dbContext.Categories.Add(category);
                categoryByName[name] = category;
            }
        }

        dbContext.SaveChanges();


        foreach (var row in rows)
        {
            var art = row.Cell(1).GetString().Trim();
            var name = row.Cell(2).GetString().Trim();
            var unitName = row.Cell(3).GetString().Trim();
            var price = row.Cell(4).GetValue<decimal>();
            var supplierName = row.Cell(5).GetString().Trim();
            var manufName = row.Cell(6).GetString().Trim();
            var categoryName = row.Cell(7).GetString().Trim();
            var discount = row.Cell(8).GetValue<int>();
            var stock = row.Cell(9).GetValue<int>();
            var description = row.Cell(10).GetString().Trim();
            var image = row.Cell(11).GetString().Trim();

            var product = new Product
            {
                Art = art,
                Name = name,
                UnitMeasureId = unitByName[unitName].Id,
                Price = price,
                SupplierId = supplierByName[supplierName].Id,
                ManufacturerId = manufacturerByName[manufName].Id,
                CategoryId = categoryByName[categoryName].Id,
                CurrentDiscount = discount,
                Stock = stock,
                Description = description,
                Image = image
            };

            dbContext.Products.Add(product);
        }

        dbContext.SaveChanges();
    }

    private static void SeedPickupPoints(AppDbContext ctx, string excelPath)
    {
        if (ctx.PickupPoints.Any()) return;

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheet(1);

        // В этом файле НЕТ заголовка — используем RowsUsed() БЕЗ Skip(1).
        // Каждая строка = 1 адрес.
        foreach (var row in sheet.RowsUsed())
        {
            var address = row.Cell(1).GetString().Trim();
            if (string.IsNullOrWhiteSpace(address)) continue;

            ctx.PickupPoints.Add(new PickupPoint { Address = address });
        }

        ctx.SaveChanges();
    }

    private static void SeedOrders(AppDbContext ctx, string excelPath)
    {
        if (ctx.Orders.Any()) return;

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheet(1);
        var rows = sheet.RowsUsed().Skip(1).ToList();

        // ===== ШАГ A: справочник статусов =====
        // Колонка 8 = статус. В данных встречается "Новый " с пробелом — Trim спасает.
        var statusNames = rows
            .Select(row => row.Cell(8).GetString().Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();

        var statusByName = ctx.OrderStatuses.ToDictionary(s => s.Name);

        foreach (var name in statusNames)
        {
            if (!statusByName.ContainsKey(name))
            {
                var status = new OrderStatus { Name = name };
                ctx.OrderStatuses.Add(status);
                statusByName[name] = status;
            }
        }

        ctx.SaveChanges();

        // ===== ШАГ B: загружаем справочники в память для быстрого поиска =====

        // PickupPointId в Excel — это ПОЗИЦИЯ адреса в файле пунктов выдачи (1, 2, 11 и т.д.).
        // У нас в БД у PickupPoint автоинкрементный Id, и если сидер пунктов отработал ПЕРВЫМ
        // и БД была пустой — Id'ы будут 1, 2, 3... ровно по порядку загрузки. То есть
        // позиция из Excel = Id в БД. Чтобы не зависеть от удачи, делаем явное сопоставление
        // через порядок по Id:
        var pickupByPosition = ctx.PickupPoints
            .OrderBy(p => p.Id)
            .ToList(); // индекс 0 = позиция 1, индекс 1 = позиция 2 ...

        // Товары — по артикулу. Артикул хранится в Product.Art.
        var productByArt = ctx.Products.ToDictionary(p => p.Art);

        // Пользователи — по ФИО. Это для поиска ClientId по строке клиента в Excel.
        var userByFio = ctx.Users
            .GroupBy(u => u.FullName)
            .ToDictionary(g => g.Key, g => g.First());

        // ===== ШАГ C: заказы + items =====

        foreach (var row in rows)
        {
            // Пропускаем пустые строки (бывает мусор в конце).
            if (row.Cell(1).IsEmpty()) continue;

            var orderArticle = row.Cell(1).GetValue<int>();
            var rawItems = row.Cell(2).GetString().Trim(); // "А112Т4, 2, F635R4, 2"
            var orderDate = row.Cell(3).GetDateTime();
            var deliveryDate = row.Cell(4).GetDateTime();
            var pickupPos = row.Cell(5).GetValue<int>(); // позиция пункта выдачи (1..36)
            var clientFio = row.Cell(6).GetString().Trim();
            var pickupCode = row.Cell(7).GetValue<int>();
            var statusName = row.Cell(8).GetString().Trim();

            // Находим пункт выдачи по позиции. Позиция 1 = индекс 0.
            if (pickupPos < 1 || pickupPos > pickupByPosition.Count)
                continue; // битая ссылка — пропускаем строку
            var pickup = pickupByPosition[pickupPos - 1];

            // Находим клиента (может не найтись — это допустимо, поле nullable).
            userByFio.TryGetValue(clientFio, out var client);

            // Создаём заказ. SaveChanges позже одним пакетом.
            var order = new Order
            {
                OrderArticle = orderArticle,
                OrderDate = orderDate,
                DeliveryDate = deliveryDate,
                PickupPointId = pickup.Id,
                ClientId = client?.Id, // null если не нашли
                PickupCode = pickupCode,
                StatusId = statusByName[statusName].Id
            };

            // ----- Парсинг "артикул, кол-во, артикул, кол-во, ..." -----
            var parts = rawItems
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            // Идём парами. Если последняя часть осталась без пары — игнорируем её.
            for (int i = 0; i + 1 < parts.Length; i += 2)
            {
                var art = parts[i];
                if (!int.TryParse(parts[i + 1], out var qty)) continue;
                if (!productByArt.TryGetValue(art, out var product)) continue;

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = qty
                });
            }

            ctx.Orders.Add(order);
        }

        ctx.SaveChanges();
    }
}