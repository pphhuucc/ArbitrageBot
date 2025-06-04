using ArbitrageBot.Services.Interfaces;
using System;
using System.Threading.Tasks;
using ccxt;

namespace ArbitrageBot.Services
{
    public class CoinbaseService : IExchangeService
    {
        private readonly dynamic _coinbase;

        public CoinbaseService()
        {
            _coinbase = new coinbase(); // Sửa từ coinbasepro thành coinbase
        }

        public string ExchangeName => "Coinbase";

        public async Task<decimal> GetPriceAsync(string symbol)
        {
            try
            {
                // Coinbase sử dụng format: BTC-USD thay vì BTC/USDT
                var coinbaseSymbol = symbol.Replace("/USDT", "-USD").Replace("/BUSD", "-USD").Replace("/USDC", "-USD");
                
                dynamic ticker = await _coinbase.fetchTicker(coinbaseSymbol);
                return Convert.ToDecimal(ticker["last"]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Coinbase API error: {ex.Message}");
            }
        }
    }
}