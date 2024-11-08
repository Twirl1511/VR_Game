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
    [SerializeField] private float _gravityChangeDelay = 2f;

    public Vector3 NormalDirection { get; private set; }
    public Vector3 GravityDirection { get; private set; }

    private float _timer;
    private bool _isReadyToChangeGravity = true;

    public event Action OnChangeFloor;




    private void Start()
    {
        GravityDirection = Vector3.down * _gravityForce;
        NormalDirection = Vector3.up;
    }

    private void FixedUpdate()
    {
        if (_isReadyToChangeGravity)
            CheckGravityDirection();
        else
            _isReadyToChangeGravity = IsReadyToChangeGravity();
    }

    private void CheckGravityDirection()
    {
        Vector3 direction = (_floorDetector.position - _center.position).normalized;
        Ray ray = new Ray(_center.position, direction);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, _distanceToFloor + _radiusFloorDetector, _groundLayer))
        {
            GravityDirection = Vector3.Normalize(-hitInfo.normal) * _gravityForce;
            NormalDirection = Vector3.Normalize(hitInfo.normal);

            OnChangeFloor?.Invoke();
            _isReadyToChangeGravity = false;
        }
    }

    private bool IsReadyToChangeGravity()
    {
        _timer += Time.deltaTime;

        if (_timer < _gravityChangeDelay)
            return false;

        _timer = 0;
        return true;
    }




    public void MoveFloorDetector(Vector3 direction)
    {
        Vector3 targetPosition = _center.position + direction * _radiusFloorDetector;
        _floorDetector.position = targetPosition;
    }
}
