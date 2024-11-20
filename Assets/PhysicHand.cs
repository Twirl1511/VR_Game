using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhysicsHand : MonoBehaviour
{
    [Header("PID")]
    [SerializeField] private float _frequency = 50f;
    [SerializeField] private float _damping = 1f;
    [SerializeField] private float _rotFrequency = 100f;
    [SerializeField] private float _rotDamping = 0.9f;
    [SerializeField] private Rigidbody _playerRigidbody;
    [SerializeField] private Transform _target;
    [SerializeField] private float _squareMagnitudeMaxDistance = 10f;

    [SerializeField] private InputActionReference _buttonAction;

    [Space]
    [Header("Springs")]
    [SerializeField] private float _climbForce = 1000f;
    //[SerializeField] private float _climbDrag = 1f;
    //[SerializeField] private float _maxJumpForce = 5f;
    [SerializeField] private float _squareMagnitudeAllowJump = 50f;

    private Rigidbody _selfRigidbody;
    private Collider _selfCollider;
    private Vector3 _previousPosition;
    private bool _isColliding;
    private bool _isJumpButtonHeld;
    private bool _isInPropperPosition;
    private bool _isJumping;

    public event Action OnJump;

    void Start()
    {
        transform.SetPositionAndRotation(_target.position, _target.rotation);
        _selfRigidbody = GetComponent<Rigidbody>();
        _selfCollider = GetComponent<Collider>();
        _selfRigidbody.maxAngularVelocity = float.PositiveInfinity;
        _previousPosition = transform.position;

        _buttonAction.action.performed += EnableJump;
        _buttonAction.action.canceled += DisableJump;
    }

    private void OnDisable()
    {
        _buttonAction.action.performed -= EnableJump;
        _buttonAction.action.canceled -= DisableJump;
    }

    private void FixedUpdate()
    {
        //ReturnCollider();
        PIDMovement();
        PIDRotation();

        if (_isColliding && _isJumpButtonHeld && IsVelocityEnoughForJump()) 
            HookesLaw();
    }

    private void ReturnCollider()
    {
        float distance = (transform.position - _target.position).sqrMagnitude;

        if(distance > _squareMagnitudeMaxDistance)
        {
            _selfCollider.isTrigger = true;
            _isInPropperPosition = false;
        }
        else
        {
            _selfCollider.isTrigger = false;
            _isInPropperPosition = true;
        }
    }

    private void HookesLaw()
    {
        Vector3 displacementFromResting = transform.position - _target.position;
        Vector3 force = displacementFromResting * _climbForce;
        //float drag = GetDrag();

        //Vector3 adjustedForce = Quaternion.FromToRotation(force.normalized, Camera.main.transform.forward) * force;
        //_playerRigidbody.AddForce(adjustedForce, ForceMode.Acceleration);
        _playerRigidbody.AddForce(force, ForceMode.Acceleration);
        //_playerRigidbody.AddForce(drag * -_playerRigidbody.velocity * _climbDrag, ForceMode.Acceleration);

        OnJump?.Invoke();
    }

    private void DisableJump(InputAction.CallbackContext obj)
    {
        _isJumpButtonHeld = false;
    }

    private void EnableJump(InputAction.CallbackContext obj)
    {
        _isJumpButtonHeld = true;
    }

    private void PIDMovement()
    {
        float kp = (6f * _frequency) * (6f * _frequency) * 0.25f;
        float kd = 4.5f * _frequency * _damping;
        float g = 1 / (1 + kd * Time.fixedDeltaTime + kp * Time.fixedDeltaTime * Time.fixedDeltaTime);
        float ksg = kp * g;
        float kdg = (kd + kp * Time.fixedDeltaTime) * g;
        Vector3 force = (_target.position - transform.position) * ksg + (_playerRigidbody.linearVelocity - _selfRigidbody.linearVelocity) * kdg;
        _selfRigidbody.AddForce(force, ForceMode.Acceleration);
    }

    private void PIDRotation()
    {
        float kp = (6f * _rotFrequency) * (6f * _rotFrequency) * 0.25f;
        float kd = 4.5f * _rotFrequency * _rotDamping;
        float g = 1 / (1 + kd * Time.fixedDeltaTime + kp * Time.fixedDeltaTime * Time.fixedDeltaTime);
        float ksg = kp * g;
        float kdg = (kd + kp * Time.fixedDeltaTime) * g;
        Quaternion q = _target.rotation * Quaternion.Inverse(transform.rotation);
        if (q.w < 0)
        {
            q.x = -q.x;
            q.y = -q.y;
            q.z = -q.z;
            q.w = -q.w;
        }
        q.ToAngleAxis(out float angle, out Vector3 axis);
        axis.Normalize();
        axis *= Mathf.Deg2Rad;
        Vector3 torque = ksg * axis * angle + -_selfRigidbody.angularVelocity * kdg;
        _selfRigidbody.AddTorque(torque, ForceMode.Acceleration);
    }

    private float GetDrag()
    {
        Vector3 handVelocity = (_target.localPosition - _previousPosition) / Time.fixedDeltaTime;
        float drag = 1 / handVelocity.magnitude + 0.01f;
        drag = drag > 1 ? 1 : drag;
        drag = drag < 0.03f ? 0.03f : drag;
        _previousPosition = transform.position;
        return drag;
    }

    private bool IsVelocityEnoughForJump()
    {
        return _selfRigidbody.linearVelocity.sqrMagnitude > _squareMagnitudeAllowJump;
    }

    private void OnCollisionEnter(Collision collision)
    {
        _isColliding = true;
    }

    private void OnCollisionExit(Collision other)
    {
        _isColliding = false;
    }
}
