using System.Collections.Generic;
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
    public class PlayerInputServerSystem : Network.PlayerInputServerSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            enterGameQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<PlayerEnterGameMessage>()/*, ComponentType.ReadOnly<Player>()*/ },
                None = new ComponentType[] { typeof(NetworkDisconnectedMessage) }
            });
        }
        protected EntityQuery enterGameQuery;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //
            if (enterGameQuery.IsEmptyIgnoreFilter == false)
            {
                EntityManager.AddComponent<PlayerShipMoveInputNetMessage>(enterGameQuery);
                EntityManager.AddComponent<PlayerShipFireInputNetMessage>(enterGameQuery);
            }

            //
            inputDeps = base.OnUpdate(inputDeps);


            //
            using (var moveInputCommandBuffer = new SampleCommandBuffer<ShipMoveInput>(Allocator.TempJob))
            using (var weaponInputCommandBuffer = new SampleCommandBuffer<WeaponInput>(Allocator.TempJob))
            {
                var inputDepsA = new MoveInputJob
                {
                    shipDisableMoveInputFromEntity = GetComponentDataFromEntity<ShipLostInputState>(true),

                    moveInputCommandBuffer = moveInputCommandBuffer.ToConcurrent(),
                }
                .Schedule(this, inputDeps);

                var inputDepsB = new FireInputJob
                {
                    shipDisableFireInputFromEntity = GetComponentDataFromEntity<ShipLostInputState>(true),

                    weaponInstalledStateFromEntity = GetComponentDataFromEntity<WeaponInstalledState>(true),
                    weaponControlFromEntity = GetComponentDataFromEntity<WeaponControl>(true),

                    translationFromEntity = GetComponentDataFromEntity<Translation>(true),
                    rotationFromEntity = GetComponentDataFromEntity<Rotation>(true),

                    weaponInputCommandBuffer = weaponInputCommandBuffer.ToConcurrent(),
                }
                .Schedule(this, inputDeps);

                moveInputCommandBuffer.Playback(EntityManager, inputDepsA);
                weaponInputCommandBuffer.Playback(EntityManager, inputDepsB);
            }

            return inputDeps;
        }


        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct MoveInputJob : IJobForEachWithEntity<PlayerActorArray, PlayerShipMoveInputNetMessage>
        {
            [ReadOnly] public ComponentDataFromEntity<ShipLostInputState> shipDisableMoveInputFromEntity;

            public SampleCommandBuffer<ShipMoveInput>.Concurrent moveInputCommandBuffer;

            public void Execute(Entity _, int index,
                [ReadOnly] ref PlayerActorArray playerActorArray,
                [ReadOnly, ChangedFilter] ref PlayerShipMoveInputNetMessage moveInput)
            {
                if (playerActorArray.shipEntity == Entity.Null)
                    return;

                if (shipDisableMoveInputFromEntity.Exists(playerActorArray.shipEntity))
                    return;

                //moveInput
                {
                    moveInputCommandBuffer.SetComponent(playerActorArray.shipEntity, moveInput.shipMoveInput);
                }
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        [ExcludeComponent(typeof(NetworkDisconnectedMessage))]
        struct FireInputJob : IJobForEachWithEntity<PlayerActorArray, PlayerShipFireInputNetMessage>
        {
            [ReadOnly] public ComponentDataFromEntity<ShipLostInputState> shipDisableFireInputFromEntity;

            public SampleCommandBuffer<WeaponInput>.Concurrent weaponInputCommandBuffer;

            [ReadOnly] public ComponentDataFromEntity<WeaponInstalledState> weaponInstalledStateFromEntity;
            [ReadOnly] public ComponentDataFromEntity<WeaponControl> weaponControlFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Translation> translationFromEntity;
            [ReadOnly] public ComponentDataFromEntity<Rotation> rotationFromEntity;

            public void Execute(Entity _, int index,
                [ReadOnly]ref PlayerActorArray playerActorArray,
                [ChangedFilter] ref PlayerShipFireInputNetMessage fireInput)
            {
                if (playerActorArray.shipEntity == Entity.Null)
                    return;

                if (shipDisableFireInputFromEntity.Exists(playerActorArray.shipEntity))
                    return;


                var fireAction = fireInput.fireAction;
                if (fireAction == FireAction.none)
                    return;
                fireInput.fireAction = FireAction.none;


                var weaponEntity = Entity.Null;
                var weaponInput = new WeaponInput { fireType = FireType.Fire, firePosition = new float3(fireInput.firePosition.x, 0f, fireInput.firePosition.y) };


                if (fireAction >= FireAction.fireSlotIndex0 && fireAction < FireAction.__fireSlotIndexEnd)
                {
                    var weaponIndex = (int)fireAction;
                    weaponIndex -= (int)FireAction.__fireSlotIndexBegin;
                    weaponEntity = playerActorArray.GetWeaponEntity(weaponIndex);
                }
                else if (fireAction >= FireAction.uninstallSlotIndex0 && fireAction < FireAction.__uninstallSlotIndexEnd)
                {
                    var weaponIndex = (int)fireAction;
                    weaponIndex -= (int)FireAction.__uninstallSlotIndexBegin;
                    weaponEntity = playerActorArray.GetWeaponEntity(weaponIndex);

                    weaponInput.fireType = FireType.Uninstall;
                }
                else if (fireAction >= FireAction.uninstallAssistSlotIndex0 && fireAction < FireAction.__uninstallAssistSlotIndexEnd)
                {
                    var weaponIndex = (int)fireAction;
                    weaponIndex -= (int)FireAction.__uninstallAssistSlotIndexBegin;
                    weaponEntity = playerActorArray.GetAssistWeaponEntity(weaponIndex);

                    weaponInput.fireType = FireType.Uninstall;
                }
                else if (fireAction == FireAction.shield)
                {
                    weaponEntity = playerActorArray.curShieldWeaponEntity;
                }
                else if (fireAction == FireAction.mainFire)//中键 发射主炮
                {
                    weaponEntity = getMainSlotWeaponEntity(playerActorArray);
                }
                /*else if (fireAction == FireAction.mouseButton2)//右键 发射远离鼠标一侧的侧面炮火 发送方向取反射角度
                {
                    var shipPosition = translationFromEntity[playerActorArray.shipEntity].Value;
                    var shipRotation = rotationFromEntity[playerActorArray.shipEntity].Value;
                    var fireDirection = firePosition - shipPosition;
                    fireDirection = math.reflect(-fireDirection, math.forward(shipRotation.value));
                    firePosition = shipPosition + fireDirection;

                    weaponEntity = getWeaponEntity(playerActorArray, firePosition);
                }*/
                else if (fireAction == FireAction.autoFire)//右键 发射靠近鼠标一侧的侧面炮火
                {
                    weaponEntity = getWeaponEntity(playerActorArray, weaponInput.firePosition);
                }
                else
                {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    throw new System.IndexOutOfRangeException();
#endif
                }


                //
                if (weaponEntity != Entity.Null)
                {
                    weaponInputCommandBuffer.SetComponent(weaponEntity, weaponInput);
                }
            }

            unsafe Entity getMainSlotWeaponEntity(in PlayerActorArray playerActorArray)
            {
                var count = 0;
                var array = stackalloc Entity[PlayerActorArray.WeaponMaxCount];
                var j = 0;
                for (var i = 0; i < PlayerActorArray.WeaponMaxCount; ++i)
                {
                    var weaponEntity = playerActorArray.GetWeaponEntity(i);
                    if (weaponEntity != Entity.Null)
                    {
                        var weaponInstalledState = weaponInstalledStateFromEntity[weaponEntity];

                        if (weaponInstalledState.slot.main)
                        {
                            ++count;
                            array[j++] = weaponEntity;
                        }
                    }
                }

                if (count == 0)
                {
                    return Entity.Null;
                }
                else if (count == 1)
                {
                    return array[0];
                }
                else
                {
                    var msArray = stackalloc _ES[count];

                    for (var i = 0; i < count; ++i)
                    {
                        msArray[i] = new _ES { entity = array[i], score = getMainWeaponScore(array[i]) };
                    }

                    return getFirst(msArray, count);
                }
            }

            unsafe Entity getWeaponEntity(in PlayerActorArray playerActorArray, in float3 firePosition)
            {
                var array = stackalloc _ES[PlayerActorArray.WeaponMaxCount];

                for (var i = 0; i < PlayerActorArray.WeaponMaxCount; ++i)
                {
                    var weaponEntity = playerActorArray.GetWeaponEntity(i);
                    array[i] = new _ES { entity = weaponEntity, score = getWeaponScore(weaponEntity, firePosition) };
                }

                return getFirst(array, PlayerActorArray.WeaponMaxCount);
            }


            struct _ES
            {
                public Entity entity;
                public int score;
            }
            struct WeaponComparer : IComparer<_ES>
            {
                public int Compare(_ES x, _ES y)
                {
                    return x.score - y.score;
                }
            }

            unsafe Entity getFirst(_ES* array, int count)
            {
                _ES outES = default;
                for (var i = 0; i < count; ++i)
                {
                    var es = array[i];

                    if (es.score < outES.score)
                    {
                        outES = es;
                    }
                }

                return outES.entity;
            }


            int getMainWeaponScore(Entity mainWeaponEntity)
            {
                var weaponControl = weaponControlFromEntity[mainWeaponEntity];
                var s = 0;
                if (weaponControl.inputTime <= 0f)
                {
                    s -= 1;
                }
                return s;
            }

            const float AngleScore = 200f;
            const float TimeScore = 1000f;
            int getWeaponScore(Entity weaponEntity, float3 firePosition)
            {
                var s = 0;
                if (weaponEntity != Entity.Null)
                {
                    var weaponInstalledState = weaponInstalledStateFromEntity[weaponEntity];

                    if (weaponInstalledState.autoFire == false)//右键自动开火
                    {
                    }
                    else if (weaponInstalledState.slot.halfAngleLimitMin > 0f)
                    {
                        var slotPosition = translationFromEntity[weaponInstalledState.slotEntity].Value;
                        var slotRotation = rotationFromEntity[weaponInstalledState.slotEntity].Value;

                        var fireDirection = firePosition - slotPosition;
                        fireDirection = math.normalize(fireDirection);


                        var weaponControl = weaponControlFromEntity[weaponEntity];
                        var angle = Vector3.Angle(math.forward(slotRotation), fireDirection);
                        if (angle < weaponInstalledState.slot.halfAngleLimitMin)
                        {
                            var _as = (int)((1f - angle / weaponInstalledState.slot.halfAngleLimitMin) * AngleScore);
                            var _ts = (int)((1f - weaponControl.inputTimePresent) * TimeScore);
                            s -= _as;
                            s -= _ts;

                            //Debug.Log($"s={s}  weaponEntity={weaponEntity}  as={_as}  ts={_ts}:{weaponControl.inputTimePresent}");
                        }
                    }
                }
                return s;
            }
        }
    }

    [DisableAutoCreation]
    [AlwaysUpdateSystem]
    public class PlayerInputClientSystem : Network.PlayerInputClientSystem
    {
        IActorSpawnerMap actorSpawnerMap;

        Rewired.Player input;
        Rewired.Mouse mouseInput;
        IMouseFx mouseFx;
        protected new void OnInit(Transform root)
        {
            base.OnInit(root);

            //
            World.GetExistingSystem<NetworkStreamConnectSuccessSystem>().AddSystem(this);

            //
            actorSpawnerMap = root.GetComponentInChildren<IActorSpawnerMap>();
            Debug.Assert(actorSpawnerMap != null, $"actorSpawnerMap != null  root={root}", root);

            //
            input = Rewired.ReInput.players.GetPlayer(InputPlayer.Player0);
            mouseInput = input.controllers.Mouse;

            //
            rootPlane = new Plane(Vector3.up, root.position);

            var singletonEntity = GetSingletonEntity<MyPlayerSingleton>();
            EntityManager.AddComponent<MouseDataSingleton>(singletonEntity);


            //
            mouseFx = GameObject.Find("mousePoint").GetComponent<IMouseFx>();
        }

        protected override void OnUpdate()
        {
            if (actorInput())
                return;

            base.OnUpdate();
        }

        Plane rootPlane;
        bool getMousePointOnRootPlane(out float3 point)
        {
            var ray = Camera.main.ScreenPointToRay(mouseInput.screenPosition);
            var b = rootPlane.raycast(ray, out Vector3 p);
            point = p;
            return b;
        }

        public float mousePointMaxDistance = 50f;
        void updateMouse(ref MouseDataSingleton mouseData)
        {
            mouseData = default;
            var cameraData = GetSingleton<CameraDataSingleton>();
            if (getMousePointOnRootPlane(out var mousePoint))
            {
                var direction = mousePoint - cameraData.targetPosition;

                if (direction.Equals(float3.zero))
                {
                    mouseData.point = mousePoint;
                    mouseData.direction = new float3(0f, 0f, 1f);
                    mouseData.distance = 0f;
                }
                else
                {
                    direction.y = 0;
                    var distance = math.length(direction);
                    direction = math.normalize(direction);
                    if (distance > mousePointMaxDistance)
                    {
                        distance = mousePointMaxDistance;
                        mousePoint = cameraData.targetPosition + direction * mousePointMaxDistance;
                    }

                    mouseData.point = mousePoint;
                    mouseData.direction = direction;
                    mouseData.distance = distance;
                }
            }
            else
            {
                mouseData.point = cameraData.targetPosition;
                mouseData.direction = new float3(0f, 0f, 1f);
            }

            SetSingleton(mouseData);
        }



        PlayerActorMoveInputSerialize lastShipMoveInput;
        public float maxAngle = 90f;
        bool actorInput()
        {
            //
            MouseDataSingleton mouseSingleton = default;
            updateMouse(ref mouseSingleton);


            //
            var myPlayerSingleton = GetSingleton<MyPlayerSingleton>();
            var myPlayerEntity = myPlayerSingleton.playerEntity;
            if (myPlayerEntity == Entity.Null)
                return false;
            if (EntityManager.HasComponent<PlayerActorArray>(myPlayerEntity) == false)
                return false;
            var actors = EntityManager.GetComponentData<PlayerActorArray>(myPlayerEntity);
            if (actors.shipEntity == Entity.Null)
                return false;

            if (EntityManager.HasComponent<ShipControlInfo>(actors.shipEntity) == false)
            {
                var actor = EntityManager.GetComponentData<Actor>(actors.shipEntity);
                var shipSpawner = actorSpawnerMap.GetActorSpawner(actor.actorType) as ShipSpawner;
                shipSpawner.AddComponentTypesInMyShip(actors.shipEntity);
            }


            //if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() == false)
            {
                float3 shipPosition = EntityManager.GetComponentData<Translation>(actors.shipEntity).Value;
                quaternion shipRotation = EntityManager.GetComponentData<Rotation>(actors.shipEntity).Value;


                //
                var observerSingleton = GetSingleton<ObserverSingleton>();
                {
                    observerSingleton.position = (half2)math.lerp(shipPosition, mouseSingleton.point, 0.5f).xz;
                }
                SetSingleton(observerSingleton);


                //
                Entities
                    .WithAll<NetworkUnreliableOutBuffer>().WithAllReadOnly<NetworkConnection>()
                    .WithNone<NetworkDisconnectedMessage>()
                    .ForEach((DynamicBuffer<NetworkUnreliableOutBuffer> outBuffer) =>
                    {
                        //
                        if (EntityManager.HasComponent<ShipLostInputState>(actors.shipEntity) == false)
                        {
                            var torqueY = 0f; //[0,1]
                            if (input.GetButton(PlayerInputActions.Torque))
                            {
                                var angle = Vector3.SignedAngle(math.forward(shipRotation), mouseSingleton.direction, Vector3.up);
                                angle = Mathf.Clamp(angle, -maxAngle, maxAngle);
                                torqueY = math.pow(angle / maxAngle, 2f);
                                if (angle < 0f)
                                    torqueY = -torqueY;
                            }
                            else
                            {
                                torqueY = input.GetButton(PlayerInputActions.Torque_Left) ? -1f : input.GetButton(PlayerInputActions.Torque_Right) ? 1f : 0f;
                            }


                            var move = input.GetAxis2D(PlayerInputActions.Move_Horizontal, PlayerInputActions.Move_Vertical);
                            var s = new PlayerActorMoveInputSerialize
                            {
                                observerPosition = observerSingleton.position,
                                shipMoveInput = new ShipMoveInput
                                {
                                    moveForward = move.y > 0f,
                                    moveBack = move.y < 0f,
                                    moveLeft = move.x < 0f,
                                    moveRight = move.x > 0f,

                                    torqueY = (half)torqueY,

                                    accelerate = input.GetButton(PlayerInputActions.Accelerate),
                                }
                            };

                            if (math.distance(s.observerPosition, lastShipMoveInput.observerPosition) > 10f
                            || s.shipMoveInput.moveForward != lastShipMoveInput.shipMoveInput.moveForward
                            || s.shipMoveInput.moveBack != lastShipMoveInput.shipMoveInput.moveBack
                            || s.shipMoveInput.moveLeft != lastShipMoveInput.shipMoveInput.moveLeft
                            || s.shipMoveInput.moveRight != lastShipMoveInput.shipMoveInput.moveRight
                            || s.shipMoveInput.torqueY != lastShipMoveInput.shipMoveInput.torqueY
                            || s.shipMoveInput.accelerate != lastShipMoveInput.shipMoveInput.accelerate)
                            {
                                s._DoSerialize(outBuffer);

                                lastShipMoveInput = s;
                                EntityManager.SetComponentData(actors.shipEntity, s.shipMoveInput);
                            }
                        }


                        //
                        if (EntityManager.HasComponent<ShipLostInputState>(actors.shipEntity) == false)
                        {
                            //
                            FireAction fireAction = FireAction.none;
                            for (var i = PlayerInputActions.Weapon0; i <= PlayerInputActions.Weapon6; ++i)
                            {
                                if (input.GetButtonDown(i))
                                {
                                    var index = i - PlayerInputActions.Weapon0;
                                    if (input.GetButton(PlayerInputActions.Shift))
                                    {
                                        fireAction = (FireAction)index + (int)FireAction.__uninstallSlotIndexBegin;
                                    }
                                    else
                                    {
                                        fireAction = (FireAction)index + (int)FireAction.__fireSlotIndexBegin;
                                    }
                                    break;
                                }
                            }


                            if (fireAction == FireAction.none)
                            {
                                for (var i = PlayerInputActions.AssistWeapon0; i <= PlayerInputActions.AssistWeapon2; ++i)
                                {
                                    if (input.GetButtonDown(i))
                                    {
                                        var index = i - PlayerInputActions.AssistWeapon0;
                                        if (input.GetButton(PlayerInputActions.Shift))
                                        {
                                            fireAction = (FireAction)index + (int)FireAction.__uninstallAssistSlotIndexBegin;
                                        }
                                        else
                                        {
                                            //fireAction = (FireAction)index + (int)FireAction.__fireSlotIndexBegin;
                                        }
                                        break;
                                    }
                                }
                            }


                            //
                            if (fireAction == FireAction.none)
                            {
                                if (input.GetButtonDown(PlayerInputActions.MouseButton2))
                                {
                                    fireAction = FireAction.autoFire;
                                }
                                else if (input.GetButtonDown(PlayerInputActions.MouseButton1))
                                {
                                    fireAction = FireAction.mainFire;
                                }
                                /*else if (input.GetButtonDown(PlayerInputActions.MouseButton2))
                                {
                                    fireAction = FireAction.mouseButton2;
                                }*/
                                else if (input.GetButtonDown(PlayerInputActions.Shield))
                                {
                                    fireAction = FireAction.shield;
                                }
                            }



                            if (fireAction != FireAction.none)
                            {
                                //
                                var s = new PlayerActorFireInputSerialize
                                {
                                    firePosition = (half2)mouseSingleton.point.xz,

                                    fireAction = fireAction,
                                };
                                s._DoSerialize(outBuffer);


                                //
                                mouseFx.OnPlayFx(mouseSingleton.point);
                            }
                        }
                    });

            }

            return true;
        }
    }
}
