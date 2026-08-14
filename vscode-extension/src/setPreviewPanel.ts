import * as vscode from 'vscode';
import * as path from 'path';
import { ensureViewBox } from './previewPanel';

export interface SetPreviewIcon {
    name: string;
    svg: string;
}

/** Message a set-preview webview can post back to the extension. Currently just the style
 * switcher, same shape/purpose as the single-icon preview's (see previewPanel.ts). */
export interface SetPreviewWebviewMessage {
    type: 'styleChanged';
    style: string;
}

/**
 * One Webview panel per previewed icon-set *folder* (as opposed to previewPanel.ts, one per .sp
 * *file*) -- a grid/gallery of every icon in the folder at once, for checking whole-set visual
 * consistency (matching pen width, color style, ...) rather than one icon in isolation. Rendering
 * itself is driven by setPreviewCommand.ts, which shells out once to the classic root command
 * (`sketchpen <folder> -outfolder <tmp> -format svg`) instead of one `compose` call per icon --
 * this class only lays out already-rendered SVG strings into a scrollable grid with a style
 * switcher and a client-side name filter (no CLI involvement for filtering).
 */
export class SetPreviewPanelManager {
    private readonly panels = new Map<string, vscode.WebviewPanel>();
    private messageHandler: ((folder: string, message: SetPreviewWebviewMessage) => void) | undefined;

    /** Registers the callback invoked when any set-preview panel posts a message (e.g. a style
     * switch). Mirrors PreviewPanelManager.setMessageHandler -- kept separate from the
     * constructor so extension.ts doesn't need to wire this up before the owning manager exists. */
    setMessageHandler(handler: (folder: string, message: SetPreviewWebviewMessage) => void): void {
        this.messageHandler = handler;
    }

    showPreview(folder: string, icons: SetPreviewIcon[], availableStyles: string[], selectedStyle: string): void {
        const panel = this.getOrCreatePanel(folder);
        panel.webview.html = this.renderHtml(icons, availableStyles, selectedStyle);
    }

    showError(folder: string, message: string): void {
        const panel = this.panels.get(folder);
        panel?.webview.postMessage({ type: 'error', message });
    }

    reveal(folder: string): vscode.WebviewPanel {
        const panel = this.getOrCreatePanel(folder);
        panel.reveal(vscode.ViewColumn.Active, false);
        return panel;
    }

    dispose(): void {
        for (const panel of this.panels.values()) {
            panel.dispose();
        }
        this.panels.clear();
    }

    private getOrCreatePanel(folder: string): vscode.WebviewPanel {
        const existing = this.panels.get(folder);
        if (existing) {
            return existing;
        }

        const panel = vscode.window.createWebviewPanel(
            'sketchpenSetPreview',
            `Preview Set: ${path.basename(folder)}`,
            { viewColumn: vscode.ViewColumn.Active, preserveFocus: false },
            { enableScripts: true, retainContextWhenHidden: true }
        );

        panel.webview.onDidReceiveMessage((message: SetPreviewWebviewMessage) => {
            this.messageHandler?.(folder, message);
        });

        panel.onDidDispose(() => this.panels.delete(folder));
        this.panels.set(folder, panel);
        return panel;
    }

    private renderHtml(icons: SetPreviewIcon[], availableStyles: string[], selectedStyle: string): string {
        const styleOptions = ['', ...availableStyles]
            .map((name) => {
                const label = name === '' ? 'Default' : name;
                const selected = name === selectedStyle ? ' selected' : '';
                return `<option value="${escapeHtml(name)}"${selected}>${escapeHtml(label)}</option>`;
            })
            .join('');

        const cells = icons
            .map((icon) => {
                const sized = ensureViewBox(icon.svg);
                return `<div class="cell" data-name="${escapeHtml(icon.name.toLowerCase())}">
  <div class="thumb">${sized}</div>
  <div class="label">${escapeHtml(icon.name)}</div>
</div>`;
            })
            .join('\n');

        return `<!DOCTYPE html>
<html>
<head>
<meta charset="UTF-8">
<style>
  html, body {
    height: 100%;
    margin: 0;
    background: var(--vscode-editor-background);
    color: var(--vscode-foreground);
    font-family: var(--vscode-font-family);
  }
  #toolbar {
    position: sticky;
    top: 0;
    display: flex;
    gap: 16px;
    align-items: center;
    padding: 8px 12px;
    background: var(--vscode-editorWidget-background);
    border-bottom: 1px solid var(--vscode-widget-border, transparent);
    font-size: 12px;
    z-index: 2;
  }
  #toolbar input, #toolbar select {
    background: var(--vscode-dropdown-background);
    color: var(--vscode-dropdown-foreground);
    border: 1px solid var(--vscode-dropdown-border, transparent);
    padding: 3px 6px;
    font-family: inherit;
    font-size: inherit;
  }
  #count {
    opacity: 0.7;
    white-space: nowrap;
  }
  #error-banner {
    padding: 6px 12px;
    background: #5a1d1d;
    color: #fff;
    font-size: 12px;
    display: none;
    white-space: pre-wrap;
  }
  #grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(96px, 1fr));
    gap: 16px;
    padding: 16px;
  }
  .cell {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
  }
  .cell.hidden {
    display: none;
  }
  .thumb {
    width: 72px;
    height: 72px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  .thumb svg {
    width: 100%;
    height: 100%;
  }
  .label {
    font-size: 10px;
    text-align: center;
    word-break: break-word;
    max-width: 92px;
    opacity: 0.8;
  }
</style>
</head>
<body>
<div id="toolbar">
  <span id="count"></span>
  <input id="filter" type="text" placeholder="Filter by name...">
  <label>Style: <select id="style-select">${styleOptions}</select></label>
</div>
<div id="error-banner"></div>
<div id="grid">
${cells}
</div>
<script>
  const vscode = acquireVsCodeApi();
  const cells = Array.from(document.querySelectorAll('.cell'));
  const countLabel = document.getElementById('count');

  function updateCount(visible) {
    countLabel.textContent = visible === cells.length
      ? cells.length + ' icon' + (cells.length === 1 ? '' : 's')
      : visible + ' / ' + cells.length + ' icons';
  }
  updateCount(cells.length);

  const filter = document.getElementById('filter');
  filter.addEventListener('input', () => {
    const q = filter.value.trim().toLowerCase();
    let visible = 0;
    for (const cell of cells) {
      const match = !q || cell.dataset.name.includes(q);
      cell.classList.toggle('hidden', !match);
      if (match) visible++;
    }
    updateCount(visible);
  });

  const styleSelect = document.getElementById('style-select');
  styleSelect.addEventListener('change', () => {
    vscode.postMessage({ type: 'styleChanged', style: styleSelect.value });
  });

  const banner = document.getElementById('error-banner');
  window.addEventListener('message', (event) => {
    const data = event.data;
    if (data && data.type === 'error') {
      banner.textContent = data.message;
      banner.style.display = 'block';
    }
  });
</script>
</body>
</html>`;
    }
}

function escapeHtml(text: string): string {
    return text.replace(/[&<>"']/g, (c) => {
        switch (c) {
            case '&':
                return '&amp;';
            case '<':
                return '&lt;';
            case '>':
                return '&gt;';
            case '"':
                return '&quot;';
            default:
                return '&#39;';
        }
    });
}
