using UnityEngine;


public class Sliding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private ControlerManager controler;

    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding Settings")]
    [SerializeField] private float maxSlideTime = 0.75f;
    [SerializeField] private float slideForce = 400f;
    public bool CanSlide { get{return pm.state == PlayerMovement.MovementState.Sprinting &&  !pm.restricted  && !pm.freeze && pm.grounded && !pm.sliding && controler.moveInput.magnitude > 0.1f; } }
    
    private float slideTimer;
    private Vector3 slideDirection;
    

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (pm == null) pm = GetComponent<PlayerMovement>();
        if (controler == null) controler = GetComponent<ControlerManager>();
        
    }

 

    private void FixedUpdate()
    {
        if (pm != null && pm.sliding)
        {
            SlideMovement();
        }
    }

    public void StartSlide()
    {
        float horizontalInput = controler.moveInput.x;
        float verticalInput = controler.moveInput.y;
        
        pm.sliding = true;

        slideDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;
        
        pm.Crouch();
            
        slideTimer = maxSlideTime;
    }

    private void SlideMovement()
    {
        if (orientation == null) return;
        
        if (!pm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(slideDirection * slideForce, ForceMode.Force);

            
            slideTimer -= Time.fixedDeltaTime;
        }
       
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(slideDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0f)
        {
            StopSlide();
        }
    }

    public void StopSlide()
    {
        pm.sliding = false;
        
        if (!pm.TryToUncrouch())
        { 
            pm.crouching = true;
        }
        
    }

    private void OnDisable()
    {
        if (pm != null && pm.sliding)
        {
            StopSlide();
        }
    }
}