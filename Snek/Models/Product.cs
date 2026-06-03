namespace Snek.Models;

public class Product
{
    public int Id { get; set; }
    public string Art { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int UnitMeasureId { get; set; }
    public virtual UnitMeasure UnitMeasure { get; set; } = null!;
    public decimal Price { get; set; }
    public int SupplierId { get; set; }
    public virtual Supplier Supplier { get; set; } = null!;
    public int ManufacturerId { get; set; }
    public virtual Manufacturer Manufacturer { get; set; } = null!;
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    public int CurrentDiscount { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; } = null!;
    public string Image { get; set; } = null!;
}