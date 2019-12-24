using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace RN.Network.SpaceWar
{
    public partial class CameraControllerSetting : MonoBehaviour
    {
        public CameraSetting data;
    }

    [System.Serializable]
    public class CameraSetting
    {
        [Header("target")]
        public float targetSmoothTime = 0.5f;
        public float moveScale = 20f;

        [Header("aim")]
        public float aimSmoothTime = 0.75f;
        public float lookForwardScale = 2.5f;
        public float moveLinearScale = 75f;

        [Header("zoom")]
        public float zoomScale = 0.1f;
        public float zoomDampTime = 1f;
        public float overMoveLength = 7.5f;
        public float zoomBScale = 0.005f;

        [Header("mouseX")]
        public float mouseXScale = 0.2f;
        public float eulerAngleYSmoothTime = 0.1f;
    }

    public struct CameraControllerSingleton : IComponentData
    {
        public quaternion targetRotation;
    }

    public class CameraControllerSystem : ComponentSystem
    {
        //
        Rewired.Player input;
        Rewired.Mouse mouseInput;

        //
        CameraSetting setting;

        //
        Transform target;
        Transform aimTarget;
        Animator animator;

        Vector3 targetCurrentVelocity;
        Vector3 currentAimVelocity;

        Vector3 move3d_last;


        float targetEulerAngleY;
        float currentEulerAngleYVelocity;

        //
        float zoomA = 1f;
        float zoomB = 0f;
        readonly int zoomId = Animator.StringToHash("zoom");

        CameraControllerSetting controller
        {
            set
            {
                setting = value.data;

                target = value.transform.Find(nameof(target));
                aimTarget = value.transform.Find(nameof(aimTarget));

                animator = value.transform.Find(nameof(animator)).GetComponent<Animator>();


                targetEulerAngleY = target.eulerAngles.y;


                var parentConstraint = Camera.main.GetComponent<ParentConstraint>();
                parentConstraint.SetSource(0, new ConstraintSource { weight = 1f, sourceTransform = value.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0) });
                //parentConstraint.SetSources(new List<ConstraintSource>() { new ConstraintSource { weight = 1f, sourceTransform = ccsetting.transform.GetChild(0).GetChild(0) } });
            }
        }

        protected void OnInit(Transform root)
        {
            input = Rewired.ReInput.players.GetPlayer(InputPlayer.Player0);
            mouseInput = input.controllers.Mouse;

            controller = GameObject.FindObjectOfType<CameraControllerSetting>();


            var singletonEntity = GetSingletonEntity<MyPlayerSingleton>();
            EntityManager.AddComponentData(singletonEntity, new CameraControllerSingleton { targetRotation = target.rotation, });
        }

        protected override void OnUpdate()
        {
            if (setting == null)
                return;

            //
            float3 moveLinear = float3.zero;
            var hasShip = updateByShip(ref moveLinear);
            if (hasShip == false)
            {
                updateWithoutShip(ref moveLinear);
            }

            //
            updateByMouse(moveLinear, hasShip);
        }


        bool updateByShip(ref float3 moveLinear)
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


            float3 shipPosition = EntityManager.GetComponentObject<Transform>(actors.shipEntity).position;
            moveLinear = EntityManager.GetComponentData<RigidbodyVelocity>(actors.shipEntity).linear;

            //
            shipPosition += moveLinear * Time.fixedDeltaTime;
            target.position = Vector3.SmoothDamp(target.position, shipPosition, ref targetCurrentVelocity, setting.targetSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);

            return true;
        }


        void updateWithoutShip(ref float3 moveLinear)
        {
            var move2d = input.GetAxis2D(PlayerInputActions.Move_Horizontal, PlayerInputActions.Move_Vertical);
            if (move2d != Vector2.zero)
                move2d.Normalize();

            var move3d = new Vector3(move2d.x, 0f, move2d.y);

            move3d = target.TransformDirection(move3d);
            move3d = Vector3.SmoothDamp(move3d_last, move3d, ref targetCurrentVelocity, setting.targetSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
            move3d_last = move3d;

            var position = target.position;
            position += move3d * setting.moveScale * Time.fixedDeltaTime;
            target.position = position;

            //
            moveLinear = move3d * setting.moveScale;
        }


        void updateByMouse(float3 moveLinear, bool hasShip)
        {
            //
            {
                if (input.GetButton(PlayerInputActions.MouseButton0))
                {
                    targetEulerAngleY += mouseInput.screenPositionDelta.x * setting.mouseXScale;
                }

                var eulerAngles = target.eulerAngles;
                eulerAngles.y = Mathf.SmoothDampAngle(eulerAngles.y, targetEulerAngleY, ref currentEulerAngleYVelocity, setting.eulerAngleYSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
                target.eulerAngles = eulerAngles;


                var singletonEntity = GetSingletonEntity<MyPlayerSingleton>();
                EntityManager.SetComponentData(singletonEntity, new CameraControllerSingleton { targetRotation = target.rotation, });
            }



            //
            {
                //瞄准点不能在角色后面
                var mouseSingleton = GetSingleton<MouseSingleton>();
                var targetForward = math.forward(target.rotation);
                var aimDot = math.dot(targetForward, moveLinear);
                if (aimDot < 0f)
                {
                    aimDot = 0f;
                    moveLinear = default;
                }
                aimTarget.position = Vector3.SmoothDamp(aimTarget.position,
                    mouseSingleton.point + targetForward * setting.lookForwardScale + moveLinear * Time.fixedDeltaTime * setting.moveLinearScale,
                    ref currentAimVelocity,
                    setting.aimSmoothTime,
                    Mathf.Infinity, Time.fixedDeltaTime);

                //
                if (hasShip)
                {
                    var moveLength = math.length(moveLinear);
                    zoomB = 1f;
                    if (aimDot > 0f && moveLength > setting.overMoveLength)
                    {
                        zoomB -= (moveLength - setting.overMoveLength) * aimDot * setting.zoomBScale;
                        zoomB = Mathf.Clamp(zoomB, 0.5f, 1f);
                    }
                }
            }



            //
            if (hasShip)
            {
                var mouseZ = input.GetAxis(PlayerInputActions.MouseZ);

                zoomA += mouseZ * setting.zoomScale;
                zoomA = Mathf.Clamp01(zoomA);

                animator.SetFloat(zoomId, zoomA * zoomB, setting.zoomDampTime, Time.fixedDeltaTime);
            }
            else
            {
                animator.SetFloat(zoomId, 1f, setting.zoomDampTime, Time.fixedDeltaTime);
            }
        }
    }
}
