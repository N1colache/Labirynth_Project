using UnityEngine;

public class LineOfSightDetector : MonoBehaviour
{
    [SerializeField] private LayerMask m_playerLayerMask;
    [SerializeField] private float m_detectionRange = 10.0f;
    [SerializeField] private float m_detectionHeight = 1.5f;
    [SerializeField] private bool m_showDebugVisual = true;

    public GameObject PerformDetection(GameObject potentialTarget)
    {
        if (potentialTarget == null) return null;

        
        Vector3 rayOrigin = transform.position + Vector3.up * m_detectionHeight;
        
        // Cible le centre du torse plutôt que les pieds
        Vector3 targetPoint = potentialTarget.transform.position + Vector3.up * (m_detectionHeight * 0.5f);
        Vector3 directionToTarget = targetPoint - rayOrigin;
        float distanceToTarget = directionToTarget.magnitude;

        // Vérifie si la cible est trop éloignée
        if (distanceToTarget > m_detectionRange)
        {
            return null;
        }

        // Lance le rayon avec une direction normalisée
        if (Physics.Raycast(rayOrigin, directionToTarget.normalized, out RaycastHit hit, m_detectionRange, m_playerLayerMask))
        {
            if (hit.collider.gameObject == potentialTarget)
            {
                if (m_showDebugVisual && enabled)
                {
                    Debug.DrawLine(rayOrigin, hit.point, Color.green);
                }
                return hit.collider.gameObject;
            }
            else if (m_showDebugVisual && enabled)
            {
                // Dessine un rayon rouge si un obstacle bloque la vue
                Debug.DrawLine(rayOrigin, hit.point, Color.red);
            }
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position + Vector3.up * m_detectionHeight,
            m_detectionRange
        );
    }
}