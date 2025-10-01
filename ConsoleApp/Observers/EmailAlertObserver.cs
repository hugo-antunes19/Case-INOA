using ConsoleApp.Interfaces;
using Microsoft.Extensions.Configuration; // IConfiguration para detalhes do email
using ConsoleApp.Services;

namespace ConsoleApp.Observers;

public class EmailAlertObserver : IObserver
{
    private readonly float _maxPrice;
    private readonly float _minPrice;
    private readonly IConfiguration _config;
    private readonly TimeSpan _alertCooldown;
    private DateTime _lastAlertTimestamp;
    private readonly IEmailService _emailService;

    public EmailAlertObserver(float maxPrice, float minPrice, IConfiguration config, int Cooldown, IEmailService emailService)
    {
        _maxPrice = maxPrice;
        _minPrice = minPrice;
        _config = config;
        _alertCooldown = TimeSpan.FromMinutes(Cooldown);
        _lastAlertTimestamp = DateTime.MinValue;
        _emailService = emailService;
    }

    public Task Update(ISubject subject) // Modificado para não ser async -> estava travando o console, apenas o email é async
    {
        // Se enviou um email muito recentemente, não enviar novamente (Cooldown)
        if (DateTime.Now - _lastAlertTimestamp < _alertCooldown)
        {
            return Task.CompletedTask;
        }

        if (subject is StockMonitor monitor && monitor.CurrentPrice.HasValue)
        {
            float currentPrice = monitor.CurrentPrice.Value;
            string ativo = monitor.Ativo;
            string? alertSubject = null;
            string? alertBody = null;

            if (currentPrice >= _maxPrice)
            {
                alertSubject = $"ALERTA: Vendo o {ativo}!!!";
                alertBody = $"{ativo} ultrapassou o valor máximo de {_maxPrice:C}. Preço atual: {currentPrice:C}. Recomendamos a venda deste ativo.";
            }
            else if (currentPrice <= _minPrice)
            {
                alertSubject = $"ALERTA: Compre o {ativo}!!!";
                alertBody = $"{ativo} caiu abaixo do valor mínimo de {_minPrice:C}. Preço atual: {currentPrice:C}. Recomendamos a compra deste ativo.";
            }

            if (!string.IsNullOrEmpty(alertSubject) && !string.IsNullOrEmpty(alertBody))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n!!! LIMITE ATINGIDO - Disparando alerta por email !!!\n");
                Console.ResetColor();
                _ = _emailService.EnviarAlertaAsync(_config, alertBody, alertSubject);
                _lastAlertTimestamp = DateTime.Now;
            }
        }
        return Task.CompletedTask;
    }
}