using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;

namespace ShoppingApp.Controllers;

/// <summary>ADMIN — view all orders and change order status.</summary>
[Authorize(Roles = "Admin")]
public class AdminOrdersController : Controller
{
    private readonly AppDbContext _db;

    public AdminOrdersController(AppDbContext db) => _db = db;

    // GET /AdminOrders
    public async Task<IActionResult> Index()
    {
        var orders = await _db.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return View(orders);
    }

    // GET /AdminOrders/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();
        return View(order);
    }

    // POST /AdminOrders/UpdateStatus
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) return NotFound();

        order.Status = status;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Order #{order.Id} is now {status}.";
        return RedirectToAction(nameof(Details), new { id = orderId });
    }
}
