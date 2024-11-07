using System;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    [SerializeField] private CustomGravityCharacterController _customGravityCharacterController;
    [SerializeField] private CustomGravityActionBasedModeProvider gravityProvider;
    [SerializeField] private float _distanceToFloor = 2f;
    [SerializeField] private float _radiusFloorDetector = 5f;
    [SerializeField] private Transform _floorDetector;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _center;                  
    [SerializeField] private float _gravityForce = 9.81f;

    public Vector3 NormalDirection { get; private set; }
    public Vector3 GravityDirection { get; private set; }

    private RaycastHit _previousRaycastHit;

    public event Action OnChangeFloor;




    private void FixedUpdate()
    {
        CheckGravityDirection();
    }

    private void CheckGravityDirection()
    {
        Ray ray = new Ray(_floorDetector.position, -_floorDetector.up);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, _distanceToFloor, _groundLayer))
        {
            if (_previousRaycastHit.normal != hitInfo.normal)
                OnChangeFloor?.Invoke();

            _previousRaycastHit = hitInfo;
            GravityDirection = Vector3.Normalize(-hitInfo.normal) * _gravityForce;
            NormalDirection = Vector3.Normalize(hitInfo.normal);
        }
    }




    public void MoveFloorDetector(Vector3 direction)
    {
        Vector3 targetPosition = _center.position + direction * _radiusFloorDetector;
        _floorDetector.position = targetPosition;
    }
}
