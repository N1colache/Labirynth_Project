using UnityEngine;

public class Balancing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private ControlerManager cm;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerStamina stamina;
    [SerializeField] private Vaulting vaulting;
    [SerializeField] private Transform orientation;
    [SerializeField] private CameraEffects camEffects;
    
    [Header("BalancingSettings")]
    [SerializeField] private float rayDiscrepancy = 0.4f;
    [SerializeField] private float rayLength = 1.3f;
    [SerializeField] private float speedOscillation = 1.3f;
    [SerializeField] private float maxAngle = 3f;
    [SerializeField] private float balanceStaminaCost = 3f;
    [SerializeField] private float fallPush = 3f;
    [SerializeField] private LayerMask whatIsVaulting;

    public Vector3 BeamDirection { get; private set; }
    private Vector3 beamRight;
    private Vector3 beamCenter;
    private float beamHeight;
    [SerializeField] private float centerStrength = 5f;
    
    public  bool IsBalancing { get; private set; }
    public bool OnBeam { get; private set; }
    private bool lastBalancing;

    private void Start()
    {
        if (pm == null) pm = GetComponent<PlayerMovement>();
        if (cm == null) cm = GetComponent<ControlerManager>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (stamina == null) stamina = GetComponent<PlayerStamina>();
        if (vaulting == null) vaulting = GetComponent<Vaulting>();
        if (camEffects == null) camEffects = GetComponent<CameraEffects>();
    }

    private void Update()
    {
        Vector3 center = transform.position;
        Vector3 right;
        Vector3 left;

        bool centerHit;
        bool rightHit;
        bool leftHit;
        if (!OnBeam)
        {
            
           
            right = transform.position + orientation.right * rayDiscrepancy * 1.5f;
            left = transform.position - orientation.right * rayDiscrepancy * 1.5f;
              
            centerHit = Physics.Raycast(center, Vector3.down,out  RaycastHit beamHit, rayLength, whatIsVaulting);
            rightHit = Physics.Raycast(right, Vector3.down, rayLength, whatIsVaulting);
            leftHit = Physics.Raycast(left, Vector3.down, rayLength, whatIsVaulting);

            if (centerHit && !rightHit && !leftHit && !pm.vaulting && !stamina.IsExhausted)
            {
                BoxCollider box = beamHit.transform.GetComponent<BoxCollider>();

                if (box != null)
                {
                    float xColliderLenght = box.size.x * beamHit.transform.lossyScale.x;
                    float zColliderLenght = box.size.z * beamHit.transform.lossyScale.z;
                    
                    if (xColliderLenght < zColliderLenght)
                    {
                        BeamDirection = beamHit.transform.forward;
                    }
                    else
                    {
                        BeamDirection = beamHit.transform.right;
                    }
                    
                    Vector3 flat = BeamDirection;
                    flat.y = 0;
                    BeamDirection = flat.normalized;
                    if (Vector3.Dot(BeamDirection, orientation.forward) < 0)
                    {
                        BeamDirection = -BeamDirection;
                    }
                    beamRight = Vector3.Cross(Vector3.up, BeamDirection);
                    beamCenter = beamHit.transform.TransformPoint(box.center);
                }
                else
                {
                    Vector3 dir = orientation.forward;
                    dir.y = 0;
                    BeamDirection = dir.normalized;
                    beamRight = Vector3.Cross(Vector3.up, BeamDirection);
                    beamCenter = transform.position;
                    
                }
                beamHeight = beamHit.point.y + vaulting.PlayerHalfHeight;
                OnBeam = true;
            }
               
                      
        }
        else 
        {
            right = transform.position + beamRight * rayDiscrepancy * 1.5f; 
            left = transform.position -  beamRight *  rayDiscrepancy * 1.5f;
             
            centerHit = Physics.Raycast(center, Vector3.down, rayLength, whatIsVaulting);
            rightHit = Physics.Raycast(right, Vector3.down, rayLength, whatIsVaulting);
            leftHit = Physics.Raycast(left, Vector3.down, rayLength, whatIsVaulting);  
                   
            
            OnBeam = centerHit && !rightHit && !leftHit && !pm.vaulting;
        }
        float angle = Mathf.Sin(Time.time * speedOscillation) * maxAngle;
        
        IsBalancing = OnBeam && !vaulting.perched;
        if (IsBalancing)
        {
            stamina.Drain(balanceStaminaCost);
            if (stamina.IsExhausted)
            {
                OnBeam = false;
                IsBalancing = false;
                
                rb.AddForce(beamRight * fallPush * Mathf.Sign(angle), ForceMode.Impulse);
            }
        }
        if (IsBalancing)
        {
            camEffects.SetBalanceTilt(angle);
        }
        
        if (lastBalancing && !IsBalancing)
        {
            camEffects.DoTilt(0f);
        }
        lastBalancing = IsBalancing;

    }

    private void FixedUpdate()
    {
        
        if (!rb.isKinematic && IsBalancing)
        {
            float gap = beamHeight - transform.position.y;
            Vector3 linearVelocity = rb.linearVelocity;
            linearVelocity.y = gap * centerStrength;
            rb.linearVelocity = linearVelocity;
            float speed = Vector3.Dot(rb.linearVelocity , beamRight);
            rb.linearVelocity = rb.linearVelocity - beamRight * speed;
            float discrepancy = Vector3.Dot(transform.position - beamCenter ,beamRight);
            rb.linearVelocity = rb.linearVelocity - beamRight * discrepancy * centerStrength;
        }
        
    }
    
    
}
