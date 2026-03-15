using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class DestructElement : MonoBehaviour
{
    int objectHealth = 100;
     public int objectCurrentHealth;

   

    private void Awake()
    {
        objectCurrentHealth = objectHealth;
        GameEvents.OnComboExecuted += HandleComboExecuted;
        

    }
  

    public void DestructLogic(int damage)
    {
        
        
        
            TakeDamage(damage);
             
        

        if(objectCurrentHealth <= 0)
        {
            GameEvents.OnComboExecuted -= HandleComboExecuted;


           

            Destroy(gameObject);
        }
    }

    void TakeDamage(int damage)
    {
        objectCurrentHealth -= damage;
       
    }
    private void HandleComboExecuted(ComboSystem.DamageType damageType,
                                     int totalDamage,
                                     ComboSystem.StatusEffect statusEffect, GameObject target)
    {
        if (target != this.gameObject) return;

        if (statusEffect == ComboSystem.StatusEffect.Nullie)
        {
           return;
        }
    }
}
