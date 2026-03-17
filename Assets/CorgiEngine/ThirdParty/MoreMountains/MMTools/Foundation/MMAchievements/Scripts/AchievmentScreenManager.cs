using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    public class AchievementScreenManager : MonoBehaviour
    {
        [SerializeField]
        private MMAchievementList achievementList;

        [SerializeField]
        private GameObject achievementPanelPrefab; // Prefab del panel de logro
        [SerializeField]
        private Transform achievementsContainer; // Contenedor donde se instanciarán los paneles

        /// <summary>
        /// Activa los logros en la pantalla según su estado
        /// </summary>
        public void DisplayAchievements()
        {
            // Limpiar los logros existentes en el contenedor
            foreach (Transform child in achievementsContainer)
            {
                Destroy(child.gameObject);
            }

            // Iterar sobre la lista de logros
            foreach (MMAchievement achievement in achievementList.Achievements)
            {
                // Instanciar un panel para cada logro
                GameObject achievementPanel = Instantiate(achievementPanelPrefab, achievementsContainer);

                // Configurar el panel según el estado del logro
                bool isUnlocked = achievement.UnlockedStatus;
                achievementPanel.SetActive(true);

                // Aquí puedes personalizar el panel, por ejemplo, asignar texto o imágenes
                AchievementPanel panelScript = achievementPanel.GetComponent<AchievementPanel>();
                if (panelScript != null)
                {
                    panelScript.SetAchievementData(achievement.Title, isUnlocked);
                }
            }
        }
    }
}