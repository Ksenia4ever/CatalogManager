(() => {
    'use strict';

    const root = document.getElementById('c11Panel');
    if (!root) return;

    const status = root.querySelector('.c11-status');
    const startButton = root.querySelector('.c11-start');
    const stopButton = root.querySelector('.c11-stop');
    const connectionCount = root.querySelector('.c11-connections');
    const eventCount = root.querySelector('.c11-events');
    const byteCount = root.querySelector('.c11-bytes');
    const delayLabel = root.querySelector('.c11-delay');
    const feed = root.querySelector('.c11-feed');

    const encoder = new TextEncoder();
    let connection = null;
    let running = false;
    let intentionalStop = false;
    let lastId = 0;
    let connections = 0;
    let received = 0;
    let bytes = 0;
    let generation = 0;

    function update(statusText) {
        status.textContent = statusText;
        startButton.disabled = running;
        stopButton.disabled = !running;
        root.dataset.running = String(running);
        connectionCount.textContent = connections;
        eventCount.textContent = received;
        byteCount.textContent = bytes;
    }

    function receive(item, fromHistory = false) {
        if (!running || !item || !Number.isSafeInteger(item.id) || item.id <= lastId) return;
        lastId = item.id;
        if (fromHistory) return; // History already exists in the C10 server-history panel.

        received++;
        const json = JSON.stringify(item);
        bytes += encoder.encode(json).length;
        const delay = Date.now() - Date.parse(item.createdAt);
        delayLabel.textContent = Number.isFinite(delay)
            ? `${Math.max(0, Math.round(delay))} ms` : '—';
        const li = document.createElement('li');
        li.textContent = `#${item.id} [${item.kind}] ${item.message}`;
        feed.prepend(li);
        while (feed.children.length > 12) feed.lastChild.remove();
        update('Connected');
    }

    async function stop() {
        const current = connection;
        generation++;
        intentionalStop = true;
        running = false;
        connection = null;
        update('Stopped');
        if (current) {
            try { await current.stop(); }
            catch (error) { console.debug('C11 stop:', error); }
        }
    }

    async function start() {
        if (running) return;
        if (typeof signalR === 'undefined') {
            update('SignalR client library was not loaded');
            return;
        }

        running = true;
        intentionalStop = false;
        const token = ++generation;
        update('Connecting…');

        const current = new signalR.HubConnectionBuilder()
            .withUrl('/connectivityHub')
            .withAutomaticReconnect()
            .build();
        connection = current;

        current.on('ConnectivityEventReceived', item => {
            if (running && token === generation) receive(item);
        });
        current.onreconnecting(() => {
            if (running && token === generation) update('Reconnecting…');
        });
        current.onreconnected(async () => {
            if (!running || token !== generation) return;
            connections++;
            update('Connected');
            // Sync cursor without counting old events as live measurements.
            try {
                const history = await current.invoke('GetHistory');
                if (running && token === generation)
                    for (const item of history) receive(item, true);
            } catch (error) { console.warn('C11 history:', error); }
        });
        current.onclose(() => {
            if (!intentionalStop && token === generation) {
                running = false;
                connection = null;
                update('Disconnected — press Start');
            }
        });

        try {
            await current.start();
            if (!running || token !== generation) {
                await current.stop();
                return;
            }
            connections++;
            update('Connected');
            // Events published during connection setup are recovered below.
            const history = await current.invoke('GetHistory');
            if (running && token === generation)
                for (const item of history) receive(item, true);
        } catch (error) {
            if (token === generation) {
                running = false;
                connection = null;
                update(`Connection error: ${error.message}`);
            }
        }
    }

    startButton.addEventListener('click', () => void start());
    stopButton.addEventListener('click', () => void stop());
    window.addEventListener('pagehide', () => { void stop(); });
    window.addEventListener('pageshow', event => {
        if (event.persisted) void start();
    });
    update('Stopped');
    void start();
})();