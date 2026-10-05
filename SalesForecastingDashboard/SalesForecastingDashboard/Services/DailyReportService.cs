using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SalesForecastingDashboard.Models;
using Microsoft.EntityFrameworkCore;
using SalesForecastingDashboard.Data;

namespace SalesForecastingDashboard.Services
{
    public class DailyReportService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<DailyReportService> _logger;

        public DailyReportService(IServiceProvider provider, ILogger<DailyReportService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DailyReportService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // --- 1) YEREL SAATTE 00:30'U HEDEFLE ---
                var nowLocal = DateTime.Now; // senin makinenin saati
                var runTimeLocal = nowLocal.Date.AddHours(11).AddMinutes(32); // 00:30

                if (runTimeLocal <= nowLocal)
                {
                    //  ertesi günün 00:30'una al
                    runTimeLocal = runTimeLocal.AddDays(1);
                }

                var delay = runTimeLocal - nowLocal;

                
                if (delay <= TimeSpan.Zero)
                {
                    delay = TimeSpan.FromSeconds(10);
                }

                _logger.LogInformation(
                    $"📅 Next daily report scheduled for {runTimeLocal:yyyy-MM-dd HH:mm:ss} (in {delay})."
                );

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Uygulama kapanırken buraya düşer, while'dan çık
                    break;
                }

                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    await SendDailyReport(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Daily Report Error");
                }
            }
        }

        private async Task SendDailyReport(CancellationToken token)
        {
            using var scope = _provider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ProjectContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            // >>> UTC TARİH ARALIĞI (DB için) <<<
            var todayUtc = DateTime.UtcNow.Date;
            var tomorrowUtc = todayUtc.AddDays(1);

            // LOW STOCK
            var lowStock = await context.Products
                .Where(p => p.Stock <= p.SafetyStock)
                .ToListAsync(token);

            // BUGÜN VERİLEN SİPARİŞLER (UTC bazlı)
            var ordersToday = await context.Orders
                .Where(o => o.OrderDate >= todayUtc && o.OrderDate < tomorrowUtc)
                .Include(o => o.Product)
                .ToListAsync(token);

            // --- EMAIL BODY ---
            string message = "<h2>📊 Daily Inventory Report</h2>";

            // LOW STOCK LIST
            message += "<h3>⚠ Low Stock Items</h3>";
            if (lowStock.Any())
            {
                foreach (var item in lowStock)
                    message += $"<p><b>{item.Name}</b>: Stock {item.Stock}</p>";
            }
            else
            {
                message += "<p>No low stock items 🎉</p>";
            }

            // ORDERS
            message += "<h3>📦 Orders Today</h3>";
            if (ordersToday.Any())
            {
                foreach (var o in ordersToday)
                    message += $"<p>Order #{o.Id} — {o.Product.Name} — Qty: {o.Quantity}</p>";
            }
            else
            {
                message += "<p>No new orders today.</p>";
            }

            await emailService.SendEmailAsync(
                to: "brcksdagi@gmail.com",
                subject: "📅 Daily Inventory Report",
                body: message
            );
        }
    }
}
