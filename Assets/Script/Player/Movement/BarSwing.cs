
using UnityEngine;

public class BarSwing : MonoBehaviour
{
   
   [Header("References")]
   [SerializeField] private PlayerMovement pm;
   [SerializeField] private ControlerManager cm;
   [SerializeField] private Rigidbody rb;
   [SerializeField] private PlayerStamina stamina;
   [SerializeField] private LedgeGrabbing lg;
   [SerializeField] private Transform orientation;
   [SerializeField] private CameraEffects cameraEffects;
   
   [Header("BarSettings")] 
   [SerializeField] private LayerMask whatIsBar;
   [SerializeField] private LayerMask whatIsGround;
   [SerializeField] private float grabRadius = 0.6f;
   [SerializeField] private float grabHeight = 1f;
   [SerializeField] private float grabReach = 0.3f;
   [SerializeField] private float swingRadius = 1.2f;
   [SerializeField] private float collisionRadius = 0.4f;
   [SerializeField] private float collisionHalfHeight = 0.6f;
   
   [Header("SwingSettings")] 
   [SerializeField] private float pumpStrength = 2f;
   [SerializeField] private float swingDamping = 0.3f;
   [SerializeField] private float maxSwingAngle = 75f;
   [SerializeField] private float hangStaminaCost = 2f;
   [SerializeField] private float releaseFov = 90f;
   [SerializeField] private float normalFOV = 80f;
   
   [Header("JumpSettings")]
   [SerializeField] private float releaseUpBoost = 4f;
   [SerializeField] private float releaseForwardBoost = 2f;
   [SerializeField] private float swingJumpCost = 8f;
   [SerializeField] private float dropCooldown = 0.5f;
   
   private Vector3 gripPoint;
   private Vector3 barAxis;
   private Transform lastBar;
   private Vector3 swingForward;
   private float swingSpeed;
   private float swingAngle;
   private float noGrabUntil;

   private void Start()
   {
      if (pm == null) pm = GetComponent<PlayerMovement>();
      if (cm == null) cm = GetComponent<ControlerManager>();
      if (rb == null) rb = GetComponent<Rigidbody>();
      if (stamina == null) stamina = GetComponent<PlayerStamina>();
      if (lg == null) lg = GetComponent<LedgeGrabbing>();
   }

   private void Update()
   {
      if (pm.swinging && cm.crouchInput || pm.swinging && stamina.IsExhausted)
      {
         noGrabUntil = Time.time + dropCooldown;
         StopSwing();
         return;
      }
      if (pm.swinging && cm.ConsumeJump())
      {
         StopSwing();
         if (stamina.TryConsume(swingJumpCost))
         {
            rb.AddForce(Vector3.up * releaseUpBoost + swingForward * releaseForwardBoost, ForceMode.Impulse);
            cameraEffects.DoFov(releaseFov);
            Invoke(nameof(ResetFOV), 0.4f);
            
         }
         return;
      }
      if (pm.swinging)
      {
         stamina.Drain(hangStaminaCost);
         float gravity = Physics.gravity.magnitude;
         float acceleration = -(gravity / swingRadius) * Mathf.Sin(swingAngle);

         if (cm.moveInput.y > 0)
         {
            acceleration += pumpStrength * Mathf.Sign(swingSpeed);
         }
         float previousAngle = swingAngle;
         swingSpeed += acceleration *  Time.deltaTime;
         swingSpeed *= 1 - (swingDamping * Time.deltaTime);
         swingAngle += swingSpeed * Time.deltaTime;
            
         float limit = maxSwingAngle * Mathf.Deg2Rad;

         if (Mathf.Abs(swingAngle) > limit)
         {
               swingAngle = limit * Mathf.Sign(swingAngle);
               swingSpeed = 0f;
         }
         Vector3 direction = Vector3.down * Mathf.Cos(swingAngle) + swingForward * Mathf.Sin(swingAngle);
         Vector3 newPos = gripPoint + direction * swingRadius;
         if (Physics.CheckCapsule(newPos + Vector3.up * collisionHalfHeight, newPos - Vector3.up * collisionHalfHeight,
                collisionRadius, whatIsGround))
         {
            swingAngle = previousAngle;
            swingSpeed = 0;
            direction = Vector3.down * Mathf.Cos(swingAngle) + swingForward * Mathf.Sin(swingAngle);
            newPos = gripPoint + direction * swingRadius;
         }

         transform.position = newPos;
         return;
      }
      
      if (!pm.grounded  && !pm.vaulting && !lg.holding && !pm.swinging &&!stamina.IsExhausted)
      {
         Collider newBar = null;
         Collider[] colliders = Physics.OverlapCapsule(transform.position + Vector3.up * 0.3f + orientation.forward * grabReach, transform.position + Vector3.up * grabHeight +  orientation.forward * grabReach, grabRadius , whatIsBar);
         foreach (Collider c in colliders)
         {
            if (c.transform != lastBar)
            {
               newBar = c;
               break;
            }
         }
         

         if (newBar != null)
         {
            
            BoxCollider bar = newBar.GetComponent<BoxCollider>();

            if (bar == null)
            { 
                  return;
               
            }
            float xColliderLenght = bar.size.x * bar.transform.lossyScale.x;
            float zColliderLenght = bar.size.z * bar.transform.lossyScale.z;
                    
            if (xColliderLenght < zColliderLenght)
            {
               barAxis = bar.transform.forward;
            }
            else
            {
               barAxis = bar.transform.right;
            }
                    
            Vector3 flat = barAxis;
            flat.y = 0;
            barAxis = flat.normalized;
            
            Vector3 barCenter = bar.transform.TransformPoint(bar.center);
            float lenghtBar  = Vector3.Dot(transform.position - barCenter, barAxis);
            gripPoint = barCenter + barAxis * lenghtBar;
            
            StartSwing(bar.transform);
         }
      }

    
   }

   private void StartSwing(Transform barTransform)
   {
      swingForward = Vector3.Cross(Vector3.up, barAxis);
      if (Vector3.Dot(swingForward, orientation.forward) < 0)
      {
         swingForward = -swingForward;
      }
      Vector3 direction =  transform.position - gripPoint;
      swingAngle = Mathf.Atan2(Vector3.Dot(direction, swingForward), Vector3.Dot(direction, Vector3.down));
      
      float limit = maxSwingAngle * Mathf.Deg2Rad;
      
      if (Mathf.Abs(swingAngle) > limit)
      {
         swingAngle = limit * Mathf.Sign(swingAngle);
         
      }

      Vector3 tangent = Vector3.up * Mathf.Sin(swingAngle) + swingForward * Mathf.Cos(swingAngle);
      swingSpeed = Vector3.Dot(rb.linearVelocity, tangent) / swingRadius;
      
      Vector3 arcDirection = Vector3.down * Mathf.Cos(swingAngle) + swingForward * Mathf.Sin(swingAngle);
      Vector3 startPos = gripPoint + arcDirection * swingRadius ;

      if (Physics.CheckCapsule(startPos + Vector3.up * collisionHalfHeight, startPos - Vector3.up * collisionHalfHeight, collisionRadius, whatIsGround))
      {
         return;
      }
      
      cm.ConsumeJump();
      
      rb.isKinematic = true ;
      pm.restricted = true;
      pm.swinging = true ;
      CancelInvoke(nameof(ResetLastBar));
      lastBar = barTransform;


      transform.position = startPos;



   }

   private void StopSwing()
   {
      rb.isKinematic = false ;
      pm.restricted = false;
      pm.swinging = false ;
      cm.crouchInput = false;
      Invoke(nameof(ResetLastBar), 1f);
      
      Vector3 direction =  Vector3.up * Mathf.Sin(swingAngle) + swingForward * Mathf.Cos(swingAngle);
      Vector3 speedOut = direction * swingSpeed * swingRadius;
      rb.linearVelocity = speedOut;
      
   }
   
   private void ResetLastBar()
   {
      lastBar = null;
   }

   private void ResetFOV()
   {
      cameraEffects.DoFov(normalFOV);
   }
}
