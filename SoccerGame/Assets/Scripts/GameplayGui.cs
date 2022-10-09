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
    Gradient powerColorGradient;
    [SerializeField]
    RawImage powerMeter0;
    [SerializeField]
    RawImage powerMeter1;

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


    public void UpdateScore(int team0Score, int team1Score)
    {
        scoreLabel.text = team0Score + " - " + team1Score;
    }

    public void UpdatePowerMeter(int teamIndex, float powerPercent)
    {
        if (teamIndex == 0)
        {
            powerMeter0.transform.localScale = new Vector3(powerPercent, powerMeter0.transform.localScale.y, powerMeter0.transform.localScale.z);
            powerMeter0.color = powerColorGradient.Evaluate(powerPercent);
        }
        else if (teamIndex == 1)
        {
            powerMeter1.transform.localScale = new Vector3(powerPercent, powerMeter1.transform.localScale.y, powerMeter1.transform.localScale.z);
            powerMeter1.color = powerColorGradient.Evaluate(powerPercent);
        }
    }
}
