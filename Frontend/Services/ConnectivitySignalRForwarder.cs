using CatalogManager.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CatalogManager.Services;

// C11: forwards C10's in-memory events to all connected SignalR clients.
// The store remains the single source of truth; existing C10 transports stay unchanged.
public sealed class ConnectivitySignalRForwarder : BackgroundService
{
    private readonly ConnectivityEventStore _events;
    private readonly IHubContext<ConnectivityHub> _hub;
    private readonly ILogger<ConnectivitySignalRForwarder> _logger;

    public ConnectivitySignalRForwarder(
        ConnectivityEventStore events,
        IHubContext<ConnectivityHub> hub,
        ILogger<ConnectivitySignalRForwarder> logger)
    {
        _events = events;
        _hub = hub;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        long after = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            IReadOnlyList<ConnectivityEvent> items;
            try
            {
                items = await _events.WaitAfterAsync(
                    after,
                    TimeSpan.FromSeconds(20),
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            foreach (var item in items)
            {
                if (item.Id <= after) continue;
                after = item.Id;
                try
                {
                    await _hub.Clients.All.SendAsync(
                        "ConnectivityEventReceived", item, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not forward C11 event {EventId}", item.Id);
                }
            }
        }
    }
}