using UnityEngine;
using MoreMountains.Tools;

public class AchievementManager : MonoBehaviour
{
    [SerializeField]
    private MMAchievementList achievementList; // Tu ScriptableObject con la lista de logros
    
    [SerializeField]
    private AchievementScreenManager achievementScreenManager; // Referencia al gestor de la pantalla de logros

    void Start()
    {
        // Asegúrate de que la lista de logros esté cargada al inicio
        if (achievementList != null)
        {
            MMAchievementManager.LoadAchievementList(achievementList);
            MMAchievementManager.LoadSavedAchievements(); // Carga el progreso guardado
        }
        else
        {
            Debug.LogError("AchievementList no está asignada en AchievementManager.");
        }

        // Opcional: Actualizar la pantalla de logros al inicio
        if (achievementScreenManager != null)
        {
            achievementScreenManager.DisplayAchievements();
        }
        else
        {
            Debug.LogError("AchievementScreenManager no está asignado en AchievementManager.");
        }
    }

    /// <summary>
    /// Bloquea todos los logros y actualiza la UI.
    /// Este método puede ser llamado por un botón de reinicio.
    /// </summary>
    public void ResetAndRefreshAchievements()
    {
        if (achievementList == null)
        {
            Debug.LogError("AchievementList no asignada.");
            return;
        }

        // Opción 1: Usar el método de MoreMountains si quieres su lógica de reinicio completa
        // MMAchievementManager.ResetAchievements(); 
        // Esto ya debería poner UnlockedStatus = false y ProgressCurrent = 0 y guardar.

        // Opción 2: Implementación manual para asegurar UnlockedStatus = false
        Debug.Log("Reiniciando todos los logros...");
        foreach (MMAchievement achievement in MMAchievementManager.AchievementsList) // Usar la lista del Manager
        {
            achievement.UnlockedStatus = false;
            // achievement.ProgressCurrent = 0; // Si también quieres reiniciar el progreso
            Debug.Log($"Logro {achievement.AchievementID} bloqueado. Estado: {achievement.UnlockedStatus}");
        }
        MMAchievementManager.SaveAchievements(); // Guarda los cambios
        Debug.Log("Logros guardados después del reinicio.");

        // Actualiza la pantalla de logros
        if (achievementScreenManager != null)
        {
            achievementScreenManager.DisplayAchievements();
            Debug.Log("Pantalla de logros actualizada después del reinicio.");
        }
        else
        {
            Debug.LogError("AchievementScreenManager no está asignado. No se puede actualizar la UI.");
        }
    }
}