using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
   [SerializeField] private ControlerManager cm;
   
   [SerializeField] private float gamepadSensitivity = 200f;
   [SerializeField] private float mouseSensitivity = 0.3f;

   [SerializeField] private Transform orientation ;
   [SerializeField] private Transform camHolder ;
   [SerializeField] private PlayerInput playerInput;
   
   
   float _xRotation;
   float _yRotation;

   private void Start()
   {
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
      
      mouseSensitivity =  PlayerPrefs.GetFloat("MouseSensitivity",mouseSensitivity);
      gamepadSensitivity=  PlayerPrefs.GetFloat("GamePadSensitivity",gamepadSensitivity);
   }

   void Update()
   {
      if(Time.timeScale== 0)return;
      Vector2 lookInput = cm.lookInput;



      float sensX;
      float sensY;
      
      if (cm.IsGamePad)
      {
         sensX = lookInput.x * Time.deltaTime * gamepadSensitivity;
         sensY = lookInput.y * Time.deltaTime * gamepadSensitivity;
      }
      else
      {
         sensX = lookInput.x * mouseSensitivity;
         sensY = lookInput.y * mouseSensitivity;
      }
      

      _yRotation += sensX;

      _xRotation -= sensY;
      _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

      camHolder.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
      orientation.rotation = Quaternion.Euler(0f, _yRotation, 0f);
      
   }
}
