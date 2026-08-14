import * as path from 'path';
import * as fs from 'fs/promises';

const SKETCHPEN_CONFIG_FILE = '.sketchpen.json';
const DEFAULT_STYLES_SUBFOLDER = 'styles';

interface SketchPenConfig {
    stylesPath?: string;
    styles?: Record<string, { label?: string }>;
    // Values aren't needed client-side (compose --profile resolves them CLI-side) -- only the
    // names, to populate the "Export Package…" quick-pick (see listExportProfiles below).
    exportProfiles?: Record<string, unknown>;
}

interface FoundConfig {
    config: SketchPenConfig;
    configDirectory: string;
}

export interface StyleOption {
    name: string;
    label: string;
}

/**
 * Light TypeScript-side mirror of SketchPen.Plot.Compile.SketchPenConfigResolver (see
 * src/SketchPen.Plot/Compile/SketchPenConfigResolver.cs): walks upward from `folder` (inclusive)
 * looking for the nearest `.sketchpen.json`, same as the CLI does -- a folder without its own
 * config inherits the nearest ancestor's in full (no per-key merging across levels). Malformed
 * JSON at a given level is treated the same as no file there and the walk continues upward.
 * Returns `null` if none is found anywhere up to the filesystem root.
 */
async function findConfig(folder: string): Promise<FoundConfig | null> {
    let dir = path.resolve(folder);

    while (true) {
        try {
            const configText = await fs.readFile(path.join(dir, SKETCHPEN_CONFIG_FILE), 'utf8');
            const config = JSON.parse(configText) as SketchPenConfig;
            return { config, configDirectory: dir };
        } catch {
            // No/unreadable/malformed .sketchpen.json at this level -- keep walking upward.
        }

        const parent = path.dirname(dir);
        if (parent === dir) {
            return null; // Reached the filesystem root.
        }
        dir = parent;
    }
}

/**
 * Resolves the folder a `.sp` file's styles/globals live in: the nearest ancestor config's
 * `stylesPath` (resolved relative to that config's own directory) if one is found and non-empty,
 * else `<folder>/styles`. Never throws -- a missing/malformed config is just treated as "use the
 * default", same as the CLI-side resolver.
 */
export async function resolveStylesFolder(folder: string): Promise<string> {
    const found = await findConfig(folder);
    if (found?.config.stylesPath && found.config.stylesPath.trim().length > 0) {
        return path.resolve(found.configDirectory, found.config.stylesPath);
    }
    return path.join(folder, DEFAULT_STYLES_SUBFOLDER);
}

/**
 * Lists the named styles available for a `.sp` file's folder: every `*.globals` file in the
 * resolved styles folder except `default.globals` itself, sorted, paired with a display label
 * from the same (inheritance-resolved) config's `styles` metadata if present, else just the style
 * name itself. Used to populate the preview panels' style switchers (see
 * previewPanel.ts/setPreviewPanel.ts) -- a best-effort directory listing for editor convenience,
 * not something the CLI needs to know about for this to work (though `language-info` exposes the
 * same information for other tooling -- see LanguageInfoCommandHandler.cs). Returns an empty list
 * if the styles folder doesn't exist.
 */
export async function listAvailableStyles(folder: string): Promise<StyleOption[]> {
    const found = await findConfig(folder);
    const stylesFolder =
        found?.config.stylesPath && found.config.stylesPath.trim().length > 0
            ? path.resolve(found.configDirectory, found.config.stylesPath)
            : path.join(folder, DEFAULT_STYLES_SUBFOLDER);

    try {
        const entries = await fs.readdir(stylesFolder);
        return entries
            .filter((name) => name.endsWith('.globals') && name !== 'default.globals')
            .map((name) => name.slice(0, -'.globals'.length))
            .sort((a, b) => a.localeCompare(b))
            .map((name) => ({ name, label: found?.config.styles?.[name]?.label || name }));
    } catch {
        return [];
    }
}

/**
 * Lists the named `exportProfiles` entries (see .sketchpen.json's schema, mirrored server-side by
 * SketchPen.Plot.Compile.SketchPenConfig) available to a `.sp` file's folder, for the "Export
 * Package…" command's quick-pick (see exportCommands.ts). Only the names are needed here -- the
 * actual composer/sizes/styles/resolutions values are resolved CLI-side by `compose --profile`,
 * never duplicated in TypeScript.
 */
export async function listExportProfiles(folder: string): Promise<string[]> {
    const found = await findConfig(folder);
    const profiles = found?.config.exportProfiles;
    return profiles ? Object.keys(profiles).sort((a, b) => a.localeCompare(b)) : [];
}
