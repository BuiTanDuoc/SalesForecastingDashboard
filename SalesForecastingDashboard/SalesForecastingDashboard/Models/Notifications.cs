using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesForecastingDashboard.Models
{
    [Table("notifications")]
    public class Notification
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("product_id")]
        public int ProductId { get; set; }

        // navigation
        public Product? Product { get; set; }
    }
}
