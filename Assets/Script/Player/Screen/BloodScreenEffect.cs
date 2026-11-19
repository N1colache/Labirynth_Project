using System;
using UnityEngine;
using UnityEngine.UI;

public class BloodScreenEffect : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    private Image bloodScreen;
    
    [SerializeField] private float startShowing = 0.7f;
    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float flashAlpha = 0.5f;
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private float pulseThreshold = 0.25f;
    [SerializeField] private float pulseSpeed = 6f;
    
    private float lastHealth;
    private float flashTimer;

    private void Start()
    {
        bloodScreen = GetComponent<Image>();
        lastHealth = playerHealth.CurrentHealth;
    }

    private void Update()
    {
        float healthPercent =  playerHealth.CurrentHealth / playerHealth.MaxHealth;
        
        float intensity = Mathf.InverseLerp(startShowing, 0, healthPercent);
        float baseAlpha =  intensity * maxAlpha;

        if (healthPercent < pulseThreshold)
        {
            baseAlpha *= (0.85f + 0.15f * Mathf.Sin(Time.time * pulseSpeed));
        }

        if (playerHealth.CurrentHealth < lastHealth)
        {
            flashTimer = flashDuration;
        }

        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
        }
        float flashValue = Mathf.Max(0, flashAlpha * (flashTimer / flashDuration));
        
        float finalAlpha = Mathf.Max(baseAlpha,  flashValue);
        Color color = bloodScreen.color;
        color.a = finalAlpha;
        bloodScreen.color = color;
        
        lastHealth = playerHealth.CurrentHealth;
            
    }
}
