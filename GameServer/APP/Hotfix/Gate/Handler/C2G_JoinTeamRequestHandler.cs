using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;

namespace Fantasy;

public sealed class C2G_JoinTeamRequestHandler : MessageRPC<C2G_JoinTeamRequest, G2C_JoinTeamResponse>
{
    protected override async FTask Run(Session session, C2G_JoinTeamRequest request, G2C_JoinTeamResponse response, Action reply)
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

        var result = await TeamService.Join(database, player, request.TeamId);
        response.ErrorCode = result.errorCode;
        response.Team = result.team;
    }
}
