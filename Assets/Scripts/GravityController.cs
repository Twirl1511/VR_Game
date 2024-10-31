using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    public float speed = 5f;   
    [SerializeField] private float _gravityForce = 9.81f;
    [SerializeField] private Transform _model;
    public LayerMask groundLayer;
    [SerializeField] private AnimationCurve _animationCurve;
    [SerializeField] private GameObject _cameraOffset;

    public Transform center;                  // Центр для перемещения модели
    public float radius = 5f;                 // Радиус, на котором объект будет находиться
    public CustomGravityActionBasedModeProvider gravityProvider; // Ссылка на провайдера ввода

    private Vector3 initialPosition;          // Исходное положение модели
    private CharacterController characterController;

    //public Vector3 transInWSpace;
    public Vector3 GravityDirection { get; private set; }
    public Transform CameraOffsetTransform => _cameraOffset.transform;

    public Vector3 normalDirection;
    public Vector3 inputForwardProjectedInWorldSpace;
    public Vector3 originalTranformForward;
    public Vector3 inputForwardInWorldSpaceTEST;
    private Vector3 inputMoveTest;
    private Quaternion forwardRotationTest;

    private void Start()
    {
        GravityDirection = Vector3.down * _gravityForce;
        characterController = GetComponent<CharacterController>();
        initialPosition = _model.position;     // Сохраняем начальное положение модели
    }

    private void FixedUpdate()
    {
        
        // Получаем вектор направления от ввода
        Vector2 input = gravityProvider.Input;

        // Проверка наличия ввода
        if (input != Vector2.zero) // Если есть движение
        {
            // Получаем направление движения в мировом пространстве
            Vector3 newInput = GetTranslationInWorldSpace(input);

            // Перемещаем модель к новой позиции
            Vector3 targetPosition = center.position + newInput * radius;
            _model.position = targetPosition;
        }

        SurfaceAlignment();
    }

    private Vector3 GetTranslationInWorldSpace(Vector2 input)
    {
        var inputMove = Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);
        inputMoveTest = inputMove;
        Transform originTransform = _cameraOffset.transform;

        inputForwardInWorldSpaceTEST = Camera.main.transform.forward;
        inputForwardProjectedInWorldSpace = Vector3.ProjectOnPlane(Camera.main.transform.forward, normalDirection);
        
        var forwardRotation = Quaternion.FromToRotation(_cameraOffset.transform.forward, inputForwardProjectedInWorldSpace);

        forwardRotationTest = new Quaternion(forwardRotation.x, -forwardRotation.z, forwardRotation.y, forwardRotation.w) ;
        var translationInRigSpace = forwardRotationTest * inputMove;
        var translationInWorldSpace = originTransform.TransformDirection(translationInRigSpace);

        /// на земле по игрику
        /// на стене по зету!!!!!
        /// вращение

        return translationInWorldSpace;
    }

    private void SurfaceAlignment()
    {
        Ray ray = new Ray(_model.position, -_model.up);
        RaycastHit hitInfo;

        Quaternion rotationRef = Quaternion.identity;

        if (Physics.Raycast(ray, out hitInfo, groundLayer))
        {
            GravityDirection = Vector3.Normalize(-hitInfo.normal) * _gravityForce;
            normalDirection = Vector3.Normalize(hitInfo.normal);
            // Применяем выравнивание с использованием анимационной кривой
            rotationRef = Quaternion.Lerp(_cameraOffset.transform.rotation, Quaternion.FromToRotation(Vector3.up, hitInfo.normal), _animationCurve.Evaluate(Time.time) * speed);
            _cameraOffset.transform.rotation = Quaternion.Euler(rotationRef.eulerAngles.x, _cameraOffset.transform.eulerAngles.y, rotationRef.eulerAngles.z);
        }
    }
}
