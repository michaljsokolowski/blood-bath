using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public int damageTrap = 5;
    private bool isDamaging = false;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player") && collision is BoxCollider)
        {
            Renderer renderer = GetComponent<Renderer>();
            Debug.Log("Player in trap");
            var combat = collision.GetComponent<CombatScript>();
            if (combat != null)
            {
               
                combat.TakeDamage(damageTrap, null);
            }
            renderer.material.color = Color.red;
        }
    }
}
