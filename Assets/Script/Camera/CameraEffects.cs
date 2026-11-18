using UnityEngine;
using DG.Tweening;

public class CameraEffects : MonoBehaviour
{
    [SerializeField]  private Camera cam;

    private void Start()
    {
       if(cam == null) cam = GetComponent<Camera>();
    }
    public void DoFov(float endValue)
    {
        cam.DOKill();
        cam.DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOKill();
        transform.DOLocalRotate(new Vector3(0f, 0f, zTilt), 0.25f);
    }

    public void DoRotate(float duration)
    {
        transform.DOKill();
        transform.DOLocalRotate(new Vector3(360, 0f, 0f), duration,RotateMode.LocalAxisAdd );
    }
    
    public void DoShake()
    {
        transform.DOKill();
        transform.DOShakePosition(0.2f,0.1f);
    }

    public void SetBalanceTilt(float zBalance)
    {
        transform.DOKill();
        transform.localRotation = Quaternion.Euler(0f, 0f, zBalance);
    }
    
}