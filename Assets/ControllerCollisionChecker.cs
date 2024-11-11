using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerCollisionChecker : MonoBehaviour
{
    public bool IsTriggered { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        IsTriggered = true;
    }

    private void OnTriggerExit(Collider other)
    {
        IsTriggered = false;
    }
}
