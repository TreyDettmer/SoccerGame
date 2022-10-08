using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private List<PlayerInput> playerInputs = new List<PlayerInput>();
    [SerializeField]
    private List<Transform> spawnPoints;
    [SerializeField]
    private GameObject playerObjectPrefab;
    PlayerInputManager playerInputManager;
    List<InputDevice> inputDevices;
    bool isSplitScreen = false;
    public bool isLocalMultiplayer = false;


    private void Awake()
    {

        playerInputManager = FindObjectOfType<PlayerInputManager>();
        playerInputManager.playerPrefab = playerObjectPrefab;
    }

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += SpawnPlayer;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= SpawnPlayer;
    }

    public void SpawnPlayer(PlayerInput playerInput)
    {
        Debug.Log("Adding player!");
        playerInputs.Add(playerInput);
        Transform playerParent = playerInput.transform.parent;
        playerParent.position = spawnPoints[playerInputs.Count - 1].position;
        playerParent.rotation = spawnPoints[playerInputs.Count - 1].rotation;
        if (playerParent.forward.z < 0)
        {
            playerParent.GetComponentInChildren<FollowPlayer>().lookInOppositeDirection = true;
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
        inputDevices = new List<InputDevice>();
        if (!playerInputManager)
        {
            Debug.LogError("No PlayerInputManager detected");
            return;
        }
        foreach (InputDevice device in InputSystem.devices)
        {
            // Mouse is not a valid controller
            if (device.displayName != "Mouse" && device.displayName != "Keyboard")
            {
                inputDevices.Add(device);
                Debug.Log(device.displayName);
            }

        }
        if (inputDevices.Count > 1 && isLocalMultiplayer)
        {
            isSplitScreen = true;
            playerInputManager.splitScreen = isSplitScreen;
        }
        for (int i = 0; i < inputDevices.Count; i++)
        {
            if (inputDevices[i].displayName == "Keyboard")
            {
                playerInputManager.JoinPlayer(i, -1, "Keyboard and mouse", inputDevices[i]);
            }
            else
            {
                playerInputManager.JoinPlayer(i, -1, "Gamepad", inputDevices[i]);
            }
        }
        
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}