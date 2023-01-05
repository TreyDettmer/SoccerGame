using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;

public class GameplayManager : MonoBehaviour
{
    [HideInInspector]
    public static GameplayManager instance;
    [HideInInspector] public int team0Score = 0;
    [HideInInspector] public int team1Score = 0;
    private List<PlayerInput> playerInputs = new List<PlayerInput>();
    private List<Player> players = new List<Player>();
    private List<Player>[] teams = { new List<Player>(), new List<Player>() };
    [SerializeField]
    private GameObject aiPlayerObjectPrefab;
    private bool gameIsCountingDown = false;
    private GameClock gameClock;
    public float gameLengthInMinutes = 1f;
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
    [SerializeField] private AudioClip crowdAudioClip;
    [SerializeField] private AudioClip whistleAudioClip;
    [SerializeField] private AudioClip endOfPeriodWhistleAudioClip;
    private Goal[] goals;
    private Ball ball;
    int numberOfAiOnTeam0 = 1;
    int numberOfAiOnTeam1 = 1;

    public GameObject playerPrefab;
    public Camera stadiumCamera;
    IEnumerator postGoalRoutine;
    bool postGoalRoutineIsRunning = false;


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
        gameClock = GetComponent<GameClock>();
        gameClock.realTimeGameLengthInMinutes = MatchSettings.instance.gameLengthInMinutes;
        AudioManager.instance.StopAllSounds();
        AudioManager.instance.Play("Theme2");
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
            if (playerInputs.Count == 3)
            {
                stadiumCamera.rect = new Rect(.5f, 0f, 1, .5f);
            }
        }

        FindObjectOfType<InputManager>().DisableJoining();
        AssignTeammatesAndOpponents();
        StartCoroutine(GameCountdown());
        foreach (PlayerGui playerGui in FindObjectsOfType<PlayerGui>())
        {
            playerGui.ConfigureViewportRect();
        }
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
        player.InitializeTeamInfo();
    }

    void SpawnAiPlayers()
    {
        for (int i = 0; i < numberOfAiOnTeam0; i++)
        {
            Player player = Instantiate(playerPrefab).GetComponent<Player>();
            player.teamIndex = 0;
            teams[0].Add(player);
            players.Add(player);
            player.InitializeTeamInfo();
            player.UpdateGameState(Player.GameState.Gameplay);

        }
        for (int i = 0; i < numberOfAiOnTeam1; i++)
        {
            Player player = Instantiate(playerPrefab).GetComponent<Player>();
            player.teamIndex = 1;
            teams[1].Add(player);
            players.Add(player);
            player.InitializeTeamInfo();
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
        AudioManager.instance.Play("Goal");
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

        postGoalRoutine = PostGoalRoutine(2f);
        StartCoroutine(postGoalRoutine);
        StartCoroutine(GameplayGui.instance.GoalLabelRoutine(2f));
    }


    // called when a player somehow goes out of the map
    public void ResetPlayer(Player player)
    {
        if (player.teamIndex == 0)
        {
            player.transform.position = team0SpawnPoints[0].position;
            player.transform.rotation = team0SpawnPoints[0].rotation;
            player.GetComponent<Rigidbody>().velocity = Vector3.zero;
            player.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            player.ResetValues();
        }
        else
        {
            player.transform.position = team1SpawnPoints[0].position;
            player.transform.rotation = team1SpawnPoints[0].rotation;
            player.GetComponent<Rigidbody>().velocity = Vector3.zero;
            player.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            player.ResetValues();
        }
    }

    IEnumerator PostGoalRoutine(float duration)
    {
        postGoalRoutineIsRunning = true;
        Time.timeScale = .15f;
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
        postGoalRoutineIsRunning = false;
    }

    public void ResetPlayers()
    {
        for (int i = 0; i < teams[0].Count; i++)
        {
            teams[0][i].transform.position = team0SpawnPoints[i].position;
            teams[0][i].transform.rotation = team0SpawnPoints[i].rotation;
            teams[0][i].GetComponent<Rigidbody>().velocity = Vector3.zero;
            teams[0][i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            teams[0][i].ResetValues();
        }
        for (int i = 0; i < teams[1].Count; i++)
        {
            teams[1][i].transform.position = team1SpawnPoints[i].position;
            teams[1][i].transform.rotation = team1SpawnPoints[i].rotation;
            teams[1][i].GetComponent<Rigidbody>().velocity = Vector3.zero;
            teams[1][i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
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
        AudioManager.instance.Play("OutOfBounds");
        ResetPlayers();
        ResetBall(teamIndex);
        StartCoroutine(GameCountdown(2f));
    }

    public void PlayerFoul(Player fouler, Player fouled)
    {
        fouled.OnFouled();
        fouler.UpdatePlayerState(Player.PlayerState.Penalized);
    }


    public void RestartGame()
    {

        HumanController[] humanControllers = FindObjectsOfType<HumanController>();
        for (int i = 0; i < humanControllers.Length; i++)
        {
            humanControllers[i].ToggleUIActionMap(false);
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //ResetPlayers();
        //ResetBall();
        //team0Score = 0;
        //team1Score = 0;
        //GameplayGui.instance.ToggleScoreLabel(false);
        //gameClock.ResetClock();
        //GameplayGui.instance.UpdateScore(team0Score, team1Score);
        //StartCoroutine(GameCountdown());
    }

    IEnumerator GameCountdown(float _countdownTime = -1f)
    {
        gameIsCountingDown = true;
        for (int i = 0; i < players.Count; i++)
        {
            players[i].UpdatePlayerState(Player.PlayerState.Waiting);
            players[i].UpdateGameState(Player.GameState.SelectSides);
        }
        // switch to gameplay action map
        HumanController[] humanControllers = FindObjectsOfType<HumanController>();
        for (int i = 0; i < humanControllers.Length; i++)
        {
            humanControllers[i].ToggleUIActionMap(false);
        }
        if (_countdownTime == -1f)
        {
            GameplayGui.instance.ToggleScoreLabel(false);
        }
        Time.timeScale = 1f;
        float currentCountdownTime = _countdownTime == -1f ? countdownTime : _countdownTime;
        while (currentCountdownTime > 0f)
        {
            GameplayGui.instance.UpdateCountdown(currentCountdownTime.ToString());
            yield return new WaitForSeconds(1f);
            currentCountdownTime -= 1f;
        }

        gameClock.ResumeClock();
        AudioManager.instance.Play("Whistle1");
        for (int i = 0; i < players.Count; i++)
        {
            players[i].UpdatePlayerState(Player.PlayerState.Playing);
            players[i].UpdateGameState(Player.GameState.Gameplay);
        }

        GameplayGui.instance.UpdateCountdown("");
        GameplayGui.instance.ToggleScoreLabel(true);
        gameIsCountingDown = false;

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
                        if (!updatedControlledBy)
                        {
                            if (playerInputs[i].GetComponent<HumanController>().teamIndex == playerWithBall.teamIndex)
                            {
                                updatedControlledBy = true;
                                playerWithBall.SetNewController(playerInputs[i].GetComponent<HumanController>());
                                playerInputs[i].GetComponent<HumanController>().SetNewPlayer(playerWithBall);
                            }
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

    public void HumanControllerSwitchPlayers(Player player, HumanController humanController)
    {
        int teamIndex = player.teamIndex;
        int thisPlayersIndex = teams[player.teamIndex].IndexOf(player);
        int index = (thisPlayersIndex + 1) % teams[teamIndex].Count;
        while (index != thisPlayersIndex)
        {
            if (teams[teamIndex][index].GetComponent<AiController>().enabled)
            {
                teams[teamIndex][index].SetNewController(humanController);
                humanController.SetNewPlayer(teams[teamIndex][index]);
                return;
            }
            index = (index + 1) % teams[teamIndex].Count;
        }
    }

    public Vector3 GetGoalPositionForTeam(int teamIndex)
    {
        if (teamIndex != 0 && teamIndex != 1)
        {
            return Vector3.zero;
        }
        return Array.Find(goals, goal => goal.teamIndex == teamIndex).transform.position; 
    }

    public void QuitGame()
    {
        // switch to ui action map
        HumanController[] humanControllers = FindObjectsOfType<HumanController>();
        for (int i = 0; i < humanControllers.Length; i++)
        {
            humanControllers[i].ToggleUIActionMap(true);
        }
        PlayerInputManager playerInputManager = FindObjectOfType<PlayerInputManager>();
        SceneManager.MoveGameObjectToScene(playerInputManager.gameObject, SceneManager.GetActiveScene());
        SceneManager.LoadScene(1);
    }
    public void PauseForPeriodEnd()
    {
        if (postGoalRoutineIsRunning)
        {
            StopCoroutine(postGoalRoutine);
            // since we just stopped the post goal routine from finishing, make sure goals get re-enabled
            Goal[] goals = FindObjectsOfType<Goal>();
            for (int i = 0; i < goals.Length; i++)
            {
                goals[i].isEnabled = true;
            }
            postGoalRoutineIsRunning = false;
        }
        AudioManager.instance.Play("Whistle2");
        // Disable players
        for (int i = 0; i < players.Count; i++)
        {
            players[i].UpdateGameState(Player.GameState.SelectSides);
            players[i].UpdatePlayerState(Player.PlayerState.Waiting);
        }

        gameClock.isPaused = true;
        GameplayGui.instance.EnablePauseMenu(true);
        Time.timeScale = 0f;
        // switch to ui action map
        HumanController[] humanControllers = FindObjectsOfType<HumanController>();
        for (int i = 0; i < humanControllers.Length; i++)
        {
            humanControllers[i].ToggleUIActionMap(true);
        }
    }

    public void PauseGame()
    {
        // cannot pause during countdown
        if (gameIsCountingDown || postGoalRoutineIsRunning)
        {
            return;
        }
        // Disable players
        for (int i = 0; i < players.Count; i++)
        {
            players[i].UpdateGameState(Player.GameState.SelectSides);
            players[i].UpdatePlayerState(Player.PlayerState.Waiting);
        }

        gameClock.isPaused = true;
        GameplayGui.instance.EnablePauseMenu(true);
        Time.timeScale = 0f;
        // switch to ui action map
        HumanController[] humanControllers = FindObjectsOfType<HumanController>();
        for (int i = 0; i < humanControllers.Length; i++)
        {
            humanControllers[i].ToggleUIActionMap(true);
        }

    }

    public void ResumeGameFromPeriodEnd()
    {
        ResetPlayers();
        ResetBall();
        StartCoroutine(GameCountdown(2f));
    }

    public void ResumeGame()
    {
        // switch to ui action map
        HumanController[] humanControllers = FindObjectsOfType<HumanController>();
        for (int i = 0; i < humanControllers.Length; i++)
        {
            humanControllers[i].ToggleUIActionMap(false);
        }

        // determine how we should resume the game based on whether a period just ended
        if (GameClock.instance.elapsedMinutes == 45f && GameClock.instance.elapsedSeconds == 0f)
        {
            ResumeGameFromPeriodEnd();
        }
        else if (GameClock.instance.elapsedMinutes == 90f && GameClock.instance.elapsedSeconds == 0f)
        {
            ResumeGameFromPeriodEnd();
        }
        else if (GameClock.instance.elapsedMinutes == 105f && GameClock.instance.elapsedSeconds == 0f)
        {
            ResumeGameFromPeriodEnd();
        }
        else
        {
            Time.timeScale = 1f;
            gameClock.ResumeClock();         
            for (int i = 0; i < players.Count; i++)
            {
                players[i].UpdatePlayerState(Player.PlayerState.Playing);
                players[i].UpdateGameState(Player.GameState.Gameplay);
            }
            
            
            
        }
    }
}
