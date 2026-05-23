# Local MongoDB Setup

Created: 2026-05-24

The first playable server flow uses MongoDB through Fantasy's database layer.

## Local Development Settings

| Setting | Value |
| --- | --- |
| Connection string | `mongodb://127.0.0.1:27017` |
| Database name | `idrpg3d_dev` |
| Fantasy config location | `GameServer/APP/Entity/Fantasy.config` |

## Install

Preferred command:

```powershell
winget install --id MongoDB.Server --exact --silent --accept-package-agreements --accept-source-agreements
```

If `winget` cannot download the MSI, download and install MongoDB Community Server manually from:

```text
https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-8.3.2-signed.msi
```

The local helper script records the same flow:

```powershell
.\Scripts\install-mongodb.ps1
```

## Verify

Check the Windows service:

```powershell
Get-Service | Where-Object { $_.Name -like '*Mongo*' -or $_.DisplayName -like '*Mongo*' }
```

Check the port:

```powershell
Test-NetConnection 127.0.0.1 -Port 27017
```

Expected:

```text
TcpTestSucceeded : True
```

## Fantasy Configuration

The game server config should include this database entry under its `<world>` node:

```xml
<database dbType="MongoDB" dbName="idrpg3d_dev" dbConnection="mongodb://127.0.0.1:27017"/>
```

Fantasy code accesses this through `scene.World.Database` or `scene.World["idrpg3d_dev"]`.

## Smoke Test

After MongoDB is running:

```powershell
dotnet build GameServer/IDRPG3D.GameServer.sln -f net8.0 -m:1
dotnet run --project GameServer/APP/Main/IDRPG3D.GameServer.Main.csproj -- smoke
```

The smoke command writes and reads a development player, idle battle result, and team record in `idrpg3d_dev`.

Current verified output:

```text
First playable smoke passed.
PlayerId=8146127795728359136, BattleId=4935911106302621444, TeamId=5892162853560949241, Database=idrpg3d_dev
```
