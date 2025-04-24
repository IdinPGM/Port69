using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class PlayIntro : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "MainMenu"; // ชื่อ Scene ที่จะโหลดต่อ

    void Start()
    {
        videoPlayer.loopPointReached += EndReached; // เรียกเมื่อวิดีโอจบ
    }

    void EndReached(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName); // โหลด scene ต่อไป
    }
}
