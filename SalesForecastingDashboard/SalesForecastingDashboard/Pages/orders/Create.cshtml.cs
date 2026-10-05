
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using SalesForecastingDashboard.Models;
using SalesForecastingDashboard.Data;

namespace SalesForecastingDashboard.Pages.orders
{
    public class CreateModel : PageModel
    {
        private readonly ProjectContext _context;

        public CreateModel(ProjectContext context)
        {
            _context = context;

            // PostgreSQL timestamp fix (UTC zorunluluğu)
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        [BindProperty]
        public Order Order { get; set; } = new();

        public List<Product> Products { get; set; } = new();

        public SelectList? ProductList { get; set; }

        [BindProperty]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            Products = await _context.Products.ToListAsync();
            ProductList = new SelectList(Products, "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Products = await _context.Products.ToListAsync();
            ProductList = new SelectList(Products, "Id", "Name");

            if (Order.ProductId == 0 || Order.Quantity <= 0)
                return Page();

            // --- PRODUCT GET ---
            var product = await _context.Products.FirstAsync(p => p.Id == Order.ProductId);

           
            if (product.CreatedAt.Kind != DateTimeKind.Utc)
                product.CreatedAt = DateTime.SpecifyKind(product.CreatedAt, DateTimeKind.Utc);

            if (Order.Quantity > product.Stock)
            {
                ErrorMessage = $"Only {product.Stock} units available!";
                return Page();
            }

            // --- STOCK UPDATE ---
            product.Stock -= Order.Quantity;
            if (product.Stock < 0)
                product.Stock = 0;

            // --- ORDER DATE (force UTC) ---
            Order.OrderDate = DateTime.UtcNow;

            // --- SAVE ORDER ---
            _context.Orders.Add(Order);
            await _context.SaveChangesAsync(); // Order.Id burada oluşur

            // --- NOTIFICATION ---
            if (product.Stock <= product.SafetyStock)
            {
                bool exists = await _context.Notifications
                    .AnyAsync(n => n.ProductId == product.Id && !n.IsRead);

                if (!exists)
                {
                    var notification = new Notification
                    {
                        ProductId = product.Id,
                        Message = $"LOW STOCK ALERT: {product.Name} — Current Stock: {product.Stock}",

                        // Force UTC
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }
            }

            // --- COOKIE ---
            Response.Cookies.Append(
                "NewOrderId",
                Order.Id.ToString(),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddMinutes(5),
                    HttpOnly = false
                }
            );

            return RedirectToPage("/orders/Index");
        }
    }
}
