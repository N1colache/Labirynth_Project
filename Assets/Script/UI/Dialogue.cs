using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComp;
    public string[] lines;
    public float textSpeed = 0.5f;
    private int index;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ControlerManager controler;
    private bool ReadyToSkip = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controler = rb.GetComponentInParent<ControlerManager>();
        textComp.text = string.Empty;
        StartDialogue();
    }

    // Update is called once per frame
    void Update()
    {
        if (ReadyToSkip && controler.ConsumeSkip())
        {
            Debug.Log("skipped");
            ReadyToSkip = false;
            if(textComp.text == lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComp.text = lines[index];
            }
            /*if (lines[index].Length > lines[index].Length)
            {
            }*/
            Invoke(nameof(ResetSkip),1.0f);
        }
    }

    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComp.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComp.text = string.Empty;
            StartCoroutine (TypeLine());

        }
        else
        {
            gameObject.SetActive(false);

        }

    }

    private void ResetSkip()
    {
        ReadyToSkip = true;
    }
}
