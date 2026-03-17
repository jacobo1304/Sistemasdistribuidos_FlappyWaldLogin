using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;

    [SerializeField] private string baseUrl = "https://sid-restapi.onrender.com";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator PostRequest(string endpoint, string jsonBody, System.Action<string> callback, string token = null)
    {
        yield return PostRequestWithResult(endpoint, jsonBody, (success, responseText, statusCode) =>
        {
            callback?.Invoke(success ? responseText : null);
        }, token);
    }

    public IEnumerator PostRequestWithResult(string endpoint, string jsonBody, System.Action<bool, string, long> callback, string token = null)
    {
        string url = BuildUrl(endpoint);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }

        yield return request.SendWebRequest();

        bool success = request.result == UnityWebRequest.Result.Success;
        string responseText = request.downloadHandler != null ? request.downloadHandler.text : null;

        if (!success)
        {
            Debug.LogError("Error: " + request.error + " | URL: " + url + " | Status: " + request.responseCode + " | Body: " + responseText);
        }

        callback?.Invoke(success, responseText, request.responseCode);
    }

    public IEnumerator GetRequest(string endpoint, System.Action<string> callback, string token = null)
    {
        yield return GetRequestWithResult(endpoint, (success, responseText, statusCode) =>
        {
            callback?.Invoke(success ? responseText : null);
        }, token);
    }

    public IEnumerator GetRequestWithResult(string endpoint, System.Action<bool, string, long> callback, string token = null)
    {
        string url = BuildUrl(endpoint);

        UnityWebRequest request = UnityWebRequest.Get(url);

        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }

        yield return request.SendWebRequest();

        bool success = request.result == UnityWebRequest.Result.Success;
        string responseText = request.downloadHandler != null ? request.downloadHandler.text : null;

        if (!success)
        {
            Debug.LogError("Error: " + request.error + " | URL: " + url + " | Status: " + request.responseCode + " | Body: " + responseText);
        }

        callback?.Invoke(success, responseText, request.responseCode);
    }

    private string BuildUrl(string endpoint)
    {
        string safeBase = string.IsNullOrEmpty(baseUrl) ? string.Empty : baseUrl.TrimEnd('/');
        string safeEndpoint = string.IsNullOrEmpty(endpoint) ? string.Empty : endpoint.Trim();

        if (!safeEndpoint.StartsWith("/"))
        {
            safeEndpoint = "/" + safeEndpoint;
        }

        return safeBase + safeEndpoint;
    }
}