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

    [Header("Stagger Status Effect")]
    [SerializeField] private float staggerDuration = 1.5f;
    private Coroutine activeStaggerCoroutine;

    [Header("Pull Status Effect")]
    [SerializeField] private float pullDuration = 1f;
    [SerializeField] private float pullSpeed = 10f;
    private Coroutine activePullCoroutine;

    private bool isStaggered = false;

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
        if (statusEffect == ComboSystem.StatusEffect.Bleed)
        {
            if (activeBleedCoroutine != null)
                StopCoroutine(activeBleedCoroutine);
            activeBleedCoroutine = StartCoroutine(BleedRoutine());
        }
        else if (statusEffect == ComboSystem.StatusEffect.Stagger)
        {
            if (activeStaggerCoroutine != null)
                StopCoroutine(activeStaggerCoroutine);
            activeStaggerCoroutine = StartCoroutine(StaggerRoutine());
        }
        else if (statusEffect == ComboSystem.StatusEffect.Pull)
        {
            if (activePullCoroutine != null)
                StopCoroutine(activePullCoroutine);
            activePullCoroutine = StartCoroutine(PullRoutine());
        }
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
    private IEnumerator StaggerRoutine()
    {
        isStaggered = true;
        float elapsed = 0f;

        while (elapsed < staggerDuration)
        {
            yield return null;
            elapsed += Time.deltaTime;

            if (isDead) yield break;
        }

        isStaggered = false;
        activeStaggerCoroutine = null;
    }

    private IEnumerator PullRoutine()
    {
        float elapsed = 0f;

        while (elapsed < pullDuration)
        {
            if (isDead) yield break;

            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0f;
            transform.position += direction * pullSpeed * Time.deltaTime;

            elapsed += Time.deltaTime;
            yield return null;
        }

        activePullCoroutine = null;
    }

    void Update()
    {
        if (isStaggered) return;

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