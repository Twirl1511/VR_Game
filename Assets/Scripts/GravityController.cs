using System;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    private const float GRAVITY_FORCE = 9.81f;
    private float ANGLE_OFFSET = 90f;


    [SerializeField] private CustomGravityCharacterController _customGravityCharacterController;
    [SerializeField] private CustomGravityActionBasedModeProvider gravityProvider;
    [SerializeField] private PhysicsHandsController _physicsHandsController;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _distanceToCheckNewSurface = 1.5f;
    [SerializeField] private float _radiusSurfaceDetector = 1f;
    [SerializeField] private Transform _surfaceDetector;
    [SerializeField] private Transform _center;                  
    [SerializeField] private float _gravityChangeDelay = 0.2f;

    [Header("Ground and Fall distance")]
    [Space]
    [SerializeField] private float _checkGroundDistance = 1f;
    [SerializeField] private float _checkFallDistance = 1.8f;

    [Header("Jump")]
    [Space]
    [SerializeField] private float _jumpDurationTransition = 2.0f;
    [SerializeField] private AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private Jump _jump;

    [Header("Change surface 270")]
    [Space]
    [SerializeField] private int _maxRaycasts = 3;
    [SerializeField] private float _edgeDistance = 0.2f;
    [SerializeField] private float _teleportaionDistanceCloseToEdge = 0.5f;
    [SerializeField] private float _defaultDrag = 100f;
    [SerializeField] private float _duringJumpDrag = 3f;


    public Vector3 NormalDirection { get; private set; }
    public Vector3 GravityVelocity { get; private set; }
    public bool CanChangeGravity { get; private set; }
    public bool IsGround { get; private set; }


    private bool _isJumping;
    private bool _isTransitioning;
    private float _changeGravityTimer;
    private float _transitioningGravityDirectionElapsedTime;
    private Vector3 _gravityDirection;
    private Vector3 _defaultGravityDirection;
    private Collider[] _collidersForJumpCheck = new Collider[1];
    private Rigidbody _rigidbody;


    public event Action OnChangeSurface;




    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _defaultGravityDirection = Vector3.down * GRAVITY_FORCE;
        ResetGravityToDefault();

        if(_physicsHandsController != null)
            _physicsHandsController.OnJump += StartJumping;

        _jump.OnJump += StartJumping;
    }

    private void OnDestroy()
    {
        if (_physicsHandsController != null)
            _physicsHandsController.OnJump -= StartJumping;

        _jump.OnJump -= StartJumping;
    }

    private void FixedUpdate()
    {
        TransitioningGravityDirectionWhileJumping();
        UpdateGravityVelocity();

        if (!CanChangeGravity)
        {
            CanChangeGravity = IsReadyToChangeGravity();
        }

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

    private void TransitioningGravityDirectionWhileJumping()
    {
        if(_isJumping == false)
        {
            _isTransitioning = false;
            return;
        }

        if (_isTransitioning)
        {
            _transitioningGravityDirectionElapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(_transitioningGravityDirectionElapsedTime / _jumpDurationTransition);
            float curveValue = _curve.Evaluate(progress);
            _gravityDirection = Vector3.Lerp(_gravityDirection, _defaultGravityDirection, curveValue);

            if (progress >= 1.0f) /// 100%
            {
                _gravityDirection = _defaultGravityDirection;
                NormalDirection = Vector3.up;
                _changeGravityTimer = 0;
                _isTransitioning = false; 
            }
        }
    }

    private void StartTransition()
    {
        _transitioningGravityDirectionElapsedTime = 0f;
        _isTransitioning = true;
    }

    private void StartJumping()
    {
        _rigidbody.drag = _duringJumpDrag;
        SetIsJumping(true);
        CanChangeGravity = false;
        _changeGravityTimer = 0;
        StartTransition();
    }

    private void ResetGravityToDefault()
    {
        _gravityDirection = _defaultGravityDirection;
        NormalDirection = Vector3.up;
        _changeGravityTimer = 0;
    }

    private bool IsOnEdge(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction);
        return !Physics.Raycast(ray, out _, _distanceToCheckNewSurface, _groundLayer);
    }

    private void CheckGravityDirection()
    {
        Vector3 currentOrigin = _center.position;
        Vector3 currentDirection = (_surfaceDetector.position - _center.position).normalized;
        Quaternion rotation = Quaternion.AngleAxis(ANGLE_OFFSET, Vector3.Cross(currentDirection, -transform.up));

        Vector3 edgeOriginPoint = currentOrigin + (currentDirection * _edgeDistance);

        for (int i = 0; i < _maxRaycasts; i++)
        {
            Ray ray = new Ray(currentOrigin, currentDirection);
            Debug.DrawRay(currentOrigin, currentDirection * GetModifiedDistance(i), Color.red);

            currentOrigin += currentDirection * _distanceToCheckNewSurface;
            currentDirection = rotation * currentDirection;

            if (i == 1)
            {
                continue;
            }

            if (!Physics.Raycast(ray, out RaycastHit hitInfo, GetModifiedDistance(i), _groundLayer))
            {
                continue;
            }

            if (i == 0) // first forward raycast hitting a wall on 90 degrees
            {
                SetNewSurface(hitInfo);
                return;
            }
            else if (i == 2) // third raycast hitting an edge at 270 degrees
            {
                if (IsOnEdge(edgeOriginPoint, -transform.up))
                {
                    Vector3 teleportPosition = currentOrigin + (currentDirection * _teleportaionDistanceCloseToEdge);
                    transform.position = teleportPosition;
                    SetNewSurface(hitInfo);
                }

                return;
            }
        }




        return;
        float GetModifiedDistance(int index)
        {
            return index == 2 ? _distanceToCheckNewSurface * 1.5f : _distanceToCheckNewSurface;
        }
    }

    private void CheckGravityDirectionWithSphereCast()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _distanceToCheckNewSurface, _collidersForJumpCheck, _groundLayer);

        if (hitCount == 0)
            return;

        SetIsJumping(false); /// вообще это лишнее, так как игрок должен приклеиться к поверхности
        Vector3 direction = (_collidersForJumpCheck[0].ClosestPoint(transform.position) - transform.position).normalized;
        if (Physics.Raycast(transform.position, direction, out RaycastHit hitInfo, _distanceToCheckNewSurface, _groundLayer))
        {
            SetNewSurface(hitInfo);
        }
    }

    private void SetNewSurface(RaycastHit hitInfo)
    {
        _gravityDirection = -hitInfo.normal * GRAVITY_FORCE;
        NormalDirection = hitInfo.normal;

        CanChangeGravity = false;
        SetIsJumping(false);
        _rigidbody.drag = _defaultDrag;

        OnChangeSurface?.Invoke();
    }

    private void SetIsJumping(bool value)
    {
        _isJumping = value;
    }


    

    public void MoveSurfaceDetector(Vector3 direction)
    {
        Vector3 targetPosition = _center.position + direction * _radiusSurfaceDetector;
        _surfaceDetector.position = targetPosition;
    }

    public void UpdateGravityVelocity()
    {
        Physics.Raycast(transform.position, -transform.up, out RaycastHit hitInfo, float.MaxValue);
        // TODO: rewrite to squaremagnitude
        float distanceToSurface = Vector3.Distance(transform.position, hitInfo.point);

        if (IsOnSurface())
        {
            IsGround = true;
            GravityVelocity = Vector3.zero;
        }
        else if (IsNearSurface())
        {
            IsGround = false;
            GravityVelocity += _gravityDirection * Time.deltaTime;
        }
        else if(IsFalling())
        {
            IsGround = false;

            // falling down
            if (CanChangeGravity && !_isJumping)
            {
                ResetGravityToDefault();
            }

            GravityVelocity += _gravityDirection * Time.deltaTime;
        }




        return;
        bool IsOnSurface()
        {
            return distanceToSurface <= _checkGroundDistance;
        }
        bool IsNearSurface()
        {
            return distanceToSurface > _checkGroundDistance && distanceToSurface < _checkFallDistance;
        }
        bool IsFalling()
        {
            return distanceToSurface > _checkFallDistance;
        }
    }
}
