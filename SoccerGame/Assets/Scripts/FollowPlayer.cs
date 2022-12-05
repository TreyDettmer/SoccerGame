using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FollowPlayer : MonoBehaviour
{

    public PlayerController player;
    private List<Transform> targets = new List<Transform>();

    public Vector3 cameraOffset;
    private float yAngle = 0f;
    private float xAngle = 0f;
    public float minYOffset = -10f;
    public float maxYOffset = 80f;
    private float yOffset = 0f;

    [Range(0.01f,1.0f)]
    public float smoothFactor = 0.125f;
    private Vector3 velocity;
    public bool lookInOppositeDirection = false;

    public bool lookAtPlayer = false;

    public bool rotateAroundPlayer = true;
    public float horizontalRotationSpeed = 5.0f;
    public float verticalRotationSpeed = 2.0f;
    public float minZoom = 105f;
    public float maxZoom = 60f;
    public float zoomLimiter = 50f;

    public GameObject ballDirectionImageLeft;
    public GameObject ballDirectionImageRight;
    private bool isBallDirectionImageEnabled = false;

    private Camera cam;
    private Transform ballTransform;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnGameplayStart()
    {
        if (player.teamIndex == 1)
        {
            cameraOffset.z *= -1;
        }

        targets.Add(player.transform);
        ballTransform = FindObjectOfType<Ball>().transform;
        targets.Add(ballTransform);
        yOffset = cameraOffset.y;
        cam = GetComponent<Camera>();
        Debug.Log(yOffset);
    }

    private void LateUpdate()
    {


    }

    private void FixedUpdate()
    {
        if (player.gameState != PlayerController.GameState.Gameplay)
        {
            return;
        }
        Vector3 newPos = player.transform.position + cameraOffset;
        float ballDiff = Mathf.Abs(ballTransform.position.z - transform.position.z);
        float playerDiff = Mathf.Abs(player.transform.position.z - transform.position.z);
        if (ballDiff < playerDiff && !player.hasBall)
        {
            
            float zOffset = Mathf.Clamp(ballTransform.position.z - Mathf.Sign(transform.forward.z) * 10f, -500f, 500f);
            newPos = new Vector3(newPos.x, newPos.y, zOffset);
        }
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
        //float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        //cam.fieldOfView = newZoom;

    }

    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }
        return bounds.center;
    }

    float GetGreatestDistance()
    {

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }
        return bounds.size.x;
    }

    public void Orbit(Vector2 movement)
    {
        float xMovement = Mathf.Clamp(movement.x, -1f, 1f);
        float yMovement = Mathf.Clamp(movement.y, -.25f, .25f);
        yOffset += -yMovement * verticalRotationSpeed;
        yOffset = Mathf.Clamp(yOffset, minYOffset, maxYOffset);
        
        if (rotateAroundPlayer)
        {
            
            Quaternion camTurnAngle = Quaternion.AngleAxis(xMovement * horizontalRotationSpeed, Vector3.up);
            cameraOffset = new Vector3(cameraOffset.x, yOffset, cameraOffset.z);
            cameraOffset = camTurnAngle * cameraOffset;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
