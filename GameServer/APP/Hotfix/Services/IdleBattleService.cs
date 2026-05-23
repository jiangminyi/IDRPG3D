using Fantasy.Async;
using Fantasy.Database;
using Fantasy.Entitas;

namespace Fantasy;

public static class IdleBattleService
{
    public static async FTask<IdleBattleSave?> GetRunningBattle(IDatabase database, long playerId)
    {
        return await database.First<IdleBattleSave>(b => b.PlayerId == playerId && b.IsRunning);
    }

    public static async FTask<(uint errorCode, IdleBattleSave? battle)> Start(Scene scene, IDatabase database, long playerId, int mapId)
    {
        var currentBattle = await GetRunningBattle(database, playerId);
        if (currentBattle != null)
        {
            return (GameErrorCodes.IdleBattleAlreadyRunning, currentBattle);
        }

        var now = GameClock.NowSeconds();
        var battle = Entity.Create<IdleBattleSave>(scene, false, false);
        battle.PlayerId = playerId;
        battle.MapId = mapId <= 0 ? 1 : mapId;
        battle.StartTime = now;
        battle.LastSettleTime = now;
        battle.IsRunning = true;
        await database.Save(battle);
        return (GameErrorCodes.Success, battle);
    }

    public static async FTask<(uint errorCode, IdleBattleSave? battle)> Stop(IDatabase database, long playerId, long battleId)
    {
        var battle = await database.Query<IdleBattleSave>(battleId);
        if (battle == null || battle.PlayerId != playerId || !battle.IsRunning)
        {
            return (GameErrorCodes.IdleBattleNotRunning, null);
        }

        Settle(battle, GameClock.NowSeconds());
        battle.IsRunning = false;
        await database.Save(battle);
        return (GameErrorCodes.Success, battle);
    }

    public static void Settle(IdleBattleSave battle, long now)
    {
        var elapsed = Math.Max(0, now - battle.LastSettleTime);
        if (elapsed == 0)
        {
            return;
        }

        var minutes = Math.Max(1, elapsed / 60);
        battle.Exp += minutes * 10;
        battle.Gold += minutes * 5;
        battle.LastSettleTime = now;

        var reward = battle.Rewards.FirstOrDefault(r => r.ItemId == 1001);
        if (reward == null)
        {
            reward = new RewardSave { ItemId = 1001 };
            battle.Rewards.Add(reward);
        }

        reward.Count += minutes;
    }
}
