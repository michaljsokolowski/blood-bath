using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeTrap : MonoBehaviour
{
   [SerializeField]
    float timeTo = 5f;
   [SerializeField]
    Text displayTimer;
    [SerializeField]
    float remaningTime;
    [SerializeField]
    GameObject textObject;

    [SerializeField]
    float dmageInterval = 1f;
    float nextDamageTime = 0f;

    

    public int trapDamage = 10;

    private void Start()
    {
        remaningTime = timeTo;
        textObject.SetActive(false);

    }
    public void OnTriggerEnter(Collider collision)
    {

        if (collision.CompareTag("Player") && collision is BoxCollider)
        {
            
           StartCoroutine(TimerToDamage(1f));
            var combat = collision.GetComponent<CombatScript>();
            if (timeTo <= 0)
            {
                combat.TakeDamage(trapDamage, null);
            }

        }
        StartCoroutine(resetTimer(10f));
    }

     //do zrobienia corutyna by nie lecia³ damage co klatkê a co jakiœ czas
    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && other is BoxCollider && Time.time >= nextDamageTime)
        {
            
            var combat = other.GetComponent<CombatScript>();
            if (timeTo <= 0)
            {
                  combat.TakeDamage(trapDamage, null);
                
                nextDamageTime = Time.time + dmageInterval;
            }
        }
       
    }
    private void Update()
    {
        displayTimer.text = timeTo.ToString("0");
    }
    IEnumerator TimerToDamage(float time)
    {
        textObject.SetActive(true);
        while (remaningTime > 0)
        {
            yield return new WaitForSeconds(time);
            timeTo--;
            updateTime();
            
        }
        countDownFinished();
       
    }

    void updateTime()
    {
        remaningTime = timeTo;
        displayTimer.text = timeTo.ToString();
        
    }

    void countDownFinished()
    {
        if (remaningTime == 0)
        {
            StopCoroutine(TimerToDamage(0f));
            textObject.SetActive(false);
            
        }
    }
    IEnumerator resetTimer(float time)
    {
        yield return new WaitForSeconds(time);

        timeTo = 5;
        updateTime();
    }
    IEnumerator damagePerTick()
    {
        yield return new WaitForSeconds(1f);
        
        
        
    }
}
