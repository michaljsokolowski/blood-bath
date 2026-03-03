using System.Collections;
using UnityEngine;

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
    private bool isParrying = false;
    public float parryDuration = 0.5f;
    private float parryEndTime;
    public float parryRange = 1.5f;

    public float blockDamageMultiplier = 0.5f;
    private bool isBlocking = false;

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

        if (isParrying && Time.time >= parryEndTime)
        {
            StopParry();
        }
    }

    public void ProcessPlayerInput()
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

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartParry();
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            StopParry();
        }

        if (Input.GetKey(KeyCode.Q))
        {
            if (!isBlocking)
            {
                StartBlocking();
            }
        }
        else
        {
            if (isBlocking)
            {
                StopBlocking();
            }
        }
    }

    private void StartParry()
    {
        if (isBlocking)
        {
            StopBlocking();
        }
        isParrying = true;
        parryEndTime = Time.time + parryDuration;
        animator.SetTrigger("parry");
        Debug.Log("Player started parrying!");
    }

    private void StopParry()
    {
        isParrying = false;
        Debug.Log("Player stopped parrying!");
    }

    private void StartBlocking()
    {
        if (isParrying)
        {
            StopParry();
        }
        isBlocking = true;
        animator.SetBool("isBlocking", true);
        Debug.Log("Player started blocking!");
    }

    private void StopBlocking()
    {
        isBlocking = false;
        animator.SetBool("isBlocking", false);
        Debug.Log("Player stopped blocking!");
    }

    public bool IsParrying()
    {
        return isParrying;
    }

    private void ExecuteComboEffect(ComboSystem.DamageType damageType, int totalDamage, ComboSystem.StatusEffect statusEffect)
    {
        if (comboExecutedDebug)
        {
            Debug.Log($"Combo executed: {damageType}, total damage: {totalDamage}, status effect: {statusEffect}");
        }
        ApplyAttackDamage(totalDamage, damageType == ComboSystem.DamageType.Slash ? lightAttackRange : heavyAttackRange);
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
    // TODO
    // FIX BLOCK 
    public void TakeDamage(int damageAmount, Transform attacker)
    {
        if (isParrying && attacker != null)
        {
            Vector3 directionToAttacker = (attacker.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToAttacker);
            float distanceToAttacker = Vector3.Distance(transform.position, attacker.position);
            if (dotProduct > 0.5f && distanceToAttacker <= parryRange)
            {
                Debug.Log("parry successful");
                return; 
            }
        }

        if (isBlocking)
        {
            damageAmount = (int)(damageAmount * blockDamageMultiplier);
            Debug.Log($"block successful, damage reduced to {damageAmount}");
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