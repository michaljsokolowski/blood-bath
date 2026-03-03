using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CombatScript : MonoBehaviour
{
    public ComboSystem comboSystem;
    public Animator animator;

    [Header("Combat Settings")]
    [Header("Light Attack")]
    public float lightAttackDamage = 4.5f;
    public float lightAttackRange = 2f;
    public float lightAttackCooldown = 0.5f;

    [Header("Heavy Attack")]
    public float heavyAttackDamage = 7f;
    public float heavyAttackRange = 3f;
    public float heavyAttackCooldown = 1.0f;

    public LayerMask enemyLayers;

    [Header("Player Stats")]
    private int currentHealth = 100;
    public int maxHealth = 100;
    public bool takenDamageDebug;
    private bool hasDied = false;

    [Header("Parry Settings")]
    public float parryDuration = 0.3f;
    private float parryEndTime;
    public float parryRange = 1.5f;

    [Header("Parry Counter-Attack")]
    [Tooltip("Damage dealt to the enemy when a parry succeeds.")]
    public int parryCounterDamage = 15;
    [Tooltip("Duration the enemy is staggered after being parried.")]
    public float parryStaggerDuration = 1.5f;

    [Header("Block Settings")]
    public float blockDamageMultiplier = 0.3f;
    [Tooltip("Maximum angle (degrees) from forward for a block to be effective.")]
    public float blockAngle = 120f;

    private enum DefensiveState { None, Parrying, Blocking }
    private DefensiveState defensiveState = DefensiveState.None;

    private PlayerHealthBar healthBar;

    [Header("Debug Log Enabler")]
    public bool lightAttackDebug;
    public bool heavyAttackDebug;
    public bool comboExecutedDebug;
    public bool showAttackRange;
    public bool showParryRange;

    private float lastLightAttackTime;
    private float lastHeavyAttackTime;
    private BloodCount blood;

    public GameObject damageText;

    private void Start()
    {
        comboSystem = GetComponent<ComboSystem>();
        comboSystem.OnComboExecuted += ExecuteComboEffect;
        healthBar = GameObject.FindGameObjectWithTag("Healthbar").GetComponent<PlayerHealthBar>();
        blood = FindObjectOfType<BloodCount>();
        healthBar.SetHealth(currentHealth);
    }

    private void Update()
    {
        healthBar.SetHealth(currentHealth);

        if (defensiveState == DefensiveState.Parrying && Time.time >= parryEndTime)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                TransitionToBlock();
            }
            else
            {
                StopDefending();
            }
        }
    }

    public void ProcessPlayerInput()
    {
        if (defensiveState == DefensiveState.None)
        {
            if (Input.GetButtonDown("LightAttack") || Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (Time.time >= lastLightAttackTime + lightAttackCooldown)
                {
                    ExecuteLightAttack();
                    lastLightAttackTime = Time.time;
                }
            }
            else if (Input.GetButtonDown("HeavyAttack") || Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (Time.time >= lastHeavyAttackTime + heavyAttackCooldown)
                {
                    ExecuteHeavyAttack();
                    lastHeavyAttackTime = Time.time;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartParry();
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            StopDefending();
        }
    }

    private void StartParry()
    {
        defensiveState = DefensiveState.Parrying;
        parryEndTime = Time.time + parryDuration;
        animator.SetBool("isBlocking", false);
        animator.SetTrigger("parry");
        Debug.Log("Player started parrying!");
    }

    private void TransitionToBlock()
    {
        defensiveState = DefensiveState.Blocking;
        animator.SetBool("isBlocking", true);
        Debug.Log("Parry window expired — player is now blocking.");
    }

    private void StopDefending()
    {
        defensiveState = DefensiveState.None;
        animator.SetBool("isBlocking", false);
        Debug.Log("Player stopped defending.");
    }

    public bool IsParrying()
    {
        return defensiveState == DefensiveState.Parrying;
    }

    public bool IsBlocking()
    {
        return defensiveState == DefensiveState.Blocking;
    }

    public void ApplyParryCounter(Transform attacker)
    {
        if (attacker == null) return;

        EnemyScript enemyScript = attacker.GetComponent<EnemyScript>();
        if (enemyScript != null)
            enemyScript.TakeDamage(parryCounterDamage);

        newBaseAIScript newAI = attacker.GetComponent<newBaseAIScript>();
        if (newAI != null)
            newAI.EnemyReceiveHit(parryCounterDamage);

        DummyScript dummy = attacker.GetComponent<DummyScript>();
        if (dummy != null)
            dummy.EnemyReceiveHit(parryCounterDamage);

        NavMeshAgent enemyAgent = attacker.GetComponent<NavMeshAgent>();
        if (enemyAgent != null)
        {
            StartCoroutine(StaggerEnemy(enemyAgent, parryStaggerDuration));
        }

        Debug.Log($"Parry counter! Dealt {parryCounterDamage} damage and staggered {attacker.name} for {parryStaggerDuration}s.");
    }

    private IEnumerator StaggerEnemy(NavMeshAgent enemyAgent, float duration)
    {
        if (enemyAgent == null) yield break;
        enemyAgent.isStopped = true;
        yield return new WaitForSeconds(duration);
        if (enemyAgent != null)
            enemyAgent.isStopped = false;
    }

    private void ExecuteComboEffect(ComboSystem.DamageType damageType, int totalDamage, ComboSystem.StatusEffect statusEffect, GameObject? target)
    {
        if (comboExecutedDebug)
        {
            Debug.Log($"Combo executed: {damageType}, total damage: {totalDamage}, status effect: {statusEffect}, target: {target?.name ?? "None"}");
        }
        float range = damageType == ComboSystem.DamageType.Slash ? lightAttackRange : heavyAttackRange;
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward, range, enemyLayers);
        foreach (Collider enemy in hitEnemies)
        {
            EnemyScript enemyScript = enemy.GetComponent<EnemyScript>();
            if (enemyScript != null)
                enemyScript.TakeDamage(totalDamage);

            newBaseAIScript newbaseAIScript = enemy.GetComponent<newBaseAIScript>();
            if (newbaseAIScript != null)
                newbaseAIScript.EnemyReceiveHit(totalDamage);

            DummyScript dummyScript = enemy.GetComponent<DummyScript>();
            if (dummyScript != null)
                dummyScript.EnemyReceiveHit(totalDamage);

            GameEvents.RaiseComboExecuted(damageType, totalDamage, statusEffect, enemy.gameObject);
        }
    }

    private void ExecuteLightAttack()
    {
        comboSystem.RegisterAttack(ComboSystem.AttackType.Light);
        bool hitEnemy = ApplyAttackDamage((int)(lightAttackDamage * blood.DMGMulti), lightAttackRange);
        animator.SetTrigger("fast attack");
        if (lightAttackDebug)
        {
            Debug.Log($"Light Attack: Damage={lightAttackDamage * blood.DMGMulti}, Cooldown={lightAttackCooldown}s, Hit={(hitEnemy ? "enemy" : "nothing")}");
        }
    }

    private void ExecuteHeavyAttack()
    {
        comboSystem.RegisterAttack(ComboSystem.AttackType.Heavy);
        bool hitEnemy = ApplyAttackDamage((int)(heavyAttackDamage * blood.DMGMulti), heavyAttackRange);
        animator.SetTrigger("heavy attack");
        if (heavyAttackDebug)
        {
            Debug.Log($"Heavy Attack: Damage={heavyAttackDamage * blood.DMGMulti}, Cooldown={heavyAttackCooldown}s, Hit={(hitEnemy ? "enemy" : "nothing")}");
        }
    }

public bool CheckIfEnemyHit(float range)
    {
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward, range, enemyLayers);
        return hitEnemies.Length > 0;
    }

    private bool ApplyAttackDamage(int damage, float range)
    {
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward, range, enemyLayers);
        if (hitEnemies.Length > 0)
        {
            foreach (Collider enemy in hitEnemies)
            {
                EnemyScript enemyScript = enemy.GetComponent<EnemyScript>();
                if (enemyScript != null)
                {
                    enemyScript.TakeDamage(damage);
                }                                                                   //added to check if new enemy AI works well with combat script
                newBaseAIScript newbaseAIScript = enemy.GetComponent<newBaseAIScript>();
                if (newbaseAIScript != null)
                {
                    newbaseAIScript.EnemyReceiveHit(damage);
                }
                DummyScript dummyScript = enemy.GetComponent<DummyScript>();
                if (dummyScript != null)
                {
                    dummyScript.EnemyReceiveHit(damage);
                }
            }
            return true;
        }
        return false;
    }
    public void TakeDamage(int damageAmount, Transform attacker)
    {
        if (defensiveState == DefensiveState.Parrying && attacker != null)
        {
            Vector3 directionToAttacker = (attacker.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToAttacker);
            float distanceToAttacker = Vector3.Distance(transform.position, attacker.position);

            if (dotProduct > 0.5f && distanceToAttacker <= parryRange)
            {
                Debug.Log("Parry successful! (TakeDamage fallback)");
                ApplyParryCounter(attacker);
                return;
            }
        }

        if (defensiveState == DefensiveState.Blocking && attacker != null)
        {
            Vector3 directionToAttacker = (attacker.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToAttacker);
            float angleToAttacker = Mathf.Acos(Mathf.Clamp(dotProduct, -1f, 1f)) * Mathf.Rad2Deg;

            if (angleToAttacker <= blockAngle * 0.5f)
            {
                damageAmount = (int)(damageAmount * blockDamageMultiplier);
                Debug.Log($"Block successful! Damage reduced to {damageAmount} (angle: {angleToAttacker:F1}°)");
            }
            else
            {
                Debug.Log($"Block failed — attacker at {angleToAttacker:F1}° (max {blockAngle * 0.5f}°). Full damage taken.");
            }
        }
        else if (defensiveState == DefensiveState.Blocking && attacker == null)
        {
            // No attacker reference (e.g. trap) — block still reduces damage
            damageAmount = (int)(damageAmount * blockDamageMultiplier);
            Debug.Log($"Block successful (no direction). Damage reduced to {damageAmount}");
        }

        currentHealth -= damageAmount;
        SpawnsDamagePopups.Instance.DamageDone(damageAmount, transform.position, false);

        if (currentHealth > 0)
        {
            if (takenDamageDebug)
            {
                Debug.Log($"Player took {damageAmount} damage. Current health: {currentHealth}");
            }
        }
        else
        {
            if (!hasDied)
            {
                hasDied = true;
                Debug.Log("Player died!");
            }
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        SpawnsDamagePopups.Instance.HealingDone(healAmount, transform.position);
    }

    public IEnumerator HealOverTime(int healAmount, int times, float interval)
    {
        for (int i = 0; i < times; i++)
        {
            currentHealth += healAmount;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
            SpawnsDamagePopups.Instance.HealingDone(healAmount, transform.position);
            yield return new WaitForSeconds(interval);
        }
    }

    private void OnDrawGizmos()
    {
        if (showAttackRange)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + transform.forward, lightAttackRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.forward, heavyAttackRange);
        }
        if (showParryRange)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, parryRange);
        }
    }
}