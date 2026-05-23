using Fantasy.Entitas;
using Fantasy.Entitas.Interface;

namespace Fantasy;

public sealed class TeamSave : Entity, ISupportedSerialize
{
    public long LeaderPlayerId { get; set; }
    public List<long> MemberPlayerIds { get; set; } = new();
    public long CreatedAt { get; set; }
}
