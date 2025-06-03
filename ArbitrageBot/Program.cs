using ArbitrageBot.Services;
using ArbitrageBot.Services.Interfaces;
using ArbitrageBot.Core;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("🚀 Arbitrage Bot – Bước 3: Phát hiện cơ hội");

// Khởi tạo danh sách sàn (mới có Binance, sẽ thêm KuCoin sau)
List<IExchangeService> exchanges = new()
{
    new BinanceService(),
    new KucoinService()
    // new KucoinService() // sẽ thêm sau
};

var engine = new ArbitrageEngine(exchanges);
var binance = new BinanceService();
var symbols = await binance.GetCommonSymbolsAsync("USDT");
//symbols = symbols.Take(20).ToList(); // giới hạn để test

Console.WriteLine($"\n🔍 Đang phân tích {symbols.Count} cặp coin...\n");

foreach (var symbol in symbols)
{
    var giaTheoSan = new List<(string Exchange, decimal Price)>();

    foreach (var exchange in exchanges)
    {
        try
        {
            var price = await exchange.GetPriceAsync(symbol);
            giaTheoSan.Add((exchange.ExchangeName, price));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Không lấy được giá {symbol} từ {exchange.ExchangeName}: {ex.Message}");
        }
    }

    if (giaTheoSan.Count < 2)
    {
        Console.WriteLine($"⚠️ Không đủ dữ liệu cho {symbol} (ít hơn 2 sàn có giá).");
        continue;
    }

    // Hiển thị giá từng sàn
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"\n📌 {symbol}");
    Console.ResetColor();

    foreach (var g in giaTheoSan)
    {
        Console.WriteLine($"   🏷️ {g.Exchange,-10}: {g.Price:N5} USD");
    }

    // Tính arbitrage từ dữ liệu đã có
    var buy = giaTheoSan.OrderBy(p => p.Price).First();
    var sell = giaTheoSan.OrderByDescending(p => p.Price).First();

    var profit = (sell.Price - buy.Price) / buy.Price * 100 - 0.2m;

    if (profit > 0.1m)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ Cơ hội arbitrage: Mua {buy.Exchange} → Bán {sell.Exchange} = Lãi {Math.Round(profit, 2)}%");
        Console.ResetColor();
    }
    //else
    //{
    //    Console.ForegroundColor = ConsoleColor.DarkGray;
    //    Console.WriteLine($"❌ Không có arbitrage đủ lợi nhuận (chênh lệch: {Math.Round(profit, 2)}%)");
    //    Console.ResetColor();
    //}

    await Task.Delay(200); // delay tránh spam API
}
