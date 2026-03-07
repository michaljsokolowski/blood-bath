using System.Collections;
using UnityEngine;

public class PlayerEvade : MonoBehaviour
{
    public CharacterController character_controller;
    public float evade_travel_distance = 10f;
    public float evade_time = 0.2f;
    public float evade_cooldown = 1f;
    private bool is_evading_in_progress = false;
    private float next_evade_allowed_time = 0f;

    private Vector3 evade_movement_direction;

    void Update()
    {
        check_for_evade_input();
    }

    private void check_for_evade_input()
    {
        if ((Input.GetButtonDown("EvadeButton") || Input.GetKeyDown(KeyCode.LeftShift)) && Time.time >= next_evade_allowed_time)
        {
            if (!is_evading_in_progress)
            {
                Debug.Log("Evade input detected");
                StartCoroutine(execute_evade_movement());
            }
        }
    }

    private IEnumerator execute_evade_movement()
    {
        Debug.Log("Evade initiated.");
        is_evading_in_progress = true;

        evade_movement_direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

        if (evade_movement_direction == Vector3.zero)
        {
            evade_movement_direction = -transform.forward;
        }

        Debug.Log("Evade direction: " + evade_movement_direction);

        float evade_speed = evade_travel_distance / evade_time;
        float elapsed_time = 0f;

        while (elapsed_time < evade_time)
        {
            character_controller.Move(evade_movement_direction * evade_speed * Time.deltaTime);
            elapsed_time += Time.deltaTime;
            yield return null;
        }

        next_evade_allowed_time = Time.time + evade_cooldown;
        Debug.Log("Evade completed. Cooldown starts.");
        is_evading_in_progress = false;
    }
}
