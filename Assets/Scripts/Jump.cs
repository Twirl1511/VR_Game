using System;
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

    private void PressButton(InputAction.CallbackContext _)
    {
        if (!IsGround)
            return;

        ExecuteJump();
    }

    private void ExecuteJump()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 forwardOnSurface = Vector3.ProjectOnPlane(cameraForward, _gravityController.NormalDirection).normalized;
        bool isLookingBelowHorizon = Vector3.Dot(cameraForward, _gravityController.NormalDirection) < 0;
        if (isLookingBelowHorizon)
        {
            _rigidbody.AddForce(forwardOnSurface * _force, ForceMode.Acceleration);
        }
        else
        {
            _rigidbody.AddForce(cameraForward * _force, ForceMode.Acceleration);
        }

        OnJump?.Invoke();
    }
}
