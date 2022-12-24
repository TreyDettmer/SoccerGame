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
    public Image[] controllerIcons;
    private List<PlayerInput> playerInputs = new List<PlayerInput>();
    private List<bool> playerReadys = new List<bool>();
    public Color[] playerColors = { Color.blue, Color.red, Color.green, Color.yellow, Color.magenta, Color.cyan };
    public Image team0Border;
    public Image team1Border;
    public TextMeshProUGUI team0AiCountLabel;
    public TextMeshProUGUI team1AiCountLabel;
    public TextMeshProUGUI errorLabel;
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
        for (int i = 0; i < 6; i++)
        {
            playerInputs.Add(null);
            playerReadys.Add(true);
        }
        PlayerInput[] existingPlayerInputs = FindObjectsOfType<PlayerInput>();
        for (int i = 0; i < existingPlayerInputs.Length; i++)
        {
            PlayerJoined(existingPlayerInputs[i]);
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

        controllerIcons[indexOfNewPlayerInput].transform.GetChild(0).gameObject.SetActive(true);
        controllerIcons[indexOfNewPlayerInput].color = playerColors[indexOfNewPlayerInput];
        
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
                if (controllerIcons[index].rectTransform.anchoredPosition.x == 0f)
                {
                    controllerIcons[index].rectTransform.anchoredPosition = new Vector2(-60f, controllerIcons[index].rectTransform.anchoredPosition.y);
                    playerInput.GetComponent<HumanController>().teamIndex = 0;
                    team0Players += 1;
                    if (team0Players + team0AiCount > maxPlayersOnTeam)
                    {
                        team0AiCount = maxPlayersOnTeam - team0Players;
                        UpdateAiCountLabels();
                    }
                    audioSource.PlayOneShot(clickSound);

                }
                else if (controllerIcons[index].rectTransform.anchoredPosition.x > 0f)
                {
                    controllerIcons[index].rectTransform.anchoredPosition = new Vector2(0f, controllerIcons[index].rectTransform.anchoredPosition.y);
                    team1Players -= 1;
                    if (team1Players == 0 && team1AiCount <= 0)
                    {
                        team1AiCount = 1; ;
                        UpdateAiCountLabels();
                    }
                    playerInput.GetComponent<HumanController>().teamIndex = -1;
                    audioSource.PlayOneShot(clickSound);
                }
            }
            else
            {
                if (controllerIcons[index].rectTransform.anchoredPosition.x == 0f)
                {
                    controllerIcons[index].rectTransform.anchoredPosition = new Vector2(60f, controllerIcons[index].rectTransform.anchoredPosition.y);
                    team1Players += 1;
                    playerInput.GetComponent<HumanController>().teamIndex = 1;
                    if (team1Players + team1AiCount > maxPlayersOnTeam)
                    {
                        team1AiCount = maxPlayersOnTeam - team1Players;
                        UpdateAiCountLabels();
                    }
                    audioSource.PlayOneShot(clickSound);
                }
                else if (controllerIcons[index].rectTransform.anchoredPosition.x < 0f)
                {
                    controllerIcons[index].rectTransform.anchoredPosition = new Vector2(0f, controllerIcons[index].rectTransform.anchoredPosition.y);
                    team0Players -= 1;
                    if (team0Players == 0 && team0AiCount <= 0)
                    {
                        team0AiCount = 1; ;
                        UpdateAiCountLabels();
                    }
                    playerInput.GetComponent<HumanController>().teamIndex = -1;
                    audioSource.PlayOneShot(clickSound);
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

                audioSource.PlayOneShot(aiCountChangedSound);
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
                audioSource.PlayOneShot(aiCountChangedSound);
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
                audioSource.PlayOneShot(clickSound);
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
                audioSource.PlayOneShot(clickSound);
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
        audioSource.PlayOneShot(readyUpSound);
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
            SceneManager.LoadScene(2);
        }
        else
        {
            for (int i = 0; i < playerReadys.Count; i++)
            {
                playerReadys[i] = false;
                controllerIcons[i].transform.GetChild(0).GetChild(0).gameObject.SetActive(readiedUp);
            }
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
        errorLabel.text = errorText;
        yield return new WaitForSeconds(4f);
        errorLabel.text = "";
    }
}
