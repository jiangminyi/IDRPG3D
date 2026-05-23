using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;

namespace Fantasy;

public sealed class C2G_EnterWorldRequestHandler : MessageRPC<C2G_EnterWorldRequest, G2C_EnterWorldResponse>
{
    protected override async FTask Run(Session session, C2G_EnterWorldRequest request, G2C_EnterWorldResponse response, Action reply)
    {
        var component = session.GetComponent<GameSessionComponent>();
        if (component == null || component.PlayerId != request.PlayerId || component.Token != request.Token)
        {
            response.ErrorCode = GameErrorCodes.PlayerNotFound;
            return;
        }

        var database = session.Scene.World.Database;
        var player = await PlayerService.GetById(database, request.PlayerId);
        if (player == null)
        {
            response.ErrorCode = GameErrorCodes.PlayerNotFound;
            return;
        }

        response.Player = SnapshotFactory.ToBrief(player);
        response.Team = await TeamService.GetTeamInfo(database, player.TeamId);

        var currentBattle = await IdleBattleService.GetRunningBattle(database, player.Id);
        if (currentBattle != null)
        {
            IdleBattleService.Settle(currentBattle, GameClock.NowSeconds());
            await database.Save(currentBattle);
            response.CurrentBattle = SnapshotFactory.ToSummary(currentBattle);
        }
    }
}
