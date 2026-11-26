using System.Collections;
using UnityEngine;
using TMPro;


[RequireComponent(typeof(Collider))]
public class Dialogue : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI textComp;

    [Header("Testo")]
    [SerializeField, TextArea] private string[] lines;
    [SerializeField] private float textSpeed = 0.05f;
    [SerializeField] private float skipCooldown = 0.2f;

    [Header("Attivazione")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private bool playOnce = true;

    private ControlerManager controler;
    private int index;
    private float nextSkipTime;
    private Coroutine typing;
    private bool isRunning;
    private bool hasPlayed;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Start()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isRunning || (playOnce && hasPlayed)) return;

        Rigidbody otherRb = other.attachedRigidbody;
        if (otherRb == null) return;

        if (rb != null && otherRb != rb) return;

        ControlerManager found = otherRb.GetComponentInParent<ControlerManager>();
        if (found == null) return;

        controler = found;
        StartDialogue();
    }

    void Update()
    {
        if (!isRunning || controler == null || Time.unscaledTime < nextSkipTime) return;
        if (!controler.ConsumeSkip()) return;

        nextSkipTime = Time.unscaledTime + skipCooldown;

        if (typing != null)
        {
            
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
        textComp.text = lines[index];
        textComp.maxVisibleCharacters = 0;
        textComp.ForceMeshUpdate();
        int total = textComp.textInfo.characterCount;

        for (int i = 1; i <= total; i++)
        {
            textComp.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(textSpeed); 
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
            EndDialogue();
        }
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
        if (isRunning) EndDialogue();
    }
}
