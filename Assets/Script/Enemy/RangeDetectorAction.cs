using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Range Detector", 
    story: "Update Range [Detector] and assign [Target]", 
    category: "Action", 
    id: "5fce0489594a8ad689e7534e4a78ced5"
)]
public partial class RangeDetectorAction : Action
{
    [SerializeReference] public BlackboardVariable<RangeDetector> Detector;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    protected override Status OnUpdate()
    {
        // Sécurité contre les crashs si le Blackboard est mal configuré
        if (Detector == null || Detector.Value == null || Target == null)
        {
            return Status.Failure;
        }

        // Mise à jour du détecteur et assignation de la cible dans le Blackboard
        GameObject foundTarget = Detector.Value.UpdateDetector();
        Target.Value = foundTarget;

        // Renvoie Success si une cible a été trouvée, Failure sinon
        return foundTarget != null ? Status.Success : Status.Failure;
    }
}