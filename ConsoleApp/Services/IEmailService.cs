using Microsoft.Extensions.Configuration;

namespace ConsoleApp.Services;

public interface IEmailService
{
    Task EnviarAlertaAsync(IConfiguration config, string bodyMessage, string subject);
}