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
        if (Input.GetKeyDown(KeyCode.F))
        {
            hitZone.SetActive(!hitZone.activeSelf);
        }
    }

    public void ActivateAbility()
    {

    }

}
