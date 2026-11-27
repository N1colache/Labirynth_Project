using UnityEngine;

public class RangeDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float m_detectionRadius = 10f;
    [SerializeField] private LayerMask m_detectionMask;
    [SerializeField] private int m_maxTargetsToDetect = 16;
    [SerializeField] private bool m_showDebugVisual = true;

    private Collider[] m_hitColliders;

    public GameObject DetectedTarget { get; private set; }

    private void Awake()
    {
        // Allocation unique du tableau de colliders pour éviter les allocations en mémoire vive
        m_hitColliders = new Collider[m_maxTargetsToDetect];
    }

    public GameObject UpdateDetector()
    {
        // Utilisation de NonAlloc pour zéro allocation de mémoire à l'exécution
        int numColliders = Physics.OverlapSphereNonAlloc(
            transform.position, 
            m_detectionRadius, 
            m_hitColliders, 
            m_detectionMask
        );

        if (numColliders > 0)
        {
            GameObject closest = null;
            float closestDistanceSqr = float.MaxValue;
            Vector3 currentPosition = transform.position;

            for (int i = 0; i < numColliders; i++)
            {
                Vector3 directionToTarget = m_hitColliders[i].transform.position - currentPosition;
                // sqrMagnitude évite le calcul de racine carrée
                float dSqr = directionToTarget.sqrMagnitude; 

                if (dSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqr;
                    closest = m_hitColliders[i].gameObject;
                }
            }

            DetectedTarget = closest;
        }
        else
        {
            DetectedTarget = null;
        }

        return DetectedTarget;
    }

    private void OnDrawGizmosSelected()
    {
        if (!m_showDebugVisual) return;

        Gizmos.color = (DetectedTarget != null) ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, m_detectionRadius);
    }
}