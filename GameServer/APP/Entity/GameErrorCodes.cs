namespace Fantasy;

public static class GameErrorCodes
{
    public const uint Success = (uint)IDRPG3DErrorCode.IDRPG3DErrorCodeSuccess;
    public const uint Unknown = (uint)IDRPG3DErrorCode.IDRPG3DErrorCodeUnknown;
    public const uint InvalidAccount = (uint)IDRPG3DErrorCode.IDRPG3DErrorCodeInvalidAccount;
    public const uint PlayerNotFound = (uint)IDRPG3DErrorCode.IDRPG3DErrorCodePlayerNotFound;
    public const uint TeamNotFound = (uint)IDRPG3DErrorCode.IDRPG3DErrorCodeTeamNotFound;
    public const uint AlreadyInTeam = (uint)IDRPG3DErrorCode.IDRPG3DErrorCodeAlreadyInTeam;
    public const uint IdleBattleAlreadyRunning = (uint)IDRPG3DErrorCode.IDRPG3DErrorCodeIdleBattleAlreadyRunning;
    public const uint IdleBattleNotRunning = (uint)IDRPG3DErrorCode.IDRPG3DErrorCodeIdleBattleNotRunning;
}
