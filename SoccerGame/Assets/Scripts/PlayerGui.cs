using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerGui : MonoBehaviour
{

    [SerializeField]
    Gradient powerColorGradient;

    public RawImage powerMeter;
    [SerializeField]
    GameObject powerMeterObject;
    [SerializeField]
    RectTransform powerMeterRightTransform;

    bool isPowerMeterOnLeft = true;

    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<Camera>().rect.x == .5f)
        {
            isPowerMeterOnLeft = false;
        }
        if (isPowerMeterOnLeft == false)
        {
            powerMeterObject.GetComponent<RectTransform>().position = powerMeterRightTransform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdatePowerMeter(float powerPercent)
    {

        powerMeter.transform.localScale = new Vector3(powerPercent, powerMeter.transform.localScale.y, powerMeter.transform.localScale.z);
        powerMeter.color = powerColorGradient.Evaluate(powerPercent);
        
    }
}
