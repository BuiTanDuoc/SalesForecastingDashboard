
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesForecastingDashboard.Models;
using System.Text;
using System.Globalization;
using SalesForecastingDashboard.Data;

namespace SalesForecastingDashboard.Controllers
{
    [Route("export")]
    public class ExportController : Controller
    {
        private readonly ProjectContext _context;

        public ExportController(ProjectContext ctx)
        {
            _context = ctx;
        }

        /* ============================================================
         * 1) FULL ANALYTICS EXPORT
         * ============================================================ */
        [HttpGet("full-analytics")]
        public async Task<IActionResult> ExportFullAnalyticsCsv()
        {
            var sb = new StringBuilder();

            /* ------------------------------------------------------------
             * [SalesTrend]
             * ------------------------------------------------------------ */
            sb.AppendLine("[SalesTrend]");
            sb.AppendLine("Date,Sales");

            var fromDate = DateTime.UtcNow.Date.AddDays(-29);

            var dailySales = await _context.Orders
                .Where(o => o.OrderDate >= fromDate)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Day = g.Key, Qty = g.Sum(x => x.Quantity) })
                .OrderBy(x => x.Day)
                .ToListAsync();

            for (int i = 0; i < 30; i++)
            {
                var date = fromDate.AddDays(i);
                var qty = dailySales.FirstOrDefault(x => x.Day == date)?.Qty ?? 0;

                sb.AppendLine($"{date:yyyy-MM-dd},{qty}");
            }

            sb.AppendLine();
            sb.AppendLine();

            /* ------------------------------------------------------------
             * [ProductList]
             * ------------------------------------------------------------ */
            sb.AppendLine("[ProductList]");
            sb.AppendLine("ProductId,ProductName,Stock,SafetyStock");

            var products = await _context.Products
                .OrderBy(p => p.Id)
                .ToListAsync();

            foreach (var p in products)
            {
                sb.AppendLine(string.Join(",", new string[]
                {
                    p.Id.ToString(CultureInfo.InvariantCulture),
                    p.Name,
                    p.Stock.ToString(CultureInfo.InvariantCulture),
                    p.SafetyStock.ToString(CultureInfo.InvariantCulture)
                }));
            }

            sb.AppendLine();
            sb.AppendLine();

            /* ------------------------------------------------------------
             * PRODUCT SALES DATA
             * ------------------------------------------------------------ */
            var productSales = await _context.Orders
                .GroupBy(o => new { o.ProductId, o.Product.Name })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.Name,
                    TotalQty = g.Sum(x => x.Quantity),
                    FirstDate = g.Min(o => o.OrderDate),
                    LastDate = g.Max(o => o.OrderDate)
                })
                .ToListAsync();

            /* ------------------------------------------------------------
             * [ProductForecast]
             * ------------------------------------------------------------ */
            sb.AppendLine("[ProductForecast]");
            sb.AppendLine("ProductId,ProductName,AvgDailySales,Day1,Day2,Day3,Day4,Day5,Day6,Day7");

            foreach (var s in productSales)
            {
                var days = Math.Max(1, (s.LastDate.Date - s.FirstDate.Date).Days + 1);
                var avgDaily = (double)s.TotalQty / days;

                var forecast7 = Enumerable.Range(1, 7)
                    .Select(d => (avgDaily * d).ToString("F2", CultureInfo.InvariantCulture))
                    .ToArray();

                sb.AppendLine(string.Join(",", new string[]
                {
                    s.ProductId.ToString(CultureInfo.InvariantCulture),
                    s.Name,
                    avgDaily.ToString("F2", CultureInfo.InvariantCulture),
                    forecast7[0],
                    forecast7[1],
                    forecast7[2],
                    forecast7[3],
                    forecast7[4],
                    forecast7[5],
                    forecast7[6]
                }));
            }

            sb.AppendLine();
            sb.AppendLine();

            /* ------------------------------------------------------------
             * [ReorderSuggestions]  
             * ------------------------------------------------------------ */
            sb.AppendLine("[ReorderSuggestions]");
            sb.AppendLine("Product,CurrentStock,SafetyStock,AvgDailySales,DaysUntilSafety,RecommendedOrderQty");

           

            foreach (var s in productSales)
            {
                var product = products.First(p => p.Id == s.ProductId);

                var daysData = Math.Max(1, (s.LastDate.Date - s.FirstDate.Date).Days + 1);
                var avg = (double)s.TotalQty / daysData;

                var diff = product.Stock - product.SafetyStock;

                int daysUntilSafety =
                    avg <= 0 ? int.MaxValue :
                    diff <= 0 ? 0 :
                    (int)Math.Floor(diff / avg);

                int target = product.SafetyStock * 2;
                int recommendedQty = Math.Max(0, target - product.Stock);

                sb.AppendLine(string.Join(",", new string[]
                {
                    product.Name,
                    product.Stock.ToString(CultureInfo.InvariantCulture),
                    product.SafetyStock.ToString(CultureInfo.InvariantCulture),
                    avg.ToString("F2", CultureInfo.InvariantCulture),
                    (daysUntilSafety == int.MaxValue ? "-" : daysUntilSafety.ToString(CultureInfo.InvariantCulture)),
                    recommendedQty.ToString(CultureInfo.InvariantCulture)
                }));
            }

            /* ------------------------------------------------------------
             * RETURN FINAL CSV
             * ------------------------------------------------------------ */
            return File(
                Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                $"analytics_export_{DateTime.Now:yyyyMMdd}.csv"
            );
        }

    }
}
