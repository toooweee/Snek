using DocumentFormat.OpenXml.Drawing.Charts;

namespace Snek.Models;

public class OrderStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public virtual List<Order> Orders { get; set; } = new();
}