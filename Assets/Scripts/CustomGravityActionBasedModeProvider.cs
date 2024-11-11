using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class CustomGravityActionBasedModeProvider : ContinuousMoveProviderBase
{
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
    
    [SerializeField] private CustomGravityCharacterController _customCharacterController;

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

        Input = ReadInput();

        switch (_gravityApplicationModeOverrided)
        {
            case GravityApplicationMode.Immediately:
                MoveRig(Input);
                break;
            case GravityApplicationMode.AttemptingMove:
                if (Input != Vector2.zero || _gravityDirection != Vector3.zero)
                    MoveRig(Input);
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
