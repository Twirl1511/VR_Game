using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomCharacterController : MonoBehaviour
{
    public float moveSpeed = 5f;               // �������� ��������
    public float gravity = -9.81f;             // ����������
    public Vector3 surfaceNormal = Vector3.up; // ������� ������� ����������� (�� ��������� �����)
    public Transform vrCamera;                 // ������ �� ������ VR ��� ����������� �������
    public GravityController gravityController;

    private Rigidbody rb;
    private Vector3 velocity;
    public bool isGrounded = true;
    public float groundDistance = 2f;

    private Vector3 SurfaceNormal => gravityController.NormalDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
    }

    // ����� ��� ��������� ������� �����������
    public void SetSurfaceNormal(Vector3 newNormal)
    {
        surfaceNormal = newNormal.normalized;
    }

    // ����� ����������� � ������ ����������� ������� � VR-�����
    public void Move(Vector2 input)
    {
        if (vrCamera == null)
        {
            Debug.LogWarning("VR Camera not assigned.");
            return;
        }

        // ���������� ������ ����������� �������� � ������ ���������� ������
        Vector3 cameraForward = Vector3.ProjectOnPlane(vrCamera.forward, SurfaceNormal).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(vrCamera.right, SurfaceNormal).normalized;

        Vector3 moveDirection = (cameraRight * input.x + cameraForward * input.y).normalized;

        // ���������� ����������� �������� �� ������� �����������
        Vector3 relativeMoveDirection = Vector3.ProjectOnPlane(moveDirection, SurfaceNormal).normalized;

        // ������������ ��������� �������� ��������
        Vector3 moveVelocity = relativeMoveDirection * moveSpeed;

        //��������� ����������, ���� �������� �� �� �����������
        if (!isGrounded)
        {
            velocity += gravityController.GravityDirection * Time.deltaTime;
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
        //// ������ ����� � ����������� ��� ����������
        //Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        //Move(input);

        // ���������� ������� �����������
        UpdateSurfaceNormal();
    }

    // �������� ������� ����������� � ���������� �������
    private void UpdateSurfaceNormal()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, groundDistance))
        {
            isGrounded = true;
            //SetSurfaceNormal(hit.normal);
        }
        else
        {
            isGrounded = false;
        }
    }
}
