using CatalogManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogManager.Controllers;

[Route("CurrencyRates")]
public class CurrencyRatesController : Controller
{
    private readonly ICurrencyRatesService _service;
    private readonly ILogger<CurrencyRatesController> _logger;

    public CurrencyRatesController(ICurrencyRatesService service, ILogger<CurrencyRatesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index() => View();

    [HttpGet("Data")]
    public async Task<IActionResult> Data(CancellationToken cancellationToken)
    {
        try
        {
            var rates = await _service.GetRatesAsync(cancellationToken);
            return Json(new { source = "PrivatBank", rates });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The browser disconnected or cancelled the request.
            return new EmptyResult();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            _logger.LogWarning(ex, "Could not load currency rates from PrivatBank");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = "Currency rates are temporarily unavailable. Please try again later."
            });
        }
    }
}