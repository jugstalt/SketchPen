import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs/promises';

/**
 * One Webview panel per previewed .sp source file, showing the last successfully rendered SVG
 * inline (embedded directly in the page, not via <img src> -- avoids file-URI caching issues
 * entirely for content that gets rewritten on every save). On a compose failure, the panel is
 * deliberately left showing the last good render (a small error banner is posted on top instead
 * of blanking the panel) -- see diagnosticsManager.ts.
 */
export class PreviewPanelManager {
    private readonly panels = new Map<string, vscode.WebviewPanel>();

    async showPreview(sourceFile: string, svgFilePath: string): Promise<void> {
        const panel = this.getOrCreatePanel(sourceFile);
        const svgContent = await fs.readFile(svgFilePath, 'utf8');
        panel.webview.html = this.renderHtml(svgContent);
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

        panel.onDidDispose(() => this.panels.delete(sourceFile));
        this.panels.set(sourceFile, panel);
        return panel;
    }

    private renderHtml(svgContent: string): string {
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
  }
  #error-banner {
    position: fixed;
    top: 0; left: 0; right: 0;
    padding: 6px 10px;
    background: #5a1d1d;
    color: #fff;
    font-family: var(--vscode-font-family);
    font-size: 12px;
    display: none;
    white-space: pre-wrap;
  }
  svg {
    max-width: 80vw;
    max-height: 80vh;
  }
</style>
</head>
<body>
<div id="error-banner"></div>
${svgContent}
<script>
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
