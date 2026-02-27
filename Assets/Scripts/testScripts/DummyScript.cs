using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyScript : MonoBehaviour
{
    public int maxHealth = 1000;
    private int currentHealth;

    public FloatingHealthBar healthBar;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<FloatingHealthBar>();
        healthBar.DoHealthBar(currentHealth, maxHealth);
    }

    public void EnemyReceiveHit(int damage)
    {
        EnemyTakeDamage(damage);
        SpawnsDamagePopups.Instance.DamageDone(damage, transform.position, false);
    }

    public void EnemyTakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("dummy took " + damage + " damage. Current health: " + currentHealth);
        healthBar.DoHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
            healthBar.DoHealthBar(currentHealth, maxHealth);
            Debug.Log("Dummy health reset to " + maxHealth);
        }
    }
}

