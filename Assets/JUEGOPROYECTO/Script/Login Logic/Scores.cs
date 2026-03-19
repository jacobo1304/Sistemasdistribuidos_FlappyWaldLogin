using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using JUEGOPROYECTO.Management;

public class ScoreManager : MonoBehaviour
{
    private const string LocalBestScorePrefix = "LOCAL_BEST_SCORE_";
    private const string LocalLeaderboardKey = "LOCAL_LEADERBOARD_V1";

    [Serializable]
    private class LocalLeaderboardEntry
    {
        public string username;
        public int score;
    }

    [Serializable]
    private class LocalLeaderboardStore
    {
        public LocalLeaderboardEntry[] entries;
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string username;
        public int score;
    }

    [Serializable]
    private class LeaderboardUserResponseArrayWrapper
    {
        public UserResponse[] users;
    }

    [Serializable]
    private class LeaderboardUserArrayWrapper
    {
        public User[] users;
    }

    [Serializable]
    private class LeaderboardObjectWrapper
    {
        public UserResponse[] leaderboard;
        public UserResponse[] users;
        public UserResponse[] data;
        public UserResponse[] usuarios;
    }

    [Serializable]
    private class LeaderboardObjectUserWrapper
    {
        public User[] leaderboard;
        public User[] users;
        public User[] data;
        public User[] usuarios;
    }

    public static ScoreManager Instance;

    [Header("Endpoints")]
    public string scoreEndpoint = "";
    public string leaderboardEndpoint = "";
    [Header("Root Query Mode")]
    [Tooltip("If enabled, score update uses GET query on root, e.g. /?username=...&score=...")]
    public bool useRootQueryScore = true;
    public string rootScoreEndpoint = "/";
    public string scoreQueryKey = "score";
    [Tooltip("If enabled, leaderboard uses GET on root. Optionally appends leaderboard=true query.")]
    public bool useRootQueryLeaderboard = true;
    public string rootLeaderboardEndpoint = "/";
    public bool appendLeaderboardQuery = false;
    public string leaderboardQueryKey = "leaderboard";
    public string leaderboardQueryValue = "true";

    public int CurrentScore { get; private set; }

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

    public void ResetRunScore()
    {
        CurrentScore = 0;
    }

    public void AddScore(int points)
    {
        if (points <= 0)
        {
            return;
        }

        CurrentScore += points;
    }

    public void UpdateScore(int score)
    {
        CurrentScore = Mathf.Max(0, score);
    }

    public void SubmitCurrentScore()
    {
        if (AuthManager.Instance == null)
        {
            Debug.LogWarning("ScoreManager: AuthManager.Instance is null. Score was not submitted.");
            return;
        }

        string username = AuthManager.Instance.GetUsername();
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("ScoreManager: Username unavailable. Score was not submitted.");
            return;
        }

        SaveLocalBestScore(username, CurrentScore);

        if (APIManager.Instance == null)
        {
            Debug.LogWarning("ScoreManager: APIManager.Instance is null. Score was not submitted to backend (saved locally).");
            return;
        }

        StartCoroutine(SubmitCurrentScoreCoroutine(username));
    }

    private IEnumerator SubmitCurrentScoreCoroutine(string username)
    {
        string token = AuthManager.Instance != null ? AuthManager.Instance.GetToken() : null;
        bool success = false;
        string response = null;
        long statusCode = 0;

        if (useRootQueryScore)
        {
            string endpoint = BuildRootScoreQueryEndpoint(username, CurrentScore);
            yield return APIManager.Instance.GetRequestWithResult(endpoint, (ok, body, code) =>
            {
                success = ok;
                response = body;
                statusCode = code;
            }, token);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(scoreEndpoint))
            {
                Debug.LogWarning("ScoreManager: scoreEndpoint is empty. Configure it in inspector.");
                yield break;
            }

            ScoreUpdateRequest request = new ScoreUpdateRequest(username, CurrentScore);
            string json = JsonUtility.ToJson(request);

            yield return APIManager.Instance.PostRequestWithResult(NormalizeEndpoint(scoreEndpoint), json, (ok, body, code) =>
            {
                success = ok;
                response = body;
                statusCode = code;
            }, token);
        }

        if (!success)
        {
            string errorMessage = ExtractApiErrorMessage(response);
            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = "No se pudo actualizar el score. Código: " + statusCode;
            }

            Debug.LogError("Score update failed: " + errorMessage + " | Body: " + response);
            yield break;
        }

        Debug.Log("Score actualizado: " + response);
    }

    public void GetLeaderboard()
    {
        GetLeaderboard((entries) =>
        {
            Debug.Log("Leaderboard entries: " + (entries != null ? entries.Length : 0));
        });
    }

    public void GetLeaderboard(Action<LeaderboardEntry[]> callback)
    {
        if (APIManager.Instance == null)
        {
            Debug.LogWarning("ScoreManager: APIManager.Instance is null. Cannot load leaderboard.");
            callback?.Invoke(Array.Empty<LeaderboardEntry>());
            return;
        }

        StartCoroutine(GetLeaderboardCoroutine(callback));
    }

    private IEnumerator GetLeaderboardCoroutine(Action<LeaderboardEntry[]> callback)
    {
        string token = AuthManager.Instance != null ? AuthManager.Instance.GetToken() : null;
        bool success = false;
        string response = null;
        long statusCode = 0;

        if (useRootQueryLeaderboard)
        {
            string rootEndpoint = BuildRootLeaderboardQueryEndpoint();
            yield return APIManager.Instance.GetRequestWithResult(rootEndpoint, (ok, body, code) =>
            {
                success = ok;
                response = body;
                statusCode = code;
            }, token);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(leaderboardEndpoint))
            {
                Debug.LogWarning("ScoreManager: leaderboardEndpoint is empty. Configure it in inspector.");
                callback?.Invoke(Array.Empty<LeaderboardEntry>());
                yield break;
            }

            yield return APIManager.Instance.GetRequestWithResult(NormalizeEndpoint(leaderboardEndpoint), (ok, body, code) =>
            {
                success = ok;
                response = body;
                statusCode = code;
            }, token);
        }

        if (!success)
        {
            string errorMessage = ExtractApiErrorMessage(response);
            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = "No se pudo cargar leaderboard. Código: " + statusCode;
            }

            Debug.LogError("Leaderboard request failed: " + errorMessage + " | Body: " + response);

            LeaderboardEntry[] localFallback = MergeWithLocalRun(Array.Empty<LeaderboardEntry>());
            SortLeaderboard(localFallback);
            callback?.Invoke(localFallback);
            yield break;
        }

        LeaderboardEntry[] entries = ParseLeaderboardResponse(response);
        entries = MergeWithLocalRun(entries);
        SortLeaderboard(entries);
        callback?.Invoke(entries);
    }

    private LeaderboardEntry[] ParseLeaderboardResponse(string response)
    {
        if (string.IsNullOrEmpty(response))
        {
            return Array.Empty<LeaderboardEntry>();
        }

        string trimmed = response.Trim();

        // Case 1: endpoint returns a top-level array
        if (trimmed.StartsWith("["))
        {
            string wrappedResponses = "{\"users\":" + trimmed + "}";
            LeaderboardUserResponseArrayWrapper responseArrayWrapper = SafeFromJson<LeaderboardUserResponseArrayWrapper>(wrappedResponses);
            if (responseArrayWrapper != null && responseArrayWrapper.users != null)
            {
                return MapFromUserResponses(responseArrayWrapper.users);
            }

            LeaderboardUserArrayWrapper userArrayWrapper = SafeFromJson<LeaderboardUserArrayWrapper>(wrappedResponses);
            if (userArrayWrapper != null && userArrayWrapper.users != null)
            {
                return MapFromUsers(userArrayWrapper.users);
            }

            return Array.Empty<LeaderboardEntry>();
        }

        // Case 2: endpoint returns a single object with "usuario"
        UserResponse singleResponse = SafeFromJson<UserResponse>(trimmed);
        if (singleResponse != null && singleResponse.usuario != null)
        {
            return MapFromUserResponses(new UserResponse[] { singleResponse });
        }

        // Case 3: endpoint returns an object with a list key
        LeaderboardObjectWrapper objectWrapper = SafeFromJson<LeaderboardObjectWrapper>(trimmed);
        if (objectWrapper != null)
        {
            if (objectWrapper.leaderboard != null)
            {
                return MapFromUserResponses(objectWrapper.leaderboard);
            }
            if (objectWrapper.users != null)
            {
                return MapFromUserResponses(objectWrapper.users);
            }
            if (objectWrapper.data != null)
            {
                return MapFromUserResponses(objectWrapper.data);
            }
            if (objectWrapper.usuarios != null)
            {
                return MapFromUserResponses(objectWrapper.usuarios);
            }
        }

        LeaderboardObjectUserWrapper objectUserWrapper = SafeFromJson<LeaderboardObjectUserWrapper>(trimmed);
        if (objectUserWrapper == null)
        {
            return Array.Empty<LeaderboardEntry>();
        }

        if (objectUserWrapper.leaderboard != null)
        {
            return MapFromUsers(objectUserWrapper.leaderboard);
        }
        if (objectUserWrapper.users != null)
        {
            return MapFromUsers(objectUserWrapper.users);
        }
        if (objectUserWrapper.data != null)
        {
            return MapFromUsers(objectUserWrapper.data);
        }
        if (objectUserWrapper.usuarios != null)
        {
            return MapFromUsers(objectUserWrapper.usuarios);
        }

        return Array.Empty<LeaderboardEntry>();
    }

    private LeaderboardEntry[] MergeWithLocalRun(LeaderboardEntry[] entries)
    {
        string localUsername = AuthManager.Instance != null ? AuthManager.Instance.GetUsername() : string.Empty;
        int runScore = Mathf.Max(0, GameSessionData.LastScore);
        List<LeaderboardEntry> merged = new List<LeaderboardEntry>(entries ?? Array.Empty<LeaderboardEntry>());

        // Include all locally persisted users (multi-account on same device)
        MergeLocalEntriesInto(merged);

        // Ensure current run is reflected for active user
        if (!string.IsNullOrEmpty(localUsername))
        {
            int persistedBest = GetLocalBestScore(localUsername);
            int localScore = Mathf.Max(runScore, persistedBest);
            UpsertMergedEntry(merged, localUsername, localScore);
        }

        return merged.ToArray();
    }

    private int GetLocalBestScore(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return 0;
        }

        int best = Mathf.Max(0, PlayerPrefs.GetInt(LocalBestScorePrefix + username, 0));
        List<LocalLeaderboardEntry> entries = LoadLocalLeaderboardEntries();
        for (int i = 0; i < entries.Count; i++)
        {
            LocalLeaderboardEntry entry = entries[i];
            if (entry == null || string.IsNullOrEmpty(entry.username))
            {
                continue;
            }

            if (!string.Equals(entry.username, username, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            best = Mathf.Max(best, Mathf.Max(0, entry.score));
        }

        return best;
    }

    private void SaveLocalBestScore(string username, int score)
    {
        if (string.IsNullOrEmpty(username))
        {
            return;
        }

        string key = LocalBestScorePrefix + username;
        int previousBest = Mathf.Max(0, PlayerPrefs.GetInt(key, 0));
        int newBest = Mathf.Max(previousBest, Mathf.Max(0, score));
        PlayerPrefs.SetInt(key, newBest);
        UpsertLocalLeaderboardEntry(username, newBest);
        PlayerPrefs.Save();
    }

    private void UpsertLocalLeaderboardEntry(string username, int score)
    {
        if (string.IsNullOrEmpty(username))
        {
            return;
        }

        List<LocalLeaderboardEntry> localEntries = LoadLocalLeaderboardEntries();
        bool changed = UpsertLocalEntry(localEntries, username, score);
        if (!changed)
        {
            return;
        }

        SaveLocalLeaderboardEntries(localEntries);
    }

    private List<LocalLeaderboardEntry> LoadLocalLeaderboardEntries()
    {
        string json = PlayerPrefs.GetString(LocalLeaderboardKey, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            return new List<LocalLeaderboardEntry>();
        }

        LocalLeaderboardStore store = SafeFromJson<LocalLeaderboardStore>(json);
        if (store == null || store.entries == null)
        {
            return new List<LocalLeaderboardEntry>();
        }

        return new List<LocalLeaderboardEntry>(store.entries);
    }

    private void SaveLocalLeaderboardEntries(List<LocalLeaderboardEntry> entries)
    {
        LocalLeaderboardStore store = new LocalLeaderboardStore
        {
            entries = entries != null ? entries.ToArray() : Array.Empty<LocalLeaderboardEntry>()
        };

        string json = JsonUtility.ToJson(store);
        PlayerPrefs.SetString(LocalLeaderboardKey, json);
    }

    private bool UpsertLocalEntry(List<LocalLeaderboardEntry> entries, string username, int score)
    {
        if (entries == null || string.IsNullOrEmpty(username))
        {
            return false;
        }

        int sanitizedScore = Mathf.Max(0, score);
        for (int i = 0; i < entries.Count; i++)
        {
            LocalLeaderboardEntry entry = entries[i];
            if (entry == null || string.IsNullOrEmpty(entry.username))
            {
                continue;
            }

            if (string.Equals(entry.username, username, StringComparison.OrdinalIgnoreCase))
            {
                if (sanitizedScore > entry.score)
                {
                    entry.score = sanitizedScore;
                    return true;
                }

                return false;
            }
        }

        entries.Add(new LocalLeaderboardEntry
        {
            username = username,
            score = sanitizedScore
        });

        return true;
    }

    private void MergeLocalEntriesInto(List<LeaderboardEntry> merged)
    {
        if (merged == null)
        {
            return;
        }

        List<LocalLeaderboardEntry> localEntries = LoadLocalLeaderboardEntries();
        for (int i = 0; i < localEntries.Count; i++)
        {
            LocalLeaderboardEntry local = localEntries[i];
            if (local == null || string.IsNullOrEmpty(local.username))
            {
                continue;
            }

            UpsertMergedEntry(merged, local.username, local.score);
        }
    }

    private void UpsertMergedEntry(List<LeaderboardEntry> merged, string username, int score)
    {
        if (merged == null || string.IsNullOrEmpty(username))
        {
            return;
        }

        int sanitizedScore = Mathf.Max(0, score);
        int index = merged.FindIndex(e => e != null && string.Equals(e.username, username, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            merged[index].score = Mathf.Max(merged[index].score, sanitizedScore);
            return;
        }

        merged.Add(new LeaderboardEntry
        {
            username = username,
            score = sanitizedScore
        });
    }

    private LeaderboardEntry[] MapFromUserResponses(UserResponse[] responses)
    {
        if (responses == null)
        {
            return Array.Empty<LeaderboardEntry>();
        }

        LeaderboardEntry[] mapped = new LeaderboardEntry[responses.Length];
        for (int i = 0; i < responses.Length; i++)
        {
            User user = responses[i] != null ? responses[i].usuario : null;
            mapped[i] = new LeaderboardEntry
            {
                username = user != null ? user.username : "",
                score = (user != null && user.data != null) ? user.data.score : 0
            };
        }

        return mapped;
    }

    private LeaderboardEntry[] MapFromUsers(User[] users)
    {
        if (users == null)
        {
            return Array.Empty<LeaderboardEntry>();
        }

        LeaderboardEntry[] mapped = new LeaderboardEntry[users.Length];
        for (int i = 0; i < users.Length; i++)
        {
            User user = users[i];
            mapped[i] = new LeaderboardEntry
            {
                username = user != null ? user.username : "",
                score = (user != null && user.data != null) ? user.data.score : 0
            };
        }

        return mapped;
    }

    private T SafeFromJson<T>(string json) where T : class
    {
        if (string.IsNullOrEmpty(json))
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

    private void SortLeaderboard(LeaderboardEntry[] entries)
    {
        if (entries == null || entries.Length <= 1)
        {
            return;
        }

        Array.Sort(entries, (a, b) =>
        {
            int aScore = a != null ? a.score : 0;
            int bScore = b != null ? b.score : 0;
            return bScore.CompareTo(aScore);
        });
    }

    private string ExtractApiErrorMessage(string response)
    {
        if (string.IsNullOrEmpty(response))
        {
            return null;
        }

        ApiErrorResponse err = SafeFromJson<ApiErrorResponse>(response);
        if (err != null && !string.IsNullOrEmpty(err.message))
        {
            return err.message;
        }

        if (response.Contains("Cannot POST") || response.Contains("Cannot GET"))
        {
            return "Ruta no encontrada en backend";
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

    private string BuildRootScoreQueryEndpoint(string username, int score)
    {
        string root = NormalizeEndpoint(rootScoreEndpoint);
        string safeUser = UnityEngine.Networking.UnityWebRequest.EscapeURL(username ?? string.Empty);
        string key = string.IsNullOrEmpty(scoreQueryKey) ? "score" : scoreQueryKey;
        string safeKey = UnityEngine.Networking.UnityWebRequest.EscapeURL(key);
        string safeScore = UnityEngine.Networking.UnityWebRequest.EscapeURL(score.ToString());
        return root + "?username=" + safeUser + "&" + safeKey + "=" + safeScore;
    }

    private string BuildRootLeaderboardQueryEndpoint()
    {
        string root = NormalizeEndpoint(rootLeaderboardEndpoint);
        if (!appendLeaderboardQuery)
        {
            return root;
        }

        string key = string.IsNullOrEmpty(leaderboardQueryKey) ? "leaderboard" : leaderboardQueryKey;
        string value = leaderboardQueryValue ?? "true";
        string safeKey = UnityEngine.Networking.UnityWebRequest.EscapeURL(key);
        string safeValue = UnityEngine.Networking.UnityWebRequest.EscapeURL(value);
        return root + "?" + safeKey + "=" + safeValue;
    }
}