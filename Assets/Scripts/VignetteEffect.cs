using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class VignetteEffect : MonoBehaviour
{
    // ���� ��� ��������� ��������
    [SerializeField] private float _vignetteIntensity = 0.5f; // ������������� ��������
    [SerializeField] private float vignetteDuration = 2f;    // ����������������� ������� � ��������

    // ���������� ��� ���������� ��������
    private float currentVignetteTime = 0f;
    private bool isVignetteActive = false;

    // ��������� ������������� � ������ ��������
    private PostProcessVolume postProcessVolume;
    private Vignette _vignette;
    



    private void Start()
    {
        // �������� ��������� PostProcessVolume � ��������� � ���� ��������
        postProcessVolume = GetComponent<PostProcessVolume>();

        if (postProcessVolume != null && postProcessVolume.profile.TryGetSettings(out _vignette))
        {
            _vignette.intensity.value = 0f; // ��������� �������� �� ���������
        }
        else
        {
            Debug.LogWarning("�� ������� ����� ������ �������� � PostProcessVolume.");
        }
    }

    private void Update()
    {
        // ���� �������� �������, ��������� ����� �������
        if (isVignetteActive)
        {
            currentVignetteTime -= Time.deltaTime;

            // ���� ����� �������, ��������� ��������
            if (currentVignetteTime <= 0f)
            {
                DisableVignette();
            }
        }
    }

    // ����� ��� ��������� ��������
    public void EnableVignette()
    {
        if (_vignette != null)
        {
            _vignette.intensity.value = _vignetteIntensity;
            currentVignetteTime = vignetteDuration;
            isVignetteActive = true;
        }
    }

    // ����� ��� ���������� ��������
    public void DisableVignette()
    {
        if (_vignette != null)
        {
            _vignette.intensity.value = 0f;
            isVignetteActive = false;
        }
    }

    // ����� ��� ��������� ������� �������
    public void SetVignetteDuration(float duration)
    {
        vignetteDuration = duration;
    }
}
