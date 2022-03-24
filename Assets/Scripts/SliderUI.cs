using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderUI : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] Text text;
    [SerializeField] float minValue;
    [SerializeField] float maxValue;
    [SerializeField] bool isFloat;
    [SerializeField] float currentValue;

    void Start()
    {
        if (isFloat) { slider.wholeNumbers = false; } else { slider.wholeNumbers = true; }
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = currentValue;

        text.text = currentValue.ToString();

        slider.onValueChanged.AddListener((value) =>
        {
            text.text = value.ToString();
        });
    }


}
