using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace ConsoleApp.Services;

public class BrapiPriceService : IPriceService
{
    private static readonly HttpClient _httpClient = new();

    public async Task<float?> ObterPrecoDoAtivoAsync(string ativo)
    {
        try
        {
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
}