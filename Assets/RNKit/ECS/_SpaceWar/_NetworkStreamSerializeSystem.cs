using RN.Network.SpaceWar;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace RN.Network
{
    public enum NetworkSerializeType : short
    {
        NetworkId = 1,

        NetworkPing,
        NetworkPingResult,

        NetworkVersion,
        NetworkVersionResult,

        PlayerEnterGame,

        PlayerCreate,
        PlayerDestroy,

        PlayerName,
        PlayerNameChange,

        PlayerTeam,
        PlayerTeamChange,

        PlayerScore,
        KillInfo,

        PlayerGameReady,
        PlayerGameStart,

        PlayerActorSelect,
        PlayerObserverPosition,
        PlayerActorMoveInput,
        PlayerActorFireInput,

        ActorCreate,
        ActorDestroy,

        ActorSyncFrame_T,
        ActorSyncFrame_R,
        ActorSyncFrame_T_R,
        ActorSyncFrame_RB_VD,
        ActorSyncFrame_RB_T_V,
        ActorSyncFrame_RB_R_V,
        ActorSyncFrame_RB_T_R_V,

        ActorSyncFrame_Datas0,
        ActorSyncFrame_Datas1,
        //ActorSyncFrame_Datas2,
        //ActorSyncFrame_Data3,
        //ActorSyncFrame_Data4,

        //SynActorAttributes,

        WeaponInstalledState,

        //
        _MaxCount,
    }


    partial class NetworkStreamSerializeSystem
    {
        partial struct SerializeJob
        {
            partial void OnExecute
                (
                    int index, Entity entity,
                    short type,
                    DataStreamReader reader, ref DataStreamReader.Context ctx,
                    EntityCommandBuffer.Concurrent commandBuffer
                )
            {
                //Debug.LogWarning("SerializeJob=" + (NetworkSerializeType)type);
                switch ((NetworkSerializeType)type)
                {
                    case NetworkSerializeType.NetworkId:                /**/{ var s = new NetworkIdSerialize { };               /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.NetworkPing:              /**/{ var s = new NetworkPingSerialize { };             /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.NetworkPingResult:        /**/{ var s = new NetworkPingResultSerialize { };       /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.NetworkVersion:           /**/{ var s = new NetworkVersionSerialize { };          /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.NetworkVersionResult:     /**/{ var s = new NetworkVersionResultSerialize { };    /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }

                    case NetworkSerializeType.PlayerEnterGame:          /**/{ var s = new PlayerEnterGameSerialize { };         /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.PlayerCreate:             /**/{ var s = new PlayerCreateSerialize { };            /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.PlayerDestroy:            /**/{ var s = new PlayerDestroySerialize { };           /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.PlayerName:               /**/{ var s = new PlayerNameSerialize { };              /**/s.Deserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.PlayerNameChange:         /**/{ var s = new PlayerNameChangeSerialize { };        /**/s.Deserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.PlayerTeam:               /**/{ var s = new PlayerTeamSerialize { };              /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.PlayerTeamChange:         /**/{ var s = new PlayerTeamChangeSerialize { };        /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.PlayerScore:              /**/{ var s = new PlayerScoreSerialize { };             /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.KillInfo:                 /**/{ var s = new KillInfoSerialize { };                /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.PlayerGameReady:          /**/{ var s = new PlayerGameReadySerialize { };         /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.PlayerGameStart:          /**/{ var s = new PlayerGameStartSerialize { };         /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.PlayerActorSelect:        /**/{ var s = new PlayerActorSelectSerialize { };       /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.PlayerObserverPosition:   /**/{ var s = new PlayerObserverPositionSerialize { };  /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.PlayerActorMoveInput:     /**/{ var s = new PlayerActorMoveInputSerialize { };    /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.PlayerActorFireInput:     /**/{ var s = new PlayerActorFireInputSerialize { };    /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }

                    case NetworkSerializeType.ActorCreate:              /**/{ var s = new ActorCreateSerialize { };             /**/s.Deserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.ActorDestroy:             /**/{ var s = new ActorDestroySerialize { };            /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }

                    case NetworkSerializeType.ActorSyncFrame_T:         /**/{ var s = new ActorSyncFrame_T_Serialize { };       /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.ActorSyncFrame_R:         /**/{ var s = new ActorSyncFrame_R_Serialize { };       /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.ActorSyncFrame_T_R:       /**/{ var s = new ActorSyncFrame_T_R_Serialize { };     /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.ActorSyncFrame_RB_VD:     /**/{ var s = new ActorSyncFrame_RB_VD_Serialize { };   /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.ActorSyncFrame_RB_T_V:    /**/{ var s = new ActorSyncFrame_RB_T_V_Serialize { };  /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.ActorSyncFrame_RB_R_V:    /**/{ var s = new ActorSyncFrame_RB_R_V_Serialize { };  /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.ActorSyncFrame_RB_T_R_V:  /**/{ var s = new ActorSyncFrame_RB_T_R_V_Serialize { };/**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.ActorSyncFrame_Datas0:    /**/{ var s = new ActorSyncFrame_Datas0_Serialize { };  /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    case NetworkSerializeType.ActorSyncFrame_Datas1:    /**/{ var s = new ActorSyncFrame_Datas1_Serialize { };  /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    //case NetworkSerializeType.ActorSyncFrame_Datas2:  /**/{ var s = new ActorSyncFrame_Datas2_Serialize { };  /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }

                    case NetworkSerializeType.WeaponInstalledState:     /**/{ var s = new WeaponInstalledStateSerialize { };    /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }

                    //case NetworkSerializeType.:                       /**/{ var s = new Serialize { };                        /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    //case NetworkSerializeType.:                       /**/{ var s = new Serialize { };                        /**/s._DoDeserialize(reader, ref ctx); s.Execute(index, entity, commandBuffer); break; }
                    default: throw new System.Exception("SerializeJob type=" + type);
                }
            }
        }
    }
}