using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace ConsoleApp.Services;

public class SmtpEmailService : IEmailService
{
    public async Task EnviarAlertaAsync(IConfiguration config, string bodyMessage, string subject)
    {
        try
        {
            // Acessa os parâmetros do config.json para envio do email
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
        // Trata possíveis erros
        catch (Exception ex)
        {
            Console.WriteLine($"Falha ao enviar email de alerta: {ex.Message}. Verifique o arquivo config.json");
        }
    }
}