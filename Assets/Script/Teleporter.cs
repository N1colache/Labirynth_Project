
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private Transform teleportPoint;
    [SerializeField] private string tagPlayer = "Player";
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagPlayer))
        {
            Rigidbody rb = other.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
                rb.position = teleportPoint.position;
                rb.transform.position = teleportPoint.position;
                rb.linearVelocity = Vector3.zero;
            }
            LandingRoll lr = other.GetComponentInParent<LandingRoll>();
            if (lr != null)
            {
                lr.ResetFallHeight();
            }
        }
    }
    
}
