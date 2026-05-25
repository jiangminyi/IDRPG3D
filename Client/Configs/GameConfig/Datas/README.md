# IDRPG3D 配置表说明

这里是项目的 Luban 配置源表目录。策划和开发需要改配置时，优先改这里的 `.xlsx`，然后运行导表脚本生成 Unity 可读取的代码和 bytes。

## 原型阶段先改哪几张

技能系统目前先看这几张：

| 文件 | 用途 | 当前建议 |
| --- | --- | --- |
| `skill.xlsx` | 技能主表，一个技能一行 | 最常改。配置技能 ID、显示名、描述、冷却、距离、动画 Key，并引用效果和投射物 |
| `effect.xlsx` | 技能命中后的逻辑效果 | 配伤害、治疗、控制、加 Buff、击退等。当前寒冰箭/火球术先用 Damage |
| `projectile.xlsx` | 投射物和特效表现 | 配飞行速度、飞行 Prefab、出手特效、命中特效、备用颜色 |
| `buff.xlsx` | Buff/光环/持续效果预留 | 后续做减速、眩晕、灼烧、光环时再重点改 |
| `skill_effect.xlsx` | 一个技能多个效果的高级关系表 | 当前原型先预留；运行时主要读取 `skill.xlsx` 的 `effectId` |

也就是说，现在想加一个简单技能，通常只需要：

1. 在 `skill.xlsx` 新增技能。
2. 在 `effect.xlsx` 新增或复用一个效果。
3. 在 `projectile.xlsx` 新增或复用一个投射物表现。
4. 回到 `skill.xlsx` 填好 `effectId` 和 `projectileId`。

## 表头行规则

Luban 表前几行不是普通数据，不能随便删：

| 行 | 含义 |
| --- | --- |
| 第 1 行 `##var` | 字段名，程序生成代码会用 |
| 第 2 行 `##type` | 字段类型，例如 `int`、`string`、`float` |
| 第 3 行 `##group` | 分组，`c` 客户端，`s` 服务器，`c,s` 两端 |
| 第 4 行 `##` | 中文说明，给人看的 |
| 第 5 行开始 | 真正的数据 |

## 导表

从 `Client` 目录运行：

```bat
cmd /c "set AI_MODE=1 && Configs\GameConfig\gen_code_bin_to_project_lazyload.bat"
```

生成结果会进入：

| 位置 | 内容 |
| --- | --- |
| `Client/UnityProject/Assets/GameScripts/HotFix/GameProto/GameConfig/` | Luban 生成的 C# 配置代码 |
| `Client/UnityProject/Assets/AssetRaw/Configs/bytes/` | Unity 运行时读取的配置 bytes |

不要手改生成目录里的代码；要改配置就改这里的 Excel 源表。
