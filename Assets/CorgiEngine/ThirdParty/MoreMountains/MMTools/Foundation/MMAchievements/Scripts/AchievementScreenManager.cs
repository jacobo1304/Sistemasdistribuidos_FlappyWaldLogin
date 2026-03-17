using UnityEngine;
using MoreMountains.Tools; // Necesario para MMAchievement y MMAchievementList
using System.Collections.Generic; // Si usas MMAchievementManager.AchievementsList directamente

public class AchievementScreenManager : MonoBehaviour
{
    // No necesitas una referencia a MMAchievementList aquí si AchievementManager ya la carga.
    // MMAchievementManager.AchievementsList será la fuente de verdad.

    [SerializeField]
    private GameObject achievementPanelPrefab; // El prefab de tu panel de logro individual (que usa AchievementPanel.cs)
    [SerializeField]
    private Transform achievementsContainer; // El objeto padre en la UI donde se instanciarán los paneles

    void Awake()
    {
        // Es buena práctica asegurarse de que los logros estén cargados antes de intentar mostrarlos.
        // AchievementManager.cs se encarga de esto en su Start().
        // Si este script pudiera activarse antes, considera cargar aquí también o depender del orden de ejecución.
    }

    /// <summary>
    /// Limpia y muestra todos los logros basados en su estado actual.
    /// </summary>
    public void DisplayAchievements()
    {
        if (achievementPanelPrefab == null || achievementsContainer == null)
        {
            Debug.LogError("AchievementPanelPrefab o AchievementsContainer no asignados en AchievementScreenManager.");
            return;
        }

        // Limpiar los logros existentes en el contenedor
        foreach (Transform child in achievementsContainer)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("Paneles de logros antiguos limpiados.");

        // Iterar sobre la lista de logros del MMAchievementManager
        if (MMAchievementManager.AchievementsList == null || MMAchievementManager.AchievementsList.Count == 0)
        {
            Debug.LogWarning("No hay logros cargados en MMAchievementManager.AchievementsList para mostrar.");
            // Podrías haber olvidado llamar a MMAchievementManager.LoadAchievementList(yourListAsset)
            return;
        }
        
        Debug.Log($"Mostrando {MMAchievementManager.AchievementsList.Count} logros.");
        foreach (MMAchievement achievement in MMAchievementManager.AchievementsList)
        {
            // Instanciar un panel para cada logro
            GameObject achievementPanelGO = Instantiate(achievementPanelPrefab, achievementsContainer);
            AchievementPanel panelScript = achievementPanelGO.GetComponent<AchievementPanel>();

            if (panelScript != null)
            {
                // Configurar el panel según el estado del logro
                // El Debug.Log ya está en panelScript.SetAchievementData
                panelScript.SetAchievementData(achievement.Title, achievement.UnlockedStatus);
            }
            else
            {
                Debug.LogError($"El prefab {achievementPanelPrefab.name} no tiene el script AchievementPanel adjunto.");
            }
        }
        Debug.Log("Nuevos paneles de logros instanciados y configurados.");
    }
}
