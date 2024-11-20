using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FirePistol : MonoBehaviour
{
    private const float LIFE_TIME = 5f;




    [SerializeField]
    private Rigidbody _bulletPrefab;

    [SerializeField]
    private Transform _spawnPointTransform;

    [SerializeField]
    private float _speed;




    private void Start()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        interactable.activated.AddListener(Fire); 
    }

    private void Fire(ActivateEventArgs args)
    {
        GameObject bullet = Instantiate(
            _bulletPrefab.gameObject, 
            _spawnPointTransform.position, 
            Quaternion.identity);

        bullet.GetComponent<Rigidbody>().linearVelocity = _spawnPointTransform.forward * _speed;

        Destroy(bullet, LIFE_TIME);
    }
}
