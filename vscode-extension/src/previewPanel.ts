import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs/promises';

/** Message a webview can post back to the extension. Currently just the style switcher. */
export interface PreviewWebviewMessage {
    type: 'styleChanged';
    style: string;
}

const SVG_ROOT_SIZE = /<svg[^>]*\swidth="([\d.]+)"[^>]*\sheight="([\d.]+)"/;

/**
 * Adds a `viewBox` to the rendered SVG's root element (derived from its existing width/height
 * attributes) if it doesn't already have one. The CLI's SVG output has no viewBox -- it's sized
 * directly in pixels at the requested reference size -- so without this, CSS-resizing the `<svg>`
 * element just changes its viewport rather than scaling the artwork, which is what actually made
 * the preview look small regardless of panel size. Falls back to the untouched SVG if the width/
 * height attributes can't be found (defensive only; every CLI-generated SVG has them).
 */
function ensureViewBox(svg: string): string {
    if (svg.includes('viewBox=')) {
        return svg;
    }
    const match = svg.match(SVG_ROOT_SIZE);
    if (!match) {
        return svg;
    }
    const [, width, height] = match;
    return svg.replace('<svg', `<svg viewBox="0 0 ${width} ${height}"`);
}

/** Builds the grid/coordinate overlay -- lines every 10 logical units across the icon's -50..+50
 * coordinate square (see docs/SYNTAX.md#coordinate-system), plus a highlighted origin crosshair
 * and axis-end labels. Purely a display aid layered on top of the rendered icon; never sent to
 * the CLI or saved anywhere. Uses the icon's own viewBox dimensions so lines line up exactly. */
function buildGridOverlay(width: number, height: number): string {
    const steps = 10; // 10 logical units per grid line, -50..+50 -> 10 divisions
    const stepX = width / steps;
    const stepY = height / steps;
    const lines: string[] = [];

    for (let i = 1; i < steps; i++) {
        lines.push(`<line x1="${i * stepX}" y1="0" x2="${i * stepX}" y2="${height}" class="grid-line" />`);
        lines.push(`<line x1="0" y1="${i * stepY}" x2="${width}" y2="${i * stepY}" class="grid-line" />`);
    }

    const cx = width / 2;
    const cy = height / 2;

    // Proportional to the icon's own size (a fixed pixel offset, e.g. "22", only looked right at
    // whatever size it was tuned against -- previewSize is user-configurable, and export sizes
    // vary a lot more than that. text-anchor does the actual left/right/center alignment; these
    // margins just keep the label off the very edge/axis line, so they only need to be "small",
    // not exact.
    const margin = Math.min(width, height) * 0.02;
    const fontSize = Math.min(width, height) * 0.035;

    const labels = [
        // x-axis labels, sitting just above the horizontal center line.
        { x: margin, y: cy - margin, anchor: 'start', text: '-50' },
        { x: width - margin, y: cy - margin, anchor: 'end', text: '+50' },
        // y-axis labels, sitting just right of the vertical center line.
        { x: cx + margin, y: margin + fontSize, anchor: 'start', text: '-50' },
        { x: cx + margin, y: height - margin, anchor: 'start', text: '+50' },
        { x: cx + margin, y: cy - margin, anchor: 'start', text: '0' }
    ];

    return `
<svg id="grid-overlay" viewBox="0 0 ${width} ${height}" xmlns="http://www.w3.org/2000/svg">
  ${lines.join('\n  ')}
  <line x1="${cx}" y1="0" x2="${cx}" y2="${height}" class="grid-axis" />
  <line x1="0" y1="${cy}" x2="${width}" y2="${cy}" class="grid-axis" />
  ${labels
      .map(
          (l) =>
              `<text x="${l.x}" y="${l.y}" text-anchor="${l.anchor}" font-size="${fontSize}" class="grid-label">${l.text}</text>`
      )
      .join('\n  ')}
</svg>`;
}

/**
 * One Webview panel per previewed .sp source file, showing the last successfully rendered SVG
 * inline (embedded directly in the page, not via <img src> -- avoids file-URI caching issues
 * entirely for content that gets rewritten on every save). On a compose failure, the panel is
 * deliberately left showing the last good render (a small error banner is posted on top instead
 * of blanking the panel) -- see diagnosticsManager.ts.
 *
 * Also hosts two purely cosmetic display aids that never touch the CLI: a resizable/scaled-up
 * icon (via ensureViewBox, above) and an optional coordinate-grid overlay (toggled client-side,
 * state kept via the webview's own getState/setState so it survives re-renders within one panel
 * session). The one aid that DOES need the CLI is the style switcher -- selecting a style posts a
 * `styleChanged` message back to the extension (see setMessageHandler), which is expected to
 * re-render with that style and call showPreview again.
 */
export class PreviewPanelManager {
    private readonly panels = new Map<string, vscode.WebviewPanel>();
    private messageHandler: ((sourceFile: string, message: PreviewWebviewMessage) => void) | undefined;

    /** Registers the callback invoked when any preview panel posts a message (e.g. a style
     * switch). One handler for all panels -- kept separate from the constructor so extension.ts
     * doesn't need to wire this up before DiagnosticsManager (the natural owner) exists yet. */
    setMessageHandler(handler: (sourceFile: string, message: PreviewWebviewMessage) => void): void {
        this.messageHandler = handler;
    }

    async showPreview(
        sourceFile: string,
        svgFilePath: string,
        availableStyles: string[],
        selectedStyle: string
    ): Promise<void> {
        const panel = this.getOrCreatePanel(sourceFile);
        const svgContent = await fs.readFile(svgFilePath, 'utf8');
        panel.webview.html = this.renderHtml(svgContent, availableStyles, selectedStyle);
    }

    showError(sourceFile: string, message: string): void {
        const panel = this.panels.get(sourceFile);
        panel?.webview.postMessage({ type: 'error', message });
    }

    reveal(sourceFile: string): vscode.WebviewPanel {
        const panel = this.getOrCreatePanel(sourceFile);
        panel.reveal(vscode.ViewColumn.Beside, true);
        return panel;
    }

    hasPanel(sourceFile: string): boolean {
        return this.panels.has(sourceFile);
    }

    dispose(): void {
        for (const panel of this.panels.values()) {
            panel.dispose();
        }
        this.panels.clear();
    }

    private getOrCreatePanel(sourceFile: string): vscode.WebviewPanel {
        const existing = this.panels.get(sourceFile);
        if (existing) {
            return existing;
        }

        const panel = vscode.window.createWebviewPanel(
            'sketchpenPreview',
            `Preview: ${path.basename(sourceFile)}`,
            { viewColumn: vscode.ViewColumn.Beside, preserveFocus: true },
            { enableScripts: true, retainContextWhenHidden: true }
        );

        panel.webview.onDidReceiveMessage((message: PreviewWebviewMessage) => {
            this.messageHandler?.(sourceFile, message);
        });

        panel.onDidDispose(() => this.panels.delete(sourceFile));
        this.panels.set(sourceFile, panel);
        return panel;
    }

    private renderHtml(svgContent: string, availableStyles: string[], selectedStyle: string): string {
        const sized = ensureViewBox(svgContent);
        const dimensionMatch = sized.match(SVG_ROOT_SIZE);
        const width = dimensionMatch ? parseFloat(dimensionMatch[1]) : 100;
        const height = dimensionMatch ? parseFloat(dimensionMatch[2]) : 100;
        const gridOverlay = buildGridOverlay(width, height);

        const styleOptions = ['', ...availableStyles]
            .map((name) => {
                const label = name === '' ? 'Default' : name;
                const selected = name === selectedStyle ? ' selected' : '';
                return `<option value="${name}"${selected}>${label}</option>`;
            })
            .join('');

        return `<!DOCTYPE html>
<html>
<head>
<meta charset="UTF-8">
<style>
  html, body {
    height: 100%;
    margin: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--vscode-editor-background);
    overflow: hidden;
  }
  #toolbar {
    position: fixed;
    top: 0; left: 0; right: 0;
    display: flex;
    gap: 16px;
    align-items: center;
    padding: 6px 10px;
    background: var(--vscode-editorWidget-background);
    border-bottom: 1px solid var(--vscode-widget-border, transparent);
    font-family: var(--vscode-font-family);
    font-size: 12px;
    color: var(--vscode-foreground);
    z-index: 2;
  }
  #toolbar label {
    display: flex;
    align-items: center;
    gap: 4px;
  }
  #toolbar select {
    background: var(--vscode-dropdown-background);
    color: var(--vscode-dropdown-foreground);
    border: 1px solid var(--vscode-dropdown-border, transparent);
  }
  #error-banner {
    position: fixed;
    top: 32px; left: 0; right: 0;
    padding: 6px 10px;
    background: #5a1d1d;
    color: #fff;
    font-family: var(--vscode-font-family);
    font-size: 12px;
    display: none;
    white-space: pre-wrap;
    z-index: 2;
  }
  #preview-container {
    position: relative;
    width: min(88vw, 88vh);
    height: min(88vw, 88vh);
  }
  #preview-container > svg {
    position: absolute;
    top: 0; left: 0;
    width: 100%;
    height: 100%;
    display: block;
  }
  /* Qualified with the "#preview-container > svg" prefix (matching the generic icon-sizing rule
     above) so this wins the specificity comparison -- "#preview-container > svg" alone (one ID +
     one type selector) would otherwise always beat a bare "#grid-overlay" (one ID), leaving the
     overlay permanently visible regardless of the "visible" class. */
  #preview-container > svg#grid-overlay {
    pointer-events: none;
    display: none;
  }
  #preview-container > svg#grid-overlay.visible {
    display: block;
  }
  .grid-line {
    stroke: var(--vscode-editorLineNumber-foreground, #888);
    stroke-width: 0.3;
    opacity: 0.35;
  }
  .grid-axis {
    stroke: var(--vscode-editorLineNumber-activeForeground, #c0c0c0);
    stroke-width: 0.5;
    opacity: 0.6;
  }
  .grid-label {
    /* font-size set per-element (proportional to the icon's own size, see buildGridOverlay) --
       not fixed here. */
    fill: var(--vscode-editorLineNumber-foreground, #888);
    font-family: var(--vscode-font-family);
    opacity: 0.7;
  }
</style>
</head>
<body>
<div id="toolbar">
  <label><input type="checkbox" id="grid-toggle"> Grid</label>
  <label>Style: <select id="style-select">${styleOptions}</select></label>
</div>
<div id="error-banner"></div>
<div id="preview-container">
${sized}
${gridOverlay}
</div>
<script>
  const vscode = acquireVsCodeApi();

  const banner = document.getElementById('error-banner');
  window.addEventListener('message', (event) => {
    const data = event.data;
    if (data && data.type === 'error') {
      banner.textContent = data.message;
      banner.style.display = 'block';
    }
  });

  const gridToggle = document.getElementById('grid-toggle');
  const gridOverlay = document.getElementById('grid-overlay');
  const state = vscode.getState() || { grid: false };
  gridToggle.checked = state.grid;
  gridOverlay.classList.toggle('visible', state.grid);
  gridToggle.addEventListener('change', () => {
    gridOverlay.classList.toggle('visible', gridToggle.checked);
    vscode.setState({ ...vscode.getState(), grid: gridToggle.checked });
  });

  const styleSelect = document.getElementById('style-select');
  styleSelect.addEventListener('change', () => {
    vscode.postMessage({ type: 'styleChanged', style: styleSelect.value });
  });
</script>
</body>
</html>`;
    }
}
