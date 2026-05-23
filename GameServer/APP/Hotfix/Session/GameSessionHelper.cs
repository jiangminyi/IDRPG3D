using Fantasy.Network;

namespace Fantasy;

public static class GameSessionHelper
{
    public static bool TryGetPlayerId(Session session, out long playerId)
    {
        var component = session.GetComponent<GameSessionComponent>();
        if (component == null || component.PlayerId == 0)
        {
            playerId = 0;
            return false;
        }

        playerId = component.PlayerId;
        return true;
    }

    public static void Bind(Session session, long playerId, string token)
    {
        var component = session.GetOrAddComponent<GameSessionComponent>();
        component.PlayerId = playerId;
        component.Token = token;
    }
}
