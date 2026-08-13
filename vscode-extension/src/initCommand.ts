import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs/promises';

// Verbatim from plot/styles/default.globals in the main repo -- a proven, working default rather
// than an invented one.
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

// AGENTS.md -- a self-contained reference for AI coding agents (Claude Code, Copilot, Cursor,
// etc.) working in this folder, following the emerging cross-tool convention of the same name.
// Deliberately NOT scoped to the SketchPen repo itself (Init normally runs in someone else's
// project, which won't have docs/SYNTAX.md checked out locally) -- condenses the essentials plus,
// critically, a render-and-look workflow so an agent can actually verify its own output visually
// instead of guessing blind. See docs/SYNTAX.md and docs/CLI.md on GitHub for the full reference.
const AGENTS_MD = `# SketchPen icons -- agent instructions

This folder contains SketchPen icon scripts (\`.sp\`), reusable templates
(\`.spt\`, included via \`#include\`), and a \`styles/\` folder with \`.globals\`
color themes. SketchPen compiles these to PNG or SVG.

Full reference: <https://github.com/jugstalt/SketchPen/blob/main/docs/SYNTAX.md>
(language) and <https://github.com/jugstalt/SketchPen/blob/main/docs/CLI.md>
(CLI). This file is a condensed cheat sheet, not a replacement.

## Workflow: render, then look

**Always render and view your icon before considering it done -- do not judge
an icon purely by reading the code.**

\`\`\`bash
sketchpen compose <file>.sp --composer png --sizes 128 --out /tmp/preview.png
\`\`\`

Then open/view \`/tmp/preview.png\`. Iterate: edit the \`.sp\` file, re-render,
look again, until it actually looks right. A non-zero exit code plus stderr
means a compile error (wrong syntax, undefined \`@@variable\`, etc.) -- fix
that before re-rendering.

## Coordinate system

Every icon is drawn in a logical **-50..+50 square**, origin at the center,
X right, Y **down** (screen convention). Actual pixel size is applied later
(\`--sizes\`) -- write shapes in this -50..+50 space regardless of target size.

## Statement syntax

\`\`\`
keyword.method(param1, param2, ...);
\`\`\`

One statement per line, semicolon optional (auto-appended), spaces
irrelevant outside \`"..."\` strings. Colors: \`"#ff0000"\`, \`"#f00"\`,
\`"#ff0000", 128\` (alpha), or \`255, 0, 0[, 128]\` (RGB[A]).

**IMPORTANT: never put an apostrophe inside a \`//\` comment** (e.g. "don't",
"it's") -- a known lexer limitation crashes compilation on it. Rephrase
instead (e.g. "do not", "it is").

**IMPORTANT: for \`circle.*\`, prefer a hex-string color** (\`"#rrggbb"\`) over
numeric \`r,g,b(,a)\` when passing it inline after \`pos...\`. \`pos...\` is
itself 1-4 numbers, and a numeric color has no type boundary to stop at --
it silently gets swallowed into the position instead, unless \`pos\` already
uses its full 4 values. For a numeric color, call
\`brush.color(r,g,b[,a])\`/\`pen.color(r,g,b[,a])\` on its own line right
before the \`circle.*\` call instead.

## Commands (all take a logical -50..+50 position unless noted)

- \`pen.color(c)\` / \`.width(w)\` / \`.minwidth(w)\` / \`.maxwidth(w)\` /
  \`.cap("round"|"flat"|"square")\` -- outline style for \`*.draw(...)\`.
- \`brush.color(c)\` -- fill color for \`*.fill(...)\`.
- \`gradientbrush.color(c)\` + \`.points(x1,y1, x2,y2)\` -- adds a linear
  gradient to the next fill once its color is non-transparent.
- \`line.draw(x1,y1, x2,y2[, color[, width]])\`
- \`rect.draw/fill(width, height[, cornerRadius[, color]])\` -- centered at
  \`(0,0)\` before any transform.
- \`circle.draw/fill(pos...[, color])\`, \`circle.arc/pie(startAngle,
  sweepAngle, pos...[, color])\` -- \`pos\` is 1-4 numbers: \`diameter\` /
  \`diameterX,diameterY\` / \`diameter,x,y\` / \`diameterX,diameterY,x,y\`
  (always centered, never top-left). Angles: 0 deg = right (+X), positive =
  clockwise.
- \`path.begin()\` (new path) / \`.start()\` (new subfigure, same path) /
  \`.addlines(x1,y1, x2,y2, ...)\` / \`.addarc(startAngle, sweepAngle,
  pos...)\` / \`.addpoint(x,y)\` / \`.addcubic(cp1x,cp1y, cp2x,cp2y, x,y)\`
  (cubic Bezier) / \`.addquad(cpx,cpy, x,y)\` (quadratic Bezier) / \`.close()\`
  (line back to start) / \`.draw([color])\` / \`.fill([color])\`.
- \`text.draw(text, size, x, y[, color])\`
- \`transform.translate(x,y)\` / \`.rotate(angle[, pivotX, pivotY])\` /
  \`.scale(ratio)\` or \`.scale(ratioX, ratioY)\` / \`.reset()\` -- cumulative
  until \`reset()\`; typically used around \`#include\` to place/repeat a
  template at different positions/angles.
- \`globals.set(name, value)\` / \`.tryset(name, value)\` (tryset = only if
  unset) -- **\`.globals\` files only**.

\`@@name\` anywhere a parameter is expected reads a globals variable.
Arithmetic (\`+ - * /\`, parens, e.g. \`-3.6 * @@percent\`) works in any
numeric parameter.

## Reuse: \`#include\` and \`repeat\`

\`#include "templates/foo.spt"\` textually inlines a template (path relative
to the including file). \`repeat(n) { ...statements... }\` (header and \`}\`
each alone on their own line, \`n\` a literal integer) unrolls a block \`n\`
times -- combine with cumulative \`transform.rotate\` for radial patterns
instead of copy-pasting \`n\` near-identical blocks.

## Styling

\`styles/default.globals\` is always loaded first. Render with a named style
via \`-style <name>\` (root command) or \`--styles <name>\` (compose),
loading \`styles/<name>.globals\` first (its \`set\` calls win over
\`default.globals\`'s \`tryset\` calls). List available styles: look at the
\`.globals\` files in \`styles/\` (or wherever \`.sketchpen.json\`'s
\`stylesPath\` points, if present).

## Creating a new icon-set folder

When asked to create a **new** icon-set folder (e.g. a fresh \`Music/\` folder
of instrument icons in a project that may already have other icon sets),
**always give it a \`.sketchpen.json\`** -- do not just rely on the implicit
\`./styles\` fallback and skip writing the file.

1. First check whether the project already has other icon-set folders with
   their own styles (a \`styles/\` subfolder, or an existing
   \`.sketchpen.json\`). If so, point the new folder's \`.sketchpen.json\` at
   that same shared location instead of creating a separate copy, so the
   whole project stays visually consistent and a style change updates every
   icon set at once:

   \`\`\`json
   {
     "stylesPath": "../shared-styles-folder-name"
   }
   \`\`\`

   (\`stylesPath\` is resolved relative to this \`.sketchpen.json\`'s own
   directory -- adjust the \`../...\` to wherever the shared folder actually
   is.)
2. If this is the first icon-set folder in the project, still create a
   \`.sketchpen.json\` -- just point it at a local \`styles/\` subfolder next
   to the new \`.sp\` files:

   \`\`\`json
   {
     "stylesPath": "./styles"
   }
   \`\`\`

   This makes the styles location explicit and discoverable up front, so
   any icon-set folder created later in the project can point straight at
   it instead of duplicating \`default.globals\`.

## Minimal worked example

\`\`\`csharp
// A filled circle with an outlined ring
circle.fill(70, "#4a90d9");
circle.draw(70);
circle.draw(30, "#fff");
\`\`\`

## Conventions for this folder

- New icons: one \`.sp\` file per icon, in this folder.
- Reusable shapes: \`.spt\` templates under \`templates/\`, included via
  \`#include\`.
- Match the pen width / color style already used by existing icons in this
  folder (open a couple of the existing \`.sp\` files and \`styles/*.globals\`
  for reference before inventing new values).
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
 * usable: styles/default.globals (always auto-included from the local styles/ subfolder -- no
 * .sketchpen.json needed for this common case, see docs/SYNTAX.md), a templates/ folder with one
 * example .spt (demonstrating #include), a starter .sp that already renders something, and an
 * AGENTS.md so AI coding agents (Claude Code, Copilot, Cursor, ...) working in this folder know
 * the syntax and, critically, know to render-and-look at their own output rather than guessing
 * blind. Never overwrites existing files -- each artifact is created independently and only if
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

            record(
                'styles/default.globals',
                await createIfMissing(path.join(targetDir, 'styles', 'default.globals'), DEFAULT_GLOBALS)
            );
            record(
                'templates/example.spt',
                await createIfMissing(path.join(targetDir, 'templates', 'example.spt'), EXAMPLE_TEMPLATE)
            );
            record('AGENTS.md', await createIfMissing(path.join(targetDir, 'AGENTS.md'), AGENTS_MD));

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
