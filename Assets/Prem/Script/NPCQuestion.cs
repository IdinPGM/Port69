using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPCQuestionSystem3D : MonoBehaviour
{
    [Header("ตั้งค่าคำถาม")]
    [TextArea(3, 5)] public string ข้อความคำถาม;
    public string คำตอบที่ถูกต้อง;
    [TextArea(3, 5)] public string ข้อความหลังตอบถูก;
    [TextArea(2, 5)] public string ข้อความตอบผิด;

    [Header("ส่วนติดต่อผู้ใช้")]
    public GameObject แผงคำถาม;
    public TMP_Text ข้อความคำถามUI;
    public TMP_Text ข้อความตอบกลับUI;
    public TMP_InputField ช่องใส่คำตอบ;
    public Button ปุ่มส่งคำตอบ;
    public Button ปุ่มปิด;
    public GameObject ข้อความกดR;

    [Header("ตั้งค่าฟอนต์")]
    public TMP_FontAsset ฟอนต์คำถาม;
    public TMP_FontAsset ฟอนต์คำตอบ;
    public TMP_FontAsset ฟอนต์ข้อความตอบกลับ;
    public TMP_FontAsset ฟอนต์ข้อความกดR;

    [Header("ตั้งค่าการแสดงผล")]
    public float ระยะทางโต้ตอบ = 3f;
    public bool ต้องหันหน้ามาก่อน = true;
    public string ข้อความบอกกดR = "กด [R] เพื่อพูดคุย";

    private bool ตอบแล้ว = false;
    private bool กำลังพิมพ์ = false;
    private bool สามารถเคลื่อนที่ได้ = true;
    private Vector3 ความเร็วก่อนหยุด;
    private Rigidbody rbผู้เล่น;
    private Animator animatorผู้เล่น;
    private GameObject ผู้เล่น;
    private CharacterController controllerผู้เล่น;
    private TMP_Text ข้อความกดRUI;

    private struct MouseState
    {
        public CursorLockMode lockState;
        public bool visible;
    }
    private MouseState สถานะเมาส์ก่อนเปิดUI;

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

        // ตั้งค่าข้อความกด R
        if (ข้อความกดR != null)
        {
            ข้อความกดRUI = ข้อความกดR.GetComponent<TMP_Text>();
            if (ข้อความกดRUI != null)
            {
                ข้อความกดRUI.text = ข้อความบอกกดR;
                if (ฟอนต์ข้อความกดR) ข้อความกดRUI.font = ฟอนต์ข้อความกดR;
            }
            ข้อความกดR.SetActive(false);
        }

        แผงคำถาม.SetActive(false);

        // ตั้งค่าฟอนต์
        if (ฟอนต์คำถาม) ข้อความคำถามUI.font = ฟอนต์คำถาม;
        if (ฟอนต์คำตอบ) ช่องใส่คำตอบ.fontAsset = ฟอนต์คำตอบ;
        if (ฟอนต์ข้อความตอบกลับ) ข้อความตอบกลับUI.font = ฟอนต์ข้อความตอบกลับ;

        ปุ่มส่งคำตอบ.onClick.AddListener(ตรวจสอบคำตอบ);
        ปุ่มปิด.onClick.AddListener(ปิดแผงคำถาม);
        ช่องใส่คำตอบ.onValueChanged.AddListener(เมื่อกำลังพิมพ์);
    }

    private void Update()
    {
        if (ผู้เล่น == null) return;

        // ตรวจสอบระยะทางและมุมมอง
        float ระยะทาง = Vector3.Distance(transform.position, ผู้เล่น.transform.position);
        bool อยู่ในระยะ = ระยะทาง <= ระยะทางโต้ตอบ;
        bool กำลังมอง = true;

        if (ต้องหันหน้ามาก่อน)
        {
            Vector3 ทิศทางสู่NPC = (transform.position - ผู้เล่น.transform.position).normalized;
            float มุม = Vector3.Dot(ผู้เล่น.transform.forward, ทิศทางสู่NPC);
            กำลังมอง = มุม > 0.5f;
        }

        // แสดง/ซ่อน ข้อความกด R
        if (ข้อความกดR != null)
        {
            ข้อความกดR.SetActive(อยู่ในระยะ && กำลังมอง && !แผงคำถาม.activeSelf && !ตอบแล้ว);
        }

        if (Input.GetKeyDown(KeyCode.R) && อยู่ในระยะ && กำลังมอง)
        {
            if (!แผงคำถาม.activeSelf && !ตอบแล้ว && !กำลังพิมพ์)
            {
                เปิดแผงคำถาม();
            }
            else if (แผงคำถาม.activeSelf)
            {
                ปิดแผงคำถาม();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && แผงคำถาม.activeSelf)
        {
            ปิดแผงคำถาม();
        }
    }

    private void เปิดแผงคำถาม()
    {
        if (แผงคำถาม.activeSelf) return;

        // บันทึกสถานะเมาส์ปัจจุบัน
        สถานะเมาส์ก่อนเปิดUI = new MouseState
        {
            lockState = Cursor.lockState,
            visible = Cursor.visible
        };

        // ซ่อนข้อความกด R
        if (ข้อความกดR != null) ข้อความกดR.SetActive(false);

        // หยุดการเคลื่อนที่
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

        // ตั้งค่า UI
        แผงคำถาม.SetActive(true);
        ข้อความคำถามUI.text = ข้อความคำถาม;
        ข้อความตอบกลับUI.text = "";
        ช่องใส่คำตอบ.text = "";
        ช่องใส่คำตอบ.ActivateInputField();

        // ตั้งค่าการควบคุมเมาส์
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void เมื่อกำลังพิมพ์(string text)
    {
        กำลังพิมพ์ = !string.IsNullOrEmpty(text);
    }

    private void ตรวจสอบคำตอบ()
    {
        if (ช่องใส่คำตอบ.text.Trim().ToLower() == คำตอบที่ถูกต้อง.ToLower())
        {
            ข้อความตอบกลับUI.text = "<color=#4CAF50>✓ ตอบถูก!</color>\n" + ข้อความหลังตอบถูก;
            ตอบแล้ว = true;
            ปุ่มส่งคำตอบ.interactable = false;
            ช่องใส่คำตอบ.interactable = false;
        }
        else
        {
            ข้อความตอบกลับUI.text = $"<color=#F44336>✗ ตอบผิด!</color> {ข้อความตอบผิด}\n" +
                                   $"<size=85%><color=#9E9E9E>คุณพิมพ์: {ช่องใส่คำตอบ.text}</color></size>";
            ช่องใส่คำตอบ.text = "";
            ช่องใส่คำตอบ.ActivateInputField();
        }
    }

    public void ปิดแผงคำถาม()
    {
        if (!แผงคำถาม.activeSelf) return;

        แผงคำถาม.SetActive(false);

        // คืนสถานะการเคลื่อนที่
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

        // คืนค่าการควบคุมเมาส์
        StartCoroutine(คืนค่าสถานะเมาส์());
    }

    private IEnumerator คืนค่าสถานะเมาส์()
    {
        yield return null; // รอจนกว่าจะสิ้นสุดเฟรมปัจจุบัน

        Cursor.lockState = สถานะเมาส์ก่อนเปิดUI.lockState;
        Cursor.visible = สถานะเมาส์ก่อนเปิดUI.visible;

        // ยืนยันอีกครั้งในเฟรมถัดไป
        yield return null;
        Cursor.lockState = สถานะเมาส์ก่อนเปิดUI.lockState;
        Cursor.visible = สถานะเมาส์ก่อนเปิดUI.visible;
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
}