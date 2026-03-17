using UnityEngine;
using TMPro;
using UnityEngine.UI; // Necesario para Image

public class AchievementPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_Text achievementTitleText;
    [SerializeField]
    private Image unlockedIcon; // Cambiado a Image
    [SerializeField]
    private Image lockedIcon;   // Cambiado a Image

    public void SetAchievementData(string title, bool isUnlocked)
    {
        if (achievementTitleText != null)
        {
            achievementTitleText.text = title;
        }

        if (unlockedIcon != null)
        {
            unlockedIcon.gameObject.SetActive(isUnlocked); // Activa/desactiva el GameObject del Image
        }

        if (lockedIcon != null)
        {
            lockedIcon.gameObject.SetActive(!isUnlocked); // Activa/desactiva el GameObject del Image
        }
        Debug.Log($"Panel UI -> Logro: {title}, Estado: {(isUnlocked ? "Desbloqueado" : "Bloqueado")}");
    }
}
