import * as vscode from 'vscode';

/** Inline "Preview | Export SVG | Export PNG" links above every .sp file, mirroring the
 * editor-title icon and Explorer context menu -- just another entry point to the same commands. */
export class SketchPenCodeLensProvider implements vscode.CodeLensProvider {
    provideCodeLenses(document: vscode.TextDocument): vscode.CodeLens[] {
        if (!document.fileName.endsWith('.sp')) {
            return [];
        }

        const range = new vscode.Range(0, 0, 0, 0);
        return [
            new vscode.CodeLens(range, {
                title: '$(open-preview) Preview',
                command: 'sketchpen.preview',
                arguments: [document.uri]
            }),
            new vscode.CodeLens(range, {
                title: 'Export SVG',
                command: 'sketchpen.exportSvg',
                arguments: [document.uri]
            }),
            new vscode.CodeLens(range, {
                title: 'Export PNG',
                command: 'sketchpen.exportPng',
                arguments: [document.uri]
            })
        ];
    }
}
