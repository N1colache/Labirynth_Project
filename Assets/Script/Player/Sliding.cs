using UnityEngine;


public class Sliding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerObj;
    [SerializeField] private ControlerManager controler;

    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding Settings")]
    [SerializeField] private float maxSlideTime = 0.75f;
    [SerializeField] private float slideForce = 400f;
    [SerializeField] private float slideYScale = 0.5f;
    
    private float slideTimer;
    private float startYScale;
    private bool slideInputPrevious;

    private float horizontalInput;
    private float verticalInput;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (pm == null) pm = GetComponent<PlayerMovement>();
        if (controler == null) controler = GetComponent<ControlerManager>();

        if (playerObj != null)
        {
            startYScale = playerObj.localScale.y;
        }
    }

    private void Update()
    {
        if (controler == null) return;

        horizontalInput = controler.moveInput.x;
        verticalInput = controler.moveInput.y;

        bool hasInput = horizontalInput != 0f || verticalInput != 0f;
        bool canSlide = !pm.sliding && !pm.restricted && !pm.freeze && !pm.activeGrapple && hasInput && pm.grounded;

        // Détection de l'appui (Front montant)
        if (controler.slideInput && !slideInputPrevious && canSlide)
        {
            StartSlide();
        }

        // Relâchement de la touche de glissade
        if (!controler.slideInput && pm.sliding)
        {
            StopSlide();
        }

        slideInputPrevious = controler.slideInput;
    }

    private void FixedUpdate()
    {
        if (pm != null && pm.sliding)
        {
            SlideMovement();
        }
    }

    private void StartSlide()
    {
        pm.sliding = true;

        if (playerObj != null)
        {
            playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        }

        // Force vers le bas pour plaquer le joueur au sol au lancement
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlideMovement()
    {
        if (orientation == null) return;

        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Glissade sur sol plat ou en montée
        if (!pm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            // Décompte correct du timer en FixedUpdate
            slideTimer -= Time.fixedDeltaTime;
        }
        // Glissade en descente (prise de vitesse infinie/conservée)
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0f)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        pm.sliding = false;

        if (playerObj != null)
        {
            playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
        }
    }

    private void OnDisable()
    {
        if (pm != null && pm.sliding)
        {
            StopSlide();
        }
    }
}