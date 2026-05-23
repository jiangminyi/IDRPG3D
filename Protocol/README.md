# Protocol

This directory is reserved for IDRPG3D network protocol definitions shared by the Unity client and Fantasy server.

Initial protocol work should stay small:

- `Login`
- `EnterWorld`
- `StartIdleBattle`
- `BattleRewardPush`
- `TeamCreate`
- `TeamJoin`

Fantasy owns the protocol export/generation pipeline. TEngine client code should consume generated protocol code through a game-facing network facade, such as `GameNetworkService`, instead of calling framework internals from gameplay/UI code.
