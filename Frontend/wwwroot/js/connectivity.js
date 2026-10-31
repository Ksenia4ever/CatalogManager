
(() => {
    'use strict';

    const root = document.getElementById('c10Root');
    if (!root) return;

    const enc = new TextEncoder();

    const panels = [
        ...root.querySelectorAll('.c10-panel')
    ];

    const runners = new Map();

    const MODES = ['ws', 'long', 'adaptive'];
    const MAX_HISTORY = 200;
    const MAX_LIVE_EVENTS = 12;

    let pageClosing = false;
    let initializing = false;
    let initialized = false;
    let baseline = 0;

    const historyEvents = new Map();

    const clampDelay = value =>
        Math.max(0, Math.round(value));

    function panel(mode) {
        return panels.find(
            p => p.dataset.mode === mode
        );
    }

    function refresh(p, s) {
        if (!p) return;

        p.querySelector('.c10-status').textContent =
            s.status;

        p.querySelector('.c10-requests').textContent =
            s.requests;

        p.querySelector('.c10-events').textContent =
            s.events;

        p.querySelector('.c10-bytes').textContent =
            s.bytes;

        p.querySelector('.c10-delay').textContent =
            s.delay === null
                ? '—'
                : `${s.delay} ms`;

        p.querySelector('.c10-interval').textContent =
            s.interval === null
                ? '—'
                : `${s.interval} ms`;

        p.querySelector('.c10-start').disabled =
            s.running;

        p.querySelector('.c10-stop').disabled =
            !s.running;

        p.dataset.running = String(s.running);
    }

    function newState(last) {
        return {
            running: true,
            status: 'Starting…',
            requests: 0,
            events: 0,
            bytes: 0,
            delay: null,
            interval: null,
            last: last,
            controller: new AbortController(),
            socket: null,
            timer: null
        };
    }

    // --------------------------------------------------
    // Shared server event history
    // --------------------------------------------------

    const historyCard =
        document.createElement('section');

    historyCard.className =
        'card mt-4 mb-4';

    const historyBody =
        document.createElement('div');

    historyBody.className = 'card-body';

    const historyTitle =
        document.createElement('h3');

    historyTitle.className = 'h5';

    historyTitle.textContent =
        'Server event history';

    const historyInfo =
        document.createElement('p');

    historyInfo.className =
        'text-muted small mb-2';

    historyInfo.textContent =
        'Loading events from server...';

    const historyList =
        document.createElement('ol');

    historyList.className =
        'list-group list-group-numbered';

    historyBody.appendChild(historyTitle);
    historyBody.appendChild(historyInfo);
    historyBody.appendChild(historyList);

    historyCard.appendChild(historyBody);

    // The shared history is displayed separately
    // from the three transport measurements.
    root.appendChild(historyCard);

    function validEvent(event) {
        return event &&
            Number.isSafeInteger(event.id) &&
            event.id > 0 &&
            typeof event.kind === 'string' &&
            typeof event.message === 'string';
    }

    function rememberEvents(events) {
        let changed = false;

        for (const event of events) {
            if (!validEvent(event)) {
                continue;
            }

            if (!historyEvents.has(event.id)) {
                historyEvents.set(event.id, event);
                changed = true;
            }
        }

        if (historyEvents.size > MAX_HISTORY) {
            const ids = [...historyEvents.keys()]
                .sort((a, b) => a - b);

            while (historyEvents.size > MAX_HISTORY) {
                historyEvents.delete(ids.shift());
            }

            changed = true;
        }

        if (changed) {
            renderHistory();
        }
    }

    function renderHistory() {
        const events = [...historyEvents.values()]
            .sort((a, b) => b.id - a.id);

        historyList.replaceChildren();

        historyInfo.textContent =
            `${events.length} event(s) loaded from the ` +
            'server. History is shared by all three ' +
            'transports and is not counted in their ' +
            'live performance measurements.';

        if (events.length === 0) {
            const empty =
                document.createElement('p');

            empty.className =
                'text-muted mb-0';

            empty.textContent =
                'No events have been published yet.';

            historyList.appendChild(empty);
            return;
        }

        for (const event of events) {
            const item =
                document.createElement('li');

            item.className =
                'list-group-item';

            const eventTime =
                Date.parse(event.createdAt);

            const timeText =
                Number.isFinite(eventTime)
                    ? new Date(eventTime)
                        .toLocaleTimeString()
                    : 'Unknown time';

            item.textContent =
                `#${event.id} [${event.kind}] ` +
                `${event.message} — ${timeText}`;

            historyList.appendChild(item);
        }
    }

    async function loadHistory() {
        const response = await fetch(
            '/Connectivity/Poll?after=0',
            {
                cache: 'no-store'
            }
        );

        if (!response.ok) {
            throw new Error(
                `Cannot load event history: ` +
                `HTTP ${response.status}`
            );
        }

        const payload = await response.json();

        if (
            !Number.isSafeInteger(payload.latestId) ||
            payload.latestId < 0 ||
            !Array.isArray(payload.events)
        ) {
            throw new Error(
                'Invalid server history response'
            );
        }

        historyEvents.clear();

        // Load all events currently available
        // in the server's bounded in-memory store.
        rememberEvents(payload.events);

        baseline = payload.latestId;

        renderHistory();
    }

    // --------------------------------------------------
    // Live events
    // --------------------------------------------------

    function accept(mode, p, s, events) {
        if (!s.running) return;

        const ordered = [...events].sort(
            (a, b) => a.id - b.id
        );

        for (const event of ordered) {
            if (
                !validEvent(event) ||
                event.id <= s.last
            ) {
                continue;
            }

            s.last = event.id;
            s.events++;

            const timestamp =
                Date.parse(event.createdAt);

            s.delay =
                Number.isFinite(timestamp)
                    ? clampDelay(
                        Date.now() - timestamp
                    )
                    : null;

            const li =
                document.createElement('li');

            li.textContent =
                `#${event.id} [${event.kind}] ` +
                event.message;

            const feed =
                p.querySelector('.c10-feed');

            feed.prepend(li);

            while (
                feed.children.length >
                MAX_LIVE_EVENTS
            ) {
                feed.lastChild.remove();
            }
        }

        // Update the shared event history.
        // Duplicate IDs are ignored.
        rememberEvents(ordered);

        refresh(p, s);
    }

    async function request(s, path) {
        s.requests++;

        const response = await fetch(path, {
            signal: s.controller.signal,
            cache: 'no-store'
        });

        const text = await response.text();

        s.bytes += enc.encode(text).length;

        if (!response.ok) {
            throw new Error(
                `HTTP ${response.status}`
            );
        }

        return JSON.parse(text);
    }

    function pause(milliseconds, signal) {
        return new Promise(resolve => {
            if (signal.aborted) {
                resolve();
                return;
            }

            let timer;

            const onAbort = () => {
                clearTimeout(timer);

                signal.removeEventListener(
                    'abort',
                    onAbort
                );

                resolve();
            };

            timer = setTimeout(() => {
                signal.removeEventListener(
                    'abort',
                    onAbort
                );

                resolve();
            }, milliseconds);

            signal.addEventListener(
                'abort',
                onAbort,
                { once: true }
            );
        });
    }

    function stop(mode) {
        const s = runners.get(mode);

        if (!s) return;

        s.running = false;
        s.status = 'Stopped';

        clearTimeout(s.timer);
        s.controller.abort();

        if (s.socket) {
            try {
                s.socket.close();
            } catch (error) {
                console.debug(
                    'WebSocket close:',
                    error
                );
            }
        }

        runners.delete(mode);

        refresh(panel(mode), s);
    }

    async function polling(mode, p, s) {
        let interval =
            mode === 'adaptive'
                ? 500
                : null;

        while (s.running && !pageClosing) {
            try {
                s.status = 'Connected';
                s.interval = interval;

                refresh(p, s);

                const endpoint =
                    mode === 'long'
                        ? 'LongPoll'
                        : 'Poll';

                const payload = await request(
                    s,
                    `/Connectivity/${endpoint}` +
                    `?after=${s.last}`
                );

                if (
                    !s.running ||
                    pageClosing
                ) {
                    break;
                }

                const events =
                    payload.events || [];

                accept(mode, p, s, events);

                if (mode === 'adaptive') {
                    interval =
                        events.length > 0
                            ? 500
                            : Math.min(
                                interval * 1.6,
                                5000
                            );

                    s.interval =
                        Math.round(interval);

                    refresh(p, s);

                    await pause(
                        interval,
                        s.controller.signal
                    );
                }
            } catch (error) {
                if (
                    !s.running ||
                    pageClosing ||
                    error.name === 'AbortError'
                ) {
                    break;
                }

                s.status =
                    `Connection error: ` +
                    error.message;

                refresh(p, s);

                await pause(
                    1500,
                    s.controller.signal
                );
            }
        }
    }

    function websockets(p, s) {
        if (
            !s.running ||
            pageClosing
        ) {
            return;
        }

        const scheme =
            location.protocol === 'https:'
                ? 'wss:'
                : 'ws:';

        const ws = new WebSocket(
            `${scheme}//${location.host}` +
            '/Connectivity/Socket'
        );

        s.socket = ws;
        s.requests++;

        ws.onopen = () => {
            if (
                !s.running ||
                pageClosing
            ) {
                ws.close();
                return;
            }

            ws.send(`subscribe:${s.last}`);

            s.status = 'Connected';

            refresh(p, s);
        };

        ws.onmessage = event => {
            if (
                !s.running ||
                pageClosing
            ) {
                return;
            }

            s.bytes +=
                enc.encode(event.data).length;

            try {
                accept(
                    'ws',
                    p,
                    s,
                    [JSON.parse(event.data)]
                );
            } catch {
                s.status =
                    'Invalid message';

                refresh(p, s);
            }
        };

        ws.onerror = () => {
            if (
                s.running &&
                !pageClosing
            ) {
                s.status =
                    'WebSocket error';

                refresh(p, s);
            }
        };

        ws.onclose = () => {
            if (
                !s.running ||
                pageClosing
            ) {
                return;
            }

            s.running = false;

            s.status =
                'Socket closed — start again';

            runners.delete('ws');

            refresh(p, s);
        };
    }

    function start(mode) {
        if (
            pageClosing ||
            !initialized ||
            runners.has(mode)
        ) {
            return;
        }

        const p = panel(mode);

        if (!p) return;

        const s = newState(baseline);

        runners.set(mode, s);

        refresh(p, s);

        try {
            if (mode === 'ws') {
                websockets(p, s);
            } else {
                void polling(mode, p, s);
            }
        } catch (error) {
            if (
                !s.running ||
                pageClosing
            ) {
                return;
            }

            s.running = false;

            s.status =
                `Connection error: ` +
                error.message;

            runners.delete(mode);

            refresh(p, s);
        }
    }

    // --------------------------------------------------
    // Buttons
    // --------------------------------------------------

    panels.forEach(p => {
        const mode = p.dataset.mode;

        p.querySelector('.c10-start')
            .addEventListener(
                'click',
                () => start(mode)
            );

        p.querySelector('.c10-stop')
            .addEventListener(
                'click',
                () => stop(mode)
            );

        refresh(p, {
            status: 'Stopped',
            running: false,
            requests: 0,
            events: 0,
            bytes: 0,
            delay: null,
            interval: null
        });
    });

    // --------------------------------------------------
    // Demo event
    // --------------------------------------------------

    const demoForm =
        document.getElementById(
            'c10DemoForm'
        );

    if (demoForm) {
        demoForm.addEventListener(
            'submit',
            async event => {
                event.preventDefault();

                const label =
                    document.getElementById(
                        'c10PublishStatus'
                    );

                try {
                    const body =
                        new FormData(
                            event.currentTarget
                        );

                    const response = await fetch(
                        '/Connectivity/DemoEvent',
                        {
                            method: 'POST',
                            body
                        }
                    );

                    if (!response.ok) {
                        throw new Error(
                            `HTTP ${response.status}`
                        );
                    }

                    if (label) {
                        label.textContent =
                            'Demo event published.';
                    }
                } catch (error) {
                    if (label) {
                        label.textContent =
                            `Failed: ${error.message}`;
                    }
                }
            }
        );
    }

    // --------------------------------------------------
    // Page lifecycle
    // --------------------------------------------------

    function stopAll() {
        for (const mode of MODES) {
            stop(mode);
        }
    }

    function startAll() {
        for (const mode of MODES) {
            start(mode);
        }
    }

    async function initialize() {
        if (initializing) return;

        initializing = true;

        try {
            for (const p of panels) {
                p.querySelector(
                    '.c10-status'
                ).textContent =
                    'Loading history…';
            }

            await loadHistory();

            if (pageClosing) return;

            initialized = true;

            startAll();
        } catch (error) {
            console.error(
                'C10 initialization failed:',
                error
            );

            historyInfo.textContent =
                `Could not load history: ` +
                error.message;

            for (const p of panels) {
                p.querySelector(
                    '.c10-status'
                ).textContent =
                    'Initialization failed';
            }
        } finally {
            initializing = false;
        }
    }

    window.addEventListener(
        'pagehide',
        () => {
            pageClosing = true;
            stopAll();
        }
    );

    window.addEventListener(
        'pageshow',
        event => {
            if (!event.persisted) {
                return;
            }

            pageClosing = false;

            initialized = false;

            stopAll();

            void initialize();
        }
    );

    void initialize();

})();
