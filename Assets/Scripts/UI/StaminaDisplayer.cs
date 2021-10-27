using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StaminaDisplayer : MonoBehaviour
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

    // Called from player on Start
    public static void StartDisplay(float maxValue)
    {
        slider.maxValue = maxValue;
        slider.value = maxValue;
    }

    // Called from player each time stamina is updated
    public static void UpdateDisplay(float newValue)
    {
        slider.value = newValue;
    }
}
