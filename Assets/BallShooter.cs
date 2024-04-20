using UnityEngine;
using System.Collections;

public class BallShooter : MonoBehaviour
{
    [SerializeField]
    private Transform _gun;

    [SerializeField]
    private Transform _shootPosition;

    [SerializeField]
    private GameObject _ball;

    [SerializeField]
    private Transform _circle;




    private void Start()
    {
        StartCoroutine(ShootForever());
    }

    public void Shoot()
    {
        _circle.eulerAngles = new Vector3(
            0,
            0,
            Random.Range(0, 361));

        GameObject ball = Instantiate(_ball, _shootPosition.transform.position, Quaternion.identity, null);
        ball.GetComponent<Rigidbody>().velocity = _shootPosition.forward * 10f;
        Destroy(ball, 10);
    }

    private IEnumerator ShootForever()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            Shoot();
        }
    }
}
