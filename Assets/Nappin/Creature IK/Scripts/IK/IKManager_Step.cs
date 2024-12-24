using UnityEngine;
using UnityEngine.Events;


namespace CreatureIK
{
    public class IKManager_Step : MonoBehaviour
    {
        [Header("Joints specifics")]
        [Tooltip("Joints from root to end")]
        public Transform[] joints;
        [Tooltip("Lift applied to all the joints")]
        public AnimationCurve jointsLift;

        [Header("Step specifics")]
        [Tooltip("Layer used as ground")]
        public LayerMask groundLayer;
        [Tooltip("Threshold after which a step is triggered")]
        [Min(0.01f)]
        public float thresholdStep = 0.75f;
        [Tooltip("Foot distance from the body")]
        [Min(0.01f)]
        public float footBodyDistance = 2f;

        [Header("Step animation")]
        [Tooltip("Movement of the foot when a step is executed")]
        public AnimationCurve stepAnimation;
        [Tooltip("Step speed")]
        public float stepAnimationSpeed = 5f;
        [Tooltip("Step height")]
        public float stepHeight = 1.2f;

        [Header("Leg in body")]
        [Tooltip("Legs with the same leg group inside a body can move at the same time")]
        public int legGroup = 0;
        public UnityEvent hasStepped;
        [Space(15)]

        public bool showDebug = true;


        private Transform target;
        private Vector3[] jointsPos;
        private float[] jointsDist;
        private bool hasBodyParent = false;
        private float[] jointLiftArr;

        private Vector3 stepOffset;
        private Vector3 raycastHitPoint;
        private Vector3 groundTarget;

        private bool canStep = true;
        private bool triggerStep = false;
        private float curveDeltaTime = 0f;

        private bool isGrounded = true;
        private float maxExtension = 0f;

        private Quaternion prevTargetRot;
        private Vector3 prevTargetPos;

        private Vector3 prevGroundTarget;
        private Vector3 stepDistance;
        private bool startedStep = true;
        private Vector3 groundDirection = Vector3.down;


        /**/


        #region Setup

        private void Awake()
        {
            //assign vars
            target = this.transform;

            //create array pos
            jointsPos = new Vector3[joints.Length];
            for (int i = 0; i < jointsPos.Length; i++) jointsPos[i] = joints[i].position;
            groundTarget = jointsPos[jointsPos.Length - 1];

            //create array distances
            jointsDist = new float[joints.Length - 1];
            for (int i = 0; i < joints.Length - 1; i++)
            {
                jointsDist[i] = (jointsPos[i + 1] - jointsPos[i]).magnitude;
                maxExtension += jointsDist[i];
            }

            //fill joints lifts
            jointLiftArr = new float[joints.Length];
            for (int i = 0; i < joints.Length; i++) jointLiftArr[i] = jointsLift.Evaluate(1f / joints.Length * (i - 1)) / 100f;

            StoreTarget();
        }


        private void Update()
        {
            if (triggerStep || (!hasBodyParent && (prevTargetRot != target.rotation || prevTargetPos != target.position))) UpdateStep();
        }


        public void ManualUpdate()
        {
            UpdateStep();
        }


        private void UpdateStep()
        {
            CheckGround();
            TriggerStep();
            if (isGrounded) UpdateJoints();

            StoreTarget();
        }

        #endregion


        #region Solve Step

        private void UpdateJoints()
        {
            jointsPos[0] = target.position;
            jointsPos = Utils.IKUtils.SolvePointsFB(jointsPos, jointsDist, groundTarget, jointLiftArr, 100);

            for (int i = 0; i < jointsPos.Length - 1; i++)
            {
                joints[i].position = jointsPos[i];
                joints[i].rotation = Quaternion.LookRotation((jointsPos[i + 1] - jointsPos[i]).normalized) * Quaternion.Euler(90f, 0f, 0f);
            }
        }


        private void CheckGround()
        {
            //check ground
            RaycastHit hit;
            stepOffset = jointsPos[0] + target.right * footBodyDistance;
            if (Physics.Raycast(stepOffset, groundDirection, out hit, Mathf.Infinity, groundLayer)) raycastHitPoint = hit.point;

            //set pos if just reached ground again
            isGrounded = (raycastHitPoint - jointsPos[0]).magnitude < maxExtension;
            if (!isGrounded) for (int i = 0; i < joints.Length; i++) jointsPos[i] = joints[i].position;
        }


        private void StoreTarget()
        {
            prevTargetPos = target.position;
            prevTargetRot = target.rotation;
        }

        public void HasBodyParent(bool _hasParent)
        {
            hasBodyParent = _hasParent;
        }

        #endregion


        #region Step Handlers

        private void TriggerStep()
        {
            if ((raycastHitPoint - jointsPos[jointsPos.Length - 1]).magnitude > thresholdStep && canStep) triggerStep = true;

            if (triggerStep)
            {
                //get current step positions
                if (startedStep)
                {
                    prevGroundTarget = groundTarget;
                }
                startedStep = false;
                stepDistance = raycastHitPoint - prevGroundTarget;

                curveDeltaTime += Time.deltaTime * stepAnimationSpeed;
                groundTarget = prevGroundTarget + stepDistance * (curveDeltaTime) + new Vector3(0f, stepAnimation.Evaluate(curveDeltaTime) * stepHeight, 0f);

                if (curveDeltaTime >= 1f)
                {
                    hasStepped.Invoke();
                    triggerStep = false;
                    curveDeltaTime = 0f;
                    startedStep = true;
                }
            }
        }


        public void CanStep(bool _status)
        {
            canStep = _status;
        }

        #endregion


        private void OnDrawGizmos()
        {
            if (Application.isPlaying && showDebug)
            {
                if (isGrounded) Gizmos.color = Color.yellow;
                else Gizmos.color = Color.red;
                Gizmos.DrawLine(stepOffset, raycastHitPoint);

                Gizmos.color = Color.blue;
                for (int i = 0; i < jointsPos.Length - 1; i++) Gizmos.DrawLine(jointsPos[i], jointsPos[i + 1]);

                Gizmos.color = Color.red;
                for (int i = 0; i < jointsPos.Length - 1; i++) Gizmos.DrawWireSphere(jointsPos[i], 0.1f);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(stepOffset, Vector3.one / 10f);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(raycastHitPoint, 0.1f);
            }
        }
    }
}