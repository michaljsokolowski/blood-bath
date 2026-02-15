using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap2x2Logic : MonoBehaviour
{
    public int trap2x2DMG = 5;
    public GameObject trap2x1;
    public GameObject trap1x2b;
    public GameObject trap2x1b;
    public GameObject trap1x2;
    private bool firstTrapSet = false;
    private bool secondTrapSet = false;

    public void OnTriggerEnter(Collider collision)
    {

        if (collision.CompareTag("Player") && collision is BoxCollider)
        {

            int random;
            random = Random.Range(0, 2);
            if (random == 0)
            {
                trap2x1.SetActive(true);
                trap1x2.SetActive(true);
                firstTrapSet = true;
                
            }
            else
            {
                trap1x2b.SetActive(true);
                trap2x1b.SetActive(true);
                secondTrapSet = true;
               
            }
            enabler();
        }
        
    }
    public void enabler()
    {
        if (firstTrapSet == true)
        {
            StartCoroutine(trapDelay());
            StopCoroutine(trapDelay());
        }
        else if (secondTrapSet == true)
        {
            StartCoroutine (trapDelay());
            StopCoroutine(trapDelay());
        }
    }
    IEnumerator trapDelay()
    {
        yield return new WaitForSeconds(2f);
        trap1x2.SetActive(false);
        trap1x2b.SetActive(false);
        trap2x1b.SetActive(false);
        trap2x1.SetActive(false);
        //StopAllCoroutines();
    }
}
