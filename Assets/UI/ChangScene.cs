using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeButton : MonoBehaviour
{
    [SerializeField] private AudioClip buttonClickClip; // เสียงเมื่อกดปุ่ม
    [SerializeField] private string sceneToLoad; // ชื่อซีนที่จะโหลด

    private AudioSource audioSource;

    private void Start()
    {
        // ตรวจสอบว่ามี AudioSource หรือไม่ ถ้าไม่มีก็เพิ่ม
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void OnButtonClicked()
    {
        // เล่นเสียงเมื่อกดปุ่ม
        if (buttonClickClip != null)
        {
            audioSource.PlayOneShot(buttonClickClip);
        }

        // เปลี่ยนซีนหลังจากเสียงเล่นเสร็จ (ถ้ามีเสียง)
        float delay = buttonClickClip != null ? buttonClickClip.length : 0f;
        Invoke("LoadScene", delay);
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}