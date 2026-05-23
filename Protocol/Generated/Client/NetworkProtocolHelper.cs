using System.Runtime.CompilerServices;
using Fantasy;
using Fantasy.Async;
using Fantasy.Network;
using System.Collections.Generic;
#pragma warning disable CS8618
namespace Fantasy
{
   public static class NetworkProtocolHelper
   {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_LoginResponse> C2G_LoginRequest(this Session session, C2G_LoginRequest C2G_LoginRequest_request)
		{
			return (G2C_LoginResponse)await session.Call(C2G_LoginRequest_request);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_LoginResponse> C2G_LoginRequest(this Session session, string account, string deviceId)
		{
			using var C2G_LoginRequest_request = Fantasy.C2G_LoginRequest.Create();
			C2G_LoginRequest_request.Account = account;
			C2G_LoginRequest_request.DeviceId = deviceId;
			return (G2C_LoginResponse)await session.Call(C2G_LoginRequest_request);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_EnterWorldResponse> C2G_EnterWorldRequest(this Session session, C2G_EnterWorldRequest C2G_EnterWorldRequest_request)
		{
			return (G2C_EnterWorldResponse)await session.Call(C2G_EnterWorldRequest_request);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_EnterWorldResponse> C2G_EnterWorldRequest(this Session session, long playerId, string token)
		{
			using var C2G_EnterWorldRequest_request = Fantasy.C2G_EnterWorldRequest.Create();
			C2G_EnterWorldRequest_request.PlayerId = playerId;
			C2G_EnterWorldRequest_request.Token = token;
			return (G2C_EnterWorldResponse)await session.Call(C2G_EnterWorldRequest_request);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_StartIdleBattleResponse> C2G_StartIdleBattleRequest(this Session session, C2G_StartIdleBattleRequest C2G_StartIdleBattleRequest_request)
		{
			return (G2C_StartIdleBattleResponse)await session.Call(C2G_StartIdleBattleRequest_request);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_StartIdleBattleResponse> C2G_StartIdleBattleRequest(this Session session, int mapId)
		{
			using var C2G_StartIdleBattleRequest_request = Fantasy.C2G_StartIdleBattleRequest.Create();
			C2G_StartIdleBattleRequest_request.MapId = mapId;
			return (G2C_StartIdleBattleResponse)await session.Call(C2G_StartIdleBattleRequest_request);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_StopIdleBattleResponse> C2G_StopIdleBattleRequest(this Session session, C2G_StopIdleBattleRequest C2G_StopIdleBattleRequest_request)
		{
			return (G2C_StopIdleBattleResponse)await session.Call(C2G_StopIdleBattleRequest_request);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_StopIdleBattleResponse> C2G_StopIdleBattleRequest(this Session session, long battleId)
		{
			using var C2G_StopIdleBattleRequest_request = Fantasy.C2G_StopIdleBattleRequest.Create();
			C2G_StopIdleBattleRequest_request.BattleId = battleId;
			return (G2C_StopIdleBattleResponse)await session.Call(C2G_StopIdleBattleRequest_request);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void G2C_BattleRewardPush(this Session session, G2C_BattleRewardPush G2C_BattleRewardPush_message)
		{
			session.Send(G2C_BattleRewardPush_message);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void G2C_BattleRewardPush(this Session session, IdleBattleSummary battle, List<ItemReward> newRewards, long exp, long gold)
		{
			using var G2C_BattleRewardPush_message = Fantasy.G2C_BattleRewardPush.Create();
			G2C_BattleRewardPush_message.Battle = battle;
			G2C_BattleRewardPush_message.NewRewards = newRewards;
			G2C_BattleRewardPush_message.Exp = exp;
			G2C_BattleRewardPush_message.Gold = gold;
			session.Send(G2C_BattleRewardPush_message);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_CreateTeamResponse> C2G_CreateTeamRequest(this Session session, C2G_CreateTeamRequest C2G_CreateTeamRequest_request)
		{
			return (G2C_CreateTeamResponse)await session.Call(C2G_CreateTeamRequest_request);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_CreateTeamResponse> C2G_CreateTeamRequest(this Session session)
		{
			using var C2G_CreateTeamRequest_request = Fantasy.C2G_CreateTeamRequest.Create();
			return (G2C_CreateTeamResponse)await session.Call(C2G_CreateTeamRequest_request);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_JoinTeamResponse> C2G_JoinTeamRequest(this Session session, C2G_JoinTeamRequest C2G_JoinTeamRequest_request)
		{
			return (G2C_JoinTeamResponse)await session.Call(C2G_JoinTeamRequest_request);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async FTask<G2C_JoinTeamResponse> C2G_JoinTeamRequest(this Session session, long teamId)
		{
			using var C2G_JoinTeamRequest_request = Fantasy.C2G_JoinTeamRequest.Create();
			C2G_JoinTeamRequest_request.TeamId = teamId;
			return (G2C_JoinTeamResponse)await session.Call(C2G_JoinTeamRequest_request);
		}

   }
}