# Framework Versions

Created: 2026-05-23

## Pinned Baselines

| Area | Framework | Version | Source | Notes |
| --- | --- | --- | --- | --- |
| Client | TEngine | `TEngine6.2.1` | `https://github.com/Alex-Rachel/TEngine/releases/tag/TEngine6.2.1` | Stable release source placed under `Client/`. |
| Server | Fantasy | `2026.0.1019` | `https://github.com/qq362946/Fantasy/releases/tag/2026.0.1019` | Stable release source placed under `Server/`. |
| Server package | Fantasy-Net | `2026.0.1019` | `Server/Fantasy.Packages/Fantasy.Net/Fantasy.Net.csproj` | Package version in release source. |
| Server tool | Fantasy.Cli | `2026.0.1019` | `Server/Fantasy.Packages/Fantasy.Cil/Fantasy.Cli.csproj` | CLI version in release source. |
| Unity package | Fantasy.Unity | from Fantasy `2026.0.1019` | `Server/Fantasy.Packages/Fantasy.Unity/` | Use this package version with the matching server release first. |

## Toolchain Notes

- TEngine recommends Unity `2021.3.20f1c1` or newer and supports Unity `2019.4`, `2020.3`, `2021.3`, and `2022.3`.
- The checked-in TEngine project version is Unity `2021.3.45f1`, recorded in `Client/UnityProject/ProjectSettings/ProjectVersion.txt`.
- Fantasy README targets `.NET 8.0+`, C# `12.0`, and Unity `2022.3.62+` for Unity-side support.
- Because the client baseline is TEngine first, the initial Unity editor target should be confirmed before adding Fantasy.Unity to the client manifest.

## Upgrade Rules

1. Upgrade only one framework at a time.
2. Record the old version, new version, source URL, and reason in this file.
3. Re-run protocol export and a client/server smoke test after Fantasy upgrades.
4. Re-open the Unity project and verify package import, hotfix generation, and resource build menus after TEngine upgrades.
5. Keep gameplay code behind project-owned facades such as `GameNetworkService` so framework API changes stay localized.

## Retrieval Notes

The stable release source archives were downloaded from GitHub tag archives because direct `git fetch`/`git clone --branch` access intermittently timed out on this machine. The temporary download files are intentionally excluded by `.gitignore`.

`Server/.gitattributes` in the downloaded Fantasy `2026.0.1019` archive contained a stray patch hunk header on the first line. It was removed locally because Git treated it as an invalid attribute rule and emitted warnings on every repository operation.
