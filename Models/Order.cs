using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models;

public enum OrderStatus
{
    PendingPayment = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public decimal Total { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;

    /// <summary>Reference returned by the (mock) payment gateway.</summary>
    [StringLength(100)]
    public string? PaymentReference { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}
