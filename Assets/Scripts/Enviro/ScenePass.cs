using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ScenePass : MonoBehaviour
{
    int nextSceneIndex;
    public List<GameObject> enemyList;
    Collider m_ObjectCollider;

    private void Awake()
    {
        nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        m_ObjectCollider = GetComponent<BoxCollider>();
        

    }
    private void Update()
    {
        enemyList = new List<GameObject>(GameObject.FindGameObjectsWithTag("soldier"));

        if (enemyList != null)
        {
           m_ObjectCollider.isTrigger = false;
        }
        if (enemyList.Count == 0) 
        {
            m_ObjectCollider.isTrigger = true;
        }
      
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown("space"))
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}
