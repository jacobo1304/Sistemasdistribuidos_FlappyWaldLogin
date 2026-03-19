using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneManager1 : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "Menu";
    [SerializeField] private string PlayAgainSceneName = "FlappyWald";


    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
    public void JugardeNuevo()
    {
        SceneManager.LoadScene(PlayAgainSceneName);
    }
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
