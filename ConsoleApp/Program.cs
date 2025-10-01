using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;


class Program
{
    static async Task Main(string[] args)
    {
        string ativo = args[0].ToUpper();
        float first = float.Parse(args[1], CultureInfo.InvariantCulture);
        float second = float.Parse(args[2], CultureInfo.InvariantCulture);
        int delay;
        try
        {
            delay = int.Parse(args[3], CultureInfo.InvariantCulture);
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine("O valor do delay não foi fornecido. Usando o valor padrão de 300 segundos. (5 minutos)");
            delay = 300;
        }
        float max;
        float min; 
        // Definir variáveis para armazenar o valor máximo e mínimo do ativo a ser monitorado
        // Erro caso o input seja errado
        if (first > second)
        {
            max = first;
            min = second;
        }
        else
        {
            max = second;
            min = first;
        }
        Console.WriteLine($"Monitorando o ativo {ativo} com valor máximo de {max} e valor mínimo de {min}. Verificando a cada {delay} segundos.");
        while (true)
        {
            float? currentlyPrice = ObterPrecoDoAtivo(ativo);
            if (currentlyPrice == null)
                {
                    throw new InvalidOperationException($"Não foi possível obter o preço do {ativo}. Verifique se o código do {ativo} está correto.");
                }
            if (currentlyPrice > max)
                {
                    await EnviaMensagemEmail($"O ativo {ativo} ultrapassou o valor máximo de {max}. Preço atual: {currentlyPrice}. Recomendamos vender o ativo.", $"Como o ativo {ativo} atingiu um valor acima de {max}, sugerimos a venda.");
                    // Aguardar 5 minutos antes de verificar novamente
                    await Task.Delay(delay * 1000);
                }
                else if (currentlyPrice < min)
                {
                    await EnviaMensagemEmail($"O ativo {ativo} caiu abaixo do valor mínimo de {min}. Preço atual: {currentlyPrice}. Recomendamos comprar o ativo", $"Como o ativo {ativo} esteve abaixo de {min}, sugerimos a compra.");
                    // Aguardar 5 minutos antes de verificar novamente
                    await Task.Delay(delay * 1000);
                }
                else
                {
                    Console.WriteLine($"O ativo {ativo} está dentro da faixa definida. Preço atual: {currentlyPrice}");
                    await Task.Delay(1000);
                }
        }
    }
    public static async Task EnviaMensagemEmail(string bodyMessage = "Teste de envio de email usando configuração externa.", string subject = "Teste de Envio de Email")
    {
        try
        {
            // Carregar as configurações do arquivo config.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();
            // Lê o arquivo de configuração (config.json) e armazena os valores em variáveis
            string? smtpHost = config["SmtpSettings:Host"];
            string? portString = config["SmtpSettings:Port"];
            string? emailRemetente = config["SmtpSettings:SenderEmail"];
            string? senhaRemetente = config["SmtpSettings:SenderPassword"];
            string? emailDestinatario = config["DefaultRecipient"];

            // Trata o caso de valores nulos ou inválidos
            if (string.IsNullOrEmpty(smtpHost) ||
                string.IsNullOrEmpty(portString) ||
                string.IsNullOrEmpty(emailRemetente) ||
                string.IsNullOrEmpty(senhaRemetente) ||
                string.IsNullOrEmpty(emailDestinatario))
            {
                throw new InvalidOperationException("Alguma configuração está ausente ou incorreta, cheque o arquivo config.json.");
            }
            
            if (!int.TryParse(portString, out int smtpPort))
            {
                throw new InvalidOperationException("Alguma configuração está ausente ou incorreta, cheque o arquivo config.json.");
            }

            Console.WriteLine("Configurações carregadas. Preparando para enviar email...");

            // Cria a mensagem com base no arquivo de configuração
            var mensagem = new MailMessage();
            mensagem.From = new MailAddress(emailRemetente);
            mensagem.To.Add(new MailAddress(emailDestinatario));
            mensagem.Subject = subject;
            mensagem.Body = bodyMessage;
            mensagem.IsBodyHtml = false;

            // Configura o cliente SMTP
            var smtpClient = new SmtpClient(smtpHost, smtpPort);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential(emailRemetente, senhaRemetente);

            // Envia o email
            Console.WriteLine("Enviando...");
            await smtpClient.SendMailAsync(mensagem);

            Console.WriteLine("Email enviado com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocorreu um erro: {ex.Message}");
        }
    }


     public static float? ObterPrecoDoAtivo(string ativo)
    {
        try
        {
            using (var client = new HttpClient())
            {
                string url = $"https://brapi.dev/api/quote/{ativo}";
                
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = response.Content.ReadAsStringAsync().Result;
                    
                    var jsonObject = JObject.Parse(jsonContent);
                    var priceToken = jsonObject["results"]?[0]?["regularMarketPrice"];
                    
                    if (priceToken != null)
                    {
                        return (float)priceToken;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Em caso de erro, podemos logar a mensagem e garantir que o método retorne null.
            Console.WriteLine($"[Função ObterPrecoDoAtivo] Erro interno: {ex.Message}");
        }
        return null;
    }
}