using UnityEngine;

public class ProjectileAddon : MonoBehaviour
{
    private Rigidbody rb;
    private bool targetHit;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void onCollisionEnter(Collision collision)
    {
        if (targetHit)
            return;
        else
        {
            targetHit = true;
        }
        
        // Objet arrete de bouger/colle
        rb.isKinematic = true;
        
        //object suit la cible si elle bouge
        transform.SetParent(collision.transform);
    }
}
