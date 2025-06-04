using ArbitrageBot.Services;
using ArbitrageBot.Services.Interfaces;
using ArbitrageBot.Core;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("🚀 Multi-CEX Arbitrage Bot");
Console.WriteLine("⏹️ Nhấn Ctrl+C để dừng\n");

// Chỉ 5 sàn CEX
List<IExchangeService> exchanges = new()
{
    new BinanceService(),
    new KucoinService(),
    new GateioService(),
    new HuobiService(),
    new CoinbaseService()
};

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"📊 Kết nối với {exchanges.Count} sàn CEX:");
Console.ResetColor();

foreach (var exchange in exchanges)
{
    Console.WriteLine($"   ✅ {exchange.ExchangeName}");
}

var engine = new ArbitrageEngine(exchanges);
var binance = new BinanceService();

Console.WriteLine("\n🔍 Đang lấy danh sách symbols...");
var symbols = await binance.GetCommonSymbolsAsync("USDT");
Console.WriteLine($"📋 Tìm thấy {symbols.Count} symbols\n");

int roundCount = 1;

// Quét liên tục
while (true)
{
    try
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"🔄 === LƯỢT QUÉT #{roundCount} ({exchanges.Count} sàn CEX) === {DateTime.Now:HH:mm:ss} ===");
        Console.ResetColor();

        var allOpportunities = new List<(string symbol, decimal profit, string buyExchange, string sellExchange, bool verified)>();
        var realOpportunities = new List<string>();
        int processed = 0;

        foreach (var symbol in symbols.Take(40)) // Tăng lên 40 vì chỉ CEX nhanh hơn
        {
            try
            {
                processed++;
                
                // Lấy giá từ tất cả sàn CEX
                var prices = new List<(string exchange, decimal price)>();
                var exchangeCount = 0;

                foreach (var exchange in exchanges)
                {
                    try
                    {
                        var price = await exchange.GetPriceAsync(symbol);
                        prices.Add((exchange.ExchangeName, price));
                        exchangeCount++;
                        Console.Write($"○"); // ○ = CEX
                    }
                    catch 
                    { 
                        Console.Write($"x");
                        continue; 
                    }
                }

                if (prices.Count < 2)
                {
                    Console.WriteLine($" ⚠️ {symbol} (chỉ có {prices.Count} sàn)");
                    continue;
                }

                // Tìm cơ hội arbitrage CEX ↔ CEX
                var buy = prices.OrderBy(p => p.price).First();
                var sell = prices.OrderByDescending(p => p.price).First();
                var profit = (sell.price - buy.price) / buy.price * 100 - 0.2m; // Fee 0.1% mỗi sàn

                Console.Write($" {symbol.Replace("/USDT", "")} ({exchangeCount}/{exchanges.Count})");

                if (profit > 0.1m) // Threshold thấp hơn cho CEX
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($" 💰{profit:F2}% ({buy.exchange}→{sell.exchange})");
                    Console.ResetColor();

                    // Verify contract nếu là cơ hội tốt
                    bool isVerified = false;
                    if (profit > 0.2m) // Chỉ verify những cơ hội tốt
                    {
                        try
                        {
                            var opportunity = await engine.FindOpportunityAsync(symbol);
                            if (opportunity != null && opportunity.IsContractVerified)
                            {
                                isVerified = true;
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"     ✅ CONTRACT VERIFIED!");
                                Console.ResetColor();
                                realOpportunities.Add($"{symbol} ({profit:F2}% - {buy.exchange}→{sell.exchange})");
                            }
                        }
                        catch { }
                    }

                    allOpportunities.Add((symbol, profit, buy.exchange, sell.exchange, isVerified));
                }
                else
                {
                    Console.WriteLine($" ={profit:F2}%");
                }

                // Progress indicator
                if (processed % 10 == 0)
                {
                    Console.WriteLine($"   📊 [{processed}/40] - {allOpportunities.Count} cơ hội tìm thấy");
                }

                await Task.Delay(80); // Delay ngắn vì chỉ CEX
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ {symbol}: {ex.Message}");
            }
        }

        // Tổng kết lượt quét
        Console.WriteLine($"\n📊 === KẾT QUẢ LƯỢT #{roundCount} ===");
        
        if (allOpportunities.Any())
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"💰 {allOpportunities.Count} CƠ HỘI ARBITRAGE CEX (> 0.1%):");
            Console.ResetColor();
            
            foreach (var opp in allOpportunities.OrderByDescending(o => o.profit).Take(10))
            {
                var color = opp.verified ? ConsoleColor.Green : ConsoleColor.Yellow;
                Console.ForegroundColor = color;
                var status = opp.verified ? "✅ VERIFIED" : "⚠️ Chưa verify";
                Console.WriteLine($"   {opp.profit:F2}% {opp.symbol} ({opp.buyExchange}→{opp.sellExchange}) {status}");
                Console.ResetColor();
            }
            
            if (realOpportunities.Any())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n🎯 {realOpportunities.Count} CƠ HỘI ĐÃ VERIFIED:");
                foreach (var realOpp in realOpportunities)
                {
                    Console.WriteLine($"   🔥 {realOpp}");
                }
                Console.ResetColor();
            }

            // Top profitable pairs
            var topPairs = allOpportunities
                .GroupBy(o => new { o.buyExchange, o.sellExchange })
                .Select(g => new { 
                    Pair = $"{g.Key.buyExchange}→{g.Key.sellExchange}", 
                    Count = g.Count(), 
                    AvgProfit = g.Average(x => x.profit) 
                })
                .OrderByDescending(x => x.AvgProfit)
                .Take(3);

            Console.WriteLine($"\n📈 Top Exchange Pairs:");
            foreach (var pair in topPairs)
            {
                Console.WriteLine($"   🔥 {pair.Pair}: {pair.Count} cơ hội, avg {pair.AvgProfit:F2}%");
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("   ❌ Không có cơ hội arbitrage nào > 0.1%");
            Console.ResetColor();
        }

        roundCount++;
        
        // Delay giữa các lượt quét
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"\n⏳ Nghỉ 30 giây trước lượt tiếp theo... (5 sàn CEX)\n");
        Console.ResetColor();
        
        await Task.Delay(30000); // Nghỉ 30 giây
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Lỗi trong lượt quét #{roundCount}: {ex.Message}");
        await Task.Delay(8000); // Nghỉ 8 giây khi có lỗi
    }
}