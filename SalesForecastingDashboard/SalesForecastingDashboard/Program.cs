using Microsoft.EntityFrameworkCore;
using SalesForecastingDashboard.Data;
using SalesForecastingDashboard.Models;
using SalesForecastingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProjectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")) //verıtabanına baglandık
);

builder.Services.AddHostedService<NotificationService>();

builder.Services.AddRazorPages();

builder.Services.AddSingleton<EmailService>();
builder.Services.AddHostedService<DailyReportService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();

app.Run();
