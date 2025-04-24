using UnityEngine;

public class NPCQuestionSystem3D : MonoBehaviour
{
    [Header("ตั้งค่าคำถาม")]
    [TextArea(3, 5)] public string ข้อความคำถาม;
    public string คำตอบที่ถูกต้อง;
    [TextArea(3, 5)] public string ข้อความหลังตอบถูก;
    [TextArea(2, 5)] public string ข้อความตอบผิด;

    [Header("ตั้งค่าการแสดงผล")]
    public float ระยะทางโต้ตอบ = 3f; // Kept for Gizmos, but not used for detection
    public bool ต้องหันหน้ามาก่อน = true;
    public string ข้อความบอกกดR = "กด [R] เพื่อพูดคุย";

    private bool ตอบแล้ว = false;
    private bool สามารถเคลื่อนที่ได้ = true;
    private bool สามารถโต้ตอบได้ = true;
    private bool อยู่ในระยะ = false; // Controlled by collider
    private Vector3 ความเร็วก่อนหยุด;
    private Rigidbody rbผู้เล่น;
    private Animator animatorผู้เล่น;
    private GameObject ผู้เล่น;
    private CharacterController controllerผู้เล่น;
    private BoxCollider boxCollider;

    private void Start()
    {
        // หาคอมโพเนนต์จากผู้เล่นโดยอัตโนมัติ
        ผู้เล่น = GameObject.FindGameObjectWithTag("Player");
        if (ผู้เล่น != null)
        {
            rbผู้เล่น = ผู้เล่น.GetComponent<Rigidbody>();
            animatorผู้เล่น = ผู้เล่น.GetComponent<Animator>();
            controllerผู้เล่น = ผู้เล่น.GetComponent<CharacterController>();
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] ไม่พบผู้เล่นที่มีแท็ก 'Player'!");
        }

        // หาคอมโพเนนต์ BoxCollider
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogWarning($"[{gameObject.name}] ไม่พบ BoxCollider บน GameObject นี้! โปรดเพิ่ม BoxCollider เพื่อกำหนดพื้นที่โต้ตอบ");
        }
        else
        {
            boxCollider.isTrigger = true; // Ensure it's a trigger
        }

        // Validate UIManager
        if (NPCQuestionUIManager.Instance == null)
        {
            Debug.LogError($"[{gameObject.name}] ไม่พบ NPCQuestionUIManager ในฉาก! โปรดเพิ่ม NPCQuestionUIManager");
        }
    }

    private void Update()
    {
        if (ผู้เล่น == null) return;

        // ตรวจสอบมุมมอง
        bool กำลังมอง = true;
        if (ต้องหันหน้ามาก่อน)
        {
            Vector3 ทิศทางสู่NPC = (transform.position - ผู้เล่น.transform.position).normalized;
            float มุม = Vector3.Dot(ผู้เล่น.transform.forward, ทิศทางสู่NPC);
            กำลังมอง = มุม > 0.5f;
        }

        // แสดง/ซ่อน ข้อความกด R ผ่าน UIManager
        if (NPCQuestionUIManager.Instance != null)
        {
            bool เงื่อนไขแสดงข้อความ = อยู่ในระยะ && กำลังมอง && !NPCQuestionUIManager.Instance.IsPanelOpen() && !ตอบแล้ว && สามารถโต้ตอบได้;
            if (เงื่อนไขแสดงข้อความ)
            {
                NPCQuestionUIManager.Instance.ShowPrompt(ข้อความบอกกดR);
            }
            else
            {
                NPCQuestionUIManager.Instance.HidePrompt();
                if (!เงื่อนไขแสดงข้อความ)
                {
                    Debug.Log($"[{gameObject.name}] ข้อความกดR ไม่แสดง: อยู่ในระยะ={อยู่ในระยะ}, กำลังมอง={กำลังมอง}, แผงคำถามปิดอยู่={(!NPCQuestionUIManager.Instance.IsPanelOpen())}, ยังไม่ตอบ={(!ตอบแล้ว)}, สามารถโต้ตอบได้={(สามารถโต้ตอบได้)}");
                }
            }
        }

        // ตรวจสอบการกด R (เฉพาะเปิดแผงเท่านั้น)
        if (Input.GetKeyDown(KeyCode.R) && อยู่ในระยะ && กำลังมอง && สามารถโต้ตอบได้)
        {
            if (!NPCQuestionUIManager.Instance.IsPanelOpen() && !ตอบแล้ว)
            {
                if (NPCQuestionUIManager.Instance != null)
                {
                    NPCQuestionUIManager.Instance.OpenPanel(this);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            อยู่ในระยะ = true;
            Debug.Log($"[{gameObject.name}] ผู้เล่นอยู่ในระยะของ BoxCollider");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            อยู่ในระยะ = false;
            Debug.Log($"[{gameObject.name}] ผู้เล่นออกจากระยะของ BoxCollider");
            if (NPCQuestionUIManager.Instance != null)
            {
                NPCQuestionUIManager.Instance.HidePrompt();
            }
        }
    }

    public void OnPanelOpened()
    {
        // Stop player movement
        if (rbผู้เล่น != null)
        {
            สามารถเคลื่อนที่ได้ = false;
            ความเร็วก่อนหยุด = rbผู้เล่น.linearVelocity;
            rbผู้เล่น.linearVelocity = Vector3.zero;
        }

        if (controllerผู้เล่น != null)
        {
            สามารถเคลื่อนที่ได้ = false;
        }

        if (animatorผู้เล่น != null)
        {
            animatorผู้เล่น.enabled = false;
        }
    }

    public void OnPanelClosed()
    {
        // Restore player movement
        สามารถเคลื่อนที่ได้ = true;

        if (rbผู้เล่น != null)
        {
            rbผู้เล่น.linearVelocity = ความเร็วก่อนหยุด;
        }

        if (controllerผู้เล่น != null)
        {
            controllerผู้เล่น.Move(Vector3.zero);
        }

        if (animatorผู้เล่น != null)
        {
            animatorผู้เล่น.enabled = true;
        }
    }

    private void FixedUpdate()
    {
        if (!สามารถเคลื่อนที่ได้)
        {
            if (rbผู้เล่น != null)
            {
                rbผู้เล่น.linearVelocity = Vector3.zero;
            }

            if (controllerผู้เล่น != null)
            {
                controllerผู้เล่น.Move(Vector3.zero);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, ระยะทางโต้ตอบ);
    }

    // Public methods for UIManager to access NPC data
    public string GetQuestion()
    {
        return ข้อความคำถาม;
    }

    public string GetCorrectAnswer()
    {
        return คำตอบที่ถูกต้อง;
    }

    public string GetSuccessMessage()
    {
        return ข้อความหลังตอบถูก;
    }

    public string GetFailureMessage()
    {
        return ข้อความตอบผิด;
    }

    public void SetAnswered(bool value)
    {
        ตอบแล้ว = value;
    }

    public void DisableInteraction()
    {
        สามารถโต้ตอบได้ = false;
    }
}