using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomCharacterController : MonoBehaviour
{
    public float moveSpeed = 5f;               // —корость движени€
    public float gravity = -9.81f;             // √равитаци€
    public Vector3 surfaceNormal = Vector3.up; // Ќормаль текущей поверхности (по умолчанию вверх)
    public Transform vrCamera;                 // —сылка на камеру VR дл€ направлени€ взгл€да
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

    // ћетод дл€ установки нормали поверхности
    public void SetSurfaceNormal(Vector3 newNormal)
    {
        surfaceNormal = newNormal.normalized;
    }

    // ћетод перемещени€ с учетом направлени€ взгл€да в VR-шлеме
    public void Move(Vector2 input)
    {
        if (vrCamera == null)
        {
            Debug.LogWarning("VR Camera not assigned.");
            return;
        }

        // ќпредел€ем вектор направлени€ движени€ с учетом ориентации камеры
        Vector3 cameraForward = Vector3.ProjectOnPlane(vrCamera.forward, SurfaceNormal).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(vrCamera.right, SurfaceNormal).normalized;

        Vector3 moveDirection = (cameraRight * input.x + cameraForward * input.y).normalized;

        // ѕроецируем направление движени€ на текущую поверхность
        Vector3 relativeMoveDirection = Vector3.ProjectOnPlane(moveDirection, SurfaceNormal).normalized;

        // –ассчитываем финальную скорость движени€
        Vector3 moveVelocity = relativeMoveDirection * moveSpeed;

        //ѕримен€ем гравитацию, если персонаж не на поверхности
        if (!isGrounded)
        {
            velocity += gravityController.GravityDirection * Time.deltaTime;
        }
        else
        {
            velocity = Vector3.zero;  // —брос вертикальной скорости при касании
        }

        // ѕримен€ем финальное перемещение через Rigidbody
        rb.MovePosition(rb.position + (moveVelocity + velocity) * Time.deltaTime);
    }

    void Update()
    {
        //// ѕример ввода с контроллера или клавиатуры
        //Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        //Move(input);

        // ќбновление нормали поверхности
        UpdateSurfaceNormal();
    }

    // ѕроверка касани€ поверхности и обновление нормали
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
