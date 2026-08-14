import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs/promises';
import * as crypto from 'crypto';
import { render, ComposeJsonResult } from './cliClient';
import { SetPreviewPanelManager, SetPreviewWebviewMessage, SetPreviewIcon } from './setPreviewPanel';
import { listAvailableStyles } from './stylesFolder';

/** Resolves the folder to preview: the right-clicked Explorer folder, the containing folder of a
 * right-clicked/currently-open .sp or .spt file (so the command also makes sense from the
 * Command Palette with an icon open), or a folder picker as a last resort. */
async function resolveFolder(uri?: vscode.Uri): Promise<string | undefined> {
    if (uri) {
        try {
            const stat = await fs.stat(uri.fsPath);
            return stat.isDirectory() ? uri.fsPath : path.dirname(uri.fsPath);
        } catch {
            // Fall through -- stale/inaccessible URI.
        }
    }

    const activeFile = vscode.window.activeTextEditor?.document.fileName;
    if (activeFile && (activeFile.endsWith('.sp') || activeFile.endsWith('.spt'))) {
        return path.dirname(activeFile);
    }

    const picked = await vscode.window.showOpenDialog({
        canSelectFiles: false,
        canSelectFolders: true,
        canSelectMany: false,
        openLabel: 'Preview this icon set'
    });
    return picked?.[0]?.fsPath;
}

/** A stable, per-folder temp output directory (mirrors DiagnosticsManager.getPreviewOutputPath's
 * per-file hash convention), recreated fresh on every render so a renamed/deleted icon's old SVG
 * doesn't linger on disk -- harmless either way since only files from the *current* render's
 * result.filesWritten are ever read back, but cheap to keep tidy. */
async function getSetPreviewOutputDir(context: vscode.ExtensionContext, folder: string): Promise<string> {
    const hash = crypto.createHash('md5').update(folder).digest('hex');
    const dir = path.join(context.globalStorageUri.fsPath, 'set-preview', hash);
    await fs.rm(dir, { recursive: true, force: true });
    await fs.mkdir(dir, { recursive: true });
    return dir;
}

/**
 * `SketchPen: Preview Set` -- renders every `.sp` file directly inside a folder in one shot (via
 * the classic root command, see cliClient.render) and shows them all as a grid in a
 * SetPreviewPanelManager webview, for checking whole-set visual consistency at a glance. A
 * sibling to DiagnosticsManager's per-file live preview, but manual/on-demand (no save-triggered
 * re-render or #include dependency tracking -- a folder-wide render is too expensive to fire on
 * every keystroke/save the way the single-icon preview does) and with its own, folder-keyed style
 * selection instead of DiagnosticsManager's per-file one.
 */
export function registerSetPreviewCommand(context: vscode.ExtensionContext, setPreviewPanel: SetPreviewPanelManager): void {
    const selectedStyles = new Map<string, string>();

    async function renderAndShow(folder: string): Promise<void> {
        const outDir = await getSetPreviewOutputDir(context, folder);
        const selectedStyle = selectedStyles.get(folder) ?? '';

        let result: ComposeJsonResult;
        try {
            result = await render({
                path: folder,
                outFolder: outDir,
                style: selectedStyle || undefined,
                format: 'svg'
            });
        } catch {
            void vscode.window.showWarningMessage("SketchPen CLI not found -- can't preview this set.");
            return;
        }

        if (!result.success) {
            const message = result.error?.message ?? result.message ?? 'Unknown error';
            setPreviewPanel.showError(folder, message);
            void vscode.window.showErrorMessage(`SketchPen: ${message}`);
            return;
        }

        if (result.filesWritten.length === 0) {
            void vscode.window.showInformationMessage('SketchPen: no .sp files found directly in this folder.');
            return;
        }

        const icons: SetPreviewIcon[] = [];
        for (const filePath of [...result.filesWritten].sort((a, b) => a.localeCompare(b))) {
            try {
                const svg = await fs.readFile(filePath, 'utf8');
                icons.push({ name: path.basename(filePath, '.svg'), svg });
            } catch {
                // Unreadable file -- skip it rather than failing the whole preview.
            }
        }

        const availableStyles = await listAvailableStyles(folder);
        setPreviewPanel.showPreview(folder, icons, availableStyles, selectedStyle);
    }

    setPreviewPanel.setMessageHandler((folder, message: SetPreviewWebviewMessage) => {
        if (message.type !== 'styleChanged') {
            return;
        }
        selectedStyles.set(folder, message.style);
        void renderAndShow(folder);
    });

    context.subscriptions.push(
        vscode.commands.registerCommand('sketchpen.previewSet', async (uri?: vscode.Uri) => {
            const folder = await resolveFolder(uri);
            if (!folder) {
                return;
            }
            setPreviewPanel.reveal(folder);
            await renderAndShow(folder);
        })
    );
}
