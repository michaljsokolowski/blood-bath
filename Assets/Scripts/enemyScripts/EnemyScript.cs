// EnemyScript.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;
    public float shootInterval = 2f;
    public float bulletSpeed = 10f;
    public float bulletLifetime = 5f;
    public float fireRange = 10f;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public LayerMask obstacleMask;

    private Transform player;
    private float nextShootTime;

    public SpawningBlood blood;
    public orbSpawn orbs;
    public FloatingHealthBar healthBar;
    private bool isDead = false;

    [Header("Bleed Status Effect")]
    [Tooltip("Damage dealt per bleed tick.")]
    public int bleedDamagePerTick = 3;

    [Tooltip("How many seconds between each bleed tick.")]
    public float bleedTickInterval = 0.5f;

    [Tooltip("Total duration of the bleed effect in seconds.")]
    public float bleedDuration = 3f;

    private Coroutine activeBleedCoroutine;   // tracked so we can refresh it

    void Start()
    {
        currentHealth = maxHealth;
        healthBar     = GetComponentInChildren<FloatingHealthBar>();
        player        = GameObject.FindGameObjectWithTag("Player").transform;
        blood         = FindObjectOfType<SpawningBlood>();
        orbs          = FindObjectOfType<orbSpawn>();
        healthBar.DoHealthBar(maxHealth, maxHealth);

        GameEvents.OnComboExecuted += HandleComboExecuted;
    }

    void OnDestroy()
    {
        GameEvents.OnComboExecuted -= HandleComboExecuted;
    }

    private void HandleComboExecuted(ComboSystem.DamageType damageType,
                                     int totalDamage,
                                     ComboSystem.StatusEffect statusEffect)
    {
        if (statusEffect != ComboSystem.StatusEffect.Bleed)
            return;
        if (activeBleedCoroutine != null)
            StopCoroutine(activeBleedCoroutine);

        activeBleedCoroutine = StartCoroutine(BleedRoutine());
    }

    private IEnumerator BleedRoutine()
    {
        float elapsed = 0f;

        while (elapsed < bleedDuration)
        {
            yield return new WaitForSeconds(bleedTickInterval);
            elapsed += bleedTickInterval;

            if (isDead) yield break;          // enemy died mid-bleed

            TakeDamage(bleedDamagePerTick);
            Debug.Log($"{gameObject.name} is bleeding - took {bleedDamagePerTick} bleed damage.");
        }

        activeBleedCoroutine = null;
    }

    void Update()
    {
        if (player != null && IsPlayerInRange() && CanSeePlayer()) {
            if (Time.time >= nextShootTime) {
                Shoot();
                nextShootTime = Time.time + shootInterval;
            }
        }
    }

    bool IsPlayerInRange()
    {
        Vector3 enemyPosition  = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 playerPosition = new Vector3(player.position.x,    0, player.position.z);
        return Vector3.Distance(enemyPosition, playerPosition) <= fireRange;
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - firePoint.position).normalized;
        RaycastHit hit;

        if (Physics.Raycast(firePoint.position, directionToPlayer, out hit, fireRange))
            if (hit.collider.gameObject.tag == "Obstacle")
                return false;

        return true;
    }

    void Shoot()
    {
        if (player == null) return;

        Vector3 direction = (player.position - firePoint.position).normalized;
        direction.y = 0;

        GameObject bullet         = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletScript bulletScript = bullet.GetComponent<BulletScript>();
        if (bulletScript != null)
            bulletScript.SetAttacker(this.transform);

        bullet.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;
        Destroy(bullet, bulletLifetime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fireRange);
    }

    public void EnemyReceiveHit(int damage) => TakeDamage(damage);

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.DoHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Vector3 enemyPosition = new Vector3(transform.position.x, 0, transform.position.z);
        if (blood != null && orbs != null) {
            blood.SpawnBloodAt(enemyPosition);
            orbs.SpawnOrbAt(enemyPosition);
        }

        Debug.Log("enemy died!");
        Destroy(gameObject);
    }
}