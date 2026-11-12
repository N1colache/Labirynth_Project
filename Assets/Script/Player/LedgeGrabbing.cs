using System;
using UnityEngine;

public class LedgeGrabbing : MonoBehaviour
{
   [Header("References")] 
   public PlayerMovement pm;
   public ControlerManager controler;
   public Transform orientation;
   public Transform cam;
   private Rigidbody rb;

   [Header("Ledge Grabbing")] 
   public float moveToLedgeSpeed;
   public float maxLedgeGrabDistance;

   public float minTimeOnLedge;
   private float timeOnLedge;

   public bool holding;
   [Header("Ledge Jumping")]
   public float ledgeJumpForwardForce;
   public float ledgeJumpUpwardForce;
   
   [Header("Ledge Detection")]
   public float ledgeDetectionLenght;
   public float ledgeSphereCastRadius;
   public LayerMask whatIsLedge;
   
   private Transform lastLedge;
   private Transform currentLedge;
   
   private RaycastHit ledgeHit;
   
   [Header("Exiting")]
   public bool exitingLedge;
   public float exitLedgeTime;
   private float exitLedgeTimer;

   private void Awake()
   {
      rb = GetComponent<Rigidbody>();
   }
   private void Update()
   {
      LedgeDetection();
      SubStateMachine();
   }

   private void SubStateMachine()
   {
      float horizontalInput = controler.moveInput.x;
      float verticalInput = controler.moveInput.y;
      bool anyInputPressed = horizontalInput != 0 || verticalInput != 0;
      
      // substate 1 - holding onto ledge
      if (holding)
      {
         FreezeRigidbodyOnLedge();
         
         timeOnLedge += Time.deltaTime;

         if (timeOnLedge > minTimeOnLedge && anyInputPressed)
         {
            ExitLedgeHold();
         }

         if (controler.ConsumeJump())
         {
            LedgeJump();
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
      
      float distanceToLedge = Vector3.Distance(transform.position, ledgeHit.transform.position);
      
      if(ledgeHit.transform == lastLedge) return;
      
      if(distanceToLedge < maxLedgeGrabDistance && !holding)  EnterLedgeHold();
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
      holding = true;

      pm.unlimited = true;
      pm.restricted = true;
      
      currentLedge = ledgeHit.transform;
      lastLedge = ledgeHit.transform;
      
      rb.useGravity = false;
      rb.linearVelocity = Vector3.zero;
   }

   private void FreezeRigidbodyOnLedge()
   {
      rb.useGravity = false;
      
      Vector3 directionToLedge = currentLedge.position - transform.position;
      float distanceToLedge = Vector3.Distance(transform.position, currentLedge.position);

      if (distanceToLedge > 1f)
      {
         if (rb.linearVelocity.magnitude < moveToLedgeSpeed)
         {
            rb.AddForce(directionToLedge.normalized * moveToLedgeSpeed * 1000f * Time.deltaTime);
         }
      }
      else
      {
         
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
      
      rb.useGravity = true;
      StopAllCoroutines();
      Invoke(nameof(ResetLastLedge), 1f);
   }
   
   private void ResetLastLedge()
   {
      lastLedge = null;
   }
   
   
   
 

}
