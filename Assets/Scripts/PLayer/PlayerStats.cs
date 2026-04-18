using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public int healthMax = 100;
    public int health = 100;

    [Header("Optional")]
    public bool destroyOnDeath = false;
    public GameObject deathScreen;

    void Start()
    {
        health = Mathf.Clamp(health, 0, healthMax);
    }

    // public void TakeDamage(int amount)
    // {
    //     if (amount <= 0) return;

    //     health -= amount;
    //     if (health < 0) health = 0;

    //     Debug.Log($"Player took {amount} damage. Health: {health}/{healthMax}");

    //     if (health <= 0)
    //         Die();
    // }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        health += amount;
        if (health > healthMax) health = healthMax;

        Debug.Log($"Player healed {amount}. Health: {health}/{healthMax}");
    }

    // void Die()
    // {
    //     Debug.Log("Player died.");

    //     if (deathScreen)
    //         deathScreen.SetActive(true);

    //     if (destroyOnDeath)
    //         GameObject.SetActive(false);
    // }
}
