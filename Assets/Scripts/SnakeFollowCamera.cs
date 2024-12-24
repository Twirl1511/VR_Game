using System.Collections.Generic;
using UnityEngine;

public class SnakeFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform; // ������, �� ������� ������� ������
    [SerializeField] private List<Transform> _segments;  // ������ ��������� ������
    [SerializeField] private float _initialSegmentSpacing = 0.5f; // ��������� ���������� ����� ����������
    [SerializeField] private float _moveSpeed = 5f;      // �������� �������� ���������
    [SerializeField] private LayerMask _groundLayer;     // ���� �����������

    private void Update()
    {
        MoveSegments();
    }

    private void MoveSegments()
    {
        Vector3 previousPosition = _cameraTransform.position;
        float currentSpacing = _initialSegmentSpacing; // ��������� ���������� ��� ������� ��������

        // ���������� ������ ������ (������� �� �������)
        if (Physics.Raycast(previousPosition + Vector3.up, Vector3.down, out RaycastHit headHit, 2f, _groundLayer))
        {
            previousPosition = headHit.point;
        }

        // ���������� ��������� ��������
        for (int i = 0; i < _segments.Count; i++)
        {
            Transform segment = _segments[i];
            float distance = Vector3.Distance(segment.position, previousPosition);

            // ���������, ����� �� ���������� �������
            if (distance > currentSpacing)
            {
                Vector3 direction = (previousPosition - segment.position).normalized;
                Vector3 targetPosition = segment.position + direction * (distance - currentSpacing);

                // ��������� ����������� ��� �������� ��������
                if (Physics.Raycast(targetPosition + Vector3.up, Vector3.down, out RaycastHit hit, 2f, _groundLayer))
                {
                    segment.position = hit.point;
                    segment.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.LookRotation(direction);
                }
                else
                {
                    segment.position = targetPosition;
                }
            }

            // ��������� ������� ��� ���������� ��������
            previousPosition = segment.position;

            // ��������� ���������� ��� ���������� ��������
            currentSpacing *= 0.9f;
        }
    }
}
