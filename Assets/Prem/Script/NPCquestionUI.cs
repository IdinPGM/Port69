using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPCQuestionUIManager : MonoBehaviour
{
    public static NPCQuestionUIManager Instance { get; private set; }

    [Header("ส่วนติดต่อผู้ใช้")]
    [SerializeField] private GameObject แผงคำถาม;
    [SerializeField] private TMP_Text ข้อความคำถามUI;
    [SerializeField] private TMP_Text ข้อความตอบกลับUI;
    [SerializeField] private TMP_InputField ช่องใส่คำตอบ;
    [SerializeField] private Button ปุ่มส่งคำตอบ;
    [SerializeField] private Button ปุ่มปิด;
    [SerializeField] private GameObject ข้อความกดR;

    [Header("ตั้งค่าฟอนต์")]
    [SerializeField] private TMP_FontAsset ฟอนต์คำถาม;
    [SerializeField] private TMP_FontAsset ฟอนต์คำตอบ;
    [SerializeField] private TMP_FontAsset ฟอนต์ข้อความตอบกลับ;
    [SerializeField] private TMP_FontAsset ฟอนต์ข้อความกดR;

    private TMP_Text ข้อความกดRUI;
    private NPCQuestionSystem3D activeNPC; // The NPC currently being interacted with
    private bool isTyping = false;
    private bool isPanelOpen = false;

    private struct MouseState
    {
        public CursorLockMode lockState;
        public bool visible;
    }
    private MouseState previousMouseState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Validate UI elements
        if (แผงคำถาม == null) Debug.LogWarning("[NPCQuestionUIManager] ไม่ได้กำหนด GameObject แผงคำถาม!");
        if (ข้อความคำถามUI == null) Debug.LogWarning("[NPCQuestionUIManager] ไม่ได้กำหนด ข้อความคำถามUI!");
        if (ข้อความตอบกลับUI == null) Debug.LogWarning("[NPCQuestionUIManager] ไม่ได้กำหนด ข้อความตอบกลับUI!");
        if (ช่องใส่คำตอบ == null) Debug.LogWarning("[NPCQuestionUIManager] ไม่ได้กำหนด ช่องใส่คำตอบ!");
        if (ปุ่มส่งคำตอบ == null) Debug.LogWarning("[NPCQuestionUIManager] ไม่ได้กำหนด ปุ่มส่งคำตอบ!");
        if (ปุ่มปิด == null) Debug.LogWarning("[NPCQuestionUIManager] ไม่ได้กำหนด ปุ่มปิด!");
        if (ข้อความกดR == null) Debug.LogWarning("[NPCQuestionUIManager] ไม่ได้กำหนด GameObject ข้อความกดR!");

        // Setup Press R prompt
        if (ข้อความกดR != null)
        {
            ข้อความกดRUI = ข้อความกดR.GetComponent<TMP_Text>();
            if (ข้อความกดRUI != null)
            {
                if (ฟอนต์ข้อความกดR) ข้อความกดRUI.font = ฟอนต์ข้อความกดR;
            }
            else
            {
                Debug.LogWarning("[NPCQuestionUIManager] ข้อความกดR ไม่มีคอมโพเนนต์ TMP_Text!");
            }
            ข้อความกดR.SetActive(false);
        }

        // Initialize question panel
        if (แผงคำถาม != null) แผงคำถาม.SetActive(false);

        // Apply fonts
        if (ข้อความคำถามUI != null && ฟอนต์คำถาม) ข้อความคำถามUI.font = ฟอนต์คำถาม;
        if (ช่องใส่คำตอบ != null && ฟอนต์คำตอบ) ช่องใส่คำตอบ.fontAsset = ฟอนต์คำตอบ;
        if (ข้อความตอบกลับUI != null && ฟอนต์ข้อความตอบกลับ) ข้อความตอบกลับUI.font = ฟอนต์ข้อความตอบกลับ;

        // Add listeners
        if (ปุ่มส่งคำตอบ != null)
        {
            ปุ่มส่งคำตอบ.onClick.RemoveAllListeners();
            ปุ่มส่งคำตอบ.onClick.AddListener(SubmitAnswer);
        }
        if (ปุ่มปิด != null)
        {
            ปุ่มปิด.onClick.RemoveAllListeners();
            ปุ่มปิด.onClick.AddListener(ClosePanel);
        }
        if (ช่องใส่คำตอบ != null)
        {
            ช่องใส่คำตอบ.onValueChanged.RemoveAllListeners();
            ช่องใส่คำตอบ.onValueChanged.AddListener(OnTyping);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isPanelOpen)
        {
            ClosePanel();
        }
    }

    public void ShowPrompt(string promptText)
    {
        if (ข้อความกดR != null && ข้อความกดRUI != null)
        {
            ข้อความกดRUI.text = promptText;
            ข้อความกดR.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (ข้อความกดR != null)
        {
            ข้อความกดR.SetActive(false);
        }
    }

    public void OpenPanel(NPCQuestionSystem3D npc)
    {
        if (isPanelOpen || npc == null) return;

        activeNPC = npc;
        isPanelOpen = true;

        // Save mouse state
        previousMouseState = new MouseState
        {
            lockState = Cursor.lockState,
            visible = Cursor.visible
        };

        // Setup UI
        if (แผงคำถาม != null)
        {
            แผงคำถาม.SetActive(true);
            ข้อความคำถามUI.text = activeNPC.GetQuestion();
            ข้อความตอบกลับUI.text = "";
            ช่องใส่คำตอบ.text = "";
            ช่องใส่คำตอบ.ActivateInputField();
        }

        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Notify NPC to stop player movement
        if (activeNPC != null)
        {
            activeNPC.OnPanelOpened();
        }
    }

    private void OnTyping(string text)
    {
        isTyping = !string.IsNullOrEmpty(text);
    }

    private void SubmitAnswer()
    {
        if (activeNPC == null) return;

        string playerAnswer = ช่องใส่คำตอบ.text.Trim().ToLower();
        string correctAnswer = activeNPC.GetCorrectAnswer().ToLower();

        if (playerAnswer == correctAnswer)
        {
            ข้อความตอบกลับUI.text = "<color=#4CAF50>✓ ตอบถูก!</color>\n" + activeNPC.GetSuccessMessage();
            activeNPC.SetAnswered(true);
            ปุ่มส่งคำตอบ.interactable = false;
            ช่องใส่คำตอบ.interactable = false;
            StartCoroutine(HandleCorrectAnswer());
        }
        else
        {
            ข้อความตอบกลับUI.text = $"<color=#F44336>✗ ตอบผิด!</color> {activeNPC.GetFailureMessage()}\n" +
                                   $"<size=85%><color=#9E9E9E>คุณพิมพ์: {ช่องใส่คำตอบ.text}</color></size>";
            ช่องใส่คำตอบ.text = "";
            ช่องใส่คำตอบ.ActivateInputField();
        }
    }

    private IEnumerator HandleCorrectAnswer()
    {
        yield return new WaitForSeconds(3f); // Wait 3 seconds
        ClosePanel();
        if (activeNPC != null) activeNPC.DisableInteraction();
    }

    public void ClosePanel()
    {
        if (!isPanelOpen) return;

        if (แผงคำถาม != null) แผงคำถาม.SetActive(false);

        // Restore mouse state
        StartCoroutine(RestoreMouseState());

        // Notify NPC to restore player movement
        if (activeNPC != null)
        {
            activeNPC.OnPanelClosed();
        }

        isPanelOpen = false;
        isTyping = false;
        activeNPC = null;
    }

    private IEnumerator RestoreMouseState()
    {
        yield return null; // Wait until end of frame
        Cursor.lockState = previousMouseState.lockState;
        Cursor.visible = previousMouseState.visible;

        // Confirm again next frame
        yield return null;
        Cursor.lockState = previousMouseState.lockState;
        Cursor.visible = previousMouseState.visible;
    }

    public bool IsPanelOpen()
    {
        return isPanelOpen;
    }
}