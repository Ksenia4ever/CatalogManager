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

    // C14: Notifications only for changes received from a different client.
    const activity = document.getElementById('notesRemoteActivity');
    let activityTimer = null;
    const remoteEvents = [];
    const MAX_EVENTS = 8;

    function announceRemote(messages) {
        if (!activity || !messages.length) return;
        messages.forEach(message => remoteEvents.unshift(message));
        remoteEvents.length = Math.min(remoteEvents.length, MAX_EVENTS);
        activity.replaceChildren();
        const heading = document.createElement('strong');
        heading.textContent = `Recent remote changes (last ${remoteEvents.length})`;
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
        const before = new Map(previous.map((block, index) => [block.id, { block, index }]));
        const after = new Map(next.map((block, index) => [block.id, { block, index }]));
        const messages = [];
        const effects = new Map();
        let removed = 0;

        for (const [id, entry] of after) {
            const old = before.get(id);
            if (!old) {
                effects.set(id, 'notes-remote-added');
                messages.push(`Added: ${entry.block.type} (position ${entry.index + 1})`);
            } else if (old.block.text !== entry.block.text || old.block.type !== entry.block.type) {
                effects.set(id, 'notes-remote-updated');
                messages.push(`Edited: ${entry.block.type} (position ${entry.index + 1})`);
            } else if (old.index !== entry.index) {
                effects.set(id, 'notes-remote-moved');
                messages.push(`Moved: ${entry.block.type} to position ${entry.index + 1}`);
            }
        }
        for (const [id, old] of before) {
            if (!after.has(id)) {
                removed++;
                messages.push(`Deleted: ${old.block.type} (previous position ${old.index + 1})`);
            }
        }

        for (const [id, className] of effects) {
            const node = Array.from(container.children).find(el => el.dataset.blockId === id);
            if (!node) continue;
            node.classList.add(className);
            const marker = document.createElement('span');
            marker.className = 'notes-remote-label badge rounded-pill';
            marker.textContent = className === 'notes-remote-added' ? 'Added remotely'
                : className === 'notes-remote-updated' ? 'Edited remotely' : 'Moved remotely';
            node.querySelector('.card-body')?.prepend(marker);
            window.setTimeout(() => {
                node.classList.remove(className);
                marker.remove();
            }, 3500);
        }
        if (removed) {
            // Deleted blocks no longer exist in the new snapshot; a notice is displayed instead.
            const notice = document.createElement('p');
            notice.className = 'notes-remote-deleted alert alert-warning py-2';
            notice.textContent = `${removed} block(s) deleted in another browser.`;
            container.prepend(notice);
            window.setTimeout(() => notice.remove(), 3500);
        }
        announceRemote(messages);
    }

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
    // Treat URL placeholders as missing IDs, not as real document names.
    // The regular expression alone would accept the literal strings "null" and "undefined".
    function validId(value) {
        return typeof value === 'string'
            && value.toLowerCase() !== 'null'
            && value.toLowerCase() !== 'undefined'
            && /^[A-Za-z0-9_-]{1,64}$/.test(value);
    }

    function documentIdFromUrl() {
        const value = new URLSearchParams(window.location.search).get('doc');
        return validId(value) ? value : 'shared';
    }
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
    // F3: Native HTML Drag and Drop for block ordering.
    // A dedicated handle avoids interfering with selecting or editing text.
    let draggedBlockId = null;
    let dropTargetId = null;
    let dropAfter = false;

    const f3Style = document.createElement('style');
    f3Style.textContent = `
        .notes-drag-handle { cursor: grab; user-select: none; touch-action: none; }
        .notes-drag-handle:active { cursor: grabbing; }
        .notes-f3-dragging { opacity: .45; }
        .notes-f3-before { outline: 3px solid #0d6efd; outline-offset: 1px; }
        .notes-f3-after { box-shadow: 0 4px 0 0 #0d6efd; }
        @media (prefers-reduced-motion: no-preference) {
            .notes-f3-before, .notes-f3-after { transition: outline .12s, box-shadow .12s; }
        }
    `;
    document.head.appendChild(f3Style);

    function clearDragMarkers() {
        container.querySelectorAll('.notes-f3-dragging, .notes-f3-before, .notes-f3-after')
            .forEach(el => el.classList.remove('notes-f3-dragging', 'notes-f3-before', 'notes-f3-after'));
        dropTargetId = null;
    }

    function resetDrag() {
        draggedBlockId = null;
        clearDragMarkers();
    }

    function positionDrop(event, node) {
        if (!draggedBlockId || node.dataset.blockId === draggedBlockId) return false;
        const rect = node.getBoundingClientRect();
        dropAfter = event.clientY >= rect.top + rect.height / 2;
        clearDragMarkers();
        dropTargetId = node.dataset.blockId;
        node.classList.add(dropAfter ? 'notes-f3-after' : 'notes-f3-before');
        const source = Array.from(container.children)
            .find(el => el.dataset.blockId === draggedBlockId);
        source?.classList.add('notes-f3-dragging');
        return true;
    }

    container.addEventListener('dragstart', event => {
        const handle = event.target.closest?.('.notes-drag-handle');
        if (!handle) {
            event.preventDefault();
            return;
        }
        const node = handle.closest('.notes-block');
        if (!node) return;
        draggedBlockId = node.dataset.blockId;
        event.dataTransfer.effectAllowed = 'move';
        event.dataTransfer.setData('text/plain', draggedBlockId);
        node.classList.add('notes-f3-dragging');
    });
    container.addEventListener('dragover', event => {
        if (!draggedBlockId) return;
        const node = event.target.closest?.('.notes-block');
        if (!node || !positionDrop(event, node)) return;
        event.preventDefault();
        event.dataTransfer.dropEffect = 'move';
    });
    container.addEventListener('drop', event => {
        if (!draggedBlockId) return;
        event.preventDefault();
        const targetNode = event.target.closest?.('.notes-block');
        if (!targetNode || targetNode.dataset.blockId === draggedBlockId) {
            resetDrag();
            return;
        }
        positionDrop(event, targetNode);
        const sourceIndex = blocks.findIndex(b => b.id === draggedBlockId);
        if (sourceIndex < 0 || !dropTargetId) {
            resetDrag();
            return;
        }
        const targetId = dropTargetId;
        const after = dropAfter;
        const [moved] = blocks.splice(sourceIndex, 1);
        const targetIndex = blocks.findIndex(b => b.id === targetId);
        if (targetIndex < 0) {
            blocks.splice(sourceIndex, 0, moved);
            resetDrag();
            return;
        }
        const destination = targetIndex + (after ? 1 : 0);
        blocks.splice(destination, 0, moved);
        resetDrag();
        if (destination !== sourceIndex) {
            render();
            queueSave();
        }
    });
    container.addEventListener('dragend', resetDrag);
    // Preserve normal focus and text selection in textarea/select controls.

    function render() {
        container.replaceChildren();
        blocks.forEach((b, index) => {
            const node = template.content.firstElementChild.cloneNode(true);
            node.dataset.blockId = b.id;
            const handle = document.createElement('button');
            handle.type = 'button';
            handle.className = 'notes-drag-handle btn btn-outline-secondary btn-sm me-2 mb-2';
            handle.textContent = '☰ Drag';
            handle.draggable = true;
            handle.title = 'Drag to reorder this block';
            handle.setAttribute('aria-label', `Drag block ${index + 1} to reorder`);
            (node.querySelector('.card-body') || node).prepend(handle);
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
    function applySnapshot(snapshot, remote = false) {
        const previous = remote ? blocks.map(b => ({ ...b })) : [];
        version = snapshot.version;
        blocks = (snapshot.blocks || []).map(normalize);
        dirty = false;
        render();
        if (remote) visualizeRemote(previous, blocks);
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
            applySnapshot(snapshot, true);
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
            await join(documentIdFromUrl());
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

    // C15: Browser File API — import JSON and export JSON/Markdown.
    // Controls are created dynamically to avoid changing the C12-C14 Razor View.
    const fileTools = document.createElement('div');
    fileTools.className = 'd-flex flex-wrap gap-2 mt-2 mb-3 align-items-center';
    fileTools.setAttribute('aria-label', 'C15 document file tools');

    const importFileButton = document.createElement('button');
    importFileButton.type = 'button';
    importFileButton.className = 'btn btn-outline-primary';
    importFileButton.textContent = 'Import JSON file';

    const exportJsonButton = document.createElement('button');
    exportJsonButton.type = 'button';
    exportJsonButton.className = 'btn btn-outline-success';
    exportJsonButton.textContent = 'Export JSON';

    const exportMarkdownButton = document.createElement('button');
    exportMarkdownButton.type = 'button';
    exportMarkdownButton.className = 'btn btn-outline-secondary';
    exportMarkdownButton.textContent = 'Export Markdown';

    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.accept = '.json,application/json';
    fileInput.hidden = true;
    fileInput.setAttribute('aria-label', 'Select a notes JSON file');

    const fileDescription = document.createElement('span');
    fileDescription.className = 'text-muted small';
    fileDescription.textContent = 'C15 · FileReader, Blob and object URLs';

    fileTools.append(importFileButton, exportJsonButton,
        exportMarkdownButton, fileInput, fileDescription);
    // Existing Import C12 button remains unchanged.
    importButton.insertAdjacentElement('afterend', fileTools);

    const MAX_FILE_BYTES = 1024 * 1024;
    const MAX_IMPORT_BLOCKS = 500;
    const MAX_TEXT_LENGTH = 10000;

    function validateImportedDocument(payload) {
        // Accept exports from this version and a plain array of blocks.
        const importedBlocks = Array.isArray(payload) ? payload : payload?.blocks;
        if (!Array.isArray(importedBlocks)) {
            throw new Error('JSON must contain a blocks array.');
        }
        if (importedBlocks.length > MAX_IMPORT_BLOCKS) {
            throw new Error(`Maximum ${MAX_IMPORT_BLOCKS} blocks per file.`);
        }
        const ids = new Set();
        return importedBlocks.map((item, index) => {
            if (!item || typeof item !== 'object' || Array.isArray(item)) {
                throw new Error(`Block ${index + 1} must be an object.`);
            }
            if (!TYPES.has(item.type) || typeof item.text !== 'string') {
                throw new Error(`Block ${index + 1} has an invalid type or text.`);
            }
            if (item.text.length > MAX_TEXT_LENGTH) {
                throw new Error(`Text in block ${index + 1} is too long.`);
            }
            // Do not trust IDs from files; assign fresh IDs for the target document.
            const result = { id: newId(), type: item.type, text: item.text };
            if (ids.has(result.id)) throw new Error('Duplicate block ID generated.');
            ids.add(result.id);
            return result;
        });
    }

    function downloadText(filename, contents, mimeType) {
        const blob = new Blob([contents], { type: mimeType });
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = filename;
        document.body.append(anchor);
        anchor.click();
        anchor.remove();
        // Release the temporary browser object URL after the click has started.
        window.setTimeout(() => URL.revokeObjectURL(url), 1000);
    }

    function markdownFromBlocks(items) {
        return items.map(block => {
            const text = block.text.replace(/\r\n/g, '\n');
            if (block.type === 'heading1') return `# ${text}`;
            if (block.type === 'heading2') return `## ${text}`;
            if (block.type === 'bullets') {
                return text.split('\n').filter(line => line.trim())
                    .map(line => `- ${line.replace(/^\s*[-*]\s+/, '')}`).join('\n');
            }
            if (block.type === 'numbers') {
                return text.split('\n').filter(line => line.trim())
                    .map((line, index) => `${index + 1}. ${line.replace(/^\s*\d+\.\s+/, '')}`)
                    .join('\n');
            }
            return text;
        }).join('\n\n') + '\n';
    }

    exportJsonButton.addEventListener('click', () => {
        const payload = {
            format: 'CatalogManagerNotes',
            formatVersion: 1,
            exportedAt: new Date().toISOString(),
            documentId,
            blocks: blocks.map(({ id, type, text }) => ({ id, type, text }))
        };
        downloadText(`${documentId}.json`, JSON.stringify(payload, null, 2),
            'application/json;charset=utf-8');
        setStatus('JSON document exported from browser.');
    });

    exportMarkdownButton.addEventListener('click', () => {
        downloadText(`${documentId}.md`, markdownFromBlocks(blocks),
            'text/markdown;charset=utf-8');
        setStatus('Markdown file exported from browser.');
    });

    importFileButton.addEventListener('click', () => fileInput.click());

    fileInput.addEventListener('change', () => {
        const file = fileInput.files?.[0];
        fileInput.value = '';
        if (!file) return;
        if (file.size > MAX_FILE_BYTES) {
            setStatus('File is too large (maximum 1 MB).');
            return;
        }
        if (!file.name.toLowerCase().endsWith('.json')) {
            setStatus('Please select a .json file.');
            return;
        }

        const reader = new FileReader();
        reader.onerror = () => setStatus('The browser could not read the selected file.');
        reader.onload = () => {
            let imported;
            try {
                imported = validateImportedDocument(JSON.parse(String(reader.result)));
            } catch (error) {
                setStatus(`Import rejected: ${error.message}`);
                return;
            }
            if (!connected) {
                setStatus('Connect to the server before importing.');
                return;
            }
            if (saving) {
                setStatus('A save is still running. Import again after it completes.');
                return;
            }
            if (!window.confirm(
                `Replace all blocks in shared document "${documentId}" with ${imported.length} block(s)? ` +
                'The change will be synchronized to other browsers.')) return;

            clearTimeout(timer);
            blocks = imported;
            render();
            queueSave();
            setStatus('JSON imported; saving to server…');
        };
        reader.readAsText(file, 'UTF-8');
    });

    window.addEventListener('pagehide', () => {
        // Unlike C12 localStorage, network operations are not guaranteed during unload.
        // Leave the page only after “Saved on server” is displayed.
        clearTimeout(timer);
    });
    initialize();
})();