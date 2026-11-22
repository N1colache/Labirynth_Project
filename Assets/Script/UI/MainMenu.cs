using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;


public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject titlePanelPrincipal;
    [SerializeField] private GameObject titlePanelOption;

    [SerializeField] private GameObject buttonPlay;
    [SerializeField] private GameObject firstSlider;

    [SerializeField] private String gameScene;

    [SerializeField] private Slider sliderMouse;
    [SerializeField] private Slider sliderGamePad;
    
    [SerializeField] private TextMeshProUGUI valueSliderMouse;
    [SerializeField] private TextMeshProUGUI valueSliderGamePad;
    
    [SerializeField] private float defaultValueGamePad;
    [SerializeField] private float defaultValueMouse;
    
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1;
        
        sliderMouse.value =  PlayerPrefs.GetFloat("MouseSensitivity",defaultValueMouse);
        sliderGamePad.value =  PlayerPrefs.GetFloat("GamePadSensitivity",defaultValueGamePad);
        
        SetMouseSensitivity(sliderMouse.value);
        SetGamepadSensitivity(sliderGamePad.value);
    }

    private void ShowPanel(GameObject panel, GameObject selected)
    {
        titlePanel.SetActive(false);
        titlePanelPrincipal.SetActive(false);
        titlePanelOption.SetActive(false);
        panel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(selected);
    }

    private void OnSkip()
    {
        if (titlePanel.activeSelf)
        {
            ShowPanel(titlePanelPrincipal ,buttonPlay);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameScene);
    }
    
    public void OpenOptions()
    {
        ShowPanel(titlePanelOption,firstSlider);
    }
    public void CloseOptions()
    {
        ShowPanel(titlePanelPrincipal,buttonPlay);
    }
    
    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void SetMouseSensitivity(float value)
    {
        valueSliderMouse.text = value.ToString("F2");
        PlayerPrefs.SetFloat("MouseSensitivity", value);
    }
    public void SetGamepadSensitivity(float value)
    {
        valueSliderGamePad.text = value.ToString("F0");
        PlayerPrefs.SetFloat("GamePadSensitivity", value);
    }



}
