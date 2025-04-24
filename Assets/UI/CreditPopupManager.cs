using UnityEngine;

public class CreditPopupManager : MonoBehaviour
{
    public GameObject creditPanel;

    public void ShowCredit()
    {
        creditPanel.SetActive(true);
    }

    public void HideCredit()
    {
        creditPanel.SetActive(false);
    }
}
