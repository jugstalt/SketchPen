import * as vscode from 'vscode';
import * as path from 'path';
import { compose, ComposeJsonResult } from './cliClient';
import { PreviewPanelManager } from './previewPanel';
import { DiagnosticsManager } from './diagnosticsManager';

interface ComposerChoice {
    id: string;
    label: string;
    description: string;
    /** Single-icon composers require exactly one .sp file; batch composers take a whole folder. */
    batch: boolean;
    extension: string;
}

// Matches docs/CLI.md's composer table exactly.
const COMPOSERS: ComposerChoice[] = [
    { id: 'png', label: 'Image', description: 'A single PNG (png)', batch: false, extension: 'png' },
    { id: 'png-zip', label: 'Images', description: 'ZIP, split by style/size/resolution (png-zip)', batch: true, extension: 'zip' },
    { id: 'web-sprite-zip', label: 'Web Sprites', description: 'CSS sprite sheet + demo HTML (web-sprite-zip)', batch: true, extension: 'zip' },
    { id: 'svg', label: 'Vector Image', description: 'A single SVG (svg)', batch: false, extension: 'svg' },
    { id: 'svg-zip', label: 'Images (SVG)', description: 'ZIP of individual SVGs (svg-zip)', batch: true, extension: 'zip' },
    { id: 'svg-vars-zip', label: 'Images (CSS Variables)', description: 'ZIP of themeable HTML pages (svg-vars-zip)', batch: true, extension: 'zip' }
];

function reportResult(result: ComposeJsonResult): void {
    if (result.success) {
        void vscode.window.showInformationMessage(`SketchPen: wrote ${result.filesWritten.join(', ')}`);
    } else {
        const message = result.error?.message ?? result.message ?? 'Unknown error';
        void vscode.window.showErrorMessage(`SketchPen: ${message}`);
    }
}

/**
 * Resolves the target .sp file path for a command invocation. `uri` is set when the command was
 * triggered from the Explorer context menu (or a CodeLens) -- VS Code passes the clicked
 * resource as the first argument in that case. Falls back to the active editor for Command
 * Palette invocations, where no argument is passed.
 */
function resolveSpFilePath(uri?: vscode.Uri): string | undefined {
    if (uri?.fsPath.endsWith('.sp')) {
        return uri.fsPath;
    }

    const document = vscode.window.activeTextEditor?.document;
    if (document?.fileName.endsWith('.sp')) {
        return document.fileName;
    }

    void vscode.window.showWarningMessage('SketchPen: open a .sp file first.');
    return undefined;
}

/** Parses a comma-separated list of positive integer ratios, e.g. "1, 2,3" -> [1, 2, 3].
 * Returns null for anything that doesn't match (caller re-prompts via validateInput). */
function parseRatios(input: string): number[] | null {
    if (!/^\s*\d+\s*(,\s*\d+\s*)*$/.test(input)) {
        return null;
    }
    const ratios = [...new Set(input.split(',').map((r) => parseInt(r.trim(), 10)))].filter((r) => r > 0);
    return ratios.length > 0 ? ratios.sort((a, b) => a - b) : null;
}

async function exportSingle(composerId: 'svg' | 'png', uri?: vscode.Uri): Promise<void> {
    const spFilePath = resolveSpFilePath(uri);
    if (!spFilePath) {
        return;
    }

    const defaultSize = vscode.workspace.getConfiguration('sketchpen').get<number>('previewSize', 128);
    const size = await vscode.window.showInputBox({
        prompt: 'Size',
        value: String(defaultSize),
        validateInput: (v) => (/^\d+$/.test(v) ? undefined : 'Enter a whole number')
    });
    if (!size) {
        return;
    }

    const base = path.basename(spFilePath, '.sp');

    // Resolutions only make sense for raster output -- SVG is resolution-independent (see
    // docs/CLI.md#vector-svg-output), so the ratio prompt is PNG-only.
    let ratios = [1];
    if (composerId === 'png') {
        const ratiosInput = await vscode.window.showInputBox({
            prompt:
                'Resolutions to export, as pixel-ratio multipliers of Size (comma-separated, e.g. 1,2,3 for @1x/@2x/@3x)',
            value: '1',
            validateInput: (v) => (parseRatios(v) ? undefined : 'Enter one or more whole numbers, comma-separated (e.g. 1,2,3)')
        });
        if (!ratiosInput) {
            return;
        }
        ratios = parseRatios(ratiosInput) ?? [1];
    }

    if (ratios.length === 1 && ratios[0] === 1) {
        // Single file -- same save-dialog UX as before.
        const defaultUri = vscode.Uri.file(path.join(path.dirname(spFilePath), `${base}.${composerId}`));
        const outUri = await vscode.window.showSaveDialog({ defaultUri });
        if (!outUri) {
            return;
        }

        const result = await compose({ path: spFilePath, composer: composerId, sizes: size, out: outUri.fsPath });
        reportResult(result);
        return;
    }

    // Multiple resolutions -- pick a target folder and write one PNG per ratio, named exactly
    // like the root command's own fixed matrix (<name>_<size>@<ratio>.png, see docs/CLI.md#output-naming-and-location).
    const picked = await vscode.window.showOpenDialog({
        canSelectFiles: false,
        canSelectFolders: true,
        canSelectMany: false,
        defaultUri: vscode.Uri.file(path.dirname(spFilePath)),
        openLabel: 'Export here'
    });
    if (!picked || picked.length === 0) {
        return;
    }
    const targetFolder = picked[0].fsPath;

    const writtenFiles: string[] = [];
    for (const ratio of ratios) {
        const outPath = path.join(targetFolder, `${base}_${size}@${ratio}.png`);
        const result = await compose({
            path: spFilePath,
            composer: 'png',
            sizes: String(Number(size) * ratio),
            out: outPath
        });
        if (!result.success) {
            reportResult(result);
            return;
        }
        writtenFiles.push(...result.filesWritten);
    }
    void vscode.window.showInformationMessage(`SketchPen: wrote ${writtenFiles.join(', ')}`);
}

async function exportPackage(uri?: vscode.Uri): Promise<void> {
    const spFilePath = resolveSpFilePath(uri);
    if (!spFilePath) {
        return;
    }

    const picked = await vscode.window.showQuickPick(
        COMPOSERS.map((c) => ({ label: c.label, description: c.description, composer: c })),
        { placeHolder: 'Choose a composer' }
    );
    if (!picked) {
        return;
    }

    const composer = picked.composer;
    const targetPath = composer.batch ? path.dirname(spFilePath) : spFilePath;

    const sizes = await vscode.window.showInputBox({
        prompt: 'Sizes (comma-separated, e.g. 16,32,64) — leave empty for the composer default',
        placeHolder: '16,32,64'
    });
    if (sizes === undefined) {
        return;
    }

    const styles = await vscode.window.showInputBox({
        prompt: "Styles (comma-separated, empty entry = default style, e.g. ',bg-dark') — leave empty for default only",
        placeHolder: ',bg-dark'
    });
    if (styles === undefined) {
        return;
    }

    // --resolutions is DPI-based and only meaningful for the two composers that bake a
    // size/resolution matrix into their ZIP (docs/CLI.md's compose table) -- e.g. 96,144,192
    // produces @1x/@1.5x/@2x-equivalent PNGs per size/style.
    let resolutions: string | undefined;
    if (composer.id === 'png-zip' || composer.id === 'web-sprite-zip') {
        resolutions = await vscode.window.showInputBox({
            prompt: 'Resolutions in DPI (comma-separated, e.g. 96,144,192 for @1x/@1.5x/@2x) — leave empty for @1x (96) only',
            placeHolder: '96,144,192'
        });
        if (resolutions === undefined) {
            return;
        }
    }

    const base = path.basename(targetPath, path.extname(targetPath));
    const defaultUri = vscode.Uri.file(path.join(path.dirname(spFilePath), `${base}.${composer.extension}`));
    const outUri = await vscode.window.showSaveDialog({ defaultUri });
    if (!outUri) {
        return;
    }

    const result = await compose({
        path: targetPath,
        composer: composer.id,
        sizes: sizes || undefined,
        styles: styles || undefined,
        resolutions: resolutions || undefined,
        out: outUri.fsPath
    });
    reportResult(result);
}

export function registerExportCommands(
    context: vscode.ExtensionContext,
    previewPanel: PreviewPanelManager,
    diagnosticsManager: DiagnosticsManager
): void {
    context.subscriptions.push(
        vscode.commands.registerCommand('sketchpen.preview', async (uri?: vscode.Uri) => {
            const spFilePath = resolveSpFilePath(uri);
            if (!spFilePath) {
                return;
            }
            const document = await vscode.workspace.openTextDocument(spFilePath);
            previewPanel.reveal(spFilePath);
            await diagnosticsManager.renderAndDiagnose(document);
        }),
        vscode.commands.registerCommand('sketchpen.exportSvg', (uri?: vscode.Uri) => exportSingle('svg', uri)),
        vscode.commands.registerCommand('sketchpen.exportPng', (uri?: vscode.Uri) => exportSingle('png', uri)),
        vscode.commands.registerCommand('sketchpen.exportPackage', (uri?: vscode.Uri) => exportPackage(uri))
    );
}
