using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityScript : MonoBehaviour
{

    public GameObject hitZone;
    private AttackVisual attackVisual;

    private void Start()
    {
        attackVisual = hitZone.GetComponentInChildren<AttackVisual>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            hitZone.SetActive(!hitZone.activeSelf);
            attackVisual.StartCharge();
        }
    }

    public void ActivateAbility()
    {

    }

}
