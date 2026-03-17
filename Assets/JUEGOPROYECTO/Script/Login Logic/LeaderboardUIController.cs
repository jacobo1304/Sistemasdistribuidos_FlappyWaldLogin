using TMPro;
using UnityEngine;

public class LeaderboardUIController : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentRoot;
    public LeaderboardRowView rowPrefab;
    public TMP_Text emptyStateText;

    [Header("Behavior")]
    public bool loadOnStart = true;

    private void Start()
    {
        if (loadOnStart)
        {
            RefreshLeaderboard();
        }
    }

    public void RefreshLeaderboard()
    {
        if (contentRoot == null || rowPrefab == null)
        {
            Debug.LogWarning("LeaderboardUIController: Missing contentRoot or rowPrefab reference.");
            return;
        }

        ClearRows();

        if (ScoreManager.Instance == null)
        {
            Debug.LogWarning("LeaderboardUIController: ScoreManager.Instance is null.");
            ShowEmptyState("Leaderboard unavailable");
            return;
        }

        ShowEmptyState("Loading...");

        ScoreManager.Instance.GetLeaderboard((entries) =>
        {
            ClearRows();

            if (entries == null || entries.Length == 0)
            {
                ShowEmptyState("No scores yet");
                return;
            }

            HideEmptyState();

            for (int i = 0; i < entries.Length; i++)
            {
                ScoreManager.LeaderboardEntry entry = entries[i];
                LeaderboardRowView row = Instantiate(rowPrefab, contentRoot);
                row.Bind(i + 1, entry.username, entry.score);
            }
        });
    }

    private void ClearRows()
    {
        if (contentRoot == null)
        {
            return;
        }

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }

    private void ShowEmptyState(string message)
    {
        if (emptyStateText != null)
        {
            emptyStateText.gameObject.SetActive(true);
            emptyStateText.text = message;
        }
    }

    private void HideEmptyState()
    {
        if (emptyStateText != null)
        {
            emptyStateText.gameObject.SetActive(false);
        }
    }
}
