using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;


[RequireComponent(typeof(Collider))]
public class DatapadText : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Key interactKey = Key.E;
    [SerializeField] private bool allowGamepad = true;

    [Header("UI")]
    [SerializeField] private GameObject imagePanel;
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite imageToShow;
    [SerializeField] private GameObject interactPrompt;

    [Header("Options")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool pauseGameWhileOpen = true;

    private bool playerInRange;
    private bool isOpen;
    private int closedOnFrame = -1;
    private IDisposable closeListener;

    private static bool anyPanelOpen;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Start()
    {
        if (imagePanel != null) imagePanel.SetActive(false);
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange || isOpen || anyPanelOpen) return;

        if (Time.frameCount == closedOnFrame) return;

        bool keyPressed = Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame;
        bool padPressed = allowGamepad && Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (keyPressed || padPressed)
            Open();
    }

    private void Open()
    {
        isOpen = true;
        anyPanelOpen = true;

        if (targetImage != null && imageToShow != null)
            targetImage.sprite = imageToShow;

        imagePanel.SetActive(true);
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (pauseGameWhileOpen) Time.timeScale = 0f;

        
        StartCoroutine(ListenForAnyInputNextFrame());
    }

    private IEnumerator ListenForAnyInputNextFrame()
    {
        yield return null; 
        closeListener = InputSystem.onAnyButtonPress.CallOnce(_ => Close());
    }

    private void Close()
    {
        closeListener?.Dispose();
        closeListener = null;

        isOpen = false;
        anyPanelOpen = false;
        closedOnFrame = Time.frameCount;

        imagePanel.SetActive(false);
        if (pauseGameWhileOpen) Time.timeScale = 1f;
        if (interactPrompt != null && playerInRange) interactPrompt.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = true;
        if (interactPrompt != null && !isOpen) interactPrompt.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void OnDisable()
    {
        
        if (isOpen) Close();
    }
}
