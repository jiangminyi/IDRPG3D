# Config Driven Heroes And Dynamic Levels Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move the local combat prototype from hard-coded hero stats/temporary regen to config-driven hero stats, skills, resources, dynamic enemy levels, XP leveling, and world-bar level display.

**Architecture:** Keep this as a prototype layer in `GameplayPrototype` and `LocalTest`, using Luban source Excel as the data source. Add small runtime components for progression/resources and a multi-skill auto combat brain, while keeping existing `IDRPG3DCombatUnit`, wave spawning, projectiles, buffs, and combat event stream intact.

**Tech Stack:** Unity C#, TEngine/Luban generated configs, NUnit EditMode tests, Blink prefabs for available placeholder effects.

---

### File Structure

- Modify `Client/Configs/GameConfig/Datas/stage.xlsx`, `wave.xlsx`, `skill.xlsx`, `skill_level.xlsx`, `effect.xlsx`, `buff.xlsx`, `projectile.xlsx`, `__tables__.xlsx`: extend config data for dynamic levels, resources, and new skills.
- Create `Client/Configs/GameConfig/Datas/hero.xlsx` and `hero_level.xlsx`: hero base stats and growth.
- Modify generated config outputs through the Luban export script, not by editing generated `.cs` files directly.
- Modify `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DCombatUnit.cs`: add level metadata and death reward metadata, not resource logic.
- Create `IDRPG3DCombatResource.cs`: handles rage/mana current/max/gain/spend/regen.
- Create `IDRPG3DHeroProgression.cs`: handles XP, level-up, stat scaling hooks, and level change events.
- Create `IDRPG3DPrototypeSkillRuntime.cs`: defines skill cast mode, target rule, costs, gains, threat multiplier, and VFX paths.
- Create `IDRPG3DPrototypeSkillBook.cs`: stores multiple runtime skills per unit and cooldown/resource checks.
- Create `IDRPG3DPrototypeCombatDirector.cs`: helper selection for lowest HP ally, dead ally, area targets, and XP distribution.
- Modify `IDRPG3DAutoCombatBrain.cs`: choose skills from `SkillBook` instead of a single skill caster when present.
- Modify `IDRPG3DPrototypeSkillCaster.cs`: accept runtime metadata, resource spending/gain, area/heal/resurrect/charge cast modes.
- Modify `IDRPG3DWorldUnitBar.cs`: find `Level` TMP/Text child and update level plus resource fill.
- Modify `IDRPG3DLocalSkillConfigLoader.cs`: load new skill level metadata and build runtime skill data.
- Modify `IDRPG3DLocalWaveConfigLoader.cs`: load stage/wave dynamic level fields and enemy XP reward.
- Modify `IDRPG3DLocalTestBootstrap.cs`: configure heroes from `hero.xlsx`, attach skill books, remove hard-coded temporary HP/regen, calculate dynamic enemy levels, and distribute XP on enemy death.
- Modify `Client/UnityProject/Assets/Editor/IDRPG3DGameplayPrototypeTests/IDRPG3DGameplayCoreTests.cs`: add focused tests for resource spending/gain, dynamic enemy level clamp, XP level-up, and level bar updates.

### Implementation Tasks

### Task 1: Inspect and update config workbook schemas

**Files:**
- Modify: `Client/Configs/GameConfig/Datas/__tables__.xlsx`
- Modify: `Client/Configs/GameConfig/Datas/stage.xlsx`
- Modify: `Client/Configs/GameConfig/Datas/wave.xlsx`
- Modify: `Client/Configs/GameConfig/Datas/skill.xlsx`
- Modify: `Client/Configs/GameConfig/Datas/skill_level.xlsx`
- Modify: `Client/Configs/GameConfig/Datas/effect.xlsx`
- Modify: `Client/Configs/GameConfig/Datas/buff.xlsx`
- Modify: `Client/Configs/GameConfig/Datas/projectile.xlsx`
- Create: `Client/Configs/GameConfig/Datas/hero.xlsx`
- Create: `Client/Configs/GameConfig/Datas/hero_level.xlsx`

- [ ] Add hero and hero level table registrations.
- [ ] Add stage dynamic level fields: `levelMode`, `minEnemyLevel`, `maxEnemyLevel`, `baseLevelOffset`, `powerScale`.
- [ ] Add wave dynamic level fields: `levelMode`, `fixedEnemyLevel`, `levelOffset`, `minEnemyLevel`, `maxEnemyLevel`, `hpMultiplier`, `attackMultiplier`.
- [ ] Add skill fields: `resourceType`, `castMode`, `targetRule`, `threatMultiplier`.
- [ ] Add skill level fields: `resourceCost`, `resourceGain`.
- [ ] Add effect/buff data rows for stun, thunder attack-speed slow, heal, resurrect, rage gain, and extra threat.
- [ ] Add projectile rows for hero2 basic ranged, hero3 basic ranged, warrior charge/impact placeholders, heal pulse, and shockwave.

### Task 2: Generate config outputs and fix compile breaks

**Files:**
- Generated under: `Client/UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/`
- Generated under: `Client/UnityProject/Assets/AssetRaw/Configs/bytes/`

- [ ] Run from `Client`: `cmd /c "set AI_MODE=1 && Configs\GameConfig\gen_code_bin_to_project_lazyload.bat"`.
- [ ] Verify generated `GameConfig.stage.Stage`, `Wave`, `skill.Skill`, `skill.SkillLevel`, `stage.Hero`, `stage.HeroLevel` compile.
- [ ] Do not manually edit generated `.cs` files.

### Task 3: Add runtime progression and resources

**Files:**
- Modify: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DCombatUnit.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DCombatResource.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DHeroProgression.cs`
- Test: `Client/UnityProject/Assets/Editor/IDRPG3DGameplayPrototypeTests/IDRPG3DGameplayCoreTests.cs`

- [ ] Write failing tests for level metadata, resource spend/gain, and XP level-up.
- [ ] Add `Level`, `ExperienceReward`, and level change event support.
- [ ] Add resource component with `None`, `Rage`, `Mana`, max/current/regen, spend and gain APIs.
- [ ] Add hero progression component with current XP, required XP, level-up events, and stat refresh support.
- [ ] Run targeted tests.

### Task 4: Add multi-skill runtime and AI selection

**Files:**
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DPrototypeSkillRuntime.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DPrototypeSkillBook.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DPrototypeCombatDirector.cs`
- Modify: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DAutoCombatBrain.cs`
- Modify: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DPrototypeSkillCaster.cs`
- Test: `Client/UnityProject/Assets/Editor/IDRPG3DGameplayPrototypeTests/IDRPG3DGameplayCoreTests.cs`

- [ ] Write failing tests for skill book resource/cooldown checks and hero skill priorities.
- [ ] Implement runtime metadata: cast mode, target rule, resource type/cost/gain, threat multiplier.
- [ ] Implement AI rules:
  - Warrior: charge if out of melee and ready; thunder if rage >= 20 and nearby enemies; otherwise basic attack.
  - Mage: fireball, frostbolt, basic ranged.
  - Priest: resurrect dead ally, heal low ally, basic ranged.
- [ ] Keep existing single-skill caster path working for tests and fallback.

### Task 5: Load heroes and dynamic waves from config

**Files:**
- Modify: `Client/UnityProject/Assets/GameScripts/LocalTest/IDRPG3DLocalSkillConfigLoader.cs`
- Modify: `Client/UnityProject/Assets/GameScripts/LocalTest/IDRPG3DLocalWaveConfigLoader.cs`
- Modify: `Client/UnityProject/Assets/GameScripts/LocalTest/IDRPG3DLocalTestBootstrap.cs`
- Test: `Client/UnityProject/Assets/Editor/IDRPG3DGameplayPrototypeTests/IDRPG3DGameplayCoreTests.cs`

- [ ] Write failing tests for dynamic enemy level clamping.
- [ ] Load hero base stats from `hero.xlsx` and level growth from `hero_level.xlsx`.
- [ ] Remove hard-coded temporary hero HP/attack/regen values from bootstrap.
- [ ] Attach `IDRPG3DCombatResource`, `IDRPG3DHeroProgression`, and `IDRPG3DPrototypeSkillBook` from config.
- [ ] Compute stage/wave enemy level from active hero levels.
- [ ] Award XP to active heroes when enemies die.

### Task 6: World bar level/resource display

**Files:**
- Modify: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DWorldUnitBar.cs`
- Test: `Client/UnityProject/Assets/Editor/IDRPG3DGameplayPrototypeTests/IDRPG3DGameplayCoreTests.cs`

- [ ] Write failing test that a child named `Level` gets updated to `Lv.X`.
- [ ] Bind resource fill to `IDRPG3DCombatResource` when present.
- [ ] Subscribe to unit level/resource changes and refresh without per-frame expensive searches.

### Task 7: Verify and document VFX gaps

**Files:**
- Modify config rows only as needed.
- Final report in assistant response.

- [ ] Use Blink placeholders found:
  - `BearBossStun.prefab` for stun.
  - `Shockwave.prefab` for Thunder Clap.
  - `Heal_Pulse.prefab` for Heal.
  - `WarriorBuff.prefab` or `Enraged.prefab` for warrior/rage feedback.
  - Existing Frostbolt/Fireball effects for mage spells.
- [ ] Leave missing upgrade/resurrect effect paths empty or fallback.
- [ ] Report missing effect needs to the user.

### Task 8: Final verification

**Files:**
- All touched source/config files.

- [ ] Run config export after final config edits.
- [ ] Run `dotnet build Client/UnityProject/IDRPG3D.GameplayPrototype.Tests.csproj --no-restore`.
- [ ] Run `dotnet build Client/UnityProject/IDRPG3D.LocalTest.csproj --no-restore`.
- [ ] If Unity MCP is available, run relevant EditMode tests and read console.
- [ ] Summarize changes, tests, and known missing art assets.
