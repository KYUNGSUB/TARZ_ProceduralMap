using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float rotationSpeed = 12f;

    private CharacterController controller;
    private Camera mainCamera;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0f, v).normalized;

        if (input.sqrMagnitude < 0.01f)
            return;

        Vector3 move = input;

        controller.Move(move * moveSpeed * Time.deltaTime);

        Quaternion targetRot = Quaternion.LookRotation(move);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }
}