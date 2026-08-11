# Releasing `SketchPen.Cli`

How to test the packaged CLI locally, and how to cut a real release. See
[`docs/CLI.md`](CLI.md#installation) for end-user install instructions and
[`.github/workflows/release.yml`](../.github/workflows/release.yml) for the
automated publish pipeline this page describes.

## Contents

- [Test locally (no publishing involved)](#test-locally-no-publishing-involved)
- [One-time setup: NuGet API key](#one-time-setup-nuget-api-key)
- [Cutting a release](#cutting-a-release)
- [Testing the release without going fully public](#testing-the-release-without-going-fully-public)

## Test locally (no publishing involved)

Builds the `.nupkg` from your current working tree and installs it from a
local folder — nothing touches NuGet.org or GitHub.

```bash
dotnet pack src/SketchPen/SketchPen.csproj -c Release -o ./local-nupkg
dotnet tool install -g --add-source ./local-nupkg SketchPen.Cli
```

Then, from **outside** the repo folder (so you're not accidentally picking up
a source-tree file instead of the installed tool):

```bash
sketchpen --help
sketchpen plot/basic/disk.sp -outfolder out
sketchpen compose plot/basic/disk.sp --composer svg --sizes 64 --out disk.svg
```

After changing code, repack and re-install to pick up the change (`dotnet
tool install` won't overwrite an existing install, so use `update`):

```bash
dotnet pack src/SketchPen/SketchPen.csproj -c Release -o ./local-nupkg
dotnet tool update -g --add-source ./local-nupkg SketchPen.Cli
```

When you're done testing:

```bash
dotnet tool uninstall -g SketchPen.Cli
```

`local-nupkg/` is a scratch folder — delete it or keep it out of commits
(check `git status` before committing; it isn't `.gitignore`d by name, only
`publish/` and a few build-artifact patterns are).

### Testing the GitHub Actions workflow itself

Two ways to check `.github/workflows/release.yml` without a real release:

- **[`act`](https://github.com/nektos/act)** (needs Docker) — runs the workflow
  entirely offline. The `dotnet nuget push` step will fail without a real
  `NUGET_API_KEY`, which is expected/fine to ignore when just checking the
  earlier build/pack steps.
- **A real but harmless run** — push a pre-release tag (see
  [below](#testing-the-release-without-going-fully-public)) once the API key
  secret is configured.

## One-time setup: NuGet API key

Required once, before the very first real release — **you** need to do this
(API key creation/handling isn't something to hand to an assistant):

1. Create an account at [nuget.org](https://www.nuget.org/) if you don't have
   one.
2. Go to your account → **API Keys** → **Create**. Scope it to
   **Push new packages and package versions**, glob pattern `SketchPen.Cli*`
   (or restrict to the exact package once it exists after the first publish).
3. In the GitHub repo: **Settings → Secrets and variables → Actions → New
   repository secret**, name it `NUGET_API_KEY`, paste the key.

The workflow reads this as `${{ secrets.NUGET_API_KEY }}` — nothing else to
configure.

## Cutting a release

Once the secret is set up, every release is just:

```bash
git tag v0.1.0
git push --tags
```

Pushing a tag matching `v*.*.*` triggers `.github/workflows/release.yml`,
which:

1. Builds `src/SketchPen/SketchPen.csproj` in Release.
2. Packs it, using the tag (minus the leading `v`) as the NuGet package
   version — `v0.1.0` → package version `0.1.0`.
3. Pushes the resulting `.nupkg` to NuGet.org (`--skip-duplicate`, so
   re-running after a partial failure is safe).
4. Creates a GitHub Release for the tag with auto-generated release notes and
   the `.nupkg` attached.

Watch progress under the repo's **Actions** tab. Once it's green, anyone can
install/update with:

```bash
dotnet tool install -g SketchPen.Cli      # first time
dotnet tool update -g SketchPen.Cli       # later releases
```

**Versioning**: the tag drives the package version directly, so use normal
[semver](https://semver.org/) (`vMAJOR.MINOR.PATCH`, e.g. `v0.2.0`,
`v1.0.0`). There's no separate version number to keep in sync anywhere else in
the repo — `src/SketchPen/SketchPen.csproj`'s `<Version>0.1.0</Version>` is
only a fallback for ad-hoc local `dotnet pack` runs without CI, and is
overridden by `-p:Version=` in the workflow.

## Testing the release without going fully public

To dry-run the *real* pipeline (NuGet.org push, GitHub Release) without
shipping something users might install by mistake, push a
[pre-release](https://semver.org/#spec-item-9) tag:

```bash
git tag v0.0.1-test1
git push --tags
```

`0.0.1-test1` is a valid NuGet pre-release version — `dotnet tool install -g
SketchPen.Cli` (no version specified) ignores it, so it won't surface for
normal installs, but you can confirm the whole pipeline worked with:

```bash
dotnet tool install -g SketchPen.Cli --version 0.0.1-test1
```

Delete the tag afterward if you don't want it lingering
(`git push --delete origin v0.0.1-test1 && git tag -d v0.0.1-test1`) — note
this does **not** unpublish the NuGet package or GitHub Release; NuGet.org
packages can be unlisted (not deleted) from the package's own management page
if needed.
