using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    public interface IForceFx
    {
        void OnPlayFx(in ShipForceAttribute shipForceAttribute);
    }


    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShipForceFxClientSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
        }
        protected override void OnUpdate()
        {
            Entities
                .WithAllReadOnly<ShipForceAttribute>()
                .WithNone<ControlForceDirection>()
                .ForEach((Entity entity, ref ShipForceAttribute shipForceAttribute) =>
                {
                    var shipT = EntityManager.GetComponentObject<Transform>(entity);

                    var forceFxT = shipT.GetChild(ShipSpawner.ForceFx_TransformIndex);

                    var fx = forceFxT.GetComponent<IForceFx>();
                    if (fx != null)
                        fx.OnPlayFx(shipForceAttribute);
                });


            Entities
                .WithAllReadOnly<ShipForceAttribute, ShipForceControl, ControlForceDirection, ControlTorqueAngular>()
                .ForEach((Entity entity, ref ShipForceAttribute shipForceAttribute,
                ref ShipForceControl shipForceControl, ref ControlForceDirection controlForceDirection, ref ControlTorqueAngular controlTorqueAngular) =>
                {
                    var shipT = EntityManager.GetComponentObject<Transform>(entity);

                    var forceFxT = shipT.GetChild(ShipSpawner.ForceFx_TransformIndex);

                    var fx = forceFxT.GetComponent<IForceFx>();

                    if (fx != null)
                    {
                        var force = controlForceDirection.force;
                        var accelerate = shipForceControl.accelerateFire;
                        var torque = controlTorqueAngular.torque * controlTorqueAngular.angular.y;

                        shipForceAttribute.force = Mathf.Lerp(shipForceAttribute.force, force, 0.5f);
                        shipForceAttribute.torque = Mathf.Lerp(shipForceAttribute.torque, torque, 0.5f);
                        shipForceAttribute.accelerate = accelerate || accelerate;

                        fx.OnPlayFx(shipForceAttribute);
                    }
                });
        }
    }
}
