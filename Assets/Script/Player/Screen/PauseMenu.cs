using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private PlayerCam playerCam;
    
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject titlePanelOption;
    
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private GameObject firstSlider;
    
    [SerializeField] private String menuScene;
    [SerializeField] private ControlerManager cm;
    
    [SerializeField] private Slider sliderMouse;
    [SerializeField] private Slider sliderGamePad;
    
    [SerializeField] private TextMeshProUGUI valueSliderMouse;
    [SerializeField] private TextMeshProUGUI valueSliderGamePad;
    
    [SerializeField] private float defaultValueGamePad;
    [SerializeField] private float defaultValueMouse;
    public bool IsPaused { get; private set; }
    
    
    private void Start()
    {
        sliderMouse.value =  PlayerPrefs.GetFloat("MouseSensitivity",defaultValueMouse);
        sliderGamePad.value =  PlayerPrefs.GetFloat("GamePadSensitivity",defaultValueGamePad);
        
        SetMouseSensitivity(sliderMouse.value);
        SetGamepadSensitivity(sliderGamePad.value);
    }
    private void OnPause()
    {
        if (IsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Resume()
    {
        IsPaused  = false;
        titlePanel.SetActive(false);
        titlePanelOption.SetActive(false);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cm.ConsumeJump();
        cm.crouchInput =  false;
    }

    public void Pause()
    {
        IsPaused = true;
        titlePanel.SetActive(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        EventSystem.current.SetSelectedGameObject(resumeButton);
    }
    
    public void GoToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(menuScene);
    }
    
    private void ShowPanel(GameObject panel, GameObject selected)
    {
        titlePanel.SetActive(false);
        titlePanelOption.SetActive(false);
        panel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(selected);
    }
    
    public void OpenOptions()
    {
        ShowPanel(titlePanelOption,firstSlider);
    }
    public void CloseOptions()
    {
        ShowPanel(titlePanel,resumeButton);
    }
    
    public void SetMouseSensitivity(float value)
    {
        valueSliderMouse.text = value.ToString("F2");
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        playerCam.SetMouseSensitivity(value);
    }
    public void SetGamepadSensitivity(float value)
    {
        valueSliderGamePad.text = value.ToString("F0");
        PlayerPrefs.SetFloat("GamePadSensitivity", value);
        playerCam.SetGamePadSensitivity(value);
    }
}
