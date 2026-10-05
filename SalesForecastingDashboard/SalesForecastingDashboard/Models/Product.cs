using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesForecastingDashboard.Models
{
    [Table("products")]
    public class Product
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("stock")]
        public int Stock { get; set; }

        [Column("safety_stock")]
        public int SafetyStock { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Order> Orders { get; set; } = new();

       [Column("alertsent")]
        public bool AlertSent { get; set; } = false;


    }
}
