using Fantasy.Entitas;

namespace Fantasy;

public sealed class GameSessionComponent : Entity
{
    public long PlayerId { get; set; }
    public string Token { get; set; } = string.Empty;
}
