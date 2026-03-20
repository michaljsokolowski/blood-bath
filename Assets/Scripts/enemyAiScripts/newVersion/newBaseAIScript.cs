using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class newBaseAIScript : MonoBehaviour
{
    [Header("Immunity and Knockback parameters")]
    [SerializeField] private float immunityDuration;
    [SerializeField] private float knockbackDuration = 0.15f; 
    private Coroutine knockbackRoutine;

    [Header("Bleed Status Effect")]
    [Tooltip("Damage dealt per bleed tick.")]
    [SerializeField] private int bleedDamagePerTick = 3;

    [Tooltip("How many seconds between each bleed tick.")]
    [SerializeField] private float bleedTickInterval = 0.5f;

    [Tooltip("Total duration of the bleed effect in seconds.")]
    [SerializeField] private float bleedDuration = 3f;

    private Coroutine activeBleedCoroutine;

    [Header("Stagger Status Effect")]
    [SerializeField] private float staggerDuration = 5f;
    private Coroutine activeStaggerCoroutine;

    [Header("Pull Status Effect")]
    [SerializeField] private float pullDuration = 1f;
    [SerializeField] private float pullSpeed = 10f;
    private Coroutine activePullCoroutine;


    [Header("Moved form RangedEnemyAIScript")]
    public float repositionRadius = 5f;
    public int repositionSampleCount = 12;

    [Header("Moved from MeleeEnemyAIScript")]
    public AttackSO[] attackSOs;

    private float attackCooldownTimer = 1f;
    private int attackIndex = 0;
    [SerializeField] private AttackingPatters attackingPatters;

    [Header("Originally in BaseAIScript")]
    protected NavMeshAgent agent;

    protected Transform player;

    [SerializeField] protected LayerMask groundMask;
    [SerializeField] protected LayerMask playerMask;

    //patrolling
    protected Vector3 walkPoint;
    protected bool isWalkPointSet = false;
    [SerializeField] protected float walkPointRange = 5f;

    //attacking
    [SerializeField] protected float timeBetweenAttacks = 2f;
    protected bool hasAlreadyAttacked;

    //States
    [SerializeField] protected float sightRange = 10f;
    [SerializeField] private float chaseOffset = 3f;

    private float attackRange;

    protected float distanceToPlayer;

    [Header("Moved from EnemyScript.cs")]
    [SerializeField] protected int maxHealth = 500;
    protected int currentHealth;
    [SerializeField] protected float bulletSpeed = 10f;
    [SerializeField] protected float bulletLifetime = 2f;

    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected LayerMask obstacleMask;

    public SpawningBlood blood;
    public orbSpawn orbs;
    public FloatingHealthBar healthBar;
    protected bool isDead = false;

    protected virtual void Start()
    {
        //agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<FloatingHealthBar>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        blood = FindObjectOfType<SpawningBlood>();
        orbs = FindObjectOfType<orbSpawn>();
        healthBar.DoHealthBar(currentHealth, maxHealth);
        agent = GetComponent<NavMeshAgent>();

        GameEvents.OnComboExecuted += HandleComboExecuted;
        GameEvents.OnAbilityUsed += HandleAbilityUsed;
    }

    protected virtual void OnDestroy()
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

    private void HandleAbilityUsed(string text)
    {
        // Example: If the ability used is "Fireball", apply a burn effect
        if (text == "Trafiony")
        {
            // Apply burn effect logic here
            Debug.Log("yah.");
        }
    }

    private IEnumerator BleedRoutine()
    {
        float elapsed = 0f;

        while (elapsed < bleedDuration)
        {
            yield return new WaitForSeconds(bleedTickInterval);
            elapsed += bleedTickInterval;

            if (isDead) yield break;

            EnemyTakeDamage(bleedDamagePerTick);
            Debug.Log($"{gameObject.name} is bleeding - took {bleedDamagePerTick} bleed damage.");
        }

        activeBleedCoroutine = null;
    }

    private IEnumerator StaggerRoutine()
    {
        agent.isStopped = true;
        float elapsed = 0f;

        while (elapsed < staggerDuration)
        {
            yield return null;
            elapsed += Time.deltaTime;

            if (isDead) yield break;
        }

        Debug.Log("enemy got staggered for " + staggerDuration + " seconds");

        agent.isStopped = false;
        activeStaggerCoroutine = null;
    }

    private IEnumerator PullRoutine()
    {
        float elapsed = 0f;
        agent.isStopped = true;

        while (elapsed < pullDuration)
        {
            if (isDead) yield break;

            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0f;
            transform.position += direction * pullSpeed * Time.deltaTime;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("enemy got pulled towards the player");

        agent.isStopped = false;
        activePullCoroutine = null;
    }

    protected virtual void Update()
    {
        distanceToPlayer = (player.position - transform.position).sqrMagnitude;
        float newAttackRange = GetAttackRange();
        //Debug.Log("Current attack range value: " + newAttackRange);

        if (attackSOs.Length != 0 && attackSOs[attackIndex].isRanged == true)
        {
            //Debug.Log("Enemy in RangedMode");
            HandleStatesRanged(distanceToPlayer, newAttackRange);
        }
        else
        {
            //Debug.Log("Enemy in MeleeMode");
            HandleStatesMelee(distanceToPlayer, newAttackRange);
        }
        if(attackCooldownTimer > 0f)
        {
            //Debug.Log("attackCooldownTimer decreased. Current value: " + attackCooldownTimer);
            attackCooldownTimer -= Time.deltaTime;
        }

    }

    protected virtual void HandleStatesMelee(float distanceToPlayer, float newAttackRange)
    {
        if (distanceToPlayer < newAttackRange * newAttackRange)
        {
            //Debug.Log("Attacking player in MeleeMode");
            AttackPlayer();
        }
        else if (distanceToPlayer < (newAttackRange + chaseOffset) * (newAttackRange + chaseOffset) )
        {
            //Debug.Log("Chasing player in MeleeMode");
            ChasePlayer();
        }
        else
        {
            RandomPatrol();
        }       
    }

    private void HandleStatesRanged(float distanceToPlayer, float newAttackRange)
    {
        if (distanceToPlayer < ((newAttackRange * newAttackRange) / 2))
        {
            //if player is too close, run away from him
            if (HasLineOfSight())
            {
                Flee();
            }

        }
        else if (distanceToPlayer < newAttackRange * newAttackRange)
        {
            //AttackPlayer();

            if (HasLineOfSight())
            {
                AttackPlayer();
            }
            else
            {
                WalkAroundObstacle();
            }
        }
        else if (distanceToPlayer < (newAttackRange + chaseOffset) * (newAttackRange + chaseOffset))
        {
            ChasePlayer();
        }
        else
        {
            RandomPatrol();
        }
    }

    private void Flee()
    {
        //choose direction away from the player and choose a point in that direction to run to
        Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
        Vector3 fleeTarget = transform.position + directionAwayFromPlayer * 2;

        //if the point exists, run towards it
        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeTarget, out hit, 2f, NavMesh.AllAreas))
        {
            //Debug.Log("enemy flees");
            agent.SetDestination(hit.position);
        }
    }

    protected void WalkAroundObstacle()
    {
        Vector3 center = player.position;
        float angleStep = 360f / repositionSampleCount;

        for (int i = 0; i < repositionSampleCount; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 samplePos = center + dir * repositionRadius;

            NavMeshHit navHit;

            if (NavMesh.SamplePosition(samplePos, out navHit, 1.0f, NavMesh.AllAreas))
            {
                // Simulate shot from sample position
                Vector3 shootDir = (player.position - navHit.position).normalized;
                float dist = Vector3.Distance(navHit.position, player.position);

                if (!Physics.SphereCast(navHit.position, bulletPrefab.GetComponent<SphereCollider>().radius * 0.8f, shootDir, out RaycastHit hit, dist, obstacleMask)
                    || hit.transform == player)
                {
                    agent.SetDestination(navHit.position);
                    return;
                }
            }
        }
    }

    //checks for obstacles between enemy and player while trying to shoot
    protected bool HasLineOfSight()
    {
        RaycastHit hit;
        Vector3 directionToPlayer = (player.position - firePoint.position).normalized;

        if (Physics.SphereCast(transform.position, 0.5f, directionToPlayer, out hit, attackRange, obstacleMask))
        {
            Debug.Log("obstacle between enemy and player");
            return false;
        }
        else
        {
            return true;
        }

    }

    protected void AttackPlayer()
    {
        agent.SetDestination(transform.position); // Stop
        //transform.LookAt(player);

        if (attackCooldownTimer <= 0f)
        {
            attackCooldownTimer = attackSOs[attackIndex].cooldown;

            if (attackingPatters == AttackingPatters.RoundRobin)
            {
                Debug.Log("DoAttack called!");
                DoAttack(attackIndex);
                //Debug.Log("Right after DoAttack called!");
                attackIndex++;

                if (attackIndex >= attackSOs.Length)
                {
                    attackIndex = 0;
                }
            }
        }
    }

    private void DoAttack(int attackIndex)
    {
        //Debug.Log("DoAttack called! " + attackSOs[attackIndex].attackName);
        attackSOs[attackIndex].ExecuteAttack(transform, player, firePoint, playerMask, this);
    }

    protected virtual void RandomPatrol()
    {
        //searches for a random point in range to walk to
        if (!isWalkPointSet)
        {
            SearchWalkPoint();
        }

        // walks towards the point
        if (isWalkPointSet)
        {
            agent.SetDestination(walkPoint);
        }

        //resets the point upon reaching it
        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
        {
            isWalkPointSet = false;
        }
    }

    //search for a random point in range to walk to
    protected virtual void SearchWalkPoint()
    {
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        float randomZ = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        //Debug.Log("walk point selected:" + walkPoint);

        //check if its in boundaries of NavMeshSurface (i think)
        if (Physics.Raycast(walkPoint, -transform.up, 2f, groundMask))
        {
            //Debug.Log("walk point selected");
            isWalkPointSet = true;
        }
    }

    protected virtual void ChasePlayer()
    {
        //starts chasing the player
        Debug.Log("Enemy AI chases!");
        agent.SetDestination(player.position);
    }

    private float GetAttackRange()
    {
        if (attackSOs.Length != 0)
        {
            return attackSOs[attackIndex].attackRange;
        }
        else
        {
            return 2f;
        }
    }

    public void EnemyReceiveHit(int damage)
    {

        EnemyTakeDamage(damage); // You can replace this with your enemy-specific logic
        SpawnsDamagePopups.Instance.DamageDone(damage, transform.position, false);

        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        float knockbackDistance = damage * 0.15f;
        knockbackRoutine = StartCoroutine(KnockbackFrom(player.position, knockbackDistance));
    }

    private IEnumerator KnockbackFrom(Vector3 sourcePosition, float knockbackDistance)
    {
        Vector3 direction = (transform.position - sourcePosition).normalized;
        direction.y = 0f;

        Vector3 start = transform.position;
        Vector3 end = start + direction * knockbackDistance;

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            float t = elapsed / knockbackDuration;
            transform.position = Vector3.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
    }

    public void EnemyTakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("enemy took " + damage + " damage. Current health: " + currentHealth);
        healthBar.DoHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
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
        if (blood != null && orbs != null)
        {
            blood.SpawnBloodAt(enemyPosition);
            orbs.SpawnOrbAt(enemyPosition);
        }
        Debug.Log("enemy died!");
        Destroy(gameObject);
        Debug.Log("Die() called for " + gameObject.name);
    }

    private enum AttackingPatters
    {
        Random,
        RoundRobin,
    }

    protected virtual void OnDrawGizmos()
    {
        //sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, (attackRange + chaseOffset));
        //attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
