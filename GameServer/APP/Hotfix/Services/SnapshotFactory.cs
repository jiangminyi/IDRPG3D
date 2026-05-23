namespace Fantasy;

public static class SnapshotFactory
{
    public static PlayerBrief ToBrief(PlayerSave player)
    {
        return new PlayerBrief
        {
            PlayerId = player.Id,
            Name = player.Name,
            Level = player.Level
        };
    }

    public static IdleBattleSummary ToSummary(IdleBattleSave battle)
    {
        var summary = new IdleBattleSummary
        {
            BattleId = battle.Id,
            MapId = battle.MapId,
            StartTime = battle.StartTime,
            LastSettleTime = battle.LastSettleTime,
            Exp = battle.Exp,
            Gold = battle.Gold
        };

        foreach (var reward in battle.Rewards)
        {
            summary.Rewards.Add(new ItemReward
            {
                ItemId = reward.ItemId,
                Count = reward.Count
            });
        }

        return summary;
    }

    public static TeamInfo ToTeamInfo(TeamSave team, IReadOnlyDictionary<long, PlayerSave> players)
    {
        var info = new TeamInfo
        {
            TeamId = team.Id,
            LeaderPlayerId = team.LeaderPlayerId
        };

        foreach (var playerId in team.MemberPlayerIds)
        {
            if (!players.TryGetValue(playerId, out var player))
            {
                continue;
            }

            info.Members.Add(new TeamMemberInfo
            {
                PlayerId = player.Id,
                Name = player.Name,
                Level = player.Level,
                IsOnline = true
            });
        }

        return info;
    }
}
