using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesForecastingDashboard.Models
{
    [Table("orders")]   // PostgreSQL küçük harf istiyor
    public class Order
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]     // DB ile birebir aynı olsun diye ekledim
        public int ProductId { get; set; }
        
       [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    }
}
