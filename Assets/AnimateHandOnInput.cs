using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour
{
    [SerializeField]
    private InputActionProperty 
        _pinchActionProperty,
        _gripActionProperty;

    [SerializeField]
    private Animator _animator;

    private readonly int _trigger = Animator.StringToHash("Trigger");
    private readonly int _grip = Animator.StringToHash("Grip");




    private void Update()
    {
        float triggervalue = _pinchActionProperty.action.ReadValue<float>();
        _animator.SetFloat(_trigger, triggervalue);

        float gripValue = _gripActionProperty.action.ReadValue<float>();
        _animator.SetFloat(_grip, gripValue);

    }
}
