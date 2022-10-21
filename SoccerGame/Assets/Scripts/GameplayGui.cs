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

}
