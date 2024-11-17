using UnityEngine;

public class ObjectToCatch : MonoBehaviour
{
    public bool IsTouched { get; private set; }




    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        IsTouched = true;
    }
}
