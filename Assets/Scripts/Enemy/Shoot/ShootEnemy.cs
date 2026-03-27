using UnityEngine;
using UnityEngine.AI;

public class ShootEnemy : MonoBehaviour
{
    public Transform player;
    public GameObject EnemyBullet;
    public Transform firePoint;
    public float bulletSpeed = 15f;
    public float shootcool = 2f;

    [Header("Behavior")]
    public float detectionRange = 10f;
    public float attackRange = 5f;
    public bool chasePlayer = true;

    NavMeshAgent agent;
    float shootTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void Update()
    {
        if (!player) return;

        float d = Vector3.Distance(transform.position, player.position);
        shootTimer -= Time.deltaTime;

        if (d > detectionRange)
        {
            if (agent) agent.ResetPath();
            return;
        }

        if (chasePlayer && d > attackRange)
        {
            if (agent) agent.SetDestination(player.position);
        }
        else
        {
            if (agent) agent.ResetPath();
        }

        FacePlayer();

        if (d <= attackRange && shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootcool;
        }
    }

    void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    void Shoot()
    {
        if (!EnemyBullet || !firePoint) return;

        GameObject bullet = Instantiate(EnemyBullet, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb) rb.linearVelocity = firePoint.forward * bulletSpeed;

        Destroy(bullet, 2.5f);
    }
}
