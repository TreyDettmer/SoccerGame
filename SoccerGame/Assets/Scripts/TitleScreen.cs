using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    PlayerInputManager playerInputManager;

    private void Awake()
    {
        PlayerInputManager[] playerInputManagers = FindObjectsOfType<PlayerInputManager>();
        for (int i = playerInputManagers.Length - 1; i >= 0; i--)
        {
            if (playerInputManagers[i] != GetComponent<PlayerInputManager>())
            {
                Destroy(playerInputManagers[i].gameObject);
            }
        }
        playerInputManager = GetComponent<PlayerInputManager>();
        HumanController[] humanControllers = FindObjectsOfType<HumanController>();
        for (int i = 0; i < humanControllers.Length; i++)
        {
            //Destroy(humanControllers[i].gameObject);
        }
        MatchSettings matchSettings = FindObjectOfType<MatchSettings>();
        if (matchSettings)
        {
            Destroy(matchSettings.gameObject);
        }
        Time.timeScale = 1f;

    }
    // Start is called before the first frame update
    void Start()
    {
        //Animator[] uiAnimators = GetComponentsInChildren<Animator>();
        //for (int i = 0; i < uiAnimators.Length; i++)
        //{
        //    uiAnimators[i].Play(0);
        //}
        AudioManager.instance.StopAllSounds();
        AudioManager.instance.Play("Theme1");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += LoadNextScene;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= LoadNextScene;
    }

    void LoadNextScene(PlayerInput playerInput)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
