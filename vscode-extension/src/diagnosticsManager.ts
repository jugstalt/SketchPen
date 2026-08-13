import * as vscode from 'vscode';
import * as crypto from 'crypto';
import * as path from 'path';
import * as fs from 'fs/promises';
import { compose, ComposeJsonResult } from './cliClient';
import { PreviewPanelManager } from './previewPanel';
import { SketchPenCompletionProvider } from './completionProvider';

// SyntaxErrorException.Statement is formatted "<lineNumber>: <tokens>" -- there is no
// column/character-offset anywhere in the compiler, so diagnostics are whole-line only. See
// docs/CLI.md#the-language-info-subcommand and the plan this was built from.
const STATEMENT_LINE = /^(\d+):/;

// Matches PreComplier.Compile's own recognition rule (a trimmed line starting with "#include ").
const INCLUDE_LINE = /^\s*#include\s+"([^"]+)"/gm;

const DEBOUNCE_MS = 300;

/**
 * Drives both live preview and error diagnostics from a single `compose --composer svg`
 * invocation per .sp save: on success it's a rendered preview, on failure it's a structured
 * error (including "unknown variable" errors, which only surface during an actual render pass,
 * not a parse-only check) -- no separate "check" command needed. .spt files can't be composed
 * standalone (see README); .globals saves instead refresh the @@variable completion cache.
 *
 * Also tracks #include edges (mirroring PreComplier.IncludeFile's own resolution rule: relative
 * to the including file's directory, falling back to as-given) so that saving a .spt re-renders
 * every .sp file that transitively includes it -- but only ones currently being previewed, to
 * avoid firing the CLI for icons nobody is looking at.
 */
export class DiagnosticsManager {
    private readonly diagnostics = vscode.languages.createDiagnosticCollection('sketchpen');
    private readonly debounceTimers = new Map<string, ReturnType<typeof setTimeout>>();
    private readonly refreshedFolders = new Set<string>();

    /** Absolute include-file path -> set of absolute .sp paths that transitively include it. */
    private readonly includedBy = new Map<string, Set<string>>();

    constructor(
        private readonly context: vscode.ExtensionContext,
        private readonly previewPanel: PreviewPanelManager,
        private readonly completionProvider: SketchPenCompletionProvider
    ) {
        context.subscriptions.push(this.diagnostics);
    }

    register(): void {
        this.context.subscriptions.push(
            vscode.workspace.onDidSaveTextDocument((doc) => this.handleSave(doc)),
            vscode.workspace.onDidOpenTextDocument((doc) => this.handleOpen(doc))
        );
    }

    private handleOpen(document: vscode.TextDocument): void {
        if (!this.isSketchPenFile(document.fileName)) {
            return;
        }

        const folder = path.dirname(document.uri.fsPath);
        if (!this.refreshedFolders.has(folder)) {
            this.refreshedFolders.add(folder);
            void this.completionProvider.refreshGlobalVariables(folder);
        }

        if (document.fileName.endsWith('.sp')) {
            void this.trackIncludes(document);
        }
    }

    private handleSave(document: vscode.TextDocument): void {
        if (!this.isSketchPenFile(document.fileName)) {
            return;
        }

        if (document.fileName.endsWith('.globals')) {
            // A saved .globals file's own directory is its *styles folder*, not necessarily the
            // icon-set folder(s) that use it -- styles can live anywhere (see
            // .sketchpen.json/"stylesPath" in completionProvider.ts's resolveStylesFolder) and
            // one styles folder can be shared by several icon sets. Rather than reverse-resolving
            // which icon-set folders reference this particular styles folder, just refresh every
            // folder we already know about (populated by handleOpen as documents are opened) --
            // correctness over precision, and each refresh is one cheap `language-info` call.
            for (const folder of this.refreshedFolders) {
                void this.completionProvider.refreshGlobalVariables(folder);
            }
            return;
        }

        if (document.fileName.endsWith('.spt')) {
            void this.handleTemplateSave(document);
            return;
        }

        // .sp
        this.scheduleRender(document);
    }

    /** Re-renders every currently-previewed .sp file that transitively includes the saved .spt. */
    private async handleTemplateSave(document: vscode.TextDocument): Promise<void> {
        const dependents = this.includedBy.get(document.uri.fsPath);
        if (!dependents || dependents.size === 0) {
            return;
        }

        for (const spPath of [...dependents]) {
            if (!this.previewPanel.hasPanel(spPath)) {
                continue; // Only re-render icons the user is actively previewing.
            }
            try {
                const spDocument = await vscode.workspace.openTextDocument(spPath);
                this.scheduleRender(spDocument);
            } catch {
                // The .sp file no longer exists -- drop the stale edge.
                dependents.delete(spPath);
            }
        }
    }

    private scheduleRender(document: vscode.TextDocument): void {
        const key = document.uri.fsPath;
        const existingTimer = this.debounceTimers.get(key);
        if (existingTimer) {
            clearTimeout(existingTimer);
        }

        this.debounceTimers.set(
            key,
            setTimeout(() => {
                this.debounceTimers.delete(key);
                void this.renderAndDiagnose(document);
            }, DEBOUNCE_MS)
        );
    }

    /** Also used by the manual "Preview" command and Explorer/CodeLens entry points, not just on save. */
    async renderAndDiagnose(document: vscode.TextDocument): Promise<void> {
        void this.trackIncludes(document);

        const outPath = await this.getPreviewOutputPath(document.uri.fsPath);
        const previewSize = vscode.workspace.getConfiguration('sketchpen').get<number>('previewSize', 128);

        let result: ComposeJsonResult;
        try {
            result = await compose({
                path: document.uri.fsPath,
                composer: 'svg',
                sizes: String(previewSize),
                out: outPath
            });
        } catch {
            // CLI not found -- already surfaced once via the activation-time prompt; don't spam.
            return;
        }

        if (result.success) {
            this.diagnostics.delete(document.uri);
            await this.previewPanel.showPreview(document.uri.fsPath, outPath);
            return;
        }

        const error = result.error;
        if (!error) {
            return;
        }

        const targetUri = error.codeFile ? vscode.Uri.file(error.codeFile) : document.uri;
        const lineMatch = error.statement ? error.statement.match(STATEMENT_LINE) : null;
        const line = lineMatch ? Math.max(0, parseInt(lineMatch[1], 10) - 1) : 0;

        const range = new vscode.Range(line, 0, line, Number.MAX_SAFE_INTEGER);
        const diagnostic = new vscode.Diagnostic(range, error.message, vscode.DiagnosticSeverity.Error);
        diagnostic.source = 'sketchpen';

        this.diagnostics.set(targetUri, [diagnostic]);
        // Deliberately don't touch the preview panel's rendered content here -- leave the last
        // good render showing rather than blanking it while the user fixes a typo.
        this.previewPanel.showError(document.uri.fsPath, error.message);
    }

    /**
     * Rebuilds the #include edges rooted at this .sp file: parses its text (and, recursively,
     * every file it includes, read from disk) for `#include "..."` lines, resolving each the
     * same way PreComplier.IncludeFile does -- relative to the including file's own directory
     * first, then falling back to the path as given. Registers a reverse edge from every
     * (transitively) included file back to this .sp file, so handleTemplateSave can find it.
     */
    private async trackIncludes(document: vscode.TextDocument): Promise<void> {
        if (!document.fileName.endsWith('.sp')) {
            return;
        }

        const spPath = document.uri.fsPath;
        for (const dependents of this.includedBy.values()) {
            dependents.delete(spPath);
        }

        await this.collectIncludes(document.getText(), path.dirname(spPath), spPath, new Set());
    }

    private async collectIncludes(
        text: string,
        baseDir: string,
        rootSpPath: string,
        visited: Set<string>
    ): Promise<void> {
        for (const match of text.matchAll(INCLUDE_LINE)) {
            const includePath = await this.resolveIncludePath(match[1], baseDir);
            if (!includePath || visited.has(includePath)) {
                continue;
            }
            visited.add(includePath);

            const dependents = this.includedBy.get(includePath) ?? new Set<string>();
            dependents.add(rootSpPath);
            this.includedBy.set(includePath, dependents);

            try {
                const includedText = await fs.readFile(includePath, 'utf8');
                await this.collectIncludes(includedText, path.dirname(includePath), rootSpPath, visited);
            } catch {
                // Missing include file -- already surfaced as a compile error via renderAndDiagnose.
            }
        }
    }

    private async resolveIncludePath(includeFile: string, baseDir: string): Promise<string | null> {
        const relativeToIncluder = path.isAbsolute(includeFile) ? includeFile : path.join(baseDir, includeFile);
        if (await this.fileExists(relativeToIncluder)) {
            return relativeToIncluder;
        }
        if (await this.fileExists(includeFile)) {
            return path.resolve(includeFile);
        }
        return null;
    }

    private async fileExists(filePath: string): Promise<boolean> {
        try {
            await fs.access(filePath);
            return true;
        } catch {
            return false;
        }
    }

    private isSketchPenFile(fileName: string): boolean {
        return fileName.endsWith('.sp') || fileName.endsWith('.spt') || fileName.endsWith('.globals');
    }

    private async getPreviewOutputPath(sourceFile: string): Promise<string> {
        const dir = this.context.globalStorageUri.fsPath;
        await fs.mkdir(dir, { recursive: true });

        // Stable per-source-file name so repeated saves overwrite the same temp file instead of
        // accumulating garbage.
        const hash = crypto.createHash('md5').update(sourceFile).digest('hex');
        return path.join(dir, `${hash}.svg`);
    }
}
