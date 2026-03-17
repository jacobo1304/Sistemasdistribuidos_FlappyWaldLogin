using UnityEngine;
using System.Collections;
using System;

public class ScoreManager : MonoBehaviour
{
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
    }

    public static ScoreManager Instance;

    [Header("Endpoints")]
    public string scoreEndpoint = "";
    public string leaderboardEndpoint = "";

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
        if (APIManager.Instance == null)
        {
            Debug.LogWarning("ScoreManager: APIManager.Instance is null. Score was not submitted.");
            return;
        }

        if (AuthManager.Instance == null)
        {
            Debug.LogWarning("ScoreManager: AuthManager.Instance is null. Score was not submitted.");
            return;
        }

        if (string.IsNullOrWhiteSpace(scoreEndpoint))
        {
            Debug.LogWarning("ScoreManager: scoreEndpoint is empty. Configure it in inspector.");
            return;
        }

        string username = AuthManager.Instance.GetUsername();
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("ScoreManager: Username unavailable. Score was not submitted.");
            return;
        }

        ScoreUpdateRequest request = new ScoreUpdateRequest(username, CurrentScore);
        string json = JsonUtility.ToJson(request);
        string token = AuthManager.Instance.GetToken();

        StartCoroutine(APIManager.Instance.PostRequest(NormalizeEndpoint(scoreEndpoint), json, (response) =>
        {
            Debug.Log("Score actualizado: " + response);
        }, token));
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

        if (string.IsNullOrWhiteSpace(leaderboardEndpoint))
        {
            Debug.LogWarning("ScoreManager: leaderboardEndpoint is empty. Configure it in inspector.");
            callback?.Invoke(Array.Empty<LeaderboardEntry>());
            return;
        }

        string token = AuthManager.Instance != null ? AuthManager.Instance.GetToken() : null;
        StartCoroutine(APIManager.Instance.GetRequest(NormalizeEndpoint(leaderboardEndpoint), (response) =>
        {
            Debug.Log("Leaderboard: " + response);

            LeaderboardEntry[] entries = ParseLeaderboardResponse(response);
            callback?.Invoke(entries);
        }, token));
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
        if (objectWrapper == null)
        {
            return Array.Empty<LeaderboardEntry>();
        }

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

        return Array.Empty<LeaderboardEntry>();
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

    private string NormalizeEndpoint(string endpoint)
    {
        if (string.IsNullOrEmpty(endpoint))
        {
            return string.Empty;
        }

        return endpoint.StartsWith("/") ? endpoint : "/" + endpoint;
    }
}