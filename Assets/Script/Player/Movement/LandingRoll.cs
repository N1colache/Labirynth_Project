using UnityEngine;

public class LandingRoll : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private ControlerManager cm;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform orientation;
    [SerializeField] private CameraEffects cam;
    
    [Header("RollSettings")]
    [SerializeField] private float minFallHeight = 4f;
    [SerializeField] private float lapsTimeCrouchInput = 0.3f;
    [SerializeField] private float rollTime = 0.3f;
    [SerializeField] private float rollStrength = 0.3f;
    
    [Header("HardLanding")]
    [SerializeField] private float timeStuck = 0.3f;
    [SerializeField] private float hardLandingTime = 0.6f;
    private bool wasGrounded = false;
    private float heightPoint;
    
   
    
    void Start()
    {
        if (pm == null) pm = GetComponent<PlayerMovement>();
        if (cm == null) cm = GetComponent<ControlerManager>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (cam == null) cam = GetComponent<CameraEffects>();
    }
    
    void Update()
    {
        if (wasGrounded && !pm.grounded)
        {
           heightPoint = transform.position.y; 
        }

        if (!pm.grounded)
        {
            if (transform.position.y > heightPoint)
            {
                heightPoint = transform.position.y;
            }
        }

        if (pm.grounded && !wasGrounded)
        {
            float heinghtfall =  heightPoint - transform.position.y;
            
            if (heinghtfall >= minFallHeight)
            {
                if (cm != null && Time.time - cm.LastCrouchPressTime < lapsTimeCrouchInput)
                {  
                    Rolling();
                }
                else 
                {
                    HardLanding();
                }
            }
            
        }
        wasGrounded = pm.grounded;
    }
    
    private void Rolling()
    {
        pm.Crouch();
        rb.AddForce(orientation.forward * rollStrength, ForceMode.Impulse);
        cam.DoRotate(rollTime);
        
        Invoke(nameof(EndRolling), rollTime);
    }

    private void EndRolling()
    {
        if (!pm.TryToUncrouch())
        {
            pm.crouching = true;
        }
    }
    
    private void HardLanding()
    {
        pm.restricted = true;
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        cam.DoShake();
        Invoke(nameof(StopHardLanding), hardLandingTime);
    }
    
    private void StopHardLanding()
    {
        pm.restricted = false;
    }
}
