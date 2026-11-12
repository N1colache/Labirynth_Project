using UnityEngine;

public class Climbing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private LedgeGrabbing lg;
    [SerializeField] private ControlerManager controler;

    [Header("Climbing Settings")]
    [SerializeField] private float climSpeed = 5f;
    [SerializeField] private float maxClimTime = 2f;
    private float climTimer;
    public bool climbing { get; private set; }

    [Header("Climb Jumping")]
    [SerializeField] private float climJumpUpForce = 7f;
    [SerializeField] private float climbJumpBackForce = 12f;
    [SerializeField] private int climbJumps = 1;
    private int climbJumpLeft;

    [Header("Detection")]
    [SerializeField] private float detectionLenght = 0.7f;
    [SerializeField] private float sphereCastRadius = 0.3f;
    [SerializeField] private float maxWallLookAngle = 30f;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    [SerializeField] private float minWallNormalAngleChange = 15f;

    [Header("Exiting")]
    public bool exitingWall;
    [SerializeField] private float exitingWallTime = 0.2f;
    private float exitingWallTimer;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (pm == null) pm = GetComponent<PlayerMovement>();
        if (lg == null) lg = GetComponent<LedgeGrabbing>();
    }

    private void Update()
    {
        WallCheck();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (climbing && !exitingWall)
        {
            ClimbingMovement();
        }
    }

    private void StateMachine()
    {
        // 1. Suspension à un rebord (LedgeGrabbing prioritaire)
        if (lg != null && (lg.holding || lg.exitingLedge))
        {
            if (climbing) StopClimbing();
            return;
        }

        // 2. En phase d'éjection (Post-ClimbJump)
        if (exitingWall)
        {
            if (climbing) StopClimbing();

            if (exitingWallTimer > 0f)
            {
                exitingWallTimer -= Time.deltaTime;
            }
            else
            {
                exitingWall = false;
            }

            return;
        }

        // 3. Escalade active
        bool canClimb = wallFront && controler != null && controler.moveInputBool && wallLookAngle < maxWallLookAngle;

        if (canClimb && !exitingWall)
        {
            if (!climbing && climTimer > 0f)
            {
                StartClimbing();
            }

            if (climTimer > 0f)
            {
                climTimer -= Time.deltaTime;
            }

            if (climTimer <= 0f)
            {
                StopClimbing();
            }
        }
        else if (climbing)
        {
            StopClimbing();
        }

      
        if (wallFront && controler != null && controler.ConsumeJump() && climbJumpLeft > 0)
        {
            ClimbJump();
        }
    }

    private void WallCheck()
    {
        if (orientation == null) return;

        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLenght, whatIsWall);

        if (!wallFront) return;

        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool isNewWall = frontWallHit.transform != lastWall || Vector3.Angle(lastWallNormal, frontWallHit.normal) > minWallNormalAngleChange;

        // Réinitialise la jauge d'escalade lors de l'accès à un nouveau mur ou si le joueur touche le sol
        if (isNewWall || (pm != null && pm.grounded))
        {
            climTimer = maxClimTime;
            climbJumpLeft = climbJumps;
        }
    }

    private void StartClimbing()
    {
        climbing = true;
        if (pm != null) pm.climbing = true;

        rb.useGravity = false;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    private void ClimbingMovement()
    {
        // Déplacement vertical contrôlé sur le mur
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, climSpeed, rb.linearVelocity.z);
    }

    private void StopClimbing()
    {
        climbing = false;
        if (pm != null) pm.climbing = false;

        rb.useGravity = true;
    }

    private void ClimbJump()
    {
        if ((pm != null && pm.grounded) || (lg != null && (lg.holding || lg.exitingLedge)))
            return;

        exitingWall = true;
        exitingWallTimer = exitingWallTime;

        Vector3 forceToApply = transform.up * climJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpLeft--;
    }
}