using UnityEngine;
using UnityEngine.SceneManagement; // Necesario si quieres cargar escenas desde aquí
using JUEGOPROYECTO.Management; // Asegúrate que el namespace sea correcto

public class MenuDifficultySelector : MonoBehaviour
{
    public void SelectEasyDifficulty()
    {
        if (GameDifficultyManager.Instance != null)
        {
            GameDifficultyManager.Instance.SetEasyMode();
            Debug.Log("[MenuDifficultySelector] Easy mode selected via GameDifficultyManager.");
            // Opcional: Cargar la escena del juego aquí o mediante otro botón
            // LoadGameScene(); 
        }
        else
        {
            Debug.LogError("[MenuDifficultySelector] GameDifficultyManager.Instance is not found!");
        }
    }

    public void SelectMediumDifficulty()
    {
        if (GameDifficultyManager.Instance != null)
        {
            GameDifficultyManager.Instance.SetMediumMode();
            Debug.Log("[MenuDifficultySelector] Medium mode selected via GameDifficultyManager.");
            // LoadGameScene();
        }
        else
        {
            Debug.LogError("[MenuDifficultySelector] GameDifficultyManager.Instance is not found!");
        }
    }

    public void SelectHardDifficulty()
    {
        if (GameDifficultyManager.Instance != null)
        {
            GameDifficultyManager.Instance.SetHardMode();
            Debug.Log("[MenuDifficultySelector] Hard mode selected via GameDifficultyManager.");
            // LoadGameScene();
        }
        else
        {
            Debug.LogError("[MenuDifficultySelector] GameDifficultyManager.Instance is not found!");
        }
    }

    // Ejemplo de cómo podrías cargar la escena del juego
    // public string gameSceneName = "TuEscenaDeJuego"; // Configura esto en el Inspector
    // public void LoadGameScene()
    // {
    //     if (!string.IsNullOrEmpty(gameSceneName))
    //     {
    //         SceneManager.LoadScene(gameSceneName);
    //     }
    //     else
    //     {
    //         Debug.LogError("[MenuDifficultySelector] Game scene name is not set!");
    //     }
    // }
}
