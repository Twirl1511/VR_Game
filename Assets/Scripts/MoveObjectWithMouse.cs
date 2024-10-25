using UnityEngine;

public class MoveObjectWithMouse : MonoBehaviour
{
    public Transform target;         // ������ �� ����� (����)
    public float moveSpeed = 5f;     // �������� ��������
    public float rotationSpeed = 10f; // �������� �������� �� Y
    public float stopDistance = 3f; // �������� �������� �� Y
    public LayerMask groundLayer;

    private float _initialXRotation; // ��������� ������� �� X
    private float _initialZRotation; // ��������� ������� �� Z
    private Rigidbody _rigidbody;

    void Start()
    {
        // ��������� ����������� �������� �� X � Z
        _initialXRotation = transform.eulerAngles.x;
        _initialZRotation = transform.eulerAngles.z;
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (target != null)
        {
            // ������ ���������� ������ � ������� ���� (������)
            Vector3 directionToTarget = target.position - transform.position;
            //directionToTarget.y = 0; // ���������� ��������� �� Y ��� �������� � �������������� ���������


            //if(Vector3.Distance(transform.position, target.position) > stopDistance)
            //{
            //    // ���������� ������ � ����
            //    _rigidbody.AddForce(directionToTarget.normalized * moveSpeed);
            //    //transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            //}
            

            // ���� ����������� �� ����� ����
            if (directionToTarget != Vector3.zero)
            {
                // ������������ ���� ��� �������� ������ �� ��� Y
                float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;

                // ������ ������� ������ �� ��� Y
                Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

                // ������ ������������ ������ ������ �� ��� Y
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // ���������� ��������� ���� ��� X � Z, ����� ��������� �� ����������� ���������
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
