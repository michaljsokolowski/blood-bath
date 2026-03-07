using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityScript : MonoBehaviour
{
    private void Update()
    {
         if (Input.GetKeyDown(KeyCode.E))
        {
            OnDrawGizmos();

        }

    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = new Color(255f, 164f, 41f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 5f);

    }

}
