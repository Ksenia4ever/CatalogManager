
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using CatalogManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogManager.Controllers;

// C10 endpoints intentionally bypass B9 caching.
[Route("Connectivity")]
public sealed class ConnectivityController : Controller
{
    private readonly ConnectivityEventStore _events;

    // Changes whenever the application process restarts.
    // Allows browsers to detect that in-memory event IDs were reset.
    private static readonly string ServerInstanceId =
        Guid.NewGuid().ToString("N");

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public ConnectivityController(ConnectivityEventStore events)
    {
        _events = events;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("Latest")]
    public IActionResult Latest()
    {
        Response.Headers.CacheControl = "no-store";

        return Ok(new
        {
            latestId = _events.LatestId,
            instanceId = ServerInstanceId
        });
    }

    // Long Polling:
    // Hold one HTTP request until an event arrives
    // or the 20-second timeout expires.
    [HttpGet("LongPoll")]
    public async Task<IActionResult> LongPoll(
        [FromQuery] long after = 0)
    {
        if (after < 0)
        {
            return BadRequest(new
            {
                error = "after must be non-negative"
            });
        }

        Response.Headers.CacheControl = "no-store";

        var items = await _events.WaitAfterAsync(
            after,
            TimeSpan.FromSeconds(20),
            HttpContext.RequestAborted);

        // The client may have navigated away.
        // In that case there is no useful response to send.
        if (HttpContext.RequestAborted.IsCancellationRequested)
        {
            return new EmptyResult();
        }

        return Ok(new
        {
            events = items,
            latestId = _events.LatestId
        });
    }

    // Adaptive Polling:
    // Respond immediately.
    // JavaScript adjusts the delay between requests.
    [HttpGet("Poll")]
    public IActionResult Poll([FromQuery] long after = 0)
    {
        if (after < 0)
        {
            return BadRequest(new
            {
                error = "after must be non-negative"
            });
        }

        Response.Headers.CacheControl = "no-store";

        return Ok(new
        {
            events = _events.GetAfter(after),
            latestId = _events.LatestId
        });
    }

    // Manual demonstration event.
    [HttpPost("DemoEvent")]
    [ValidateAntiForgeryToken]
    public IActionResult DemoEvent()
    {
        var item = _events.Publish(
            "demo",
            "Manual catalog event for connectivity comparison");

        return Ok(item);
    }

    // Native WebSocket endpoint.
    [HttpGet("Socket")]
    public async Task Socket()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            Response.StatusCode =
                StatusCodes.Status400BadRequest;

            return;
        }

        using var socket =
            await HttpContext.WebSockets.AcceptWebSocketAsync();

        var cancellation = HttpContext.RequestAborted;
        var buffer = new byte[512];

        WebSocketReceiveResult? first;

        try
        {
            // Example: subscribe:7
            first = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                cancellation);

            if (first.MessageType != WebSocketMessageType.Text ||
                !first.EndOfMessage)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.InvalidMessageType,
                    "Expected subscribe message",
                    CancellationToken.None);

                return;
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (WebSocketException)
        {
            return;
        }

        var subscribeText = Encoding.UTF8.GetString(
            buffer,
            0,
            first.Count);

        if (!subscribeText.StartsWith(
                "subscribe:",
                StringComparison.Ordinal) ||
            !long.TryParse(
                subscribeText[10..],
                out var after) ||
            after < 0)
        {
            await socket.CloseAsync(
                WebSocketCloseStatus.PolicyViolation,
                "Invalid subscription",
                CancellationToken.None);

            return;
        }

        try
        {
            while (
                socket.State == WebSocketState.Open &&
                !cancellation.IsCancellationRequested)
            {
                var items = await _events.WaitAfterAsync(
                    after,
                    TimeSpan.FromSeconds(20),
                    cancellation);

                foreach (var item in items)
                {
                    if (socket.State != WebSocketState.Open)
                    {
                        return;
                    }

                    var bytes =
                        JsonSerializer.SerializeToUtf8Bytes(
                            item,
                            JsonOptions);

                    await socket.SendAsync(
                        new ArraySegment<byte>(bytes),
                        WebSocketMessageType.Text,
                        true,
                        cancellation);

                    after = item.Id;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the browser leaves the page.
        }
        catch (WebSocketException)
        {
            // Expected when the socket disconnects.
        }
    }
}
