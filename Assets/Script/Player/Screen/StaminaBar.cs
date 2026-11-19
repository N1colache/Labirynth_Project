
using System;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private PlayerStamina stamina;
    [SerializeField] private Image image;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float hideDelay = 1.5f;
    [SerializeField] private float fadeSpeed = 3f;

    private float lastUsedTime;

    private void Start()
    {
        canvasGroup.alpha = 0;
    }

    private void Update()
    {
        if (stamina.CurrentStamina < stamina.MaxStamina)
        {
           lastUsedTime = Time.time;  
        }
        
        image.fillAmount = stamina.CurrentStamina / stamina.MaxStamina;
        float alpha;
        if (Time.time  - lastUsedTime < hideDelay)
        {
            alpha = 1f;
        }
        else
        {
            alpha = 0f;
        }
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, alpha, fadeSpeed * Time.deltaTime);
    }
}
