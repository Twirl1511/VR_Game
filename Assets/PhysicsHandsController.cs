using System;
using UnityEngine;

public class PhysicsHandsController : MonoBehaviour
{
    [SerializeField] private PhysicsHand[] _physicsHands;
    [SerializeField] private CustomGravityCharacterController _customGravityCharacterController;

    public event Action OnJump;

    private void Start()
    {
        for (int i = 0; i < _physicsHands.Length; i++)
        {
            _physicsHands[i].OnJump += ActivateJump;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < _physicsHands.Length; i++)
        {
            _physicsHands[i].OnJump -= ActivateJump;
        }
    }

    private void ActivateJump()
    {
        OnJump?.Invoke();
    }
}
