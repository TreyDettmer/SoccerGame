using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameClock : MonoBehaviour
{
    public static GameClock instance;
    [HideInInspector] public float elapsedSeconds = 0f;
    [HideInInspector] public float elapsedMinutes = 0f;
    public bool hasStarted = false;
    public bool isPaused = true;
    [HideInInspector] public float realTimeGameLengthInMinutes = 1f;
    // how long a second is given the real game length
    private float calculatedSecondLength;
    private float startTime = 0f;
    private float timeOfLastTick = 0f;
    string minutesText = "00";
    string secondsText = "00";


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }




    // Update is called once per frame
    void Update()
    {
        if (hasStarted)
        {
            if (isPaused)
            {
                return;
            }
            float elapsedTime = Time.time - startTime;

            if (Time.time - timeOfLastTick >= calculatedSecondLength)
            {
                elapsedSeconds += 1;
                if (elapsedSeconds >= 60f)
                {
                    elapsedMinutes += 1;
                    elapsedSeconds = 0f;
                }
                minutesText = elapsedMinutes >= 10 ? ((int)elapsedMinutes).ToString() : "0" + ((int)elapsedMinutes).ToString();
                secondsText = elapsedSeconds >= 10 ? ((int)elapsedSeconds).ToString() : "0" + ((int)elapsedSeconds).ToString();
                GameplayGui.instance.gameClockLabel.text = minutesText + ":" + secondsText;
                timeOfLastTick = Time.time;
                //Debug.Log(elapsedMinutes + ":" + elapsedSeconds + " Actual time passed: " + elapsedTime);
            }
            if ((elapsedMinutes == 45f || elapsedMinutes == 90f || elapsedMinutes == 105f || elapsedMinutes == 120f) && elapsedSeconds == 0f)
            {
                GameplayManager.instance.PauseForPeriodEnd();
            }
            
            
            
        }
    }

    public void ResumeClock()
    {
        startTime = Time.time;
        if (!hasStarted)
        {
            hasStarted = true;
        }
        isPaused = false;
        float calculatedMinuteLength = realTimeGameLengthInMinutes / 90f;
        calculatedSecondLength = calculatedMinuteLength / 60f;
    }

    public void ResetClock()
    {
        isPaused = true;
        hasStarted = false;
        elapsedMinutes = 0f;
        elapsedSeconds = 0f;
    }
}
