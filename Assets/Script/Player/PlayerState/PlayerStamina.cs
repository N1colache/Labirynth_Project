using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [SerializeField] private float maxStamina = 100f; 
    public float MaxStamina
    {
        get { return maxStamina; }
    }
    [SerializeField] private float regenPerSecond = 25f;
    [SerializeField] private float regenDelay = 0.8f;
    [SerializeField] private float exhaustedThreshold = 25f;

    public  float CurrentStamina { get; private set; }
    public float EffectiveMax { get { return maxStamina * maxMultiplier; } }
    private float lastTimeStamina;
    public float regenMultiplier = 1f;
    public float maxMultiplier = 1f;
    public  bool IsExhausted{ get; private set; }
    
    private void Awake()
    {
        CurrentStamina =  maxStamina;
    }

    public void Drain(float perSecond)
    {
        CurrentStamina =  Mathf.Max(0f, CurrentStamina - (perSecond * Time.deltaTime));
        lastTimeStamina = Time.time;
        if(CurrentStamina <= 0)
        {
            IsExhausted = true;
        }
    }

    public bool TryConsume(float amount)
    {
        if(CurrentStamina < amount) return false;
        else
        {
            CurrentStamina = CurrentStamina -  amount;   
            lastTimeStamina = Time.time;
        }
        if(CurrentStamina <= 0) 
            IsExhausted = true;
        return true;
    }

    private void Update()
    {
        CurrentStamina = Mathf.Min(CurrentStamina, EffectiveMax);
        if (CurrentStamina >= EffectiveMax) return;
        if (Time.time - lastTimeStamina < regenDelay) return;
        CurrentStamina = Mathf.Min(EffectiveMax, CurrentStamina + (regenPerSecond * regenMultiplier *Time.deltaTime));
        
        if(CurrentStamina > exhaustedThreshold)
            IsExhausted = false;
    }

    public void MultiplyMaxStamina(float amount)
    {
        maxStamina *= amount;
    }

    public void ResetStamina()
    {
        CurrentStamina = maxStamina;
        IsExhausted = false;
    }
}
