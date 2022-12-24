using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    private List<PlayerInput> playerInputs = new List<PlayerInput>();
    PlayerInputManager playerInputManager;




    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            playerInputManager = FindObjectOfType<PlayerInputManager>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    private void OnEnable()
    {
        if (playerInputManager)
        {
            playerInputManager.onPlayerJoined += SpawnPlayer;
            playerInputManager.onPlayerLeft += OnPlayerLeft;
        }
    }

    private void OnDisable()
    {
        if (playerInputManager)
        {
            playerInputManager.onPlayerJoined -= SpawnPlayer;
            playerInputManager.onPlayerLeft += OnPlayerLeft;
        }
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        SelectSidesGui.instance.PlayerLeft(playerInput);
    }

    public void SpawnPlayer(PlayerInput playerInput)
    {

        Debug.Log("Adding player!");
        if (SelectSidesGui.instance != null)
        {
            SelectSidesGui.instance.PlayerJoined(playerInput);
        }
        else
        {
            Debug.Log("No SelectSidesGui!");
            // we are not in the select sides scene so we use the GameplayManager 
            //GameplayManager.instance.PlayerJoined(playerInput);
        }
    }

    public void DisableJoining(bool shouldDisable = true)
    {
        if (shouldDisable)
        {
            playerInputManager.DisableJoining();
        }
        else
        {
            playerInputManager.EnableJoining();
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

}
