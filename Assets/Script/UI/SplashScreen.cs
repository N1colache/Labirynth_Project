using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] List<CanvasGroup> SplashScreenObjects;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private String nextSceneName = "MainMenu";
    private bool isLoading = false;


    private void Start()
    {
        StartCoroutine(PrincipalRoutine());
    }
    private IEnumerator SplashScreenCoroutine(CanvasGroup splashScreen, float baseAlpha , float endAlpha)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(baseAlpha, endAlpha, t/fadeDuration);
            splashScreen.alpha = alpha;
            yield return null;
        }
        
        splashScreen.alpha = endAlpha;
    }

    private IEnumerator PrincipalRoutine()
    {
        foreach (var splashScreenObject in SplashScreenObjects)
        {
            yield return StartCoroutine(SplashScreenCoroutine(splashScreenObject, 0, 1));
            yield return new WaitForSeconds(holdDuration);
            yield return StartCoroutine(SplashScreenCoroutine(splashScreenObject, 1, 0));
        }
        LoadNext();
    }

    private void LoadNext()
    {
        if(isLoading)return;
        isLoading = true;
        SceneManager.LoadScene(nextSceneName);
    }
}
