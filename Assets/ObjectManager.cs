using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectManager : MonoBehaviour
{
    [Header("���������")]
    public List<ObjectToCatch> objects = new List<ObjectToCatch>();
    public float activationTime = 5f; // �����, � ������� �������� ������ �������
    public float blinkingDuration = 2f; // ������������ ������� ��������
    public float scaleMultiplier = 1.5f; // �� ������� ��� ������������� ������ ��� �������

    private Queue<ObjectToCatch> objectQueue = new Queue<ObjectToCatch>();
    private List<ObjectToCatch> greenObjects = new List<ObjectToCatch>();
    private List<ObjectToCatch> missedObjects = new List<ObjectToCatch>(); // ����������� �������
    private bool isChainActive = false; // ���� ��� ��������� �������

    private void Start()
    {
        // ��������� ��� ������� �� ������
        foreach (var obj in objects)
        {
            obj.gameObject.SetActive(false);
        }

        // �������� ������ ������
        if (objects.Count > 0)
        {
            var firstObject = objects[0];
            objects.RemoveAt(0); // ������� ������ ������ �� ������, ����� �� ������������ ���
            firstObject.gameObject.SetActive(true);
            StartGreenBlinking(firstObject);

            // ����, ���� ����� �������� ������� �������
            StartCoroutine(WaitForFirstTouch(firstObject));
        }
    }

    private IEnumerator WaitForFirstTouch(ObjectToCatch firstObject)
    {
        yield return new WaitUntil(() => PlayerTouched(firstObject));

        // ������������� ������� ������� �������
        StopBlinking(firstObject);
        firstObject.gameObject.SetActive(false);

        // ��������� ������� ���������
        isChainActive = true;

        // ������������ ���������� �������
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

            // ������� ������ �������� ������ ��� ���������
            StartGreenBlinking(currentObject);

            bool touched = false;
            StartCoroutine(TimerRoutine(currentObject, () => touched = true));

            // �������, ���� ������ ������ ���������� ��� ����� �������� �������
            yield return new WaitUntil(() => !currentObject.gameObject.activeSelf || touched);

            // ���� ������ �� ������ ���������, ��������� ��� � ������ �����������
            if (!touched)
            {
                missedObjects.Add(currentObject);
            }
        }

        // ���������� ����������� �������
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
                // ������������� ������� � ��������� ������
                StopBlinking(obj);
                obj.gameObject.SetActive(false);
                onTouched?.Invoke();
                yield break;
            }

            yield return null;
        }

        // ���� ������ �����, ������ ���������� ����������
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

        // ������� ������� ������ ����� 30% � 100% ������������
        Color startColor = new Color(0, 1, 0, 0.3f);
        Color endColor = new Color(0, 1, 0, 1.0f);

        renderer.material.DOColor(endColor, blinkingDuration / 2)
            .SetLoops(-1, LoopType.Yoyo)
            .From(startColor);

        // ������� ������� �������������/�����������
        obj.transform.DOScale(Vector3.one * scaleMultiplier, blinkingDuration / 2)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopBlinking(ObjectToCatch obj)
    {
        var renderer = obj.GetComponent<Renderer>();
        renderer.material.DOKill(); // ������������� ��� �������� �� �������
        obj.transform.DOKill(); // ������������� �������� ��������
        renderer.material.color = Color.green; // ������������� ������������� ����
        obj.transform.localScale = Vector3.one; // ���������� ������ � ��������
    }

    private void ShowMissedObjects()
    {
        foreach (var obj in missedObjects)
        {
            var renderer = obj.GetComponent<Renderer>();

            // ������ ������ � ������� ����
            renderer.material.DOKill(); // ������������� ��� �������� �����
            renderer.material.color = Color.red;

            // �������� ��������� �������
            obj.transform.DOScale(Vector3.one * scaleMultiplier, blinkingDuration / 2)
                .SetLoops(-1, LoopType.Yoyo);

            obj.gameObject.SetActive(true); // ������ ������ �������
        }
    }
}
