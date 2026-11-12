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
        Wallrunning,
        Climbing,
        Vaulting,
        Crouching,
        Sliding,
        Air
    }

    [Header("References")]
    [SerializeField] private ControlerManager controler;
    [SerializeField] private Climbing climbingScript;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform orientation;
    [SerializeField] private PlayerStamina stamina;

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float slideSpeed = 12f;
    [SerializeField] private float wallRunSpeed = 8f;
    [SerializeField] private float climSpeed = 5f;
    [SerializeField] private float vaultSpeed = 10f;
    [SerializeField] private float crouchSpeed = 3.5f;
    public float maxYSpeed;

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
    public bool wallrunning;
    public bool climbing;
    public bool vaulting;
    public bool freeze;
    public bool unlimited;
    public bool restricted;
    

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI text_speed;
    [SerializeField] private TextMeshProUGUI text_state;

    // Directives d'entrées et déplacements
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;

    [SerializeField] private float coyoteTime = 0.12f;
     private float lastTimeGround;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;
        if (stamina == null) stamina = GetComponent<PlayerStamina>();
    }

    private void Update()
    {
        // Ground Check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        
        if(grounded)
            lastTimeGround = Time.time;
       

        MyInput();
        SpeedControl();
        StateHandler();
        UpdateUI();

        // Application du Drag selon le sol et l'état
        if ((state == MovementState.Walking || state == MovementState.Sprinting || state == MovementState.Crouching) )
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
        if (restricted || controler == null) return; 

        horizontalInput = controler.moveInput.x;
        verticalInput = controler.moveInput.y;
        bool canStillJump;
        if(grounded || Time.time - lastTimeGround < coyoteTime)
        {
             canStillJump = true;
        }
        else
        {
            canStillJump = false;
        }


        // Saut
        if (  readyToJump && canStillJump && controler.ConsumeJump()) 
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Crouch (Toggle)
        if (controler.crouchInput)
        {
            crouching = !crouching;

            if (crouching)
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }

            // Réinitialise l'entrée pour éviter un déclenchement en boucle
            controler.crouchInput = false; 
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
        else if (vaulting)
        {
            state = MovementState.Vaulting;
            desiredMoveSpeed = vaultSpeed;
        }
        else if (climbing)
        {
            state = MovementState.Climbing;
            desiredMoveSpeed = climSpeed;
        }
        else if (wallrunning)
        {
            state = MovementState.Wallrunning;
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
        else if (crouching)
        {
            state = MovementState.Crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        else if (grounded && controler.sprintInput && !stamina.IsExhausted)
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
        if (sliding || wallrunning || climbing || vaulting  || restricted || 
           (climbingScript != null && climbingScript.exitingWall)) 
            return;

        if (orientation == null) return;

        // Calcule la direction du mouvement par rapport à l'orientation
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Déplacement sur une pente
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0f)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        // Déplacement sur le sol
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // Déplacement dans l'air
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if (!wallrunning)
        {
            rb.useGravity = !OnSlope();
        }
    }

    private void SpeedControl()
    {
        
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
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

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2f * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2f * trajectoryHeight / gravity) + Mathf.Sqrt(2f * (displacementY - trajectoryHeight) / gravity));

        return velocityY + velocityXZ;
    }

    private void UpdateUI()
    {
        if (text_speed != null)
        {
            float displayedSpeed = OnSlope() ? rb.linearVelocity.magnitude : new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
            text_speed.SetText($"Speed: {Round(displayedSpeed, 1)} / {Round(moveSpeed, 1)}");
        }

        if (text_state != null)
        {
            text_state.SetText($"State: {state}");
        }
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, digits);
        return Mathf.Round(value * mult) / mult;
    }
}