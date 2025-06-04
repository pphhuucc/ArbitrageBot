namespace ArbitrageBot.Models
{
    public class ArbitrageOpportunity
    {
        public string BuyExchange { get; set; } = "";
        public string SellExchange { get; set; } = "";
        public string Symbol { get; set; } = "";
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
        public decimal ProfitPercent { get; set; }
        public bool IsContractVerified { get; set; }
        public string BuyContractAddress { get; set; } = "";
        public string SellContractAddress { get; set; } = "";
        public string VerificationNote { get; set; } = "";
    }
}