using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthUIController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Login UI")]
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;
    public TMP_Text loginMessageText;

    [Header("Register UI")]
    public TMP_InputField registerUsernameInput;
    public TMP_InputField registerPasswordInput;
    public TMP_Text registerMessageText;

    [Header("Flow")]
    public string sceneAfterLogin = "Menu";

    private void Start()
    {
        ShowLoginPanel();
    }

    public void ShowLoginPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(true);
        if (registerPanel != null) registerPanel.SetActive(false);
        SetLoginMessage(string.Empty, false);
        SetRegisterMessage(string.Empty, false);
    }

    public void ShowRegisterPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(true);
        SetLoginMessage(string.Empty, false);
        SetRegisterMessage(string.Empty, false);
    }

    public void OnLoginButtonPressed()
    {
        if (AuthManager.Instance == null)
        {
            SetLoginMessage("AuthManager no disponible", true);
            return;
        }

        string username = loginUsernameInput != null ? loginUsernameInput.text.Trim() : string.Empty;
        string password = loginPasswordInput != null ? loginPasswordInput.text : string.Empty;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetLoginMessage("Ingresa usuario y contraseña", true);
            return;
        }

        SetLoginMessage("Iniciando sesión...", false);

        AuthManager.Instance.Login(username, password, (ok, message) =>
        {
            if (!ok)
            {
                SetLoginMessage(message, true);
                return;
            }

            SetLoginMessage("Login exitoso", false);
            if (!string.IsNullOrEmpty(sceneAfterLogin))
            {
                SceneManager.LoadScene(sceneAfterLogin);
            }
        });
    }

    public void OnRegisterButtonPressed()
    {
        if (AuthManager.Instance == null)
        {
            SetRegisterMessage("AuthManager no disponible", true);
            return;
        }

        string username = registerUsernameInput != null ? registerUsernameInput.text.Trim() : string.Empty;
        string password = registerPasswordInput != null ? registerPasswordInput.text : string.Empty;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetRegisterMessage("Ingresa usuario y contraseña", true);
            return;
        }

        SetRegisterMessage("Registrando...", false);

        AuthManager.Instance.Register(username, password, (ok, message) =>
        {
            if (!ok)
            {
                SetRegisterMessage(message, true);
                return;
            }

            SetRegisterMessage("Registro exitoso. Ahora inicia sesión.", false);
            ShowLoginPanel();
            if (loginUsernameInput != null)
            {
                loginUsernameInput.text = username;
            }
        });
    }

    private void SetLoginMessage(string message, bool isError)
    {
        if (loginMessageText == null)
        {
            return;
        }

        loginMessageText.text = message;
        loginMessageText.color = isError ? Color.red : Color.white;
    }

    private void SetRegisterMessage(string message, bool isError)
    {
        if (registerMessageText == null)
        {
            return;
        }

        registerMessageText.text = message;
        registerMessageText.color = isError ? Color.red : Color.white;
    }
}
