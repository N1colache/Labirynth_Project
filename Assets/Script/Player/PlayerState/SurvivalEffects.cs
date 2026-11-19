
using UnityEngine;

public class SurvivalEffects : MonoBehaviour
{
  [SerializeField] private SurvivalNeeds hunger;
  [SerializeField] private SurvivalNeeds thirst;
  [SerializeField] private PlayerHealth health;
  [SerializeField] private PlayerStamina stamina;
  [SerializeField] private PlayerMovement pm;

  [SerializeField] private float slowThreshold = 0.3f;
  [SerializeField] private float slowMultiplier = 0.7f;
  [SerializeField] private float starveDamage = 5f;
  [SerializeField] private float starveInterval = 2f;
  [SerializeField] private float minThirstRegen = 0.3f;
  [SerializeField] private float dehydratedMax = 0.5f;
  
  public SurvivalNeeds Hunger {get {return hunger;}}
  public SurvivalNeeds Thirst  {get {return thirst;}}
  public PlayerHealth Health  {get {return health;}}

  private float starveTimer;

  private void Update()
  {
    health.canRegenerate = !hunger.IsEmpty;
    if (hunger.Percent < slowThreshold)
    {
      pm.speedMultiplier = slowMultiplier;
    }
    else
    {
      pm.speedMultiplier = 1;
    }

    if (hunger.IsEmpty)
    {
      starveTimer += Time.deltaTime;
      if (starveTimer >= starveInterval)
      {
        health.TakeDamage(starveDamage);
        starveTimer = 0;
      }
    }
    else
    {
      starveTimer = 0;
    }
    stamina.regenMultiplier = Mathf.Lerp(minThirstRegen, 1, thirst.Percent) ;
    if (thirst.IsEmpty)
    {
      stamina.maxMultiplier = dehydratedMax;
    }
    else
    {
      stamina.maxMultiplier = 1;
    }
    
  }
  
}
