# Prototype Spells And Health Bars Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Hero2 frostbolt, Hero3 fireball, multi-enemy wiring, and thicker enemy health bars to the local combat prototype.

**Architecture:** Keep gameplay logic in `Assets/GameScripts/GameplayPrototype` and only reference Blink prefabs from the local test/editor wiring. Skills are small serializable runtime components that spawn a projectile and apply damage on impact; health bars subscribe to `IDRPG3DCombatUnit` events and face the camera.

**Tech Stack:** Unity 2022.3, C#, Animancer, NavMeshAgent, uGUI world-space Canvas, Blink RPGBuilder visual prefabs.

---

### Task 1: Runtime Skill And Health Bar Components

**Files:**
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DPrototypeSkillDefinition.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DPrototypeProjectile.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DPrototypeSkillCaster.cs`
- Create: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DWorldHealthBar.cs`
- Modify: `Client/UnityProject/Assets/GameScripts/GameplayPrototype/IDRPG3DAutoCombatBrain.cs`
- Test: `Client/UnityProject/Assets/Editor/IDRPG3DGameplayPrototypeTests/IDRPG3DGameplayCoreTests.cs`

- [ ] Write failing tests for projectile spawning, impact damage, and health bar fill.
- [ ] Implement skill definition, caster, projectile movement/impact, and health bar update.
- [ ] Run `dotnet build Client/UnityProject/IDRPG3D.GameplayPrototype.Tests.csproj`.

### Task 2: Local Test Scene Wiring

**Files:**
- Modify: `Client/UnityProject/Assets/GameScripts/LocalTest/IDRPG3DLocalTestBootstrap.cs`

- [ ] Find all named heroes: `Hero1`, `Hero2`, `Hero3`.
- [ ] Find all named enemies: `Enemy`, `Enemy1`, `Enemy2`, `Enemy3`.
- [ ] Configure Hero2 with frostbolt and Hero3 with fireball, using Blink projectile/muzzle/hit prefab paths when running in Editor.
- [ ] Add thicker enemy health bars to all enemy units.
- [ ] Run `dotnet build Client/UnityProject/IDRPG3D.LocalTest.csproj`.
