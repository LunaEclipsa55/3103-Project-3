using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxEnemyHealth = 100;
    public int currentEnemyHealth = 100;
    public bool destroyOnDeath = true;

    void Start()
    {
        currentEnemyHealth = Mathf.Clamp(currentEnemyHealth, 0, maxEnemyHealth);
        if (currentEnemyHealth == 0) currentEnemyHealth = maxEnemyHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        currentEnemyHealth -= amount;
        if (currentEnemyHealth < 0) currentEnemyHealth = 0;

        Debug.Log($"{gameObject.name} took {amount} damage. HP: {currentEnemyHealth}/{maxEnemyHealth}");

        if (currentEnemyHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        if (destroyOnDeath)
            Destroy(gameObject);
    }
}
