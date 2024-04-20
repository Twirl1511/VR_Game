using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
    private const float TIME = 0.5f;



    [SerializeField]
    private Rigidbody _rigidbody;

    private Collider _collider;
    private Vector3 _direction;
    private bool _isHitWithSaber;
    private float _time;



    private void Start()
    {
        _collider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        if(IsReady() && _isHitWithSaber == false)
            _direction = _rigidbody.velocity.normalized;
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
        bool isSlowReflecting = false;
        if (collision.gameObject.CompareTag("Saber"))
        {
            _isHitWithSaber = true;

            if (collision.gameObject.GetComponent<BatCapsuleFollower>().CanReflect())
            {
                isSlowReflecting = true;
                print(1111);
            }
            print(222);
            StartCoroutine(AddForce(isSlowReflecting));
        }

        
    }

    private IEnumerator AddForce(bool isSlowReflecting)
    {
        if (isSlowReflecting)
        {
            yield return new WaitForFixedUpdate();
            _rigidbody.velocity = _direction * -10f;
            _isHitWithSaber = false;
        }
        else
        {
            yield return new WaitForFixedUpdate();
            _rigidbody.velocity = _rigidbody.velocity.normalized * 10f;
            _isHitWithSaber = false;
        }
    }
}
