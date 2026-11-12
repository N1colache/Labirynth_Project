using UnityEngine;
using DG.Tweening;
public class PlayerCam : MonoBehaviour
{
   [SerializeField] private ControlerManager controlerManager;
   
   [SerializeField] private float mouseSensX = 200f;
   [SerializeField] private float mouseSensY = 200f;
   [SerializeField] private float gamepadSensX = 200f;
   [SerializeField] private float gamepadSensY = 200f;

   [SerializeField] private Transform orientation ;
   [SerializeField] private Transform camHolder ;
   
   
   
   float _xRotation;
   float _yRotation;

   private void Start()
   {
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
   }

   void Update()
   {
      Vector2 lookInput = controlerManager.lookInput;

      bool usingGamepad = controlerManager.lastDeviceIsGamepad;

      float sensX = usingGamepad ? gamepadSensX : mouseSensX;
      float sensY = usingGamepad ? gamepadSensY : mouseSensY;
      
      float mouseX;
      float mouseY;
      if (usingGamepad)
      {
          mouseX = lookInput.x * Time.deltaTime * sensX;
          mouseY = lookInput.y * Time.deltaTime * sensY;
      }
      else
      {
         mouseX = lookInput.x * sensX;
         mouseY = lookInput.y * sensY;
      }
      

      _yRotation += mouseX;

      _xRotation -= mouseY;
      _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

      camHolder.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
      orientation.rotation = Quaternion.Euler(0f, _yRotation, 0f);
      
   }
}
