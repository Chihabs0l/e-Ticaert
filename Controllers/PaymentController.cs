using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;
using ShoppingApp.ViewModels;

namespace ShoppingApp.Controllers;

/// <summary>
/// MOCK payment gateway — simulates a real PSP flow (Stripe/PayPal style).
/// Card 4242 4242 4242 4242 always succeeds; any other card is declined.
/// No real API is called and no card data is stored.
/// </summary>
[Authorize(Roles = "Client")]
public class PaymentController : Controller
{
    private readonly AppDbContext _db;

    public PaymentController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Pay(int orderId)
    {
        var order = await GetOwnedOrder(orderId);
        if (order == null) return NotFound();

        if (order.Status != OrderStatus.PendingPayment)
            return RedirectToAction("Details", "Orders", new { id = order.Id });

        return View(new PaymentViewModel { OrderId = order.Id, Amount = order.Total });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(PaymentViewModel model)
    {
        var order = await GetOwnedOrder(model.OrderId);
        if (order == null) return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        // --- Fake gateway logic ---
        var cardDigits = model.CardNumber.Replace(" ", "").Replace("-", "");
        bool approved = cardDigits == "4242424242424242"
                        && model.Expiry.Length == 5
                        && model.Cvv.Length >= 3;

        if (!approved)
        {
            ModelState.AddModelError("", "Payment declined by the gateway. Use test card 4242 4242 4242 4242.");
            return View(model);
        }

        // Success → mark order as paid
        order.Status = OrderStatus.Processing;
        order.PaymentReference = "MOCK-" + Guid.NewGuid().ToString("N")[..12].ToUpper();
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Success), new { orderId = order.Id });
    }

    public async Task<IActionResult> Success(int orderId)
    {
        var order = await GetOwnedOrder(orderId);
        if (order == null) return NotFound();
        return View(order);
    }

    private async Task<Order?> GetOwnedOrder(int orderId)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        return await _db.Orders.Include(o => o.Items)
                               .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
    }
}
