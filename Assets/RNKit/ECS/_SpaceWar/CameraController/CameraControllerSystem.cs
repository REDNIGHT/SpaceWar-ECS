using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace RN.Network.SpaceWar
{
    public struct CameraDataSingleton : IComponentData
    {
        public float3 targetPosition;
        public quaternion targetRotation;
    }
    public abstract class ICameraController : MonoBehaviour
    {
        public abstract void update(bool hasShip, in float3 shipPosition, in quaternion shipRotation, in float3 shipLinear,
            in MouseDataSingleton mouseData,
            out CameraDataSingleton cameraData);
    }

    public class CameraControllerSystem : ComponentSystem
    {
        ICameraController cameraController;
        protected void OnInit(Transform root)
        {
            var singletonEntity = GetSingletonEntity<MyPlayerSingleton>();
            EntityManager.AddComponentData(singletonEntity, new CameraDataSingleton { targetRotation = quaternion.identity, });

            cameraController = GameObject.FindObjectOfType<ICameraController>();
            Debug.Assert(cameraController != null, "cameraController != null");
        }

        protected override void OnUpdate()
        {
            //
            float3 shipPosition = float3.zero;
            quaternion shipRotation = quaternion.identity;
            float3 shipLinear = float3.zero;
            var hasShip = updateByShip(ref shipPosition, ref shipRotation, ref shipLinear);


            cameraController.update(hasShip, shipPosition, shipRotation, shipLinear, GetSingleton<MouseDataSingleton>(), out CameraDataSingleton cameraData);


            var singletonEntity = GetSingletonEntity<MyPlayerSingleton>();
            EntityManager.SetComponentData(singletonEntity, cameraData);
        }


        bool updateByShip(ref float3 shipPosition, ref quaternion shipRotation, ref float3 shipLinear)
        {
            var myPlayerSingleton = GetSingleton<MyPlayerSingleton>();
            var myPlayerEntity = myPlayerSingleton.playerEntity;
            if (myPlayerEntity == Entity.Null)
                return false;
            if (EntityManager.HasComponent<PlayerActorArray>(myPlayerEntity) == false)
                return false;

            var actors = EntityManager.GetComponentData<PlayerActorArray>(myPlayerEntity);
            if (actors.shipEntity == Entity.Null)
                return false;


            var shipT = EntityManager.GetComponentObject<Transform>(actors.shipEntity);
            shipPosition = shipT.position;
            shipRotation = shipT.rotation;
            shipLinear = EntityManager.GetComponentData<RigidbodyVelocity>(actors.shipEntity).linear;

            return true;
        }
    }
}
