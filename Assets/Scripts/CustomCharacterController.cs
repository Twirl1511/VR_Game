using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomCharacterController : MonoBehaviour
{
    [SerializeField]
    private LayerMask _terrainLayerMask;
    public float moveSpeed = 5f;               // �������� ��������
    public float gravity = -9.81f;             // ����������
    public Vector3 surfaceNormal = Vector3.up; // ������� ������� ����������� (�� ��������� �����)

    private Rigidbody rb;
    private Vector3 velocity;                  // ������ ������� ��������
    public bool isGrounded;                   // ���������� ��� �������� ������� � ������������

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;                 // ��������� ���������� ���������� Rigidbody
        rb.isKinematic = false;                // ��������� Rigidbody ��������� ��������������
    }

    // ����� ��� ��������� ������� �����������
    public void SetSurfaceNormal(Vector3 newNormal)
    {
        surfaceNormal = newNormal.normalized;
    }

    // ����� �����������, ����������� ������� ����������� � Rigidbody
    public void Move(Vector2 input)
    {
        // ���������� ������ �������� � ��������� ���� ���������
        Vector3 moveDirection = (transform.right * input.x + transform.forward * input.y).normalized;

        // ���������� ����������� �� ������� �����������
        Vector3 relativeMoveDirection = Vector3.ProjectOnPlane(moveDirection, surfaceNormal).normalized;

        // ������������ ��������� �������� ��������
        Vector3 moveVelocity = relativeMoveDirection * moveSpeed;

        // ��������� ����������, ���� �������� �� �� �����������
        if (!isGrounded)
        {
            velocity += surfaceNormal * gravity * Time.deltaTime;
        }
        else
        {
            velocity = Vector3.zero;  // ����� ������������ �������� ��� �������
        }

        // ��������� ��������� ����������� ����� Rigidbody
        rb.MovePosition(rb.position + (moveVelocity + velocity) * Time.deltaTime);
    }

    void Update()
    {
        //// ������ ����� � ���������� (�������� �� ���� �������������)
        //Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        //Move(input);

        // ���������� ������� �����������
        UpdateSurfaceNormal();
    }

    // �������� ������� ����������� � ���������� �������
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
