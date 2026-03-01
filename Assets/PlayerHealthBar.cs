using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Text healthText;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float animationSpeed = 2f; // units per second

    private float currentHealth;
    private Coroutine animationCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        fillImage.fillAmount = 1f;
        UpdateHealthText();
    }

    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
        float targetFill = currentHealth / maxHealth;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        UpdateHealthText();
        animationCoroutine = StartCoroutine(AnimateFill(targetFill));
    }

    public void TakeDamage(float amount) => SetHealth(currentHealth - amount);
    public void Heal(float amount)       => SetHealth(currentHealth + amount);

    private void UpdateHealthText()
    {
        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
    }

    private IEnumerator AnimateFill(float targetFill)
    {
        while (!Mathf.Approximately(fillImage.fillAmount, targetFill))
        {
            fillImage.fillAmount = Mathf.MoveTowards(
                fillImage.fillAmount,
                targetFill,
                animationSpeed * Time.deltaTime
            );
            yield return null;
        }
        fillImage.fillAmount = targetFill;
    }
}
