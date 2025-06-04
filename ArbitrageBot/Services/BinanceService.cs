using ArbitrageBot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ccxt;

namespace ArbitrageBot.Services
{
    public class BinanceService : IExchangeService
    {
        private readonly dynamic _binance;

        public BinanceService()
        {
            _binance = new binance();
        }

        public string ExchangeName => "Binance";

        public async Task<decimal> GetPriceAsync(string symbol)
        {
            dynamic ticker = await _binance.fetchTicker(symbol);
            return Convert.ToDecimal(ticker["last"]);
        }
        
        public async Task<List<string>> GetCommonSymbolsAsync(string quote = "USDT")
        {
            var symbols = new List<string>();
            dynamic markets = await _binance.fetchMarkets();

            foreach (var market in markets)
            {
                if (market["quote"] == quote && market["active"] == true)
                {
                    symbols.Add(market["symbol"]);
                }
            }

            return symbols;
        }
    }
}