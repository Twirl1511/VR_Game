using UnityEngine;

public class BatCapsuleFollower : MonoBehaviour
{
    [SerializeField]
    private float _sensitivity = 90f;

    [SerializeField]
    private float _speed = 0.5f;

    private BatCapsule _batCapsule;
    private Rigidbody _rigidbody;
    private Vector3 _velocity;




    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_batCapsule == null)
            return;

        Vector3 destination = _batCapsule.transform.position;
        _rigidbody.transform.rotation = _batCapsule.transform.rotation;

        _velocity = (destination - _rigidbody.transform.position) * _sensitivity;

        _rigidbody.velocity = _velocity * _speed;
    }

    public void SetFollowTarget(BatCapsule batCapsule)
    {
        _batCapsule = batCapsule;
    }
}