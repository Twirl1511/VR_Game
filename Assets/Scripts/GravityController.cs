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
        NormalDirection = Vector3.up;
    }

    private void FixedUpdate()
    {
        if (gravityProvider.Input != Vector2.zero)
        {
            var translationInWorldSpace = Vector3.Normalize(gravityProvider.TranslationInWorldSpace);
            Vector3 targetPosition = center.position + translationInWorldSpace * radius;
            _model.position = targetPosition;
        }

        if(flag)
            SurfaceAlignment();
    }

    //private void SurfaceAlignment()
    //{
    //    Ray ray = new Ray(_model.position, -_model.up);
    //    RaycastHit hitInfo;

    //    Quaternion rotationRef = Quaternion.identity;

    //    if (Physics.Raycast(ray, out hitInfo, _distance, groundLayer))
    //    {
    //        GravityDirection = Vector3.Normalize(-hitInfo.normal) * _gravityForce;
    //        NormalDirection = Vector3.Normalize(hitInfo.normal);
    //    }

    //    rotationRef = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(Vector3.up, NormalDirection), _animationCurve.Evaluate(Time.time) * speed);
    //    transform.rotation = Quaternion.Euler(rotationRef.eulerAngles.x, transform.eulerAngles.y, rotationRef.eulerAngles.z);
    //}

    private void SurfaceAlignment()
    {
        Ray ray = new Ray(_model.position, -_model.up);
        RaycastHit hitInfo;

        Quaternion rotationRef = transform.rotation; // Используем текущее вращение для стабильности

        if (Physics.Raycast(ray, out hitInfo, _distance, groundLayer))
        {
            GravityDirection = Vector3.Normalize(-hitInfo.normal) * _gravityForce;
            NormalDirection = Vector3.Normalize(hitInfo.normal);

            // Получаем требуемое вращение для нормали поверхности
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, NormalDirection);

            // Плавное вращение с учетом текущего угла и заданной скорости
            rotationRef = Quaternion.Lerp(rotationRef, targetRotation, _animationCurve.Evaluate(Time.time) * speed);

            // Присваиваем итоговое вращение
            transform.rotation = rotationRef;
        }

    }

    public bool flag = true;
}
