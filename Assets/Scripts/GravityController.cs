using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    public float speed = 5f;   
    [SerializeField] private float _gravityForce = 9.81f;
    [SerializeField] private float _distance = 2f;
    [SerializeField] private Transform _model;
    public LayerMask groundLayer;
    [SerializeField] private AnimationCurve _animationCurve;
    [SerializeField] private GameObject _cameraOffset;

    public Transform center;                  // Центр для перемещения модели
    public float radius = 5f;                 // Радиус, на котором объект будет находиться
    public CustomGravityActionBasedModeProvider gravityProvider; // Ссылка на провайдера ввода

    //public Vector3 transInWSpace;
    public Vector3 GravityDirection { get; private set; }
    public Transform CameraOffsetTransform => _cameraOffset.transform;
    public Vector3 NormalDirection { get; private set; }

    private void Start()
    {
        GravityDirection = Vector3.down * _gravityForce;
    }

    private void FixedUpdate()
    {
        if (gravityProvider.Input != Vector2.zero)
        {
            var translationInWorldSpace = Vector3.Normalize(gravityProvider.TranslationInWorldSpace);
            Vector3 targetPosition = center.position + translationInWorldSpace * radius;
            _model.position = targetPosition;
        }

        SurfaceAlignment();
    }

    private void SurfaceAlignment()
    {
        Ray ray = new Ray(_model.position, -_model.up);
        RaycastHit hitInfo;

        Quaternion rotationRef = Quaternion.identity;

        if (Physics.Raycast(ray, out hitInfo, _distance, groundLayer))
        {
            GravityDirection = Vector3.Normalize(-hitInfo.normal) * _gravityForce;
            NormalDirection = Vector3.Normalize(hitInfo.normal);

            rotationRef = Quaternion.Lerp(_cameraOffset.transform.rotation, Quaternion.FromToRotation(Vector3.up, hitInfo.normal), _animationCurve.Evaluate(Time.time) * speed);
            _cameraOffset.transform.rotation = Quaternion.Euler(rotationRef.eulerAngles.x, _cameraOffset.transform.eulerAngles.y, rotationRef.eulerAngles.z);
        }
    }
}
