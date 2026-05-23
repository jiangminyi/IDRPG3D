# MCP and Skills Setup

Created: 2026-05-24

This document records the MCP integration and Codex skills currently prepared for the IDRPG3D project. It is the reference for future client, server, protocol, and Unity Editor automation work.

## MCP

| Area | Value |
| --- | --- |
| Unity MCP package | `MCP for Unity` |
| Version | `v9.7.0` |
| Unity package mode | Embedded package |
| Package path | `Client/UnityProject/Packages/com.coplaydev.unity-mcp` |
| Unity manifest entry | `"com.coplaydev.unity-mcp": "file:com.coplaydev.unity-mcp"` |
| Codex MCP endpoint | `http://127.0.0.1:8080/mcp` |
| Server command | `uvx --from mcpforunityserver mcp-for-unity --transport http --http-url http://localhost:8080` |

## MCP Status

- The Unity package is checked into the client project as an embedded package, so Unity does not need to download it from GitHub during package resolution.
- The package imported successfully in Unity and generated `MCPForUnity.Editor.dll` and `MCPForUnity.Runtime.dll`.
- The latest Unity log check after MCP import did not show new package resolve errors or C# compile errors.
- The MCP HTTP server was started locally and listened on `127.0.0.1:8080`.
- The current Codex session did not hot-load Unity MCP tools after the server started. Restart Codex to let it read the MCP config and discover the tools.

## Codex MCP Config

The active Codex config contains:

```toml
[mcp_servers.unityMCP]
url = "http://127.0.0.1:8080/mcp"

[features]
rmcp_client = true
```

## Installed Skills

| Skill | Purpose | Main Use |
| --- | --- | --- |
| `using-superpowers` | Skill entry rule | Check and use applicable skills before work. |
| `systematic-debugging` | Debugging process | Unity errors, Git problems, package failures, build failures, MCP connection issues. |
| `verification-before-completion` | Completion gate | Run fresh verification before claiming something is fixed, compiled, pushed, or complete. |
| `tengine-dev` | TEngine client development | UI, events, resources, hotfix code, module access, YooAsset/HybridCLR patterns. |
| `luban-dev` | Luban config workflow | Config tables, schema changes, export scripts, generated config code. |
| `fantasy-net` | Fantasy server development | ECS, `FTask`, network handlers, protocols, scenes, database, server config. |
| `unity-mcp-orchestrator` | Unity Editor MCP workflow | Scene inspection, GameObject operations, console checks, screenshots, tests, Unity tool orchestration. |

## Project Skill Sources

Project-specific skills were installed into `C:\Users\Administrator\.codex\skills` from these source locations:

| Skill | Source |
| --- | --- |
| `tengine-dev` | `Client/UnityProject/.claude/skills/tengine-dev` |
| `luban-dev` | `Client/UnityProject/.claude/skills/luban-dev` |
| `fantasy-net` | `Server/Skills/fantasy-net` |
| `unity-mcp-orchestrator` | `MCP for Unity v9.7.0` release package, `unity-mcp-skill` folder |

## Usage Rules

For client development:

- Use `tengine-dev` for TEngine architecture, UI, events, resource loading, hotfix boundaries, and module access.
- Use `luban-dev` when adding or editing config tables, schemas, generated config code, or export scripts.
- Use `unity-mcp-orchestrator` when operating Unity Editor through MCP, including scene checks, object creation, script validation, console reads, screenshots, and Unity tests.

For server development:

- Use `fantasy-net` for Fantasy server code, ECS entities/components/systems, `FTask`, message handlers, protocols, scenes, database, and config.

For troubleshooting:

- Start with `systematic-debugging`.
- Find the root cause before making changes.
- After changes, use `verification-before-completion` and record the command or log evidence.

## Unity MCP Workflow

When Unity MCP tools are available after restarting Codex:

1. Check editor state first.
2. Read relevant project or scene resources.
3. Use MCP tools to inspect or modify Unity.
4. Wait for compilation after script edits.
5. Read Unity Console errors and warnings.
6. Use screenshots when visual verification matters.

Do not assume Unity has compiled successfully just because files were written. Confirm through Unity logs, MCP console reads, generated assemblies, or test output.

## Notes

- `.cache/` is ignored and is only for temporary downloaded archives or extracted source used during setup.
- The embedded MCP package is committed under `Client/UnityProject/Packages/com.coplaydev.unity-mcp`.
- The local repository currently uses Unity `2022.3.21f1c1` after opening the TEngine client project with that editor.
