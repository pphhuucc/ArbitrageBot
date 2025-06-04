using ArbitrageBot.Services;
using ArbitrageBot.Services.Interfaces;
using ArbitrageBot.Core;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("🚀 Arbitrage Bot – Contract Verification (Continuous Mode)");
Console.WriteLine("⏹️ Nhấn Ctrl+C để dừng\n");

List<IExchangeService> exchanges = new()
{
    new BinanceService(),
    new KucoinService()
};

var engine = new ArbitrageEngine(exchanges);
var binance = new BinanceService();

Console.WriteLine("🔍 Đang lấy danh sách symbols...");
var symbols = await binance.GetCommonSymbolsAsync("USDT");
Console.WriteLine($"📋 Tìm thấy {symbols.Count} symbols\n");

int roundCount = 1;

// Quét liên tục
while (true)
{
    try
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"🔄 === LƯỢT QUÉT #{roundCount} === {DateTime.Now:HH:mm:ss} ===");
        Console.ResetColor();

        var realOpportunities = new List<string>();
        int processed = 0;

        foreach (var symbol in symbols.Take(50)) // Quét 50 symbols mỗi lượt để tăng tốc
        {
            try
            {
                processed++;
                var opportunity = await engine.FindOpportunityAsync(symbol);
                
                if (opportunity == null)
                {
                    Console.Write($".");
                    continue;
                }

                Console.Write($" {symbol.Replace("/USDT", "")}");

                if (opportunity.ProfitPercent > 0.1m)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($" 💰{opportunity.ProfitPercent}%");
                    Console.ResetColor();

                    Console.WriteLine($"   🔗 Buy:  {opportunity.BuyContractAddress}");
                    Console.WriteLine($"   🔗 Sell: {opportunity.SellContractAddress}");
                    
                    if (opportunity.IsContractVerified)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"   ✅ VERIFIED ARBITRAGE!");
                        Console.ResetColor();
                        realOpportunities.Add($"{symbol} ({opportunity.ProfitPercent}%)");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"   ⚠️ Different contracts");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.WriteLine($" ={opportunity.ProfitPercent:F2}%");
                }

                // Show progress every 10 symbols
                if (processed % 10 == 0)
                {
                    Console.WriteLine($" [{processed}/50]");
                }

                await Task.Delay(150); // Delay ngắn hơn
            }
            catch (Exception ex)
            {
                Console.Write($"x");
            }
        }

        // Tổng kết lượt quét
        Console.WriteLine($"\n📊 Lượt #{roundCount} hoàn thành:");
        if (realOpportunities.Any())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"🎯 {realOpportunities.Count} CƠ HỘI VERIFIED:");
            foreach (var opp in realOpportunities)
            {
                Console.WriteLine($"   ✅ {opp}");
            }
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("   ❌ Không có cơ hội verified nào");
            Console.ResetColor();
        }

        roundCount++;
        
        // Delay giữa các lượt quét
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"⏳ Nghỉ 30 giây trước lượt tiếp theo...\n");
        Console.ResetColor();
        
        await Task.Delay(30000); // Nghỉ 30 giây
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Lỗi trong lượt quét #{roundCount}: {ex.Message}");
        await Task.Delay(5000); // Nghỉ 5 giây khi có lỗi
    }
}