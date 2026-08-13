import * as vscode from 'vscode';
import * as path from 'path';
import { SketchPenCompletionProvider } from './completionProvider';

const VARIABLE_WORD = /@@[a-zA-Z_][a-zA-Z0-9_]*/;

/**
 * Hovering `@@name` shows the value declared for it in the folder's styles folder
 * `default.globals` (best-effort text match, see completionProvider.ts's readDeclaredValues --
 * not a compiled/resolved value).
 */
export class SketchPenHoverProvider implements vscode.HoverProvider {
    constructor(private readonly completionProvider: SketchPenCompletionProvider) {}

    provideHover(document: vscode.TextDocument, position: vscode.Position): vscode.Hover | undefined {
        const range = document.getWordRangeAtPosition(position, VARIABLE_WORD);
        if (!range) {
            return undefined;
        }

        const name = document.getText(range).slice(2); // strip leading "@@"
        const folder = path.dirname(document.uri.fsPath);
        const value = this.completionProvider.getGlobalValue(folder, name);
        if (value === undefined) {
            return undefined;
        }

        const markdown = new vscode.MarkdownString();
        markdown.appendCodeblock(`@@${name} = ${value}`, 'sketchpen');
        markdown.appendMarkdown('Declared in this folder’s styles/`default.globals`.');
        return new vscode.Hover(markdown, range);
    }
}
