using UnityEngine;

public class Consumable : MonoBehaviour
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
            survivalEffects.Hunger.Refill(hungerAmount);
        }
        if (thirstAmount > 0f)
        {
            survivalEffects.Thirst.Refill(thirstAmount);
        }
        if (healthAmount > 0f)
        {
            survivalEffects.Health.Heal(healthAmount);
        }
        Destroy(gameObject);
    }
    
}
