# Config-Driven Skill System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move the current Frostbolt and Fireball prototype skills into Luban-driven config while preserving the local combat scene.

**Architecture:** Add Luban skill/effect/buff/projectile tables, generate config code and bytes, then bridge generated config into the existing prototype skill definition. Keep a hardcoded fallback so Unity play mode still works while generated config assets are being refreshed.

**Tech Stack:** Unity, C#, NUnit Editor tests, TEngine project layout, Luban Excel config, Blink visual prefabs.

---

### Task 1: Document The Design

**Files:**
- Create: `docs/superpowers/specs/2026-05-25-config-driven-skill-system-design.md`
- Create: `docs/superpowers/plans/2026-05-25-config-driven-skill-system.md`

- [x] **Step 1: Write the design spec**

Capture table ownership, runtime adapter boundaries, sync event direction, performance constraints, and test expectations in the design spec.

- [x] **Step 2: Write this implementation plan**

Create a bite-sized plan that can be executed and verified in this repository.

### Task 2: Add Runtime Mapping Tests

**Files:**
- Modify: `Client/UnityProject/Assets/Editor/IDRPG3DGameplayPrototypeTests/IDRPG3DGameplayCoreTests.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DPrototypeSkillConfigRecord.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DPrototypeSkillConfigBuilder.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DCombatSyncEvents.cs`

- [ ] **Step 1: Write failing tests**

Add tests that construct a Frostbolt config record, convert it to `IDRPG3DPrototypeSkillDefinition`, and assert stable sync payloads carry IDs rather than prefab paths.

- [ ] **Step 2: Run tests and verify failure**

Run `dotnet build Client/UnityProject/IDRPG3D.GameplayPrototype.Tests.csproj` after temporarily including new scripts in generated csproj files if Unity has not refreshed them.

- [ ] **Step 3: Implement the mapping and sync event structs**

Add plain data records and a small builder that maps config fields to the existing runtime skill definition.

- [ ] **Step 4: Run tests and verify pass**

Run the same build command and confirm exit code 0.

### Task 3: Add Luban Skill Tables

**Files:**
- Modify: `Client/Configs/GameConfig/Datas/__tables__.xlsx`
- Create: `Client/Configs/GameConfig/Datas/skill.xlsx`
- Create: `Client/Configs/GameConfig/Datas/skill_effect.xlsx`
- Create: `Client/Configs/GameConfig/Datas/effect.xlsx`
- Create: `Client/Configs/GameConfig/Datas/buff.xlsx`
- Create: `Client/Configs/GameConfig/Datas/projectile.xlsx`

- [ ] **Step 1: Add tables with helper tooling**

Use `luban_helper.py` or a controlled `openpyxl` script to add `skill.TbSkill`, `skill.TbSkillEffect`, `skill.TbEffect`, `skill.TbBuff`, and `skill.TbProjectile` with map mode and `id` indexes.

- [ ] **Step 2: Seed Frostbolt and Fireball**

Insert `frostbolt` and `fireball` skills, matching the current prototype numbers and Blink asset paths.

- [ ] **Step 3: Validate config files**

Run `python C:/Users/Administrator/.codex/skills/luban-dev/scripts/luban_helper.py --data-dir Client/Configs/GameConfig/Datas validate --all`.

### Task 4: Generate Config Code And Bytes

**Files:**
- Generate: `Client/UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/*.cs`
- Generate: `Client/UnityProject/Assets/AssetRaw/Configs/bytes/*.bytes`
- Generate: `Client/UnityProject/Assets/GameScripts/HotFix/GameProto/ConfigSystem.cs`
- Generate: `Client/UnityProject/Assets/GameScripts/HotFix/GameProto/ExternalTypeUtil.cs`

- [ ] **Step 1: Run Luban generation**

From `Client`, run `cmd /c "set AI_MODE=1 && Configs\GameConfig\gen_code_bin_to_project_lazyload.bat"`.

- [ ] **Step 2: Inspect generated files**

Confirm generated skill/effect/buff/projectile C# files and bytes exist.

### Task 5: Wire Local Test Bootstrap

**Files:**
- Modify: `Client/UnityProject/Assets/GameScripts/LocalTest/IDRPG3DLocalTestBootstrap.cs`

- [ ] **Step 1: Resolve configured skills**

Replace direct `CreateFrostbolt` and `CreateFireball` calls with a helper that tries config-backed definitions first and uses the old constructors as fallback.

- [ ] **Step 2: Keep Editor prefab loading contained**

Load Blink prefabs from config paths only inside `#if UNITY_EDITOR` code.

- [ ] **Step 3: Verify local test build**

Run `dotnet build Client/UnityProject/IDRPG3D.LocalTest.csproj` after project refresh or temporary csproj inclusion, then run Unity play mode manually for visual confirmation.

### Task 6: Final Verification

**Files:**
- Review all modified files.

- [ ] **Step 1: Check git diff**

Run `git diff --stat` and inspect changed code/config files.

- [ ] **Step 2: Run builds**

Run:

```powershell
dotnet build Client/UnityProject/IDRPG3D.GameplayPrototype.Tests.csproj
dotnet build Client/UnityProject/IDRPG3D.LocalTest.csproj
```

- [ ] **Step 3: Report exact results**

Summarize generated config files, tests/builds run, and any Unity manual testing still needed.
