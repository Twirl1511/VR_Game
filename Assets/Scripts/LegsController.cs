using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegsController : MonoBehaviour
{
    [SerializeField] private Leg[] _legs;
    [SerializeField] private float _distance = 0.5f;
    [SerializeField] private float _speed = 0.5f;

    [SerializeField] private float _startTime = 0.5f;
    [SerializeField] private float _startValue = 200f;
    [SerializeField] private float _endTime = 2f;
    [SerializeField] private float _endValue = 15f;

    [SerializeField] private float _offsetDistance = 2f;
    [SerializeField] private float _moveSquareDistanceDistance = 4f;

    private Vector3 _previousPosition;
    private bool _isLegMoving;

    private void Start()
    {
        _previousPosition = transform.position;

        foreach (var leg in _legs)
            leg.SetNewPosition(leg.currentPosition.position);
    }

    private void LateUpdate()
    {
        UpdateLegPositions();
    }

    void Update()
    {
        UpdateDirectionForFutereSteps();
    }

    private void UpdateLegPositions()
    {
        for (int i = 0; i < _legs.Length; i++)
        {
            _legs[i].SetTime(Mathf.Clamp(_legs[i].Time + Time.deltaTime, _startTime, _endTime));

            if (_legs[i].CanMove && _isLegMoving == false)
            {
                if (Vector3.Distance(_legs[i].currentPosition.position, _legs[i].predictPosition.position) > _distance)
                {
                    _isLegMoving = true;
                    _legs[i].SetNewPosition(_legs[i].predictPosition.position);

                    _legs[i].SetSpeed(GetSpeed(_legs[i].Time));
                    _legs[i].SetTime(0);
                }
            }

            if (_legs[i].CanMove)
            {
                _legs[i].currentPosition.position = Vector3.Lerp(
                _legs[i].currentPosition.position,
                _legs[i].NewPosition,
                Time.deltaTime * _legs[i].Speed);


                if (Vector3.Distance(_legs[i].currentPosition.position, _legs[i].NewPosition) <= 0.1f)
                {
                    StopAllLegsExcept(i + 1);
                    _isLegMoving = false;
                }
            }
        }
    }

    private void StopAllLegsExcept(int nextLegIndex)
    {
        if (nextLegIndex >= _legs.Length)
            nextLegIndex = 0;

        for (int i = 0; i < _legs.Length; i++)
            _legs[i].SetCanMove(i == nextLegIndex);
    }

    private void UpdateDirectionForFutereSteps()
    {
        float distance = (transform.position - _previousPosition).sqrMagnitude;

        if (distance > _moveSquareDistanceDistance)
        {
            foreach (var leg in _legs)
                SetNewPositionForPredictLegStep(leg);

            _previousPosition = transform.position;
        }
    }

    private void SetNewPositionForPredictLegStep(Leg leg)
    {
        Vector3 direction = (transform.position - _previousPosition).normalized;
        Vector3 targetPosition = leg.center.position + direction * _offsetDistance;
        Vector3 localTargetPosition = transform.InverseTransformPoint(targetPosition);
        leg.predictPosition.localPosition = localTargetPosition;
    }

    private float GetSpeed(float time)
    {
        float speed = _startValue + ((_endValue - _startValue) / (_endTime - _startTime)) * (time - _startTime);
        return speed;
    }
}
