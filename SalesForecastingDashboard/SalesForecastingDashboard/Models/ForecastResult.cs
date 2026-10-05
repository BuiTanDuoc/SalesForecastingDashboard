namespace SalesForecastingDashboard.Models
{
    public class ForecastResult
    {
        public List<string> Labels { get; set; } = new();
        public List<double> Values { get; set; } = new();
        public double AvgDailySales { get; set; }
        public int DaysUntilSafety { get; set; }
        public int RecommendedOrderQty { get; set; }
    }
}
