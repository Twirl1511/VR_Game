using UnityEngine;

public class LeaveParent : MonoBehaviour
{
    void Start()
    {
        transform.SetParent(null);   
    }
}
