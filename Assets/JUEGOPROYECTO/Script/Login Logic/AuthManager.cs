using UnityEngine;
using System.Collections;
using System;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;
    private const string TokenKey = "TOKEN";
    private const string UsernameKey = "USERNAME";
    private const string LegacyTokenKey = "Token";
    private const string LegacyUsernameKey = "Username";
    private const string RegisterEndpoint = "/api/usuarios";
    private const string LoginEndpoint = "/api/auth/login";

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
        token = PlayerPrefs.GetString(TokenKey, "");
        username = PlayerPrefs.GetString(UsernameKey, "");

        if (string.IsNullOrEmpty(token))
        {
            token = PlayerPrefs.GetString(LegacyTokenKey, "");
        }

        if (string.IsNullOrEmpty(username))
        {
            username = PlayerPrefs.GetString(LegacyUsernameKey, "");
        }

        if (!string.IsNullOrEmpty(token))
        {
            PlayerPrefs.SetString(TokenKey, token);
            PlayerPrefs.SetString(LegacyTokenKey, token);
        }

        if (!string.IsNullOrEmpty(username))
        {
            PlayerPrefs.SetString(UsernameKey, username);
            PlayerPrefs.SetString(LegacyUsernameKey, username);
        }

        if (!string.IsNullOrEmpty(token) || !string.IsNullOrEmpty(username))
        {
            PlayerPrefs.Save();
        }
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

        string json = JsonUtility.ToJson(new AuthRequest(username, password));
        Debug.Log("AuthManager.Register() POST endpoint => " + RegisterEndpoint + " | body=" + json);

        StartCoroutine(APIManager.Instance.PostRequestWithResult(RegisterEndpoint, json, (success, response, statusCode) =>
        {
            Debug.Log("AuthManager.Register() POST response => success=" + success + " | code=" + statusCode + " | body=" + response);

            if (success)
            {
                if (!ValidateAuthResponse(response, false, out string businessMessage))
                {
                    callback?.Invoke(false, businessMessage);
                    return;
                }

                StoreAuthFromResponse(response, username);
                Debug.Log("Register response: " + response);
                PlayerPrefs.Save();
                callback?.Invoke(true, "Registro exitoso");
                return;
            }

            string message = ExtractApiErrorMessage(response);
            if (string.IsNullOrEmpty(message))
            {
                message = statusCode == 409 ? "El usuario ya existe" : "No se pudo registrar";
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

        string json = JsonUtility.ToJson(new AuthRequest(username, password));
        Debug.Log("AuthManager.Login() POST endpoint => " + LoginEndpoint + " | body=" + json);

        StartCoroutine(APIManager.Instance.PostRequestWithResult(LoginEndpoint, json, (success, response, statusCode) =>
        {
            Debug.Log("AuthManager.Login() POST response => success=" + success + " | code=" + statusCode + " | body=" + response);

            if (success)
            {
                if (!ValidateAuthResponse(response, true, out string businessMessage))
                {
                    callback?.Invoke(false, businessMessage);
                    return;
                }

                StoreAuthFromResponse(response, username);
                PlayerPrefs.Save();

                Debug.Log("Login exitoso");
                callback?.Invoke(true, "Login exitoso");
                return;
            }

            string message = ExtractApiErrorMessage(response);
            if (string.IsNullOrEmpty(message))
            {
                message = (statusCode == 400 || statusCode == 401) ? "Usuario o contraseña incorrectos" : "No se pudo iniciar sesión";
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
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.DeleteKey(UsernameKey);
        PlayerPrefs.DeleteKey(LegacyTokenKey);
        PlayerPrefs.DeleteKey(LegacyUsernameKey);
        PlayerPrefs.Save();
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
        if (response.Contains("Cannot POST") || response.Contains("Cannot GET"))
        {
            return "Ruta no encontrada en API: " + ExtractCannotMethodPath(response);
        }

        ApiErrorResponse err = TryParseJson<ApiErrorResponse>(response);
        if (err != null && !string.IsNullOrEmpty(err.message))
        {
            return err.message;
        }

        return null;
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

    private void StoreAuthFromResponse(string response, string fallbackUsername)
    {
        UserResponse userResponse = TryParseJson<UserResponse>(response);
        TokenResponse tokenResponse = TryParseJson<TokenResponse>(response);

        string parsedUsername = userResponse != null && userResponse.usuario != null ? userResponse.usuario.username : null;
        string parsedToken = tokenResponse != null ? tokenResponse.token : null;

        if (string.IsNullOrEmpty(parsedToken) && userResponse != null)
        {
            parsedToken = userResponse.token;
        }

        if (!string.IsNullOrEmpty(parsedToken))
        {
            token = parsedToken;
            PlayerPrefs.SetString(TokenKey, token);
            PlayerPrefs.SetString(LegacyTokenKey, token);
        }

        username = !string.IsNullOrEmpty(parsedUsername) ? parsedUsername : fallbackUsername;

        if (!string.IsNullOrEmpty(username))
        {
            PlayerPrefs.SetString(UsernameKey, username);
            PlayerPrefs.SetString(LegacyUsernameKey, username);
        }
    }

    private string ExtractCannotMethodPath(string response)
    {
        string[] markers = { "Cannot POST ", "Cannot GET ", "Cannot PUT ", "Cannot DELETE " };
        for (int i = 0; i < markers.Length; i++)
        {
            int idx = response.IndexOf(markers[i], StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                continue;
            }

            int start = idx + markers[i].Length;
            int end = response.IndexOf('<', start);
            if (end < 0)
            {
                end = response.Length;
            }

            return response.Substring(start, end - start).Trim();
        }

        return "desconocida";
    }

    private bool ValidateAuthResponse(string response, bool isLogin, out string message)
    {
        message = null;

        if (string.IsNullOrEmpty(response))
        {
            message = "Respuesta vacía del servidor";
            return false;
        }

        UserResponse userResponse = TryParseJson<UserResponse>(response);
        if (userResponse != null && userResponse.usuario != null)
        {
            if (!userResponse.usuario.estado)
            {
                message = isLogin ? "Usuario o contraseña incorrectos" : "No se pudo registrar el usuario";
                return false;
            }

            return true;
        }

        // Si backend responde solo token
        TokenResponse tokenOnly = TryParseJson<TokenResponse>(response);
        if (tokenOnly != null && !string.IsNullOrEmpty(tokenOnly.token))
        {
            return true;
        }

        // Registro por root-query puede devolver texto simple (no JSON) aunque sea exitoso
        if (!isLogin)
        {
            string apiMessage = ExtractApiErrorMessage(response);
            if (string.IsNullOrEmpty(apiMessage))
            {
                return true;
            }

            message = apiMessage;
            return false;
        }

        message = "Respuesta inválida del servidor";
        return false;
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
