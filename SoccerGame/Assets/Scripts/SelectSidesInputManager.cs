using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectSidesInputManager : MonoBehaviour
{
    public static SelectSidesInputManager instance;
    PlayerInputManager playerInputManager;
    public List<PlayerInput> playerInputs = new List<PlayerInput>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            playerInputManager = FindObjectOfType<PlayerInputManager>();
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
        SelectSidesGui.instance.PlayerJoined(playerInput);



    }

}
