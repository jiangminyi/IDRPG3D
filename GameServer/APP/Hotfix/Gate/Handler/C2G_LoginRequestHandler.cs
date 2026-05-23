using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;

namespace Fantasy;

public sealed class C2G_LoginRequestHandler : MessageRPC<C2G_LoginRequest, G2C_LoginResponse>
{
    protected override async FTask Run(Session session, C2G_LoginRequest request, G2C_LoginResponse response, Action reply)
    {
        if (string.IsNullOrWhiteSpace(request.Account))
        {
            response.ErrorCode = GameErrorCodes.InvalidAccount;
            return;
        }

        var database = session.Scene.World.Database;
        var player = await PlayerService.GetOrCreate(session.Scene, database, request.Account, request.DeviceId);
        var token = PlayerService.IssueToken(player);

        GameSessionHelper.Bind(session, player.Id, token);

        response.PlayerId = player.Id;
        response.Token = token;
        response.Player = SnapshotFactory.ToBrief(player);
    }
}
