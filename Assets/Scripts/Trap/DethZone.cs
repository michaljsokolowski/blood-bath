using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DethZone : MonoBehaviour
{
    
    public int MaxHelthDamage;
    [SerializeField]
    GameObject P1;
    [SerializeField]
    GameObject P2;
    [SerializeField]
    float speed = 10f;
    [SerializeField]
    float delay = 1f;
    [SerializeField]
    GameObject zone;

    Vector3 targetPosition;

    private void Awake()
    {
        MaxHelthDamage = 10000;
    }
    private void Start()
    {
        zone.transform.position = P1.transform.position;
        targetPosition = P2.transform.position;
        StartCoroutine(MoveZone());
    }

    IEnumerator MoveZone()
    {
        while (true)
        {
            while ((targetPosition - zone.transform.position).sqrMagnitude > 0.01f)
            {
                zone.transform.position = Vector3.MoveTowards(zone.transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }
            targetPosition = targetPosition == P1.transform.position 
                ? P2.transform.position : P1.transform.position;

            yield return new WaitForSeconds(delay);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other is BoxCollider)
        {
         
            var combat = other.GetComponent<CombatScript>();
            if (combat != null)
            {
                combat.TakeDamage(MaxHelthDamage, null);
            }
        }
    }
}
