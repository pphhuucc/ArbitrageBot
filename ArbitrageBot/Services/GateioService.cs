using ArbitrageBot.Services.Interfaces;
using System;
using System.Threading.Tasks;
using ccxt;

namespace ArbitrageBot.Services
{
    public class GateioService : IExchangeService
    {
        private readonly dynamic _gateio;

        public GateioService()
        {
            _gateio = new gateio();
        }

        public string ExchangeName => "Gate.io";

        public async Task<decimal> GetPriceAsync(string symbol)
        {
            try
            {
                dynamic ticker = await _gateio.fetchTicker(symbol);
                return Convert.ToDecimal(ticker["last"]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Gate.io API error: {ex.Message}");
            }
        }
    }
}