using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;

namespace Fantasy;

public sealed class C2G_CreateTeamRequestHandler : MessageRPC<C2G_CreateTeamRequest, G2C_CreateTeamResponse>
{
    protected override async FTask Run(Session session, C2G_CreateTeamRequest request, G2C_CreateTeamResponse response, Action reply)
    {
        if (!GameSessionHelper.TryGetPlayerId(session, out var playerId))
        {
            response.ErrorCode = GameErrorCodes.PlayerNotFound;
            return;
        }

        var database = session.Scene.World.Database;
        var player = await PlayerService.GetById(database, playerId);
        if (player == null)
        {
            response.ErrorCode = GameErrorCodes.PlayerNotFound;
            return;
        }

        var result = await TeamService.Create(session.Scene, database, player);
        response.ErrorCode = result.errorCode;
        response.Team = result.team;
    }
}
