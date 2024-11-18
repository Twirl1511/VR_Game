using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectManager : MonoBehaviour
{
    [Header("Настройки")]
    public List<ObjectToCatch> objects = new List<ObjectToCatch>();
    public float activationTime = 5f; // Время, в течение которого объект активен
    public float blinkingDuration = 2f; // Длительность мигания объектов
    public float scaleMultiplier = 1.5f; // Во сколько раз увеличивается объект при мигании

    private Queue<ObjectToCatch> objectQueue = new Queue<ObjectToCatch>();
    private List<ObjectToCatch> greenObjects = new List<ObjectToCatch>();
    private List<ObjectToCatch> missedObjects = new List<ObjectToCatch>(); // Пропущенные объекты
    private bool isChainActive = false; // Флаг для активации цепочки

    private void Start()
    {
        // Выключаем все объекты на старте
        foreach (var obj in objects)
        {
            obj.gameObject.SetActive(false);
        }

        // Включаем первый объект
        if (objects.Count > 0)
        {
            var firstObject = objects[0];
            objects.RemoveAt(0); // Удаляем первый объект из списка, чтобы не перемешивать его
            firstObject.gameObject.SetActive(true);
            StartGreenBlinking(firstObject);

            // Ждем, пока игрок коснется первого объекта
            StartCoroutine(WaitForFirstTouch(firstObject));
        }
    }

    private IEnumerator WaitForFirstTouch(ObjectToCatch firstObject)
    {
        yield return new WaitUntil(() => PlayerTouched(firstObject));

        // Останавливаем мигание первого объекта
        StopBlinking(firstObject);
        firstObject.gameObject.SetActive(false);

        // Запускаем цепочку активации
        isChainActive = true;

        // Перемешиваем оставшиеся объекты
        ShuffleObjects();
        objectQueue = new Queue<ObjectToCatch>(objects);

        StartCoroutine(ActivateObjectsRoutine());
    }

    private void ShuffleObjects()
    {
        for (int i = objects.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            var temp = objects[i];
            objects[i] = objects[randomIndex];
            objects[randomIndex] = temp;
        }
    }

    private IEnumerator ActivateObjectsRoutine()
    {
        while (objectQueue.Count > 0)
        {
            ObjectToCatch currentObject = objectQueue.Dequeue();
            currentObject.gameObject.SetActive(true);

            // Зеленый объект начинает мигать при появлении
            StartGreenBlinking(currentObject);

            bool touched = false;
            StartCoroutine(TimerRoutine(currentObject, () => touched = true));

            // Ожидаем, пока объект станет неактивным или игрок коснется объекта
            yield return new WaitUntil(() => !currentObject.gameObject.activeSelf || touched);

            // Если объект не успели коснуться, добавляем его в список пропущенных
            if (!touched)
            {
                missedObjects.Add(currentObject);
            }
        }

        // Показываем пропущенные объекты
        ShowMissedObjects();
    }

    private IEnumerator TimerRoutine(ObjectToCatch obj, System.Action onTouched)
    {
        float timer = activationTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            if (PlayerTouched(obj))
            {
                // Останавливаем мигание и выключаем объект
                StopBlinking(obj);
                obj.gameObject.SetActive(false);
                onTouched?.Invoke();
                yield break;
            }

            yield return null;
        }

        // Если таймер истек, объект становится неактивным
        obj.gameObject.SetActive(false);
    }

    private bool PlayerTouched(ObjectToCatch obj)
    {
        return obj.IsTouched;
    }

    private void StartGreenBlinking(ObjectToCatch obj)
    {
        var renderer = obj.GetComponent<Renderer>();
        renderer.material.color = Color.green;

        // Зеленые объекты мигают между 30% и 100% прозрачности
        Color startColor = new Color(0, 1, 0, 0.3f);
        Color endColor = new Color(0, 1, 0, 1.0f);

        renderer.material.DOColor(endColor, blinkingDuration / 2)
            .SetLoops(-1, LoopType.Yoyo)
            .From(startColor);

        // Зеленые объекты увеличиваются/уменьшаются
        obj.transform.DOScale(Vector3.one * scaleMultiplier, blinkingDuration / 2)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopBlinking(ObjectToCatch obj)
    {
        var renderer = obj.GetComponent<Renderer>();
        renderer.material.DOKill(); // Останавливаем все анимации на объекте
        obj.transform.DOKill(); // Останавливаем анимацию масштаба
        renderer.material.color = Color.green; // Устанавливаем окончательный цвет
        obj.transform.localScale = Vector3.one; // Возвращаем размер к обычному
    }

    private void ShowMissedObjects()
    {
        foreach (var obj in missedObjects)
        {
            var renderer = obj.GetComponent<Renderer>();

            // Красим объект в красный цвет
            renderer.material.DOKill(); // Останавливаем все анимации цвета
            renderer.material.color = Color.red;

            // Анимация изменения размера
            obj.transform.DOScale(Vector3.one * scaleMultiplier, blinkingDuration / 2)
                .SetLoops(-1, LoopType.Yoyo);

            obj.gameObject.SetActive(true); // Делаем объект видимым
        }
    }
}
