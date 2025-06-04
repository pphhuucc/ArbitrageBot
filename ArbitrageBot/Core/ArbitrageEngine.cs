using ArbitrageBot.Models;
using ArbitrageBot.Services.Interfaces;
using ArbitrageBot.Services;

namespace ArbitrageBot.Core
{
    public class ArbitrageEngine
    {
        private readonly List<IExchangeService> _exchanges;
        private readonly DynamicContractVerificationService _contractVerifier;

        public ArbitrageEngine(List<IExchangeService> exchanges)
        {
            _exchanges = exchanges;
            _contractVerifier = new DynamicContractVerificationService();
        }

        public async Task<ArbitrageOpportunity> FindOpportunityAsync(string symbol, decimal feePercent = 0.1m)
        {
            var prices = new List<(string exchange, decimal price)>();

            // Lấy giá từ các sàn
            foreach (var exchange in _exchanges)
            {
                try
                {
                    var price = await exchange.GetPriceAsync(symbol);
                    prices.Add((exchange.ExchangeName, price));
                }
                catch { continue; }
            }

            if (prices.Count < 2)
                return null;

            var buy = prices.OrderBy(p => p.price).First();
            var sell = prices.OrderByDescending(p => p.price).First();
            var profitPercent = (sell.price - buy.price) / buy.price * 100 - (feePercent * 2);

            // Initialize contract verification variables
            bool isContractVerified = false;
            string verificationNote = "";
            string buyContract = "N/A";
            string sellContract = "N/A";

            // Verify contract chỉ khi có lợi nhuận
            if (profitPercent > 0.1m)
            {
                Console.WriteLine($"🔍 Đang verify contract cho {symbol}...");
                
                try
                {
                    var verificationResult = await _contractVerifier.VerifyContractMatchAsync(
                        symbol, buy.exchange, sell.exchange);

                    isContractVerified = verificationResult.isVerified;
                    buyContract = verificationResult.buyContract;
                    sellContract = verificationResult.sellContract;
                    verificationNote = verificationResult.note;
                }
                catch (Exception ex)
                {
                    verificationNote = $"❌ Lỗi verify: {ex.Message}";
                }
            }

            return new ArbitrageOpportunity
            {
                Symbol = symbol,
                BuyExchange = buy.exchange,
                BuyPrice = buy.price,
                SellExchange = sell.exchange,
                SellPrice = sell.price,
                ProfitPercent = Math.Round(profitPercent, 2),
                IsContractVerified = isContractVerified,
                BuyContractAddress = buyContract,
                SellContractAddress = sellContract,
                VerificationNote = verificationNote
            };
        }

        public void Dispose()
        {
            _contractVerifier?.Dispose();
        }
    }
}