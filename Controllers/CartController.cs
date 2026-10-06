using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;

namespace ShoppingApp.Controllers;

[Authorize(Roles = "Client")]
public class CartController : Controller
{
    private const string CartKey = "ShoppingCart";
    private readonly AppDbContext _db;

    public CartController(AppDbContext db) => _db = db;

    private List<CartItem> GetCart()
    {
        var json = HttpContext.Session.GetString(CartKey);
        return string.IsNullOrEmpty(json)
            ? new List<CartItem>()
            : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
    }

    private void SaveCart(List<CartItem> cart)
        => HttpContext.Session.SetString(CartKey, JsonSerializer.Serialize(cart));

    public IActionResult Index()
    {
        var cart = GetCart();
        ViewBag.Total = cart.Sum(c => c.Subtotal);
        return View(cart);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null) return NotFound();

        var cart = GetCart();
        var item = cart.FirstOrDefault(c => c.ProductId == productId);
        if (item != null) item.Quantity += quantity;
        else cart.Add(new CartItem { ProductId = product.Id, Name = product.Name, Price = product.Price, Quantity = quantity });

        SaveCart(cart);
        TempData["Success"] = $"{product.Name} added to cart.";
        return RedirectToAction("Index", "Products");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        var cart = GetCart();
        cart.RemoveAll(c => c.ProductId == productId);
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    // ---------- CHECKOUT: create order (PendingPayment) -> go to mock payment ----------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        var cart = GetCart();
        if (!cart.Any())
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        // Re-check stock against the database
        foreach (var item in cart)
        {
            var product = await _db.Products.FindAsync(item.ProductId);
            if (product == null || product.Stock < item.Quantity)
            {
                TempData["Error"] = $"Not enough stock for {item.Name}.";
                return RedirectToAction(nameof(Index));
            }
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.Now,
            Total = cart.Sum(c => c.Subtotal),
            Status = OrderStatus.PendingPayment,
            Items = cart.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                ProductName = c.Name,
                UnitPrice = c.Price,
                Quantity = c.Quantity
            }).ToList()
        };

        _db.Orders.Add(order);

        // Reserve stock
        foreach (var item in cart)
        {
            var product = (await _db.Products.FindAsync(item.ProductId))!;
            product.Stock -= item.Quantity;
        }

        await _db.SaveChangesAsync();
        SaveCart(new List<CartItem>()); // clear cart

        // Redirect to the (mock) payment gateway
        return RedirectToAction("Pay", "Payment", new { orderId = order.Id });
    }
}
