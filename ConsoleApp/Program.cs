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
        // Cria a varíavel Subject (StockMonitor)
        var stockMonitor = new StockMonitor(ativo);
        var consoleDisplay = new ConsoleDisplayObserver();
        var emailAlerter = new EmailAlertObserver(Math.Max(max, min), Math.Min(max, min), config, Cooldown);

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
    
    // Funções auxiliares EnviarEmail e ObterPreco (ambos implementados na Primeira versão)
    #region Funções Auxiliares
    public static async Task<float?> ObterPrecoDoAtivoAsync(string ativo)
    {
        try
        {
            // Realiza o HTTP Get para aquisição do valor do ativo
            string url = $"https://brapi.dev/api/quote/{ativo}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string jsonContent = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(jsonContent);
                var priceToken = jsonObject["results"]?[0]?["regularMarketPrice"];
                return priceToken != null ? (float)priceToken : null;
            }
        }
        catch
        {
            Console.WriteLine("Problemas de conexão ou acesso à API. Verifique sua conexão com a internet.");
        }
        return null;
    }

    public static async Task EnviaMensagemEmailAsync(IConfiguration config, string bodyMessage, string subject)
    {
        try
        {   // Acessa os parâmetros do config.json para envio do email
            string? smtpHost = config["SmtpSettings:Host"];
            if (string.IsNullOrEmpty(smtpHost)) throw new InvalidOperationException("Config 'SmtpSettings:Host' não encontrada.");
            
            if(!int.TryParse(config["SmtpSettings:Port"], out int smtpPort)) throw new FormatException("Config 'SmtpSettings:Port' é inválida.");

            string? remetenteEmail = config["SmtpSettings:SenderEmail"];
            if (string.IsNullOrEmpty(remetenteEmail)) throw new InvalidOperationException("Config 'SmtpSettings:SenderEmail' não encontrada.");
            
            string? remetenteSenha = config["SmtpSettings:SenderPassword"];
            if (string.IsNullOrEmpty(remetenteSenha)) throw new InvalidOperationException("Config 'SmtpSettings:SenderPassword' não encontrada.");
     
            // Modificação: Permitir múltiplos destinatários
            List<string>? recipients = config.GetSection("Recipients").Get<List<string>>();
            if (recipients == null || recipients.Count == 0)
            {
                throw new InvalidOperationException("Nenhum destinatário encontrado na seção 'Recipients' do config.json.");
            }
            var mensagem = new MailMessage
            {
                From = new MailAddress(remetenteEmail),
                Subject = subject,
                Body = bodyMessage,
                IsBodyHtml = false
            };
            foreach (var recipient in recipients)
            {
                if (!string.IsNullOrWhiteSpace(recipient))
                {
                    mensagem.To.Add(recipient);
                }
            }

            var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                UseDefaultCredentials = false,
                EnableSsl = true,
                Credentials = new NetworkCredential(remetenteEmail, remetenteSenha)
            };

            Console.WriteLine($"Enviando email de alerta para {recipients.Count} destinatário(s)...");
            await smtpClient.SendMailAsync(mensagem);
            Console.WriteLine("Email de alerta enviado com sucesso!");
        }
        // Trata possíveis erros do usuário
        catch (Exception ex)
        {
            Console.WriteLine($"Falha ao enviar email de alerta: {ex.Message}. Verifique o arquivo config.json");
        }
    }
    #endregion
}