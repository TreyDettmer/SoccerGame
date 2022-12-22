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
    TextMeshProUGUI scoreLabel;
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
    public TextMeshProUGUI pauseMenuScoreLabel;

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
        scoreLabel.enabled = enable;
        gameClockLabel.enabled = enable;
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
        pauseMenuScoreLabel.text = scoreLabel.text;
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
        pauseMenu.SetActive(enable);
        
    }
    public void OnQuitButtonPressed()
    {
        GameplayManager.instance.QuitGame();
    }    

}
