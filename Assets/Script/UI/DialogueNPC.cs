using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Mettilo su un oggetto nel mondo con un Collider impostato come "Is Trigger".
// Il dialogo parte quando il Rigidbody del player entra nel trigger.
// Dopo l'ultima riga lo schermo sfuma a nero e il gioco si chiude.
[RequireComponent(typeof(Collider))]
public class DialogueNPC : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Il pannello/box del dialogo nel Canvas (viene acceso e spento)")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI textComp;

    [Header("Testo")]
    [SerializeField, TextArea] private string[] lines;
    [Tooltip("Secondi tra una lettera e l'altra")]
    [SerializeField] private float textSpeed = 0.05f;
    [Tooltip("Tempo minimo tra due skip, per evitare doppi input")]
    [SerializeField] private float skipCooldown = 0.2f;

    [Header("Attivazione")]
    [Tooltip("Il Rigidbody del player. Se lo lasci vuoto, il dialogo parte con qualsiasi Rigidbody che ha un ControlerManager")]
    [SerializeField] private Rigidbody rb;
    [Tooltip("Se attivo, il dialogo parte solo la prima volta")]
    [SerializeField] private bool playOnce = true;

    [Header("Fine del gioco")]
    [Tooltip("Immagine nera a schermo intero, messa per ultima nel Canvas così copre tutto")]
    [SerializeField] private Image fadeImage;
    [Tooltip("Durata della dissolvenza a nero in secondi")]
    [SerializeField] private float fadeDuration = 2f;
    [Tooltip("Secondi di schermo nero prima di chiudere il gioco")]
    [SerializeField] private float delayBeforeQuit = 0.5f;

    private ControlerManager controler;
    private int index;
    private float nextSkipTime;
    private Coroutine typing;
    private bool isRunning;
    private bool hasPlayed;
    private bool isEnding;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Start()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);

        // L'immagine del fade parte trasparente e non blocca i click
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            SetFadeAlpha(0f);
            fadeImage.raycastTarget = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isRunning || isEnding || (playOnce && hasPlayed)) return;

        Rigidbody otherRb = other.attachedRigidbody;
        if (otherRb == null) return;

        // Se hai assegnato un Rigidbody specifico, parte solo con quello
        if (rb != null && otherRb != rb) return;

        ControlerManager found = otherRb.GetComponentInParent<ControlerManager>();
        if (found == null) return;

        controler = found;
        StartDialogue();
    }

    void Update()
    {
        if (!isRunning || isEnding || controler == null || Time.unscaledTime < nextSkipTime) return;
        if (!controler.ConsumeSkip()) return;

        nextSkipTime = Time.unscaledTime + skipCooldown;

        if (typing != null)
        {
            // La riga si sta ancora scrivendo: mostrala tutta subito
            StopCoroutine(typing);
            typing = null;
            textComp.maxVisibleCharacters = int.MaxValue;
        }
        else
        {
            NextLine();
        }
    }

    void StartDialogue()
    {
        if (lines == null || lines.Length == 0) return;

        isRunning = true;
        hasPlayed = true;
        index = 0;
        nextSkipTime = Time.unscaledTime + skipCooldown;

        if (dialogueBox != null) dialogueBox.SetActive(true);
        typing = StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        // Mette tutto il testo ma lo rende visibile una lettera alla volta.
        // Così i tag di TextMeshPro (<b>, <color>...) non appaiono a metà.
        textComp.text = lines[index];
        textComp.maxVisibleCharacters = 0;
        textComp.ForceMeshUpdate();
        int total = textComp.textInfo.characterCount;

        for (int i = 1; i <= total; i++)
        {
            textComp.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(textSpeed); // funziona anche col gioco in pausa
        }

        typing = null;
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            typing = StartCoroutine(TypeLine());
        }
        else
        {
            // Ultima riga letta: fade a nero e chiusura del gioco
            StartCoroutine(FadeAndQuit());
        }
    }

    IEnumerator FadeAndQuit()
    {
        isEnding = true;

        if (fadeImage != null)
        {
            fadeImage.raycastTarget = true; // blocca i click durante il fade

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                SetFadeAlpha(Mathf.Clamp01(t / fadeDuration));
                yield return null;
            }
            SetFadeAlpha(1f);
        }

        yield return new WaitForSecondsRealtime(delayBeforeQuit);

        QuitGame();
    }

    void SetFadeAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        // Nell'editor Application.Quit non fa nulla: fermiamo la modalità Play
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void EndDialogue()
    {
        StopAllCoroutines();
        typing = null;
        isRunning = false;
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    void OnDisable()
    {
        if (isRunning && !isEnding) EndDialogue();
    }
}
