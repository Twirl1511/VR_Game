using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class CustomGravityActionBasedModeProvider : ContinuousMoveProviderBase
{
    [SerializeField]
    private GravityController _gravityController;

    [SerializeField]
    [Tooltip("Controls when gravity begins to take effect.")]
    private GravityApplicationMode _gravityApplicationModeOverrided;

    [SerializeField]
    private bool _isUseGravity;

    [SerializeField]
    private float _speed;

    [SerializeField]
    [Tooltip("The Input System Action that will be used to read Move data from the left hand controller. Must be a Value Vector2 Control.")]
    private InputActionProperty m_LeftHandMoveAction = new InputActionProperty(new InputAction("Left Hand Move", expectedControlType: "Vector2"));
    
    
    [SerializeField]
    [Tooltip("The Input System Action that will be used to read Move data from the right hand controller. Must be a Value Vector2 Control.")]
    private InputActionProperty m_RightHandMoveAction = new InputActionProperty(new InputAction("Right Hand Move", expectedControlType: "Vector2"));
    

    /// <summary>
    /// The Input System Action that Unity uses to read Move data from the left hand controller. Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/> Control.
    /// </summary>
    public InputActionProperty LeftHandMoveAction
    {
        get => m_LeftHandMoveAction;
        set => SetInputActionProperty(ref m_LeftHandMoveAction, value);
    }
    /// <summary>
    /// The Input System Action that Unity uses to read Move data from the right hand controller. Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/> Control.
    /// </summary>
    public InputActionProperty RightHandMoveAction
    {
        get => m_RightHandMoveAction;
        set => SetInputActionProperty(ref m_RightHandMoveAction, value);
    }
    public Vector2 Input { get; private set; }
    public Vector3 TranslationInWorldSpace { get; private set; }

    
    [SerializeField] private CustomCharacterController _customCharacterController;
    private bool _attemptedGetCharacterController;
    private Vector3 _gravityDirection;
    private bool _isMovingXROrigin;

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void OnEnable()
    {
        m_LeftHandMoveAction.EnableDirectAction();
        m_RightHandMoveAction.EnableDirectAction();
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void OnDisable()
    {
        m_LeftHandMoveAction.DisableDirectAction();
        m_RightHandMoveAction.DisableDirectAction();
    }

    private new void Update()
    {
        _isMovingXROrigin = false;

        var xrOrigin = system.xrOrigin?.Origin;
        if (xrOrigin == null)
            return;

        Vector2 input = ReadInput();
        Input = input;
        var translationInWorldSpace = ComputeDesiredMove(input);
        TranslationInWorldSpace = translationInWorldSpace;
        switch (_gravityApplicationModeOverrided)
        {
            case GravityApplicationMode.Immediately:
                MoveRig(input);
                break;
            case GravityApplicationMode.AttemptingMove:
                if (input != Vector2.zero || _gravityDirection != Vector3.zero)
                    MoveRig(input);
                break;
            default:
                Assert.IsTrue(false, $"{nameof(_gravityApplicationModeOverrided)}={_gravityApplicationModeOverrided} outside expected range.");
                break;
        }

        switch (locomotionPhase)
        {
            case LocomotionPhase.Idle:
            case LocomotionPhase.Started:
                if (_isMovingXROrigin)
                    locomotionPhase = LocomotionPhase.Moving;
                break;
            case LocomotionPhase.Moving:
                if (!_isMovingXROrigin)
                    locomotionPhase = LocomotionPhase.Done;
                break;
            case LocomotionPhase.Done:
                locomotionPhase = _isMovingXROrigin ? LocomotionPhase.Moving : LocomotionPhase.Idle;
                break;
            default:
                Assert.IsTrue(false, $"Unhandled {nameof(LocomotionPhase)}={locomotionPhase}");
                break;
        }
    }

    //protected override Vector3 ComputeDesiredMove(Vector2 input)
    //{
    //    if (input == Vector2.zero)
    //        return Vector3.zero;

    //    var xrOrigin = system.xrOrigin;
    //    if (xrOrigin == null)
    //        return Vector3.zero;

    //    // Assumes that the input axes are in the range [-1, 1].
    //    // Clamps the magnitude of the input direction to prevent faster speed when moving diagonally,
    //    // while still allowing for analog input to move slower (which would be lost if simply normalizing).
    //    var inputMove = Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);

    //    // Determine frame of reference for what the input direction is relative to
    //    /// пока поставим, что ориентир это только камера, далее тут можно будет поставить ориентир руку
    //    var forwardSourceTransform = xrOrigin.Camera.transform;
    //    var inputForwardInWorldSpace = forwardSourceTransform.forward;

    //    var originTransform = xrOrigin.Origin.transform;
    //    //var speedFactor = m_MoveSpeed * Time.deltaTime * originTransform.localScale.x; // Adjust speed with user scale

    //    var originUp = originTransform.up;

    //    //if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(inputForwardInWorldSpace, originUp)), 1f))
    //    //{
    //    //    // When the input forward direction is parallel with the rig normal,
    //    //    // it will probably feel better for the player to move along the same direction
    //    //    // as if they tilted forward or up some rather than moving in the rig forward direction.
    //    //    // It also will probably be a better experience to at least move in a direction
    //    //    // rather than stopping if the head/controller is oriented such that it is perpendicular with the rig.
    //    //    inputForwardInWorldSpace = -forwardSourceTransform.up;
    //    //}

    //    var inputForwardProjectedInWorldSpace = Vector3.ProjectOnPlane(inputForwardInWorldSpace, _gravityController.NormalDirection);
    //    var forwardRotation = Quaternion.FromToRotation(_gravityController.CameraOffsetTransform.forward, inputForwardProjectedInWorldSpace);

    //    //var newForwardRotation = new Quaternion(forwardRotation.x, -forwardRotation.z, forwardRotation.y, forwardRotation.w);
    //    var newForwardRotation = GetRotaionDependsOnNormale(forwardRotation, _gravityController.NormalDirection);
    //    var translationInRigSpace = forwardRotation * inputMove * Time.deltaTime * _speed;
    //    var translationInWorldSpace = originTransform.TransformDirection(translationInRigSpace);

    //    return translationInWorldSpace;
    //}

    protected override Vector3 ComputeDesiredMove(Vector2 input)
    {
        //if (input == Vector2.zero)
        //    return Vector3.zero;

        var xrOrigin = system.xrOrigin;
        if (xrOrigin == null)
            return Vector3.zero;

        var inputMove = Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);
        var inputForwardInWorldSpace = xrOrigin.Camera.transform.forward;

        var inputForwardProjectedInWorldSpace = Vector3.ProjectOnPlane(inputForwardInWorldSpace, _gravityController.NormalDirection);
        var forwardRotation = Quaternion.FromToRotation(_gravityController.CameraOffsetTransform.forward, inputForwardProjectedInWorldSpace);
        
        
        forwardRotationTEST = forwardRotation;
        var NEWforwardRotation = GetRotaionDependsOnNormale(forwardRotation, _gravityController.NormalDirection);
        //normaDirection = _gravityController.NormalDirection;
        NEWforwardRotationTEST = NEWforwardRotation;


        var translationInRigSpace = NEWforwardRotation * inputMove * Time.deltaTime * _speed;
        //var translationInWorldSpace = xrOrigin.Origin.transform.TransformDirection(translationInRigSpace);
        var translationInWorldSpace = _gravityController.CameraOffsetTransform.TransformDirection(translationInRigSpace);
        direction = translationInWorldSpace;
        /// inputForwardProjectedInWorldSpace подставить вместо translationInRigSpace
        return translationInWorldSpace;
    }

    private Quaternion GetRotaionDependsOnNormale(Quaternion currentQuaternion, Vector3 normalDirection)
    {
        float y = 0;

        if (Mathf.Abs(normalDirection.x) > 0)
        {
            if(currentQuaternion.y > 0)
            {
                y += Mathf.Abs(currentQuaternion.x);
            }
            else
            {
                y -= Mathf.Abs(currentQuaternion.x);
            }
        }

        if (Mathf.Abs(normalDirection.y) > 0)
        {
            y += currentQuaternion.y;
        }

        if (Mathf.Abs(normalDirection.z) > 0)
        {
            if (currentQuaternion.y > 0)
            {
                y += Mathf.Abs(currentQuaternion.z);
            }
            else
            {
                y -= Mathf.Abs(currentQuaternion.z);
            }
        }

        if(Mathf.Approximately(normalDirection.z, -1))
        {
            y *= -1;
        }
        else 
        {

        }



        return new Quaternion(0, y, 0, currentQuaternion.w);
    }

    private Vector3 direction, pointB;
    public Quaternion forwardRotationTEST;
    public Quaternion NEWforwardRotationTEST;
    public Vector3 normaDirection;

    private void OnDrawGizmos()
    {
        // Устанавливаем цвет для вектора
        Gizmos.color = Color.red;

        // Рассчитываем конечную точку вектора
        Vector3 endPoint = _gravityController.CameraOffsetTransform.position + direction.normalized * 5;

        // Рисуем основной вектор
        Gizmos.DrawLine(_gravityController.CameraOffsetTransform.position, endPoint);
    }

    private void FindCharacterController()
    {
        var xrOrigin = system.xrOrigin?.Origin;
        if (xrOrigin == null)
            return;

        // Save a reference to the optional CharacterController on the rig GameObject
        // that will be used to move instead of modifying the Transform directly.
        if (_customCharacterController == null && !_attemptedGetCharacterController)
        {
            // Try on the Origin GameObject first, and then fallback to the XR Origin GameObject (if different)
            if (!xrOrigin.TryGetComponent(out _customCharacterController) && xrOrigin != system.xrOrigin.gameObject)
                system.xrOrigin.TryGetComponent(out _customCharacterController);

            _attemptedGetCharacterController = true;
        }
    }

    protected override void MoveRig(Vector3 translationInWorldSpace)
    {
        var xrOrigin = system.xrOrigin?.Origin;
        if (xrOrigin == null)
            return;

        FindCharacterController();

        var motion = translationInWorldSpace;

        if (_customCharacterController != null && _customCharacterController.enabled)
        {
            //// Step vertical velocity from gravity
            //if (_customCharacterController.isGrounded || !_isUseGravity)
            //{
            //    _gravityDirection = Vector3.zero;
            //    /// поставить, что гравитация вниз
            //}
            //else
            //{
            //    /// тут ка краз заменить на кастомную гравитацию
            //    _gravityDirection += _gravityController.GravityDirection * Time.deltaTime;
            //}

            
            //motion += _gravityDirection * Time.deltaTime;

            if (CanBeginLocomotion() && BeginLocomotion())
            {
                // Note that calling Move even with Vector3.zero will have an effect by causing isGrounded to update
                _isMovingXROrigin = true;
                _customCharacterController.Move(motion);
                EndLocomotion();
            }
        }
        else
        {
            if (CanBeginLocomotion() && BeginLocomotion())
            {
                _isMovingXROrigin = true;
                xrOrigin.transform.position += motion;
                EndLocomotion();
            }
        }
    }

    /// <inheritdoc />
    protected override Vector2 ReadInput()
    {
        var leftHandValue = m_LeftHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
        var rightHandValue = m_RightHandMoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;

        return leftHandValue + rightHandValue;
    }

    private void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
    {
        if (Application.isPlaying)
            property.DisableDirectAction();

        property = value;

        if (Application.isPlaying && isActiveAndEnabled)
            property.EnableDirectAction();
    }
}
