using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class BallController : MonoBehaviour
{
    public XRController rightHand;
    public InputHelpers.Button button;

    [SerializeField]
    private Transform _ball;

    [SerializeField]
    private Transform _defaultPositionTransform;

    [SerializeField]
    private BallShooter _ballShooter;

    private Rigidbody _rigidbody;
    private bool _isPressed;
    private bool _isReady;





    private void Start()
    {
        _rigidbody = _ball.GetComponent<Rigidbody>();
        _isReady = true;
    }

    void Update()
    {
        if (_isReady == false)
            return;

        rightHand.inputDevice.IsPressed(button, out _isPressed);

        if (_isPressed)
        {
            StartCoroutine(Shoot());

            //_ball.position = _defaultPositionTransform.position;
            //_rigidbody.velocity = Vector3.zero;
        }
    }

    private IEnumerator Shoot()
    {
        _isReady = false;
        _ballShooter.Shoot();
        yield return new WaitForSeconds(1f);
        _isReady = true;
    }
}

