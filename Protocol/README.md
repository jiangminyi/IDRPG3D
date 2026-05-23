# Protocol

This directory contains IDRPG3D network protocol definitions shared by the Unity client and Fantasy server.

## Layout

- `NetworkProtocol/Outer/` - client/server messages.
- `NetworkProtocol/Inner/` - server-to-server messages.
- `NetworkProtocol/RouteType.Config` - Fantasy custom route ids.
- `NetworkProtocol/RoamingType.Config` - Fantasy roaming ids.
- `NetworkProtocol/OpCode.Cache` - Fantasy opcode cache, kept with protocol source.
- `Generated/Client/` - generated C# protocol files for Unity.
- `Generated/Server/` - generated C# protocol files for Fantasy.
- `ExporterSettings.json` - Fantasy protocol export settings.

## First Protocol Baseline

- Login
- Enter world
- Start idle battle
- Stop idle battle
- Battle reward push
- Create team
- Join team

The first version uses Fantasy's default ProtoBuf serialization. MemoryPack can be introduced later per-message after the first client/server loop is stable.

## Export

From this directory:

```powershell
dotnet ..\Server\Tools\ProtocolExportTool\Fantasy.ProtocolExportTool.dll export --silent
```

From the repository root:

```powershell
Push-Location Protocol
dotnet ..\Server\Tools\ProtocolExportTool\Fantasy.ProtocolExportTool.dll export --silent
Pop-Location
```

Use `--silent` for repeatable exports. In Fantasy `2026.0.1019`, the explicit `-n/-s/-c/-t` mode still asks for interactive confirmation.

TEngine client code should consume generated protocol code through a game-facing network facade, such as `GameNetworkService`, instead of calling framework internals from gameplay/UI code.
