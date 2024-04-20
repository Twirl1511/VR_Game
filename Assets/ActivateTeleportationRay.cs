using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActivateTeleportationRay : MonoBehaviour
{
    private const float TIME_INACTIVE = 0.1f;




    [SerializeField]
    private GameObject
        _leftTeleportation,
        _rightTeleportation;

    [SerializeField]
    private InputActionProperty 
        _leftActivate, 
        _rightActivate;

    [SerializeField]
    private InputActionProperty
        _leftCancel,
        _rightCancel;




    void Update()
    {
        _leftTeleportation.SetActive(
            _leftCancel.action.ReadValue<float>() < TIME_INACTIVE && 
            _leftActivate.action.ReadValue<float>() >= TIME_INACTIVE);

        _rightTeleportation.SetActive(
            _rightCancel.action.ReadValue<float>() < TIME_INACTIVE && 
            _rightActivate.action.ReadValue<float>() >= TIME_INACTIVE);
    }

}
