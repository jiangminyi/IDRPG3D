# Skill Level And Effects Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add stable skill IDs with configurable levels, multi-effect skills, healing, slow, damage-over-time, armor buffs, and a paladin-style armor aura to the local prototype.

**Architecture:** Keep `skill.xlsx` as the stable skill identity table and add `skill_level.xlsx` for level-specific range, cooldown, projectile, and visual fields. Use `skill_effect.xlsx` to map `(skillId, level)` to ordered effects, while `effect.xlsx` and `buff.xlsx` describe concrete damage, healing, debuff, DoT, and aura behavior. The runtime remains prototype-scoped and applies a small effect list per cast.

**Tech Stack:** Unity C#, NUnit editor tests, Luban Excel configs, TEngine-generated GameConfig code.

---

### Task 1: Runtime Tests

**Files:**
- Modify: `Client/UnityProject/Assets/Editor/IDRPG3DGameplayPrototypeTests/IDRPG3DGameplayCoreTests.cs`

- [ ] Add tests for level-aware skill records producing multiple ordered effects.
- [ ] Add tests for heal effects restoring health without overhealing.
- [ ] Add tests for armor reducing incoming damage.
- [ ] Add tests for DoT buffs ticking damage.
- [ ] Add tests for aura buffs applying armor to nearby allies.

### Task 2: Runtime Implementation

**Files:**
- Modify: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DCombatUnit.cs`
- Modify: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DPrototypeEffectRuntime.cs`
- Modify: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DPrototypeSkillDefinition.cs`
- Modify: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DPrototypeSkillConfigRecord.cs`
- Modify: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DPrototypeSkillConfigBuilder.cs`
- Modify: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DPrototypeProjectile.cs`

- [ ] Add health restoration and armor-aware damage to combat units.
- [ ] Add effect lists, heal effects, add-buff effects, DoT ticks, armor modifiers, and aura ticking.
- [ ] Keep existing single-effect constructors working for old tests and fallback skills.

### Task 3: Luban Tables And Loader

**Files:**
- Modify: `Client/Configs/GameConfig/Datas/__tables__.xlsx`
- Modify: `Client/Configs/GameConfig/Datas/skill.xlsx`
- Create: `Client/Configs/GameConfig/Datas/skill_level.xlsx`
- Modify: `Client/Configs/GameConfig/Datas/skill_effect.xlsx`
- Modify: `Client/Configs/GameConfig/Datas/effect.xlsx`
- Modify: `Client/Configs/GameConfig/Datas/buff.xlsx`
- Generate: `Client/UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/**`
- Generate: `Client/UnityProject/Assets/AssetRaw/Configs/bytes/**`
- Modify: `Client/UnityProject/Assets/GameScripts/LocalTest/IDRPG3DLocalSkillConfigLoader.cs`
- Modify: `Client/UnityProject/Assets/GameScripts/LocalTest/IDRPG3DLocalTestBootstrap.cs`

- [ ] Add `skill.TbSkillLevel` and data rows for level 1 sample skills.
- [ ] Configure Frostbolt slow, Fireball burn, Heal, and Paladin armor aura.
- [ ] Regenerate Luban code and bytes.
- [ ] Load skills by `(skillId, level)` and collect ordered effects from `skill_effect`.
- [ ] Bind Hero1 to aura/heal prototype behavior and keep Hero2/Hero3 using Frostbolt/Fireball.

### Task 4: Verification And Commit

**Files:**
- Review all modified files.

- [ ] Run config validation.
- [ ] Run gameplay prototype tests build.
- [ ] Run local test build.
- [ ] Commit the completed slice.
