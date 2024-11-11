using UnityEngine;

public class PhysicsHand : MonoBehaviour
{
    [Header("PID")]
    [SerializeField] private float _frequency = 50f;
    [SerializeField] private float _damping = 1f;
    [SerializeField] private float _rotFrequency = 100f;
    [SerializeField] private float _rotDamping = 0.9f;
    [SerializeField] private float _speed = 3f;
    [SerializeField] private CustomGravityCharacterController _customGravityCharacterController;
    [SerializeField] private Rigidbody _playerRigidbody;
    [SerializeField] private Transform _targetHand;
    [SerializeField] private Rigidbody _selfRigidbody;
    [SerializeField] private float _smoothSpeed = 0.1f;
    [SerializeField] private ControllerCollisionChecker _controllerCollisionChecker;

    public bool IsCollision { get; private set; }

    private Vector3 _startCollision;
    private Vector3 _smoothedPosition;
    private bool _canMove = true;

    void Start()
    {
        transform.SetPositionAndRotation(_targetHand.position, _targetHand.rotation);
        _selfRigidbody.maxAngularVelocity = float.PositiveInfinity;
        _smoothedPosition = _targetHand.position;
    }

    private void FixedUpdate()
    {
        if (!_controllerCollisionChecker.IsTriggered)
        {
            PIDMovement();
            PIDRotation();
        }
        else
        {
            /// тут нужно вручную задавать гравитацию рукам, чтобы они не улетали
        }
            

        if (IsCollision && _canMove)
        {
            _smoothedPosition = Vector3.Lerp(_smoothedPosition, _targetHand.position, _smoothSpeed * Time.deltaTime);
            Vector3 direction = _startCollision - _smoothedPosition;
            Vector2 normalizedInput = ConvertDirectionToInput(direction);

            /// прикол что в момент касания магнитуда меньше 0,21 и мы сразу кэн мув в фолс кидаем


            if (direction.magnitude > 0.21f)
            {
                _customGravityCharacterController.Move(normalizedInput, direction.magnitude * _speed);
            }
            else
            {
                _canMove = false;
            }
            /// придумать как измебжать дергания когда рука и стартовая позиция близко друг к другу
            /// при проверке магнитуды все равно дергается, нужно доводить до стартовой позиции и больше не считать
            /// вообще не заходить в из колижен

        }
        else
        {
            _smoothedPosition = _targetHand.position;
        }
    }

    private Vector2 ConvertDirectionToInput(Vector3 direction)
    {
        Vector3 localDirection = Camera.main.transform.InverseTransformDirection(direction);
        Vector2 input = new Vector2(localDirection.x, localDirection.z);

        return input.normalized;
    }

    private void PIDMovement()
    {
        float kp = (6f * _frequency) * (6f * _frequency) * 0.25f;
        float kd = 4.5f * _frequency * _damping;
        float g = 1 / (1 + kd * Time.fixedDeltaTime + kp * Time.fixedDeltaTime * Time.fixedDeltaTime);
        float ksg = kp * g;
        float kdg = (kd + kp * Time.fixedDeltaTime) * g;
        Vector3 force = (_targetHand.position - transform.position) * ksg + (_playerRigidbody.velocity - _selfRigidbody.velocity) * kdg;
        _selfRigidbody.AddForce(force, ForceMode.Acceleration);
    }

    private void PIDRotation()
    {
        float kp = (6f * _rotFrequency) * (6f * _rotFrequency) * 0.25f;
        float kd = 4.5f * _rotFrequency * _rotDamping;
        float g = 1 / (1 + kd * Time.fixedDeltaTime + kp * Time.fixedDeltaTime * Time.fixedDeltaTime);
        float ksg = kp * g;
        float kdg = (kd + kp * Time.fixedDeltaTime) * g;
        Quaternion q = _targetHand.rotation * Quaternion.Inverse(transform.rotation);
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

    private void OnCollisionEnter(Collision collision)
    {
        _startCollision = transform.position;
        IsCollision = true;
        _canMove = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        IsCollision = false;
    }
}
