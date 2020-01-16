using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace RN.Network.SpaceWar
{
    public class CameraController : ICameraController
    {
        //
        Rewired.Player input;
        Rewired.Mouse mouseInput;

        public Transform eye;
        public Transform target;
        public Transform aimTarget;
        public Animator zoomTarget;
        //public Transform rotationTarget;

        private void Awake()
        {
            //
            input = Rewired.ReInput.players.GetPlayer(InputPlayer.Player0);
            mouseInput = input.controllers.Mouse;

            //
            Debug.Assert(eye != null, "eye != null", this);
            Debug.Assert(target != null, "target != null", this);
            Debug.Assert(aimTarget != null, "aimTarget != null", this);
            Debug.Assert(zoomTarget != null, "zoomTarget != null", this);
            //Debug.Assert(rotationTarget != null, "rotationTarget != null", this);


            //
            var parentConstraint = Camera.main.GetComponent<ParentConstraint>();
            parentConstraint.SetSource(0, new ConstraintSource { weight = 1f, sourceTransform = eye });
        }
        public override void update(bool hasShip, in float3 shipPosition, in quaternion shipRotation, in float3 shipLinear,
            in MouseDataSingleton mouseData,
            out CameraDataSingleton cameraData)
        {
            updateAngleY();

            if (hasShip)
            {
                moveWithShip(shipPosition);
                aimByMouse(mouseData);
            }
            else
            {
                moveWithoutShip();
            }

            aimByShipLinear(shipLinear);
            zoomByMouse();

            cameraData.targetPosition = target.position;
            cameraData.targetRotation = target.rotation;
        }




        [Header("target")]
        public float targetSmoothTime = 0.5f;
        public float moveScale = 20f;

        Vector3 targetCurrentVelocity;
        Vector3 move3d_last;

        void moveWithShip(in float3 shipPosition)
        {
            target.position = Vector3.SmoothDamp(target.position, shipPosition, ref targetCurrentVelocity, targetSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
        }

        void moveWithoutShip()
        {
            var move2d = input.GetAxis2D(PlayerInputActions.Move_Horizontal, PlayerInputActions.Move_Vertical);
            if (move2d != Vector2.zero)
                move2d.Normalize();

            var move3d = new Vector3(move2d.x, 0f, move2d.y);

            move3d = target.TransformDirection(move3d);
            move3d = Vector3.SmoothDamp(move3d_last, move3d, ref targetCurrentVelocity, targetSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
            move3d_last = move3d;

            var position = target.position;
            position += move3d * moveScale * Time.fixedDeltaTime;
            target.position = position;
        }


        [Header("aimByMouse")]
        public float aimByMouseScale = 0.01f;
        void aimByMouse(in MouseDataSingleton mouseData)
        {
            target.position = Vector3.Lerp(target.position, mouseData.point, aimByMouseScale * zoomValue);
        }


        [Header("aimByShipLinear")]
        public float aimByShipLinearScale = 0.25f;
        public float aimByShipLinearSmoothTime = 0.25f;
        Vector3 aimByShipLinearCurrentVelocity;
        void aimByShipLinear(float3 shipLinear)
        {
            aimTarget.position = Vector3.SmoothDamp(aimTarget.position, target.position + (Vector3)shipLinear * aimByShipLinearScale, ref aimByShipLinearCurrentVelocity, aimByShipLinearSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
        }



        [Header("zoom")]
        public float zoomScale = 0.1f;
        public float zoomDampTime = 1f;
        readonly int zoomId = Animator.StringToHash("zoom");
        float zoomValue = 1f;
        void zoomByMouse()
        {
            var mouseZ = input.GetAxis(PlayerInputActions.MouseZ);
            zoomValue += mouseZ * zoomScale;
            zoomValue = Mathf.Clamp01(zoomValue);
            zoomTarget.SetFloat(zoomId, zoomValue, zoomDampTime, Time.fixedDeltaTime);
        }



        [Header("mouseX")]
        public float mouseXScale = 0.2f;
        public float eulerAngleYSmoothTime = 0.1f;
        float targetEulerAngleY;
        float currentEulerAngleYVelocity;
        void updateAngleY()
        {
            //
            if (input.GetButton(PlayerInputActions.MouseButton0))
            {
                targetEulerAngleY += mouseInput.screenPositionDelta.x * mouseXScale;
            }

            var eulerAngles = target.eulerAngles;
            eulerAngles.y = Mathf.SmoothDampAngle(eulerAngles.y, targetEulerAngleY, ref currentEulerAngleYVelocity, eulerAngleYSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
            target.eulerAngles = eulerAngles;
        }
    }
}
