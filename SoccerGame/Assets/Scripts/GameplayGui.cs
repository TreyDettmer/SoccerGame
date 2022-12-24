using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class GameplayGui : MonoBehaviour
{

    public static GameplayGui instance;
    public EventSystem eventSystem;
    [SerializeField]
    TextMeshProUGUI team0ScoreLabel;
    [SerializeField]
    TextMeshProUGUI scoreDashLabel;
    [SerializeField]
    TextMeshProUGUI team1ScoreLabel;
    [SerializeField]
    TextMeshProUGUI countdownLabel;
    [SerializeField]
    RectTransform goalLabelTransform;
    public TextMeshProUGUI gameClockLabel;
    public GameObject pauseMenu;
    public GameObject restartButton;
    public GameObject resumeButton;

    public TextMeshProUGUI pauseMenuLabel;
    public TextMeshProUGUI pauseMenuGameClockLabel;
    public TextMeshProUGUI pauseMenuTeam0ScoreLabel;
    public TextMeshProUGUI pauseMenuScoreDashLabel;
    public TextMeshProUGUI pauseMenuTeam1ScoreLabel;

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
        eventSystem.SetSelectedGameObject(resumeButton);
        EnablePauseMenu(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleScoreLabel(bool enable)
    {
        team0ScoreLabel.enabled = enable;
        team1ScoreLabel.enabled = enable;
        scoreDashLabel.enabled = enable;
        gameClockLabel.enabled = enable;
    }

    public void UpdateScore(int team0Score, int team1Score)
    {
        team0ScoreLabel.text = team0Score.ToString();
        team1ScoreLabel.text = team1Score.ToString();
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

    public void OnPauseMenuRestartButtonPressed()
    {
        GameplayManager.instance.RestartGame();
        EnablePauseMenu(false);
    }

    public void OnPauseMenuResumeButtonPressed()
    {
        GameplayManager.instance.ResumeGame();
        EnablePauseMenu(false);
    }

    public void EnablePauseMenu(bool enable)
    {
        restartButton.SetActive(false);
        resumeButton.SetActive(true);
        eventSystem.SetSelectedGameObject(resumeButton);
        pauseMenuGameClockLabel.text = gameClockLabel.text;
        pauseMenuTeam0ScoreLabel.text = team0ScoreLabel.text;
        pauseMenuTeam1ScoreLabel.text = team1ScoreLabel.text;
        if (GameClock.instance.elapsedMinutes == 45f)
        {
            pauseMenuLabel.text = "Halftime";
            resumeButton.SetActive(true);
        }
        else if (GameClock.instance.elapsedMinutes == 90f)
        {
            if (GameplayManager.instance.team0Score != GameplayManager.instance.team1Score)
            {
                pauseMenuLabel.text = "Game Over";
                eventSystem.SetSelectedGameObject(restartButton);
                resumeButton.SetActive(false);
                restartButton.SetActive(true);
            }
            else
            {
                pauseMenuLabel.text = "End of Regulation";
                eventSystem.SetSelectedGameObject(resumeButton);
                resumeButton.SetActive(true);

            }
        }
        else if (GameClock.instance.elapsedMinutes == 105f)
        {
            pauseMenuLabel.text = "Extra Time Halftime";
            eventSystem.SetSelectedGameObject(resumeButton);
        }
        else if (GameClock.instance.elapsedMinutes == 120f)
        {
            if (GameplayManager.instance.team0Score != GameplayManager.instance.team1Score)
            {
                pauseMenuLabel.text = "Game Over";
                eventSystem.SetSelectedGameObject(restartButton);
                resumeButton.SetActive(false);
                restartButton.SetActive(true);
            }
            else
            {
                pauseMenuLabel.text = "End of Extra Time";
                eventSystem.SetSelectedGameObject(restartButton);
                resumeButton.SetActive(false);
                restartButton.SetActive(true);

            }

        }
        else
        {
            pauseMenuLabel.text = "Game Paused";
        }
        pauseMenu.SetActive(enable);
        
    }
    public void OnQuitButtonPressed()
    {
        GameplayManager.instance.QuitGame();
    }    

}
