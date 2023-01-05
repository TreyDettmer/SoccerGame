using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PregameGui : MonoBehaviour
{
    public Slider matchLengthSlider;
    public TextMeshProUGUI matchLengthText;
    // Start is called before the first frame update
    void Start()
    {
        matchLengthSlider.value = MatchSettings.instance.gameLengthInMinutes;
        matchLengthSlider.onValueChanged.AddListener(delegate { OnMatchLengthSliderValueChanged(); });
        matchLengthText.text = ((int)matchLengthSlider.value).ToString() + " minutes";
        FindObjectOfType<InputManager>().DisableJoining(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMatchLengthSliderValueChanged()
    {
        MatchSettings.instance.gameLengthInMinutes = matchLengthSlider.value;
        matchLengthText.text = ((int)matchLengthSlider.value).ToString() + " minutes";
        AudioManager.instance.Play("UI_Increment");
    }

    public void StartMatch()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void GoBack()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void OnButtonSelected()
    {
        AudioManager.instance.Play("UI_Click");
    }
}
