# IDRPG3D

IDRPG3D is an idle 3D RPG project focused on automatic combat, loot farming, and team-based idle progression.

## Repository Layout

- `Client/` - Unity client baseline using TEngine stable release `TEngine6.2.1`.
- `Server/` - Fantasy server baseline using stable release `2026.0.1019`.
- `GameServer/` - Project-owned Fantasy application for IDRPG3D server code.
- `Protocol/` - Shared network protocol definitions and generation notes.
- `Docs/` - Project-level technical notes, version policy, and integration records.

## Version Policy

Framework versions are pinned in `Docs/framework-versions.md`. Upgrade one framework at a time and keep protocol compatibility checks explicit.

## Development Tooling

MCP and Codex skill usage are recorded in `Docs/mcp-and-skills.md`.

## Local Server Smoke

MongoDB setup is recorded in `Docs/local-mongodb.md`.
Unity local scene testing is recorded in `Docs/local-unity-test.md`.

Build the server with the local .NET 8 SDK:

```powershell
dotnet build GameServer/IDRPG3D.GameServer.sln -f net8.0 -m:1
```

Run the persistence smoke flow:

```powershell
dotnet run --project GameServer/APP/Main/IDRPG3D.GameServer.Main.csproj -- smoke
```
