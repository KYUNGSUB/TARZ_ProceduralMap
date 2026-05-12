using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    public float moveSpeed = 2.5f;
    public float detectRange = 30f;

    private Transform player;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > detectRange)
            return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.1f)
            return;

        transform.position += dir.normalized * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(dir.normalized);
    }
}