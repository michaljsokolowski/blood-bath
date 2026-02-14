using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public Slider slider;
    public Text healthText;

    void Awake()
    {
        slider = GetComponentInChildren<Slider>();
        healthText = GetComponentInChildren<Text>();
    }

    public void DoHealthBar(float currentValue, float maxValue)
    {
        slider.value = currentValue / maxValue;
        healthText.text = currentValue.ToString() + "/" + maxValue.ToString();
    }
}
