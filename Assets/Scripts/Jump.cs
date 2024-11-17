using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Jump : MonoBehaviour
{
    [SerializeField] private GravityController _gravityController;
    [SerializeField] private InputActionReference _jumpButton;
    [SerializeField] private float _force = 10f;


    private bool IsGround => _gravityController.IsGround;


    private Rigidbody _rigidbody;


    public event Action OnJump;




    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _jumpButton.action.performed += PressButton;
    }

    private void OnDestroy()
    {
        _jumpButton.action.performed -= PressButton;
    }

    private void PressButton(InputAction.CallbackContext obj)
    {
        if (!IsGround)
            return;

        ExecuteJump();
    }

    private void ExecuteJump()
    {
        _rigidbody.AddForce(Camera.main.transform.forward * _force, ForceMode.Acceleration);
        OnJump?.Invoke();
    }
}
