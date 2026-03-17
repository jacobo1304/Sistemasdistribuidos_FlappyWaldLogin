using TMPro;
using UnityEngine;

public class LeaderboardRowView : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text usernameText;
    public TMP_Text scoreText;

    public void Bind(int rank, string username, int score)
    {
        if (rankText != null)
        {
            rankText.text = rank.ToString();
        }

        if (usernameText != null)
        {
            usernameText.text = string.IsNullOrEmpty(username) ? "Unknown" : username;
        }

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }
}
