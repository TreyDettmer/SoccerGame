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
    private List<Player[]> players = new List<Player[]>();
    private List<Player[]>[] teams = { new List<Player[]>(), new List<Player[]>() };
    private List<Player[]> restartRequesters = new List<Player[]>();
    [SerializeField]
    private GameObject aiPlayerObjectPrefab;
    public int numberOfAi = 1;
    public Transform team0SpawnPointParent;
    public Transform team1SpawnPointParent;
    private List<Transform> team0SpawnPoints = new List<Transform>();
    private List<Transform> team1SpawnPoints = new List<Transform>();

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
    private Goal[] goals;
    private Ball ball;
    int numberOfAiOnTeam0 = 1;
    int numberOfAiOnTeam1 = 1;



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


    void LoadMatchSettings()
    {
        if (MatchSettings.instance == null)
        {

        }
        else
        {

            numberOfAiOnTeam0 = MatchSettings.instance.numberOfAiOnTeam0;
            numberOfAiOnTeam1 = MatchSettings.instance.numberOfAiOnTeam1;
            List<PlayerInput> team0PlayerInputs = MatchSettings.instance.team0PlayerInputs;
            List<PlayerInput> team1PlayerInputs = MatchSettings.instance.team1PlayerInputs;
            for (int i = 0; i < team0PlayerInputs.Count; i++)
            {
                Player[] playerComponents = team0PlayerInputs[i].GetComponents<Player>();
                teams[0].Add(playerComponents);
                playerInputs.Add(team0PlayerInputs[i]);
                players.Add(playerComponents);
                team0PlayerInputs[i].GetComponent<PlayerController>().UpdateGameState(PlayerController.GameState.Gameplay);
            }
            for (int i = 0; i < team1PlayerInputs.Count; i++)
            {
                Player[] playerComponents = team1PlayerInputs[i].GetComponents<Player>();
                teams[1].Add(playerComponents);
                playerInputs.Add(team1PlayerInputs[i]);
                players.Add(playerComponents);
                team1PlayerInputs[i].GetComponent<PlayerController>().UpdateGameState(PlayerController.GameState.Gameplay);
            }


        }


        
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        DOTween.Init();
        goals = FindObjectsOfType<Goal>();
        ball = FindObjectOfType<Ball>();
        audioSource = GetComponent<AudioSource>();
        

        for (int i = 0; i < team0SpawnPointParent.childCount; i++)
        {
            team0SpawnPoints.Add(team0SpawnPointParent.GetChild(i));
        }
        for (int i = 0; i < team1SpawnPointParent.childCount; i++)
        {
            team1SpawnPoints.Add(team1SpawnPointParent.GetChild(i));
        }

        LoadMatchSettings();

        if (MatchSettings.instance != null)
        {
            StartGame();
        }

    }


    void StartGame()
    {

        SpawnAiPlayers();

        for (int i = 0; i < teams[0].Count; i++)
        {
            SpawnPlayer(teams[0][i], team0SpawnPoints[i]);
        }
        for (int i = 0; i < teams[1].Count; i++)
        {
            SpawnPlayer(teams[1][i], team1SpawnPoints[i]);
        }
        if (playerInputs.Count > 1)
        {
            FindObjectOfType<PlayerInputManager>().splitScreen = true;
        }

        FindObjectOfType<InputManager>().DisableJoining();
        AssignTeammatesAndOpponents();
        StartCoroutine(GameCountdown());

        UpdateTeamWithBall(-1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnPlayer(Player[] playerComponents, Transform spawnPoint)
    {
        Transform playerParent = playerComponents[0].transform.parent;
        playerParent.transform.rotation = spawnPoint.rotation;
        playerParent.transform.position = spawnPoint.position;
        playerComponents[0].transform.rotation = spawnPoint.rotation;
        playerComponents[0].transform.position = spawnPoint.position;

        playerComponents[0].GetComponent<Rigidbody>().velocity = Vector3.zero;
        playerComponents[0].ResetValues();
        playerComponents[1].ResetValues();
        if (playerComponents[0].teamIndex == 0)
        {
            playerComponents[0].playingMaterial = team0PlayingMaterial;
            playerComponents[0].penalizedMaterial = team0PenalizedMaterial;
            playerComponents[1].playingMaterial = team0PlayingMaterial;
            playerComponents[1].penalizedMaterial = team0PenalizedMaterial;
            playerComponents[0].UpdateMaterial();
            MinimapController.instance.CreatePlayerCanvasPrefab(playerComponents[0], new Color(0f, 0f, 1f));
        }
        else
        {
            playerComponents[0].playingMaterial = team1PlayingMaterial;
            playerComponents[0].penalizedMaterial = team1PenalizedMaterial;
            playerComponents[1].playingMaterial = team1PlayingMaterial;
            playerComponents[1].penalizedMaterial = team1PenalizedMaterial;
            playerComponents[0].UpdateMaterial();
            MinimapController.instance.CreatePlayerCanvasPrefab(playerComponents[0], new Color(1f, 0f, 0f));
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

    void SpawnAiPlayers()
    {
        for (int i = 0; i < numberOfAiOnTeam0; i++)
        {
            GameObject aiPlayer = Instantiate(aiPlayerObjectPrefab);
            Player[] playerComponents = aiPlayer.GetComponentsInChildren<Player>();
            for (int index = 0; index < playerComponents.Length; index++)
            {
                playerComponents[index].teamIndex = 0;
            }
            teams[0].Add(playerComponents);
            players.Add(playerComponents);

        }
        for (int i = 0; i < numberOfAiOnTeam1; i++)
        {
            GameObject aiPlayer = Instantiate(aiPlayerObjectPrefab);
            Player[] playerComponents = aiPlayer.GetComponentsInChildren<Player>();
            for (int index = 0; index < playerComponents.Length; index++)
            {
                playerComponents[index].teamIndex = 1;
            }
            teams[1].Add(playerComponents);
            players.Add(playerComponents);
        }
    }

    public void PlayerJoined(PlayerInput playerInput)
    {

        Player[] playerComponents = playerInput.GetComponents<Player>();
        for (int i = 0; i < playerComponents.Length; i++)
        {
            playerComponents[i].teamIndex = 0;
        }
        teams[0].Add(playerComponents);
        playerInputs.Add(playerInput);
        players.Add(playerComponents);
        playerInput.GetComponent<PlayerController>().UpdateGameState(PlayerController.GameState.Gameplay);
        if (playerInputs.Count > 0)
        {
            // Start Game since we actually have players
            StartGame();
        }
    }

    public void SpawnPlayer(PlayerInput playerInput, Player.ControllerType controllerType)
    {
        //playerInputs.Add(playerInput);
        //Player player = playerInput.GetComponent<Player>();
        //players.Add(player);
        
        //Transform playerParent = playerInput.transform.parent;
        //playerParent.position = spawnPoints[players.Count - 1].position;
        //playerParent.rotation = spawnPoints[players.Count - 1].rotation;
        //player.teamIndex = (players.Count - 1) % 2;
        //teams[player.teamIndex].Add(player);
        //player.controllerType = controllerType;
        //if ((players.Count - 1) % 2 == 0)
        //{
        //    player.playingMaterial = team0PlayingMaterial;
        //    player.penalizedMaterial = team0PenalizedMaterial;
        //    player.UpdateMaterial();
        //    MinimapController.instance.CreatePlayerCanvasPrefab(player, new Color(0f, 0f, 1f));
        //}
        //else
        //{
        //    player.playingMaterial = team1PlayingMaterial;
        //    player.penalizedMaterial = team1PenalizedMaterial;
        //    player.UpdateMaterial();
        //    MinimapController.instance.CreatePlayerCanvasPrefab(player, new Color(1f, 0f, 0f));
        //}
        //if (playerParent.forward.z < 0)
        //{
        //    if (playerInput.GetComponentInChildren<FollowPlayer>())
        //    {
        //        playerParent.GetComponentInChildren<FollowPlayer>().lookInOppositeDirection = true;
        //    }
        //}
        //if (playerParent.GetComponentInChildren<Camera>())
        //{
        //    if (players.Count - 1 == 0)
        //    {

        //        playerParent.GetComponentInChildren<Camera>().cullingMask = team0CullingMask;
        //    }
        //    else
        //    {
        //        playerParent.GetComponentInChildren<Camera>().cullingMask = team1CullingMask;
        //    }
        //}
        //AssignTeammatesAndOpponents();

    }

    // Sets the teammates and opponents for each player
    private void AssignTeammatesAndOpponents()
    {
        
        for (int teamIndex = 0; teamIndex < teams.Length; teamIndex++)
        {
            int opposingTeamIndex = teamIndex == 1 ? 0 : 1;
            for (int playerIndex = 0; playerIndex < teams[teamIndex].Count; playerIndex++)
            {
                Debug.Log("Setting teammates for player " + playerIndex + " on team: " + teamIndex);
                List<Player[]> teamWithoutPlayer = new List<Player[]>(teams[teamIndex]);
                teamWithoutPlayer.RemoveAt(playerIndex);
                for (int i = 0; i < 2; i++)
                {
                    teams[teamIndex][playerIndex][i].teammates = teamWithoutPlayer;
                    teams[teamIndex][playerIndex][i].opponents = teams[opposingTeamIndex];
                }

            }


        }
    }

    public void SpawnPlayer(Player player, Player.ControllerType controllerType)
    {

        //players.Add(player);
        //Transform playerParent = player.transform.parent;
        //playerParent.position = spawnPoints[players.Count - 1].position;
        //playerParent.rotation = spawnPoints[players.Count - 1].rotation;
        //player.teamIndex = (players.Count - 1) % 2;
        //teams[player.teamIndex].Add(player);
        //player.controllerType = controllerType;
        //if ((players.Count - 1) % 2 == 0)
        //{
        //    player.playingMaterial = team0PlayingMaterial;
        //    player.penalizedMaterial = team0PenalizedMaterial;
        //    player.UpdateMaterial();
        //    MinimapController.instance.CreatePlayerCanvasPrefab(player, new Color(0f, 0f, 1f));
        //}
        //else
        //{
        //    player.playingMaterial = team1PlayingMaterial;
        //    player.penalizedMaterial = team1PenalizedMaterial;
        //    player.UpdateMaterial();
        //    MinimapController.instance.CreatePlayerCanvasPrefab(player, new Color(1f, 0f, 0f));
        //}
        //AssignTeammatesAndOpponents();
    }

    public void GoalScored(int teamGoalScoredOn)
    {
        audioSource.Play();
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
        for (int i = 0; i < teams[0].Count; i++)
        {
            teams[0][i][0].transform.position = team0SpawnPoints[i].position;
            teams[0][i][0].transform.rotation = team0SpawnPoints[i].rotation;
            teams[0][i][0].GetComponent<Rigidbody>().velocity = Vector3.zero;
            teams[0][i][0].ResetValues();
            teams[0][i][1].ResetValues();
        }
        for (int i = 0; i < teams[1].Count; i++)
        {
            teams[1][i][0].transform.position = team1SpawnPoints[i].position;
            teams[1][i][0].transform.rotation = team1SpawnPoints[i].rotation;
            teams[1][i][0].GetComponent<Rigidbody>().velocity = Vector3.zero;
            teams[1][i][0].ResetValues();
            teams[1][i][1].ResetValues();
        }
    }

    public void ResetBall(int teamIndex = -1)
    {
        
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
            players[i][0].UpdatePlayerState(Player.PlayerState.Waiting);
            players[i][1].UpdatePlayerState(Player.PlayerState.Waiting);
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
            players[i][0].UpdatePlayerState(PlayerController.PlayerState.Playing);
            players[i][1].UpdatePlayerState(PlayerController.PlayerState.Playing);
        }
        yield return new WaitForSeconds(1f);
        GameplayGui.instance.UpdateCountdown("");
        GameplayGui.instance.ToggleScoreLabel(true);


    }

    public void UpdateTeamWithBall(int teamIndex)
    {
        Player.teamWithBall = teamIndex;
        Player[] playerWithBall = ball.owner;
        for (int i = 0; i < players.Count; i++)
        {
            players[i][0].BallEvent(playerWithBall);
            players[i][1].BallEvent(playerWithBall);
        }
    }

    public Vector3 GetGoalPositionForTeam(int teamIndex)
    {
        if (teamIndex == 0)
        {
            return goals[1].transform.position;
        }
        else
        {
            return goals[0].transform.position;
        }
    }
}
