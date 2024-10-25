using UnityEngine;

public class MoveObjectWithMouse : MonoBehaviour
{
    public Transform target;         // Ссылка на шарик (цель)
    public float moveSpeed = 5f;     // Скорость движения
    public float rotationSpeed = 10f; // Скорость поворота по Y
    public float stopDistance = 3f; // Скорость поворота по Y
    public LayerMask groundLayer;

    private float _initialXRotation; // Начальная ротация по X
    private float _initialZRotation; // Начальная ротация по Z
    private Rigidbody _rigidbody;

    void Start()
    {
        // Сохраняем изначальные повороты по X и Z
        _initialXRotation = transform.eulerAngles.x;
        _initialZRotation = transform.eulerAngles.z;
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (target != null)
        {
            // Плавно перемещаем объект в сторону цели (шарика)
            Vector3 directionToTarget = target.position - transform.position;
            //directionToTarget.y = 0; // Игнорируем изменение по Y для движения в горизонтальной плоскости


            //if(Vector3.Distance(transform.position, target.position) > stopDistance)
            //{
            //    // Перемещаем объект к цели
            //    _rigidbody.AddForce(directionToTarget.normalized * moveSpeed);
            //    //transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            //}
            

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

            //Move();
        }
    }

    private bool ArcCast(Vector3 center, Quaternion rotation,
                           float angle, float radius, int resolution,
                           LayerMask layer, out RaycastHit hit)
    {
        rotation *= Quaternion.Euler(-angle / 2, 0, 0);

        for (int i = 0; i < resolution; i++)
        {
            Vector3 A = center + rotation * Vector3.forward * radius;
            rotation *= Quaternion.Euler(angle / resolution, 0, 0);
            Vector3 B = center + rotation * Vector3.forward * radius;
            Vector3 AB = B - A;

            if (Physics.Raycast(A, AB, out hit, AB.magnitude * 1.001f, layer))
                return true;
        }

        hit = new RaycastHit();
        return false;
    }

    private void Move()
    {
        float arcAngle = 270;
        float arcRadius = arcDistance * Time.deltaTime;
        //int arcResolution = 6;

        if (ArcCast(transform.position, transform.rotation,
                                     arcAngle, arcRadius, arcResolution,
                                     groundLayer, out RaycastHit hit))
        {
            transform.position = hit.point;
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }
    }

    public float arcDistance = 3;
    public int arcResolution = 6;
}
