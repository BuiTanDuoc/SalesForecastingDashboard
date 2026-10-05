using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalesForecastingDashboard.Data;
using SalesForecastingDashboard.Models;

namespace SalesForecastingDashboard.Pages.notifications
{
    public class IndexModel : PageModel
    {
        private readonly ProjectContext _context;

        public IndexModel(ProjectContext context)
        {
            _context = context;
        }

        public List<Notification> Notifications { get; set; } = new();

        // --- SAYFAYI GETİR ---
        public async Task OnGetAsync()
        {
            Notifications = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }


        // --- MARK ALL AS READ ---
        public async Task<IActionResult> OnPostMarkAllAsRead()
        {
            var notifs = await _context.Notifications
                .Where(n => !n.IsRead)
                .ToListAsync();

            foreach (var n in notifs)
                n.IsRead = true;

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }


        // --- DELETE ALL ---
        public async Task<IActionResult> OnPostDeleteAll()
        {
            var all = _context.Notifications;
            _context.Notifications.RemoveRange(all);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
