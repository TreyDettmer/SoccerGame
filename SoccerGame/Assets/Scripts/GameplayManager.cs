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
    private List<Player>[] teams = { new List<Player>(), new List<Player>() };
    private List<HumanController> restartRequesters = new List<HumanController>();
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

    public GameObject playerPrefab;



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
                Player player = Instantiate(playerPrefab).GetComponent<Player>();
                HumanController humanController = team0PlayerInputs[i].GetComponent<HumanController>();
                player.SetNewController(humanController);
                humanController.SetNewPlayer(player);
                player.teamIndex = humanController.teamIndex;
                teams[0].Add(player);
                players.Add(player);
                playerInputs.Add(team0PlayerInputs[i]);
                player.UpdateGameState(Player.GameState.Gameplay);
            }
            for (int i = 0; i < team1PlayerInputs.Count; i++)
            {
                Player player = Instantiate(playerPrefab).GetComponent<Player>();
                HumanController humanController = team1PlayerInputs[i].GetComponent<HumanController>();
                player.SetNewController(humanController);
                humanController.SetNewPlayer(player);
                player.teamIndex = humanController.teamIndex;
                teams[1].Add(player);
                players.Add(player);
                playerInputs.Add(team1PlayerInputs[i]);
                player.UpdateGameState(Player.GameState.Gameplay);
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

    public void SpawnPlayer(Player player, Transform spawnPoint)
    {

        player.transform.rotation = spawnPoint.rotation;
        player.transform.position = spawnPoint.position;

        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        player.ResetValues();
        if (player.teamIndex == 0)
        {
            player.UpdateMaterial();
            MinimapController.instance.CreatePlayerCanvasPrefab(player, new Color(0f, 0f, 1f));
        }
        else
        {
            player.UpdateMaterial();
            MinimapController.instance.CreatePlayerCanvasPrefab(player, new Color(1f, 0f, 0f));
        }
    }

    void SpawnAiPlayers()
    {
        for (int i = 0; i < numberOfAiOnTeam0; i++)
        {
            Player player = Instantiate(playerPrefab).GetComponent<Player>();
            player.teamIndex = 0;
            teams[0].Add(player);
            players.Add(player);
            player.UpdateGameState(Player.GameState.Gameplay);

        }
        for (int i = 0; i < numberOfAiOnTeam1; i++)
        {
            Player player = Instantiate(playerPrefab).GetComponent<Player>();
            player.teamIndex = 1;
            teams[1].Add(player);
            players.Add(player);
            player.UpdateGameState(Player.GameState.Gameplay);
        }
    }

    public void PlayerJoined(PlayerInput playerInput)
    {

        Player player = playerInput.GetComponent<Player>();
        player.teamIndex = 0;
        teams[0].Add(player);
        playerInputs.Add(playerInput);
        players.Add(player);
        player.UpdateGameState(Player.GameState.Gameplay);
        if (playerInputs.Count > 0)
        {
            // Start Game since we actually have players
            StartGame();
        }
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
                List<Player> teamWithoutPlayer = new List<Player>(teams[teamIndex]);
                teamWithoutPlayer.RemoveAt(playerIndex);
                teams[teamIndex][playerIndex].teammates = teamWithoutPlayer;
                teams[teamIndex][playerIndex].opponents = teams[opposingTeamIndex];
            }
        }
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
            teams[0][i].transform.position = team0SpawnPoints[i].position;
            teams[0][i].transform.rotation = team0SpawnPoints[i].rotation;
            teams[0][i].GetComponent<Rigidbody>().velocity = Vector3.zero;
            teams[0][i].ResetValues();
        }
        for (int i = 0; i < teams[1].Count; i++)
        {
            teams[1][i].transform.position = team1SpawnPoints[i].position;
            teams[1][i].transform.rotation = team1SpawnPoints[i].rotation;
            teams[1][i].GetComponent<Rigidbody>().velocity = Vector3.zero;
            teams[1][i].ResetValues();
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

    public void RequestedRestart(HumanController humanController, bool canceledRequest = false)
    {
        if (canceledRequest)
        {
            if (restartRequesters.Contains(humanController))
            {
                restartRequesters.Remove(humanController);
            }
            
        }
        else
        {
            if (!restartRequesters.Contains(humanController))
            {
                restartRequesters.Add(humanController);
            }
            
        }
        if (restartRequesters.Count >= playerInputs.Count)
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
            players[i].UpdatePlayerState(Player.PlayerState.Playing);
        }
        yield return new WaitForSeconds(1f);
        GameplayGui.instance.UpdateCountdown("");
        GameplayGui.instance.ToggleScoreLabel(true);


    }

    public void UpdateTeamWithBall(int teamIndex)
    {
        Player.teamWithBall = teamIndex;
        Player playerWithBall = ball.owner;

        
        if (playerWithBall != null)
        {
            if (playerWithBall.GetComponent<AiController>().enabled)
            {
                bool updatedControlledBy = false;
                for (int i = 0; i < playerInputs.Count; i++)
                {
                    if (playerInputs[i].GetComponent<HumanController>().teamIndex == playerWithBall.teamIndex)
                    {

                        //update to controlled player
                        if (ball.lastKickedBy)
                        {
                            if (ball.lastKickedBy == playerInputs[i].GetComponent<Player>())
                            {
                                updatedControlledBy = true;
                                playerWithBall.SetNewController(playerInputs[i].GetComponent<HumanController>());
                                playerInputs[i].GetComponent<HumanController>().SetNewPlayer(playerWithBall);
                            }
                        }


                    }
                }
                if (!updatedControlledBy)
                {
                    for (int i = 0; i < playerInputs.Count; i++)
                    {
                        if (playerInputs[i].GetComponent<HumanController>().teamIndex == playerWithBall.teamIndex)
                        {
                            playerWithBall.SetNewController(playerInputs[i].GetComponent<HumanController>());
                            playerInputs[i].GetComponent<HumanController>().SetNewPlayer(playerWithBall);
                        }
                    }
                }
            }
        }
        for (int i = 0; i < players.Count; i++)
        {
            players[i].BallEvent(playerWithBall);
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
