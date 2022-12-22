using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HumanController : MonoBehaviour
{

    public Player myPlayer { get; set; }

    #region Input
    private InputActionAsset playerInputAsset;
    private InputActionMap gameplayActionMap;
    private InputActionMap selectSidesActionMap;
    private InputActionMap uiActionMap;
    private PlayerInput playerInput;
    #endregion

    #region Camera
    public Camera cam;
    private FollowPlayer cameraFollowPlayer;
    #endregion

    #region GUI
    private float previousMovementTime = 0f;
    // guiSection 1 means selecting sides, guiSection 2 means selecting AI count
    [HideInInspector] public int guiSection = 1;
    [HideInInspector] public Color color;
    public PlayerGui cameraPlayerGui;
    #endregion

    #region Info
    Player.GameState gameState;
    public int teamIndex = -1;
    #endregion



    private void Awake()
    {
        cam = GetComponent<Camera>();
        playerInput = GetComponent<PlayerInput>();
        playerInputAsset = playerInput.actions;
        gameplayActionMap = playerInputAsset.FindActionMap("Player");
        selectSidesActionMap = playerInputAsset.FindActionMap("SelectSides");
        uiActionMap = playerInputAsset.FindActionMap("UI");
        cameraPlayerGui = GetComponent<PlayerGui>();
        cameraFollowPlayer = GetComponent<FollowPlayer>();
        gameState = Player.GameState.SelectSides;
        uiActionMap.Disable();
        gameplayActionMap.Disable();
        DontDestroyOnLoad(gameObject);
    }

    public void OnGameplayStart()
    {

    }

    public void SetNewPlayer(Player player)
    {
        myPlayer = player;
        cameraFollowPlayer.SetPlayer(player);
    }

    protected void GetInput()
    {
        if (myPlayer)
        {
            Vector2 direction = gameplayActionMap["Movement"].ReadValue<Vector2>();
            myPlayer.verticalInput = direction.y;
            myPlayer.horizontalInput = direction.x;
        }  
    }

    private void OnEnable()
    {
        selectSidesActionMap.Enable();
        gameplayActionMap.Enable();
    }


    private void OnDisable()
    {
        gameplayActionMap.Disable();
        selectSidesActionMap.Disable();
    }

    public void ToggleUIActionMap(bool enable)
    {
        if (enable)
        {
            gameplayActionMap.Disable();
            selectSidesActionMap.Disable();
            uiActionMap.Enable();
        }
        else
        {
            uiActionMap.Disable();
            selectSidesActionMap.Enable();
            gameplayActionMap.Enable();
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

    private void FixedUpdate()
    {
        GetInput();
    }

    #region Input Methods

    public void ToggleSprint(InputAction.CallbackContext context)
    {
        if (!myPlayer) return;
        if (context.canceled)
        {
            myPlayer.IsSprinting = false;
        }
        else
        {
            myPlayer.IsSprinting = true;
            
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!myPlayer) return;
        if (myPlayer.IsGrounded && myPlayer.playerState == Player.PlayerState.Playing)
        {
            if (myPlayer.HasBall)
            {
                myPlayer.BallStolen();
            }
            myPlayer.IsGrounded = false;
            myPlayer.IsJumping = true;
            myPlayer.Rb.velocity = new Vector3(myPlayer.Rb.velocity.x, myPlayer.jumpForce, myPlayer.Rb.velocity.z);
        }
    }

    public void Kick(InputAction.CallbackContext context)
    {
        if (!myPlayer) return;
        if (myPlayer.playerState == Player.PlayerState.Playing)
        {
            if (context.started)
            {
                myPlayer.StartKick();
            }
            else if (context.canceled)
            {
                myPlayer.StopPoweringUpKick();
            }
        }
    }

    public void Slide(InputAction.CallbackContext context)
    {
        if (!myPlayer) return;
        if (context.canceled) return;
        myPlayer.Slide();

    }

    public void ToggleChipMode(InputAction.CallbackContext context)
    {
        if (!myPlayer) return;
        if (myPlayer.isKicking)
        {
            return;
        }
        if (context.canceled)
        {
            myPlayer.chipModeEnabled = false;
        }
        else
        {
            myPlayer.chipModeEnabled = true;
        }
    }

    public void PauseGame(InputAction.CallbackContext context)
    {
        GameplayManager.instance.PauseGame();
    }

    public void MoveLeft(InputAction.CallbackContext context)
    {
        if (SelectSidesGui.instance == null)
        {
            return;
        }
        if (Time.time - previousMovementTime > .25f)
        {
           previousMovementTime = Time.time;
           SelectSidesGui.instance.PlayerMovedLeftOrRight(playerInput, true, guiSection);
        }

    }
    public void MoveRight(InputAction.CallbackContext context)
    {
        if (SelectSidesGui.instance == null)
        {
            return;
        }
        if (Time.time - previousMovementTime > .25f)
        {
            previousMovementTime = Time.time;
            SelectSidesGui.instance.PlayerMovedLeftOrRight(playerInput, false, guiSection);
        }
    }

    public void MoveUp(InputAction.CallbackContext context)
    {
        if (SelectSidesGui.instance == null)
        {
            return;
        }
        if (Time.time - previousMovementTime > .25f)
        {
            previousMovementTime = Time.time;
            SelectSidesGui.instance.PlayerMovedUpOrDown(playerInput, true, guiSection);
        }
    }

    public void MoveDown(InputAction.CallbackContext context)
    {
        if (SelectSidesGui.instance == null)
        {
            return;
        }
        if (Time.time - previousMovementTime > .25f)
        {
            previousMovementTime = Time.time;
            SelectSidesGui.instance.PlayerMovedUpOrDown(playerInput, false, guiSection);
        }
    }

    public void ReadyUp(InputAction.CallbackContext context)
    {
        if (!context.canceled)
        {
            if (SelectSidesGui.instance == null)
            {
                return;
            }
            SelectSidesGui.instance.ReadiedUp(playerInput, true);
        }
    }
    #endregion

}
