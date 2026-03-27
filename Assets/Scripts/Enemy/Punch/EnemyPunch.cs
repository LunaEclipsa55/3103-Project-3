using UnityEngine;
using UnityEngine.AI;

public class EnemyPunch : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float detectionRange = 8f;
    public float attackRange = 1.5f;
    public float moveSpeed = 3.5f;

    [Header("Attack")]
    public int damage = 10;
    public float attackCooldown = 1.5f;

    [Header("Optional")]
    public bool facePlayer = true;
    public Animator animator;

    NavMeshAgent agent;
    float attackTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange;
        }

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
        attackTimer -= Time.deltaTime;

        // stay still if player too far away
        if (d > detectionRange)
        {
            if (agent) agent.ResetPath();
            if (animator) animator.SetBool("IsRunning", false);
            return;
        }

        // chase until close enough
        if (d > attackRange)
        {
            if (agent) agent.SetDestination(player.position);
            if (animator) animator.SetBool("IsRunning", true);
        }
        else
        {
            if (agent) agent.ResetPath();
            if (animator) animator.SetBool("IsRunning", false);

            if (facePlayer)
                FacePlayer();

            if (attackTimer <= 0f)
            {
                Punch();
                attackTimer = attackCooldown;
            }
        }
    }

    void FacePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 8f);
    }

    void Punch()
    {
        if (animator) animator.SetTrigger("Punch");

        var stats = player.GetComponent<PlayerStats>();
        if (!stats) stats = player.GetComponentInParent<PlayerStats>();

        if (stats)
        {
            stats.TakeDamage(damage);
            Debug.Log($"{gameObject.name} punched player for {damage}");
        }
    }
}
