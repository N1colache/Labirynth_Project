
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlerManager : MonoBehaviour
{
    private PlayerInput _input;
    
    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
    }

    #region Movement 

     // Variable
     public Vector2 moveInput;
     public bool jumpHeld;
     private bool jumpPending;
     public bool sprintInput;
     public bool crouchInput;
   
     
     [SerializeField] private float jumpBufferTime = 0.15f;
     private float lastJumpPressTime;
     public float LastCrouchPressTime{ get; private set; }
     public bool IsGamePad{get {return _input.currentControlScheme == "Gamepad";}}

     private void Update()
     {
         if (IsGamePad && sprintInput && moveInput.magnitude < 0.1)
         {
             sprintInput = false;
         }
     }
     private void OnMovement(InputValue value)
     {
         moveInput = value.Get<Vector2>();
     }
     private void OnJump(InputValue value)
     {
         jumpHeld = value.isPressed;
         if(value.isPressed)
         {
             jumpPending = true;
             lastJumpPressTime = Time.time;
         }
     }

     public bool ConsumeJump()
     {
         if (jumpPending)
         {
             if (Time.time - lastJumpPressTime > jumpBufferTime)
             {
                 jumpPending = false;
                 return false; 
             }
             else
             {
                  jumpPending = false;
                  return true;
             }
         }
         else
         {
             return false;
         }
     }
            
     private void OnSprint(InputValue value)
     {
         if (IsGamePad)
         {
             sprintInput = true;
         }
         else
         {
              sprintInput = value.isPressed;
         }
        
     }

     private void OnCrouch(InputValue value)
     {
         crouchInput = value.isPressed;
         if(value.isPressed == true)
         {
            
             LastCrouchPressTime = Time.time;
         }
     }
    #endregion

    #region Aiming & Looking
     // Variables
     public Vector2 lookInput;
     private void OnLook(InputValue value)
        {
            lookInput = value.Get<Vector2>();
        }

    #endregion

   

    

   

  

   
    
    
}