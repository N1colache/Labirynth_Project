using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenPerSecond = 25f;
    [SerializeField] private float regenDelay = 0.8f;
    [SerializeField] private float exhaustedThreshold = 25f;

    public  float CurrentStamina { get; private set; }
    private float lastTimeStamina;
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
        if (CurrentStamina >= maxStamina) return;
        if (Time.time - lastTimeStamina < regenDelay) return;
        CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + (regenPerSecond * Time.deltaTime));
        
        if(CurrentStamina > exhaustedThreshold)
            IsExhausted = false;
    }
}
