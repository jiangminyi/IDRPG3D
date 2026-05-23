using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;

namespace Fantasy;

public sealed class C2G_StopIdleBattleRequestHandler : MessageRPC<C2G_StopIdleBattleRequest, G2C_StopIdleBattleResponse>
{
    protected override async FTask Run(Session session, C2G_StopIdleBattleRequest request, G2C_StopIdleBattleResponse response, Action reply)
    {
        if (!GameSessionHelper.TryGetPlayerId(session, out var playerId))
        {
            response.ErrorCode = GameErrorCodes.PlayerNotFound;
            return;
        }

        var result = await IdleBattleService.Stop(session.Scene.World.Database, playerId, request.BattleId);
        response.ErrorCode = result.errorCode;
        if (result.battle != null)
        {
            response.Battle = SnapshotFactory.ToSummary(result.battle);
        }
    }
}
