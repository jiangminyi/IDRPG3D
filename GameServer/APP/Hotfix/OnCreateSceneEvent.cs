using Fantasy.Async;
using Fantasy.Event;

namespace Fantasy;

public sealed class OnCreateSceneEvent : AsyncEventSystem<OnCreateScene>
{
    protected override async FTask Handler(OnCreateScene self)
    {
        self.Scene.LogDebug($"IDRPG3D scene created: {self.Scene.SceneType}");
        await FTask.CompletedTask;
    }
}
