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
    private List<Player> players = new List<Player>();
    private List<Player> restartRequesters = new List<Player>();
    [SerializeField]
    private GameObject aiPlayerObjectPrefab;
    public int numberOfAi = 1;
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

    private AudioSource audioSource;

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
        for (int i = 0; i < numberOfAi; i++)
        {
            GameObject aiPlayer = Instantiate(aiPlayerObjectPrefab);
            SpawnPlayer(aiPlayer.GetComponentInChildren<Player>(),Player.ControllerType.AI);

        }
        StartCoroutine(GameCountdown());
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnPlayer(PlayerInput playerInput, Player.ControllerType controllerType)
    {
        playerInputs.Add(playerInput);
        players.Add(playerInput.GetComponent<Player>());
        Transform playerParent = playerInput.transform.parent;
        playerParent.position = spawnPoints[players.Count - 1].position;
        playerParent.rotation = spawnPoints[players.Count - 1].rotation;
        playerInput.GetComponent<Player>().teamIndex = (players.Count - 1) % 2;
        playerInput.GetComponent<Player>().controllerType = controllerType;
        if ((players.Count - 1) % 2 == 0)
        {
            playerInput.GetComponent<Player>().playingMaterial = team0PlayingMaterial;
            playerInput.GetComponent<Player>().penalizedMaterial = team0PenalizedMaterial;
            playerInput.GetComponent<Player>().UpdateMaterial();
            MinimapController.instance.CreatePlayerCanvasPrefab(playerInput.GetComponent<Player>(), new Color(0f, 0f, 1f));
        }
        else
        {
            playerInput.GetComponent<Player>().playingMaterial = team1PlayingMaterial;
            playerInput.GetComponent<Player>().penalizedMaterial = team1PenalizedMaterial;
            playerInput.GetComponent<Player>().UpdateMaterial();
            MinimapController.instance.CreatePlayerCanvasPrefab(playerInput.GetComponent<Player>(), new Color(1f, 0f, 0f));
        }
        if (playerParent.forward.z < 0)
        {
            if (playerInput.GetComponentInChildren<FollowPlayer>())
            {
                playerParent.GetComponentInChildren<FollowPlayer>().lookInOppositeDirection = true;
            }
        }
        if (playerParent.GetComponentInChildren<Camera>())
        {
            if (players.Count - 1 == 0)
            {

                playerParent.GetComponentInChildren<Camera>().cullingMask = team0CullingMask;
            }
            else
            {
                playerParent.GetComponentInChildren<Camera>().cullingMask = team1CullingMask;
            }
        }
    }

    public void SpawnPlayer(Player player, Player.ControllerType controllerType)
    {

        players.Add(player);
        Transform playerParent = player.transform.parent;
        playerParent.position = spawnPoints[players.Count - 1].position;
        playerParent.rotation = spawnPoints[players.Count - 1].rotation;
        player.teamIndex = (players.Count - 1) % 2;
        player.controllerType = controllerType;
        if ((players.Count - 1) % 2 == 0)
        {
            player.playingMaterial = team0PlayingMaterial;
            player.penalizedMaterial = team0PenalizedMaterial;
            player.UpdateMaterial();
            MinimapController.instance.CreatePlayerCanvasPrefab(player, new Color(0f, 0f, 1f));
        }
        else
        {
            player.playingMaterial = team1PlayingMaterial;
            player.penalizedMaterial = team1PenalizedMaterial;
            player.UpdateMaterial();
            MinimapController.instance.CreatePlayerCanvasPrefab(player, new Color(1f, 0f, 0f));
        }
    }

    public void GoalScored(int teamGoalScoredOn)
    {
        audioSource.Play();
        Goal[] goals = FindObjectsOfType<Goal>();
        if (teamGoalScoredOn == 0)
        {
            team1Score += 1;
        }
        else
        {
            team0Score += 1;
        }
        GameplayGui.instance.UpdateScore(team0Score, team1Score);
        
        for (int i = 0; i < goals.Length; i++)
        {
            goals[i].isEnabled = false;
        }
        
        StartCoroutine(PostGoalRoutine(2f));
        StartCoroutine(GameplayGui.instance.GoalLabelRoutine(2f));
    }


    IEnumerator PostGoalRoutine(float duration)
    {
        Time.timeScale = .2f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        ResetPlayers();
        ResetBall();
        StartCoroutine(GameCountdown(2f));
        Goal[] goals = FindObjectsOfType<Goal>();
        for (int i = 0; i < goals.Length; i++)
        {
            goals[i].isEnabled = true;
        }
        
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

    public void PlayerFoul(Player fouler, Player fouled)
    {
        fouler.UpdatePlayerState(Player.PlayerState.Penalized);
    }

    public void PlayerRequestedRestart(Player player, bool canceledRequest = false)
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
            players[i].UpdatePlayerState(Player.PlayerState.Waiting);
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
