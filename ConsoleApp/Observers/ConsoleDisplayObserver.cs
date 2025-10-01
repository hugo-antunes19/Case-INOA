using ConsoleApp.Interfaces;

namespace ConsoleApp.Observers;

public class ConsoleDisplayObserver : IObserver
{
    public Task Update(ISubject subject)
    {
        if (subject is StockMonitor monitor && monitor.CurrentPrice.HasValue)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Preço de {monitor.Ativo}: {monitor.CurrentPrice.Value:C}");
        }
        return Task.CompletedTask;
    }
}