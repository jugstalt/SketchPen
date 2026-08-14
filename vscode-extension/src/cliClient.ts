import * as vscode from 'vscode';
import { execFile } from 'child_process';

// ---- Types matching the CLI's JSON shapes exactly (see ../../docs/CLI.md) ----

export interface EditorCompletionModel {
    method: string;
    suggestion: string;
    snippet: string;
}

export type CompletionTable = Record<string, EditorCompletionModel[]>;

export interface LanguageInfoResult {
    success: boolean;
    commands: {
        code: CompletionTable;
        template: CompletionTable;
        globals: CompletionTable;
    } | null;
    globalVariables: string[] | null;
    error: string | null;
}

export interface ComposeError {
    type: 'syntax' | 'generic';
    message: string;
    codeFile: string | null;
    statement: string | null;
}

export interface ComposeJsonResult {
    success: boolean;
    filesWritten: string[];
    error: ComposeError | null;
    message: string | null;
}

export interface ComposeArgs {
    path: string;
    // Optional when `profile` supplies it instead (see `--profile` in the CLI: composer/sizes/
    // styles/resolutions all come from .sketchpen.json's exportProfiles[profile] unless given
    // here explicitly, in which case the explicit value still wins).
    composer?: string;
    sizes?: string;
    styles?: string;
    resolutions?: string;
    profile?: string;
    out: string;
}

export interface RenderArgs {
    path: string;
    outFolder: string;
    style?: string;
    format: 'svg' | 'png';
}

export class CliNotFoundError extends Error {
    constructor() {
        super('sketchpen CLI not found on PATH (or at the configured sketchpen.cliPath).');
        this.name = 'CliNotFoundError';
    }
}

interface RunResult {
    stdout: string;
    stderr: string;
    exitCode: number;
}

function getCliPath(): string {
    return vscode.workspace.getConfiguration('sketchpen').get<string>('cliPath', 'sketchpen');
}

// Resolves with { stdout, exitCode } even on a non-zero exit code -- the CLI's --output json
// mode (and language-info, always-json) writes a valid, parseable JSON error payload to stdout
// on failure, which callers need to read. Only rejects (CliNotFoundError) when the binary
// itself can't be found/started at all.
function run(args: string[], cwd?: string): Promise<RunResult> {
    return new Promise((resolve, reject) => {
        execFile(
            getCliPath(),
            args,
            { cwd, maxBuffer: 10 * 1024 * 1024 },
            (error, stdout, stderr) => {
                if (error && (error as NodeJS.ErrnoException).code === 'ENOENT') {
                    reject(new CliNotFoundError());
                    return;
                }

                const exitCode = (error as NodeJS.ErrnoException & { code?: unknown })?.code;
                resolve({
                    stdout,
                    stderr,
                    exitCode: typeof exitCode === 'number' ? exitCode : (error ? 1 : 0)
                });
            }
        );
    });
}

export async function checkCliAvailable(): Promise<boolean> {
    try {
        const result = await run(['--version']);
        return result.exitCode === 0;
    } catch {
        return false;
    }
}

/** Runs `dotnet tool install -g SketchPen.Cli` in a new integrated terminal. Used both by the
 * activation-time "not found" prompt and by the `SketchPen: Install CLI` command / walkthrough step. */
export function installCli(): void {
    const terminal = vscode.window.createTerminal('SketchPen Install');
    terminal.show();
    terminal.sendText('dotnet tool install -g SketchPen.Cli');
}

function openInstallDocs(): Thenable<boolean> {
    return vscode.env.openExternal(
        vscode.Uri.parse('https://github.com/jugstalt/SketchPen/blob/main/docs/CLI.md#installation')
    );
}

/** Checks CLI availability and, if missing, offers to install it. Returns true if available. */
export async function ensureCliAvailable(): Promise<boolean> {
    if (await checkCliAvailable()) {
        return true;
    }

    const install = 'Install';
    const docs = 'Docs';
    const choice = await vscode.window.showWarningMessage(
        "SketchPen CLI not found — code completion, preview, and export won't work without it.",
        install,
        docs
    );

    if (choice === install) {
        installCli();
    } else if (choice === docs) {
        await openInstallDocs();
    }

    return false;
}

/**
 * `sketchpen language-info [path]` — static completion grammar, plus (when `path` is given)
 * that project's `@@variable` names from `default.globals`. See docs/CLI.md#the-language-info-subcommand.
 */
export async function getLanguageInfo(path?: string): Promise<LanguageInfoResult> {
    const args = ['language-info'];
    if (path) {
        args.push(path);
    }

    const result = await run(args);
    return JSON.parse(result.stdout) as LanguageInfoResult;
}

/**
 * `sketchpen compose <path> [--composer <id>] [--profile <name>] ... --output json`. Used both
 * for the dedicated export commands and for the shared live-preview/diagnostics mechanism (see
 * diagnosticsManager.ts). `composer` is only required if `profile` isn't given (a profile can
 * supply the composer id itself, see .sketchpen.json's `exportProfiles`).
 */
export async function compose(args: ComposeArgs): Promise<ComposeJsonResult> {
    const cliArgs = ['compose', args.path, '--out', args.out, '--output', 'json'];

    if (args.composer) {
        cliArgs.push('--composer', args.composer);
    }
    if (args.profile) {
        cliArgs.push('--profile', args.profile);
    }
    if (args.sizes) {
        cliArgs.push('--sizes', args.sizes);
    }
    if (args.styles) {
        cliArgs.push('--styles', args.styles);
    }
    if (args.resolutions) {
        cliArgs.push('--resolutions', args.resolutions);
    }

    const result = await run(cliArgs);
    return JSON.parse(result.stdout) as ComposeJsonResult;
}

/**
 * `sketchpen <path> -outfolder <dir> -format svg|png [-style <name>] --output json` -- the
 * classic root command (not `compose`), which renders every `*.sp` file directly inside `path`
 * (non-recursive) in one CLI process instead of one subprocess per icon. Used for the whole-set
 * preview (see setPreviewPanel.ts) -- far cheaper than N separate `compose` calls for a folder
 * with dozens/hundreds of icons. Its JSON shape (`{success, filesWritten, error, message}`) is
 * identical to `compose`'s (see JsonConsoleReporter.cs / ComposeJsonResult), so it's reused as-is.
 */
export async function render(args: RenderArgs): Promise<ComposeJsonResult> {
    const cliArgs = [args.path, '--outfolder', args.outFolder, '--format', args.format, '--output', 'json'];
    if (args.style) {
        cliArgs.push('--style', args.style);
    }

    const result = await run(cliArgs);
    return JSON.parse(result.stdout) as ComposeJsonResult;
}
