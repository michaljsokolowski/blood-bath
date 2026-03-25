using UnityEngine;
using System.Collections;

public class Cone : MonoBehaviour
{
    public AttackVisual attackVisual;
    public GameObject AbilityIndicator;
    public delegate void ConeHitAction(string text);
    public event ConeHitAction OnAbilityUsed;

    private void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("soldier"))
        {
            Collider[] hitEnemies = other.GetComponents<Collider>();
            foreach (Collider enemy in hitEnemies)
            {
                GameEvents.RaiseAbilityUsed(20, enemy.gameObject);
                //OnAbilityUsed?.Invoke(20, enemy.gameObject);
            }
        }

    }

    void Update()
    {
        
    }
}
