using UnityEngine;

public class MoveObjectWithMouse : MonoBehaviour
{
    public Transform target;         // ������ �� ����� (����)
    public float moveSpeed = 5f;     // �������� ��������
    public float rotationSpeed = 10f; // �������� �������� �� Y

    private float _initialXRotation; // ��������� ������� �� X
    private float _initialZRotation; // ��������� ������� �� Z

    void Start()
    {
        // ��������� ����������� �������� �� X � Z
        _initialXRotation = transform.eulerAngles.x;
        _initialZRotation = transform.eulerAngles.z;
    }

    void Update()
    {
        if (target != null)
        {
            // ������ ���������� ������ � ������� ���� (������)
            Vector3 directionToTarget = target.position - transform.position;
            directionToTarget.y = 0; // ���������� ��������� �� Y ��� �������� � �������������� ���������

            // ���������� ������ � ����
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

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
        }
    }
}
