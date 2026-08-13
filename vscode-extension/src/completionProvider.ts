import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs/promises';
import { CompletionTable, LanguageInfoResult, getLanguageInfo } from './cliClient';

type FileTypeKey = 'code' | 'template' | 'globals';

function getFileTypeKey(fileName: string): FileTypeKey | null {
    if (fileName.endsWith('.sp')) {
        return 'code';
    }
    if (fileName.endsWith('.spt')) {
        return 'template';
    }
    if (fileName.endsWith('.globals')) {
        return 'globals';
    }
    return null;
}

const KEYWORD_BEFORE_DOT = /([a-zA-Z_][a-zA-Z0-9_]*)\.\s*$/;

// Matches globals.set(name, value) / globals.tryset(name, value) declarations, e.g.
// `globals.tryset(penColor, "#000");`. Used only to show a best-effort declared value on hover
// (hoverProvider.ts) -- not a substitute for actually compiling default.globals, which is what
// `sketchpen language-info` does for the variable *names*.
const GLOBALS_DECLARATION = /globals\.(?:set|tryset)\s*\(\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*,\s*([^)]+?)\s*\)/g;

const SKETCHPEN_CONFIG_FILE = '.sketchpen.json';
const DEFAULT_STYLES_SUBFOLDER = 'styles';

/**
 * Light TypeScript-side mirror of SketchPen.Plot.Compile.StylesFolderResolver (see
 * src/SketchPen.Plot/Compile/StylesFolderResolver.cs): resolves the folder a `.sp` folder's
 * styles/globals live in. Checks `<folder>/.sketchpen.json` for a "stylesPath" key (resolved
 * relative to `folder`); falls back to `<folder>/styles` if the config file is absent, unreadable,
 * or missing that key. Never throws -- a missing/malformed config is just treated as "use the
 * default", same as the CLI-side resolver.
 */
async function resolveStylesFolder(folder: string): Promise<string> {
    try {
        const configText = await fs.readFile(path.join(folder, SKETCHPEN_CONFIG_FILE), 'utf8');
        const config = JSON.parse(configText) as { stylesPath?: string };
        if (config.stylesPath && config.stylesPath.trim().length > 0) {
            return path.resolve(folder, config.stylesPath);
        }
    } catch {
        // No/unreadable/malformed .sketchpen.json -- fall through to the default below.
    }
    return path.join(folder, DEFAULT_STYLES_SUBFOLDER);
}

/**
 * Completion for .sp/.spt/.globals files. Mirrors the existing Monaco provider
 * (src/SketchPen.Code/wwwroot/js/language/sketchpen.js) that already ships in the web app: the
 * same three cases (bare keyword / keyword.method / @@variable), the same textual (non-AST)
 * approach -- good enough given the language's shape. Static grammar and per-folder
 * @@variable names both come from `sketchpen language-info`, never reimplemented here.
 */
export class SketchPenCompletionProvider implements vscode.CompletionItemProvider {
    private commands: LanguageInfoResult['commands'] | null = null;
    private readonly globalVariablesByFolder = new Map<string, string[]>();
    private readonly globalValuesByFolder = new Map<string, Map<string, string>>();

    /** Fetches the static completion grammar once. Call on activation. */
    async initialize(): Promise<void> {
        const info = await getLanguageInfo();
        if (info.success) {
            this.commands = info.commands;
        }
    }

    /**
     * Fetches/refreshes the @@variable names for a folder's default.globals. Call once when a
     * document in that folder is first opened, and again whenever a .globals file in that
     * folder is saved (see diagnosticsManager.ts) -- never on every keystroke.
     */
    async refreshGlobalVariables(folder: string): Promise<void> {
        const info = await getLanguageInfo(folder);
        if (info.success) {
            this.globalVariablesByFolder.set(folder, info.globalVariables ?? []);
        }
        this.globalValuesByFolder.set(folder, await this.readDeclaredValues(folder));
    }

    /**
     * Best-effort declared value for `@@name` (used by hoverProvider.ts), read directly from the
     * folder's base `default.globals` (resolved via resolveStylesFolder, the same
     * .sketchpen.json/"stylesPath" convention the CLI uses) -- a textual regex match, not a
     * compile. Reflects the base declaration only; a `<style>.globals` override for the currently
     * selected style isn't accounted for, since preview always renders with the default style.
     */
    getGlobalValue(folder: string, name: string): string | undefined {
        return this.globalValuesByFolder.get(folder)?.get(name);
    }

    private async readDeclaredValues(folder: string): Promise<Map<string, string>> {
        const values = new Map<string, string>();
        try {
            const stylesFolder = await resolveStylesFolder(folder);
            const text = await fs.readFile(path.join(stylesFolder, 'default.globals'), 'utf8');
            for (const match of text.matchAll(GLOBALS_DECLARATION)) {
                // First declaration wins, matching tryset's "don't overwrite" semantics for the
                // common base-file case (set/set would normally only appear in style overrides,
                // which this best-effort reader doesn't consult).
                if (!values.has(match[1])) {
                    values.set(match[1], match[2].trim());
                }
            }
        } catch {
            // No default.globals for this folder -- leave the map empty.
        }
        return values;
    }

    provideCompletionItems(
        document: vscode.TextDocument,
        position: vscode.Position
    ): vscode.CompletionItem[] {
        const fileType = getFileTypeKey(document.fileName);
        if (!fileType || !this.commands) {
            return [];
        }

        const linePrefix = document.lineAt(position.line).text.slice(0, position.character);

        // Case 1: "...@" -> suggest @@variableName (one "@" already typed).
        if (linePrefix.endsWith('@')) {
            const folder = path.dirname(document.uri.fsPath);
            const globalVariables = this.globalVariablesByFolder.get(folder) ?? [];

            return globalVariables.map((name) => {
                const item = new vscode.CompletionItem(`@@${name}`, vscode.CompletionItemKind.Variable);
                item.insertText = `@${name}`;
                return item;
            });
        }

        // Case 2: "keyword." -> suggest that keyword's methods, as snippets.
        const dotMatch = linePrefix.match(KEYWORD_BEFORE_DOT);
        if (dotMatch) {
            const keyword = dotMatch[1];
            const table: CompletionTable = this.commands[fileType];
            const methods = table[keyword];
            if (!methods) {
                return [];
            }

            return methods.map((model) => {
                const item = new vscode.CompletionItem(model.suggestion, vscode.CompletionItemKind.Method);
                item.insertText = new vscode.SnippetString(model.snippet);
                item.detail = `${keyword}.${model.method}`;
                return item;
            });
        }

        // Case 3: bare position -> suggest keywords valid for this file type.
        const table: CompletionTable = this.commands[fileType];
        return Object.keys(table).map(
            (keyword) => new vscode.CompletionItem(keyword, vscode.CompletionItemKind.Keyword)
        );
    }
}
