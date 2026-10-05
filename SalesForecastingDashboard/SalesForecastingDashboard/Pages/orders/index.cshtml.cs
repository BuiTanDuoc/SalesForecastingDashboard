
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalesForecastingDashboard.Data;
using SalesForecastingDashboard.Models;

namespace SalesForecastingDashboard.Pages.orders
{
    public class IndexModel : PageModel
    {
        private readonly ProjectContext _context;

        public IndexModel(ProjectContext context)
        {
            _context = context;
        }

        public List<Order> Orders { get; set; } = new();
        public int? NewOrderId { get; set; }

        public async Task OnGetAsync(int? newOrderId)
        {
            NewOrderId = newOrderId ?? 0;

            Orders = await _context.Orders
                .Include(o => o.Product)
                .OrderByDescending(o => o.Id)
                .ToListAsync();




        }
    }
}
