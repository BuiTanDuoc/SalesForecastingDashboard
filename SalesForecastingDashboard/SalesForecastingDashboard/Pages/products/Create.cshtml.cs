using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalesForecastingDashboard.Data;
using SalesForecastingDashboard.Models;

namespace SalesForecastingDashboard.Pages.products
{
    public class CreateModel : PageModel
    {
        private readonly ProjectContext _context;

        public CreateModel(ProjectContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        [BindProperty]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            string name = Product.Name.Trim().ToLower();

            var existing = await _context.Products
                .FirstOrDefaultAsync(p => p.Name.ToLower() == name);

            // --- ÜRÜN VARSA ---
            if (existing != null)
            {
                existing.Stock += Product.Stock; // stok ekle
                await _context.SaveChangesAsync();

                SuccessMessage =
                 $"The product '{existing.Name}' already exists. Updated stock level: {existing.Stock}.";


                return Page();
            }

            // --- YENİ ÜRÜN EKLE ---
            Product.Name = Product.Name.Trim();
            _context.Products.Add(Product);
            await _context.SaveChangesAsync();

           SuccessMessage = $"The new product '{Product.Name}' has been added successfully.";


            return Page();
        }
    }
}
