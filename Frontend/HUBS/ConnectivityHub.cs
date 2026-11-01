using CatalogManager.Services;
using Microsoft.AspNetCore.SignalR;

namespace CatalogManager.Hubs;

// C11: real-time product and demo events using SignalR.
public sealed class ConnectivityHub : Hub
{
    private readonly ConnectivityEventStore _events;

    public ConnectivityHub(ConnectivityEventStore events)
    {
        _events = events;
    }

    // Used once after connection to recover any events missed during startup.
    public IReadOnlyList<ConnectivityEvent> GetHistory()
    {
        return _events.GetAfter(0);
    }
}