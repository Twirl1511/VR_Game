using UnityEngine;


namespace CreatureIK
{
    public class SimpleMoverTarget : MonoBehaviour
    {
        [Header("Simple mover specifics")]
        [Tooltip("Layer where the step can walk on")]
        public LayerMask groundLayer;
        [Tooltip("Layer that stops the creature")]
        public LayerMask wallLayer;
        [Tooltip("Range distance allowed from the ground")]
        public Vector2 groundDistanceRange;

        [Header("Automover specifics")]
        [Tooltip("Movement speed")]
        public float speedMov;
        [Tooltip("Damp when changing heights")]
        [Min(1)]
        public float heightDamp = 1.4f;
        [Tooltip("Movement directions")]
        public Vector3 directionMov;
        [Tooltip("Forward check distance")]
        public float forwardCheckDistance = 4f;
        [Tooltip("Can automove")]
        public bool canAutoMove = false;
        [Space(15)]

        public bool showDebug = true;

        private Transform bodyPivot;

        private Vector3 raycastHitPoint;
        private Vector3 rangeMin;
        private Vector3 rangeMax;
        private float heightRef;

        private bool canWalkWall;
        private bool canWalkFall;


        /**/


        #region Setup

        private void Awake()
        {
            //set vars
            bodyPivot = this.transform;

            //SetPosition(bodyPivot.position);
        }


        private void Update()
        {
            if (canAutoMove && canWalkFall && canWalkWall) AutoMove();
            CheckFloorDistance();
        }

        #endregion


        #region Move in Range

        public void SetPosition(Vector3 _pos)
        {

            rangeMax = raycastHitPoint + new Vector3(_pos.x, groundDistanceRange.y, _pos.z);
            rangeMin = raycastHitPoint + new Vector3(_pos.x, groundDistanceRange.x, _pos.z);

            if (_pos.y > rangeMax.y) _pos.y = rangeMax.y;
            else if (_pos.y < rangeMin.y) _pos.y = rangeMin.y;
            else _pos.y = Mathf.SmoothDamp(_pos.y, (rangeMax.y + rangeMin.y) / 2f, ref heightRef, Time.deltaTime * heightDamp);

            this.transform.position = _pos;
        }


        private void CheckFloorDistance()
        {
            RaycastHit hit;
            if (Physics.Raycast(bodyPivot.position, Vector3.down, out hit, Mathf.Infinity, groundLayer)) raycastHitPoint = hit.point;

            //check if can walk
            if (Physics.Raycast(this.transform.position, Vector3.down, out hit, Mathf.Infinity, groundLayer)) canWalkFall = true;
            else canWalkFall = false;

            if (Physics.Raycast(this.transform.position + this.transform.forward * directionMov.z * forwardCheckDistance, this.transform.forward, out hit, forwardCheckDistance, wallLayer)) canWalkWall = false;
            else canWalkWall = true;
        }


        private void AutoMove()
        {
            Vector3 attemptPos = this.transform.position + (this.transform.right * directionMov.x + this.transform.forward * directionMov.z) * Time.deltaTime * speedMov;
            SetPosition(attemptPos);
        }

        #endregion


        private void OnDrawGizmos()
        {
            if (Application.isPlaying && showDebug)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(raycastHitPoint + new Vector3(0f, groundDistanceRange.x, 0f), 0.2f);
                Gizmos.DrawSphere(raycastHitPoint + new Vector3(0f, groundDistanceRange.y, 0f), 0.2f);
            }
        }
    }
}