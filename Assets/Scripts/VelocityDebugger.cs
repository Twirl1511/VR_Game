﻿using UnityEngine;

public class VelocityDebugger : MonoBehaviour
{
    [SerializeField]
    private float maxVelocity = 20f;

    private Renderer _renderer;
    private Rigidbody _rigidbody;




    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _renderer.material.color = ColorForVelocity();
    }

    private Color ColorForVelocity()
    {
        float velocity = _rigidbody.linearVelocity.magnitude;

        if (velocity >= maxVelocity)
            return Color.red;
        else
            return Color.green;
    }
}
