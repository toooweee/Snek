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
}