using UnityEngine.UI;
using UnityEngine;

public class NeedBar : MonoBehaviour
{
    private enum NeedType
    {
        Hunger,
        Thirst,
    }
    [SerializeField] private SurvivalEffects survivalEffects;
     private SurvivalNeeds survivalNeeds;
    [SerializeField] private Image image;
    [SerializeField] private NeedType needType;


    private void Start()
    {
        if (needType == NeedType.Hunger)
        {
            survivalNeeds = survivalEffects.Hunger;
        }
        else if (needType == NeedType.Thirst)
        {
            survivalNeeds = survivalEffects.Thirst;
        }
    }

    private void Update()
    {
        image.fillAmount = survivalNeeds.Percent;
    }
}

