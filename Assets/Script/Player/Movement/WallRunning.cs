using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("WallRunning Settings")]
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float wallRunForce = 200f;
    [SerializeField] private float wallJumpForce = 12f;
    [SerializeField] private float wallJumpSideForce = 12f;
    [SerializeField] private float wallClimbSpeed = 5f;
    [SerializeField] private float maxWallRunTime = 2.5f;
    private float wallRunTimer;

    [Header("Detection")]
    [SerializeField] private float wallCheckDistance = 0.8f;
    [SerializeField] private float minJumpHeight = 1.2f;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;
    private Vector3 lastWallNormal;
    private bool hasWall;

    [Header("Exiting Wall")]
    [SerializeField] private float exitWallTime = 0.2f;
    private float exitWallTimer;
    private bool exitingWall;

    [Header("Gravity")]
    [SerializeField] private bool useGravity = true;
    [SerializeField] private float gravityCounterForce = 15f;

    [Header("Camera Effects")]
    [SerializeField] private float wallRunFov = 90f;
    [SerializeField] private float normalFov = 80f;
    [SerializeField] private float tiltAngle = 10f;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private CameraEffects cam;
    [SerializeField] private ControlerManager controler;
    [SerializeField] private LedgeGrabbing lg;
    [SerializeField] private PlayerStamina stamina;

    private Rigidbody rb;
    private PlayerMovement pm;
    private float horizontalInput;
    private float verticalInput;

    [SerializeField] private float wallRunStaminaCost = 15f;
    [SerializeField] private float wallJumpStaminaCost = 10f;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (pm == null) pm = GetComponent<PlayerMovement>();
        if (lg == null) lg = GetComponent<LedgeGrabbing>();
        if (controler == null) controler = GetComponent<ControlerManager>();
        if (stamina == null) stamina = GetComponent<PlayerStamina>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
        
        if(pm.grounded)  hasWall = false;
    }

    private void FixedUpdate()
    {
        if (pm != null && pm.wallRunning)
        {
            WallRunMovement();
        }
    }

    private void CheckForWall()
    {
        if (orientation == null) return;

        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        if (controler == null) return;

        horizontalInput = controler.moveInput.x;
        verticalInput = controler.moveInput.y;
        Vector3 currentWallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        bool sameWall = false;
        if(hasWall && Vector3.Dot(currentWallNormal, lastWallNormal) > 0.9) sameWall = true;
        
        if ((wallLeft || wallRight) && verticalInput > 0f && AboveGround() && !exitingWall  && !stamina.IsExhausted && !sameWall && !pm.IsBusy)
        {
            if (!pm.wallRunning)
            {
                StartWallRun();
            }

            if (wallRunTimer > 0f)
            {
                wallRunTimer -= Time.deltaTime;
            }
            
            if (wallRunTimer <= 0f && pm.wallRunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
                
                Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
                lastWallNormal = wallNormal;
                hasWall = true;
                rb.AddForce(wallNormal * wallJumpSideForce, ForceMode.Impulse);
            }
            
            if (!(lg != null && (lg.holding || lg.exitingLedge)) && controler.ConsumeJump() && stamina.TryConsume(wallJumpStaminaCost))
            {
                WallJump();
            }
        }
        
        else if (exitingWall)
        {
            if (pm.wallRunning)
            {
                StopWallRun();
            }

            if (exitWallTimer > 0f)
            {
                exitWallTimer -= Time.deltaTime;
            }
            else
            {
                exitingWall = false;
            }
        }
        
        else
        {
            if (pm.wallRunning)
            {
                StopWallRun();
            }
        }
    }

    private void StartWallRun()
    {
        pm.wallRunning = true;
        wallRunTimer = maxWallRunTime;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (cam != null)
        {
            cam.DoFov(wallRunFov);
            if (wallLeft) cam.DoTilt(-tiltAngle);
            if (wallRight) cam.DoTilt(tiltAngle);
        }
    }

    private void WallRunMovement()
    {
        rb.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        
        if ((orientation.forward - wallForward).sqrMagnitude > (orientation.forward - -wallForward).sqrMagnitude)
        {
            wallForward = -wallForward;
        }
        
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
        stamina.Drain(wallRunStaminaCost);
        
        if (!(wallLeft && horizontalInput > 0f) && !(wallRight && horizontalInput < 0f))
        {
            rb.AddForce(-wallNormal * 100f, ForceMode.Force);
        }
        
        if (useGravity)
        {
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        }
    }

    private void StopWallRun()
    {
        pm.wallRunning = false;

        if (cam != null)
        {
           cam.DoFov(normalFov); 
           cam.DoTilt(0f);
        }
    }

    private void WallJump()
    {
        if (lg != null && (lg.holding || lg.exitingLedge)) return;

        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        lastWallNormal = wallNormal;
        hasWall = true;
        Vector3 forceToApply = transform.up * wallJumpForce + wallNormal * wallJumpSideForce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
        
    }

    private void OnDisable()
    {
        if (pm != null && pm.wallRunning)
        {
            StopWallRun();
        }
    }
}