using System;
using UnityEngine;

public class Grappling : MonoBehaviour
{
   [Header("References")] 
   private PlayerMovement pm;
   public Transform cam;
   public Transform gunTip;
   public LayerMask whatIsGrappleable;
   public LineRenderer lr;
   
   [Header("Grappling")]
   public float maxGrappleDistance;
   public float grappleDelayTime;
   public float overShootAxis;
   
   private Vector3 grapplePoint;
   
   [Header("CoolDown")]
   public float grapplingCd;
   private float grapplingCdTimer;

   [Header("Input")] 
   public ControlerManager cm;
   private bool _attackInputPrevious;
   
   public bool grappling;

   private void Start()
   {
      pm =  GetComponent<PlayerMovement>();
      cm = GetComponent <ControlerManager>();
      
   }

   private void Update()
   {
      if (cm.attackInput && !_attackInputPrevious)
      {
         StartGrapple();
      }
      _attackInputPrevious = cm.attackInput;

      if (grapplingCdTimer > 0)
      {
         grapplingCdTimer -= Time.deltaTime;
      }
   }

   private void LateUpdate()
   {
      if (grappling)
      {
         lr.SetPosition(0,gunTip.position);
      }
   }

   private void StartGrapple()
   {
      if (grappling) return;
      if (grapplingCdTimer > 0) return;
      
      grappling = true;

      pm.freeze = true;

      RaycastHit hit;
      if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable)) 
      {
         grapplePoint = hit.point;
         
         Invoke(nameof(ExecuteGrapple), grappleDelayTime);
      }
      else
      {
         grapplePoint = cam.position + cam . forward * maxGrappleDistance;
         Invoke(nameof(StopGrapple), grappleDelayTime);
      }
      
      lr.enabled = true;
      lr.SetPosition(1, grapplePoint);
   }
   
   private void ExecuteGrapple()
   {
      pm.freeze = false;
      
      Vector3 lowestPoint = new Vector3(transform.position.x,transform.position.y - 1f,transform.position.z);
      
      float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
      float highestPointArc = grapplePointRelativeYPos + overShootAxis;
      
      if(grapplePointRelativeYPos < 0) highestPointArc = overShootAxis;
      
      pm.JumpToPosition(grapplePoint,highestPointArc);
      
      Invoke(nameof(StopGrapple), 5f);
   }

   public void StopGrapple()
   {
      pm.freeze = false;
      pm.ResetRestrictions();
      
      grappling = false;
      grapplingCdTimer = grapplingCd;
      lr.enabled = false;

   }
   
}