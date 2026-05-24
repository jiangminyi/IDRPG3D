using System.Collections.Generic;

namespace IDRPG3D.EditorTools
{
    public sealed class IDRPG3DRouteChineseGuideSection
    {
        public IDRPG3DRouteChineseGuideSection(string title, params IDRPG3DRouteChineseGuideEntry[] entries)
        {
            Title = title;
            Entries = entries;
        }

        public string Title { get; }
        public IReadOnlyList<IDRPG3DRouteChineseGuideEntry> Entries { get; }
    }

    public sealed class IDRPG3DRouteChineseGuideEntry
    {
        public IDRPG3DRouteChineseGuideEntry(string english, string chinese, string description)
        {
            English = english;
            Chinese = chinese;
            Description = description;
        }

        public string English { get; }
        public string Chinese { get; }
        public string Description { get; }
    }

    public static class IDRPG3DRouteChineseGuide
    {
        public static readonly IReadOnlyList<IDRPG3DRouteChineseGuideSection> Sections = new[]
        {
            new IDRPG3DRouteChineseGuideSection(
                "Spline Computer 主面板",
                new IDRPG3DRouteChineseGuideEntry("Close / Break", "闭合 / 断开", "把路线首尾相接，或把闭合路线重新断开。挂机循环路线通常要闭合。"),
                new IDRPG3DRouteChineseGuideEntry("Reverse", "反转方向", "交换路线起点和终点，角色沿线移动的方向会反过来。"),
                new IDRPG3DRouteChineseGuideEntry("2D Mode / 3D Mode", "2D / 3D 模式", "2D 会把点限制到一个平面；当前 3D Terrain 路线应使用 3D 模式。"),
                new IDRPG3DRouteChineseGuideEntry("Type", "曲线类型", "CatmullRom 适合手摆路线；Bezier 适合精修；Linear 是直线折线；BSpline 是平滑但不一定经过控制点。"),
                new IDRPG3DRouteChineseGuideEntry("Catmull Rom Type", "CatmullRom 参数", "控制 CatmullRom 转弯方式。默认值通常够用。"),
                new IDRPG3DRouteChineseGuideEntry("Linear Average Direction", "线性平均方向", "Linear 模式下平滑方向采样，用于让方向变化不那么突兀。"),
                new IDRPG3DRouteChineseGuideEntry("Space", "坐标空间", "World 是世界坐标，Local 是相对路线物体自身坐标。路线编辑建议 World。"),
                new IDRPG3DRouteChineseGuideEntry("Sample Mode", "采样模式", "Default 默认采样；Uniform 按距离更均匀；Optimized 减少不必要采样。角色匀速行走常配 Uniform。"),
                new IDRPG3DRouteChineseGuideEntry("Optimize Angle Threshold", "优化角度阈值", "Optimized 采样时使用，角度变化小的段会减少采样。"),
                new IDRPG3DRouteChineseGuideEntry("Update Mode", "更新模式", "控制 SplineComputer 何时更新。编辑期一般默认即可。"),
                new IDRPG3DRouteChineseGuideEntry("Rebuild", "重建路线", "手动刷新采样缓存。路线显示异常时可点一次。"),
                new IDRPG3DRouteChineseGuideEntry("Sample Rate", "采样精度", "数值越高曲线越细腻，计算越多。普通路线默认或略高即可。"),
                new IDRPG3DRouteChineseGuideEntry("Multithreaded", "多线程", "使用多线程更新采样。复杂路线很多时可考虑，早期保持关闭更直观。"),
                new IDRPG3DRouteChineseGuideEntry("Point Value Interpolation", "点数值插值", "控制点的 Size、Color、Normal 在曲线上的过渡方式。做宽度/颜色变化时才需要。"),
                new IDRPG3DRouteChineseGuideEntry("Size & Color Interpolation", "尺寸和颜色插值", "自定义 Size、Color 沿路线变化的曲线。"),
                new IDRPG3DRouteChineseGuideEntry("Normal Interpolation", "法线插值", "自定义法线沿路线变化方式，常用于生成管线、面片或朝向。")
            ),
            new IDRPG3DRouteChineseGuideSection(
                "Edit 路线点编辑",
                new IDRPG3DRouteChineseGuideEntry("Edit", "进入编辑", "打开 Dreamteck 的点编辑模式，Scene 视图会显示控制点和工具。"),
                new IDRPG3DRouteChineseGuideEntry("Point Operations", "点操作", "对当前选中的控制点批量执行操作，例如自动切线、翻转切线等。"),
                new IDRPG3DRouteChineseGuideEntry("Apply", "应用", "执行当前选择的点操作。"),
                new IDRPG3DRouteChineseGuideEntry("Coordinate Space", "坐标空间", "控制点数值显示/编辑使用世界坐标还是局部坐标。"),
                new IDRPG3DRouteChineseGuideEntry("Position", "位置", "控制点的位置。路线会经过或靠近这些点。"),
                new IDRPG3DRouteChineseGuideEntry("Tangent", "切线 1", "Bezier 点的前/后控制柄之一，影响曲线弯曲。CatmullRom 通常不用手调。"),
                new IDRPG3DRouteChineseGuideEntry("Tangent 2", "切线 2", "Bezier 点的另一个控制柄。"),
                new IDRPG3DRouteChineseGuideEntry("Normal", "法线", "点的上方向，影响生成几何、物体朝向等高级用途。"),
                new IDRPG3DRouteChineseGuideEntry("Size", "尺寸", "点的大小数值，可被生成器或采样用户读取。"),
                new IDRPG3DRouteChineseGuideEntry("Color", "颜色", "点颜色，可被渲染或采样用户读取。"),
                new IDRPG3DRouteChineseGuideEntry("Point Type", "点类型", "Bezier 点切线关系：镜像、自由平滑、断开。")
            ),
            new IDRPG3DRouteChineseGuideSection(
                "点选择 / 点批量操作",
                new IDRPG3DRouteChineseGuideEntry("Select", "选择点", "下拉选择当前控制点，也可以选择 All、None、Inverse。"),
                new IDRPG3DRouteChineseGuideEntry("All", "全选", "选中路线上的全部控制点。"),
                new IDRPG3DRouteChineseGuideEntry("None", "清空选择", "取消所有控制点选择。"),
                new IDRPG3DRouteChineseGuideEntry("Inverse", "反选", "把当前未选中的点选中，把已选中的点取消。"),
                new IDRPG3DRouteChineseGuideEntry("Flat X / Flat Y / Flat Z", "压平成 X/Y/Z 平面", "把选中点在某个轴向上的坐标统一，用来快速拉平成一条平面路线。"),
                new IDRPG3DRouteChineseGuideEntry("Mirror X / Mirror Y / Mirror Z", "按 X/Y/Z 镜像", "把选中点沿指定轴做镜像，常用于快速做对称路线。"),
                new IDRPG3DRouteChineseGuideEntry("Distribute Evenly", "均匀分布", "让选中控制点按路径顺序更均匀地分布。"),
                new IDRPG3DRouteChineseGuideEntry("Auto Bezier Tangents", "自动贝塞尔切线", "自动计算 Bezier 点切线，让曲线过渡更平滑。"),
                new IDRPG3DRouteChineseGuideEntry("Swap Bezier Tangents", "交换贝塞尔切线", "交换选中点的两根切线。"),
                new IDRPG3DRouteChineseGuideEntry("Flip Bezier Tangents", "翻转贝塞尔切线", "翻转选中点切线方向，用于修正曲线走向。"),
                new IDRPG3DRouteChineseGuideEntry("Center To Transform", "选中点居中到物体", "把选中点移动到 SplineComputer 物体 Transform 附近。"),
                new IDRPG3DRouteChineseGuideEntry("Move Transform To", "物体移动到选中点", "把 SplineComputer 物体 Transform 移到选中点位置。")
            ),
            new IDRPG3DRouteChineseGuideSection(
                "创建点工具",
                new IDRPG3DRouteChineseGuideEntry("Placement Mode", "放置模式", "新点如何落位：平面、相机平面、表面或插入。"),
                new IDRPG3DRouteChineseGuideEntry("YPlane / XPlane / ZPlane", "Y/X/Z 平面", "把鼠标投射到固定轴向平面上创建点。"),
                new IDRPG3DRouteChineseGuideEntry("CameraPlane", "相机平面", "按 Scene 视图相机朝向创建点。"),
                new IDRPG3DRouteChineseGuideEntry("Surface", "表面", "通过射线把点放到碰撞体表面；Terrain 需要 Collider 和正确 Layer。"),
                new IDRPG3DRouteChineseGuideEntry("Insert", "插入", "在已有曲线段中间插入点。"),
                new IDRPG3DRouteChineseGuideEntry("Normal Mode", "法线模式", "新点法线如何生成。通常保持默认。"),
                new IDRPG3DRouteChineseGuideEntry("Default / LookAtCamera / AlignWithCamera", "默认 / 看向相机 / 对齐相机", "创建点法线的相机相关选项，主要影响生成几何或朝向。"),
                new IDRPG3DRouteChineseGuideEntry("Calculate / Left / Right / Up / Down / Forward / Back", "计算 / 固定方向", "创建点法线可以自动计算，也可以强制为某个世界方向。"),
                new IDRPG3DRouteChineseGuideEntry("Append To", "追加到", "新点加在路线开头还是结尾。"),
                new IDRPG3DRouteChineseGuideEntry("Beginning / End", "开头 / 结尾", "新点插入路线起点前，或追加到路线终点后。"),
                new IDRPG3DRouteChineseGuideEntry("Offset / Surface Offset", "偏移 / 表面偏移", "创建点时离平面或表面的高度偏移。"),
                new IDRPG3DRouteChineseGuideEntry("Far Plane", "相机远平面距离", "CameraPlane 模式下，鼠标点会落在距离 Scene 相机多远的平面上。"),
                new IDRPG3DRouteChineseGuideEntry("Create Node", "创建节点", "新增控制点时同时创建 Node，适合以后做路线连接网络。")
            ),
            new IDRPG3DRouteChineseGuideSection(
                "移动 / 旋转 / 缩放点工具",
                new IDRPG3DRouteChineseGuideEntry("Edit Space", "编辑空间", "移动、旋转、缩放时使用世界轴、本地轴或其他空间。"),
                new IDRPG3DRouteChineseGuideEntry("World / Transform / Spline", "世界 / 物体 / 曲线空间", "World 用全局轴；Transform 用路线物体轴；Spline 会按曲线采样朝向作为编辑空间。"),
                new IDRPG3DRouteChineseGuideEntry("Move On Surface", "沿表面移动", "拖点时尝试吸附到表面。大 Terrain 上可配合我们的自动贴地模式使用。"),
                new IDRPG3DRouteChineseGuideEntry("Surface Mask", "表面层遮罩", "指定哪些 Layer 可以被表面射线命中。"),
                new IDRPG3DRouteChineseGuideEntry("Surface Offset", "表面偏移", "点吸附到表面后的离地高度。"),
                new IDRPG3DRouteChineseGuideEntry("Snap to Grid", "吸附网格", "拖点时按网格单位对齐。"),
                new IDRPG3DRouteChineseGuideEntry("Grid Size", "网格大小", "网格吸附的单位距离。"),
                new IDRPG3DRouteChineseGuideEntry("Rotate Normals", "旋转法线", "旋转点时同时旋转法线。"),
                new IDRPG3DRouteChineseGuideEntry("Rotate Tangents", "旋转切线", "旋转点时同时旋转 Bezier 切线。"),
                new IDRPG3DRouteChineseGuideEntry("Scale Sizes", "缩放尺寸", "缩放点时同时改变 Size。"),
                new IDRPG3DRouteChineseGuideEntry("Scale Tangents", "缩放切线", "缩放点时同时缩放 Bezier 切线。")
            ),
            new IDRPG3DRouteChineseGuideSection(
                "法线 / 镜像 / 删除 / 分割",
                new IDRPG3DRouteChineseGuideEntry("Normal Mode", "法线模式", "编辑点法线的方式。2D 模式下不可用。"),
                new IDRPG3DRouteChineseGuideEntry("Normal Operations", "法线操作", "批量处理选中点法线。"),
                new IDRPG3DRouteChineseGuideEntry("Rotate Normal", "旋转法线", "按角度旋转选中点的法线。"),
                new IDRPG3DRouteChineseGuideEntry("Brush Radius", "刷子半径", "删除点工具的点击影响范围。"),
                new IDRPG3DRouteChineseGuideEntry("Axis", "轴向", "镜像操作使用的轴。"),
                new IDRPG3DRouteChineseGuideEntry("Flip", "翻转", "镜像时是否翻转方向。"),
                new IDRPG3DRouteChineseGuideEntry("Weld Distance", "焊接距离", "镜像后距离足够近的点会合并。"),
                new IDRPG3DRouteChineseGuideEntry("Center", "中心", "镜像操作的中心点。"),
                new IDRPG3DRouteChineseGuideEntry("Split", "分割", "在曲线上点击位置，把一条路线切成两段。")
            ),
            new IDRPG3DRouteChineseGuideSection(
                "节点 Nodes",
                new IDRPG3DRouteChineseGuideEntry("Nodes", "节点", "节点可以把多个 Spline 的点连接起来，让多条路线共享连接点。"),
                new IDRPG3DRouteChineseGuideEntry("Select", "选择", "选择节点或节点连接。"),
                new IDRPG3DRouteChineseGuideEntry("Delete", "删除", "删除节点或连接。"),
                new IDRPG3DRouteChineseGuideEntry("Disconnect", "断开连接", "让控制点不再连接到节点。"),
                new IDRPG3DRouteChineseGuideEntry("Add Node to Point", "给点添加节点", "把选中控制点变成可连接节点。"),
                new IDRPG3DRouteChineseGuideEntry("Add Nodes to Points", "给多个点添加节点", "批量给选中点添加节点。"),
                new IDRPG3DRouteChineseGuideEntry("Create Node", "创建节点", "创建点时同步创建节点。"),
                new IDRPG3DRouteChineseGuideEntry("Merge", "合并路线", "将当前路线端点与其他路线端点合并。"),
                new IDRPG3DRouteChineseGuideEntry("Merge Endpoints", "合并端点", "合并路线时是否把端点焊接到一起。")
            ),
            new IDRPG3DRouteChineseGuideSection(
                "Node 节点组件 Inspector",
                new IDRPG3DRouteChineseGuideEntry("Connections", "连接列表", "显示当前 Node 连接了哪些 SplineComputer 和控制点。"),
                new IDRPG3DRouteChineseGuideEntry("Settings", "节点设置", "Node 的同步规则和节点类型设置。"),
                new IDRPG3DRouteChineseGuideEntry("Link point", "连接控制点", "把一个 SplineComputer 的某个控制点连接到当前 Node。"),
                new IDRPG3DRouteChineseGuideEntry("No Points Available", "没有可连接控制点", "当前拖入的路线没有可用于连接的点。"),
                new IDRPG3DRouteChineseGuideEntry("Connection already exists", "连接已存在", "这个 Node 已经连接过指定路线点，不能重复连接。"),
                new IDRPG3DRouteChineseGuideEntry("Drag & Drop SplineComputers here", "拖入路线组件到这里", "把带 SplineComputer 的对象拖到 Connections 区域可建立连接。"),
                new IDRPG3DRouteChineseGuideEntry("Transform Normals", "同步法线", "移动 Node 时同步更新连接点的法线。"),
                new IDRPG3DRouteChineseGuideEntry("Transform Size", "同步尺寸", "移动或缩放 Node 时同步连接点的 Size。"),
                new IDRPG3DRouteChineseGuideEntry("Transform Tangents", "同步切线", "移动 Node 时同步 Bezier 切线，避免连接处断裂。"),
                new IDRPG3DRouteChineseGuideEntry("Node Type", "节点类型", "控制 Node 与连接点的约束方式。普通路线连接先保持默认。"),
                new IDRPG3DRouteChineseGuideEntry("Align Tangents X / Y / Z", "按 X/Y/Z 对齐切线", "把连接点切线沿 Node 自身某个轴向对齐。"),
                new IDRPG3DRouteChineseGuideEntry("Swap Tangents", "交换切线", "反转某条连接上的切线关系，修正连接处曲线方向。")
            ),
            new IDRPG3DRouteChineseGuideSection(
                "触发器 Triggers",
                new IDRPG3DRouteChineseGuideEntry("New Group", "新建组", "创建一组路线触发器。"),
                new IDRPG3DRouteChineseGuideEntry("Add Trigger", "添加触发器", "在当前组里新增一个路线触发点。"),
                new IDRPG3DRouteChineseGuideEntry("Position", "位置百分比", "触发器在路线上的位置，0 是起点，1 是终点。"),
                new IDRPG3DRouteChineseGuideEntry("Set Distance", "按距离设置", "用路线距离而不是百分比设置触发位置。"),
                new IDRPG3DRouteChineseGuideEntry("Type", "触发类型", "Double 双向触发；Forward 正向触发；Backward 反向触发。"),
                new IDRPG3DRouteChineseGuideEntry("Work Once", "只触发一次", "触发后不再重复执行。"),
                new IDRPG3DRouteChineseGuideEntry("Event", "事件", "触发时调用的 UnityEvent。"),
                new IDRPG3DRouteChineseGuideEntry("Rename", "重命名", "修改触发器或组名称。"),
                new IDRPG3DRouteChineseGuideEntry("Duplicate", "复制", "复制当前触发器。"),
                new IDRPG3DRouteChineseGuideEntry("Move Up / Move Down", "上移 / 下移", "调整触发器排序。")
            ),
            new IDRPG3DRouteChineseGuideSection(
                "Debug / Scene 显示",
                new IDRPG3DRouteChineseGuideEntry("Editor Update Mode", "编辑器更新模式", "Default 实时更新；OnMouseUp 鼠标松开后再更新，复杂路线更省性能。"),
                new IDRPG3DRouteChineseGuideEntry("Color in Scene", "Scene 中颜色", "路线在 Scene 视图里的显示颜色。"),
                new IDRPG3DRouteChineseGuideEntry("Draw Transform Pivot", "绘制物体轴心", "显示 SplineComputer 物体自身 Transform 的轴心。"),
                new IDRPG3DRouteChineseGuideEntry("Always Draw Spline", "始终绘制路线", "即使没有选中对象也显示路线。"),
                new IDRPG3DRouteChineseGuideEntry("Draw thickness", "绘制厚度", "让 Scene 视图中的线带有可见厚度。"),
                new IDRPG3DRouteChineseGuideEntry("Always face camera", "始终面向相机", "厚度显示始终朝向 Scene 相机。"),
                new IDRPG3DRouteChineseGuideEntry("Samples", "采样数量", "当前路线被采样出的点数。"),
                new IDRPG3DRouteChineseGuideEntry("Length", "路线长度", "当前曲线总长度。")
            ),
            new IDRPG3DRouteChineseGuideSection(
                "SplineUser / Follower 常用组件",
                new IDRPG3DRouteChineseGuideEntry("SplineUser", "路线使用者基类", "所有沿路线采样、生成或移动的组件基础。通常不用直接挂，更多使用子类。"),
                new IDRPG3DRouteChineseGuideEntry("SplineFollower", "路线跟随器", "让角色或物体沿 SplineComputer 自动移动。后面挂机巡逻路线会优先用它验证表现。"),
                new IDRPG3DRouteChineseGuideEntry("Spline", "引用路线", "SplineUser 要使用的 SplineComputer。没有引用时组件不会工作。"),
                new IDRPG3DRouteChineseGuideEntry("User Configuration", "使用者配置", "组件更新、重建、多线程等基础配置折叠区。"),
                new IDRPG3DRouteChineseGuideEntry("Auto Rebuild", "自动重建", "路线变化时自动重新构建采样或生成结果。编辑期一般打开更直观。"),
                new IDRPG3DRouteChineseGuideEntry("Build On Awake", "Awake 时构建", "运行时对象 Awake 阶段构建数据。"),
                new IDRPG3DRouteChineseGuideEntry("Build On Enable", "启用时构建", "组件启用时构建数据。反复开关对象时会重新生效。"),
                new IDRPG3DRouteChineseGuideEntry("Clip From / Clip To", "裁剪起点 / 终点", "限制组件只使用路线的一部分，数值范围 0 到 1。"),
                new IDRPG3DRouteChineseGuideEntry("Clip Range", "裁剪范围", "用滑条同时设置 Clip From 和 Clip To。"),
                new IDRPG3DRouteChineseGuideEntry("Loop Samples", "循环采样", "闭合路线时让采样首尾更平滑。"),
                new IDRPG3DRouteChineseGuideEntry("Sample Modifiers", "采样修改器", "对采样结果做额外修改，例如颜色、尺寸、偏移、速度等。"),
                new IDRPG3DRouteChineseGuideEntry("Following", "跟随设置", "SplineFollower 的移动开关、模式、速度和循环行为。"),
                new IDRPG3DRouteChineseGuideEntry("Follow", "是否跟随", "打开后物体会沿路线自动移动。"),
                new IDRPG3DRouteChineseGuideEntry("Follow Mode", "跟随模式", "Uniform 按速度移动；Time 按指定时长走完整条路线。"),
                new IDRPG3DRouteChineseGuideEntry("Follow Speed", "跟随速度", "Uniform 模式下每秒沿路线移动的距离。负数通常表示反方向。"),
                new IDRPG3DRouteChineseGuideEntry("Follow duration", "跟随时长", "Time 模式下走完整条路线需要的时间。"),
                new IDRPG3DRouteChineseGuideEntry("Wrap Mode", "循环模式", "到达路线末端后的处理方式，例如停止、循环或来回往返。"),
                new IDRPG3DRouteChineseGuideEntry("Face Direction", "朝向移动方向", "让物体旋转朝向路线前进方向。角色沿路移动时通常需要开启。"),
                new IDRPG3DRouteChineseGuideEntry("Start Position", "起始位置", "Follower 起跑位置，0 是起点，1 是终点。"),
                new IDRPG3DRouteChineseGuideEntry("Automatic Start Position", "自动起始位置", "根据物体当前世界位置投影到路线，自动设置起始百分比。"),
                new IDRPG3DRouteChineseGuideEntry("Set Distance", "按距离设置", "用实际路线距离换算百分比，比直接填 0 到 1 更直观。"),
                new IDRPG3DRouteChineseGuideEntry("On Beginning Reached / On End Reached", "到达起点 / 终点事件", "Follower 到达路线起点或终点时触发 UnityEvent。")
            ),
            new IDRPG3DRouteChineseGuideSection(
                "Primitives / Presets",
                new IDRPG3DRouteChineseGuideEntry("Primitives", "程序化基础形状", "快速生成线、圆、矩形、螺旋等基础曲线。"),
                new IDRPG3DRouteChineseGuideEntry("Presets", "预设", "保存或使用已有路线预设。"),
                new IDRPG3DRouteChineseGuideEntry("Apply", "应用", "把当前基础形状或预设应用到路线。"),
                new IDRPG3DRouteChineseGuideEntry("Revert", "还原", "撤回当前未应用的基础形状修改。"),
                new IDRPG3DRouteChineseGuideEntry("Create New", "新建预设", "把当前路线保存为预设。"),
                new IDRPG3DRouteChineseGuideEntry("Preset name", "预设名称", "保存预设时使用的名字。"),
                new IDRPG3DRouteChineseGuideEntry("Description", "说明", "预设的说明文本。"),
                new IDRPG3DRouteChineseGuideEntry("Use", "使用", "加载选中的预设。")
            ),
            new IDRPG3DRouteChineseGuideSection(
                "IDRPG3D 路线辅助工具",
                new IDRPG3DRouteChineseGuideEntry("路线编辑器中文辅助", "中文辅助窗口", "当前窗口。用于学习 Dreamteck 英文 Inspector 的字段含义和常用操作。"),
                new IDRPG3DRouteChineseGuideEntry("Surface Snap Mode", "自动贴地模式", "我们自己的路线点贴地工具，适合编辑大 Terrain 上的挂机路线。"),
                new IDRPG3DRouteChineseGuideEntry("Enable", "启用自动贴地", "开启后移动或新增路线点会尝试吸附到 Terrain 或碰撞体表面。"),
                new IDRPG3DRouteChineseGuideEntry("Closed Loop", "闭合循环", "把路线首尾相接。至少需要 3 个控制点。"),
                new IDRPG3DRouteChineseGuideEntry("Snap All Points On Selected Route", "吸附所选路线全部点", "一次性把当前 SplineComputer 的所有控制点投射到地形或碰撞体表面。"),
                new IDRPG3DRouteChineseGuideEntry("Height Offset", "离地高度", "贴地后额外抬高的距离，避免角色脚底或路线点陷入地面。")
            )
        };
    }
}
