using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;

namespace Fantasy;

public sealed class C2G_StartIdleBattleRequestHandler : MessageRPC<C2G_StartIdleBattleRequest, G2C_StartIdleBattleResponse>
{
    protected override async FTask Run(Session session, C2G_StartIdleBattleRequest request, G2C_StartIdleBattleResponse response, Action reply)
    {
        if (!GameSessionHelper.TryGetPlayerId(session, out var playerId))
        {
            response.ErrorCode = GameErrorCodes.PlayerNotFound;
            return;
        }

        var result = await IdleBattleService.Start(session.Scene, session.Scene.World.Database, playerId, request.MapId);
        response.ErrorCode = result.errorCode;
        if (result.battle != null)
        {
            response.Battle = SnapshotFactory.ToSummary(result.battle);
        }
    }
}
