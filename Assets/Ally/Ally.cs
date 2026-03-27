using UnityEngine;
using UnityEngine.AI;

public class Ally : MonoBehaviour
{
    public Transform player;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float fireRate = 1f;
    public float detectionRange = 5f;

    [Header("Follow")]
    public float followRange = 10f;
    public float stopDistance = 2f;

    NavMeshAgent agent;
    float cd;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (agent)
            agent.stoppingDistance = stopDistance;
    }

    void Update()
    {
        if (!player) return;

        float dr = Vector3.Distance(transform.position, player.position);
        cd -= Time.deltaTime;

        if (dr > followRange)
        {
            if (agent) agent.ResetPath();
        }
        else if (dr > stopDistance)
        {
            if (agent) agent.SetDestination(player.position);
        }
        else
        {
            if (agent) agent.ResetPath();
        }

        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies)
        {
            if (!e) continue;
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < detectionRange && cd <= 0f)
            {
                Face(e.transform.position);
                Fire();
                break;
            }
        }
    }

    void Face(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    void Fire()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = firePoint.forward * bulletSpeed;

        cd = 1f / fireRate;
        Destroy(bullet, 3f);
    }
}
