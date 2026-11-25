using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Vaulting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private ControlerManager cm;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform orientation;
    [SerializeField] private PlayerStamina stamina;
    [SerializeField] private Balancing balancing;

    [Header("VaultingSettings")] 
    [SerializeField] private float distanceFrontPlayer;
    [SerializeField] private float lowerRayHeight;
    [SerializeField] private float topRayHeight;
    [SerializeField] private LayerMask whatIsVaulting;
    [SerializeField] private LayerMask whatIsGround;

    
    [Header("obstacleSettings")] 
    [SerializeField] private float lengthObstacle;
    [SerializeField] private float vaultDuration;
    [SerializeField] private float exitSpeed;
    [SerializeField] private float playerHalfHeight;
    [SerializeField] private float clearance;
    [SerializeField] private float singlePassage = 5f;
    [SerializeField] private float strideJump = 5f;

    [Header("StrideSettings")] 
    private  List<Vector3> StridePoints = new List<Vector3>();
    [SerializeField] private float maxDistance = 2f;
    [SerializeField] private float strideHopDuration = 2f;
    [SerializeField] private float strideClearance = 2f;
    [SerializeField] private int numberMaxObstacle = 5;

    public float PlayerHalfHeight
    {
        get { return playerHalfHeight; }
    }

    private Vector3 top; 
    private Vector3 destination;
    private bool obstacleLow;
    private bool obstacleHigh;
    public bool perched{ get; private set; }
    private void Start()
    {
        if (pm == null) pm = GetComponent<PlayerMovement>();
        if (cm == null) cm = GetComponent<ControlerManager>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (stamina == null) stamina = GetComponent<PlayerStamina>();
        if (balancing == null) balancing = GetComponent<Balancing>();
    }
    
    void Update()
    {
        
        if (perched && cm.crouchInput)
        {
            perched = false;
            pm.restricted = false;
            rb.isKinematic = false;
            cm.crouchInput = false;
            return;
        }

        if (perched && balancing.OnBeam && cm.moveInput.y > 0)
        {
            perched = false;
            pm.restricted = false;
            rb.isKinematic = false;
            return;
        }
        if (pm.grounded && cm.moveInput.y > 0 && cm.jumpHeld && !pm.vaulting)
        {
            bool onWall = Physics.Raycast( transform.position, Vector3.down,  out RaycastHit feetHit, playerHalfHeight  +  0.3f, whatIsVaulting );
            obstacleLow = Physics.Raycast(transform.position + Vector3.up * lowerRayHeight, orientation.forward, out RaycastHit lowerHit, distanceFrontPlayer , whatIsVaulting);
            obstacleHigh =  Physics.Raycast(transform.position + Vector3.up * topRayHeight, orientation.forward, distanceFrontPlayer , whatIsVaulting);
            bool sprint = pm.state == PlayerMovement.MovementState.Sprinting;
            
            if (onWall && (sprint || balancing.OnBeam && cm.sprintInput && !stamina.IsExhausted))
            {
                StridePoints.Clear();
                Vector3 lastWall  = FindNextWalls(feetHit.point);
                bool groundAfter = Physics.Raycast(lastWall + (orientation.forward * lengthObstacle) + (Vector3.up * 2),
                    Vector3.down, out RaycastHit endHit, 4f, whatIsGround | whatIsVaulting);
                if (groundAfter && endHit.point.y < lastWall.y - 0.3f)
                {
                    StridePoints.Add(endHit.point);
                }

                if (StridePoints.Count >  0)
                {
                    StartCoroutine(StrideRoutine(StridePoints));
                }
                return;
            }
                
            if (obstacleLow && !obstacleHigh)
            {
                if (Physics.Raycast(lowerHit.point + (orientation.forward * 0.2f) + (Vector3.up * 2),Vector3.down , out RaycastHit obstacleHit,2.5f , whatIsVaulting))
                {
                    top = obstacleHit.point;
                }
                else
                {
                    return;
                }
                StridePoints.Clear();
                Vector3 currentWall = top;
                StridePoints.Add(top);
               
                if(sprint)
                {
                    currentWall = FindNextWalls(top);
                }
               
                int wallCount =  StridePoints.Count;
                
                bool groundBehind = Physics.Raycast(currentWall + (orientation.forward * lengthObstacle) + (Vector3.up * 2), Vector3.down, out RaycastHit backHit, 4f, whatIsGround | whatIsVaulting);
                
                if (groundBehind && backHit.point.y < currentWall.y - 0.3f)
                {
                    destination = backHit.point;
                    StridePoints.Add(backHit.point);
                }
                else
                {
                    destination = top;
                }
                
                Vector3 summit = top + Vector3.up * (playerHalfHeight +  clearance);
                Vector3 end = destination + Vector3.up * playerHalfHeight;
                
                if (wallCount >= 2)
                {
                    StartCoroutine(StrideRoutine(StridePoints));
                }
                else
                {
                    if (stamina.TryConsume(singlePassage))
                    {
                        StartCoroutine(VaultRoutine(transform.position, summit, end));
                    }
                }
            }
        }
        else
        {
            return;
        }
        
    }

    private IEnumerator VaultRoutine(Vector3 start,Vector3 summit, Vector3 end)
    {
        rb.isKinematic = true;
        pm.vaulting = true;
        
        yield return StartCoroutine(ArcMove(start, summit, end, vaultDuration));
        
        rb.isKinematic = false;
        rb.linearVelocity = orientation.forward * exitSpeed;
        pm.vaulting = false;
    }

    private IEnumerator StrideRoutine(List<Vector3> stride)
    {
        rb.isKinematic = true;
        pm.vaulting = true;
        perched = false;
        pm.restricted = false;
        
        Vector3 currentPosition = transform.position;
        bool stop = false;
        int numberJump = 0;
        
        foreach (Vector3 point in stride)
        {
            if (point != stride[stride.Count -1] && !stamina.TryConsume(strideJump))
            {
                stop = true;
                break;
            }
            
            Vector3 end = point + Vector3.up * playerHalfHeight;
            Vector3 summit = Vector3.Lerp(currentPosition, end, 0.5f ) + Vector3.up *  strideClearance;
            yield return StartCoroutine(ArcMove(currentPosition, summit, end, strideHopDuration));
            numberJump++;

            currentPosition = end;
            if (point != stride[stride.Count - 1] && cm.moveInput.y <= 0)
            {
                stop = true;
                break;
            }
        }
        
        
        if (stop && numberJump > 0)
        {
            pm.restricted = true;
            perched = true;
        }
        else 
        {
            rb.isKinematic = false;
            if (stop)
            {
                rb.linearVelocity = Vector3.zero;
            }
            else
            {
                rb.linearVelocity = orientation.forward * exitSpeed;
            }
        }
       
        pm.vaulting = false;
      
    }
    private IEnumerator ArcMove(Vector3 start,Vector3 summit, Vector3 end, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime  / duration;
            Vector3 lerp1 = Vector3.Lerp(start, summit, t);
            Vector3 lerp2 = Vector3.Lerp(summit,end , t);
            transform.position = Vector3.Lerp(lerp1, lerp2, t);
            yield return null;
        }
    }

    private Vector3 FindNextWalls(Vector3 startPoint)
    {
        Vector3 currentWall = startPoint;
        for(int i = 0; i <numberMaxObstacle; i++)
        {
            bool touchWall = Physics.Raycast(currentWall + (Vector3.down * 0.2f), 	orientation.forward, out RaycastHit nextHit, maxDistance, whatIsVaulting);
            if (!touchWall) break;
            if (Physics.Raycast(nextHit.point + (orientation.forward * 0.2f) + (Vector3.up * 2), Vector3.down, out RaycastHit nextTopHit, 2.5f, whatIsVaulting))
            {
                StridePoints.Add(nextTopHit.point);
                currentWall = nextTopHit.point;
            }
            else 
            { 
                break;
            }
                                        
        }
        return currentWall;
    }
}
