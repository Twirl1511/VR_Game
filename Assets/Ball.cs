using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
    private const float TIME = 0.5f;



    [SerializeField]
    private Rigidbody _rigidbody;

    [SerializeField]
    private GameObject _particlesHit;

    [SerializeField]
    private MeshRenderer _mesh;

    [SerializeField]
    private TrailRenderer _trail;

    private Collider _collider;
    private Vector3 _direction;
    private bool _isHitWithSaber;
    private float _time;
    private Vector3 _hitSaberPosition;





    private void Start()
    {
        _collider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        if(IsReady() && _isHitWithSaber == false)
            _direction = _rigidbody.linearVelocity.normalized;
    }

    private bool IsReady()
    {
        _time += Time.deltaTime;
        if (_time < TIME)
            return false;

        _time = 0;
        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        bool isSaberHit = false;
        Vector3 dir = default;
        if (collision.gameObject.CompareTag("Saber"))
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _isHitWithSaber = true;
            dir = GetAngleDirection(collision.transform);
            _hitSaberPosition = transform.position;

            isSaberHit = true;
        }
        StartCoroutine(AddForce(isSaberHit, dir));
    }

    private IEnumerator AddForce(bool isSaberHit, Vector3 dir)
    {
        if (isSaberHit)
        {
            yield return new WaitForEndOfFrame();
            transform.position = _hitSaberPosition;
            _trail.enabled = true;
            _mesh.enabled = true;
            _rigidbody.linearVelocity = dir * 20f;
            _isHitWithSaber = false;
        }
        else
        {
            yield return new WaitForEndOfFrame();
            _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * 10f;
            _isHitWithSaber = false;
        }

        //yield return new WaitForEndOfFrame();
    }

    private Vector3 GetAngleDirection(Transform batTransform)
    {
        return batTransform.gameObject.GetComponent<Rigidbody>().linearVelocity;


        // �������� ���� ������� ����
        float angle = Vector3.Angle(_direction, batTransform.forward);
        // �������� ���� ������� ����

        Vector3 batEuler = batTransform.eulerAngles;
        float batAngle = Mathf.Abs(90 - batEuler.x) + Mathf.Abs(batEuler.y) + Mathf.Abs(batEuler.z);

        // ������������ ���� �������
        float bounceAngle = angle - (angle - batAngle) * 2;
        // ��������� ���� ������� � ����������� �������� ����
        Vector3 bounceDirection = Quaternion.AngleAxis(bounceAngle, Vector3.up) * -_direction.normalized;
        return bounceDirection;
    }
}
