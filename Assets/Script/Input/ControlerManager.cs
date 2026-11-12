
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlerManager : MonoBehaviour
{
    private PlayerInput _input;
    
    private void Start()
    {
        _input = GetComponent<PlayerInput>();
    }

    #region Movement 

     // Variable
     public Vector2 moveInput;
     public bool moveInputBool;
     public bool jumpHeld;
     private bool jumpPending;
     public bool sprintInput;
     public bool crouchInput;
     public bool slideInput;
     public bool upwardsRunning ;
     public bool downwardsRunning;
     public event Action<bool> OnCrouchChanged;
     
     [SerializeField] private float jumpBufferTime = 0.15f;
     private float lastJumpPressTime;
    
     private void OnMovement(InputValue value)
     {
         moveInput = value.Get<Vector2>();
                   
     }
     private void OnJump(InputValue value)
     {
         jumpHeld = value.isPressed;
         if(value.isPressed == true)
         {
             jumpPending = true;
             lastJumpPressTime = Time.time;
         }
         Debug.Log("onJump" + value.isPressed);
     }

     public bool ConsumeJump()
     {
         if (jumpPending == true)
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
         sprintInput = value.isPressed;
     }

     private void OnCrouch(InputValue value)
     {
         crouchInput = value.isPressed;
         OnCrouchChanged?.Invoke(crouchInput);
     }

     private void OnSlide(InputValue value)
     {
         slideInput = value.isPressed;
     }
     
     private void OnWallRun(InputValue value)
     {
         if (value.Get<float> () > 0.5f)

         {
             upwardsRunning = true;
         } 
         else
         {
             upwardsRunning = false;
          
         }
        
         if ( value.Get<float> () < -0.5f)
         {
             downwardsRunning = true;
         }
         else
         {
             downwardsRunning = false;
         }

        
        
     }

     private void OnClimbing(InputValue value)
     {
         moveInputBool = value.isPressed;
     }
     
     
     
    #endregion

    #region Aiming & Looking
     // Variables
     public Vector2 lookInput;
     public bool lastDeviceIsGamepad;
     
     
     private void OnLook(InputValue value)
        {
            lookInput = value.Get<Vector2>();
            lastDeviceIsGamepad = _input.currentControlScheme == "Gamepad";
        }

    #endregion

    #region Attacks

     // Variables 
     public bool attackInput;
     public bool interactInput;
  
    
     
     private void OnAttack(InputValue value)
     {
         attackInput = value.isPressed;
     }
     
     
     private void OnInteract(InputValue value)
     {
         interactInput =  value.isPressed;
     }
     
    #endregion

   

    

   

  

   
    
    
}