using ConsoleApp.Interfaces;

namespace ConsoleApp;

// Classe responsável por notificar os Observers (Subject -> o ponto de interesse da arquitetura)
public class StockMonitor : ISubject
{  
    private readonly List<IObserver> _observers = new(); // Lista dos Observers ("Partes Interessadas")
    public string Ativo { get; }
    public float? CurrentPrice { get; private set; }

    public StockMonitor(string ativo)
    {
        Ativo = ativo;
    }

    public void Attach(IObserver observer) => _observers.Add(observer);
    public void Detach(IObserver observer) => _observers.Remove(observer);

    // Notifica os Observers sobre eventuais mudanças do Ativo
    public async Task Notify()
    {
        foreach (var observer in new List<IObserver>(_observers))
        {
            await observer.Update(this);
        }
    }

    // Pega o preço do ativo e notifica os Observers
    public async Task CheckPriceAsync()
    {
        CurrentPrice = await Program.ObterPrecoDoAtivoAsync(Ativo);
        if (CurrentPrice.HasValue)
        {
            await Notify();
        }
    }
}