# First Playable Client/Server Flow Design

Created: 2026-05-23

## Purpose

The first playable backend loop should prove the whole pipeline with the smallest real game flow:

1. Unity connects through the project network facade.
2. Fantasy loads or creates a player from MongoDB.
3. The player enters the world and receives an authoritative snapshot.
4. The player starts idle battle.
5. The server settles rewards, persists them, and pushes the result.
6. The player stops idle battle.
7. Reconnect reloads the persisted state.
8. Two online players can create or join a team and receive team snapshot updates.

This phase is about proving persistence and synchronization boundaries, not building final combat, account security, sharding, or production operations.

## Decisions

- Persistence: use MongoDB through Fantasy's database layer.
- Authority: the server owns all gameplay state; the client keeps a display cache only.
- Synchronization: use full snapshots for first-version player/team state and server pushes for online changes.
- Scope: single Fantasy process, single MongoDB database, single region.
- Team data: persist a minimal team record so reconnect can restore team membership.
- Duplicate login: first version allows one online session per player; a new login replaces the old session.

## Components

### Unity Client

The client talks to Fantasy only through a project-owned facade such as `GameNetworkService`.

Responsibilities:

- Create and own the Fantasy session.
- Send login, enter world, start idle battle, stop idle battle, create team, and join team requests.
- Listen for reward and team/player update pushes.
- Maintain an in-memory `PlayerClientState` for UI display.
- Rebuild local state from the latest authoritative snapshot after reconnect.

The client must not calculate final rewards, write player state, or treat local state as authoritative.

### Fantasy Server

The first version should keep gameplay code behind small services:

- `PlayerService`: load/create/save player data and manage online player sessions.
- `IdleBattleService`: start, settle, stop, and serialize idle battle state.
- `TeamService`: create/join teams and fan out team snapshot pushes.
- `OnlinePlayerRegistry`: map `PlayerId -> Session` for direct pushes.

Message handlers should stay thin: validate session/player identity, call the service, and fill the response.

## Persistence Model

### PlayerSave

One MongoDB document per player.

- `Id` / `PlayerId`
- `Account`
- `Name`
- `Level`
- `Exp`
- `Gold`
- `Inventory`
- `TeamId`
- `IdleBattle`
- `Revision`
- `CreateTime`
- `LastLoginTime`
- `LastSaveTime`

`Revision` increments on committed gameplay changes. The client can use it later to ignore stale pushes if protocol snapshots include it.

### IdleBattleSave

Embedded in `PlayerSave`.

- `BattleId`
- `MapId`
- `IsRunning`
- `StartTime`
- `LastSettleTime`
- `PendingRewards`
- `PendingExp`
- `PendingGold`

The server calculates offline rewards from `LastSettleTime` when the player reconnects or when a battle operation is requested.

### TeamSave

One MongoDB document per team.

- `Id` / `TeamId`
- `LeaderPlayerId`
- `MemberPlayerIds`
- `CreateTime`
- `UpdateTime`

For the first version, team membership is simple and small. No matchmaking, invite workflow, permissions, or cross-server team migration are included.

## Network Flow

### Login

1. Client sends `C2G_LoginRequest(Account, DeviceId)`.
2. Server finds `PlayerSave` by `Account`.
3. If missing, server creates a default player document.
4. Server issues a temporary development token.
5. Server returns `G2C_LoginResponse(PlayerId, Token, PlayerBrief)`.

The token is a first-version session guard only. Real authentication can be replaced later without changing gameplay services.

### Enter World

1. Client sends `C2G_EnterWorldRequest(PlayerId, Token)`.
2. Server validates the token.
3. Server closes or replaces any old online session for the same player.
4. Server loads `PlayerSave` and `TeamSave`.
5. Server settles offline idle battle rewards if needed.
6. Server attaches `PlayerId` to the session and registers it online.
7. Server returns an authoritative snapshot.

The current protocol has `PlayerBrief`, `TeamInfo`, and `IdleBattleSummary`. Before implementing richer UI, add a small resource snapshot or player snapshot message so total `Exp`, `Gold`, and `Revision` are unambiguous.

### Start Idle Battle

1. Client sends `C2G_StartIdleBattleRequest(MapId)`.
2. Server verifies the player is online and the map is allowed.
3. If no battle is running, server creates `IdleBattleSave`.
4. If a battle is already running, server returns the current battle summary as an idempotent success.
5. Server saves the player document.
6. Server returns `G2C_StartIdleBattleResponse(IdleBattleSummary)`.

### Reward Settlement Push

1. Server calculates rewards from elapsed time.
2. Server applies rewards to authoritative player state.
3. Server saves the updated player document.
4. Server pushes `G2C_BattleRewardPush` to the online player.

The push represents committed server state. If persistence fails, do not send the success push for that settlement.

### Stop Idle Battle

1. Client sends `C2G_StopIdleBattleRequest(BattleId)`.
2. Server settles final rewards.
3. Server marks the battle as stopped.
4. Server saves the player document.
5. Server returns `G2C_StopIdleBattleResponse(IdleBattleSummary)`.

### Team Create / Join

1. Client sends create or join request.
2. Server updates `PlayerSave.TeamId` and `TeamSave.MemberPlayerIds`.
3. Server saves affected documents.
4. Server returns the latest `TeamInfo`.
5. Server pushes the same full `TeamInfo` to every online member.

Offline members do not need a push. They receive the current team snapshot on the next `EnterWorld`.

## Synchronization Rules

- Requests mutate state only on the server.
- Responses confirm the state after the requested operation.
- Pushes notify online clients after committed server changes.
- Reconnect always trusts the full server snapshot over local client state.
- Offline progress is calculated from persisted timestamps, not from client reports.
- First version sends full snapshots, not deltas.
- Each player's gameplay mutations should be serialized by player id to avoid overlapping reward/team writes.

## Error Handling

- MongoDB unavailable: login and enter-world fail; do not create memory-only players.
- Save failure during gameplay: report an error or skip the push; do not pretend the change committed.
- Invalid token: reject enter-world.
- Missing player: reject enter-world.
- Missing team: clear `PlayerSave.TeamId` on next enter-world or return a recoverable error.
- Duplicate online player: new login replaces the old session in the first version.

## Testing Plan

- Unit/service test: login creates a default `PlayerSave`.
- Unit/service test: enter-world loads existing player data.
- Unit/service test: start idle battle persists battle state.
- Unit/service test: settlement updates `Exp`, `Gold`, rewards, and `LastSettleTime`.
- Unit/service test: stop idle battle persists the final stopped state.
- Integration smoke: start local MongoDB, run Fantasy, login from a thin client or Unity test screen, start battle, receive reward push, reconnect, and verify state is restored.
- Team smoke: two sessions create/join the same team and both receive the latest `TeamInfo`.

## First Implementation Order

1. Add local MongoDB configuration and document the local startup command.
2. Add persistence entities for `PlayerSave`, `IdleBattleSave`, and `TeamSave`.
3. Add server services without Unity dependency.
4. Wire Fantasy message handlers to the services.
5. Add protocol fields needed for a full player resource snapshot.
6. Add generated protocol code after export.
7. Add a minimal `GameNetworkService` client flow and a simple test UI.
8. Run the smoke tests.

## Explicit Non-Goals

- Real account authentication.
- Payment, anti-cheat, rankings, mail, chat, guilds, matchmaking, or inventory depth.
- Multi-server routing for team gameplay.
- Final combat formulas.
- MemoryPack optimization.
- Delta-compressed state replication.
