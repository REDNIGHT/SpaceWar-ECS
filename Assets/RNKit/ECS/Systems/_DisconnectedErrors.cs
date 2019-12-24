using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RN.Network
{
    public enum DisconnectedErrorsInSystem
    {
        Version = DisconnectedErrors.MaxCount + 1,
        Heartbeat,

        PlayerStateA,
        PlayerStateB,

        PlayerWaiting,

        PlayerActorSelect,

        ActorListIndexA,
        ActorListIndexB,

        //OwnerPlayerId,

        MaxCount = DisconnectedErrors.MaxCount * 2,
    }

    partial class NetworkStreamStateSystem
    {
        partial void getErrorInfo(short error, ref string errorStr)
        {
            if (error < (int)DisconnectedErrors.MaxCount)
            {
                var e = (DisconnectedErrors)error;
                errorStr = e.ToString();
                return;
            }
            if (error < (int)DisconnectedErrorsInSystem.MaxCount)
            {
                var e = (DisconnectedErrorsInSystem)error;
                errorStr = e.ToString();
                return;
            }

            errorStr = $"unknow error={error}";
        }
    }
}