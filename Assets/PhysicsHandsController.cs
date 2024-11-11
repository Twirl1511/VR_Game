using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsHandsController : MonoBehaviour
{
    [SerializeField] private PhysicsHand[] _physicsHands;
    [SerializeField] private CustomGravityCharacterController _customGravityCharacterController;

    private void FixedUpdate()
    {
        bool isCollision = false;
        for (int i = 0; i < _physicsHands.Length; i++)
        {
            if (!_physicsHands[i].IsCollision)
                continue;

            isCollision = true;
        }

        if (!isCollision)
            _customGravityCharacterController.Move(Vector2.zero, 0f);
    }
}
