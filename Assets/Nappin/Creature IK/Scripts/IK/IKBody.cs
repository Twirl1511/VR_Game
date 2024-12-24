using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CreatureIK
{
    public class IKBody : MonoBehaviour
    {
        [Header("Legs specifics")]
        [Tooltip("Legs contained in the pivot gameobject - non in any particular order")]
        public IKManager_Step[] legs;
        public IKManager_Freeform[] tails;

        [Header("Body movement specifics")]
        [Tooltip("Layer where the step can walk on")]
        public LayerMask groundLayer;
        [Tooltip("Pivot child that manages body tilt")]
        public Transform bodyPivot;
        [Space(15)]

        [Tooltip("Layer where the step can walk on")]
        public float defaultTilt = 5f;
        [Tooltip("Tilt range on local X axis when the IK moves")]
        public Vector2 tiltRange_X = new Vector2(-10f, 10f);
        [Tooltip("Tilt range on local Y axis when the IK moves")]
        public Vector2 tiltRange_Y = new Vector2(-3f, 3f);
        [Space(15)]

        [Tooltip("Multiplier of the momentum tilt")]
        [Min(0f)]
        public float bodyTiltMultiplier = 60f;
        [Tooltip("Damp of the momentum tilt")]
        [Min(0f)]
        public float bodyTiltDamp = 9f;
        [Tooltip("Contribution of a slope to the IK tilt")]
        [Range(0f, 1f)]
        public float slopeTiltContribution = 0.328f;
        [Tooltip("Is tilt enabled")]
        public bool enableBodyTilt = true;
        [Space(15)]

        public bool showDebug = true;


        private Transform target;
        private Transform[] legsParent;
        private int[] stepBind;

        private List<List<IKManager_Step>> groupBind;
        private int currentAllowedSteppers = 0;

        private Vector3 raycastHitPoint;
        private Vector3 raycastHitNormal;

        private Quaternion refTiltRot;
        private Quaternion prevBodyRot;
        private Vector3 prevTargetPos;


        /**/


        #region Setup

        private void Awake()
        {
            //setup vars
            target = this.transform;
            legsParent = new Transform[legs.Length];
            stepBind = new int[legs.Length];

            for (int i = 0; i < legs.Length; i++)
            {
                legsParent[i] = legs[i].transform;
                stepBind[i] = legs[i].legGroup;
                legs[i].HasBodyParent(true);
            }

            for (int i = 0; i < tails.Length; i++) tails[i].HasBodyParent(true);

            //handle groupBind
            groupBind = new List<List<IKManager_Step>>();

            List<int> distinctGroups = new List<int>();
            distinctGroups = stepBind.GroupBy(p => p).Select(g => g.First()).ToList();
            distinctGroups.GroupBy(p => p).Select(g => g.First()).ToList();
            distinctGroups.Sort();

            for (int i = 0; i < distinctGroups.Count; i++)
            {
                List<IKManager_Step> tmpListStepLock = new List<IKManager_Step>();

                List<int> posItems = new List<int>();
                posItems = stepBind.Select((s, p) => new { p, s }).Where(x => x.s == distinctGroups[i]).Select(x => x.p).ToList();

                for (int t = 0; t < posItems.Count; t++) tmpListStepLock.Add(legs[posItems[t]]);
                groupBind.Add(tmpListStepLock);
            }

            EnableSteppers();
            StoreTarget();
        }


        private void Update()
        {
            UpdateBody();
        }


        public void ManualUpdate()
        {
            UpdateBody();
        }


        private void UpdateBody()
        {
            CheckGround();
            TiltBody();

            if (prevBodyRot != bodyPivot.rotation || prevTargetPos != target.position)
            {
                for (int i = 0; i < legs.Length; i++) legs[i].ManualUpdate();
                for (int i = 0; i < tails.Length; i++) tails[i].ManualUpdate();
            }

            StoreTarget();
        }

        #endregion


        #region Solve

        private void CheckGround()
        {
            RaycastHit hit;
            if (Physics.Raycast(bodyPivot.position, Vector3.down, out hit, Mathf.Infinity, groundLayer))
            {
                raycastHitPoint = hit.point;
                raycastHitNormal = hit.normal;
            }
        }


        private void TiltBody()
        {
            if (enableBodyTilt)
            {            
                Quaternion currentRot = bodyPivot.transform.rotation;
                Vector3 momentumTilt = target.InverseTransformVector(prevTargetPos - target.position) * bodyTiltMultiplier;
            
                Vector3 floorTilt_forward = Vector3.ProjectOnPlane(target.forward, Vector3.Lerp(Vector3.up, raycastHitNormal, slopeTiltContribution));
                Vector3 floorTilt_right = Vector3.ProjectOnPlane(target.right, Vector3.Lerp(Vector3.up, raycastHitNormal, slopeTiltContribution));
                float floorTilt_forwardAngle = Vector3.SignedAngle(floorTilt_forward, target.forward, Vector3.up);
                float floorTilt_rightAngle = Vector3.SignedAngle(floorTilt_right, target.right, Vector3.up);

                //limit range momentum
                momentumTilt = Utils.IKUtils.LockInRangeVector(momentumTilt, tiltRange_X, tiltRange_Y);

                //calculate quaternions
                Quaternion floorRot = Quaternion.AngleAxis(floorTilt_forwardAngle, target.right) * Quaternion.AngleAxis(floorTilt_rightAngle, target.forward) * target.rotation;
                Quaternion finalRot = Quaternion.Euler(bodyPivot.right * defaultTilt) * Quaternion.Euler(bodyPivot.forward * momentumTilt.x) * Quaternion.Euler(bodyPivot.right * -momentumTilt.z) * floorRot;

                //apply rotation
                Quaternion smoothRot = Utils.IKUtils.QuaternionSmoothDamp(currentRot, finalRot, ref refTiltRot, Time.deltaTime * bodyTiltDamp);
                bodyPivot.transform.rotation = smoothRot;
            }
        }

        #endregion


        #region Steppers

        public void UpdateSteppers(IKManager_Step _stepLock)
        {
            if(groupBind[currentAllowedSteppers].Contains(_stepLock))
            {
                currentAllowedSteppers++;
                if (currentAllowedSteppers > groupBind.Count - 1) currentAllowedSteppers = 0;

                EnableSteppers();
            }
        }


        private void EnableSteppers()
        {
            for (int i = 0; i < groupBind.Count; i++)
            {
                bool allowed = false;
                if (i == currentAllowedSteppers) allowed = true;

                for (int t = 0; t < groupBind[i].Count; t++) groupBind[i][t].CanStep(allowed);
            }
        }


        private void StoreTarget()
        {
            prevTargetPos = target.position;
            prevBodyRot = bodyPivot.rotation;
        }

        #endregion


        private void OnDrawGizmos()
        {
            if (Application.isPlaying && showDebug)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(raycastHitPoint, 0.1f);
                Gizmos.DrawLine(bodyPivot.position, raycastHitPoint);

                Vector3 floorTilt_forward = Vector3.ProjectOnPlane(target.forward, Vector3.Lerp(Vector3.up, raycastHitNormal, slopeTiltContribution));
                Vector3 floorTilt_right = Vector3.ProjectOnPlane(target.right, Vector3.Lerp(Vector3.up, raycastHitNormal, slopeTiltContribution));
                Gizmos.DrawLine(target.position, target.position + floorTilt_forward);
                Gizmos.DrawLine(target.position, target.position + floorTilt_right);
            }
        }
    }
}