
(() => {
    'use strict';

    const LOCAL_KEY = 'catalogmanager.c12.notes.v1';
    const TYPES = new Set(['paragraph', 'heading1', 'heading2', 'bullets', 'numbers']);

    const container = document.getElementById('notesBlocks');
    if (!container) return;

    const template = document.getElementById('notesBlockTemplate');
    const saveStatus = document.getElementById('notesSaveStatus');
    const connectionStatus = document.getElementById('notesConnectionStatus');
    const empty = document.getElementById('notesEmpty');
    const preview = document.getElementById('notesPreview');
    const previewButton = document.getElementById('notesTogglePreview');
    const documentInput = document.getElementById('notesDocumentId');
    const openButton = document.getElementById('notesOpenDocument');
    const reloadButton = document.getElementById('notesReloadServer');
    const importButton = document.getElementById('notesImportLocal');
    const addButton = document.getElementById('notesAdd');
    const clearButton = document.getElementById('notesClear');

    const activity = document.getElementById('notesRemoteActivity');

    let blocks = [];
    let documentId = 'shared';
    let version = 0;
    let dirty = false;
    let timer = null;
    let previewVisible = false;
    let connected = false;
    let saving = false;
    let generation = 0;
    let connection;

    const remoteEvents = [];
    const MAX_EVENTS = 8;

    // C13/C14: validate document identifiers before using SignalR.
    function validId(value) {
        return typeof value === 'string' &&
            /^[A-Za-z0-9_-]{1,64}$/.test(value) &&
            value.toLowerCase() !== 'null' &&
            value.toLowerCase() !== 'undefined';
    }

    function safeDocumentId(value) {
        return validId(value) ? value : 'shared';
    }

    function announceRemote(messages) {
        if (!activity || !messages.length) return;

        messages.forEach(message => remoteEvents.unshift(message));

        remoteEvents.length = Math.min(remoteEvents.length, MAX_EVENTS);

        activity.replaceChildren();

        const heading = document.createElement('strong');
        heading.textContent =
            `Recent remote changes (last ${remoteEvents.length})`;

        activity.appendChild(heading);

        const list = document.createElement('ul');
        list.className = 'mb-0 mt-1';

        for (const message of remoteEvents) {
            const li = document.createElement('li');
            li.textContent = message;
            list.appendChild(li);
        }

        activity.appendChild(list);
        activity.hidden = false;
    }

    function visualizeRemote(previous, next) {
        const before = new Map(
            previous.map((block, index) => [
                block.id,
                { block, index }
            ])
        );

        const after = new Map(
            next.map((block, index) => [
                block.id,
                { block, index }
            ])
        );

        const messages = [];
        const effects = new Map();
        let removed = 0;

        for (const [id, entry] of after) {
            const old = before.get(id);

            if (!old) {
                effects.set(id, 'notes-remote-added');
                messages.push(
                    `Added: ${entry.block.type} (position ${entry.index + 1})`
                );
            } else if (
                old.block.text !== entry.block.text ||
                old.block.type !== entry.block.type
            ) {
                effects.set(id, 'notes-remote-updated');
                messages.push(
                    `Edited: ${entry.block.type} (position ${entry.index + 1})`
                );
            } else if (old.index !== entry.index) {
                effects.set(id, 'notes-remote-moved');
                messages.push(
                    `Moved: ${entry.block.type} to position ${entry.index + 1}`
                );
            }
        }

        for (const [id, old] of before) {
            if (!after.has(id)) {
                removed++;

                messages.push(
                    `Deleted: ${old.block.type} (previous position ${old.index + 1})`
                );
            }
        }

        for (const [id, className] of effects) {
            const node = Array.from(container.children)
                .find(element => element.dataset.blockId === id);

            if (!node) continue;

            node.classList.add(className);

            const marker = document.createElement('span');
            marker.className = 'notes-remote-label badge rounded-pill';

            marker.textContent =
                className === 'notes-remote-added'
                    ? 'Added remotely'
                    : className === 'notes-remote-updated'
                        ? 'Edited remotely'
                        : 'Moved remotely';

            node.querySelector('.card-body')?.prepend(marker);

            window.setTimeout(() => {
                node.classList.remove(className);
                marker.remove();
            }, 3500);
        }

        if (removed) {
            const notice = document.createElement('p');

            notice.className =
                'notes-remote-deleted alert alert-warning py-2';

            notice.textContent =
                `${removed} block(s) deleted in another browser.`;

            container.prepend(notice);

            window.setTimeout(() => notice.remove(), 3500);
        }

        announceRemote(messages);
    }

    function setStatus(text) {
        if (saveStatus) {
            saveStatus.textContent = text;
        }
    }

    function newId() {
        return crypto.randomUUID?.() ||
            `b-${Date.now()}-${Math.random().toString(36).slice(2)}`;
    }

    function normalize(input) {
        return {
            id: typeof input?.id === 'string' && input.id
                ? input.id
                : newId(),

            type: TYPES.has(input?.type)
                ? input.type
                : 'paragraph',

            text: typeof input?.text === 'string'
                ? input.text
                : ''
        };
    }

    function esc(value) {
        return value
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#39;');
    }

    function inline(raw) {
        return esc(raw)
            .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
            .replace(/\*(.+?)\*/g, '<em>$1</em>')
            .replace(/`([^`]+)`/g, '<code>$1</code>');
    }

    function showPreview() {
        if (!previewVisible) return;

        preview.innerHTML = blocks.map(block => {
            const lines = block.text.split(/\r?\n/);

            if (block.type === 'heading1') {
                return `<h1>${inline(block.text)}</h1>`;
            }

            if (block.type === 'heading2') {
                return `<h2>${inline(block.text)}</h2>`;
            }

            if (
                block.type === 'bullets' ||
                block.type === 'numbers'
            ) {
                const tag = block.type === 'bullets'
                    ? 'ul'
                    : 'ol';

                const items = lines
                    .filter(line => line.trim())
                    .map(line => {
                        const content = line.replace(
                            /^\s*(?:[-*]|\d+\.)\s+/,
                            ''
                        );

                        return `<li>${inline(content)}</li>`;
                    })
                    .join('');

                return `<${tag}>${items}</${tag}>`;
            }

            return `<p>${lines.map(inline).join('<br>')}</p>`;
        }).join('') ||
            '<p class="text-muted">No content to preview.</p>';
    }

    function render() {
        container.replaceChildren();

        blocks.forEach((block, index) => {
            const node =
                template.content.firstElementChild.cloneNode(true);

            node.dataset.blockId = block.id;

            node.querySelector('.notes-block-type').value =
                block.type;

            node.querySelector('.notes-block-text').value =
                block.text;

            node.querySelector('.notes-move-up').disabled =
                index === 0;

            node.querySelector('.notes-move-down').disabled =
                index === blocks.length - 1;

            container.appendChild(node);
        });

        empty.hidden = blocks.length > 0;

        showPreview();
    }

    function blockIndex(element) {
        const id = element
            .closest('.notes-block')
            ?.dataset.blockId;

        return blocks.findIndex(block => block.id === id);
    }

    function applySnapshot(snapshot, remote = false) {
        const previous = remote
            ? blocks.map(block => ({ ...block }))
            : [];

        version = snapshot.version;

        blocks = (snapshot.blocks || []).map(normalize);

        dirty = false;

        render();

        if (remote) {
            visualizeRemote(previous, blocks);
        }

        setStatus(`Server version ${version} loaded.`);
    }

    async function save() {
        clearTimeout(timer);
        timer = null;

        if (!dirty || !connected || saving) return;

        saving = true;

        const currentGeneration = generation;
        const sent = JSON.stringify(blocks);
        const sentVersion = version;

        try {
            const saved = await connection.invoke(
                'SaveDocument',
                documentId,
                sentVersion,
                JSON.parse(sent)
            );

            if (currentGeneration !== generation) return;

            version = saved.version;

            dirty = JSON.stringify(blocks) !== sent;

            setStatus(`Saved on server · version ${version}`);

            if (dirty) {
                queueSave();
            }
        } catch (error) {
            console.warn('Notes save failed:', error);

            if (currentGeneration !== generation) return;

            dirty = true;

            setStatus(
                'Save failed or document changed elsewhere. ' +
                'Your unsaved text is kept here. ' +
                'Reload server version to resolve ' +
                '(this discards local edits).'
            );
        } finally {
            saving = false;
        }
    }

    function queueSave() {
        dirty = true;

        setStatus('Unsaved changes…');

        clearTimeout(timer);

        timer = setTimeout(save, 650);

        showPreview();
    }

    async function join(id) {
        const normalizedId = safeDocumentId(id);

        if (
            dirty &&
            !window.confirm(
                'Discard unsaved changes and open another document?'
            )
        ) {
            return;
        }

        clearTimeout(timer);
        timer = null;

        generation++;
        dirty = false;

        documentId = normalizedId;
        documentInput.value = normalizedId;

        history.replaceState(
            null,
            '',
            `${location.pathname}?doc=${encodeURIComponent(normalizedId)}`
        );

        try {
            if (!connected) {
                throw new Error('Not connected');
            }

            const snapshot = await connection.invoke(
                'JoinDocument',
                normalizedId
            );

            applySnapshot(snapshot);
        } catch (error) {
            setStatus(
                `Could not open document: ${error.message}`
            );
        }
    }

    async function initialize() {
        if (typeof signalR === 'undefined') {
            connectionStatus.textContent =
                'SignalR JavaScript library not loaded. Check internet/CDN.';

            return;
        }

        connection = new signalR.HubConnectionBuilder()
            .withUrl('/notesHub')
            .withAutomaticReconnect()
            .build();

        connection.on('DocumentChanged', snapshot => {
            if (
                snapshot.documentId !== documentId ||
                snapshot.version <= version
            ) {
                return;
            }

            if (dirty || saving) {
                setStatus(
                    'Another browser changed this document. ' +
                    'Your unsaved edits remain here. ' +
                    'Use Reload server version to accept their changes.'
                );

                return;
            }

            applySnapshot(snapshot, true);

            setStatus(
                `Updated from another browser · version ${version}`
            );
        });

        connection.onreconnecting(() => {
            connected = false;

            connectionStatus.textContent = 'Reconnecting…';
        });

        connection.onreconnected(async () => {
            connected = true;

            connectionStatus.textContent =
                'Connected · live synchronization';

            if (!dirty) {
                await join(documentId);
            } else {
                setStatus(
                    'Reconnected. Local unsaved changes preserved; ' +
                    'reload server version if needed.'
                );
            }
        });

        connection.onclose(() => {
            connected = false;

            connectionStatus.textContent =
                'Disconnected. Reload page to connect again.';
        });

        try {
            await connection.start();

            connected = true;

            connectionStatus.textContent =
                'Connected · live synchronization';

            const fromUrl = new URLSearchParams(
                window.location.search
            ).get('doc');

            const initialDocumentId = safeDocumentId(fromUrl);

            await join(initialDocumentId);
        } catch (error) {
            connectionStatus.textContent =
                `Connection failed: ${error.message}`;
        }
    }

    container.addEventListener('input', event => {
        if (!event.target.matches('.notes-block-text')) return;

        const index = blockIndex(event.target);

        if (index < 0) return;

        blocks[index].text = event.target.value;

        queueSave();
    });

    container.addEventListener('change', event => {
        if (!event.target.matches('.notes-block-type')) return;

        const index = blockIndex(event.target);

        if (index < 0) return;

        blocks[index].type = TYPES.has(event.target.value)
            ? event.target.value
            : 'paragraph';

        queueSave();
    });

    container.addEventListener('click', event => {
        const button = event.target.closest('button');

        if (!button) return;

        const index = blockIndex(button);

        if (index < 0) return;

        if (button.classList.contains('notes-delete')) {
            blocks.splice(index, 1);
        } else if (
            button.classList.contains('notes-move-up') &&
            index > 0
        ) {
            [blocks[index - 1], blocks[index]] =
                [blocks[index], blocks[index - 1]];
        } else if (
            button.classList.contains('notes-move-down') &&
            index < blocks.length - 1
        ) {
            [blocks[index + 1], blocks[index]] =
                [blocks[index], blocks[index + 1]];
        } else {
            return;
        }

        render();
        queueSave();
    });

    addButton.addEventListener('click', () => {
        blocks.push(
            normalize({
                type: 'paragraph',
                text: ''
            })
        );

        render();
        queueSave();

        container.lastElementChild
            ?.querySelector('textarea')
            ?.focus();
    });

    clearButton.addEventListener('click', () => {
        if (!confirm(
            'Clear the SHARED document for every browser?'
        )) {
            return;
        }

        blocks = [];

        render();
        queueSave();
    });

    previewButton.addEventListener('click', () => {
        previewVisible = !previewVisible;

        preview.hidden = !previewVisible;

        previewButton.textContent =
            previewVisible ? 'Hide preview' : 'Preview';

        previewButton.setAttribute(
            'aria-pressed',
            String(previewVisible)
        );

        showPreview();
    });

    openButton.addEventListener('click', () => {
        const requestedId = documentInput.value.trim();

        if (!validId(requestedId)) {
            setStatus(
                'Invalid document ID. Use 1–64 letters, ' +
                'numbers, - or _. The values null and ' +
                'undefined are not allowed.'
            );

            documentInput.value = documentId;

            return;
        }

        join(requestedId);
    });

    reloadButton.addEventListener('click', async () => {
        if (
            dirty &&
            !confirm(
                'Discard unsaved local changes and load ' +
                'the server document?'
            )
        ) {
            return;
        }

        await join(documentId);
    });

    importButton.addEventListener('click', () => {
        let local;

        try {
            local = JSON.parse(
                localStorage.getItem(LOCAL_KEY) || 'null'
            );
        } catch {
            local = null;
        }

        if (!Array.isArray(local)) {
            setStatus(
                'No C12 local document found in this browser.'
            );

            return;
        }

        if (!confirm(
            'Replace the current shared document ' +
            'with your C12 notes?'
        )) {
            return;
        }

        blocks = local.slice(0, 500).map(normalize);

        render();
        queueSave();
    });

    window.addEventListener('pagehide', () => {
        clearTimeout(timer);
    });

    initialize();
})();
