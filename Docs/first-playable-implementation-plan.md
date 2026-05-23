# First Playable Persistence Flow Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a local MongoDB-backed Fantasy smoke flow that can create/load a player, enter world, start idle battle, settle rewards, stop battle, and verify reconnect persistence.

**Architecture:** Add a project-owned Fantasy application under `GameServer/` instead of modifying Fantasy framework source. The app uses a three-project structure (`Entity`, `Hotfix`, `Main`) matching Fantasy examples, with persistence models in `Entity`, services and message handlers in `Hotfix`, and startup/smoke commands in `Main`.

**Tech Stack:** .NET 8, Fantasy `2026.0.1019`, MongoDB local dev database `idrpg3d_dev`, existing generated protocol files from `Protocol/Generated`.

---

## File Structure

- `GameServer/IDRPG3D.GameServer.sln`: solution for the project-owned server app.
- `GameServer/APP/Entity/IDRPG3D.GameServer.Entity.csproj`: references Fantasy.Net and generated protocol source.
- `GameServer/APP/Entity/Fantasy.config`: single-process local dev config with MongoDB `idrpg3d_dev`.
- `GameServer/APP/Entity/Persistence/*.cs`: `PlayerSave`, `IdleBattleSave`, `TeamSave`.
- `GameServer/APP/Entity/Runtime/*.cs`: session marker components.
- `GameServer/APP/Entity/GameErrorCodes.cs`: protocol error code constants.
- `GameServer/APP/Hotfix/IDRPG3D.GameServer.Hotfix.csproj`: references Entity and source generator.
- `GameServer/APP/Hotfix/Services/*.cs`: player, idle battle, and snapshot services.
- `GameServer/APP/Hotfix/Gate/Handler/*.cs`: request handlers for the existing outer protocol.
- `GameServer/APP/Hotfix/OnCreateSceneEvent.cs`: creates indexes and logs startup state.
- `GameServer/APP/Main/IDRPG3D.GameServer.Main.csproj`: executable entry.
- `GameServer/APP/Main/Program.cs`: `server` mode and `smoke` mode entry.
- `GameServer/APP/Main/Smoke/FirstPlayableSmoke.cs`: MongoDB persistence smoke command.
- `Directory.Build.targets`: pins local Fantasy.Net builds to `net8.0` while this machine uses .NET 8 SDK.
- `Docs/local-mongodb.md`: local database deployment guide.
- `Scripts/install-mongodb.ps1`: repeatable local MongoDB install helper.

## Tasks

### Task 1: Local MongoDB Deployment Assets

**Files:**
- Create: `Docs/local-mongodb.md`
- Create: `Scripts/install-mongodb.ps1`

- [ ] **Step 1: Create local MongoDB docs**

Document local install, service checks, connection string, and expected database name.

- [ ] **Step 2: Create install helper**

Create a PowerShell script that tries `winget install MongoDB.Server`, then prints manual fallback instructions if download fails.

- [ ] **Step 3: Verify docs and script parse**

Run: `Get-Content Docs/local-mongodb.md` and `Get-Content Scripts/install-mongodb.ps1`

Expected: both files read without errors.

### Task 2: GameServer Project Skeleton

**Files:**
- Create: `GameServer/IDRPG3D.GameServer.sln`
- Create: `GameServer/APP/Entity/IDRPG3D.GameServer.Entity.csproj`
- Create: `GameServer/APP/Hotfix/IDRPG3D.GameServer.Hotfix.csproj`
- Create: `GameServer/APP/Main/IDRPG3D.GameServer.Main.csproj`
- Create: `GameServer/APP/Main/Program.cs`
- Create: `GameServer/APP/Entity/Fantasy.config`

- [ ] **Step 1: Create solution and csproj files**

Use .NET 8 and project references to local Fantasy packages.

- [ ] **Step 2: Include generated protocol source**

Link `Protocol/Generated/Server/*.cs` into the Entity project so handlers use the shared protocol.

- [ ] **Step 3: Add local Fantasy.config**

Configure one machine, one process, one world, one Gate scene, and MongoDB `mongodb://127.0.0.1:27017`.

- [ ] **Step 4: Verify compile baseline**

Run: `dotnet build GameServer/IDRPG3D.GameServer.sln -f net8.0 -m:1`

Expected: build reaches missing runtime classes only if later tasks are not implemented; after all project skeleton files exist, build should pass.

### Task 3: Persistence Models and Runtime State

**Files:**
- Create: `GameServer/APP/Entity/Persistence/IdleBattleSave.cs`
- Create: `GameServer/APP/Entity/Persistence/PlayerSave.cs`
- Create: `GameServer/APP/Entity/Persistence/TeamSave.cs`
- Create: `GameServer/APP/Entity/Runtime/GameSessionComponent.cs`
- Create: `GameServer/APP/Entity/Runtime/GameErrorCodes.cs`

- [ ] **Step 1: Add MongoDB persistence entities**

Entities inherit `Entity` and implement `ISupportedSerialize`.

- [ ] **Step 2: Add session runtime component**

Stores `PlayerId` and `Token` on the Fantasy `Session`.

- [ ] **Step 3: Verify build**

Run: `dotnet build GameServer/IDRPG3D.GameServer.sln -f net8.0 -m:1`

Expected: build passes or only reports missing service/handler code planned in later tasks.

### Task 4: Services

**Files:**
- Create: `GameServer/APP/Hotfix/Services/PlayerService.cs`
- Create: `GameServer/APP/Hotfix/Services/IdleBattleService.cs`
- Create: `GameServer/APP/Hotfix/Services/TeamService.cs`
- Create: `GameServer/APP/Hotfix/Services/SnapshotFactory.cs`
- Create: `GameServer/APP/Hotfix/Services/GameClock.cs`

- [ ] **Step 1: Implement player load/create**

`PlayerService.Login` validates account, queries by account, creates default player if missing, and saves it.

- [ ] **Step 2: Implement enter world**

Validates token, attaches session component, settles offline battle if needed, and returns snapshot data.

- [ ] **Step 3: Implement idle battle**

Start is idempotent, settle grants deterministic rewards, stop settles and marks stopped.

- [ ] **Step 4: Implement team create/join**

Create persists a team and updates player `TeamId`; join adds the player to an existing team.

- [ ] **Step 5: Verify build**

Run: `dotnet build GameServer/IDRPG3D.GameServer.sln -f net8.0 -m:1`

Expected: build passes or only reports missing handler references planned in Task 5.

### Task 5: Fantasy Handlers and Scene Startup

**Files:**
- Create: `GameServer/APP/Hotfix/OnCreateSceneEvent.cs`
- Create: `GameServer/APP/Hotfix/Handlers/C2GLoginRequestHandler.cs`
- Create: `GameServer/APP/Hotfix/Handlers/C2GEnterWorldRequestHandler.cs`
- Create: `GameServer/APP/Hotfix/Handlers/C2GStartIdleBattleRequestHandler.cs`
- Create: `GameServer/APP/Hotfix/Handlers/C2GStopIdleBattleRequestHandler.cs`
- Create: `GameServer/APP/Hotfix/Handlers/C2GCreateTeamRequestHandler.cs`
- Create: `GameServer/APP/Hotfix/Handlers/C2GJoinTeamRequestHandler.cs`

- [ ] **Step 1: Add startup event**

On Gate scene creation, log database name and create collections/indexes if MongoDB is reachable.

- [ ] **Step 2: Add request handlers**

Handlers stay thin, call services, set `response.ErrorCode`, and do not throw for business errors.

- [ ] **Step 3: Verify build**

Run: `dotnet build GameServer/IDRPG3D.GameServer.sln -f net8.0 -m:1`

Expected: build passes.

### Task 6: Smoke Runner

**Files:**
- Create: `GameServer/APP/Main/Smoke/FirstPlayableSmoke.cs`
- Modify: `GameServer/APP/Main/Program.cs`

- [ ] **Step 1: Add smoke command**

`dotnet run --project GameServer/APP/Main/IDRPG3D.GameServer.Main.csproj -- smoke` creates a Fantasy scene, invokes services, and prints the persisted flow result.

- [ ] **Step 2: Run without MongoDB**

Run the smoke command.

Expected if MongoDB is unavailable: clear failure saying MongoDB connection is unavailable.

- [ ] **Step 3: Run with MongoDB**

Run the smoke command after MongoDB starts.

Expected: login creates player, enter world returns snapshot, start/settle/stop persist rewards, reconnect loads same player with updated gold/exp.

### Task 7: Verification and Commit

**Files:**
- Modify: `README.md`

- [ ] **Step 1: Link docs**

Add links to local MongoDB and first playable setup docs.

- [ ] **Step 2: Run build**

Run: `dotnet build GameServer/IDRPG3D.GameServer.sln -f net8.0 -m:1`

Expected: exit code 0.

- [ ] **Step 3: Run smoke**

Run: `dotnet run --project GameServer/APP/Main/IDRPG3D.GameServer.Main.csproj -- smoke`

Expected: pass if MongoDB installed; otherwise documented MongoDB unavailable failure.

- [ ] **Step 4: Commit**

Commit message: `feat: add first playable persistence server`
