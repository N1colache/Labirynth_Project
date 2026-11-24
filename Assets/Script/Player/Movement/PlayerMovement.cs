using System.Collections;
using TMPro;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    public enum MovementState
    {
        Freeze,
        Unlimited,
        Walking,
        Sprinting,
        WallRunning,
        Climbing,
        Vaulting,
        Crouching,
        Sliding,
        Balancing,
        Air,
        Swinging
    }

    [Header("References")]
    [SerializeField] private ControlerManager cm;
    [SerializeField] private Climbing climbingScript;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform orientation;
    [SerializeField] private PlayerStamina stamina;
    [SerializeField] private Balancing balancing;
    [SerializeField] private Sliding slidingScript;

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float slideSpeed = 12f;
    [SerializeField] private float wallRunSpeed = 8f;
    [SerializeField] private float climbSpeed = 5f;
    [SerializeField] private float vaultSpeed = 10f;
    [SerializeField] private float crouchSpeed = 3.5f;
    [SerializeField] private float balancingSpeed = 2.5f;
    public float maxYSpeed;
    public float speedMultiplier = 1f;
    public float CurrentMoveSpeed{get{return moveSpeed * speedMultiplier;}}

    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    [Header("Stamina")] 
    [SerializeField] private float sprintStaminaCost = 12f;
    
    
    [Header("Speed Multipliers")] 
    [SerializeField] private float slopeIncreaseMultiplier = 1.5f;
    [SerializeField] private float slideSpeedChangeFactor = 4f;
    [SerializeField] private float sprintSpeedChangeFactor = 2f;
    [SerializeField] private float wallRunSpeedChangeFactor = 2f;
    private float speedIncreaseMultiplier = 1f;

    [Header("Jumping & Friction")] 
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private float airMultiplier = 0.4f;
    private bool readyToJump = true;

    [Header("Crouching")] 
    [SerializeField] private float crouchYScale = 0.5f;
    private float startYScale;

    [Header("Slope & Ground Checking")] 
    [SerializeField] private float maxSlopeAngle = 40f;
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float sphereCastRadius = 0.4f;
    public bool grounded;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("State Tracking")]
    public MovementState state;
    public MovementState lastState;
    private bool keepMomentum;

    [Header("Action Flags")]
    public bool sliding;
    public bool crouching;
    public bool wallRunning;
    public bool climbing;
    public bool vaulting;
    public bool swinging;
    public bool freeze;
    public bool unlimited;
    public bool restricted;
    public bool IsBusy
    {
        get { return (swinging || vaulting || rb.isKinematic); }
    }


    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textSpeed;
    [SerializeField] private TextMeshProUGUI textState;

    // Directives d'entrées et déplacements
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;

    [SerializeField] private float coyoteTime = 0.12f;
     private float lastTimeGround;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (balancing == null) balancing = GetComponent<Balancing>();
        if (slidingScript == null) slidingScript = GetComponent<Sliding>();
        
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;
        if (stamina == null) stamina = GetComponent<PlayerStamina>();
    }
    private void Update()
    {
        // Ground Check
        grounded = Physics.SphereCast(transform.position, sphereCastRadius,Vector3.down,out RaycastHit groundHit,playerHeight  / 2 +0.3f - sphereCastRadius ,whatIsGround);
        
        if(grounded)
            lastTimeGround = Time.time;
        
        MyInput();
        SpeedControl();
        StateHandler();
        UpdateUI();

        // Application du Drag selon le sol et l'état
        if (state == MovementState.Walking || state == MovementState.Sprinting || state == MovementState.Crouching || state == MovementState.Balancing )
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0f;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        if (restricted || cm == null) return; 

        horizontalInput = cm.moveInput.x;
        verticalInput = cm.moveInput.y;
        bool canStillJump;
        if(grounded || Time.time - lastTimeGround < coyoteTime)
        {
             canStillJump = true;
        }
        else
        {
            canStillJump = false;
        }

        if (crouching && grounded && cm.ConsumeJump())
        {
            if(TryToUncrouch())
            {
                crouching = false;
                
            }
        }


        // Saut
        if (  !rb.isKinematic && readyToJump && canStillJump && !crouching && cm.ConsumeJump() ) 
        {
            readyToJump = false;
            if (sliding)
            {
                slidingScript.StopSlide();
            }
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Crouch (Toggle)
        if (cm.crouchInput)
        {

            if (grounded)
            {
                if (sliding)
                {
                    slidingScript.StopSlide();
                }
                else if(slidingScript.CanSlide)
                {
                    slidingScript.StartSlide(); 
                }
                else if (!crouching) 
                { 
                    crouching = true;
                    Crouch();
                }
                else
                {
                    if(TryToUncrouch())
                    { 
                        crouching = false;
                    }
                }
            }
            
            cm.crouchInput = false; 
            
        }
    }

    private void StateHandler()
    {
        if (freeze)
        {
            state = MovementState.Freeze;
            rb.linearVelocity = Vector3.zero;
            desiredMoveSpeed = 0f;
        }
        else if (unlimited)
        {
            state = MovementState.Unlimited;
            desiredMoveSpeed = 999f;
            return;
        }
        else if (swinging)
        {
            state = MovementState.Swinging;
        }
        else if (vaulting)
        {
            state = MovementState.Vaulting;
            desiredMoveSpeed = vaultSpeed;
        }
        else if (climbing)
        {
            state = MovementState.Climbing;
            desiredMoveSpeed = climbSpeed;
        }
        else if (wallRunning)
        {
            state = MovementState.WallRunning;
            desiredMoveSpeed = wallRunSpeed;
            speedIncreaseMultiplier = wallRunSpeedChangeFactor;
        }
        else if (sliding)
        {
            state = MovementState.Sliding;

            if (OnSlope() && rb.linearVelocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
                keepMomentum = true;
                speedIncreaseMultiplier = slideSpeedChangeFactor;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }
        else if (balancing.IsBalancing)
        {
            state = MovementState.Balancing;
            desiredMoveSpeed = balancingSpeed;
        }
        else if (crouching && grounded)
        {
            state = MovementState.Crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        else if (grounded && cm.sprintInput && !stamina.IsExhausted)
        {
            state = MovementState.Sprinting;
            desiredMoveSpeed = sprintSpeed;
            speedIncreaseMultiplier = sprintSpeedChangeFactor;
            if (verticalInput != 0 || horizontalInput != 0)
            {
                stamina.Drain(sprintStaminaCost);
            }
        }
        else if (grounded)
        {
            state = MovementState.Walking;
            desiredMoveSpeed = walkSpeed;
        }
        else 
        {
            state = MovementState.Air;
            desiredMoveSpeed = (moveSpeed < sprintSpeed) ? walkSpeed : sprintSpeed;
        }

        // Vérification de changement de vitesse cible
        if (desiredMoveSpeed != lastDesiredMoveSpeed)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;

        if (Mathf.Abs(desiredMoveSpeed - moveSpeed) < 0.1f) keepMomentum = false;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0f;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;
        float boostFactor = speedIncreaseMultiplier;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1f + (slopeAngle / 90f);
                time += Time.deltaTime * boostFactor * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
            {
                time += Time.deltaTime * boostFactor;
            }

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedIncreaseMultiplier = 1f;
        keepMomentum = false;
    }

    private void MovePlayer()
    {
        if (sliding || wallRunning || climbing || vaulting  || restricted || 
           (climbingScript != null && climbingScript.ExitingWall) || swinging) 
            return;
        if (orientation == null) return;
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (balancing.IsBalancing)
        {
            float facing = Vector3.Dot(orientation.forward, balancing.BeamDirection);
            Vector3 walkDirection = facing >= 0 ? balancing.BeamDirection  : - balancing.BeamDirection;
            moveDirection = walkDirection * verticalInput;
        }
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * CurrentMoveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0f)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * CurrentMoveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * CurrentMoveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
        if (!wallRunning)
        {
            rb.useGravity = !OnSlope();
        }
    }

    private void SpeedControl()
    {
        if(unlimited) return;
        
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > CurrentMoveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * CurrentMoveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVel.magnitude > CurrentMoveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * CurrentMoveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }

        if (maxYSpeed != 0f && rb.linearVelocity.y > maxYSpeed)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxYSpeed, rb.linearVelocity.z);
        }
    }

    private void Jump()
    {
        exitingSlope = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        lastTimeGround = -999f;
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

  

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f, whatIsGround))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0f;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
    
    private void UpdateUI()
    {
        if (textSpeed != null)
        {
            float displayedSpeed = OnSlope() ? rb.linearVelocity.magnitude : new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
            textSpeed.SetText($"Speed: {Round(displayedSpeed, 1)} / {Round(CurrentMoveSpeed, 1)}");
        }

        if (textState != null)
        {
            textState.SetText($"State: {state}");
        }
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, digits);
        return Mathf.Round(value * mult) / mult;
    }

    public void Crouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }
    
    public bool TryToUncrouch()
    {
        if (Physics.Raycast(transform.position, Vector3.up, 0.75f * playerHeight, whatIsGround))
        {
            return false;
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x,startYScale,transform.localScale.z);
            return true;
        }
    }
}