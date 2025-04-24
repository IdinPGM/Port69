using UnityEngine;

public class ExitGameButton : MonoBehaviour
{
    public void ExitGame()
    {
        Debug.Log("Game is exiting...");  // ใช้ดูใน Editor ว่าปุ่มทำงานไหม
        Application.Quit();  // ใช้งานจริงเฉพาะตอน Build เป็น .exe หรือ .apk
    }
}