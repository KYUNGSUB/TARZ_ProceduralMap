using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 12f;

    [Header("Gravity")]
    public float gravity = -20f;
    public float groundedGravity = -2f;

    [Header("Fall Safety")]
    public float minY = -20f;
    public Vector3 respawnPosition;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    private Vector3 lastSafePosition;
    public float safePositionUpdateInterval = 0.5f;
    private float safeTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        lastSafePosition = transform.position;
    }

    private void Update()
    {
        Move();

        if (transform.position.y < minY)
        {
            CharacterController controller = GetComponent<CharacterController>();
            controller.enabled = false;
            transform.position = lastSafePosition;
            controller.enabled = true;
        }

        safeTimer += Time.deltaTime;

        if (safeTimer >= safePositionUpdateInterval && controller.isGrounded)
        {
            lastSafePosition = transform.position;
            safeTimer = 0f;
        }
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0f, v).normalized;

        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 move = input * moveSpeed;

            Quaternion targetRotation = Quaternion.LookRotation(input);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            controller.Move(move * Time.deltaTime);
        }

        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = groundedGravity;
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        controller.Move(verticalVelocity * Time.deltaTime);
    }
}