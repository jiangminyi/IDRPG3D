# Core Gameplay Prototype Design

Created: 2026-05-25

## Goal

Build the first local Unity gameplay loop for IDRPG3D:

1. A team anchor advances along the authored Dreamteck spline route.
2. Heroes use `NavMeshAgent` to move toward RTS-style formation slots around that anchor.
3. Nearby enemies interrupt route movement and trigger automatic combat.
4. Movement, attack, and death animations are driven through the existing Blink animator parameters.
5. The architecture remains small enough for the current `Hero1` + `Enemy` scene, but leaves room for 1-5 heroes locally and 15 heroes in host-authoritative co-op later.

## Current Phase

This prototype is client-side only. The real backend remains responsible for account/player persistence. Dungeon combat will later use a host-authoritative model:

- The host player simulates combat.
- Non-host players send commands for their own heroes.
- The backend records results after lightweight validation.
- The backend does not run full frame-by-frame battle simulation.

## Movement Design

The spline is not attached directly to heroes. It drives a team anchor.

The team anchor samples the route by percent and advances by speed. Each hero receives a formation destination based on:

- anchor position
- route forward direction
- movement priority
- stable team order

Heroes move to their own formation destinations through `NavMeshAgent`. Destination updates are throttled so we do not call `SetDestination` every frame.

Movement priority is sorted descending:

- tank / front melee: `100`
- normal melee: `70`
- support / middle: `40`
- ranged / caster: `10`

For equal priority, team order is used as a stable tie-breaker to avoid slot jitter.

## Combat Design

The first combat loop uses a lightweight AI:

1. Heroes follow the route.
2. The route controller checks enemies at a low frequency.
3. If an alive enemy is inside detection range, heroes switch to combat.
4. Each hero chases the target until in attack range.
5. The hero stops, faces the target, plays an attack trigger, and applies damage on the attack tick.
6. Damage adds threat to the defender threat table.
7. Enemies choose the highest-threat alive target and fight back.
8. When all nearby enemies are dead, heroes project back to the nearest route point and resume route movement.

## Animation Design

The prototype drives existing Blink-style parameters when present:

- `HorizontalSpeed`
- `VerticalSpeed`
- `MoveDirectionX`
- `MoveDirectionY`
- `MoveSpeedModifier`
- `IsGrounded`
- `AttackAnimationsSpeed`
- `MeleeAttack1`
- `Dead`

Missing parameters are ignored to keep the scripts reusable with other controllers.

Mount animation is not implemented in this phase. The movement code will keep animation control behind a bridge so mount-specific animation can be added without rewriting movement/combat.

## Performance Rules

- Enemy scanning is ticked, not per-frame.
- Formation destinations are recalculated at a fixed interval.
- `NavMeshAgent.SetDestination` is throttled by time and minimum position delta.
- Animator parameter hashes are cached.
- Runtime lists are reused.
- No LINQ in hot gameplay loops.
- No `FindObjectOfType` in per-frame code.

## First Scene Scope

The first implementation targets `Assets/Scenes/IDRPG3D_LocalTest.unity`.

Expected scene objects:

- `Route_Main_01` or any `SplineComputer`
- `Hero1`
- `Enemy`

The local bootstrap attaches prototype components at runtime so the scene can be tested without manually wiring every component.

