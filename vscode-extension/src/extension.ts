import * as vscode from 'vscode';
import * as path from 'path';
import { ensureCliAvailable, installCli } from './cliClient';
import { SketchPenCompletionProvider } from './completionProvider';
import { SketchPenHoverProvider } from './hoverProvider';
import { SketchPenCodeLensProvider } from './codeLensProvider';
import { PreviewPanelManager } from './previewPanel';
import { DiagnosticsManager } from './diagnosticsManager';
import { registerExportCommands } from './exportCommands';
import { registerInitCommand } from './initCommand';
import { SetPreviewPanelManager } from './setPreviewPanel';
import { registerSetPreviewCommand } from './setPreviewCommand';

const SKETCHPEN_SELECTOR: vscode.DocumentSelector = { language: 'sketchpen' };

export async function activate(context: vscode.ExtensionContext): Promise<void> {
    const cliAvailable = await ensureCliAvailable();

    const completionProvider = new SketchPenCompletionProvider();
    const previewPanel = new PreviewPanelManager();
    const diagnosticsManager = new DiagnosticsManager(context, previewPanel, completionProvider);
    const setPreviewPanel = new SetPreviewPanelManager();

    context.subscriptions.push(
        vscode.languages.registerCompletionItemProvider(SKETCHPEN_SELECTOR, completionProvider, '.', '@'),
        vscode.languages.registerHoverProvider(SKETCHPEN_SELECTOR, new SketchPenHoverProvider(completionProvider)),
        vscode.languages.registerCodeLensProvider(SKETCHPEN_SELECTOR, new SketchPenCodeLensProvider()),
        vscode.commands.registerCommand('sketchpen.installCli', () => installCli()),
        { dispose: () => previewPanel.dispose() },
        { dispose: () => setPreviewPanel.dispose() }
    );

    registerExportCommands(context, previewPanel, diagnosticsManager);
    registerInitCommand(context); // pure file scaffolding -- works without the CLI installed
    registerSetPreviewCommand(context, setPreviewPanel);
    diagnosticsManager.register();

    if (cliAvailable) {
        await completionProvider.initialize();

        // Prime the @@variable cache for any .sp/.spt/.globals files already open when the
        // extension activates (normally this only happens on first-open, via
        // DiagnosticsManager's onDidOpenTextDocument listener, which doesn't fire retroactively).
        for (const document of vscode.workspace.textDocuments) {
            if (
                document.fileName.endsWith('.sp') ||
                document.fileName.endsWith('.spt') ||
                document.fileName.endsWith('.globals')
            ) {
                void completionProvider.refreshGlobalVariables(path.dirname(document.uri.fsPath));
            }
        }
    }
}

export function deactivate(): void {
    // Nothing to clean up beyond what's already registered in context.subscriptions.
}
