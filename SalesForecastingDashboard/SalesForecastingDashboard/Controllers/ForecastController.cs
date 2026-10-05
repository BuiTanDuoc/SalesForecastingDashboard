using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesForecastingDashboard.Data;
using SalesForecastingDashboard.Models;


namespace SalesForecastingDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForecastController : ControllerBase
    {
        private readonly ProjectContext _context;

        public ForecastController(ProjectContext context)
        {
            _context = context;
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetForecast(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            var orders = await _context.Orders
                .Where(o => o.ProductId == productId)
                .OrderBy(o => o.OrderDate)
                .Select(o => o.Quantity)
                .ToListAsync();

            if (!orders.Any())
            {
                return Ok(new ForecastResult
                {
                    Labels = new List<string> { "No Data" },
                    Values = new List<double> { 0 }
                });
            }

            int window = 3;
            var forecastValues = new List<double>();

            for (int i = 0; i < orders.Count; i++)
            {
                int start = Math.Max(0, i - window + 1);
                var slice = orders.Skip(start).Take(i - start + 1).ToList();
                forecastValues.Add(slice.Average());
            }

            var result = new ForecastResult
            {
                Labels = Enumerable.Range(1, forecastValues.Count).Select(i => $"Day {i}").ToList(),
                Values = forecastValues,
                AvgDailySales = forecastValues.Average(),
                DaysUntilSafety = (int)((product.Stock - product.SafetyStock) / (forecastValues.Average())),
                RecommendedOrderQty = Math.Max(0, product.SafetyStock * 2 - product.Stock)
            };

            return Ok(result);
        }
    }
}
