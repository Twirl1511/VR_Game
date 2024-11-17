using System.Collections.Generic;
using UnityEngine;

public class TransformMover
{
    private int _currentPointIndex = 0;
    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private float _elapsedTime = 0f;
    private bool _isMoving = false;
    private Transform _target;
    private AnimationCurve _curve;
    private float _duration;
    private List<Vector3> _points;
    private Collider _collider;

    public void Init(Transform target, List<Vector3> points, AnimationCurve curve, float duration, Collider collider)
    {
        if (target == null || points == null || points.Count < 2 || curve == null)
        {
            Debug.LogError("Invalid initialization parameters for TransformMover.");
            return;
        }
        _collider = collider;
        _collider.enabled = false;
        _target = target;
        _points = points;
        _curve = curve;
        _duration = duration;

        _currentPointIndex = 0;
        _elapsedTime = 0;
        _startPoint = _target.position;
        _isMoving = false;
    }

    public void Update()
    {
        if (_target == null || _points == null || _points.Count < 2)
            return;

        if (!_isMoving)
        {
            StartNextMove();
        }

        if (_isMoving)
        {
            _elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsedTime / _duration);
            float progress = _curve != null ? _curve.Evaluate(t) : t;

            _target.position = Vector3.Lerp(_startPoint, _endPoint, progress);

            if (_elapsedTime >= _duration)
            {
                _isMoving = false;
                if (_currentPointIndex >= _points.Count - 1)
                {
                    Debug.Log("Все точки пройдены.");
                    _collider.enabled = true;
                }
            }
        }
    }

    private void StartNextMove()
    {
        if (_currentPointIndex < _points.Count - 1)
        {
            _startPoint = _points[_currentPointIndex];
            _currentPointIndex++;
            _endPoint = _points[_currentPointIndex];
            _elapsedTime = 0f;
            _isMoving = true;
        }
    }
}
