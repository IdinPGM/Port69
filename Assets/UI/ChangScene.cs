using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // เรียก method นี้จากปุ่ม
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}