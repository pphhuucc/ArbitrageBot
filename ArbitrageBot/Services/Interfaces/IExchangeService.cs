namespace ArbitrageBot.Services.Interfaces
{
    public interface IExchangeService
    {
        Task<decimal> GetPriceAsync(string symbol);
        string ExchangeName { get; }
    }
}