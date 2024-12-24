using System.Collections.Generic;
using UnityEngine;

public class SnakeFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform; // Камера, за которой следует змейка
    [SerializeField] private List<Transform> _segments;  // Список сегментов змейки
    [SerializeField] private float _initialSegmentSpacing = 0.5f; // Начальное расстояние между сегментами
    [SerializeField] private float _moveSpeed = 5f;      // Скорость движения сегментов
    [SerializeField] private LayerMask _groundLayer;     // Слой поверхности

    private void Update()
    {
        MoveSegments();
    }

    private void MoveSegments()
    {
        Vector3 previousPosition = _cameraTransform.position;
        float currentSpacing = _initialSegmentSpacing; // Начальное расстояние для первого сегмента

        // Перемещаем голову змейки (следует за камерой)
        if (Physics.Raycast(previousPosition + Vector3.up, Vector3.down, out RaycastHit headHit, 2f, _groundLayer))
        {
            previousPosition = headHit.point;
        }

        // Перемещаем остальные сегменты
        for (int i = 0; i < _segments.Count; i++)
        {
            Transform segment = _segments[i];
            float distance = Vector3.Distance(segment.position, previousPosition);

            // Проверяем, нужно ли перемещать сегмент
            if (distance > currentSpacing)
            {
                Vector3 direction = (previousPosition - segment.position).normalized;
                Vector3 targetPosition = segment.position + direction * (distance - currentSpacing);

                // Проверяем поверхность для текущего сегмента
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

            // Обновляем позицию для следующего сегмента
            previousPosition = segment.position;

            // Уменьшаем расстояние для следующего сегмента
            currentSpacing *= 0.9f;
        }
    }
}
