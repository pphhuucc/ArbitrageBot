using ArbitrageBot.Models;
using ArbitrageBot.Services.Interfaces;

namespace ArbitrageBot.Core
{
    public class ArbitrageEngine
    {
        private readonly List<IExchangeService> _exchanges;

        public ArbitrageEngine(List<IExchangeService> exchanges)
        {
            _exchanges = exchanges;
        }

        public async Task<ArbitrageOpportunity> FindOpportunityAsync(string symbol, decimal feePercent = 0.1m)
        {
            var prices = new List<(string exchange, decimal price)>();

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

            return new ArbitrageOpportunity
            {
                Symbol = symbol,
                BuyExchange = buy.exchange,
                BuyPrice = buy.price,
                SellExchange = sell.exchange,
                SellPrice = sell.price,
                ProfitPercent = Math.Round(profitPercent, 2)
            };
        }

    }
}
