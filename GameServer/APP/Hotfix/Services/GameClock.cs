namespace Fantasy;

public static class GameClock
{
    public static long NowSeconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
