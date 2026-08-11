import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs/promises';

// Verbatim from plot/basic/_.globals in the main repo -- a proven, working default rather than
// an invented one.
const DEFAULT_GLOBALS = `// general globals
globals.tryset(penColor, "#000");
globals.tryset(outlinePenColor, "#000");
globals.tryset(brushColor, "#fff");
globals.tryset(gradientbrushColor, "#ddd");
globals.tryset(backgroundBrushColor, "#fefefe");
globals.tryset(penWidth, 6);
globals.tryset(penWidthBold, 9);
globals.set(minPenWidth, 1);
globals.set(maxPenWidth, 999);
globals.tryset(penCap, "round");

// Pens & Brushes
pen.color(@@penColor);
pen.width(@@penWidth);
pen.maxwidth(@@maxPenWidth);
pen.minwidth(@@minPenWidth);
pen.cap(@@penCap);
brush.color(@@brushColor);
gradientbrush.color(@@gradientbrushColor);
`;

const STARTER_ICON = `// icon.sp
circle.fill(60);
circle.draw(60);
`;

const EXAMPLE_TEMPLATE = `// example.spt -- a small reusable shape, included via #include "templates/example.spt"
line.draw(-20,-20, 20,20);
line.draw(-20,20, 20,-20);
`;

async function fileExists(filePath: string): Promise<boolean> {
    try {
        await fs.access(filePath);
        return true;
    } catch {
        return false;
    }
}

/** Writes `content` to `filePath` only if it doesn't already exist. Returns whether it created it. */
async function createIfMissing(filePath: string, content: string): Promise<boolean> {
    if (await fileExists(filePath)) {
        return false;
    }
    await fs.mkdir(path.dirname(filePath), { recursive: true });
    await fs.writeFile(filePath, content, 'utf8');
    return true;
}

/**
 * `SketchPen: Init` -- scaffolds everything a new icon-set folder needs to be immediately
 * usable: _.globals (always auto-included, see docs/SYNTAX.md), a templates/ folder with one
 * example .spt (demonstrating #include), and a starter .sp that already renders something.
 * Never overwrites existing files -- each of the three is created independently and only if
 * missing, so re-running Init on a partially-set-up folder just fills in the gaps.
 */
export function registerInitCommand(context: vscode.ExtensionContext): void {
    context.subscriptions.push(
        vscode.commands.registerCommand('sketchpen.init', async () => {
            const workspaceFolder = vscode.workspace.workspaceFolders?.[0]?.uri;

            const picked = await vscode.window.showOpenDialog({
                canSelectFiles: false,
                canSelectFolders: true,
                canSelectMany: false,
                defaultUri: workspaceFolder,
                openLabel: 'Initialize here'
            });
            if (!picked || picked.length === 0) {
                return;
            }

            const targetDir = picked[0].fsPath;
            const created: string[] = [];
            const skipped: string[] = [];

            const record = (label: string, wasCreated: boolean) => (wasCreated ? created : skipped).push(label);

            record('_.globals', await createIfMissing(path.join(targetDir, '_.globals'), DEFAULT_GLOBALS));
            record(
                'templates/example.spt',
                await createIfMissing(path.join(targetDir, 'templates', 'example.spt'), EXAMPLE_TEMPLATE)
            );

            const iconPath = path.join(targetDir, 'icon.sp');
            record('icon.sp', await createIfMissing(iconPath, STARTER_ICON));

            if (created.length > 0) {
                const skippedNote = skipped.length ? ` (already existed: ${skipped.join(', ')})` : '';
                void vscode.window.showInformationMessage(`SketchPen: created ${created.join(', ')}${skippedNote}.`);
            } else {
                void vscode.window.showInformationMessage('SketchPen: nothing to do, all files already exist.');
            }

            const document = await vscode.workspace.openTextDocument(iconPath);
            await vscode.window.showTextDocument(document);
        })
    );
}
