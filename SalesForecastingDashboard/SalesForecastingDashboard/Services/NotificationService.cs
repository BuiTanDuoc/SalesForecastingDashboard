
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SalesForecastingDashboard.Models;
using SalesForecastingDashboard.Data;

namespace SalesForecastingDashboard.Services
{
    public class NotificationService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IServiceProvider services, ILogger<NotificationService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("✔ Notification service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckStockLevels(stoppingToken);

               

                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        private async Task CheckStockLevels(CancellationToken token)
        {
            using (var scope = _services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ProjectContext>();

                var products = await context.Products.ToListAsync(token);

                foreach (var p in products)
                {
                    // ----------------------
                    // 1) LOW STOCK TESPİTİ
                    // ----------------------
                    if (p.Stock <= p.SafetyStock)
                    {
                        if (!p.AlertSent)
                        {
                            // YENİ bildirim üret
                            var msg = $"LOW STOCK ALERT: {p.Name} — Stock: {p.Stock}";

                            _logger.LogWarning(msg);

                            context.Notifications.Add(new Notification
                            {
                                ProductId = p.Id,
                                Message = msg,
                                CreatedAt = DateTime.UtcNow,
                                IsRead = false
                            });

                            // FLAG ---> artık tekrar oluşturma
                            p.AlertSent = true;

                            await context.SaveChangesAsync(token);
                        }
                    }
                    else
                    {
                        // ----------------------
                        // 2) STOK NORMALE DÖNDÜ
                        // ----------------------
                        if (p.AlertSent)
                        {
                            p.AlertSent = false;
                            await context.SaveChangesAsync(token);
                        }
                    }
                }
            }
        }

        // -----------------------------------------
        // GÜNLÜK RAPOR – HER GÜN SADECE 1 KEZ EMAIL
        // -----------------------------------------
        private DateTime _lastDailyEmailSent = DateTime.MinValue;

        private async Task CheckSendDailyReport(CancellationToken token)
        {
            var now = DateTime.Now;

            // Saat 22:00 civarı ve daha önce gönderilmemişse
            if (now.Hour == 22 && _lastDailyEmailSent.Date != now.Date)
            {
                using var scope = _services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ProjectContext>();

                // O günün bildirimlerini çek
                var today = DateTime.UtcNow.Date;
                var todaysAlerts = await context.Notifications
                    .Where(n => n.CreatedAt.Date == today)
                    .OrderBy(n => n.CreatedAt)
                    .ToListAsync(token);

                // Email gönderme kısmını daha sonra tamamlayacağız
                _logger.LogInformation($"📧 Daily report prepared with {todaysAlerts.Count} alerts.");

                _lastDailyEmailSent = now;
            }
        }
    }
}
