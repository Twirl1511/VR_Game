using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomVignetteController : MonoBehaviour
{
    const string k_DefaultShader = "VR/TunnelingVignette";
    public readonly int _apertureSize = Shader.PropertyToID("_ApertureSize");
    public readonly int _featheringEffect = Shader.PropertyToID("_FeatheringEffect");

    [SerializeField]
    private GravityController _gravityController;

    [SerializeField]
    private VignetteParameters _parameters = new VignetteParameters();

    [SerializeField]
    private VignetteParameters _defaultParameters = new VignetteParameters();

    private MeshRenderer m_MeshRender;
    private MeshFilter m_MeshFilter;
    private Material m_SharedMaterial;
    private MaterialPropertyBlock m_VignettePropertyBlock;
    private VignetteParameters _currentParameters = new VignetteParameters();
    private float _elapsedTime;
    private enum Status
    {
        None,
        Appear,
        Disappear
    }
    private Status _currentStatus;
    private float _timer;

    private void Start()
    {
        SetUpMaterial();
        UpdateTunnelingVignette(_defaultParameters);
        _currentParameters.apertureSize = 1;
        _currentParameters.featheringEffect = 1;
        _gravityController.OnChangeFloor += ActivateVignette;
    }

    private void OnDestroy()
    {
        _gravityController.OnChangeFloor -= ActivateVignette;
    }

    private void Update()
    {
        if (_currentStatus == Status.Appear)
        {
            if (Mathf.Approximately(_currentParameters.apertureSize, _parameters.apertureSize))
            {
                _elapsedTime = 0;
                _timer += Time.deltaTime;
                if(_timer >= _parameters.easeOutDelayTime)
                {
                    _currentStatus = Status.Disappear;
                    _timer = 0;
                }

                return;
            }


            _currentParameters.apertureSize = LerpValue(_currentParameters.apertureSize, _parameters.apertureSize, _parameters.easeInTime);
            _currentParameters.featheringEffect = LerpValue(_currentParameters.featheringEffect, _parameters.featheringEffect, _parameters.easeInTime);
            UpdateTunnelingVignette(_currentParameters);

        }
        else if(_currentStatus == Status.Disappear)
        {
            _currentParameters.apertureSize = LerpValue(_currentParameters.apertureSize, _defaultParameters.apertureSize, _parameters.easeOutTime);
            _currentParameters.featheringEffect = LerpValue(_currentParameters.featheringEffect, _defaultParameters.featheringEffect, _parameters.easeOutTime);
            UpdateTunnelingVignette(_currentParameters);

            if (Mathf.Approximately(_currentParameters.apertureSize, _defaultParameters.apertureSize))
            {
                _elapsedTime = 0;
                _currentStatus = Status.None;
            }
        }
    }

    private void UpdateTunnelingVignette(VignetteParameters parameters)
    {
        m_MeshRender.GetPropertyBlock(m_VignettePropertyBlock);
        m_VignettePropertyBlock.SetFloat(_apertureSize, parameters.apertureSize);
        m_VignettePropertyBlock.SetFloat(_featheringEffect, parameters.featheringEffect);
        m_MeshRender.SetPropertyBlock(m_VignettePropertyBlock);
    }

    private void SetUpMaterial()
    {
        if (m_MeshRender == null)
            m_MeshRender = GetComponent<MeshRenderer>();
        if (m_MeshRender == null)
            m_MeshRender = gameObject.AddComponent<MeshRenderer>();

        if (m_VignettePropertyBlock == null)
            m_VignettePropertyBlock = new MaterialPropertyBlock();

        if (m_MeshFilter == null)
            m_MeshFilter = GetComponent<MeshFilter>();
        if (m_MeshFilter == null)
            m_MeshFilter = gameObject.AddComponent<MeshFilter>();

        if (m_MeshFilter.sharedMesh == null)
        {
            Debug.LogWarning("The default mesh for the TunnelingVignetteController is not set. " +
                "Make sure to import it from the Tunneling Vignette Sample of XR Interaction Toolkit.", this);
        }

        if (m_MeshRender.sharedMaterial == null)
        {
            var defaultShader = Shader.Find(k_DefaultShader);
            if (defaultShader == null)
            {
                Debug.LogWarning("The default material for the TunnelingVignetteController is not set, and the default Shader: " + k_DefaultShader
                    + " cannot be found. Make sure they are imported from the Tunneling Vignette Sample of XR Interaction Toolkit.", this);
            }

            Debug.LogWarning("The default material for the TunnelingVignetteController is not set. " +
                "Make sure it is imported from the Tunneling Vignette Sample of XR Interaction Toolkit. + " +
                "Try creating a material using the default Shader: " + k_DefaultShader, this);

            m_SharedMaterial = new Material(defaultShader)
            {
                name = "DefaultTunnelingVignette",
            };
            m_MeshRender.sharedMaterial = m_SharedMaterial;
        }
        else
        {
            m_SharedMaterial = m_MeshRender.sharedMaterial;
        }
    }

    private float LerpValue(float start, float end, float time)
    {
        // Увеличиваем время
        _elapsedTime += Time.deltaTime;

        // Вычисляем процент завершения (от 0 до 1)
        float t = Mathf.Clamp01(_elapsedTime / time);

        // Линейная интерполяция между start и end
        return Mathf.Lerp(start, end, t);
    }

    private void ActivateVignette()
    {
        if (_currentStatus == Status.Appear)
            return;

        SetAppearStatus();
    }





    [ContextMenu("AppearStatus")]
    public void SetAppearStatus()
    {
        _currentStatus = Status.Appear;
    }

    [ContextMenu("DissapearStatus")]
    public void SetDissapearStatus()
    {
        _currentStatus = Status.Disappear;
    }

    [ContextMenu("SetNoneStatus")]
    public void SetNoneStatus()
    {
        _currentStatus = Status.None;
    }
}
