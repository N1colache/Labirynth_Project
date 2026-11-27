using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(
    name: "Line of Sight Check", 
    story: "check [Target] with Line Of Sight [Detector]", 
    category: "Conditions", 
    id: "0d5587d058328bbb8c27bd2e16b7a2bd"
)]
public partial class LineOfSightCheckCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<LineOfSightDetector> Detector;

    public override bool IsTrue()
    {
        // Sécurité : empêche les NullReferenceException dans l'arbre de comportement
        if (Detector == null || Detector.Value == null || Target == null || Target.Value == null)
        {
            return false;
        }
        else
        {
            return true;
        }

        return Detector.Value.PerformDetection(Target.Value) != null;
    }
}