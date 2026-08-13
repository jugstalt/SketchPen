import * as path from 'path';
import * as fs from 'fs/promises';

const SKETCHPEN_CONFIG_FILE = '.sketchpen.json';
const DEFAULT_STYLES_SUBFOLDER = 'styles';

/**
 * Light TypeScript-side mirror of SketchPen.Plot.Compile.StylesFolderResolver (see
 * src/SketchPen.Plot/Compile/StylesFolderResolver.cs): resolves the folder a `.sp` file's
 * styles/globals live in. Checks `<folder>/.sketchpen.json` for a "stylesPath" key (resolved
 * relative to `folder`); falls back to `<folder>/styles` if the config file is absent, unreadable,
 * or missing that key. Never throws -- a missing/malformed config is just treated as "use the
 * default", same as the CLI-side resolver.
 */
export async function resolveStylesFolder(folder: string): Promise<string> {
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
 * Lists the named styles available for a `.sp` file's folder (basenames of every `*.globals` file
 * in the resolved styles folder except `default.globals` itself, sorted). Used to populate the
 * preview panel's style switcher (see previewPanel.ts/diagnosticsManager.ts) -- a best-effort
 * directory listing for editor convenience, not something the CLI needs to know about (there is
 * no "list styles" subcommand). Returns an empty list if the styles folder doesn't exist.
 */
export async function listAvailableStyles(folder: string): Promise<string[]> {
    const stylesFolder = await resolveStylesFolder(folder);
    try {
        const entries = await fs.readdir(stylesFolder);
        return entries
            .filter((name) => name.endsWith('.globals') && name !== 'default.globals')
            .map((name) => name.slice(0, -'.globals'.length))
            .sort((a, b) => a.localeCompare(b));
    } catch {
        return [];
    }
}
