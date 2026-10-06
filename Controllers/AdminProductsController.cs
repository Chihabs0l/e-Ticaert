using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data;
using ShoppingApp.Models;

namespace ShoppingApp.Controllers;

/// <summary>ADMIN — full product CRUD.</summary>
[Authorize(Roles = "Admin")]
public class AdminProductsController : Controller
{
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".avif" };
    private const long MaxImageSize = 5 * 1024 * 1024; // 5 MB

    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminProductsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // GET /AdminProducts
    public async Task<IActionResult> Index()
        => View(await _db.Products.OrderBy(p => p.Name).ToListAsync());

    // GET /AdminProducts/Create
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
    {
        // Save the uploaded image into wwwroot/Images and store its public path.
        if (imageFile is { Length: > 0 })
        {
            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!AllowedImageExtensions.Contains(ext))
                ModelState.AddModelError("imageFile", "Only JPG, PNG, WEBP, GIF or AVIF images are allowed.");
            else if (imageFile.Length > MaxImageSize)
                ModelState.AddModelError("imageFile", "Image must be 5 MB or smaller.");
            else
            {
                var imagesFolder = Path.Combine(_env.WebRootPath, "Images");
                Directory.CreateDirectory(imagesFolder);

                // Unique name so uploads never overwrite each other, keep original extension.
                var fileName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(imagesFolder, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                    await imageFile.CopyToAsync(stream);

                product.ImageUrl = $"/Images/{fileName}";
            }
        }

        if (!ModelState.IsValid) return View(product);

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Product created.";
        return RedirectToAction(nameof(Index));
    }

    // GET /AdminProducts/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product product)
    {
        if (!ModelState.IsValid) return View(product);

        _db.Products.Update(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Product updated.";
        return RedirectToAction(nameof(Index));
    }

    // GET /AdminProducts/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Product deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
