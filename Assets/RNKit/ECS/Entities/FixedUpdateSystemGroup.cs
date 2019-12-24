using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Unity.Entities
{
    [DisableAutoCreation]
    [ExecuteAlways]
    public class MiddleCommandBufferSystem : EntityCommandBufferSystem
    {
    }

    [DisableAutoCreation]
    [ExecuteAlways]
    public class EndCommandBufferSystem : EntityCommandBufferSystem
    {
    }

    [DisableAutoCreation]
    [ExecuteAlways]
    public class FixedUpdateSystemGroup : ComponentSystemGroup
    {
        public override void SortSystemUpdateList()
        {
            //throw new System.Exception($"don't call this function.  pls call {typeof(IWorldBootstrap).Name}.Initialize");
        }
    }

    [DisableAutoCreation]
    [ExecuteAlways]
    public class FixedLateUpdateSystemGroup : ComponentSystemGroup
    {
        public override void SortSystemUpdateList()
        {
            //throw new System.Exception($"don't call this function.  pls call {typeof(IWorldBootstrap).Name}.Initialize");
        }
    }



    /*
    [DisableAutoCreation]
    [ExecuteAlways]
    public class BeginFixedUpdateEntityCommandBufferSystem : EntityCommandBufferSystem { }

    [DisableAutoCreation]
    [ExecuteAlways]
    public class EndFixedUpdateEntityCommandBufferSystem : EntityCommandBufferSystem { }

    [DisableAutoCreation]
    [ExecuteAlways]
    public class BeginLateFixedUpdateEntityCommandBufferSystem : EntityCommandBufferSystem { }

    [DisableAutoCreation]
    [ExecuteAlways]
    public class EndLateFixedUpdateEntityCommandBufferSystem : EntityCommandBufferSystem { }
    */

}