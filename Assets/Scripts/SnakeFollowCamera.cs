using System.Collections.Generic;
using UnityEngine;

public class SnakeFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform _headTransform;
    [SerializeField] private GravityController _gravityController;
    [SerializeField] private List<Transform> _segments;
    [SerializeField] private float _initialSegmentSpacing = 0.5f;
    [SerializeField] private float _moveSpeed = 100f;
    [SerializeField] private float _lerpSpeed = 100f;
    [SerializeField] private float _timeAfterStopMovingToHead = 1.5f;

    private bool _moveToHead = false;
    private float _timer;

    private void Start()
    {
        _gravityController.OnGroundedAfterFall += StartMoveToHead;
    }

    private void OnDisable()
    {
        _gravityController.OnGroundedAfterFall -= StartMoveToHead;
    }

    private void Update()
    {
        if (_moveToHead)
        {
            MoveSegmentsToHead();
        }
        else
        {
            MoveSegments();
        }
    }

    private void StartMoveToHead()
    {
        _moveToHead = true;
    }

    private void MoveSegments()
    {
        Vector3 previousPosition = _headTransform.position;
        float currentSpacing = _initialSegmentSpacing;
        Quaternion previousQuaternion = _headTransform.rotation;

        for (int i = 0; i < _segments.Count; i++)
        {
            Transform segment = _segments[i];
            float distance = Vector3.Distance(segment.position, previousPosition);

            if (distance > currentSpacing)
            {
                Vector3 direction = (previousPosition - segment.position).normalized;
                Vector3 targetPosition = segment.position + direction * (distance - currentSpacing);
                segment.position = Vector3.Lerp(segment.position, targetPosition, _moveSpeed * Time.deltaTime);
                segment.rotation = Quaternion.Lerp(segment.rotation, previousQuaternion, _lerpSpeed * Time.deltaTime);
            }

            previousPosition = segment.position;
            previousQuaternion = segment.rotation;
            currentSpacing *= 0.9f;
        }
    }

    private void MoveSegmentsToHead()
    {
        Vector3 headPosition = _headTransform.position;
        bool allSegmentsAtHead = true;

        for (int i = 0; i < _segments.Count; i++)
        {
            Transform segment = _segments[i];
            segment.position = Vector3.Lerp(segment.position, headPosition, _lerpSpeed * Time.deltaTime);

            if (Vector3.Distance(segment.position, headPosition) > 0.01f)
            {
                allSegmentsAtHead = false;
            }
        }

        _timer += Time.deltaTime;

        if (allSegmentsAtHead || _timer >= _timeAfterStopMovingToHead)
        {
            _moveToHead = false;
            _timer = 0;
        }
    }
}
