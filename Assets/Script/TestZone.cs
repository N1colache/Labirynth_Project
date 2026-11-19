using UnityEngine;

public class TestZone : MonoBehaviour
{
    [SerializeField] private float hungerAmount = 30f;
    [SerializeField] private float thirstAmount = 0f;
    [SerializeField] private float healthAmount = 0f;

    private void OnTriggerEnter(Collider other)
    {
        SurvivalEffects survivalEffects = other.GetComponentInParent<SurvivalEffects>();
        if (survivalEffects == null)
        {
            return;
        }

        if (hungerAmount > 0f)
        {
            survivalEffects.Hunger.Decrease(hungerAmount);
        }
        if (thirstAmount > 0f)
        {
            survivalEffects.Thirst.Decrease(thirstAmount);
        }
        if (healthAmount > 0f)
        {
            survivalEffects.Health.TakeDamage(healthAmount);
        }
    }
    
}
