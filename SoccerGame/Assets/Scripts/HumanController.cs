using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HumanController : MonoBehaviour
{

    Player myPlayer;

    #region Input
    private InputActionAsset playerInputAsset;
    private InputActionMap gameplayActionMap;
    private InputActionMap selectSidesActionMap;
    private PlayerInput playerInput;
    #endregion

    #region Camera
    public Camera cam;
    private FollowPlayer cameraFollowPlayer;
    #endregion

    #region GUI
    private float previousMovementTime = 0f;
    // guiSection 1 means selecting sides, guiSection 2 means selecting AI count
    private int guiSection = 1;
    private Color color;
    public PlayerGui cameraPlayerGui;
    #endregion

    #region Info
    Player.GameState gameState;
    #endregion



    private void Awake()
    {
        cam = GetComponent<Camera>();
        playerInput = GetComponent<PlayerInput>();
        playerInputAsset = playerInput.actions;
        gameplayActionMap = playerInputAsset.FindActionMap("Player");
        selectSidesActionMap = playerInputAsset.FindActionMap("SelectSides");
        cameraPlayerGui = cam.GetComponent<PlayerGui>();
        gameState = Player.GameState.Gameplay;
        gameplayActionMap.Disable();
    }

    public void OnGameplayStart()
    {

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
                myPlayer.EndKick();
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

    public void RequestRestart(InputAction.CallbackContext context)
    {
        if (!myPlayer) return;
        if (context.canceled)
        {
            GameplayManager.instance.RequestedRestart(this, true);
        }
        else
        {
            GameplayManager.instance.RequestedRestart(this);
        }
    }



    public void MoveLeft(InputAction.CallbackContext context)
    {
        if (!myPlayer) return;
        if (SelectSidesGui.instance == null)
        {
            return;
        }
        if (Time.time - previousMovementTime > .25f)
        {
           previousMovementTime = Time.time;
            SelectSidesGui.instance.PlayerMovedLeftOrRight(this, true, guiSection);
        }

    }
    public void MoveRight(InputAction.CallbackContext context)
    {
        if (!myPlayer) return;
        if (SelectSidesGui.instance == null)
        {
            return;
        }
        if (Time.time - previousMovementTime > .25f)
        {
            previousMovementTime = Time.time;
            SelectSidesGui.instance.PlayerMovedLeftOrRight(this, false, guiSection);
        }
    }

    public void MoveUp(InputAction.CallbackContext context)
    {
        if (!myPlayer) return;
        if (SelectSidesGui.instance == null)
        {
            return;
        }
        if (Time.time - previousMovementTime > .25f)
        {
            previousMovementTime = Time.time;
            SelectSidesGui.instance.PlayerMovedUpOrDown(this, true, guiSection);
        }
    }

    public void MoveDown(InputAction.CallbackContext context)
    {
        if (!myPlayer) return;
        if (SelectSidesGui.instance == null)
        {
            return;
        }
        if (Time.time - previousMovementTime > .25f)
        {
            previousMovementTime = Time.time;
            SelectSidesGui.instance.PlayerMovedUpOrDown(this, false, guiSection);
        }
    }

    public void ReadyUp(InputAction.CallbackContext context)
    {
        if (!myPlayer) return;
        if (!context.canceled)
        {
            if (SelectSidesGui.instance == null)
            {
                return;
            }
            SelectSidesGui.instance.PlayerReadiedUp(this, true);
        }
    }
    #endregion

}
