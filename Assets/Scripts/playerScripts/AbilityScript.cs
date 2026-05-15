using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityScript : MonoBehaviour
{

    public GameObject hitZone;
    public GameObject abilityIndicator;
    public AttackVisual attackVisual;

    private void Start()
    {
        attackVisual = abilityIndicator.GetComponent<AttackVisual>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            attackVisual.StartCharge();
        }

        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            hitZone.SetActive(!hitZone.activeSelf);
            attackVisual.ActivateAttack();
        }
    }

    public void ActivateAbility()
    {

    }

}
