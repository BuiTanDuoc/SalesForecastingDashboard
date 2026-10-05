
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalesForecastingDashboard.Data;
using SalesForecastingDashboard.Models;

namespace SalesForecastingDashboard.Pages.Home
{
    public class IndexModel : PageModel
    {
        private readonly ProjectContext _context;

        public IndexModel(ProjectContext context)
        {
            _context = context; //db sorguları bunun uzerınden yaapılır postgreye baglanmayı sağlr
        }

        // SUMMARY CARDS
        public int TotalProducts { get; set; }
        public int OrdersToday { get; set; }
        public int Last7DaysSales { get; set; }
        public int LowStockCount { get; set; }

        // TODAY PANEL
        public int TodayTotalSales { get; set; }
        public int TodayOrdersCount { get; set; }
        public string? TodayTopProductName { get; set; }
        public int TodayTopProductQty { get; set; }
        public double TodayAvgDailySales { get; set; }

        // CHART DATA
        public string ChartLabelsJson { get; set; } = "[]";
        public string ChartDataJson { get; set; } = "[]";

        // TOP 5 MODEL
        public class TopProductVm
        {
            public string Name { get; set; } = "";
            public int TotalSold { get; set; }

            
            public double BarWidthPercent { get; set; }
        }

        // the list for razor page
        public List<TopProductVm> TopProducts { get; set; } = new();


        public async Task OnGet()
        {
            // PostgreSQL timestamptz matching
            var utcNow = DateTime.UtcNow;
            var today = utcNow.Date;
            var sevenDaysAgo = today.AddDays(-6);
            var thirtyDaysAgo = today.AddDays(-29);

            // SUMMARY
            TotalProducts = await _context.Products.CountAsync();
            LowStockCount = await _context.Products.CountAsync(p => p.Stock <= p.SafetyStock);

            // LAST 7 DAYS
            var ordersLast7 = await _context.Orders
                .Where(o => o.OrderDate >= sevenDaysAgo && o.OrderDate < today.AddDays(1))
                .ToListAsync();

            Last7DaysSales = ordersLast7.Sum(o => o.Quantity);

            // TODAY
            var ordersToday = ordersLast7
                .Where(o => o.OrderDate.Date == today)
                .ToList();

            TodayOrdersCount = ordersToday.Count;
            OrdersToday = TodayOrdersCount;
            TodayTotalSales = ordersToday.Sum(o => o.Quantity);

            TodayAvgDailySales = Last7DaysSales > 0
                ? Math.Round(Last7DaysSales / 7.0, 1)
                : 0;

            if (ordersToday.Any())
            {
                var top = ordersToday
                    .GroupBy(o => o.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        Qty = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.Qty)
                    .First();

                var product = await _context.Products.FindAsync(top.ProductId);
                TodayTopProductName = product?.Name ?? "N/A";
                TodayTopProductQty = top.Qty;
            }
            else
            {
                TodayTopProductName = "No sales yet";
                TodayTopProductQty = 0;
            }

            // CHART — LAST 7 DAYS
            var labels = new List<string>();
            var chartValues = new List<int>();

            for (int i = 0; i < 7; i++)
            {
                var date = sevenDaysAgo.AddDays(i);
                labels.Add(date.ToString("dd MMM"));

                var total = ordersLast7
                    .Where(o => o.OrderDate.Date == date)
                    .Sum(o => o.Quantity);

                chartValues.Add(total);
            }

            ChartLabelsJson = JsonSerializer.Serialize(labels);
            ChartDataJson = JsonSerializer.Serialize(chartValues);

         
            // TOP 5 BEST SELLERS — LAST 30 DAYS
            

            var orders30 = await _context.Orders
                .Include(o => o.Product)
                .Where(o => o.OrderDate >= thirtyDaysAgo && o.OrderDate < today.AddDays(1))
                .ToListAsync();

            var grouped = orders30
                .Where(o => o.Product != null)
                .GroupBy(o => new { o.ProductId, o.Product!.Name })
                .Select(g => new TopProductVm
                {
                    Name = g.Key.Name,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToList();

            // max = %100
            int maxSold = grouped.FirstOrDefault()?.TotalSold ?? 1;

            foreach (var item in grouped)
            {
               item.BarWidthPercent = Math.Round((double)item.TotalSold / maxSold * 100, 2);
            }

           
            TopProducts = grouped;
        }
    }
}
