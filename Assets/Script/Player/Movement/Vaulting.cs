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

    private  List<Vector3> stridePoints = new List<Vector3>();
    [SerializeField] private float maxDistance = 2f;
    [SerializeField] private float strideHopDuration = 2f;
    [SerializeField] private float strideClearance = 2f;
    [SerializeField] private int numberMaxObstacle = 5;
    private Vector3 top; 
    private Vector3 destination;
    private bool obstacleLow;
    private bool obstacleHigh;
    private void Start()
    {
        if (pm == null) pm = GetComponent<PlayerMovement>();
        if (cm == null) cm = GetComponent<ControlerManager>();
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pm.grounded && cm.moveInput.y > 0 && cm.jumpHeld && !pm.vaulting)
        {
            obstacleLow = Physics.Raycast(transform.position + Vector3.up * lowerRayHeight, orientation.forward, out RaycastHit lowerHit, distanceFrontPlayer , whatIsVaulting);
            obstacleHigh =  Physics.Raycast(transform.position + Vector3.up * topRayHeight, orientation.forward, distanceFrontPlayer , whatIsVaulting);

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
                stridePoints.Clear();
                Vector3 currentWall = top;
                stridePoints.Add(top);
                bool sprint = pm.state == PlayerMovement.MovementState.Sprinting;
                if(sprint)
                {
                     for(int i = 0; i <numberMaxObstacle; i++)
                     {
                         bool touchWall = Physics.Raycast(currentWall + (Vector3.down * 0.2f), 	orientation.forward, out RaycastHit nextHit, maxDistance, whatIsVaulting);
                         if (!touchWall) break;
                         if (Physics.Raycast(nextHit.point + (orientation.forward * 0.2f) + (Vector3.up * 2), Vector3.down, out RaycastHit nextTopHit, 2.5f, whatIsVaulting))
                         {
                             stridePoints.Add(nextTopHit.point);
                             currentWall = nextTopHit.point;
                         }
                         else 
                         { 
                             break;
                         }
                                        
                     }
                }
               
                int wallCount =  stridePoints.Count;
                //Debug.Log(stridePoints.Count);
                
                bool groundBehind = Physics.Raycast(currentWall + (orientation.forward * lengthObstacle) + (Vector3.up * 2), Vector3.down, out RaycastHit backHit, 4f, whatIsGround | whatIsVaulting);
                
                
                if (groundBehind && backHit.point.y < currentWall.y - 0.3f)
                {
                    
                    destination = backHit.point;
                    stridePoints.Add(backHit.point);
                    
                }
                else
                {
                    
                    destination = top;
                    
                }
                
                Vector3 summit = top + Vector3.up * (playerHalfHeight +  clearance);
                Vector3 end = destination + Vector3.up * playerHalfHeight;
                
                if (wallCount >= 2)
                {
                    StartCoroutine(StrideRoutine(stridePoints));
                }
                else
                {
                     StartCoroutine(VaultRoutine(transform.position, summit, end));
                }
                
               
            }
            
            Debug.DrawRay(transform.position + Vector3.up * lowerRayHeight, orientation.forward * distanceFrontPlayer, Color.red);
            Debug.DrawRay(transform.position + Vector3.up * topRayHeight, orientation.forward * distanceFrontPlayer, Color.red);
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
        
        Vector3 currentPosition = transform.position;
        bool stop = false;
        
        foreach (Vector3 point in stride)
        {
            Vector3 end = point + Vector3.up * playerHalfHeight;
            Vector3 summit = Vector3.Lerp(currentPosition, end, 0.5f ) + Vector3.up *  strideClearance;
            yield return StartCoroutine(ArcMove(currentPosition, summit, end, strideHopDuration));

            currentPosition = end;
            if (cm.moveInput.y <= 0)
            {
                stop = true;
                break;
            }
        }
        
        rb.isKinematic = false;
        if (stop)
        {
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
             rb.linearVelocity = orientation.forward * exitSpeed;
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
}
