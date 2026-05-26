# Wave Spawning Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the first config-driven wave loop where normal enemies spawn ahead on the route and boss enemies spawn at fixed anchors.

**Architecture:** Add small runtime wave records and controller classes in `GameplayPrototype`, keep LocalTest prefab/template wiring in `LocalTest`, and add Luban config tables for later data-driven authoring.

**Tech Stack:** Unity C#, Dreamteck Splines, NavMeshAgent, Luban Excel config, NUnit editor tests.

---

### Task 1: Wave Core Tests

**Files:**
- Modify: `Client/UnityProject/Assets/Editor/IDRPG3DGameplayPrototypeTests/IDRPG3DGameplayCoreTests.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DWaveSpawnMode.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DWaveDefinition.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DWaveSpawnResolver.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DSpawnAnchor.cs`

- [ ] Add tests for `SplineAhead` and `FixedAnchor` spawn resolution.
- [ ] Run `dotnet build Client/UnityProject/IDRPG3D.GameplayPrototype.Tests.csproj` and confirm tests fail because types do not exist.
- [ ] Add minimal runtime types and resolver implementation.
- [ ] Run the same build and confirm it passes.

### Task 2: Wave Controller

**Files:**
- Modify: `Client/UnityProject/Assets/Editor/IDRPG3DGameplayPrototypeTests/IDRPG3DGameplayCoreTests.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DWaveController.cs`

- [ ] Add a test showing a wave does not advance while enemies are alive.
- [ ] Add a test showing a wave advances after all tracked enemies die.
- [ ] Implement `IDRPG3DWaveController` with `Configure`, `StartStage`, `TickForTest`, and death tracking.
- [ ] Run gameplay prototype tests.

### Task 3: Config Tables

**Files:**
- Modify: `Client/Configs/GameConfig/Datas/__tables__.xlsx`
- Create: `Client/Configs/GameConfig/Datas/stage.xlsx`
- Create: `Client/Configs/GameConfig/Datas/enemy.xlsx`
- Create: `Client/Configs/GameConfig/Datas/enemy_level.xlsx`
- Create: `Client/Configs/GameConfig/Datas/wave.xlsx`

- [ ] Add `stage.TbStage`, `stage.TbEnemy`, `stage.TbEnemyLevel`, and `stage.TbWave`.
- [ ] Add sample Stage 1 waves: two normal waves and one boss wave.
- [ ] Run Luban validation and client generation.

### Task 4: LocalTest Integration

**Files:**
- Modify: `Client/UnityProject/Assets/GameScripts/LocalTest/IDRPG3DLocalTestBootstrap.cs`
- Create: `Client/UnityProject/Assets/GameScripts/LocalTest/IDRPG3DLocalWaveConfigLoader.cs`

- [ ] Load stage/enemy/wave config in LocalTest.
- [ ] Hide existing `Enemy/Enemy1/Enemy2/Enemy3` objects as templates.
- [ ] Add or find a `BossSpawn_Stage01` anchor.
- [ ] Spawn normal waves ahead of the route anchor and boss wave at the fixed anchor.
- [ ] Wire spawned enemies with existing combat, health bar, grounding, and AI setup.

### Task 5: Verification

**Files:**
- Build/test only.

- [ ] Run `python C:/Users/Administrator/.codex/skills/luban-dev/scripts/luban_helper.py --data-dir Client/Configs/GameConfig/Datas validate --all`.
- [ ] Run `dotnet build Client/UnityProject/IDRPG3D.GameplayPrototype.Tests.csproj`.
- [ ] Run `dotnet build Client/UnityProject/IDRPG3D.LocalTest.csproj`.
- [ ] Run `git diff --check`.
