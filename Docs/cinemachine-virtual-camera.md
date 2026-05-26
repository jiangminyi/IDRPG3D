# Cinemachine VirtualCamera 使用说明

当前 Unity 工程使用 `com.unity.cinemachine` `2.9.7`。在本地战斗测试场景里，Cinemachine 只负责相机表现，角色中心点由玩法脚本维护。

## 当前对象分工

| 对象 | 作用 |
| --- | --- |
| `Main Camera` | 真正渲染画面的 Unity Camera，需要挂 `CinemachineBrain`。 |
| `VirtualCamera1` / `VirtualCamera2` | Cinemachine 虚拟相机，负责镜头位置、构图、阻尼、镜头切换。 |
| `CameraTarget` | 镜头跟随目标点，由 `IDRPG3DCameraTargetFollower` 在运行时移动到所有存活角色中心。 |

`CameraTarget` 的旋转不会被脚本修改。你可以在场景里调它的旋转，或直接调 VirtualCamera 的 Aim/Body 参数。

## 推荐设置

1. 选中 `Main Camera`，确认有 `CinemachineBrain`。
2. 选中当前要使用的 `VirtualCamera`。
3. 将 `Follow` 拖成场景里的 `CameraTarget`。
4. 如果希望镜头始终看向队伍中心，将 `Look At` 也拖成 `CameraTarget`。
5. `Body` 推荐先用 `Transposer` 或 `Framing Transposer`。
6. `Aim` 推荐先用 `Composer` 或 `Hard Look At`。如果你想完全手动固定角度，可以用 `Do Nothing`。
7. 多个 VirtualCamera 同时存在时，用 `Priority` 决定谁生效，数值更高的一般会成为当前镜头。

## 本项目运行时行为

`IDRPG3DLocalTestBootstrap` 现在会从以下位置加载测试角色 prefab：

- `Assets/AssetRaw/Actor/Hero.prefab`
- `Assets/AssetRaw/Actor/Enemy.prefab`

进入 Play Mode 后，脚本会生成 `Hero1`、`Hero2`、`Hero3`，后续波次怪物也会从 `Enemy.prefab` 实例化。`CameraTarget` 会跟踪这些存活角色的中心点，所以 VirtualCamera 不需要直接跟随某个英雄。

如果场景里没有 `CameraTarget`，脚本会临时创建一个兜底对象；正式调镜头时建议还是在场景里保留你自己创建的 `CameraTarget`。

## 调试建议

- 调镜头构图时，可以在 Play Mode 中开启 Cinemachine 的 `Save During Play`，但退出前要确认改动已经保存到场景。
- 如果镜头没有跟随，先检查 `Main Camera` 是否有 `CinemachineBrain`，再检查当前最高 `Priority` 的 VirtualCamera 是否设置了 `Follow`。
- 如果镜头角度突然变化，检查 VirtualCamera 的 `Aim` 是否设置为 `Same As Follow Target`。当前 `CameraTarget` 只移动不旋转，这种模式通常不适合本项目的俯视/斜俯视挂机镜头。
