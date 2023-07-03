using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Dialogue : MonoBehaviour
{
    public Dialogue instance;
    public TextMeshProUGUI textComponent;
    public DialogueObj dialogueObj;
    public MovementController movementController;
    public AudioSource audioSource;
    public Animator animator;
    public bool endTheGame = false;
    public float textSpeed;

    private bool activated;
    private int index;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        } 
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        textComponent.text = string.Empty;
        activated = false;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Submit") && activated)
        {
            if (textComponent.text == dialogueObj.lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = dialogueObj.lines[index];
            }
        }
    }

    public void SetActive(bool state)
    {
        activated = true;
        gameObject.SetActive(true);
    }

    public void StartDialogue(DialogueObj newObj)
    {
        movementController.SwitchDialogue(true);
        gameObject.SetActive(true);
        activated = true;
        dialogueObj = newObj;
        endTheGame = dialogueObj.endGame;
        audioSource.clip = dialogueObj.dialogueNoise;
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in dialogueObj.lines[index].ToCharArray())
        {
            float randomStart = Random.Range(0, 0.41f);
            textComponent.text += c;
            audioSource.time = randomStart;
            audioSource.Play();
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < dialogueObj.lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            if (endTheGame)
            {
                StartCoroutine(LoadEnding());
            } else
            {
                textComponent.text = string.Empty;
                gameObject.SetActive(false);
                movementController.SwitchDialogue(false);
            }
        }
    }

    IEnumerator LoadEnding()
    {
        animator.SetBool("isEnd", true);
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("EndingScene");
    }
}