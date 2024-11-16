using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomGravityCharacterController : MonoBehaviour
{
    [SerializeField] private GravityController _gravityController;
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _speedDebuff = 0.5f;
    [SerializeField] private float _changeAlignmentSpeed = 0.1f;
    [SerializeField] private Transform _camera;
    [SerializeField] private float _checkGroundDistance = 1f;
    [SerializeField] private AnimationCurve _animationCurve;

    public bool IsMoving { get; private set; }
    private Vector3 NormalDirection => _gravityController.NormalDirection;

    private Rigidbody _rigidBody;




    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.useGravity = false;
        _rigidBody.isKinematic = false;
    }
    private void FixedUpdate()
    {
        SurfaceAlignment();
    }

    private float GetSpeed()
    {
        return _gravityController.CanChangeGravity ? _moveSpeed : _moveSpeed * _speedDebuff;
    }

    private void SurfaceAlignment()
    {
        Quaternion rotationRef = transform.rotation;
        Quaternion alignToNormal = Quaternion.FromToRotation(transform.up, NormalDirection);
        Quaternion targetRotation = alignToNormal * rotationRef;

        rotationRef = Quaternion.Lerp(rotationRef, targetRotation, _animationCurve.Evaluate(Time.time) * _changeAlignmentSpeed);
        transform.rotation = rotationRef;
    }




    public void Move(Vector2 input)
    {
        Vector3 cameraForward = Vector3.ProjectOnPlane(_camera.forward, NormalDirection).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(_camera.right, NormalDirection).normalized;

        Vector3 moveDirection = (cameraRight * input.x + cameraForward * input.y);
        Vector3 relativeMoveDirection = Vector3.ProjectOnPlane(moveDirection, NormalDirection);
        Vector3 moveVelocity = relativeMoveDirection * GetSpeed();

        _rigidBody.MovePosition(_rigidBody.position + (moveVelocity + _gravityController.GravityVelocity) * Time.deltaTime);

        if (input != Vector2.zero)
        {
            _gravityController.MoveSurfaceDetector(relativeMoveDirection);
            IsMoving = input != Vector2.zero;
        }
    }
}
