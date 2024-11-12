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
    private Vector3 _gravityVelocity;
    private bool _isGrounded;




    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.useGravity = false;
        _rigidBody.isKinematic = false;
        //_isGrounded = GetIsGround();
    }
    private void FixedUpdate()
    {
        //_isGrounded = GetIsGround();
        SurfaceAlignment();
    }

    //private bool GetIsGround()
    //{
    //    return Physics.Raycast(transform.position, -transform.up, out _, _checkGroundDistance);
    //}

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

        Vector3 moveDirection = (cameraRight * input.x + cameraForward * input.y).normalized;
        Vector3 relativeMoveDirection = Vector3.ProjectOnPlane(moveDirection, NormalDirection).normalized;
        Vector3 moveVelocity = relativeMoveDirection * GetSpeed();

        //_gravityVelocity = GetGravityVelocity(_isGrounded);
        _rigidBody.MovePosition(_rigidBody.position + (moveVelocity + _gravityController.GravityVelocity) * Time.deltaTime);

        if (input != Vector2.zero)
        {
            _gravityController.MoveSurfaceDetector(relativeMoveDirection);
            IsMoving = input != Vector2.zero;
        }




        //return;
        //Vector3 GetGravityVelocity(bool isGrounded)
        //{
        //    Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, float.MaxValue);
        //    float distance = Vector3.Distance(transform.position, hit.point);

        //    if (distance <= _checkGroundDistance)
        //    {
        //        return _gravityVelocity = Vector3.zero;
        //    }
        //    else if (distance > _checkGroundDistance && distance < 2)
        //    {
        //        return _gravityVelocity += GravityDirection * Time.deltaTime;
        //    }
        //    else if (distance > 2)
        //    {
        //        return _gravityVelocity += Vector3.down * 9.81f * Time.deltaTime;
        //    }

        //    /// когда понимаем что гравитация вниз, то всегда поворачиваем в сторону нормали вверх
        //    /// походу это все надо переносить в гравити контроллер
        //    /// 
        //    /// 
        //    /// 
        //    /// еще надо придумать как залезать по стене на крышу!!!!!!!!!!!!!!!!!!!!!!!!
        //    /// 

        //    /// придумать сглаживание для прыжка, чтобы плеера не колбасило

        //    print("ERROR!!!!!!!!!!!!!!!!!!");
        //    return _gravityVelocity = Vector3.zero;


        //    //if (isGrounded)
        //    //    return _gravityVelocity = Vector3.zero;
        //    //else
        //    //    return _gravityVelocity += GravityDirection * Time.deltaTime;
        //}
    }

    //public void Move(Vector2 input, float speed)
    //{
    //    Vector3 cameraForward = Vector3.ProjectOnPlane(_camera.forward, NormalDirection).normalized;
    //    Vector3 cameraRight = Vector3.ProjectOnPlane(_camera.right, NormalDirection).normalized;

    //    Vector3 moveDirection = (cameraRight * input.x + cameraForward * input.y).normalized;
    //    Vector3 relativeMoveDirection = Vector3.ProjectOnPlane(moveDirection, NormalDirection).normalized;
    //    Vector3 moveVelocity = relativeMoveDirection * speed;

    //    _gravityVelocity = GetGravityVelocity(_isGrounded);
    //    _rigidBody.MovePosition(_rigidBody.position + (moveVelocity + _gravityVelocity) * Time.deltaTime);

    //    if (input != Vector2.zero)
    //    {
    //        _gravityController.MoveFloorDetector(relativeMoveDirection);
    //        IsMoving = input != Vector2.zero;
    //    }




    //    return;
    //    Vector3 GetGravityVelocity(bool isGrounded)
    //    {
    //        if (!isGrounded)
    //            return _gravityVelocity += GravityDirection * Time.deltaTime;
    //        else
    //            return _gravityVelocity = Vector3.zero;
    //    }
    //}
}
