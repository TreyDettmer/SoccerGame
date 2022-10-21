using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class GameplayManager : MonoBehaviour
{
    [HideInInspector]
    public static GameplayManager instance;
    private int team0Score = 0;
    private int team1Score = 0;
    private List<PlayerInput> playerInputs = new List<PlayerInput>();
    private List<PlayerController> players = new List<PlayerController>();
    private List<PlayerController> restartRequesters = new List<PlayerController>();
    [SerializeField]
    private List<Transform> spawnPoints;
    [SerializeField]
    Transform ballSpawnPoint;

    [SerializeField]
    float countdownTime = 3f;

    public Material team0PlayingMaterial;
    public Material team0PenalizedMaterial;
    public Material team1PlayingMaterial;
    public Material team1PenalizedMaterial;

    [SerializeField]
    Transform ballSpawnPointTeam0;
    [SerializeField]
    Transform ballSpawnPointTeam1;

    [SerializeField]
    LayerMask team0CullingMask;
    [SerializeField]
    LayerMask team1CullingMask;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            DOTween.Init();
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
        StartCoroutine(GameCountdown());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnPlayer(PlayerInput playerInput, PlayerController.ControllerType controllerType)
    {
        playerInputs.Add(playerInput);
        players.Add(playerInput.GetComponent<PlayerController>());
        Transform playerParent = playerInput.transform.parent;
        playerParent.position = spawnPoints[playerInputs.Count - 1].position;
        playerParent.rotation = spawnPoints[playerInputs.Count - 1].rotation;
        playerInput.GetComponent<PlayerController>().teamIndex = (playerInputs.Count - 1) % 2;
        playerInput.GetComponent<PlayerController>().controllerType = PlayerController.ControllerType.Switch;
        if ((playerInputs.Count - 1) % 2 == 0)
        {
            playerInput.GetComponent<PlayerController>().playingMaterial = team0PlayingMaterial;
            playerInput.GetComponent<PlayerController>().penalizedMaterial = team0PenalizedMaterial;
            playerInput.GetComponent<PlayerController>().UpdateMaterial();
            MinimapController.instance.CreatePlayerCanvasPrefab(playerInput.GetComponent<PlayerController>(), new Color(0f, 0f, 1f));
        }
        else
        {
            playerInput.GetComponent<PlayerController>().playingMaterial = team1PlayingMaterial;
            playerInput.GetComponent<PlayerController>().penalizedMaterial = team1PenalizedMaterial;
            playerInput.GetComponent<PlayerController>().UpdateMaterial();
            MinimapController.instance.CreatePlayerCanvasPrefab(playerInput.GetComponent<PlayerController>(), new Color(1f, 0f, 0f));
        }
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
        StartCoroutine(GameCountdown(2f));
    }

    public void ResetPlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {
            
            players[i].transform.position = spawnPoints[i].position;
            players[i].transform.rotation = spawnPoints[i].rotation;
            players[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
            players[i].ResetValues();
        }
    }

    public void ResetBall(int teamIndex = -1)
    {
        Ball ball = FindObjectOfType<Ball>();
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        if (teamIndex == 0)
        {
            ball.transform.position = ballSpawnPointTeam0.position;
        }
        else if (teamIndex == 1)
        {
            ball.transform.position = ballSpawnPointTeam1.position;
        }
        else
        {
            ball.transform.position = ballSpawnPoint.position;
        }
        ball.SetOwner(null);
        
    }

    public void HandleOutOfBounds(int teamIndex = -1)
    {
        ResetPlayers();
        ResetBall(teamIndex);
        StartCoroutine(GameCountdown(2f));
    }

    public void PlayerFoul(PlayerController fouler, PlayerController fouled)
    {
        fouler.UpdatePlayerState(PlayerController.PlayerState.Penalized);
    }

    public void PlayerRequestedRestart(PlayerController player, bool canceledRequest = false)
    {
        if (canceledRequest)
        {
            if (restartRequesters.Contains(player))
            {
                restartRequesters.Remove(player);
            }
            
        }
        else
        {
            if (!restartRequesters.Contains(player))
            {
                restartRequesters.Add(player);
            }
            
        }
        if (restartRequesters.Count >= players.Count)
        {
            restartRequesters.Clear();
            RestartGame();
        }
    }
    public void RestartGame()
    {

        ResetPlayers();
        ResetBall();
        team0Score = 0;
        team1Score = 0;
        GameplayGui.instance.UpdateScore(team0Score, team1Score);
        StartCoroutine(GameCountdown());
    }

    IEnumerator GameCountdown(float _countdownTime = -1f)
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].UpdatePlayerState(PlayerController.PlayerState.Waiting);
        }
        if (_countdownTime == -1f)
        {
            GameplayGui.instance.ToggleScoreLabel(false);
        }
        
        float currentCountdownTime = _countdownTime == -1f ? countdownTime : _countdownTime;
        while (currentCountdownTime > 0f)
        {
            GameplayGui.instance.UpdateCountdown(currentCountdownTime.ToString());
            yield return new WaitForSeconds(1f);
            currentCountdownTime -= 1f;
        }
        GameplayGui.instance.UpdateCountdown("Go");
        for (int i = 0; i < players.Count; i++)
        {
            players[i].UpdatePlayerState(PlayerController.PlayerState.Playing);
        }
        yield return new WaitForSeconds(1f);
        GameplayGui.instance.UpdateCountdown("");
        GameplayGui.instance.ToggleScoreLabel(true);


    }
}
