using UnityEngine;
using System.Collections;

public class Cone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("soldier"))
        {
            Debug.Log("Hit Enemy");
        }
        
    }




}
