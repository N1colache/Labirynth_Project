using System;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private PlayerStamina stamina;
    [SerializeField] private Image image;

    private void Update()
    {
        image.fillAmount = stamina.CurrentStamina / stamina.maxStamina;
    }
}
