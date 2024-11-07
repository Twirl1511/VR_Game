using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class VignetteEffect : MonoBehaviour
{
    // Поля для настройки виньетки
    [SerializeField] private float _vignetteIntensity = 0.5f; // Интенсивность виньетки
    [SerializeField] private float vignetteDuration = 2f;    // Продолжительность эффекта в секундах

    // Переменные для управления эффектом
    private float currentVignetteTime = 0f;
    private bool isVignetteActive = false;

    // Компонент постобработки и эффект виньетки
    private PostProcessVolume postProcessVolume;
    private Vignette _vignette;
    



    private void Start()
    {
        // Получаем компонент PostProcessVolume и добавляем к нему виньетку
        postProcessVolume = GetComponent<PostProcessVolume>();

        if (postProcessVolume != null && postProcessVolume.profile.TryGetSettings(out _vignette))
        {
            _vignette.intensity.value = 0f; // Отключаем виньетку по умолчанию
        }
        else
        {
            Debug.LogWarning("Не удалось найти эффект виньетки в PostProcessVolume.");
        }
    }

    private void Update()
    {
        // Если виньетка активна, уменьшаем время эффекта
        if (isVignetteActive)
        {
            currentVignetteTime -= Time.deltaTime;

            // Если время истекло, выключаем виньетку
            if (currentVignetteTime <= 0f)
            {
                DisableVignette();
            }
        }
    }

    // Метод для включения виньетки
    public void EnableVignette()
    {
        if (_vignette != null)
        {
            _vignette.intensity.value = _vignetteIntensity;
            currentVignetteTime = vignetteDuration;
            isVignetteActive = true;
        }
    }

    // Метод для отключения виньетки
    public void DisableVignette()
    {
        if (_vignette != null)
        {
            _vignette.intensity.value = 0f;
            isVignetteActive = false;
        }
    }

    // Метод для установки времени эффекта
    public void SetVignetteDuration(float duration)
    {
        vignetteDuration = duration;
    }
}
