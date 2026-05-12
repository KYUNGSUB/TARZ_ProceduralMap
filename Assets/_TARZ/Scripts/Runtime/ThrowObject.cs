using UnityEngine;

public class ThrowObject : MonoBehaviour
{
    public float damage = 10f;
    public float throwForce = 15f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = 1f;
    }

    public void Throw(Vector3 direction)
    {
        rb.AddForce(direction.normalized * throwForce, ForceMode.Impulse);
    }
}