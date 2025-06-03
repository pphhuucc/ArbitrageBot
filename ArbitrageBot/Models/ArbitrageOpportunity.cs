namespace ArbitrageBot.Models
{
    public class ArbitrageOpportunity
    {
        public string BuyExchange { get; set; }
        public string SellExchange { get; set; }
        public string Symbol { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
        public decimal ProfitPercent { get; set; }
    }
}
