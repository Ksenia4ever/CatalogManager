using System.Net;
using System.Net.Http;
using CatalogManager.Services;
using Xunit;

namespace CatalogManager.IntegrationTests.Tests;

public sealed class PrivatBankCurrencyRatesServiceTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;
        public StubHandler(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.Contains("p24api/pubinfo", request.RequestUri!.ToString());
            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body)
            });
        }
    }

    private static PrivatBankCurrencyRatesService Create(HttpStatusCode status, string json)
    {
        var client = new HttpClient(new StubHandler(status, json))
        {
            BaseAddress = new Uri("https://api.privatbank.ua/")
        };
        return new PrivatBankCurrencyRatesService(client);
    }

    [Fact]
    public async Task ValidResponse_ParsesCurrencyRates()
    {
        var service = Create(HttpStatusCode.OK,
            """[{"ccy":"USD","base_ccy":"UAH","buy":"41.50","sale":"42.30"}]""");
        var rates = await service.GetRatesAsync();
        Assert.Single(rates);
        Assert.Equal("USD", rates[0].Currency);
        Assert.Equal("UAH", rates[0].BaseCurrency);
        Assert.Equal(41.50m, rates[0].Buy);
        Assert.Equal(42.30m, rates[0].Sale);
    }

    [Fact]
    public async Task ServerError_ThrowsHttpRequestException()
    {
        var service = Create(HttpStatusCode.ServiceUnavailable, "Service unavailable");
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetRatesAsync());
    }

    [Fact]
    public async Task MalformedJson_ThrowsJsonException()
    {
        var service = Create(HttpStatusCode.OK, "not-json");
        await Assert.ThrowsAnyAsync<System.Text.Json.JsonException>(() => service.GetRatesAsync());
    }

    [Fact]
    public async Task NoValidRates_ThrowsJsonException()
    {
        var service = Create(HttpStatusCode.OK, "[]");
        await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => service.GetRatesAsync());
    }
}