using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameplayGui : MonoBehaviour
{

    public static GameplayGui instance;

    [SerializeField]
    TextMeshProUGUI scoreLabel;
    [SerializeField]
    TextMeshProUGUI countdownLabel;
    [SerializeField]
    RectTransform goalLabelTransform;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleScoreLabel(bool enable)
    {
        scoreLabel.enabled = enable;
    }

    public void UpdateScore(int team0Score, int team1Score)
    {
        scoreLabel.text = team0Score + " - " + team1Score;
    }

    public void UpdateCountdown(string newText)
    {
        countdownLabel.text = newText;
    }

    public IEnumerator GoalLabelRoutine(float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float val = Mathf.Lerp(0f, 1600f, t) - 800f;
            goalLabelTransform.anchoredPosition = new Vector2(val, goalLabelTransform.anchoredPosition.y);
            yield return null;
        }
        goalLabelTransform.anchoredPosition = new Vector2(-800f, goalLabelTransform.anchoredPosition.y);
    }

}
