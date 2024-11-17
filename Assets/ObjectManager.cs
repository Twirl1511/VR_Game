using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectManager : MonoBehaviour
{
    [Header("Настройки")]
    public List<ObjectToCatch> objects = new List<ObjectToCatch>();
    public float activationTime = 5f;
    public float blinkingDuration = 2f; 

    private Queue<ObjectToCatch> objectQueue = new Queue<ObjectToCatch>(); 
    private List<ObjectToCatch> greenObjects = new List<ObjectToCatch>();




    private void Start()
    {
        objectQueue = new Queue<ObjectToCatch>(objects);

        foreach (var obj in objects)
        {
            obj.gameObject.SetActive(false);
        }

        StartCoroutine(ActivateObjectsRoutine());
    }

    private IEnumerator ActivateObjectsRoutine()
    {
        while (objectQueue.Count > 0)
        {
            ObjectToCatch currentObject = objectQueue.Dequeue();
            currentObject.gameObject.SetActive(true);

            bool touched = false;
            StartCoroutine(TimerRoutine(currentObject, () => touched = true));

            yield return new WaitUntil(() => !currentObject.gameObject.activeSelf || touched);
        }

        StartBlinking();
    }

    private IEnumerator TimerRoutine(ObjectToCatch obj, System.Action onTouched)
    {
        float timer = activationTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            if (PlayerTouched(obj))
            {
                obj.GetComponent<Renderer>().material.color = Color.green;
                greenObjects.Add(obj);
                onTouched?.Invoke();
                yield break;
            }

            yield return null;
        }

        obj.gameObject.SetActive(false);
    }

    private bool PlayerTouched(ObjectToCatch obj)
    {
        return obj.IsTouched;
    }

    private void StartBlinking()
    {
        foreach (var obj in objects)
        {
            var renderer = obj.GetComponent<Renderer>();

            if (greenObjects.Contains(obj))
            {
                renderer.material.DOColor(new Color(0, 1, 0, 0), blinkingDuration)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                renderer.material.DOColor(new Color(1, 0, 0, 0), blinkingDuration)
                    .SetLoops(-1, LoopType.Yoyo);
            }

            obj.gameObject.SetActive(true);
        }
    }
}
