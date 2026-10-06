namespace ShoppingApp.Models;

/// <summary>Stored in session — not persisted to the database.</summary>
public class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => Price * Quantity;
}
