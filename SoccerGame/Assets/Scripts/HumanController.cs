using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class HumanController : MonoBehaviour
{

    public Player myPlayer { get; set; }

    #region Input
    private InputActionAsset playerInputAsset;
    private InputActionMap gameplayActionMap;
    private InputActionMap uiActionMap;
    private PlayerInput playerInput;
    #endregion

    #region Camera
    public Camera cam;
    public Canvas canvas;
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
        uiActionMap = playerInputAsset.FindActionMap("UI");
        cameraPlayerGui = GetComponent<PlayerGui>();
        cameraFollowPlayer = GetComponent<FollowPlayer>();
        gameState = Player.GameState.SelectSides;
        gameplayActionMap.Disable();
        uiActionMap.Enable();
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
        gameplayActionMap.Enable();
        uiActionMap.Enable();
    }


    private void OnDisable()
    {
        gameplayActionMap.Disable();
        uiActionMap.Disable();
    }

    public void ToggleUIActionMap(bool enable)
    {
        if (enable)
        {
            gameplayActionMap.Disable();
            uiActionMap.Enable();
        }
        else
        {
            uiActionMap.Disable();
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
        if (transform.position.z > 60f || transform.position.z < -60f)
        {
            cam.nearClipPlane = 8f;
            canvas.planeDistance = 10f;
        }
        else
        {
            cam.nearClipPlane = 1f;
            canvas.planeDistance = 3f;
        }
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

    public void OnSwitchPlayers(InputAction.CallbackContext context)
    {
        if (!myPlayer) return;
        if (!myPlayer.ball) return;
        if (myPlayer.ball.owner == myPlayer)
        {
            return;
        }
        if (context.performed)
        {
            GameplayManager.instance.HumanControllerSwitchPlayers(myPlayer, this);
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
        if (context.performed)
        {
            SelectSidesGui.instance.PlayerMovedLeftOrRight(playerInput, true, guiSection);
        }
        //if (SelectSidesGui.instance == null)
        //{
        //    return;
        //}
        //if (Time.time - previousMovementTime > .25f)
        //{
        //   previousMovementTime = Time.time;
        //   SelectSidesGui.instance.PlayerMovedLeftOrRight(playerInput, true, guiSection);
        //}

    }
    public void MoveRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SelectSidesGui.instance.PlayerMovedLeftOrRight(playerInput, false, guiSection);
        }
        //if (SelectSidesGui.instance == null)
        //{
        //    return;
        //}
        //if (Time.time - previousMovementTime > .25f)
        //{
        //    previousMovementTime = Time.time;
        //    SelectSidesGui.instance.PlayerMovedLeftOrRight(playerInput, false, guiSection);
        //}
    }

    public void MoveUp(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SelectSidesGui.instance.PlayerMovedUpOrDown(playerInput, true, guiSection);
        }
        //if (SelectSidesGui.instance == null)
        //{
        //    return;
        //}
        //if (Time.time - previousMovementTime > .25f)
        //{
        //    previousMovementTime = Time.time;
        //    SelectSidesGui.instance.PlayerMovedUpOrDown(playerInput, true, guiSection);
        //}
    }

    public void MoveDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SelectSidesGui.instance.PlayerMovedUpOrDown(playerInput, false, guiSection);
        }
        //if (SelectSidesGui.instance == null)
        //{
        //    return;
        //}
        //if (Time.time - previousMovementTime > .25f)
        //{
        //    previousMovementTime = Time.time;
        //    SelectSidesGui.instance.PlayerMovedUpOrDown(playerInput, false, guiSection);
        //}
    }

    public void ReadyUp(InputAction.CallbackContext context)
    {
        if (!context.canceled)
        {
            if (FindObjectOfType<TitleScreen>())
            {
                FindObjectOfType<TitleScreen>().LoadNextScene();
                return;
            }
            if (SelectSidesGui.instance == null)
            {
                return;
            }
            SelectSidesGui.instance.ReadiedUp(playerInput, true);
        }
    }

    public void SelectSidesMovementInput(InputAction.CallbackContext context)
    {
        if (SelectSidesGui.instance == null)
        {
            return;
        }
        Vector2 input = context.ReadValue<Vector2>();
        // check if we are pressing down only one input
        if (input != Vector2.zero && input.magnitude == 1f)
        {
            if (input.x > 0)
            {
                // move right
                if (Time.time - previousMovementTime > .15f)
                {
                    previousMovementTime = Time.time;
                    SelectSidesGui.instance.PlayerMovedLeftOrRight(playerInput, false, guiSection);
                }
            }
            else if (input.x < 0)
            {
                // move left
                if (Time.time - previousMovementTime > .15f)
                {
                    previousMovementTime = Time.time;
                    SelectSidesGui.instance.PlayerMovedLeftOrRight(playerInput, true, guiSection);
                }
            }
            else if (input.y > 0)
            {
                // move up
                if (Time.time - previousMovementTime > .15f)
                {
                    previousMovementTime = Time.time;
                    SelectSidesGui.instance.PlayerMovedUpOrDown(playerInput, true, guiSection);
                }
            }
            else if (input.y < 0)
            {
                // move down
                if (Time.time - previousMovementTime > .15f)
                {
                    previousMovementTime = Time.time;
                    SelectSidesGui.instance.PlayerMovedUpOrDown(playerInput, false, guiSection);
                }
            }
        }
    }
    #endregion

}
