using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalesForecastingDashboard.Data;
using SalesForecastingDashboard.Models;
using System.Text.Json;

namespace SalesForecastingDashboard.Pages.analytics
{
    public class IndexModel : PageModel
    {
        private readonly ProjectContext _context;

        public IndexModel(ProjectContext context)
        {
            _context = context;
        }

        // ---- TOTAL SALES GRAPH ----
        public string ChartLabelsJson { get; set; } = "[]";
        public string ChartDataJson  { get; set; } = "[]";

        // ---- PRODUCT BASED FORECAST ----
        public string ProductForecastJson { get; set; } = "[]";

        // ---- REORDER TABLE ----
        public class ReorderSuggestion
        {
            public string ProductName { get; set; } = string.Empty;
            public int CurrentStock { get; set; }
            public int SafetyStock { get; set; }
            public double AvgDailySales { get; set; }
            public int DaysUntilSafety { get; set; }
            public int RecommendedOrderQty { get; set; }
        }

        public List<ReorderSuggestion> ReorderSuggestions { get; set; } = new();

        public async Task OnGetAsync()
        {
            /* ============================================================
             * 1) TOTAL SALES GRAPH (LAST 30 DAYS)
             * ============================================================ */

            var fromDate = DateTime.UtcNow.Date.AddDays(-29);

            var dailySales = await _context.Orders
                .Where(o => o.OrderDate >= fromDate)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    Day = g.Key,
                    Qty = g.Sum(x => x.Quantity)
                })
                .OrderBy(x => x.Day)
                .ToListAsync();

            var allDays = Enumerable.Range(0, 30)
                .Select(offset => fromDate.AddDays(offset))
                .ToList();

            var labels = allDays.Select(d => d.ToString("MM-dd")).ToList();
            var data = allDays
                .Select(d => dailySales.FirstOrDefault(x => x.Day == d)?.Qty ?? 0)
                .ToList();

            ChartLabelsJson = JsonSerializer.Serialize(labels);
            ChartDataJson   = JsonSerializer.Serialize(data);



            /* ============================================================
             * 2) PRODUCT-BASED FORECAST (AVG DAILY SALES FOR EACH PRODUCT)
             * ============================================================ */

            var products = await _context.Products.ToListAsync();

            var productSales = await _context.Orders
                .GroupBy(o => new { o.ProductId, o.Product.Name })
                .Select(g => new
                {
                    g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalQty = g.Sum(x => x.Quantity),
                    FirstDate = g.Min(o => o.OrderDate),
                    LastDate = g.Max(o => o.OrderDate)
                })
                .ToListAsync();

            var forecastList = new List<object>();

            foreach (var s in productSales)
            {
                var daysOfData = Math.Max(1, (s.LastDate.Date - s.FirstDate.Date).Days + 1);
                var avgDaily = daysOfData > 0 ? (double)s.TotalQty / daysOfData : 0.0;

                forecastList.Add(new
                {
                    productId = s.ProductId,
                    productName = s.ProductName,
                    avgDailySales = Math.Round(avgDaily, 2)
                });
            }

            ProductForecastJson = JsonSerializer.Serialize(forecastList);



            /* ============================================================
             * 3) REORDER SUGGESTIONS TABLE
             * ============================================================ */

            const int horizonDays = 7;

            foreach (var s in productSales)
            {
                var product = products.FirstOrDefault(p => p.Id == s.ProductId);
                if (product == null) continue;

                var daysOfData = Math.Max(1, (s.LastDate.Date - s.FirstDate.Date).Days + 1);
                var avgDaily = daysOfData > 0 ? (double)s.TotalQty / daysOfData : 0.0;

                var diff = product.Stock - product.SafetyStock;

                int daysUntilSafety;
                if (avgDaily <= 0) daysUntilSafety = int.MaxValue;
                else if (diff <= 0) daysUntilSafety = 0;
                else daysUntilSafety = (int)Math.Floor(diff / avgDaily);

                var targetStock = product.SafetyStock * 2;
                var recommendQty = Math.Max(0, targetStock - product.Stock);

                if (diff <= 0 || daysUntilSafety <= horizonDays)
                {
                    ReorderSuggestions.Add(new ReorderSuggestion
                    {
                        ProductName = product.Name,
                        CurrentStock = product.Stock,
                        SafetyStock = product.SafetyStock,
                        AvgDailySales = Math.Round(avgDaily, 2),
                        DaysUntilSafety = daysUntilSafety,
                        RecommendedOrderQty = recommendQty
                    });
                }
            }
        }
    }
}
