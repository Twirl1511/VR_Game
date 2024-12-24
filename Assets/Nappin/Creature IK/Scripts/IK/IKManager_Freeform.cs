using System.Collections.Generic;
using UnityEngine;


namespace CreatureIK
{
    public class IKManager_Freeform : MonoBehaviour
    {
        [Header("Joints specifics")]
        [Tooltip("Joints from root to end")]
        public Transform[] joints;
        [Tooltip("Lift applied to all the joints")]
        public AnimationCurve jointsLift;
        [Tooltip("Solver target")]
        public Transform target;

        [Header("Anchor specifics")]
        [Tooltip("Threshold of movement for an unanchored chain")]
        [Min(0)]
        public float unanchoredTreshold = 0.02f;
        [Tooltip("Is the first point anchored")]
        public float unanchoredRotationDamp;
        [Tooltip("Is the rope anchored to its pivot")]
        public bool isAnchored = true;
        [Space(15)]

        public bool showDebug = true;


        private Vector3[] jointsPos;
        private float[] jointsDist;
        private bool hasBodyParent = false;
        private float[] jointLiftArr;

        private Quaternion rotationTiltRef;

        private bool hasParent = false;
        private Quaternion prevTargetRot;
        private Vector3 prevTargetPos;


        /**/


        #region Setup

        private void Awake()
        {
            //set parent distance
            if (this.transform.parent != null) hasParent = true;

            //create array pos
            jointsPos = new Vector3[joints.Length];
            for (int i = 0; i < jointsPos.Length; i++) jointsPos[i] = joints[i].position;

            //create array distances
            jointsDist = new float[joints.Length - 1];
            for (int i = 0; i < joints.Length - 1; i++) jointsDist[i] = (jointsPos[i + 1] - jointsPos[i]).magnitude;

            //fill joints lifts
            jointLiftArr = new float[joints.Length];
            for (int i = 0; i < joints.Length; i++) jointLiftArr[i] = jointsLift.Evaluate(1f / joints.Length * (i - 1)) / 100f;

            StoreTarget();
        }


        private void Update()
        {
            if (!hasBodyParent || (prevTargetRot != target.rotation || prevTargetPos != target.position)) UpdateFreeform();
        }


        public void ManualUpdate()
        {
            UpdateFreeform();
        }


        private void UpdateFreeform()
        {
            if (isAnchored) UpdateAchoredJoints();
            else UpdateUnachoredJoints();

            StoreTarget();
        }

        #endregion


        #region Solver

        private void UpdateUnachoredJoints()
        {
            for (int i = 0; i < joints.Length; i++)
            {
                Quaternion currentRot = joints[i].rotation;
                Quaternion targetRot = currentRot;

                if (i != 0)
                {
                    //rotation body
                    Vector3 dist = joints[i - 1].transform.position - joints[i].transform.position;
                    if (dist != Vector3.zero) targetRot = Quaternion.LookRotation(dist);

                    //position body
                    joints[i].transform.position = (joints[i].transform.position - joints[i - 1].transform.position).normalized * jointsDist[i - 1] + joints[i - 1].transform.position;
                    jointsPos[i] = joints[i].position;
                }
                else
                {
                    //rotation head
                    Vector3 dist = target.position - joints[0].position;
                    if (dist != Vector3.zero) targetRot = Quaternion.LookRotation(dist);
                }

                Quaternion smoothRot = Utils.IKUtils.QuaternionSmoothDamp(currentRot, targetRot, ref rotationTiltRef, Time.deltaTime * unanchoredRotationDamp);
                joints[i].transform.rotation = smoothRot;
            }

            //position head
            joints[0].transform.position = target.position;
            jointsPos[0] = joints[0].position;
        }


        private void UpdateAchoredJoints()
        {
            jointsPos = Utils.IKUtils.SolvePointsFB(jointsPos, jointsDist, target.position, jointLiftArr, 101);

            if (hasParent) jointsPos[0] = joints[0].position;
            else jointsPos[0] = this.transform.position;

            for (int i = 0; i < jointsPos.Length; i++)
            {
                joints[i].position = jointsPos[i];

                if (i != jointsPos.Length - 1) joints[i].rotation = Quaternion.LookRotation((jointsPos[i + 1] - jointsPos[i]).normalized) * Quaternion.Euler(0f, -90f, 0f);
                else
                {
                    Vector3 dist = (target.position - jointsPos[i]).normalized;
                    if (dist != Vector3.zero) joints[i].rotation = Quaternion.LookRotation(dist) * Quaternion.Euler(0f, -90f, 0f);
                }
            }
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


        private void OnDrawGizmos()
        {
            if (Application.isPlaying && showDebug)
            {
                Gizmos.color = Color.blue;
                for (int i = jointsPos.Length - 2; i >= 0; i--) Gizmos.DrawLine(jointsPos[i], jointsPos[i + 1]);

                Gizmos.color = Color.red;
                for (int i = 0; i < jointsPos.Length - 1; i++) Gizmos.DrawWireSphere(jointsPos[i], 0.1f);
            }
        }
    }
}