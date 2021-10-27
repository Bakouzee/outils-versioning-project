using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplayer : MonoBehaviour
{
    [SerializeField] Slider _slider;
    public static Slider slider;

    private void Awake()
    {
        if(slider == null)
        {
            slider = _slider;
        }
    }
    
    // Called by player on Start
    public static void StartDisplay(float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
    }

    // Called by player each time heal value is updated
    public static void UpdateDisplay(float newHealth)
    {
        slider.value = newHealth;
    }
}
