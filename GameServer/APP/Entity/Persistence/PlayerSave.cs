using Fantasy.Entitas;
using Fantasy.Entitas.Interface;

namespace Fantasy;

public sealed class PlayerSave : Entity, ISupportedSerialize
{
    public string Account { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public long Exp { get; set; }
    public long Gold { get; set; }
    public long TeamId { get; set; }
    public long UpdatedAt { get; set; }
}
