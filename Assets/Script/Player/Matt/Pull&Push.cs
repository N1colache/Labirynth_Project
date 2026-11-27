using UnityEngine;
using UnityEngine.InputSystem;

public class PushPull : MonoBehaviour
{
    public float grabDistance = 2f;   // distance max pour attraper une caisse

    private Transform cam;
    private Rigidbody playerRb;
    private FixedJoint joint;         // l'attache entre la caisse et le joueur

    private void Start()
    {
        cam = Camera.main.transform;
        playerRb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (joint != null)
                Release();
            else
                TryGrab();
        }
    }

    private void TryGrab()
    {
        // On regarde ce qu'il y a devant la caméra
        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, grabDistance))
        {
            // Seulement les objets avec le tag "Pushable" et un Rigidbody
            if (hit.collider.CompareTag("Pushable") && hit.rigidbody != null)
            {
                joint = hit.rigidbody.gameObject.AddComponent<FixedJoint>();
                joint.connectedBody = playerRb;
            }
        }
    }

    private void Release()
    {
        Destroy(joint);
        joint = null;
    }
}
