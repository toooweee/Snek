namespace Snek.Models;

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public virtual List<Product> Products { get; set; } = new();
}