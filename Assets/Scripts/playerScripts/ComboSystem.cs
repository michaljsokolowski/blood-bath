using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Combo
{
    // combo pattern: damage type, total damage.
    public string comboPattern;
    public ComboSystem.DamageType damageType;
    public int damage;
    public ComboSystem.StatusEffect statusEffect;
}

public class ComboSystem : MonoBehaviour
{
    [Header("Debug log enabler")]
    public bool comboMatchedDebug;
    public bool comboTimeoutDebug;
    public bool comboMissDebug;
    public bool noticeAttackDebug;
    // damage types
    public enum AttackType
    {
        Light,
        Heavy
    }
    public enum DamageType
    {
        Blunt,
        Slash
    }
    public enum StatusEffect
    {
        Bleed,
        Stagger,
        Pull
    }

    [Header("Combo Configurations")]
    public List<Combo> combos = new List<Combo>();
    
    // track current combo and reset time
    private List<AttackType> current_combo = new List<AttackType>();
    public float combo_reset_time = 2f;
    private float last_attack_time;

    // trigger event when combo is executed
    public delegate void ComboAction(DamageType damageType, int totalDamage, StatusEffect statusEffect, GameObject? target);
    public event ComboAction OnComboExecuted;

    
    private Dictionary<string, (DamageType damageType, int damage)> comboDictionary;

    // zwiększanie dmg z combo
    private BloodCount blood;

    void Start()
    {
        InitializeComboDictionary();
        blood = FindObjectOfType<BloodCount>();
    }

    private void InitializeComboDictionary()
    {
        // convert the list of combos into dictionary
        comboDictionary = new Dictionary<string, (DamageType, int)>();
        foreach (var combo in combos) {
            if (!string.IsNullOrWhiteSpace(combo.comboPattern)) {
                comboDictionary[combo.comboPattern] = (combo.damageType, combo.damage);
            }
        }
    }

    // register new attack to the current combo chain
     public void RegisterAttack(AttackType attackType)
    {
        float range = 0f;
        CombatScript CombatScript = GetComponent<CombatScript>();
        if (attackType == AttackType.Light) // sprawdzenie typu ataku, żeby wiedzieć jaką odległość sprawdzać przy combo
        {
            range = CombatScript.lightAttackRange;
        }
        else if (attackType == AttackType.Heavy)
        {
            range = CombatScript.heavyAttackRange;}

        if (Time.time - last_attack_time > combo_reset_time) { // reset combo if time exceeded
            if (comboTimeoutDebug) {
                Debug.Log("combo timeout");
            }
            current_combo.Clear();
        } else if (!CombatScript.CheckIfEnemyHit(range)) // reset combo if attack missed
        {
            if (comboMissDebug) {
                Debug.Log("attack missed, combo reset");
            }
            current_combo.Clear();
        }
        // add attack to the chain
        current_combo.Add(attackType);
        last_attack_time = Time.time;

        if (noticeAttackDebug) {
            Debug.Log(
              $"noticed attack: {attackType}. current combo: {string.Join("", current_combo)}");
        }

        CheckCombo();
    }

    private void CheckCombo()
    {
        // check if the current combo matches defined patterns
        if (current_combo.Count == 3) {
            string comboKey = string.Join("", current_combo);

            // execute combo event when combo chain matches
            if (comboDictionary.TryGetValue(comboKey, out var comboData)) {
                if (comboMatchedDebug) {
                    Debug.Log(
                      $"combo matched: {comboKey}, DamageType: {comboData.damageType}, Damage: {(int)(comboData.damage * blood.DMGMulti)}");
                }
                OnComboExecuted?.Invoke(comboData.damageType,
                                        (int)(comboData.damage * blood.DMGMulti), combos.Find(c => c.comboPattern == comboKey).statusEffect, null);
            } else {
                Debug.Log($"no combo found: {comboKey}");
            }

            // reset combo after performing one
            current_combo.Clear();
        }
    }
}
