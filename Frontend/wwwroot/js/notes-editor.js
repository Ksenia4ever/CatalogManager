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

    function setStatus(text) { saveStatus.textContent = text; }
    function newId() {
        return crypto.randomUUID?.() || `b-${Date.now()}-${Math.random().toString(36).slice(2)}`;
    }
    function normalize(input) {
        return {
            id: typeof input?.id === 'string' && input.id ? input.id : newId(),
            type: TYPES.has(input?.type) ? input.type : 'paragraph',
            text: typeof input?.text === 'string' ? input.text : ''
        };
    }
    function validId(value) { return /^[A-Za-z0-9_-]{1,64}$/.test(value); }
    function esc(value) {
        return value.replaceAll('&', '&amp;').replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;').replaceAll('"', '&quot;').replaceAll("'", '&#39;');
    }
    function inline(raw) {
        return esc(raw).replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
            .replace(/\*(.+?)\*/g, '<em>$1</em>')
            .replace(/`([^`]+)`/g, '<code>$1</code>');
    }
    function showPreview() {
        if (!previewVisible) return;
        preview.innerHTML = blocks.map(b => {
            const lines = b.text.split(/\r?\n/);
            if (b.type === 'heading1') return `<h1>${inline(b.text)}</h1>`;
            if (b.type === 'heading2') return `<h2>${inline(b.text)}</h2>`;
            if (b.type === 'bullets' || b.type === 'numbers') {
                const tag = b.type === 'bullets' ? 'ul' : 'ol';
                const items = lines.filter(l => l.trim()).map(l =>
                    `<li>${inline(l.replace(/^\s*(?:[-*]|\d+\.)\s+/, ''))}</li>`).join('');
                return `<${tag}>${items}</${tag}>`;
            }
            return `<p>${lines.map(inline).join('<br>')}</p>`;
        }).join('') || '<p class="text-muted">No content to preview.</p>';
    }
    function render() {
        container.replaceChildren();
        blocks.forEach((b, index) => {
            const node = template.content.firstElementChild.cloneNode(true);
            node.dataset.blockId = b.id;
            node.querySelector('.notes-block-type').value = b.type;
            node.querySelector('.notes-block-text').value = b.text;
            node.querySelector('.notes-move-up').disabled = index === 0;
            node.querySelector('.notes-move-down').disabled = index === blocks.length - 1;
            container.appendChild(node);
        });
        empty.hidden = blocks.length > 0;
        showPreview();
    }
    function blockIndex(element) {
        const id = element.closest('.notes-block')?.dataset.blockId;
        return blocks.findIndex(b => b.id === id);
    }
    function applySnapshot(snapshot) {
        version = snapshot.version;
        blocks = (snapshot.blocks || []).map(normalize);
        dirty = false;
        render();
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
            const saved = await connection.invoke('SaveDocument', documentId,
                sentVersion, JSON.parse(sent));
            if (currentGeneration !== generation) return;
            version = saved.version;
            // Preserve edits typed while the request was in flight.
            dirty = JSON.stringify(blocks) !== sent;
            setStatus(`Saved on server · version ${version}`);
            if (dirty) queueSave();
        } catch (error) {
            console.warn('Notes save failed:', error);
            if (currentGeneration !== generation) return;
            dirty = true;
            setStatus('Save failed or document changed elsewhere. Your unsaved text is kept here. Reload server version to resolve (this discards local edits).');
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
        if (!validId(id)) {
            setStatus('Document ID: use 1–64 letters, digits, - or _.');
            return;
        }
        if (dirty && !window.confirm('Discard unsaved changes and open another document?')) return;
        clearTimeout(timer);
        timer = null;
        generation++;
        dirty = false;
        documentId = id;
        documentInput.value = id;
        history.replaceState(null, '', `${location.pathname}?doc=${encodeURIComponent(id)}`);
        try {
            if (!connected) throw new Error('Not connected');
            const snapshot = await connection.invoke('JoinDocument', id);
            applySnapshot(snapshot);
        } catch (error) {
            setStatus(`Could not open document: ${error.message}`);
        }
    }

    async function initialize() {
        if (typeof signalR === 'undefined') {
            connectionStatus.textContent = 'SignalR JavaScript library not loaded. Check internet/CDN.';
            return;
        }
        connection = new signalR.HubConnectionBuilder()
            .withUrl('/notesHub')
            .withAutomaticReconnect()
            .build();

        connection.on('DocumentChanged', snapshot => {
            if (snapshot.documentId !== documentId || snapshot.version <= version) return;
            if (dirty || saving) {
                setStatus('Another browser changed this document. Your unsaved edits remain here. Use Reload server version to accept their changes.');
                return;
            }
            applySnapshot(snapshot);
            setStatus(`Updated from another browser · version ${version}`);
        });
        connection.onreconnecting(() => {
            connected = false;
            connectionStatus.textContent = 'Reconnecting…';
        });
        connection.onreconnected(async () => {
            connected = true;
            connectionStatus.textContent = 'Connected · live synchronization';
            if (!dirty) await join(documentId);
            else setStatus('Reconnected. Local unsaved changes preserved; reload server version if needed.');
        });
        connection.onclose(() => {
            connected = false;
            connectionStatus.textContent = 'Disconnected. Reload page to connect again.';
        });

        try {
            await connection.start();
            connected = true;
            connectionStatus.textContent = 'Connected · live synchronization';
            const fromUrl = new URLSearchParams(location.search).get('doc');
            await join(validId(fromUrl) ? fromUrl : 'shared');
        } catch (error) {
            connectionStatus.textContent = `Connection failed: ${error.message}`;
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
        blocks[index].type = TYPES.has(event.target.value) ? event.target.value : 'paragraph';
        queueSave();
    });
    container.addEventListener('click', event => {
        const button = event.target.closest('button');
        if (!button) return;
        const index = blockIndex(button);
        if (index < 0) return;
        if (button.classList.contains('notes-delete')) blocks.splice(index, 1);
        else if (button.classList.contains('notes-move-up') && index > 0)
            [blocks[index - 1], blocks[index]] = [blocks[index], blocks[index - 1]];
        else if (button.classList.contains('notes-move-down') && index < blocks.length - 1)
            [blocks[index + 1], blocks[index]] = [blocks[index], blocks[index + 1]];
        else return;
        render();
        queueSave();
    });

    addButton.addEventListener('click', () => {
        blocks.push(normalize({ type: 'paragraph', text: '' }));
        render();
        queueSave();
        container.lastElementChild?.querySelector('textarea')?.focus();
    });
    clearButton.addEventListener('click', () => {
        if (!confirm('Clear the SHARED document for every browser?')) return;
        blocks = [];
        render();
        queueSave();
    });
    previewButton.addEventListener('click', () => {
        previewVisible = !previewVisible;
        preview.hidden = !previewVisible;
        previewButton.textContent = previewVisible ? 'Hide preview' : 'Preview';
        previewButton.setAttribute('aria-pressed', String(previewVisible));
        showPreview();
    });
    openButton.addEventListener('click', () => join(documentInput.value.trim()));
    reloadButton.addEventListener('click', async () => {
        if (dirty && !confirm('Discard unsaved local changes and load the server document?')) return;
        await join(documentId);
    });
    importButton.addEventListener('click', () => {
        let local;
        try {
            local = JSON.parse(localStorage.getItem(LOCAL_KEY) || 'null');
        } catch { local = null; }
        if (!Array.isArray(local)) {
            setStatus('No C12 local document found in this browser.');
            return;
        }
        if (!confirm('Replace the current shared document with your C12 notes?')) return;
        blocks = local.slice(0, 500).map(normalize);
        render();
        queueSave();
    });

    window.addEventListener('pagehide', () => {
        // Unlike C12 localStorage, network operations are not guaranteed during unload.
        // Leave the page only after “Saved on server” is displayed.
        clearTimeout(timer);
    });
    initialize();
})();