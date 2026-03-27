using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 10;
    public float life = 3f;

    void Start()
    {
        Destroy(gameObject, life);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var stats = other.GetComponent<PlayerStats>();
            if (!stats) stats = other.GetComponentInParent<PlayerStats>();
            if (stats) stats.TakeDamage(damage);
        }

        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
