# Wave Spawning Design

**Goal:** Build the first version of the idle wave loop: normal waves spawn ahead on the route, boss waves spawn at fixed scene anchors.

## Design

The wave system uses two spawn positioning rules.

- `SplineAhead`: normal monsters spawn at a distance in front of the team route anchor. This keeps the map from filling with enemies and makes the team feel like it is pushing forward through encounters.
- `FixedAnchor`: boss monsters spawn at a hand-placed scene anchor. This lets boss fights match terrain, props, arena layout, and later camera direction.

Waves progress only after all living enemies from the current wave are defeated. After a short delay, the next wave begins. A stage can loop after the final wave for idle farming.

## Config

The first config pass adds:

- `stage.xlsx`: stage id, route key, loop mode, first wave group.
- `enemy.xlsx`: enemy identity, model key/path, base combat stats.
- `enemy_level.xlsx`: level multipliers for enemy strength.
- `wave.xlsx`: wave order, spawn mode, spawn ahead distance or fixed anchor id, enemy id, enemy level, count, formation radius, boss flag, next-wave delay.

The stable data for synchronization is `stageId`, `waveIndex`, `enemyId`, `enemyLevel`, `spawnMode`, `spawnAnchorId`, `spawnDistanceAhead`, and a future `spawnSeed`.

## Runtime

`IDRPG3DWaveController` owns wave state. It asks a provider for wave definitions, resolves a spawn center through `IDRPG3DWaveSpawnResolver`, spawns enemies through an `IDRPG3DEnemyFactory`, tracks current-wave units, and advances when all tracked enemies are dead.

`IDRPG3DSpawnAnchor` is a scene component with a stable `anchorId`. Boss waves reference that id.

The LocalTest scene will keep using the existing `Hero1/Hero2/Hero3` setup. Existing scene enemies can be used as templates, hidden at startup, and cloned by the wave spawner.

## First Slice

The first playable slice implements:

- Normal wave position calculation from current route anchor.
- Fixed boss anchor lookup.
- Wave state progression after enemy death.
- LocalTest integration using scene enemy templates.

Later slices can add side ambush, off-camera spawn, drop tables, elite affixes, spawn seeds, and camera control.
