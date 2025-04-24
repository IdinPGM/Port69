using UnityEngine;
using System.Collections;

public class NPCwind : MonoBehaviour
{
    [Header("การตั้งค่าหลัก")]
    [Tooltip("ระยะการโต้ตอบ (หน่วยเมตร)")]
    public float interactionRange = 3f;
    [Tooltip("ปุ่มที่ใช้โต้ตอบ")]
    public KeyCode interactionKey = KeyCode.R;
    [Tooltip("วัตถุที่จะให้จางหายไป")]
    public GameObject targetObject;

    [Header("เอฟเฟกต์การจางหาย")]
    [Tooltip("เวลาที่ใช้จางหาย (วินาที)")]
    public float fadeDuration = 2f;
    [Tooltip("ทำลายวัตถุเมื่อจางหายหมด")]
    public bool destroyAfterFade = true;
    [Tooltip("เอฟเฟกต์พิเศษขณะจางหาย")]
    public ParticleSystem fadeEffect;
    [Tooltip("เสียงขณะจางหาย")]
    public AudioClip fadeSound;

    private Transform player;
    private AudioSource audioSource;
    private bool isInteracting = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (fadeSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (targetObject == null)
        {
            Debug.LogError("ไม่ได้กำหนด Target Object ใน NPCInteraction");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(interactionKey) && !isInteracting)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= interactionRange)
            {
                StartCoroutine(FadeOutObject());
            }
        }
    }

    private IEnumerator FadeOutObject()
    {
        isInteracting = true;

        // ตรวจสอบว่ามีวัตถุเป้าหมายหรือไม่
        if (targetObject == null) yield break;

        // เตรียม Material
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
        Material[] materials = new Material[renderers.Length];
        Color[] originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] = renderers[i].material;
            originalColors[i] = materials[i].color;
            SetupMaterialForFading(materials[i]);
        }

        // เริ่มเอฟเฟกต์
        if (fadeEffect != null) fadeEffect.Play();
        if (fadeSound != null && audioSource != null) audioSource.PlayOneShot(fadeSound);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            float alpha = Mathf.Lerp(1f, 0f, progress);

            foreach (Material mat in materials)
            {
                Color newColor = mat.color;
                newColor.a = alpha;
                mat.color = newColor;
            }

            yield return null;
        }

        // จบกระบวนการ
        if (destroyAfterFade)
        {
            Destroy(targetObject);
        }
        else
        {
            targetObject.SetActive(false);
        }

        isInteracting = false;
    }

    private void SetupMaterialForFading(Material material)
    {
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.EnableKeyword("_ALPHABLEND_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    // วาดเส้นแสดงระยะโต้ตอบใน Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}