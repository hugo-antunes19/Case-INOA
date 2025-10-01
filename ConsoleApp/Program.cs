using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using ConsoleApp.Interfaces; // Interfaces que criamos
using ConsoleApp.Observers; // Os observadores do evento
using ConsoleApp.Services; // O serviço para monitoramento e envio de email

namespace ConsoleApp; // Namespace principal

class Program
{
    // Criar apeas um cliente HTTP -> evita gasto de memória e problemas com sockets
    private static readonly HttpClient _httpClient = new(); 

    static async Task Main(string[] args)
    {
        if (args.Length < 3 || !float.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float max) || !float.TryParse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float min))
        {
            Console.WriteLine("Uso correto: dotnet run -- <ATIVO> <MAX> <MIN>");
            return;
        }
        int Cooldown = 10; // Valor padrão do Cooldown do email
        if (args.Length > 3)
        {
            if (!int.TryParse(args[3], out Cooldown))
            {
                Console.WriteLine($"AVISO: Cooldown '{args[3]}' inválido. Usando o padrão de {Cooldown} minutos.");
            }
            else
            {
                Console.WriteLine($"Cooldown setado para {Cooldown} minutos.");
            }
        }
        else
        {
            Console.WriteLine($"AVISO: Cooldown não especificado, usando cooldown padrão de 10 minutos.");
        }
        string ativo = args[0].ToUpper();

        // Optional = false -> O arquivo config.json é obrigatório para a aplicação
        // Cria a variável para ler o arquivo de configuração
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", optional: false) 
            .Build();
        // Cria instâncias dos serviços
        IPriceService priceService = new BrapiPriceService();
        IEmailService emailService = new SmtpEmailService();

        // Cria objetos
        var stockMonitor = new StockMonitor(ativo, priceService);
        var consoleDisplay = new ConsoleDisplayObserver();
        var emailAlerter = new EmailAlertObserver(max, min, config, Cooldown, emailService);

        // Atrela ao Subject (StockMonitor) os Observers (consoleDisplay e emailAlerter)
        stockMonitor.Attach(consoleDisplay);
        stockMonitor.Attach(emailAlerter);

        Console.WriteLine("\n------ Iniciando Monitoramento com Padrão Observer ------");
        Console.WriteLine("Pressione Ctrl + C para abortar o processamento.");
        await Task.Delay(2000);

        // O funcionamento principal do código: Checa o valor com StockMonitor e o ISubject Notifica os Observers
        while (true)
        {
            await stockMonitor.CheckPriceAsync();
            await Task.Delay(1000); // Verifica 1 vez por segundo
        }
    }
}