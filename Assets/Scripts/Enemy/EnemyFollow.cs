using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 8f;
    public float stopDistance = 2f;
    public bool facePlayer = true;

    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        if (agent)
            agent.stoppingDistance = stopDistance;
    }

    void Update()
    {
        if (!player || !agent) return;

        float d = Vector3.Distance(transform.position, player.position);

        if (d > detectionRange)
        {
            agent.ResetPath();
            return;
        }

        if (d > stopDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.ResetPath();
            if (facePlayer)
            {
                Vector3 dir = (player.position - transform.position).normalized;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion lookRot = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 8f);
                }
            }
        }
    }
}