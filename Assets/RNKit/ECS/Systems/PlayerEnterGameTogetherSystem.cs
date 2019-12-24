using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace RN.Network
{
    //这里的规则是需要所有玩家都到齐了 才能一起进入游戏

    //todo...


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerEnterGameTogetherServerSystem : ComponentSystem
    {
        public int playerMaxCount = 10;

        protected override void OnDestroy()
        {
        }

        protected override void OnCreate()
        {
        }

        protected override void OnUpdate()
        {
        }
    }



    //----------------------------------------------------------------------------------------------------------------------


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class PlayerEnterGameTogetherClientSystem : ComponentSystem
    {
        protected override void OnDestroy()
        {
        }

        protected override void OnCreate()
        {
        }

        protected override void OnUpdate()
        {
        }

        public bool EnterGame()
        {
            return true;
        }
    }
}