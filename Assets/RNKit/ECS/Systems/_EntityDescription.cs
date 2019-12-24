using System;

namespace RN.Network
{
    public class ServerNetworkEntity : Attribute { }

    public class ClientNetworkEntity : Attribute { }

    public class ServerRecorderEntity : Attribute { }

    public class ClientPlayerEntity : Attribute { }

    public class ActorEntity : Attribute { }

    class _EntityDescription
    {
        object[] entityDescription = new object[]
        {
            new ServerNetworkEntity(),
                new object[]//Connection
                {
                    new NetworkConnection(),                                    //: IComponentData
                    new NetworkInBuffer(),                                      //: IBufferElementData
                    new NetworkReliableOutBuffer(),                             //: IBufferElementData
                    new NetworkUnreliableOutBuffer(),                           //: IBufferElementData
                    new NetworkConnectedMessage(),                              //: IComponentData             : Message
                    new NetworkDisconnectedMessage(),                           //: IComponentData             : Message
                },
                new object[]//Components
                {
                    new NetworkVersionNetMessage(),                             //: IComponentData             : NetMessage
                    new NetworkPingNetMessage(),                                //: IComponentData             : NetMessage
                    new NetworkHeartbeat(),                                     //: IComponentData
                    new NetworkId(),                                            //: IComponentData

                    new PlayerEnterGameNetMessage(),                            //: IComponentData             : NetMessage
                    new PlayerEnterGameMessage(),                               //: IComponentData             : Message
                    
                    new Player(),                                               //: IComponentData

                    new PlayerName(),                                           //: IComponentData
                    new PlayerNameChangeNetMessage(),                           //: IComponentData             : NetMessage

                    new PlayerTeam(),                                           //: IComponentData
                    new PlayerTeamChangeNetMessage(),                           //: IComponentData             : NetMessage
                    
                    new PlayerScore(),                                          //: IComponentData
                    
                    new PlayerActorType(),                                      //: IComponentData
                    new PlayerActorSelectNetMessage(),                          //: IComponentData             : NetMessage

                    new PlayerGameReady(),                                      //: IComponentData
                    new PlayerGameStartNetMessage(),                            //: IComponentData             : Message

                    new ObserverPosition(),                                     //: IComponentData
                    new ObserverVisibleDistance(),                              //: IComponentData
                    new ObserverCreateVisibleActorBuffer(),                     //: IBufferElementData
                    new ObserverSyncVisibleActorBuffer(),                       //: IBufferElementData
                    new ObserverDestroyVisibleActorBuffer(),                    //: IBufferElementData

                    new ActorDatas0Buffer(),                                    //: IBufferElementData
                    new ActorDatas1Buffer(),                                    //: IBufferElementData
                    //new ActorDatas2Buffer(),                                  //: IBufferElementData
                    //new ActorDatas3List(),                                    //: IBufferElementData
                    //new ActorDatas4List(),                                    //: IBufferElementData
                    
                    new PlayerActorArray(),                                     //: IBufferElementData
                },

            new ServerRecorderEntity(),
                new object[]//Components
                {
                    new Recorder(),                                             //: IComponentData
                    
                    new ObserverPosition(),                                     //: IComponentData
                    new ObserverVisibleDistance { valueSq = -1f },              //: IComponentData
                    new ObserverCreateVisibleActorBuffer(),                     //: IBufferElementData
                    new ObserverSyncVisibleActorBuffer(),                       //: IBufferElementData
                    new ObserverDestroyVisibleActorBuffer(),                    //: IBufferElementData

                    new ActorDatas0Buffer(),                                    //: IBufferElementData
                    new ActorDatas1Buffer(),                                    //: IBufferElementData
                    //new ActorDatas2Buffer(),                                  //: IBufferElementData
                    //new ActorDatas3List(),                                    //: IBufferElementData
                    //new ActorDatas4List(),                                    //: IBufferElementData
                },

            //每个客户端只有一个
            new ClientNetworkEntity(),
                new object[]//Connection
                {
                    new NetworkConnection(),                                    //: IComponentData
                    new NetworkInBuffer(),                                      //: IBufferElementData
                    new NetworkReliableOutBuffer(),                             //: IBufferElementData
                    new NetworkUnreliableOutBuffer(),                           //: IBufferElementData
                    new NetworkConnectedMessage(),                              //: IComponentData             : Message
                    new NetworkDisconnectedMessage(),                           //: IComponentData             : Message
                },
                new object[]//Components
                {
                    new NetworkVersionResultNetMessage(),                       //: IComponentData             : NetMessage
                    new NetworkPingResultNetMessage(),                          //: IComponentData             : NetMessage
                    new NetworkId(),                                            //: IComponentData
                    
                    new PlayerEnterGameResultNetMessage(),                      //: IComponentData             : NetMessage
                    new PlayerEnterGameMessage(),                               //: IComponentData             : Message

                    new PlayerCreateNetMessages(),                              //: IBufferElementData         : NetMessage
                    new PlayerDestroyNetMessages(),                             //: IBufferElementData         : NetMessage

                    new PlayerNameNetMessages(),                                //: IBufferElementData         : NetMessage
                    new PlayerTeamNetMessages(),                                //: IBufferElementData         : NetMessage
                    new PlayerScoreNetMessages(),                               //: IBufferElementData         : NetMessage
                    
                    new PlayerGameReadyNetMessage(),                            //: IComponentData             : NetMessage
                    new PlayerGameStartNetMessage(),                            //: IComponentData             : Message
                    
                    new PlayerActorType(),                                      //: IComponentData 

                    new ActorCreateSerializeNetMessage(),                       //: IBufferElementData         : NetMessage
                    new ActorDestroySerializeNetMessage(),                      //: IBufferElementData         : NetMessage

                    new ActorSyncFrame_T_NetMessage(),                          //: IBufferElementData         : NetMessage
                    new ActorSyncFrame_T_R_NetMessage(),                        //: IBufferElementData         : NetMessage
                    new ActorSyncFrame_RB_VD_NetMessage(),                      //: IBufferElementData         : NetMessage
                    new ActorSyncFrame_RB_T_R_V_NetMessage(),                   //: IBufferElementData         : NetMessage
                    
                    new ActorDatas0Buffer(),                                    //: IBufferElementData
                    new ActorDatas1Buffer(),                                    //: IBufferElementData         : NetMessage
                    //new ActorDatas2Buffer(),                                  //: IBufferElementData         : NetMessage
                    //new ActorDatas3List(),                                    //: IBufferElementData         : NetMessage
                    //new ActorDatas4List(),                                    //: IBufferElementData         : NetMessage
                },


                //每个客户端都会有其他所有客户端的信息(包括自己)
            new ClientPlayerEntity(),
                new object[]
                {
                    new Player(),                                               //: IComponentData
                    new PlayerName(),                                           //: IComponentData
                    new PlayerTeam(),                                           //: IComponentData
                    new PlayerScore(),                                          //: IComponentData

                    new PlayerActorArray(),                                     //: IBufferElementData
                },
        };
    }



}
