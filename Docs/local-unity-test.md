# Local Unity Test Scene

Created: 2026-05-24

The Unity client has a temporary local test scene for the first client/server flow.

## Scene

| Item | Value |
| --- | --- |
| Scene path | `Client/UnityProject/Assets/Scenes/IDRPG3D_LocalTest.unity` |
| Builder menu | `IDRPG3D/Local Test/Rebuild Scene` |
| Open menu | `IDRPG3D/Local Test/Open Scene` |
| Build settings menu | `IDRPG3D/Local Test/Add Scene To Build Settings` |
| Bootstrap script | `Client/UnityProject/Assets/GameScripts/LocalTest/IDRPG3DLocalTestBootstrap.cs` |

This scene is intentionally separate from the normal TEngine startup scene. It is a disposable local harness for testing MongoDB, Fantasy server connectivity, and the first playable request flow.

## Local Services

MongoDB uses the TCP protocol on:

```text
127.0.0.1:27017
```

Fantasy listens for the current Gate scene on:

```text
127.0.0.1:20000 KCP/UDP
```

Do not test MongoDB with a browser. `http://127.0.0.1:27017/` is an HTTP request, but MongoDB is a database TCP protocol endpoint, so a blank page or protocol error is expected.

## Start Server

Build the server:

```powershell
dotnet build GameServer/IDRPG3D.GameServer.sln -f net8.0 -m:1
```

Run the local Fantasy process:

```powershell
dotnet run --project GameServer/APP/Main/IDRPG3D.GameServer.Main.csproj -- -m Develop -g 1
```

Run the MongoDB persistence smoke test:

```powershell
dotnet run --project GameServer/APP/Main/IDRPG3D.GameServer.Main.csproj -- smoke
```

## Unity Verification

1. Open `Client/UnityProject` in Unity `2022.3.21f1c1`.
2. Run `IDRPG3D/Local Test/Rebuild Scene`.
3. Open `Assets/Scenes/IDRPG3D_LocalTest.unity`.
4. Enter Play Mode.
5. Use `Check Ports` to verify MongoDB.

The GameServer label shows `KCP/UDP`; the current button cannot prove KCP connectivity with a TCP socket. The real GameServer check will be added when `GameNetworkService` binds to Fantasy.Unity.

Expected current UI:

- MongoDB shows `OPEN` when the local MongoDB service is running.
- The flow buttons exist: `Login`, `Enter World`, `Start Idle`, `Stop Idle`, `Create Team`.
- The flow buttons currently log placeholders until Fantasy.Unity RPC binding is added.
