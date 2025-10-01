namespace ConsoleApp.Services;

public interface IPriceService
{
    Task<float?> ObterPrecoDoAtivoAsync(string ativo);
}