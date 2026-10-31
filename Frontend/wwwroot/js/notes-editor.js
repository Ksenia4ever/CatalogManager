(() => {
    'use strict';

    const KEY = 'catalogmanager.c12.notes.v1';
    const VALID_TYPES = new Set(['paragraph', 'heading1', 'heading2', 'bullets', 'numbers']);
    const container = document.getElementById('notesBlocks');
    if (!container) return;

    const template = document.getElementById('notesBlockTemplate');
    const status = document.getElementById('notesSaveStatus');
    const empty = document.getElementById('notesEmpty');
    const preview = document.getElementById('notesPreview');
    const togglePreview = document.getElementById('notesTogglePreview');

    let blocks = [];
    let saveTimer = null;
    let previewVisible = false;

    function newId() {
        return (typeof crypto !== 'undefined' && crypto.randomUUID)
            ? crypto.randomUUID()
            : `block-${Date.now()}-${Math.random().toString(36).slice(2)}`;
    }

    function normalize(value) {
        return {
            id: typeof value?.id === 'string' && value.id ? value.id : newId(),
            type: VALID_TYPES.has(value?.type) ? value.type : 'paragraph',
            text: typeof value?.text === 'string' ? value.text : ''
        };
    }

    function load() {
        try {
            const json = localStorage.getItem(KEY);
            if (json !== null) {
                const parsed = JSON.parse(json);
                if (!Array.isArray(parsed)) throw new Error('Document must be a list');
                blocks = parsed.slice(0, 500).map(normalize);
            } else {
                blocks = [normalize({ type: 'heading1', text: 'My notes' }),
                normalize({ type: 'paragraph', text: 'Start writing here.' })];
            }
            status.textContent = 'Document restored. Changes save automatically.';
        } catch (error) {
            console.warn('Could not restore notes document:', error);
            blocks = [];
            status.textContent = 'Could not load saved document. You can start a new one.';
        }
        render();
    }

    function save() {
        clearTimeout(saveTimer);
        saveTimer = null;
        try {
            localStorage.setItem(KEY, JSON.stringify(blocks));
            status.textContent = `Saved locally at ${new Date().toLocaleTimeString()}`;
        } catch (error) {
            console.error('Could not save notes:', error);
            status.textContent = 'Could not save. Check browser storage settings.';
        }
    }

    function queueSave() {
        status.textContent = 'Saving…';
        clearTimeout(saveTimer);
        saveTimer = setTimeout(save, 350);
        if (previewVisible) renderPreview();
    }

    function escapeHtml(text) {
        return text.replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#39;');
    }

    function inlineMarkdown(raw) {
        // Escape HTML before applying a small, safe subset of inline Markdown.
        const safe = escapeHtml(raw);
        return safe.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
            .replace(/\*(.+?)\*/g, '<em>$1</em>')
            .replace(/`([^`]+)`/g, '<code>$1</code>');
    }

    function renderPreview() {
        preview.replaceChildren();
        if (!previewVisible) return;
        const html = blocks.map(block => {
            const lines = block.text.split(/\r?\n/);
            if (block.type === 'heading1') return `<h1>${inlineMarkdown(block.text)}</h1>`;
            if (block.type === 'heading2') return `<h2>${inlineMarkdown(block.text)}</h2>`;
            if (block.type === 'bullets' || block.type === 'numbers') {
                const tag = block.type === 'bullets' ? 'ul' : 'ol';
                const items = lines.filter(line => line.trim())
                    .map(line => `<li>${inlineMarkdown(line.replace(/^\s*(?:[-*]|\d+\.)\s+/, ''))}</li>`)
                    .join('');
                return `<${tag}>${items}</${tag}>`;
            }
            return `<p>${lines.map(inlineMarkdown).join('<br>')}</p>`;
        }).join('');
        // Only escaped user input is embedded; tags are generated from fixed strings.
        preview.innerHTML = html || '<p class="text-muted">No content to preview.</p>';
    }

    function render() {
        container.replaceChildren();
        blocks.forEach((block, index) => {
            const node = template.content.firstElementChild.cloneNode(true);
            node.dataset.blockId = block.id;
            node.querySelector('.notes-block-type').value = block.type;
            node.querySelector('.notes-block-text').value = block.text;
            node.querySelector('.notes-move-up').disabled = index === 0;
            node.querySelector('.notes-move-down').disabled = index === blocks.length - 1;
            container.appendChild(node);
        });
        empty.hidden = blocks.length > 0;
        renderPreview();
    }

    function findBlock(element) {
        const article = element.closest('.notes-block');
        if (!article) return null;
        const index = blocks.findIndex(block => block.id === article.dataset.blockId);
        return index < 0 ? null : { article, index };
    }

    container.addEventListener('input', event => {
        if (!event.target.matches('.notes-block-text')) return;
        const found = findBlock(event.target);
        if (!found) return;
        blocks[found.index].text = event.target.value;
        queueSave();
    });

    container.addEventListener('change', event => {
        if (!event.target.matches('.notes-block-type')) return;
        const found = findBlock(event.target);
        if (!found) return;
        blocks[found.index].type = VALID_TYPES.has(event.target.value)
            ? event.target.value : 'paragraph';
        queueSave();
    });

    container.addEventListener('click', event => {
        const button = event.target.closest('button');
        if (!button) return;
        const found = findBlock(button);
        if (!found) return;
        const index = found.index;
        if (button.classList.contains('notes-delete')) {
            blocks.splice(index, 1);
        } else if (button.classList.contains('notes-move-up') && index > 0) {
            [blocks[index - 1], blocks[index]] = [blocks[index], blocks[index - 1]];
        } else if (button.classList.contains('notes-move-down') && index < blocks.length - 1) {
            [blocks[index + 1], blocks[index]] = [blocks[index], blocks[index + 1]];
        } else {
            return;
        }
        render();
        queueSave();
    });

    document.getElementById('notesAdd').addEventListener('click', () => {
        const block = normalize({ type: 'paragraph', text: '' });
        blocks.push(block);
        render();
        queueSave();
        container.lastElementChild?.querySelector('textarea')?.focus();
    });

    document.getElementById('notesClear').addEventListener('click', () => {
        if (!window.confirm('Delete all blocks from this browser?')) return;
        blocks = [];
        render();
        save();
    });

    togglePreview.addEventListener('click', () => {
        previewVisible = !previewVisible;
        preview.hidden = !previewVisible;
        togglePreview.setAttribute('aria-pressed', String(previewVisible));
        togglePreview.textContent = previewVisible ? 'Hide preview' : 'Preview';
        renderPreview();
    });

    window.addEventListener('pagehide', () => {
        if (saveTimer !== null) save();
    });

    load();
})();