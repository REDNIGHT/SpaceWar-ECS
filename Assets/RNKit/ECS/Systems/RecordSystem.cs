using Unity.Entities;
using Unity.Jobs;

namespace RN.Network
{
    [ServerRecorderEntity]
    public struct Recorder : IComponentData
    {
    }


    [DisableAutoCreation]
    public class RecordServerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            //todo...  保存时间

            Entities
                .WithAll<Recorder>()
                //.WithNone<>()
                .ForEach((DynamicBuffer<ObserverCreateVisibleActorBuffer> createVisibleActorList
                , DynamicBuffer<ObserverSyncVisibleActorBuffer> syncVisibleActorList
                , DynamicBuffer<ObserverDestroyVisibleActorBuffer> destroyVisibleActorList) =>
                {
                    //todo...  保存到文件


                });

            Entities
                .WithAll<Recorder>()
                //.WithNone<>()
                .ForEach(
                (DynamicBuffer<ActorDatas0Buffer> syncActorDatas0List
                , DynamicBuffer<ActorDatas1Buffer> syncActorDatas1List
                //, DynamicBuffer<ActorDatas2Buffer> syncActorDatas2List
                //, DynamicBuffer<ActorDatas3List> syncActorDatas3List
                //, DynamicBuffer<ActorDatas4List> syncActorDatas4List
                ) =>
                {
                    //todo...  保存到文件


                });

            //todo...  保存score...
        }
    }

    [DisableAutoCreation]
    public class RecordClientSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {

        }
    }
}
