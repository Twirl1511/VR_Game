using System;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    [SerializeField] private CustomGravityCharacterController _customGravityCharacterController;
    [SerializeField] private CustomGravityActionBasedModeProvider gravityProvider;
    [SerializeField] private float _distanceToSurface = 0.5f;
    [SerializeField] private float _radiusSurfaceDetector = 1f;
    [SerializeField] private Transform _surfaceDetector;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _center;                  
    [SerializeField] private float _gravityForce = 9.81f;
    [SerializeField] private float _gravityChangeDelay = 0.8f;

    [Space]
    [SerializeField] private float _checkGroundDistance = 1f;
    [SerializeField] private float _checkFallDistance = 2f;
    [SerializeField] private Rigidbody _rigidbody;



    public Vector3 NormalDirection { get; private set; }
    public Vector3 GravityDirection { get; private set; }
    public Vector3 GravityVelocity { get; private set; }
    public bool IsJumping { get; private set; }

    private float _timer;
    public bool CanChangeGravity { get; private set; }

    public event Action OnChangeFloor;




    private void Start()
    {
        CanChangeGravity = true;
        ResetGravity();
    }

    private void FixedUpdate()
    {
        UpdateGravityVelocity();

        if (CanChangeGravity && _customGravityCharacterController.IsMoving)
            CheckGravityDirection();
        else
            CanChangeGravity = IsReadyToChangeGravity();
    }

    private void ResetGravity()
    {
        GravityDirection = Vector3.down * _gravityForce;
        NormalDirection = Vector3.up;
    }

    private void CheckGravityDirection()
    {
        Vector3 direction = (_surfaceDetector.position - _center.position).normalized;
        Ray ray = new Ray(_center.position, direction);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, _distanceToSurface + _radiusSurfaceDetector, _groundLayer))
        {
            GravityDirection = Vector3.Normalize(-hitInfo.normal) * _gravityForce;
            NormalDirection = Vector3.Normalize(hitInfo.normal);

            OnChangeFloor?.Invoke();
            CanChangeGravity = false;
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

    public void MoveSurfaceDetector(Vector3 direction)
    {
        Vector3 targetPosition = _center.position + direction * _radiusSurfaceDetector;
        _surfaceDetector.position = targetPosition;
    }

    public void UpdateGravityVelocity()
    {
        Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, float.MaxValue);
        // TODO: rewrite to squaremagnitude
        float distanceToSurface = Vector3.Distance(transform.position, hit.point);

        if (distanceToSurface <= _checkGroundDistance)
        {
            GravityVelocity = Vector3.zero;
        }
        else if (distanceToSurface > _checkGroundDistance && distanceToSurface < _checkFallDistance)
        {
            GravityVelocity += GravityDirection * Time.deltaTime;
        }
        else if (distanceToSurface > _checkFallDistance )
        {
            if (CanChangeGravity)
            {
                ResetGravity();
                GravityVelocity += GravityDirection * Time.deltaTime;
            }
            else
            {
                GravityVelocity += GravityDirection * Time.deltaTime;
            }
        }
    }
}
