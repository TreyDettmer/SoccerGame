using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectSidesPlayer : MonoBehaviour
{
    protected InputActionAsset playerInputAsset;
    protected InputActionMap playerActionMap;
    protected PlayerInput playerInput;
    float previousMovementTime = 0f;
    public bool isReadyToStart = false;
    // guiSection 1 means selecting sides, guiSection 2 means selecting AI count
    public int guiSection = 1;
    public int teamIndex = -1;
    public Color color;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInputAsset = playerInput.actions;
        playerActionMap = playerInputAsset.FindActionMap("SelectSides");
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void MoveLeft(InputAction.CallbackContext context)
    //{
    //    if (Time.time - previousMovementTime > .25f)
    //    {
    //        previousMovementTime = Time.time;
    //        SelectSidesGui.instance.PlayerMovedLeftOrRight(this,playerInput, true, guiSection);
    //    }
        
    //}
    //public void MoveRight(InputAction.CallbackContext context)
    //{
    //    if (Time.time - previousMovementTime > .25f)
    //    {
    //        previousMovementTime = Time.time;
    //        SelectSidesGui.instance.PlayerMovedLeftOrRight(this, playerInput, false, guiSection);
    //    }
    //}

    //public void MoveUp(InputAction.CallbackContext context)
    //{
    //    if (Time.time - previousMovementTime > .25f)
    //    {
    //        previousMovementTime = Time.time;
    //        SelectSidesGui.instance.PlayerMovedUpOrDown(this, playerInput, true, guiSection);
    //    }
    //}

    //public void MoveDown(InputAction.CallbackContext context)
    //{
    //    if (Time.time - previousMovementTime > .25f)
    //    {
    //        previousMovementTime = Time.time;
    //        SelectSidesGui.instance.PlayerMovedUpOrDown(this, playerInput, false, guiSection);
    //    }
    //}

    //public void ReadyUp(InputAction.CallbackContext context)
    //{
    //    if (!context.canceled)
    //    {
    //        isReadyToStart = true;
    //        SelectSidesGui.instance.PlayerReadiedUp(this, playerInput, true);
    //    }
    //}
}
