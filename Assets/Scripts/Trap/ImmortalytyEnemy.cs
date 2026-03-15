using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ImmortalytyEnemy : MonoBehaviour
{

    private int maxHealth = 50;
    [SerializeField]
    private float currentHealth;
    const int minHealth = 0;
    [SerializeField]
    private CombatScript _combatScript;
    [SerializeField]
    private ComboSystem _combo;
    public bool isObjectOnScene = false;
    private FloatingHealthBar healthBar;
    [SerializeField]
    private float orginalLightDamage;
    [SerializeField]
    private float orginalHevyDamage;
    [SerializeField]
    private int originalComboDamage;

    Coroutine changeCombo;

    private void Awake()
    {
        _combatScript = GameObject.FindAnyObjectByType<CombatScript>();
        _combo = GameObject.FindAnyObjectByType<ComboSystem>();
        healthBar = GetComponentInChildren<FloatingHealthBar>();


        isObjectOnScene = true;

        orginalLightDamage = _combatScript.lightAttackDamage;
        orginalHevyDamage = _combatScript.heavyAttackDamage;
        foreach (var combo in _combo.combos)
        {
            originalComboDamage = combo.damage;
        }

        ImmortalityAdded();
       
    }

    private void Start()
    {
        currentHealth = maxHealth;

        healthBar.DoHealthBar(currentHealth, maxHealth);
    }


    
  public void ImmortalTakeDamage()
    {
        currentHealth -= orginalLightDamage;
        currentHealth -= orginalHevyDamage;

        ComboChange();
        StartCoroutine(ChangeComboDamage());



        healthBar.DoHealthBar(currentHealth, maxHealth);

        if (currentHealth <= minHealth)
        {
            isObjectOnScene = false;
            _combatScript.lightAttackDamage = orginalLightDamage;
            _combatScript.heavyAttackDamage = orginalHevyDamage;

            foreach (var combo in _combo.combos)
            {
                combo.damage = originalComboDamage;
            }


            Destroy(gameObject);
        }
    }

    public void ImmortalityAdded()
    {
       
        if (currentHealth >= minHealth)
        {
            GameObject immortalityObj = GameObject.FindGameObjectWithTag("immortalityObject");
            if (isObjectOnScene == true)
            {
                _combatScript.lightAttackDamage = 0;
                _combatScript.heavyAttackDamage = 0;
                foreach (var combo in _combo.combos)
                {
                    combo.damage = 0;
                    
                }
                


            }
            
        }

       

    }

    void ComboChange()
    {
       
        foreach (var combo in _combo.combos)
        {
            combo.damage = originalComboDamage;
        }
    }

    void ComboReset() 
    {         
        foreach (var combo in _combo.combos)
        {
            combo.damage = 0;
            
        }
    }
    IEnumerator ChangeComboDamage()
    {
        yield return new WaitForSeconds(4f);
        ComboReset();
    }


}
