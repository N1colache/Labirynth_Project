
using UnityEngine;
using System.Collections;

public class LedgeGrabbing : MonoBehaviour
{
   [Header("References")] 
   public PlayerMovement pm;
   public ControlerManager controler;
   public Transform orientation;
   public Transform cam;
   private Rigidbody rb;
   [SerializeField] private PlayerStamina stamina;

   [Header("Ledge Grabbing")] 
   public float moveToLedgeSpeed;
   public float maxLedgeGrabDistance;

   public float minTimeOnLedge;
   private float timeOnLedge;
   private Vector3 hangPoint;
   [SerializeField] private float hangOffset = 0.5f;
   public bool holding{get; private set;}
   [Header("Ledge Jumping")]
   public float ledgeJumpForwardForce;
   public float ledgeJumpUpwardForce;
   
   [Header("Ledge Detection")]
   public float ledgeDetectionLenght;
   public float ledgeSphereCastRadius;
   public LayerMask whatIsLedge;
   
   private Transform lastLedge;
   private Transform currentLedge;
   private Vector3 ledgeNormal;
   
   private RaycastHit ledgeHit;
   
   [Header("Exiting")]
   public bool exitingLedge {get; private set;}
   public float exitLedgeTime;
   private float exitLedgeTimer;
   
   [Header("Mantle")] 
   [SerializeField] private float mantleHeight;
   [SerializeField] private float mantleForward;
   [SerializeField] private float mantleDuration;
   [SerializeField] private float playerHalfHeight;
   [SerializeField] private LayerMask whatIsGround;
   
   [SerializeField] private float hangStaminaCost = 5f;
   [SerializeField] private float ledgeJumpStaminaCost = 10f;
   [SerializeField] private float shimmySpeed = 2f;
   private float ledgeTopY;
   [SerializeField] private float hangBelowTop = 1.2f;
   [SerializeField] private float maxPullTime = 0.5f;
   private float pullTimer;

   private void Awake()
   {
      if (rb == null) rb = GetComponent<Rigidbody>();
      if (stamina == null) stamina = GetComponent<PlayerStamina>();
   }
   private void Update()
   {
      LedgeDetection();
      SubStateMachine();
   }
   private void SubStateMachine()
   {
      float horizontalInput = controler.moveInput.x;
      
      // substate 1 - holding onto ledge
      if (holding)
      {
         FreezeRigidbodyOnLedge();
         if (!holding)
         {
            return;
         }
         timeOnLedge += Time.deltaTime;
         stamina.Drain(hangStaminaCost);

         if ( stamina.IsExhausted )
         {
            ExitLedgeHold();
            return;
         }

         if (timeOnLedge > minTimeOnLedge && controler.crouchInput)
         {
            controler.crouchInput = false;
            ExitLedgeHold();
            return;
         }

         if (controler.ConsumeJump() && stamina.TryConsume(ledgeJumpStaminaCost))
         {
             float lookDot = Vector3.Dot(cam.forward,-ledgeNormal);
            
                     if (lookDot > 0.5)
                     {
                        Vector3 origin = transform.position + (Vector3.up * mantleHeight + (-ledgeNormal) * mantleForward);
                        if (Physics.Raycast(origin, Vector3.down, out RaycastHit topHit, mantleHeight + 1f,
                               whatIsGround))
                        {
                           
                           ExitLedgeHold();
                           Vector3 target = topHit.point + Vector3.up * playerHalfHeight;
                           StartCoroutine(MantleRoutine(target));
                           
                        }
                        else
                        {
                           return;
                        }
                        
                     }
                     else
                     {
                        LedgeJump();
                     }
         }
         if (holding && horizontalInput != 0)
         {
            Vector3 wallRight = Vector3.Cross(Vector3.up, -ledgeNormal);
            Vector3 move = wallRight * horizontalInput * shimmySpeed * Time.deltaTime;
            Vector3 nextPoint = hangPoint + move;
            Vector3 checkPoint = nextPoint;
            checkPoint.y = ledgeTopY - 0.1f;
            
            if (Physics.Raycast(checkPoint, -ledgeNormal, hangOffset + 0.3f, whatIsLedge))
            {
               hangPoint = nextPoint;
               transform.position += move;
               rb.position = transform.position;
            }
         }
         // Substate 2 - Exiting Ledge
         
      }
      else if (exitingLedge)
      {
         if (exitLedgeTimer > 0) exitLedgeTimer -= Time.deltaTime;
         else exitingLedge = false;
      }
      
   }
   private void LedgeDetection()
   {
      bool ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.forward, out ledgeHit, ledgeDetectionLenght, whatIsLedge);
      
      if (!ledgeDetected)
      {
         return;
      }
      if (Mathf.Abs(ledgeHit.normal.y) > 0.3f) return;
      
      float distanceToLedge = Vector3.Distance(transform.position, ledgeHit.point);
      
      if(ledgeHit.transform == lastLedge) return;
      
      if(distanceToLedge < maxLedgeGrabDistance && !holding && controler.jumpHeld && !pm.IsBusy)  EnterLedgeHold();
   }
   private void LedgeJump()
   {
      ExitLedgeHold();

      Invoke(nameof(DelayedJumpForce), 0.05f);
   }
   private void DelayedJumpForce()
   {
      Vector3 forceToAdd = cam.forward * ledgeJumpForwardForce + orientation.up * ledgeJumpUpwardForce;
      rb.linearVelocity = Vector3.zero;
      rb.AddForce(forceToAdd, ForceMode.Impulse);
   }
   private void EnterLedgeHold()
   {
      pullTimer = 0f;
      
      holding = true;

      pm.unlimited = true;
      pm.restricted = true;
      
      currentLedge = ledgeHit.transform;
      ledgeNormal = ledgeHit.normal;
      ledgeNormal.y = 0f;
      ledgeNormal = ledgeNormal.normalized;
      hangPoint = ledgeHit.point + ledgeNormal * hangOffset;
      Vector3 topOrigin = ledgeHit.point + Vector3.up * mantleHeight + (-ledgeNormal) * 0.1f;

      if (Physics.Raycast(topOrigin, Vector3.down, out RaycastHit topHit, mantleHeight + 1f, whatIsGround))
      {
         ledgeTopY = topHit.point.y;
         hangPoint.y = ledgeTopY - hangBelowTop;
      }
      else
      {
         ledgeTopY = ledgeHit.point.y;
         hangPoint.y = transform.position.y;
      }
      lastLedge = ledgeHit.transform;
      
      rb.useGravity = false;
      rb.linearVelocity = Vector3.zero;
   }
   private void FreezeRigidbodyOnLedge()
   {
      if (currentLedge == null)
      {
         ExitLedgeHold();
         return;
      }
         
      rb.useGravity = false;
      
      Vector3 directionToLedge = hangPoint - transform.position;
      float distanceToLedge = Vector3.Distance(transform.position, hangPoint);
      float seuil = Mathf.Max(0.1f, moveToLedgeSpeed * Time.deltaTime);
      
      if (distanceToLedge > seuil)
      {
        
         rb.linearVelocity = (directionToLedge.normalized * moveToLedgeSpeed);
         pullTimer += Time.deltaTime;
         if (pullTimer > maxPullTime)
         {
            ExitLedgeHold();
            return;
         }

      }
      else
      {
         transform.position = hangPoint;
         rb.position = transform.position;
         rb.linearVelocity = Vector3.zero;
         
            if (!pm.freeze)
            {
               pm.freeze = true;
            }
            if (pm.unlimited)
            {
               pm.unlimited = false; 
            }
         
      }
      
      if(distanceToLedge > maxLedgeGrabDistance) ExitLedgeHold();
   }
   private void ExitLedgeHold()
   {
      exitingLedge = true;
      exitLedgeTimer = exitLedgeTime;
      
      holding = false;
      timeOnLedge = 0f;
      
      pm.restricted = false;
      pm.freeze = false;
      pm.unlimited = false;
      
      rb.useGravity = true;
      if (rb.isKinematic == false)
      {
         rb.linearVelocity = Vector3.zero;
      }
      StopAllCoroutines();
      Invoke(nameof(ResetLastLedge), 1f);
   }
   private void ResetLastLedge()
   {
      lastLedge = null;
   }
   private IEnumerator MantleRoutine(Vector3 target)
   {
      rb.isKinematic = true;
      Vector3 start = transform.position;
      float t = 0f;

      while (t < 1f)
      {
         t += Time.deltaTime / mantleDuration;
         transform.position = Vector3.Lerp(start, target, t);
         yield return null;
      }

      rb.isKinematic = false;
      rb.linearVelocity = Vector3.zero;
   }
   
   
 

}
