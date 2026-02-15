using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowDownScript : MonoBehaviour
{

    public float slowDown = 5f;
    public ThirdPersonMovement thirdPersonMovement;
    public float originalSpeed;

    private void Awake()
    {
        originalSpeed = thirdPersonMovement.movement_speed;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && other is BoxCollider)
        {
            thirdPersonMovement.movement_speed = slowDown;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player") && other is BoxCollider)
        {
            thirdPersonMovement.movement_speed = originalSpeed;
        }
    }
}
