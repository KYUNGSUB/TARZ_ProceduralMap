using UnityEngine;

public class PlayerFollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow")]
    public Vector3 offset = new Vector3(0f, 18f, -12f);
    public float followSmoothTime = 0.15f;

    [Header("Look")]
    public bool lookAtTarget = true;
    public Vector3 lookOffset = new Vector3(0f, 1f, 0f);

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            followSmoothTime
        );

        if (lookAtTarget)
        {
            transform.LookAt(target.position + lookOffset);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}