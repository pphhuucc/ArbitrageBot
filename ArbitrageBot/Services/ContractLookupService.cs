using System.Text.Json;

namespace ArbitrageBot.Services
{
    public class ContractLookupService
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _cache;

        public ContractLookupService()
        {
            _httpClient = new HttpClient();
            _cache = new Dictionary<string, string>();
        }

        public async Task<string> GetContractAddressAsync(string symbol, string network = "ethereum")
        {
            try
            {
                var cacheKey = $"{symbol}_{network}";
                if (_cache.ContainsKey(cacheKey))
                {
                    return _cache[cacheKey];
                }

                var coinId = await GetCoinIdFromSymbol(symbol);
                if (string.IsNullOrEmpty(coinId))
                {
                    return "";
                }

                var contractAddress = await GetContractFromCoinId(coinId, network);
                
                if (!string.IsNullOrEmpty(contractAddress))
                {
                    _cache[cacheKey] = contractAddress;
                }

                return contractAddress ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Lỗi tìm contract cho {symbol}: {ex.Message}");
                return "";
            }
        }

        private async Task<string> GetCoinIdFromSymbol(string symbol)
        {
            try
            {
                var cleanSymbol = symbol.Replace("/USDT", "").Replace("/BUSD", "").Replace("/USDC", "");
                
                var url = "https://api.coingecko.com/api/v3/coins/list";
                var response = await _httpClient.GetStringAsync(url);
                
                using var document = JsonDocument.Parse(response);
                var coins = document.RootElement.EnumerateArray();

                foreach (var coin in coins)
                {
                    var coinSymbol = coin.GetProperty("symbol").GetString();
                    if (string.Equals(coinSymbol, cleanSymbol, StringComparison.OrdinalIgnoreCase))
                    {
                        return coin.GetProperty("id").GetString() ?? "";
                    }
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        private async Task<string> GetContractFromCoinId(string coinId, string network)
        {
            try
            {
                var url = $"https://api.coingecko.com/api/v3/coins/{coinId}";
                var response = await _httpClient.GetStringAsync(url);
                
                using var document = JsonDocument.Parse(response);
                var root = document.RootElement;

                if (root.TryGetProperty("platforms", out var platforms))
                {
                    var networkKey = network.ToLower() switch
                    {
                        "ethereum" => "ethereum",
                        "bsc" => "binance-smart-chain",
                        "polygon" => "polygon-pos",
                        _ => "ethereum"
                    };

                    if (platforms.TryGetProperty(networkKey, out var contractElement))
                    {
                        var contract = contractElement.GetString();
                        return !string.IsNullOrEmpty(contract) ? contract : "";
                    }
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        public string GetKnownContract(string symbol, string network = "ethereum")
        {
            var knownContracts = new Dictionary<string, Dictionary<string, string>>
            {
                ["ethereum"] = new()
                {
                    {"BTC/USDT", "0x2260FAC5E5542a773Aa44fBCfeDf7C193bc2C599"},
                    {"ETH/USDT", "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2"},
                    {"USDT/USDT", "0xdAC17F958D2ee523a2206206994597C13D831ec7"},
                    {"USDC/USDT", "0xA0b73E1Ff0B80914AB6fe0444E65848C4C34450b"},
                }
            };

            return knownContracts.TryGetValue(network, out var contracts) 
                ? contracts.GetValueOrDefault(symbol) ?? ""
                : "";
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}