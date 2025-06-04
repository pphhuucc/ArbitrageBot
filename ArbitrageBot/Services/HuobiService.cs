using ArbitrageBot.Services.Interfaces;
using System;
using System.Threading.Tasks;
using ccxt;

namespace ArbitrageBot.Services
{
    public class HuobiService : IExchangeService
    {
        private readonly dynamic _huobi;

        public HuobiService()
        {
            _huobi = new huobi();
        }

        public string ExchangeName => "Huobi";

        public async Task<decimal> GetPriceAsync(string symbol)
        {
            try
            {
                dynamic ticker = await _huobi.fetchTicker(symbol);
                return Convert.ToDecimal(ticker["last"]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Huobi API error: {ex.Message}");
            }
        }
    }
}