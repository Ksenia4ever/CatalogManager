
namespace CatalogManager.Services;

// C10: bounded, in-memory event log and asynchronous notifications.
public sealed record ConnectivityEvent(
    long Id,
    string Kind,
    string Message,
    DateTimeOffset CreatedAt);

public sealed class ConnectivityEventStore
{
    private readonly object _gate = new();
    private readonly List<ConnectivityEvent> _events = new();

    private TaskCompletionSource<bool> _changed = NewSignal();

    private long _sequence;

    private static TaskCompletionSource<bool> NewSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public ConnectivityEvent Publish(string kind, string message)
    {
        lock (_gate)
        {
            var item = new ConnectivityEvent(
                ++_sequence,
                kind,
                message,
                DateTimeOffset.UtcNow);

            _events.Add(item);

            if (_events.Count > 200)
            {
                _events.RemoveAt(0);
            }

            var old = _changed;

            _changed = NewSignal();

            old.TrySetResult(true);

            return item;
        }
    }

    public IReadOnlyList<ConnectivityEvent> GetAfter(long after)
    {
        lock (_gate)
        {
            return _events
                .Where(x => x.Id > after)
                .ToArray();
        }
    }

    public long LatestId
    {
        get
        {
            lock (_gate)
            {
                return _sequence;
            }
        }
    }

    // Avoids a missed wake-up:
    // check event history and get the signal under one lock.
    public async Task<IReadOnlyList<ConnectivityEvent>> WaitAfterAsync(
        long after,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        Task signal;

        lock (_gate)
        {
            var available = _events
                .Where(x => x.Id > after)
                .ToArray();

            if (available.Length > 0)
            {
                return available;
            }

            signal = _changed.Task;
        }

        try
        {
            await signal.WaitAsync(timeout, cancellationToken);
        }
        catch (TimeoutException)
        {
            // No new events arrived during the waiting period.
            // This is normal for Long Polling.
            return Array.Empty<ConnectivityEvent>();
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // The client disconnected or canceled the HTTP request.
            // For example, the user navigated to another page.
            // This is expected behavior, not an application error.
            return Array.Empty<ConnectivityEvent>();
        }

        // An event was published while the request was waiting.
        return GetAfter(after);
    }
}
