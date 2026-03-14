using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityScript : MonoBehaviour
{

    public GameObject hitZone;

    
    private void Start()
    {
    }

    private void Update()
    {
      
    }

    public void ActivateAbility()
    {
        

    }
     private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("soldier"))
        {
            Debug.Log("Hit Enemy");
        }
        
    }


}
