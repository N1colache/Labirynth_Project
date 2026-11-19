using UnityEngine;

public class SurvivalNeeds : MonoBehaviour
{
    [SerializeField] string neededName;
    [SerializeField] float maxValue = 100f;
    [SerializeField] float decayRate;

    public float CurrentValue
    {
        get; private set;
    }

    public float MaxValue
    {
        get
        {
            return maxValue;
        }
    }

    public float Percent
    {
        get
        {
            return CurrentValue/maxValue;
        }
    }
    public bool IsEmpty
    {
        get
        {
            return CurrentValue <= 0;
        }
        
    }

    private float decayMultiplier = 1f;
    
    

    private void Start()
    {
        CurrentValue = maxValue;
    }

    private void Update()
    {
        CurrentValue -= decayRate * decayMultiplier * Time.deltaTime ;
        CurrentValue  =  Mathf.Max(0, CurrentValue);
    }

    public void Refill(float amount)
    {
        CurrentValue = Mathf.Min(CurrentValue +  amount, maxValue);
    }

    public void Decrease(float amount)
    {
        CurrentValue =  Mathf.Max(CurrentValue - amount, 0f) ;
    }

    public void MultiplyDecay(float amount)
    {
        decayMultiplier *= amount;
    }

    public void ResetCurrentValue()
    {
        CurrentValue = maxValue;
    }
    
    [ContextMenu("Vider")]
    private void TestEmpty()
    {
        CurrentValue = 0f;
    }
    
    [ContextMenu("TestRefill")]
    private void TestRefill()
    {
        Refill(100);
    }
    
    [ContextMenu("TestShow")]
    private void TestShow()
    {
        Debug.Log(neededName + ":" + CurrentValue);
    }
}
