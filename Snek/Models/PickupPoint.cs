using DocumentFormat.OpenXml.Drawing.Charts;

namespace Snek.Models;

public class PickupPoint
{
    public int Id { get; set; }
    public string Address { get; set; } = null!;

    public virtual List<Order> Orders { get; set; } = new();
}