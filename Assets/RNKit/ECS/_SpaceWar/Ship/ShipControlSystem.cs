using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShipControlServerSystem : JobComponentSystem
    {
        MiddleCommandBufferSystem middleBarrier;

        protected void OnInit(Transform root)
        {
            middleBarrier = World.GetExistingSystem<MiddleCommandBufferSystem>();
        }


        [BurstCompile]
        struct DragByShipForwordJob : IJobForEach<RigidbodyLinearDragChange, RigidbodyVelocity, ShipControlInfo, Rotation>
        {
            public void Execute(ref RigidbodyLinearDragChange rigidbodyLinearDragChange, [ReadOnly]ref RigidbodyVelocity rigidbodyVelocity, [ReadOnly]ref ShipControlInfo shipControlData, [ReadOnly]ref Rotation rotation)
            {
                var linearVelocity = rigidbodyVelocity.linear;
                if (math.lengthsq(linearVelocity) > 0.001f)
                {
                    var dot = math.dot(math.normalize(linearVelocity), math.forward(rotation.Value));
                    dot = math.max(0, dot);
                    //dot = math.abs(dot);
                    var drag = 1f - dot;

                    rigidbodyLinearDragChange.drag = drag;
                    rigidbodyLinearDragChange.drag *= shipControlData.dragByLRBVelocity;
                }
                else
                {
                    rigidbodyLinearDragChange.drag = 0f;
                }
            }
        }

        [BurstCompile]
        public struct TorqueJob : IJobForEach<ControlTorqueAngular, ShipTorqueControl, ShipControlInfo, ShipMoveInput, ActorAttribute1<_VelocityLevel>>
        {
            public float fixedDeltaTime;
            public void Execute(ref ControlTorqueAngular controlTorqueAngular, ref ShipTorqueControl shipTorqueControl,
                 [ReadOnly]ref ShipControlInfo shipControlData, [ReadOnly]ref ShipMoveInput shipMoveInput, [ReadOnly] ref ActorAttribute1<_VelocityLevel> shipVelocity)
            {
                if (shipMoveInput.lost)
                {
                    shipTorqueControl.noControlTorqueTime = 0f;
                    controlTorqueAngular.torque = 0f;
                    return;
                }

                var velocityLevel = (int)shipVelocity.value;
                //velocityLevel += 1;

                var velocity = shipControlData.getVelocity(velocityLevel);

                var torqueY = math.clamp(shipMoveInput.torqueY, -1f, 1f);

                //out
                controlTorqueAngular.torque = velocity.torque;
                if (torqueY != 0f)
                {
                    shipTorqueControl.noControlTorqueTime = shipTorqueControl.noControlTorqueBeginTime;
                }
                else
                {
                    controlTorqueAngular.torque *= shipTorqueControl.noControlTorqueTime;

                    shipTorqueControl.noControlTorqueTime -= fixedDeltaTime;
                    shipTorqueControl.noControlTorqueTime = math.clamp(shipTorqueControl.noControlTorqueTime, 0f, 1f);
                }
                controlTorqueAngular.maxTorque = velocity.maxTorque;
                controlTorqueAngular.angular = new float3 { y = torqueY };
            }
        }

        [BurstCompile]
        public struct ForceJob : IJobForEach<ControlForceDirection, ShipForceControl, ShipControlInfo, ShipMoveInput, ActorAttribute1<_VelocityLevel>, Rotation>
        {
            public float fixedDeltaTime;
            public void Execute(ref ControlForceDirection controlForceDirection, ref ShipForceControl shipForceControl,
                [ReadOnly] ref ShipControlInfo shipControlData, [ReadOnly]ref ShipMoveInput shipMoveInput, [ReadOnly] ref ActorAttribute1<_VelocityLevel> shipVelocityLevel, [ReadOnly]ref Rotation rotation)
            {
                shipForceControl.moveDirection = float3.zero;

                if (shipMoveInput.lost)
                {
                    shipForceControl.noControlTime = 0f;
                    controlForceDirection.force = 0f;
                    return;
                }


                var shipForward = math.forward(rotation.Value);
                var shipBack = -shipForward;
                var shipLeft = math.mul(rotation.Value, new float3(-1f, 0f, 0f));
                var shipRight = -shipLeft;


                //
                var velocityLevel = (int)shipVelocityLevel.value;
                var velocity = shipControlData.getVelocity(velocityLevel);

                if (shipMoveInput.accelerate == false)
                {
                    if (shipMoveInput.moveForward)
                        shipForceControl.moveDirection = shipForward * shipControlData.forwardScale;
                    else if (shipMoveInput.moveBack)
                        shipForceControl.moveDirection = shipBack * shipControlData.backScale;

                    if (shipMoveInput.moveLeft)
                        shipForceControl.moveDirection += shipLeft * shipControlData.leftRightScale;
                    else if (shipMoveInput.moveRight)
                        shipForceControl.moveDirection += shipRight * shipControlData.leftRightScale;

                    if ((shipMoveInput.moveForward || shipMoveInput.moveBack)
                     && (shipMoveInput.moveLeft || shipMoveInput.moveRight))
                    {
                        shipForceControl.moveDirection *= shipControlData.moveComboScale;
                    }


                    var force = velocity.force;
                    if (shipForceControl.moveDirection.Equals(default))
                    {
                        ///不控制时 控制的force慢慢降低到0
                        force *= shipForceControl.noControlTime;
                        shipForceControl.noControlTime -= fixedDeltaTime;
                        shipForceControl.noControlTime = math.clamp(shipForceControl.noControlTime, 0f, 1f);
                    }
                    else
                    {
                        shipForceControl.noControlTime = shipForceControl.noControlBeginTime;
                    }


                    //out
                    controlForceDirection.force = force;
                    controlForceDirection.maxVelocity = velocity.maxVelocity;
                    controlForceDirection.direction = shipForceControl.moveDirection;
                }
                else if (shipMoveInput.accelerate && velocityLevel == 0)
                {
                    if (shipMoveInput.moveForward)
                        shipForceControl.moveDirection = shipForward * shipControlData.forwardScale;
                    else if (shipMoveInput.moveBack)
                        shipForceControl.moveDirection = shipBack * shipControlData.backScale;
                    else

                    if (shipMoveInput.moveLeft)
                        shipForceControl.moveDirection += shipLeft * shipControlData.leftRightScale;
                    else if (shipMoveInput.moveRight)
                        shipForceControl.moveDirection += shipRight * shipControlData.leftRightScale;

                    if (shipMoveInput.moveForward == false && shipMoveInput.moveBack == false
                        && shipMoveInput.moveLeft == false && shipMoveInput.moveRight == false)
                    {
                        shipForceControl.moveDirection = shipForward * shipControlData.forwardScale;
                    }
                    else if ((shipMoveInput.moveForward || shipMoveInput.moveBack)
                        && (shipMoveInput.moveLeft || shipMoveInput.moveRight))
                    {
                        shipForceControl.moveDirection *= shipControlData.moveComboScale;
                    }



                    //out
                    shipForceControl.noControlTime = 0f;
                    controlForceDirection.force = velocity.force;
                    controlForceDirection.maxVelocity = velocity.maxVelocity;
                    controlForceDirection.direction = shipForceControl.moveDirection;
                }
                else if (shipMoveInput.accelerate)
                {
                    if (shipControlData.accelerateForwardOnly == false)
                    {
                        if (shipMoveInput.moveForward)
                            shipForceControl.moveDirection = shipForward * shipControlData.forwardScale;
                        else if (shipMoveInput.moveBack)
                            shipForceControl.moveDirection = shipBack * shipControlData.backScale;

                        if (shipMoveInput.moveLeft)
                            shipForceControl.moveDirection += shipLeft * shipControlData.leftRightScale;
                        else if (shipMoveInput.moveRight)
                            shipForceControl.moveDirection += shipRight * shipControlData.leftRightScale;

                        if (shipMoveInput.moveForward == false && shipMoveInput.moveBack == false
                            && shipMoveInput.moveLeft == false && shipMoveInput.moveRight == false)
                        {
                            shipMoveInput.accelerate = false;
                        }
                        else if ((shipMoveInput.moveForward || shipMoveInput.moveBack)
                         && (shipMoveInput.moveLeft || shipMoveInput.moveRight))
                        {
                            shipForceControl.moveDirection *= shipControlData.moveComboScale;
                        }
                    }
                    else
                    {
                        shipForceControl.moveDirection = math.forward(rotation.Value);
                    }

                    shipForceControl.noControlTime = 0f;
                }
            }
        }

        [BurstCompile]
        public struct AcceleratePowerJob : IJobForEach_BCCCCC<ActorAttribute3Modifys<_Power>, ActorAttribute3<_Power>, ShipForceControl, ShipControlInfo, ShipMoveInput, ActorAttribute1<_VelocityLevel>>
        {
            public float fixedDeltaTime;

            public void Execute(DynamicBuffer<ActorAttribute3Modifys<_Power>> powerModifys, [ReadOnly] ref ActorAttribute3<_Power> power, ref ShipForceControl shipForceControl,
                [ReadOnly] ref ShipControlInfo shipControlData, [ReadOnly] ref ShipMoveInput shipMoveInput, [ReadOnly] ref ActorAttribute1<_VelocityLevel> shipVelocityLevel)
            {
                shipForceControl.accelerateFire = false;

                //
                if (shipMoveInput.accelerate)
                {
                    //
                    var velocityLevel = (int)shipVelocityLevel.value;
                    if (velocityLevel <= 0)
                        return;

                    //
                    var accelerate = shipControlData.getAccelerate(velocityLevel);

                    if (power.value >= 0f)
                    {
                        powerModifys.Add(new ActorAttribute3Modifys<_Power>
                        {
                            //player = weaponInstalledState.shipActorOwner.playerEntity,//自己消耗自己的power 就不需要知道是谁消耗了
                            value = -accelerate.consumePower * fixedDeltaTime,
                            attribute3ModifyType = Attribute3SubModifyType.ValueOffset
                        });

                        shipForceControl.accelerateFire = true;
                    }
                }
            }
        }

        //[BurstCompile]
        public struct AccelerateJob : IJobForEachWithEntity<ControlForceDirection, ShipForceControl, ShipControlInfo, ActorAttribute1<_VelocityLevel>, Translation>
        {
            public EntityCommandBuffer.Concurrent middleCommandBuffer;

            public void Execute(Entity shipEntity, int index,
                ref ControlForceDirection controlForceDirection, ref ShipForceControl shipForceControl,
                [ReadOnly] ref ShipControlInfo shipControlData, [ReadOnly] ref ActorAttribute1<_VelocityLevel> shipVelocity, [ReadOnly]ref Translation translation)
            {
                var velocityLevel = (int)shipVelocity.value;

                if (shipForceControl.OnAccelerate(velocityLevel))
                {
                    var accelerate = shipControlData.getAccelerate(velocityLevel);

                    //out
                    controlForceDirection.force = accelerate.force;
                    controlForceDirection.maxVelocity = accelerate.maxVelocity;
                    controlForceDirection.direction = shipForceControl.moveDirection;

                    /*if (accelerateMessage && disableMessage == false)
                    {
                        AccelerateFxSpawner.createInServer(middleCommandBuffer, index, shipEntity, translation);
                    }*/
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var inputDepsA = new DragByShipForwordJob
            {
            }
            .Schedule(this, inputDeps);


            var inputDepsB = new TorqueJob
            {
                fixedDeltaTime = Time.fixedDeltaTime,
            }
            .Schedule(this, inputDeps);


            inputDeps = new ForceJob
            {
                fixedDeltaTime = Time.fixedDeltaTime,
            }
            .Schedule(this, inputDeps);

            inputDeps = new AcceleratePowerJob
            {
                fixedDeltaTime = Time.fixedDeltaTime,
            }
            .Schedule(this, inputDeps);

            inputDeps = new AccelerateJob
            {
                middleCommandBuffer = middleBarrier.CreateCommandBuffer().ToConcurrent(),
            }
            .Schedule(this, inputDeps);
            middleBarrier.AddJobHandleForProducer(inputDeps);


            return JobHandle.CombineDependencies(inputDepsA, inputDepsB, inputDeps);
        }
    }

    [DisableAutoCreation]
    //[AlwaysUpdateSystem]
    public class ShipControlClientSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var inputDepsA = new ShipControlServerSystem.TorqueJob
            {
                fixedDeltaTime = Time.fixedDeltaTime,
            }
            .Schedule(this, inputDeps);

            inputDeps = new ShipControlServerSystem.ForceJob
            {
                fixedDeltaTime = Time.fixedDeltaTime,
            }
            .Schedule(this, inputDeps);

            inputDeps = new ShipControlServerSystem.AcceleratePowerJob
            {
                fixedDeltaTime = Time.fixedDeltaTime,
            }
            .Schedule(this, inputDeps);

            inputDeps = new ShipControlServerSystem.AccelerateJob
            {
            }
            .Schedule(this, inputDeps);

            return JobHandle.CombineDependencies(inputDepsA, inputDeps);
        }
    }
}
//ForceMode in Unity3D
//https://www.cnblogs.com/yaohj/p/4890618.html