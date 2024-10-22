using UnityEngine;

public class MoveObjectWithMouse : MonoBehaviour
{
    public Transform target;         // Ссылка на шарик (цель)
    public float moveSpeed = 5f;     // Скорость движения
    public float rotationSpeed = 10f; // Скорость поворота по Y

    private float _initialXRotation; // Начальная ротация по X
    private float _initialZRotation; // Начальная ротация по Z

    void Start()
    {
        // Сохраняем изначальные повороты по X и Z
        _initialXRotation = transform.eulerAngles.x;
        _initialZRotation = transform.eulerAngles.z;
    }

    void Update()
    {
        if (target != null)
        {
            // Плавно перемещаем объект в сторону цели (шарика)
            Vector3 directionToTarget = target.position - transform.position;
            directionToTarget.y = 0; // Игнорируем изменение по Y для движения в горизонтальной плоскости

            // Перемещаем объект к цели
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

            // Если направление не равно нулю
            if (directionToTarget != Vector3.zero)
            {
                // Рассчитываем угол для поворота только по оси Y
                float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;

                // Создаём ротацию только по оси Y
                Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

                // Плавно поворачиваем объект только по оси Y
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Возвращаем начальные углы для X и Z, чтобы сохранить их изначальное положение
                transform.rotation = Quaternion.Euler(_initialXRotation, transform.eulerAngles.y, _initialZRotation);
            }
        }
    }
}
