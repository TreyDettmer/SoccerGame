using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager instance;
    private int team0Score = 0;
    private int team1Score = 0;
    private List<PlayerInput> playerInputs = new List<PlayerInput>();
    private List<PlayerController> players = new List<PlayerController>();
    [SerializeField]
    private List<Transform> spawnPoints;
    [SerializeField]
    Transform ballSpawnPoint;

    [SerializeField]
    LayerMask team0CullingMask;
    [SerializeField]
    LayerMask team1CullingMask;

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

    public void SpawnPlayer(PlayerInput playerInput)
    {
        playerInputs.Add(playerInput);
        players.Add(playerInput.GetComponent<PlayerController>());
        Transform playerParent = playerInput.transform.parent;
        playerParent.position = spawnPoints[playerInputs.Count - 1].position;
        playerParent.rotation = spawnPoints[playerInputs.Count - 1].rotation;
        playerInput.GetComponent<PlayerController>().teamIndex = playerInputs.Count - 1;
        if (playerParent.forward.z < 0)
        {
            playerParent.GetComponentInChildren<FollowPlayer>().lookInOppositeDirection = true;
        }
        if (playerInputs.Count - 1 == 0)
        {
            playerParent.GetComponentInChildren<Camera>().cullingMask = team0CullingMask;
        }
        else
        {
            playerParent.GetComponentInChildren<Camera>().cullingMask = team1CullingMask;
        }
    }

    public void GoalScored(int teamGoalScoredOn)
    {
        if (teamGoalScoredOn == 0)
        {
            team1Score += 1;
        }
        else
        {
            team0Score += 1;
        }
        GameplayGui.instance.UpdateScore(team0Score, team1Score);
        ResetPlayers();
        ResetBall();
    }

    public void ResetPlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {
            
            players[i].transform.position = spawnPoints[i].position;
            players[i].transform.rotation = spawnPoints[i].rotation;
            players[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    public void ResetBall()
    {
        Ball ball = FindObjectOfType<Ball>();
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        ball.transform.position = ballSpawnPoint.position;
    }
}
