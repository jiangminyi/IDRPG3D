using Fantasy.Async;
using Fantasy.Database;
using Fantasy.Entitas;

namespace Fantasy;

public static class PlayerService
{
    public static async FTask<PlayerSave?> GetById(IDatabase database, long playerId)
    {
        return await database.Query<PlayerSave>(playerId);
    }

    public static async FTask<PlayerSave> GetOrCreate(Scene scene, IDatabase database, string account, string deviceId)
    {
        var normalizedAccount = account.Trim();
        var player = await database.First<PlayerSave>(p => p.Account == normalizedAccount);
        if (player != null)
        {
            player.DeviceId = deviceId;
            player.UpdatedAt = GameClock.NowSeconds();
            await database.Save(player);
            return player;
        }

        player = Entity.Create<PlayerSave>(scene, false, false);
        player.Account = normalizedAccount;
        player.DeviceId = deviceId;
        player.Name = normalizedAccount;
        player.Level = 1;
        player.UpdatedAt = GameClock.NowSeconds();
        await database.Save(player);
        return player;
    }

    public static string IssueToken(PlayerSave player)
    {
        return $"{player.Id:N}:{Guid.NewGuid():N}";
    }
}
