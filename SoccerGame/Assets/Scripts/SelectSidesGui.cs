using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SelectSidesGui : MonoBehaviour
{
    public static SelectSidesGui instance;
    private int playerCount = 0;
    public RectTransform[] controllerIcons;
    public Sprite keyboardAndMouseBackgroundSprite;
    public Sprite keyboardAndMouseForegroundSprite;
    public Sprite controllerBackgroundSprite;
    public Sprite controllerForegroundSprite;
    private List<PlayerInput> playerInputs = new List<PlayerInput>();
    private List<bool> playerReadys = new List<bool>();
    public Color[] playerColors = { Color.blue, Color.red, Color.green, Color.yellow, Color.magenta, Color.cyan };
    public Image team0Border;
    public Image team1Border;
    public TextMeshProUGUI team0AiCountLabel;
    public TextMeshProUGUI team1AiCountLabel;
    public TextMeshProUGUI errorLabel;
    public TextMeshProUGUI instructionsLabel;
    int team0Players = 0;
    int team1Players = 0;
    int team0AiCount = 1;
    int team1AiCount = 1;
    bool team0IsReady = false;
    bool team1IsReady = false;
    int maxPlayersOnTeam = 6;
    AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip aiCountChangedSound;
    public AudioClip readyUpSound;

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
        audioSource = GetComponent<AudioSource>();
        UpdateAiCountLabels();
        for (int i = 0; i < 4; i++)
        {
            playerInputs.Add(null);
            playerReadys.Add(true);
        }
        PlayerInput[] existingPlayerInputs = FindObjectsOfType<PlayerInput>();
        for (int i = 0; i < existingPlayerInputs.Length; i++)
        {
            PlayerJoined(existingPlayerInputs[i]);
        }

        // ensure that player inputs start in the select sides section, not the AI count section
        for (int i = 0; i < playerInputs.Count; i++)
        {
            if (playerInputs[i] == null)
            {
                continue;             
            }
            playerInputs[i].GetComponent<HumanController>().guiSection = 1;
            PlayerMovedUpOrDown(playerInputs[i], true, 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayerJoined(PlayerInput playerInput)
    {
        int indexOfNewPlayerInput = 0;
        while (playerInputs[indexOfNewPlayerInput] != null)
        {
            indexOfNewPlayerInput++;
            if (indexOfNewPlayerInput == playerInputs.Count)
            {
                Debug.LogError("Cannot add any more players.");
                return;
            }
        }
        playerInputs[indexOfNewPlayerInput] = playerInput;
        playerReadys[indexOfNewPlayerInput] = false;
        playerInput.GetComponent<HumanController>().color = playerColors[indexOfNewPlayerInput];
        if (playerInput.devices[0].name == "Keyboard" || playerInput.devices[0].name == "Mouse")
        {
            controllerIcons[indexOfNewPlayerInput].GetComponent<Image>().sprite = keyboardAndMouseBackgroundSprite;
            controllerIcons[indexOfNewPlayerInput].transform.GetChild(0).gameObject.GetComponent<Image>().sprite = keyboardAndMouseForegroundSprite;
        }
        else
        {
            controllerIcons[indexOfNewPlayerInput].GetComponent<Image>().sprite = controllerBackgroundSprite;
            controllerIcons[indexOfNewPlayerInput].transform.GetChild(0).gameObject.GetComponent<Image>().sprite = controllerForegroundSprite;
        }
        controllerIcons[indexOfNewPlayerInput].transform.GetChild(0).gameObject.SetActive(true);
        controllerIcons[indexOfNewPlayerInput].GetComponent<Image>().color = playerColors[indexOfNewPlayerInput];
        AudioManager.instance.Play("UI_Click");
    }

    

    public void PlayerLeft(PlayerInput playerInput)
    {
        //int index = playerInputs.IndexOf(playerInput);
        //playerInputs[index] = null;
        //playerReadys[index] = true;
        //controllerIcons[index].transform.GetChild(0).gameObject.SetActive(false);
        //controllerIcons[index].transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        //controllerIcons[index].rectTransform.anchoredPosition = new Vector2(0f, controllerIcons[index].rectTransform.anchoredPosition.y);
        Debug.Log("Player Left!");
        // recalculate players on each team and AI count

    }

    public void PlayerMovedLeftOrRight(PlayerInput playerInput, bool movedLeft, int guiSection)
    {
        int index = playerInputs.IndexOf(playerInput);
        if (guiSection == 1)
        {
            if (movedLeft)
            {
                if (controllerIcons[index].anchoredPosition.x == 0f)
                {
                    controllerIcons[index].anchoredPosition = new Vector2(-60f, controllerIcons[index].anchoredPosition.y);
                    playerInput.GetComponent<HumanController>().teamIndex = 0;
                    team0Players += 1;
                    if (team0Players + team0AiCount > maxPlayersOnTeam)
                    {
                        team0AiCount = maxPlayersOnTeam - team0Players;
                        UpdateAiCountLabels();
                    }
                    AudioManager.instance.Play("UI_Click");

                }
                else if (controllerIcons[index].anchoredPosition.x > 0f)
                {
                    controllerIcons[index].anchoredPosition = new Vector2(0f, controllerIcons[index].anchoredPosition.y);
                    team1Players -= 1;
                    if (team1Players == 0 && team1AiCount <= 0)
                    {
                        team1AiCount = 1; ;
                        UpdateAiCountLabels();
                    }
                    playerInput.GetComponent<HumanController>().teamIndex = -1;
                    AudioManager.instance.Play("UI_Click");
                }
            }
            else
            {
                if (controllerIcons[index].anchoredPosition.x == 0f)
                {
                    controllerIcons[index].anchoredPosition = new Vector2(60f, controllerIcons[index].anchoredPosition.y);
                    team1Players += 1;
                    playerInput.GetComponent<HumanController>().teamIndex = 1;
                    if (team1Players + team1AiCount > maxPlayersOnTeam)
                    {
                        team1AiCount = maxPlayersOnTeam - team1Players;
                        UpdateAiCountLabels();
                    }
                    AudioManager.instance.Play("UI_Click");
                }
                else if (controllerIcons[index].anchoredPosition.x < 0f)
                {
                    controllerIcons[index].anchoredPosition = new Vector2(0f, controllerIcons[index].anchoredPosition.y);
                    team0Players -= 1;
                    if (team0Players == 0 && team0AiCount <= 0)
                    {
                        team0AiCount = 1; ;
                        UpdateAiCountLabels();
                    }
                    playerInput.GetComponent<HumanController>().teamIndex = -1;
                    AudioManager.instance.Play("UI_Click");
                }
            }
        }
        else if (guiSection == 2)
        {
            if (playerInput.GetComponent<HumanController>().teamIndex == 1)
            {
                if (movedLeft)
                {
                    team1AiCount--;
                }
                else
                {
                    team1AiCount++;
                }

                team1AiCount = Mathf.Clamp(team1AiCount, 0, 6);

                AudioManager.instance.Play("UI_Increment");
                UpdateAiCountLabels();
            }
            else if (playerInput.GetComponent<HumanController>().teamIndex == 0)
            {
                if (movedLeft)
                {
                    team0AiCount--;
                }
                else
                {
                    team0AiCount++;
                }

                team0AiCount = Mathf.Clamp(team0AiCount, 0, 6);
                AudioManager.instance.Play("UI_Increment");
                UpdateAiCountLabels();
            }
        }
        

    }


    public void PlayerMovedUpOrDown(PlayerInput playerInput, bool movedUp, int guiSection)
    {
        int index = playerInputs.IndexOf(playerInput);
        if (guiSection == 1)
        {
            if (!movedUp && playerInput.GetComponent<HumanController>().teamIndex != -1)
            {
                playerInput.GetComponent<HumanController>().guiSection = 2;
                if (playerInput.GetComponent<HumanController>().teamIndex == 1)
                {
                    team1Border.color = playerInput.GetComponent<HumanController>().color;
                }
                else
                {
                    team0Border.color = playerInput.GetComponent<HumanController>().color;
                }
                AudioManager.instance.Play("UI_Click");
            }
        }
        else if (guiSection == 2)
        {
            if (movedUp)
            {
                playerInput.GetComponent<HumanController>().guiSection = 1;
                if (playerInput.GetComponent<HumanController>().teamIndex == 1)
                {
                    team1Border.color = Color.white;
                }
                else
                {
                    team0Border.color = Color.white;
                }
                AudioManager.instance.Play("UI_Click");
            }
        }
    }

    public void ReadiedUp(PlayerInput playerInput,bool readiedUp)
    {
        if (playerInput.GetComponent<HumanController>().teamIndex == -1)
        {
            return;
        }

        int index = playerInputs.IndexOf(playerInput);
        if (playerReadys[index] == readiedUp)
        {
            return;
        }
        controllerIcons[index].transform.GetChild(0).GetChild(0).gameObject.SetActive(readiedUp);
        playerReadys[index] = readiedUp;
        AudioManager.instance.Play("UI_Ready");
        // return if not every player has readied up
        for (int i = 0; i < playerReadys.Count; i++)
        {
            if (playerReadys[i] == false)
            {
                return;
            }
        }


        if (CanPlay())
        {
            MatchSettings.instance.numberOfAiOnTeam0 = team0AiCount;
            MatchSettings.instance.numberOfAiOnTeam1 = team1AiCount;
            List<PlayerInput> team0PlayerInputs = new List<PlayerInput>();
            List<PlayerInput> team1PlayerInputs = new List<PlayerInput>();
            for (int i = 0; i < playerInputs.Count; i++)
            {
                if (playerInputs[i] == null)
                {
                    continue;
                }
                if (playerInputs[i].GetComponent<HumanController>().teamIndex == 0)
                {
                    team0PlayerInputs.Add(playerInputs[i]);
                }
                else if (playerInputs[i].GetComponent<HumanController>().teamIndex == 1)
                {
                    team1PlayerInputs.Add(playerInputs[i]);
                }
            }
            MatchSettings.instance.team0PlayerInputs = team0PlayerInputs;
            MatchSettings.instance.team1PlayerInputs = team1PlayerInputs;
            Debug.Log("Starting game!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {

            playerReadys[index] = false;
            controllerIcons[index].transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            
        }
        
    }

    public bool CanPlay()
    {
        bool playerIsOnTeam0 = false;
        bool playerIsOnTeam1 = false;
        List<PlayerInput> enabledInputs = new List<PlayerInput>();
        for (int i = 0; i < playerInputs.Count; i++)
        {
            if (playerInputs[i] != null)
            {
                enabledInputs.Add(playerInputs[i]);
            }
        }
        for (int i = 0; i < enabledInputs.Count; i++)
        {

            HumanController player = enabledInputs[i].GetComponent<HumanController>();
            if (player.teamIndex == 1)
            {
                playerIsOnTeam1 = true;
            }
            else if (player.teamIndex == 0)
            {
                playerIsOnTeam0 = true;
            }
        }
        if (enabledInputs.Count > 1 && (!playerIsOnTeam0 || !playerIsOnTeam1))
        {
            // check that every controller is assigned to a team
            for (int i = 0; i < enabledInputs.Count; i++)
            {
                int teamIndex = enabledInputs[i].GetComponent<HumanController>().teamIndex;
                if (teamIndex == -1)
                {
                    StartCoroutine(DisplayError("All controllers must be assigned to a team."));
                    return false;
                }
            }
        }

        if (enabledInputs.Count == 1)
        {
            if (enabledInputs[0].GetComponent<HumanController>().teamIndex == -1)
            {
                StartCoroutine(DisplayError("All controllers must be assigned to a team."));
                return false;
            }
        }

        if (team0Players + team0AiCount > maxPlayersOnTeam)
        {
            StartCoroutine(DisplayError("Team 0 has more than the 6 player limit."));
            return false;
        }

        if (team1Players + team1AiCount > maxPlayersOnTeam)
        {
            StartCoroutine(DisplayError("Team 1 has more than the 6 player limit."));
            return false;
        }


        return true;
    }

    public void UpdateAiCountLabels()
    {
        team1AiCountLabel.text = team1AiCount.ToString();
        team0AiCountLabel.text = team0AiCount.ToString();
    }

    IEnumerator DisplayError(string errorText)
    {
        instructionsLabel.enabled = false;
        errorLabel.text = errorText;
        yield return new WaitForSeconds(4f);
        errorLabel.text = "";
        instructionsLabel.enabled = true;
    }
}
