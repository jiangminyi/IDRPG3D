using Fantasy.Async;
using Fantasy.Database;
using Fantasy.Entitas;

namespace Fantasy;

public static class TeamService
{
    public static async FTask<TeamInfo?> GetTeamInfo(IDatabase database, long teamId)
    {
        if (teamId == 0)
        {
            return null;
        }

        var team = await database.Query<TeamSave>(teamId);
        if (team == null)
        {
            return null;
        }

        return await BuildTeamInfo(database, team);
    }

    public static async FTask<(uint errorCode, TeamInfo? team)> Create(Scene scene, IDatabase database, PlayerSave player)
    {
        if (player.TeamId != 0)
        {
            return (GameErrorCodes.AlreadyInTeam, await GetTeamInfo(database, player.TeamId));
        }

        var now = GameClock.NowSeconds();
        var team = Entity.Create<TeamSave>(scene, false, false);
        team.LeaderPlayerId = player.Id;
        team.MemberPlayerIds.Add(player.Id);
        team.CreatedAt = now;

        player.TeamId = team.Id;
        player.UpdatedAt = now;

        await database.Save(team);
        await database.Save(player);
        return (GameErrorCodes.Success, await BuildTeamInfo(database, team));
    }

    public static async FTask<(uint errorCode, TeamInfo? team)> Join(IDatabase database, PlayerSave player, long teamId)
    {
        if (player.TeamId != 0)
        {
            return (GameErrorCodes.AlreadyInTeam, await GetTeamInfo(database, player.TeamId));
        }

        var team = await database.Query<TeamSave>(teamId);
        if (team == null)
        {
            return (GameErrorCodes.TeamNotFound, null);
        }

        if (!team.MemberPlayerIds.Contains(player.Id))
        {
            team.MemberPlayerIds.Add(player.Id);
        }

        player.TeamId = team.Id;
        player.UpdatedAt = GameClock.NowSeconds();

        await database.Save(team);
        await database.Save(player);
        return (GameErrorCodes.Success, await BuildTeamInfo(database, team));
    }

    private static async FTask<TeamInfo> BuildTeamInfo(IDatabase database, TeamSave team)
    {
        var players = new Dictionary<long, PlayerSave>();
        foreach (var playerId in team.MemberPlayerIds)
        {
            var player = await database.Query<PlayerSave>(playerId);
            if (player != null)
            {
                players[playerId] = player;
            }
        }

        return SnapshotFactory.ToTeamInfo(team, players);
    }
}
