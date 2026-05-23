using LightProto;
using System;
using MemoryPack;
using System.Collections.Generic;
using Fantasy;
using Fantasy.Pool;
using Fantasy.Network.Interface;
using Fantasy.Serialize;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8618
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable RedundantTypeArgumentsOfMethod
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PreferConcreteValueOverDefault
// ReSharper disable RedundantNameQualifier
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable RedundantUsingDirective
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
namespace Fantasy
{
    [Serializable]
    [ProtoContract]
    public partial class ItemReward : AMessage, IDisposable
    {
        public static ItemReward Create(bool autoReturn = true)
        {
            var itemReward = MessageObjectPool<ItemReward>.Rent();
            itemReward.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                itemReward.SetIsPool(false);
            }
            
            return itemReward;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            ItemId = default;
            Count = default;
            MessageObjectPool<ItemReward>.Return(this);
        }
        [ProtoMember(1)]
        public int ItemId { get; set; }
        [ProtoMember(2)]
        public long Count { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class PlayerBrief : AMessage, IDisposable
    {
        public static PlayerBrief Create(bool autoReturn = true)
        {
            var playerBrief = MessageObjectPool<PlayerBrief>.Rent();
            playerBrief.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                playerBrief.SetIsPool(false);
            }
            
            return playerBrief;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            PlayerId = default;
            Name = default;
            Level = default;
            MessageObjectPool<PlayerBrief>.Return(this);
        }
        [ProtoMember(1)]
        public long PlayerId { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public int Level { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class TeamMemberInfo : AMessage, IDisposable
    {
        public static TeamMemberInfo Create(bool autoReturn = true)
        {
            var teamMemberInfo = MessageObjectPool<TeamMemberInfo>.Rent();
            teamMemberInfo.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                teamMemberInfo.SetIsPool(false);
            }
            
            return teamMemberInfo;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            PlayerId = default;
            Name = default;
            Level = default;
            IsOnline = default;
            MessageObjectPool<TeamMemberInfo>.Return(this);
        }
        [ProtoMember(1)]
        public long PlayerId { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public int Level { get; set; }
        [ProtoMember(4)]
        public bool IsOnline { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class TeamInfo : AMessage, IDisposable
    {
        public static TeamInfo Create(bool autoReturn = true)
        {
            var teamInfo = MessageObjectPool<TeamInfo>.Rent();
            teamInfo.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                teamInfo.SetIsPool(false);
            }
            
            return teamInfo;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            TeamId = default;
            foreach (var __t in Members) __t.Dispose();
            Members.Clear();
            LeaderPlayerId = default;
            MessageObjectPool<TeamInfo>.Return(this);
        }
        [ProtoMember(1)]
        public long TeamId { get; set; }
        [ProtoMember(2)]
        public List<TeamMemberInfo> Members { get; set; } = new List<TeamMemberInfo>();
        [ProtoMember(3)]
        public long LeaderPlayerId { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class IdleBattleSummary : AMessage, IDisposable
    {
        public static IdleBattleSummary Create(bool autoReturn = true)
        {
            var idleBattleSummary = MessageObjectPool<IdleBattleSummary>.Rent();
            idleBattleSummary.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                idleBattleSummary.SetIsPool(false);
            }
            
            return idleBattleSummary;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            BattleId = default;
            MapId = default;
            StartTime = default;
            LastSettleTime = default;
            foreach (var __t in Rewards) __t.Dispose();
            Rewards.Clear();
            Exp = default;
            Gold = default;
            MessageObjectPool<IdleBattleSummary>.Return(this);
        }
        [ProtoMember(1)]
        public long BattleId { get; set; }
        [ProtoMember(2)]
        public int MapId { get; set; }
        [ProtoMember(3)]
        public long StartTime { get; set; }
        [ProtoMember(4)]
        public long LastSettleTime { get; set; }
        [ProtoMember(5)]
        public List<ItemReward> Rewards { get; set; } = new List<ItemReward>();
        [ProtoMember(6)]
        public long Exp { get; set; }
        [ProtoMember(7)]
        public long Gold { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class C2G_LoginRequest : AMessage, IRequest
    {
        public static C2G_LoginRequest Create(bool autoReturn = true)
        {
            var c2G_LoginRequest = MessageObjectPool<C2G_LoginRequest>.Rent();
            c2G_LoginRequest.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                c2G_LoginRequest.SetIsPool(false);
            }
            
            return c2G_LoginRequest;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            Account = default;
            DeviceId = default;
            MessageObjectPool<C2G_LoginRequest>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.C2G_LoginRequest; } 
        [ProtoIgnore]
        public G2C_LoginResponse ResponseType { get; set; }
        [ProtoMember(1)]
        public string Account { get; set; }
        [ProtoMember(2)]
        public string DeviceId { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class G2C_LoginResponse : AMessage, IResponse
    {
        public static G2C_LoginResponse Create(bool autoReturn = true)
        {
            var g2C_LoginResponse = MessageObjectPool<G2C_LoginResponse>.Rent();
            g2C_LoginResponse.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                g2C_LoginResponse.SetIsPool(false);
            }
            
            return g2C_LoginResponse;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            ErrorCode = 0;
            PlayerId = default;
            Token = default;
            if (Player != null)
            {
                Player.Dispose();
                Player = null;
            }
            MessageObjectPool<G2C_LoginResponse>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.G2C_LoginResponse; } 
        [ProtoMember(4)]
        public uint ErrorCode { get; set; }
        [ProtoMember(1)]
        public long PlayerId { get; set; }
        [ProtoMember(2)]
        public string Token { get; set; }
        [ProtoMember(3)]
        public PlayerBrief Player { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class C2G_EnterWorldRequest : AMessage, IRequest
    {
        public static C2G_EnterWorldRequest Create(bool autoReturn = true)
        {
            var c2G_EnterWorldRequest = MessageObjectPool<C2G_EnterWorldRequest>.Rent();
            c2G_EnterWorldRequest.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                c2G_EnterWorldRequest.SetIsPool(false);
            }
            
            return c2G_EnterWorldRequest;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            PlayerId = default;
            Token = default;
            MessageObjectPool<C2G_EnterWorldRequest>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.C2G_EnterWorldRequest; } 
        [ProtoIgnore]
        public G2C_EnterWorldResponse ResponseType { get; set; }
        [ProtoMember(1)]
        public long PlayerId { get; set; }
        [ProtoMember(2)]
        public string Token { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class G2C_EnterWorldResponse : AMessage, IResponse
    {
        public static G2C_EnterWorldResponse Create(bool autoReturn = true)
        {
            var g2C_EnterWorldResponse = MessageObjectPool<G2C_EnterWorldResponse>.Rent();
            g2C_EnterWorldResponse.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                g2C_EnterWorldResponse.SetIsPool(false);
            }
            
            return g2C_EnterWorldResponse;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            ErrorCode = 0;
            if (Player != null)
            {
                Player.Dispose();
                Player = null;
            }
            if (Team != null)
            {
                Team.Dispose();
                Team = null;
            }
            if (CurrentBattle != null)
            {
                CurrentBattle.Dispose();
                CurrentBattle = null;
            }
            MessageObjectPool<G2C_EnterWorldResponse>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.G2C_EnterWorldResponse; } 
        [ProtoMember(4)]
        public uint ErrorCode { get; set; }
        [ProtoMember(1)]
        public PlayerBrief Player { get; set; }
        [ProtoMember(2)]
        public TeamInfo Team { get; set; }
        [ProtoMember(3)]
        public IdleBattleSummary CurrentBattle { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class C2G_StartIdleBattleRequest : AMessage, IRequest
    {
        public static C2G_StartIdleBattleRequest Create(bool autoReturn = true)
        {
            var c2G_StartIdleBattleRequest = MessageObjectPool<C2G_StartIdleBattleRequest>.Rent();
            c2G_StartIdleBattleRequest.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                c2G_StartIdleBattleRequest.SetIsPool(false);
            }
            
            return c2G_StartIdleBattleRequest;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            MapId = default;
            MessageObjectPool<C2G_StartIdleBattleRequest>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.C2G_StartIdleBattleRequest; } 
        [ProtoIgnore]
        public G2C_StartIdleBattleResponse ResponseType { get; set; }
        [ProtoMember(1)]
        public int MapId { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class G2C_StartIdleBattleResponse : AMessage, IResponse
    {
        public static G2C_StartIdleBattleResponse Create(bool autoReturn = true)
        {
            var g2C_StartIdleBattleResponse = MessageObjectPool<G2C_StartIdleBattleResponse>.Rent();
            g2C_StartIdleBattleResponse.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                g2C_StartIdleBattleResponse.SetIsPool(false);
            }
            
            return g2C_StartIdleBattleResponse;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            ErrorCode = 0;
            if (Battle != null)
            {
                Battle.Dispose();
                Battle = null;
            }
            MessageObjectPool<G2C_StartIdleBattleResponse>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.G2C_StartIdleBattleResponse; } 
        [ProtoMember(2)]
        public uint ErrorCode { get; set; }
        [ProtoMember(1)]
        public IdleBattleSummary Battle { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class C2G_StopIdleBattleRequest : AMessage, IRequest
    {
        public static C2G_StopIdleBattleRequest Create(bool autoReturn = true)
        {
            var c2G_StopIdleBattleRequest = MessageObjectPool<C2G_StopIdleBattleRequest>.Rent();
            c2G_StopIdleBattleRequest.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                c2G_StopIdleBattleRequest.SetIsPool(false);
            }
            
            return c2G_StopIdleBattleRequest;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            BattleId = default;
            MessageObjectPool<C2G_StopIdleBattleRequest>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.C2G_StopIdleBattleRequest; } 
        [ProtoIgnore]
        public G2C_StopIdleBattleResponse ResponseType { get; set; }
        [ProtoMember(1)]
        public long BattleId { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class G2C_StopIdleBattleResponse : AMessage, IResponse
    {
        public static G2C_StopIdleBattleResponse Create(bool autoReturn = true)
        {
            var g2C_StopIdleBattleResponse = MessageObjectPool<G2C_StopIdleBattleResponse>.Rent();
            g2C_StopIdleBattleResponse.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                g2C_StopIdleBattleResponse.SetIsPool(false);
            }
            
            return g2C_StopIdleBattleResponse;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            ErrorCode = 0;
            if (Battle != null)
            {
                Battle.Dispose();
                Battle = null;
            }
            MessageObjectPool<G2C_StopIdleBattleResponse>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.G2C_StopIdleBattleResponse; } 
        [ProtoMember(2)]
        public uint ErrorCode { get; set; }
        [ProtoMember(1)]
        public IdleBattleSummary Battle { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class G2C_BattleRewardPush : AMessage, IMessage
    {
        public static G2C_BattleRewardPush Create(bool autoReturn = true)
        {
            var g2C_BattleRewardPush = MessageObjectPool<G2C_BattleRewardPush>.Rent();
            g2C_BattleRewardPush.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                g2C_BattleRewardPush.SetIsPool(false);
            }
            
            return g2C_BattleRewardPush;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            if (Battle != null)
            {
                Battle.Dispose();
                Battle = null;
            }
            foreach (var __t in NewRewards) __t.Dispose();
            NewRewards.Clear();
            Exp = default;
            Gold = default;
            MessageObjectPool<G2C_BattleRewardPush>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.G2C_BattleRewardPush; } 
        [ProtoMember(1)]
        public IdleBattleSummary Battle { get; set; }
        [ProtoMember(2)]
        public List<ItemReward> NewRewards { get; set; } = new List<ItemReward>();
        [ProtoMember(3)]
        public long Exp { get; set; }
        [ProtoMember(4)]
        public long Gold { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class C2G_CreateTeamRequest : AMessage, IRequest
    {
        public static C2G_CreateTeamRequest Create(bool autoReturn = true)
        {
            var c2G_CreateTeamRequest = MessageObjectPool<C2G_CreateTeamRequest>.Rent();
            c2G_CreateTeamRequest.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                c2G_CreateTeamRequest.SetIsPool(false);
            }
            
            return c2G_CreateTeamRequest;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            MessageObjectPool<C2G_CreateTeamRequest>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.C2G_CreateTeamRequest; } 
        [ProtoIgnore]
        public G2C_CreateTeamResponse ResponseType { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class G2C_CreateTeamResponse : AMessage, IResponse
    {
        public static G2C_CreateTeamResponse Create(bool autoReturn = true)
        {
            var g2C_CreateTeamResponse = MessageObjectPool<G2C_CreateTeamResponse>.Rent();
            g2C_CreateTeamResponse.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                g2C_CreateTeamResponse.SetIsPool(false);
            }
            
            return g2C_CreateTeamResponse;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            ErrorCode = 0;
            if (Team != null)
            {
                Team.Dispose();
                Team = null;
            }
            MessageObjectPool<G2C_CreateTeamResponse>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.G2C_CreateTeamResponse; } 
        [ProtoMember(2)]
        public uint ErrorCode { get; set; }
        [ProtoMember(1)]
        public TeamInfo Team { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class C2G_JoinTeamRequest : AMessage, IRequest
    {
        public static C2G_JoinTeamRequest Create(bool autoReturn = true)
        {
            var c2G_JoinTeamRequest = MessageObjectPool<C2G_JoinTeamRequest>.Rent();
            c2G_JoinTeamRequest.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                c2G_JoinTeamRequest.SetIsPool(false);
            }
            
            return c2G_JoinTeamRequest;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            TeamId = default;
            MessageObjectPool<C2G_JoinTeamRequest>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.C2G_JoinTeamRequest; } 
        [ProtoIgnore]
        public G2C_JoinTeamResponse ResponseType { get; set; }
        [ProtoMember(1)]
        public long TeamId { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public partial class G2C_JoinTeamResponse : AMessage, IResponse
    {
        public static G2C_JoinTeamResponse Create(bool autoReturn = true)
        {
            var g2C_JoinTeamResponse = MessageObjectPool<G2C_JoinTeamResponse>.Rent();
            g2C_JoinTeamResponse.AutoReturn = autoReturn;
            
            if (!autoReturn)
            {
                g2C_JoinTeamResponse.SetIsPool(false);
            }
            
            return g2C_JoinTeamResponse;
        }
        
        public void Return()
        {
            if (!AutoReturn)
            {
                SetIsPool(true);
                AutoReturn = true;
            }
            else if (!IsPool())
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            if (!IsPool()) return; 
            ErrorCode = 0;
            if (Team != null)
            {
                Team.Dispose();
                Team = null;
            }
            MessageObjectPool<G2C_JoinTeamResponse>.Return(this);
        }
        public uint OpCode() { return OuterOpcode.G2C_JoinTeamResponse; } 
        [ProtoMember(2)]
        public uint ErrorCode { get; set; }
        [ProtoMember(1)]
        public TeamInfo Team { get; set; }
    }
}