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
    [Tooltip("The Input System Action that will be used to read Move data from the left hand controller. Must be a Value Vector2 Control.")]
    InputActionProperty m_LeftHandMoveAction = new InputActionProperty(new InputAction("Left Hand Move", expectedControlType: "Vector2"));
    /// <summary>
    /// The Input System Action that Unity uses to read Move data from the left hand controller. Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/> Control.
    /// </summary>
    public InputActionProperty leftHandMoveAction
    {
        get => m_LeftHandMoveAction;
        set => SetInputActionProperty(ref m_LeftHandMoveAction, value);
    }

    [SerializeField]
    [Tooltip("The Input System Action that will be used to read Move data from the right hand controller. Must be a Value Vector2 Control.")]
    InputActionProperty m_RightHandMoveAction = new InputActionProperty(new InputAction("Right Hand Move", expectedControlType: "Vector2"));
    /// <summary>
    /// The Input System Action that Unity uses to read Move data from the right hand controller. Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/> Control.
    /// </summary>
    public InputActionProperty rightHandMoveAction
    {
        get => m_RightHandMoveAction;
        set => SetInputActionProperty(ref m_RightHandMoveAction, value);
    }

    public Vector2 Input { get; private set; }
    public Vector2 TransInWorldSpace { get; private set; }

    private CharacterController _characterController;
    private bool _attemptedGetCharacterController;
    private Vector3 _gravityDirection;
    private bool _isMovingXROrigin;

    public float _speed;


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

        switch (_gravityApplicationModeOverrided)
        {
            case GravityApplicationMode.Immediately:
                MoveRig(translationInWorldSpace);
                break;
            case GravityApplicationMode.AttemptingMove:
                if (input != Vector2.zero || _gravityDirection != Vector3.zero)
                    MoveRig(translationInWorldSpace);
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

    protected override Vector3 ComputeDesiredMove(Vector2 input)
    {
        //return base.ComputeDesiredMove(input);

        if (input == Vector2.zero)
            return Vector3.zero;

        var xrOrigin = system.xrOrigin;
        if (xrOrigin == null)
            return Vector3.zero;

        // Assumes that the input axes are in the range [-1, 1].
        // Clamps the magnitude of the input direction to prevent faster speed when moving diagonally,
        // while still allowing for analog input to move slower (which would be lost if simply normalizing).
        var inputMove = Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);

        // Determine frame of reference for what the input direction is relative to
        /// пока поставим, что ориентир это только камера, далее тут можно будет поставить ориентир руку
        var forwardSourceTransform = xrOrigin.Camera.transform;
        var inputForwardInWorldSpace = forwardSourceTransform.forward;

        var originTransform = xrOrigin.Origin.transform;
        //var speedFactor = m_MoveSpeed * Time.deltaTime * originTransform.localScale.x; // Adjust speed with user scale

        var originUp = originTransform.up;

        //if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(inputForwardInWorldSpace, originUp)), 1f))
        //{
        //    // When the input forward direction is parallel with the rig normal,
        //    // it will probably feel better for the player to move along the same direction
        //    // as if they tilted forward or up some rather than moving in the rig forward direction.
        //    // It also will probably be a better experience to at least move in a direction
        //    // rather than stopping if the head/controller is oriented such that it is perpendicular with the rig.
        //    inputForwardInWorldSpace = -forwardSourceTransform.up;
        //}

        var inputForwardProjectedInWorldSpace = Vector3.ProjectOnPlane(inputForwardInWorldSpace, _gravityController.normalDirection);
        var forwardRotation = Quaternion.FromToRotation(_gravityController.CameraOffsetTransform.forward, inputForwardProjectedInWorldSpace);

        var forwardRotationTest = new Quaternion(forwardRotation.x, -forwardRotation.z, forwardRotation.y, forwardRotation.w);

        var translationInRigSpace = forwardRotationTest * inputMove * Time.deltaTime * _speed;
        var translationInWorldSpace = originTransform.TransformDirection(translationInRigSpace);

        return translationInWorldSpace;
    }

    void FindCharacterController()
    {
        var xrOrigin = system.xrOrigin?.Origin;
        if (xrOrigin == null)
            return;

        // Save a reference to the optional CharacterController on the rig GameObject
        // that will be used to move instead of modifying the Transform directly.
        if (_characterController == null && !_attemptedGetCharacterController)
        {
            // Try on the Origin GameObject first, and then fallback to the XR Origin GameObject (if different)
            if (!xrOrigin.TryGetComponent(out _characterController) && xrOrigin != system.xrOrigin.gameObject)
                system.xrOrigin.TryGetComponent(out _characterController);

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

        if (_characterController != null && _characterController.enabled)
        {
            // Step vertical velocity from gravity
            if (_characterController.isGrounded || !_isUseGravity)
            {
                _gravityDirection = Vector3.zero;
                /// поставить, что гравитация вниз
            }
            else
            {
                /// тут ка краз заменить на кастомную гравитацию
                _gravityDirection += _gravityController.GravityDirection * Time.deltaTime;
            }

            
            motion += _gravityDirection * Time.deltaTime;

            if (CanBeginLocomotion() && BeginLocomotion())
            {
                // Note that calling Move even with Vector3.zero will have an effect by causing isGrounded to update
                _isMovingXROrigin = true;
                _characterController.Move(motion);
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

    void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
    {
        if (Application.isPlaying)
            property.DisableDirectAction();

        property = value;

        if (Application.isPlaying && isActiveAndEnabled)
            property.EnableDirectAction();
    }
}
