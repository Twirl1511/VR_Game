using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomCharacterController : MonoBehaviour
{
    [SerializeField]
    private LayerMask _terrainLayerMask;
    public float moveSpeed = 5f;               // Скорость движения
    public float gravity = -9.81f;             // Гравитация
    public Vector3 surfaceNormal = Vector3.up; // Нормаль текущей поверхности (по умолчанию вверх)

    private Rigidbody rb;
    private Vector3 velocity;                  // Вектор текущей скорости
    public bool isGrounded;                   // Переменная для проверки касания с поверхностью

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;                 // Отключаем встроенную гравитацию Rigidbody
        rb.isKinematic = false;                // Позволяем Rigidbody управлять столкновениями
    }

    // Метод для установки нормали поверхности
    public void SetSurfaceNormal(Vector3 newNormal)
    {
        surfaceNormal = newNormal.normalized;
    }

    // Метод перемещения, учитывающий нормаль поверхности и Rigidbody
    public void Move(Vector2 input)
    {
        // Определяем вектор движения в локальных осях персонажа
        Vector3 moveDirection = (transform.right * input.x + transform.forward * input.y).normalized;

        // Проецируем направление на текущую поверхность
        Vector3 relativeMoveDirection = Vector3.ProjectOnPlane(moveDirection, surfaceNormal).normalized;

        // Рассчитываем финальную скорость движения
        Vector3 moveVelocity = relativeMoveDirection * moveSpeed;

        // Применяем гравитацию, если персонаж не на поверхности
        if (!isGrounded)
        {
            velocity += surfaceNormal * gravity * Time.deltaTime;
        }
        else
        {
            velocity = Vector3.zero;  // Сброс вертикальной скорости при касании
        }

        // Применяем финальное перемещение через Rigidbody
        rb.MovePosition(rb.position + (moveVelocity + velocity) * Time.deltaTime);
    }

    void Update()
    {
        //// Пример ввода с клавиатуры (замените по мере необходимости)
        //Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        //Move(input);

        // Обновление нормали поверхности
        UpdateSurfaceNormal();
    }

    // Проверка касания поверхности и обновление нормали
    private void UpdateSurfaceNormal()
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(transform.position, -transform.up, out hitInfo, 1f))
        {
            isGrounded = true;
            SetSurfaceNormal(hitInfo.normal);
        }
        else
        {
            isGrounded = false;
        }
    }
}
