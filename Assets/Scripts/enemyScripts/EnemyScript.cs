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

    void Start()
    {
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<FloatingHealthBar>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        blood = FindObjectOfType<SpawningBlood>();
        orbs = FindObjectOfType<orbSpawn>();
        healthBar.DoHealthBar(maxHealth, maxHealth);
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

    // checks if the player is within firing range
    bool IsPlayerInRange()
    {
        Vector3 enemyPosition =
          new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 playerPosition = new Vector3(player.position.x, 0, player.position.z);
        float distance = Vector3.Distance(enemyPosition, playerPosition);

        return distance <= fireRange;
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - firePoint.position).normalized;
        RaycastHit hit;

        if (Physics.Raycast(firePoint.position, directionToPlayer, out hit, fireRange)) {
            if (hit.collider.gameObject.tag == "Obstacle") {
                return false;
            }
        }

        return true;
    }

    void Shoot()
    {
        if (player != null) {
            Vector3 direction = (player.position - firePoint.position).normalized;
            direction.y = 0;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            BulletScript bulletScript = bullet.GetComponent<BulletScript>();
            if (bulletScript != null)
            {
                bulletScript.SetAttacker(this.transform); 
            }
            bullet.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;

            Destroy(bullet, bulletLifetime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fireRange);
    }
    public void EnemyReceiveHit(int damage)
    {
        TakeDamage(damage);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        // Debug.Log("enemy took " + damage + " damage. Current health: " +
        // currentHealth);
        healthBar.DoHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0) {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
            return;
        isDead = true;
        Vector3 enemyPosition =
          new Vector3(transform.position.x, 0, transform.position.z);
        if (blood != null && orbs != null) {
            blood.SpawnBloodAt(enemyPosition);
            orbs.SpawnOrbAt(enemyPosition);
        }
        Debug.Log("enemy died!");
        Destroy(gameObject);
        Debug.Log("Die() called for " + gameObject.name);
    }
}
