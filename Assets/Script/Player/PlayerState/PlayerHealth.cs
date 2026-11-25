using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float regenDelay = 4f;
    [SerializeField] private float regenRate = 15f;
    
    public float CurrentHealth {get; private set;}
    public float MaxHealth
    {
        get { return maxHealth; }
    }
    public bool IsDead { get; private set; }
    public bool canRegenerate = true;
    
    private float lastDamageTime;
    
    private void  Start()
    {
        CurrentHealth = maxHealth;
    }
    
    private void Update()
    {
        if (!IsDead && canRegenerate && Time.time - lastDamageTime > regenDelay && CurrentHealth < maxHealth)
        {
            Heal(regenRate * Time.deltaTime);
        }

        
    }
    
     public void TakeDamage(float damage)
    {
        if(IsDead) return;
        
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(0, CurrentHealth);
        lastDamageTime = Time.time;
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
    public void Heal(float amount)
    {
        if(IsDead) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    private void Die()
    {
        IsDead = true;
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        CurrentHealth += amount;
    }

    [ContextMenu("Test dégâts")]
    private void TestDamage()
    {
        TakeDamage(28);
    }
    
    [ContextMenu("Test Heal")]
    private void TestHeal()
    {
        Heal(28);
    }
    
    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        IsDead = false;
        lastDamageTime = 0f;
    }
}
