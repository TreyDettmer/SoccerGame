using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FollowPlayer : MonoBehaviour
{

    private HumanController humanController;



    [HideInInspector] public Player player;
    

    public Vector3 cameraOffset;
    public float minYOffset = -10f;
    public float maxYOffset = 80f;
    private float yOffset = 0f;

    [Range(0.01f,1.0f)]
    public float smoothFactor = 0.125f;
    private Vector3 velocity;
    public bool lookInOppositeDirection = false;

    public bool lookAtPlayer = false;

    public bool rotateAroundPlayer = true;
    public float transitionSpeed = 10f;

    public GameObject ballDirectionImageLeft;
    public GameObject ballDirectionImageRight;
    private bool isBallDirectionImageEnabled = false;

    private Camera cam;
    private Transform ballTransform;
    private bool isTransitioningToNewPlayer = false;
    
    private void Awake()
    {
        humanController = GetComponent<HumanController>();
    }
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    public void SetPlayer(Player _player)
    {
        if (player != null)
        {
            player.SetNewController(null);
            isTransitioningToNewPlayer = true;
        }
        if (_player == null)
        {
            player = null;
            return;
        }
        player = _player;
        if (humanController.teamIndex == 0)
        {

            cameraOffset.z = -Mathf.Abs(cameraOffset.z);

        }
        else
        {

            cameraOffset.z = Mathf.Abs(cameraOffset.z);
            
        }
        //transform.position = player.transform.position + cameraOffset;
        //transform.rotation = player.transform.rotation;
        //transform.LookAt(player.transform);
        ballTransform = FindObjectOfType<Ball>().transform;
    }

    public void OnGameplayStart()
    {


        
        yOffset = cameraOffset.y;
        cam = GetComponent<Camera>();
        Debug.Log(yOffset);
    }

    private void LateUpdate()
    {


    }

    private void FixedUpdate()
    {

        if (!ballTransform || !player) return;
        if (player.gameState != Player.GameState.Gameplay)
        {
            return;
        }

        Vector3 newPos = player.transform.position + cameraOffset;
        if (isTransitioningToNewPlayer)
        {
            transform.position = Vector3.Slerp(transform.position, newPos, transitionSpeed * Time.fixedDeltaTime);
            if ((transform.position - newPos).sqrMagnitude < 2f)
            {
                isTransitioningToNewPlayer = false;
            }
            return;
        }
        float ballDiff = Mathf.Abs(ballTransform.position.z - transform.position.z);
        float playerDiff = Mathf.Abs(player.transform.position.z - transform.position.z);
        //if (ballDiff < playerDiff && !player.HasBall)
        //{
            
        //    float zOffset = Mathf.Clamp(ballTransform.position.z - Mathf.Sign(transform.forward.z) * 10f, -500f, 500f);
        //    newPos = new Vector3(newPos.x, newPos.y, zOffset);
        //}
        Vector3 ballViewportPosition = cam.WorldToViewportPoint(ballTransform.position);
        if (ballViewportPosition.x < 0f || ballViewportPosition.x > 1f)
        {
            if (!isBallDirectionImageEnabled)
            {
                if (ballViewportPosition.x > 1f)
                {
                    ballDirectionImageRight.SetActive(true);
                    ballDirectionImageLeft.SetActive(false);
                }
                else
                {
                    ballDirectionImageLeft.SetActive(true);
                    ballDirectionImageRight.SetActive(false);
                }

                isBallDirectionImageEnabled = true;
            }
        }
        else
        {
            if (isBallDirectionImageEnabled)
            {
                ballDirectionImageLeft.SetActive(false);
                ballDirectionImageRight.SetActive(false);
                isBallDirectionImageEnabled = false;
            }
        }

        transform.position = Vector3.Slerp(transform.position, newPos, smoothFactor);
        if (lookAtPlayer || rotateAroundPlayer)
        {
            transform.LookAt(player.transform);
        }


    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
