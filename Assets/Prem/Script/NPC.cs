using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactionRange = 3f; // ระยะที่ผู้เล่นสามารถโต้ตอบได้
    public KeyCode interactKey = KeyCode.E; // ปุ่มสำหรับเริ่มบทสนทนา
    public LayerMask playerLayer; // Layer ของผู้เล่น

    [Header("UI Elements")]
    public GameObject interactPrompt; // ข้อความแจ้งให้กด E (เช่น "Press E to talk")
    public GameObject dialogueBox; // กล่องข้อความบทสนทนา
    public TextMeshProUGUI dialogueText; // ข้อความในกล่องบทสนทนา
    public Image continueIndicator; // ไอคอนแสดงว่าคลิกเพื่อไปต่อ

    [Header("Dialogue")]
    [TextArea(3, 10)]
    public string[] dialogueLines; // บทสนทนา
    private int currentLine = 0; // บรรทัดปัจจุบัน

    private Transform player;
    private bool isInRange = false;
    private bool isDialogueActive = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        interactPrompt.SetActive(false);
        dialogueBox.SetActive(false);
        continueIndicator.gameObject.SetActive(false);
    }

    void Update()
    {
        // ตรวจสอบระยะห่างระหว่าง NPC และผู้เล่น
        float distance = Vector3.Distance(transform.position, player.position);
        isInRange = distance <= interactionRange;

        // แสดงข้อความกด E เมื่ออยู่ในระยะ
        interactPrompt.SetActive(isInRange && !isDialogueActive);

        // ตรวจสอบการกดปุ่ม E เมื่ออยู่ในระยะ
        if (isInRange && Input.GetKeyDown(interactKey) && !isDialogueActive)
        {
            StartDialogue();
        }

        // ตรวจสอบการคลิกเมาส์เพื่อไปข้อความต่อไป
        if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            NextLine();
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        currentLine = 0;
        dialogueBox.SetActive(true);
        interactPrompt.SetActive(false);
        continueIndicator.gameObject.SetActive(false);
        DisplayLine();
    }

    void DisplayLine()
    {
        if (currentLine < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLine];
            // เปิดไอคอนคลิกเพื่อไปต่อหลังจากแสดงข้อความเสร็จ
            Invoke("ShowContinueIndicator", 0.1f);
        }
        else
        {
            EndDialogue();
        }
    }

    void ShowContinueIndicator()
    {
        continueIndicator.gameObject.SetActive(true);
    }

    void NextLine()
    {
        continueIndicator.gameObject.SetActive(false);
        currentLine++;
        DisplayLine();
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        dialogueBox.SetActive(false);
    }

    // วาด Gizmos เพื่อแสดงระยะ互動ใน Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}