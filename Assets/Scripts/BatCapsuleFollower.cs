using UnityEngine;

public class BatCapsuleFollower : MonoBehaviour
{
    private const float TIME = 0.5f;




    [SerializeField]
    private float _sensitivity = 90f;

    [SerializeField]
    private float _speed = 0.5f;

    [SerializeField]
    private float _speedToReflect = 5f;

    private BatCapsule _batCapsule;
    private Rigidbody _rigidbody;
    private Vector3 _velocity;
    private float _time;
    private bool _canReflect;



    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private bool IsReady()
    {
        _time += Time.deltaTime;
        if (_time < TIME)
            return false;

        _time = 0;
        return true;
    }

    private void FixedUpdate()
    {
        if (_batCapsule == null)
            return;

        Vector3 destination = _batCapsule.transform.position;
        _rigidbody.transform.rotation = _batCapsule.transform.rotation;

        _velocity = (destination - _rigidbody.transform.position) * _sensitivity;

        _rigidbody.linearVelocity = _velocity * _speed;
    }

    public void SetFollowTarget(BatCapsule batCapsule)
    {
        _batCapsule = batCapsule;
    }

    public bool CanReflect()
    {
        return _canReflect = _rigidbody.linearVelocity.magnitude <= _speedToReflect;
    }
}