using UnityEngine;

public class PhysicsHand : MonoBehaviour
{
    [Header("PID")]
    [SerializeField] private float _frequency = 50f;
    [SerializeField] private float _damping = 1f;
    [SerializeField] private float _rotFrequency = 100f;
    [SerializeField] private float _rotDamping = 0.9f;
    [SerializeField] private Rigidbody _playerRigidbody;
    [SerializeField] private Transform _targetHand;
    [SerializeField] private Rigidbody _selfRigidbody;

    void Start()
    {
        transform.SetPositionAndRotation(_targetHand.position, _targetHand.rotation);
        _selfRigidbody.maxAngularVelocity = float.PositiveInfinity;
    }

    private void FixedUpdate()
    {
        PIDMovement();
        PIDRotation();
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
}
