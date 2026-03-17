using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    public int points = 1;

    private bool consumed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (consumed)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            consumed = true;
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(points);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            consumed = true;
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(points);
            }
        }
    }
}
