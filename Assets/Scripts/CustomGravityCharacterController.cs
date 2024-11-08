using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomGravityCharacterController : MonoBehaviour
{
    [SerializeField] private GravityController _gravityController;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _changeAlignmentSpeed = 0.1f;
    [SerializeField] private Transform _camera;
    [SerializeField] private float _checkGroundDistance = 1f;
    [SerializeField] private AnimationCurve _animationCurve;

    private Vector3 NormalDirection => _gravityController.NormalDirection;
    private Vector3 GravityDirection => _gravityController.GravityDirection;

    private Rigidbody _rigidBody;
    private Vector3 _gravityVelocity;
    private bool _isGrounded;




    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.useGravity = false;
        _rigidBody.isKinematic = false;
    }
    private void FixedUpdate()
    {
        CheckGround();
        SurfaceAlignment();
    }

    private void CheckGround()
    {
        _isGrounded = Physics.Raycast(
            transform.position, 
            -transform.up, 
            out _, 
            _checkGroundDistance);
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

        Vector3 moveDirection = (cameraRight * input.x + cameraForward * input.y).normalized;
        Vector3 relativeMoveDirection = Vector3.ProjectOnPlane(moveDirection, NormalDirection).normalized;
        Vector3 moveVelocity = relativeMoveDirection * _moveSpeed;

        _gravityVelocity = GetGravityVelocity(_isGrounded);
        _rigidBody.MovePosition(_rigidBody.position + (moveVelocity + _gravityVelocity) * Time.deltaTime);

        if(input != Vector2.zero)
            _gravityController.MoveFloorDetector(relativeMoveDirection);




        return;
        Vector3 GetGravityVelocity(bool isGrounded)
        {
            if (!isGrounded)
                return _gravityVelocity += GravityDirection * Time.deltaTime;
            else
                return _gravityVelocity = Vector3.zero;
        }
    }
}
