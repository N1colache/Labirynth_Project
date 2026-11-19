using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class DeathManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SurvivalEffects survivalEffects;
    [SerializeField] private PlayerStamina  playerStamina;
    [SerializeField] private Transform  spawnPoint;
    
    [SerializeField] private Image  image;
    [SerializeField] private float  timeFade = 1f;
    [SerializeField] private float  waitTime = 2f;

    private bool wasDead;

    private void Update()
    {
        if (playerHealth.IsDead && !wasDead)
        {
            StartCoroutine(nameof(RespawnRoutine));
        }
        wasDead = playerHealth.IsDead;
    }

    private IEnumerator RespawnRoutine()
    {
        pm.restricted = true;
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        float t = 0f;
        while (t < timeFade)
        {
            t += Time.deltaTime;
            float alpha = t/timeFade;
            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;
            yield return null;
        }
        
        Color baseColor = image.color;
        baseColor.a = 1.0f;
        image.color = baseColor;
        
        yield return new WaitForSeconds(waitTime);
        
        pm.freeze = false;
        pm.unlimited = false;
        rb.useGravity = true;
        rb.isKinematic = false;
        
        transform.position = spawnPoint.position;
        rb.position = spawnPoint.position;
        rb.linearVelocity = Vector3.zero;
        
        playerHealth.ResetHealth();
        playerStamina.ResetStamina();
        survivalEffects.Hunger.ResetCurrentValue();
        survivalEffects.Thirst.ResetCurrentValue();
        
        pm.restricted = false;
        
        float r = 0f;
        while (r < timeFade)
        {
            r += Time.deltaTime;
            float alpha = 1 - (r/timeFade);
            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;
            yield return null;
        }
        Color alphaColor = image.color;
        alphaColor.a = 0f;
        image.color = alphaColor;
    }
}
