using System;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    [SerializeField] private CustomGravityCharacterController _customGravityCharacterController;
    [SerializeField] private CustomGravityActionBasedModeProvider gravityProvider;
    [SerializeField] private PhysicsHandsController _physicsHandsController;
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

    [Header("Jump")]
    [Space]
    [SerializeField] private float _jumpDurationTransition = 1.0f;
    [SerializeField] private AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float _radiusOverlapSphere = 1.5f;
    [SerializeField] private Jump _jump;


    public Vector3 NormalDirection { get; private set; }
    public Vector3 GravityDirection { get; private set; }
    public Vector3 DefaultGravityDirection { get; private set; }
    public Vector3 GravityVelocity { get; private set; }
    public bool IsJumping { get; private set; }
    public bool CanChangeGravity { get; private set; }
    public bool IsGround { get; private set; }


    private bool _isJumping;
    private float _changeGravityTimer;
    private float _elapsedTime;
    private bool _isTransitioning;
    private Collider[] _collidersForJumpCheck = new Collider[1];


    public event Action OnChangeFloor;




    private void Start()
    {
        DefaultGravityDirection = Vector3.down * _gravityForce;

        _physicsHandsController.OnJump += StartJumping;
        _jump.OnJump += StartJumping;

        CanChangeGravity = true;
        ResetGravity();
    }

    private void OnDestroy()
    {
        _physicsHandsController.OnJump -= StartJumping;
        _jump.OnJump -= StartJumping;
    }

    private void FixedUpdate()
    {
        TransitioningGravityDirection();
        UpdateGravityVelocity();

        if(CanChangeGravity == false)
            CanChangeGravity = IsReadyToChangeGravity();


        if (_isJumping)
        {
            if (CanChangeGravity)
            {
                CheckGravityDirectionWithSphereCast();
            }

            return;
        }


        if (CanChangeGravity && _customGravityCharacterController.IsMoving)
            CheckGravityDirection();
    }

    private bool IsReadyToChangeGravity()
    {
        _changeGravityTimer += Time.deltaTime;

        if (_changeGravityTimer < _gravityChangeDelay)
            return false;

        _changeGravityTimer = 0;
        return true;
    }

    private void TransitioningGravityDirection()
    {
        if(_isJumping == false)
        {
            _isTransitioning = false;
            return;
        }

        if (_isTransitioning)
        {
            _elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsedTime / _jumpDurationTransition);
            float curveValue = _curve.Evaluate(progress);
            GravityDirection = Vector3.Lerp(GravityDirection, DefaultGravityDirection, curveValue);

            if (progress >= 1.0f)
            {
                ResetGravity();
                _isTransitioning = false; 
            }
        }
    }

    private void StartTransition()
    {
        _elapsedTime = 0f;
        _isTransitioning = true;
    }

    private void StartJumping()
    {
        _isJumping = true;
        StartTransition();
    }

    private void ResetGravity()
    {
        GravityDirection = DefaultGravityDirection;
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
            _isJumping = false;
        }
    }

    private void CheckGravityDirectionWithSphereCast()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _radiusOverlapSphere, _collidersForJumpCheck, _groundLayer);

        if (hitCount == 0)
            return;

        Vector3 direction = (_collidersForJumpCheck[0].ClosestPoint(transform.position) - transform.position).normalized;
        if (Physics.Raycast(transform.position, direction, out RaycastHit hitInfo, float.MaxValue, _groundLayer))
        {
            GravityDirection = Vector3.Normalize(-hitInfo.normal) * _gravityForce;
            NormalDirection = Vector3.Normalize(hitInfo.normal);

            OnChangeFloor?.Invoke();
            CanChangeGravity = false;
            _isJumping = false;
        }
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
            IsGround = true;
        }
        else if (distanceToSurface > _checkGroundDistance && distanceToSurface < _checkFallDistance)
        {
            GravityVelocity += GravityDirection * Time.deltaTime;
            IsGround = false;
        }
        else if (distanceToSurface > _checkFallDistance)
        {
            IsGround = false;
            if (_isJumping)
            {
                GravityVelocity += GravityDirection * Time.deltaTime;
            }
            else
            {
                // TODO: rewrite!!!!!!!!!!!!!
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
}
