using UnityEngine;
using System.Collections;
using System;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    [Header("Auth Endpoints")]
    public string registerEndpoint = "";
    public string loginEndpoint = "";

    private string token;
    private string username;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        token = PlayerPrefs.GetString("TOKEN", "");
        username = PlayerPrefs.GetString("USERNAME", "");
    }

    public void Register(string username, string password)
    {
        Register(username, password, null);
    }

    public void Register(string username, string password, Action<bool, string> callback)
    {
        if (APIManager.Instance == null)
        {
            callback?.Invoke(false, "API no disponible");
            return;
        }

        if (string.IsNullOrWhiteSpace(registerEndpoint))
        {
            callback?.Invoke(false, "Configura registerEndpoint en AuthManager");
            return;
        }

        string json = JsonUtility.ToJson(new ScoreUpdateRequest(username, 0));

        StartCoroutine(APIManager.Instance.PostRequestWithResult(NormalizeEndpoint(registerEndpoint), json,
            (success, response, statusCode) =>
        {
            if (success)
            {
                StoreUserFromResponse(response, username);
                Debug.Log("Register response: " + response);
                callback?.Invoke(true, "Registro exitoso");
                return;
            }

            string message = ExtractApiErrorMessage(response);
            if (string.IsNullOrEmpty(message))
            {
                if (statusCode == 409)
                {
                    message = "El usuario ya existe";
                }
                else if (statusCode == 404)
                {
                    message = "Endpoint de registro no encontrado. Revisa registerEndpoint en AuthManager.";
                }
                else
                {
                    message = "No se pudo registrar";
                }
            }
            callback?.Invoke(false, message);
        }));
    }

    public void Login(string username, string password)
    {
        Login(username, password, null);
    }

    public void Login(string username, string password, Action<bool, string> callback)
    {
        if (APIManager.Instance == null)
        {
            callback?.Invoke(false, "API no disponible");
            return;
        }

        if (string.IsNullOrWhiteSpace(loginEndpoint))
        {
            callback?.Invoke(false, "Configura loginEndpoint en AuthManager");
            return;
        }

        string json = JsonUtility.ToJson(new ScoreUpdateRequest(username, 0));

        StartCoroutine(APIManager.Instance.PostRequestWithResult(NormalizeEndpoint(loginEndpoint), json,
            (success, response, statusCode) =>
        {
            if (success && !string.IsNullOrEmpty(response))
            {
                TokenResponse tr = TryParseJson<TokenResponse>(response);
                if (tr != null && !string.IsNullOrEmpty(tr.token))
                {
                    token = tr.token;
                    PlayerPrefs.SetString("TOKEN", token);
                }

                StoreUserFromResponse(response, username);
                PlayerPrefs.Save();

                Debug.Log("Login exitoso");
                callback?.Invoke(true, "Login exitoso");
                return;
            }

            string message = ExtractApiErrorMessage(response);
            if (string.IsNullOrEmpty(message))
            {
                if (statusCode == 401)
                {
                    message = "Usuario o contraseña incorrectos";
                }
                else if (statusCode == 404)
                {
                    message = "Endpoint de login no encontrado. Revisa loginEndpoint en AuthManager.";
                }
                else
                {
                    message = "No se pudo iniciar sesión";
                }
            }
            callback?.Invoke(false, message);
        }));
    }

    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(token);
    }

    public void Logout()
    {
        token = "";
        username = "";
        PlayerPrefs.DeleteKey("TOKEN");
        PlayerPrefs.DeleteKey("USERNAME");
    }

    public string GetToken()
    {
        return token;
    }

    public string GetUsername()
    {
        return username;
    }

    private string ExtractApiErrorMessage(string response)
    {
        if (string.IsNullOrEmpty(response))
        {
            return null;
        }

        // HTML/text body fallback
        if (response.Contains("Cannot POST"))
        {
            return "Ruta no encontrada en API: " + ExtractCannotPostPath(response);
        }

        ApiErrorResponse err = TryParseJson<ApiErrorResponse>(response);
        if (err != null && !string.IsNullOrEmpty(err.message))
        {
            return err.message;
        }

        return null;
    }

    private string NormalizeEndpoint(string endpoint)
    {
        if (string.IsNullOrEmpty(endpoint))
        {
            return string.Empty;
        }

        return endpoint.StartsWith("/") ? endpoint : "/" + endpoint;
    }

    private T TryParseJson<T>(string json) where T : class
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        string trimmed = json.TrimStart();
        if (!trimmed.StartsWith("{"))
        {
            return null;
        }

        try
        {
            return JsonUtility.FromJson<T>(json);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private void StoreUserFromResponse(string response, string fallbackUsername)
    {
        UserResponse userResponse = TryParseJson<UserResponse>(response);
        string parsedUsername = userResponse != null && userResponse.usuario != null ? userResponse.usuario.username : null;
        username = !string.IsNullOrEmpty(parsedUsername) ? parsedUsername : fallbackUsername;

        if (!string.IsNullOrEmpty(username))
        {
            PlayerPrefs.SetString("USERNAME", username);
        }
    }

    private string ExtractCannotPostPath(string response)
    {
        const string marker = "Cannot POST ";
        int idx = response.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return "desconocida";
        }

        int start = idx + marker.Length;
        int end = response.IndexOf('<', start);
        if (end < 0)
        {
            end = response.Length;
        }

        return response.Substring(start, end - start).Trim();
    }
}

[System.Serializable]
public class TokenResponse
{
    public string token;
}

[System.Serializable]
public class ApiErrorResponse
{
    public string message;
}
