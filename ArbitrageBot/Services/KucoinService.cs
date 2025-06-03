using ArbitrageBot.Services.Interfaces;
using System;
using System.Threading.Tasks;
using ccxt;

namespace ArbitrageBot.Services
{
    public class KucoinService : IExchangeService
    {
        private readonly dynamic _kucoin;

        public KucoinService()
        {
            _kucoin = new kucoin(); // Tên class viết thường: ccxt.kucoin
        }

        public string ExchangeName => "KuCoin";

        public async Task<decimal> GetPriceAsync(string symbol)
        {
            // ccxt dùng định dạng symbol kiểu "BTC/USDT"
            dynamic ticker = await _kucoin.fetchTicker(symbol);
            return Convert.ToDecimal(ticker["last"]);
        }
    }
}
