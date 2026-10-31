using System.Globalization;
using System.Text.Json;

namespace CatalogManager.Services;

public record CurrencyRate(string Currency, string BaseCurrency, decimal Buy, decimal Sale);

public interface ICurrencyRatesService
{
    Task<IReadOnlyList<CurrencyRate>> GetRatesAsync(CancellationToken cancellationToken = default);
}

public sealed class PrivatBankCurrencyRatesService : ICurrencyRatesService
{
    private readonly HttpClient _httpClient;

    public PrivatBankCurrencyRatesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<CurrencyRate>> GetRatesAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            "p24api/pubinfo?json&exchange&coursid=5", cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new JsonException("PrivatBank response must be a JSON array.");

        var rates = new List<CurrencyRate>();
        foreach (var item in document.RootElement.EnumerateArray())
        {
            string? currency = ReadString(item, "ccy");
            string? baseCurrency = ReadString(item, "base_ccy");
            string? buyText = ReadString(item, "buy");
            string? saleText = ReadString(item, "sale");

            if (string.IsNullOrWhiteSpace(currency) || string.IsNullOrWhiteSpace(baseCurrency) ||
                !decimal.TryParse(buyText, NumberStyles.Number, CultureInfo.InvariantCulture, out var buy) ||
                !decimal.TryParse(saleText, NumberStyles.Number, CultureInfo.InvariantCulture, out var sale) ||
                buy <= 0 || sale <= 0)
                continue;

            rates.Add(new CurrencyRate(currency.ToUpperInvariant(), baseCurrency.ToUpperInvariant(), buy, sale));
        }

        if (rates.Count == 0)
            throw new JsonException("PrivatBank returned no valid exchange rates.");

        return rates;
    }

    private static string? ReadString(JsonElement item, string name)
    {
        if (!item.TryGetProperty(name, out var value)) return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }
}