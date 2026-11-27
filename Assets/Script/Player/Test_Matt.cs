
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_Matt : MonoBehaviour
{
  [Header("Reference")] 
  public Transform cam;
  public Transform attackPoint;
  public GameObject objectToThrow;
  
  [Header("Settings")] 
  public int totalThrows;
  public float throwCooldown;
  
  [Header("Throwing")] 
  public float throwForce;
  public float throwUpwardForce;
  
  [Header("Aim")]
  // Décoche le layer du joueur, pour que le rayon ne touche pas le corps
  public LayerMask aimLayers = ~0;
  public float maxAimDistance = 500f;

  private bool readyToThrow;

  private void Start()
  {
    readyToThrow = true;
    
    // Si "cam" n'est pas rempli, on prend la caméra principale
    if (cam == null && Camera.main != null)
      cam = Camera.main.transform;
  }

  void Update()
  {
    if (Mouse.current != null 
        && Mouse.current.leftButton.wasPressedThisFrame 
        && readyToThrow
        && totalThrows > 0)
    {
      Throw();
    }
  }

  private void Throw()
  {
    readyToThrow = false;
    
    // Point visé : ce qu'il y a au centre de l'écran (sous le viseur)
    Vector3 targetPoint = cam.position + cam.forward * maxAimDistance;
 
    if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit,
          maxAimDistance, aimLayers, QueryTriggerInteraction.Ignore))
    {
      targetPoint = hit.point;
    }
 
    // Direction depuis le point de lancer vers le point visé
    Vector3 forceDirection = (targetPoint - attackPoint.position).normalized;
    
    
    
    
    // instantiate object to throw (orienté vers la cible)
    GameObject projectile = Instantiate(objectToThrow, attackPoint.position,
      Quaternion.LookRotation(forceDirection));
    
    // Le projectile ne doit pas se cogner contre le joueur en sortant
    Collider projectileCol = projectile.GetComponent<Collider>();
    if (projectileCol != null)
    {
      foreach (Collider playerCol in transform.root.GetComponentsInChildren<Collider>())
        Physics.IgnoreCollision(projectileCol, playerCol);
    }
    
    //Get riRigidbody component
    Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
    
    //Calculate direction
    projectileRb.linearVelocity = transform.forward * throwForce;
    
    // Add force
    Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;
    
    projectileRb.AddForce(forceToAdd, ForceMode.Impulse);
    
    totalThrows--;
    
    //implement ThrowCooldown
    Invoke(nameof(ResetThrow), throwCooldown);
  }

  private void ResetThrow()
  {
    readyToThrow = true;
  }
}
