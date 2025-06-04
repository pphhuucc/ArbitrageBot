namespace ArbitrageBot.Services
{
    public class DynamicContractVerificationService
    {
        private readonly ContractLookupService _contractLookup;
        private readonly Dictionary<string, (string contract, DateTime cached)> _cache;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(24);

        public DynamicContractVerificationService()
        {
            _contractLookup = new ContractLookupService();
            _cache = new Dictionary<string, (string contract, DateTime cached)>();
        }

        public async Task<(bool isVerified, string buyContract, string sellContract, string note)> 
            VerifyContractMatchAsync(string symbol, string buyExchange, string sellExchange)
        {
            try
            {
                var buyContract = await GetContractForExchange(symbol, buyExchange);
                var sellContract = await GetContractForExchange(symbol, sellExchange);

                if (string.IsNullOrEmpty(buyContract) || string.IsNullOrEmpty(sellContract))
                {
                    return (false, buyContract, sellContract, 
                           "❓ Không tìm thấy contract address cho một hoặc cả hai sàn");
                }

                bool isMatch = string.Equals(buyContract, sellContract, StringComparison.OrdinalIgnoreCase);
                
                if (isMatch)
                {
                    return (true, buyContract, sellContract, 
                           "✅ CONTRACT VERIFIED - Đây là cơ hội arbitrage THỰC SỰ!");
                }
                else
                {
                    return (false, buyContract, sellContract, 
                           "⚠️ CONTRACT KHÁC NHAU - Token khác nhau cùng tên, KHÔNG phải arbitrage!");
                }
            }
            catch (Exception ex)
            {
                return (false, "ERROR", "ERROR", 
                       $"❌ Lỗi khi verify contract: {ex.Message}");
            }
        }

        private async Task<string> GetContractForExchange(string symbol, string exchange)
        {
            var cacheKey = $"{symbol}_{exchange}";
            
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                if (DateTime.Now - cached.cached < _cacheExpiry)
                {
                    return cached.contract;
                }
                _cache.Remove(cacheKey);
            }

            string network = GetNetworkForExchange(exchange, symbol);
            
            var contract = await _contractLookup.GetContractAddressAsync(symbol, network);
            
            if (string.IsNullOrEmpty(contract))
            {
                contract = _contractLookup.GetKnownContract(symbol, network);
            }

            if (!string.IsNullOrEmpty(contract))
            {
                _cache[cacheKey] = (contract, DateTime.Now);
            }

            return contract ?? "";
        }

        private string GetNetworkForExchange(string exchange, string symbol)
        {
            return exchange.ToLower() switch
            {
                "binance" => "ethereum",
                "kucoin" => "ethereum",
                _ => "ethereum"
            };
        }

        public void Dispose()
        {
            _contractLookup?.Dispose();
        }
    }
}