# Config-Driven Skill System Design

## Goal

Build the first practical skill configuration pipeline for the local combat prototype: Frostbolt and Fireball should be defined by Luban tables, converted into runtime skill definitions, and remain compatible with future Buff, Aura, control, knockback, and host-authoritative sync work.

## Scope

This first phase keeps the current Unity combat prototype intact and moves skill numbers and visual asset paths out of hardcoded constructors. It does not replace the current projectile, AI, damage, or animation systems yet. The design intentionally leaves server persistence and multiplayer transport for later phases, but every combat-facing config record uses stable IDs so future sync events can reference data deterministically.

## Tables

`skill.TbSkill` is the cast entry. It stores `id`, `skillKey`, display text, cooldown, range, target type, animation key, projectile ID, primary effect ID, and sync event key.

`skill.TbEffect` describes what happens when a skill lands. Phase one uses damage only; the fields also reserve buff, duration, radius, knockback distance, and control type.

`skill.TbBuff` stores persistent effects. Phase one seeds no runtime buff behavior, but the table shape supports stun, slow, attack speed modifiers, damage modifiers, periodic ticks, and aura radius.

`skill.TbProjectile` stores visual and ballistic projectile data: speed, hit radius, lifetime, prefab paths, muzzle path, impact path, and fallback color.

The tables live in `Client/Configs/GameConfig/Datas` and generate client config code under `Client/UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig`.

## Runtime Components

`IDRPG3DPrototypeSkillConfigRecord` is a small plain C# input record used by tests and by the Luban adapter. It avoids coupling gameplay prototype tests directly to generated Luban classes.

`IDRPG3DPrototypeSkillConfigBuilder` converts a record plus optional loaded prefab references into `IDRPG3DPrototypeSkillDefinition`. The builder clamps invalid numeric values through the existing definition constructor and keeps the runtime shape stable.

`IDRPG3DLocalTestBootstrap` resolves skill IDs for `Hero2` and `Hero3`. In the Unity Editor it will try to load Blink prefabs from config paths, then fall back to the existing built-in Frostbolt and Fireball definitions if generated config access is unavailable.

## Sync Model

Combat sync should use event/state replication, not full deterministic lockstep. Clients send player commands such as move, force attack, or cast. The host computes combat and broadcasts events with stable IDs:

- `CastSkill`: caster unit ID, target unit ID or position, skill ID, cast sequence.
- `SpawnProjectile`: projectile ID, source, target, spawn position, skill ID.
- `ApplyEffect`: effect ID, source, target, final numeric result.
- `ApplyBuff` / `RemoveBuff`: buff ID, target, stack, remaining time.
- `UnitDied`: unit ID and source.

Visual clients can play local prediction for responsiveness, but host events are the source of truth. Config version or hash checks should be added before online play so every peer uses the same skill/effect/buff data.

## Performance Notes

Runtime combat should resolve config IDs once and cache converted definitions. Per-frame combat should not use reflection, string path parsing, or table scans. Projectile prefabs should later be loaded and pooled through YooAsset or a battle asset manager; this phase keeps Editor-only direct asset loading for the local scene.

## Testing

Editor tests cover the config-to-runtime conversion and sync payload shape. The local test scene remains the manual integration check: `Hero2` casts Frostbolt, `Hero3` casts Fireball, enemies show thicker health bars, and the prototype still runs if config generation has not been refreshed.
