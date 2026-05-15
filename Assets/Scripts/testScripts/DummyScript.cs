using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyScript : MonoBehaviour
{
    public int maxHealth = 1000;
    private int currentHealth;

    public FloatingHealthBar healthBar;

    [Header("Bleed Status Effect")]
    [Tooltip("Damage dealt per bleed tick.")]
    public int bleedDamagePerTick = 3;

    [Tooltip("How many seconds between each bleed tick.")]
    public float bleedTickInterval = 0.5f;

    [Tooltip("Total duration of the bleed effect in seconds.")]
    public float bleedDuration = 3f;

    private Coroutine activeBleedCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<FloatingHealthBar>();
        healthBar.DoHealthBar(currentHealth, maxHealth);

        GameEvents.OnComboExecuted += HandleComboExecuted;
        GameEvents.OnAbilityUsed += HandleAbilityUsed;  
    }

    void OnDestroy()
    {
        GameEvents.OnComboExecuted -= HandleComboExecuted;
        GameEvents.OnAbilityUsed -= HandleAbilityUsed;
    }

    private void HandleComboExecuted(ComboSystem.DamageType damageType,
                                     int totalDamage,
                                     ComboSystem.StatusEffect statusEffect, GameObject target)
    {
        if (target != this.gameObject) return;

        if (statusEffect == ComboSystem.StatusEffect.Bleed)
        {
            if (activeBleedCoroutine != null)
                StopCoroutine(activeBleedCoroutine);

            activeBleedCoroutine = StartCoroutine(BleedRoutine());
        }
    }

    private void HandleAbilityUsed(int totalDamage, GameObject target)
    {
        if (target != this.gameObject) return;

        EnemyTakeDamage(totalDamage);
    }

    private IEnumerator BleedRoutine()
    {
        float elapsed = 0f;

        while (elapsed < bleedDuration)
        {
            yield return new WaitForSeconds(bleedTickInterval);
            elapsed += bleedTickInterval;

            EnemyTakeDamage(bleedDamagePerTick);
            Debug.Log($"{gameObject.name} is bleeding - took {bleedDamagePerTick} bleed damage.");
        }

        activeBleedCoroutine = null;
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

