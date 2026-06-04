namespace Snek.Models;

public class Order
{
    public int Id { get; set; }
    public int OrderArticle { get; set; }  
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public int PickupCode { get; set; }

    public int PickupPointId { get; set; }
    public virtual PickupPoint PickupPoint { get; set; } = null!;

    public int StatusId { get; set; }
    public virtual OrderStatus Status { get; set; } = null!;

    public int? ClientId { get; set; }
    public virtual User? Client { get; set; }

    public virtual List<OrderItem> Items { get; set; } = new();
}