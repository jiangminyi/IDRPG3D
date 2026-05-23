using Fantasy.Entitas;
using Fantasy.Entitas.Interface;

namespace Fantasy;

public sealed class IdleBattleSave : Entity, ISupportedSerialize
{
    public long PlayerId { get; set; }
    public int MapId { get; set; }
    public long StartTime { get; set; }
    public long LastSettleTime { get; set; }
    public long Exp { get; set; }
    public long Gold { get; set; }
    public List<RewardSave> Rewards { get; set; } = new();
    public bool IsRunning { get; set; }
}

public sealed class RewardSave
{
    public int ItemId { get; set; }
    public long Count { get; set; }
}
