using UnityEngine;

public class SurfaceTransition : MonoBehaviour
{
    public float transitionSpeed = 5f; // Скорость перемещения
    public float arcHeight = 0.5f; // Высота дуги

    private Vector3 currentTarget; // Текущая цель движения
    private Vector3 controlPoint; // Контрольная точка для дуги
    private bool isTransitioning = false;

    public void StartTransition(Vector3 initialTarget)
    {
        currentTarget = initialTarget;
        isTransitioning = true;
        UpdateControlPoint();
    }

    private void Update()
    {
        if (!isTransitioning) return;

        // Обработка ввода игрока
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (input.magnitude > 0.1f)
        {
            // Обновляем направление движения на основе ввода
            Vector3 strafeDirection = transform.right * input.x + transform.forward * input.y;
            currentTarget += strafeDirection * Time.deltaTime * transitionSpeed;

            UpdateControlPoint();
        }

        // Движение по дуге
        Vector3 newPosition = CalculateArcPosition(transform.position, currentTarget, controlPoint, Time.deltaTime * transitionSpeed);
        transform.position = newPosition;

        // Поворот игрока в сторону движения
        Vector3 direction = (currentTarget - transform.position).normalized;
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);
        }

        // Проверка завершения перехода
        if (Vector3.Distance(transform.position, currentTarget) < 0.1f)
        {
            isTransitioning = false;
        }
    }

    private void UpdateControlPoint()
    {
        // Контрольная точка находится между текущей позицией и целью, приподнятая вверх
        controlPoint = (transform.position + currentTarget) / 2 + Vector3.up * arcHeight;
    }

    private Vector3 CalculateArcPosition(Vector3 start, Vector3 end, Vector3 control, float t)
    {
        // Квадратичная Bézier кривая
        return Mathf.Pow(1 - t, 2) * start +
               2 * (1 - t) * t * control +
               Mathf.Pow(t, 2) * end;
    }
}
