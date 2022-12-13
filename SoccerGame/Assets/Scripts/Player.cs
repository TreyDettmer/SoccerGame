using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine;

public class Player : MonoBehaviour
{

    public enum ControllerType
    {
        Keyboard,
        Xbox,
        Switch,
        AI
    }

    public enum GameState
    {
        SelectSides,
        Gameplay
    }

    public enum PlayerState
    {
        Penalized,
        Waiting,
        Playing
    }

    public static int teamWithBall = -1;

    public GameState gameState = GameState.SelectSides;

    [Header("Movement")]
    public float maxSpeed;
    public float acceleration;
    public float maxAccelForce;
    public float speedFactor = 1f;
    public float maxAccelForceFactor = 1f;
    public Vector3 forceScale;
    public float rotationSpeed = 1f;
    public float kickingRotationSpeed = 0.8f;
    public Vector3 goalVelocity = Vector3.zero;
    public AnimationCurve AccelerationFactorFromDot;
    public AnimationCurve MaxAccelerationFactorFromDot;
    public Transform graphicsTransform;
    public float horizontalInput;
    public float verticalInput;
    public float currentMaxSpeed;
    private Vector3 unitGoalVelocity;
    private Vector3 previousPosition;
    private Vector3 currentPosition;

    [Header("Physics")]
    public float defaultHeight;
    public float springStrength;
    public float springDampening;
    public float jumpForce;
    public float uprightSprintStrength;
    public float uprightSpringDampening;

    public float legSpringStrength;
    public float legSpringDampening;

    [Header("Kicking")]
    public AnimationCurve kickPowerCurve;
    public AnimationCurve kickHeightCurve;
    public AnimationCurve chipPowerCurve;
    public AnimationCurve chipHeightCurve;
    public float kickHeightFactor = 10f;
    public float kickPowerFactor = 30f;
    public float kickSpinFactor = 20f;
    public float maxKickBackswingTime = 1.5f;
    public float kickForce = 0f;
    public float kickHeightForce = 0f;
    public bool isKicking = false;
    public bool canKick = true;
    public bool chipModeEnabled = false;
    public float kickBackswingElapsedTime = 0f;


    [SerializeField]
    public LayerMask ballLayerMask;
    public bool isWithinKickingRange = false;
    [HideInInspector]
    public Ball ball;
    public Rigidbody ballRb;
    public AiController aiController;


    #region Info
    [HideInInspector] public bool IsSliding { get; set; } = false;
    [SerializeField]
    private float slideForce = 5f;
    [SerializeField]
    private float maxSlideSpeed = 10f;
    [SerializeField]
    private float slideTime = 1f;
    [SerializeField]
    private float slideCooldownTime = 0.5f;
    [SerializeField]
    private float penaltyTime = 2f;
    private Vector3 slideDirection = Vector3.zero;
    private bool slidIntoBall = false;
    private bool shouldBePenalized = false;
    [HideInInspector] public bool CanSlide { get; set; } = true;
    [SerializeField]
    private float ballDribbleDirectionFactor = .3f;
    [SerializeField]
    private float sprintingMaxSpeed = 4f;

    [HideInInspector] public bool IsSprinting { get; set; } = false;
    [HideInInspector] public bool CanDribble { get; set; } = true;
    [HideInInspector] public bool IsGrounded { get; set; } = false;
    [HideInInspector] public bool IsJumping { get; set; } = false;

    [HideInInspector] public bool HasBall { get; set; } = false;
    [SerializeField] private float ballcanDribbleAngle = 45f;
    [SerializeField] private float ballcanDribbleDistance = 4f;
    [SerializeField] private float ballcanDribbleCooldown = 1f;

    [SerializeField] private LayerMask groundLayerMask;

    [SerializeField] private Transform groundChecker;


    [HideInInspector] public Rigidbody Rb { get; set; }
    private Quaternion goalRotation;

    private Goalie myGoalie;
    #endregion




    public List<Player> teammates = new List<Player>();
    
    public List<Player> opponents = new List<Player>();

    #region Appearance
    [Header("Appearance")]
    public GameObject[] blueTeamSkins;
    public GameObject[] redTeamSkins;
    public SkinnedMeshRenderer meshRenderer;
    public Material playingMaterial;
    public Material penalizedMaterial;
    public Animator animator;
    #endregion

    public PlayerState playerState;

    public int teamIndex = -1;

    
    [SerializeField]


    public AudioSource audioSource;

    [HideInInspector]
    public Vector3 myGoalsPosition = Vector3.zero;
    [HideInInspector]
    public Vector3 opponentsGoalsPosition = Vector3.zero;
    [HideInInspector]
    public LayerMask playerLayerMask;


    #region GUI
    protected PlayerGui cameraPlayerGui;

    
    #endregion


    // Start is called before the first frame update
    protected virtual void Start()
    {
        playerState = PlayerState.Waiting;
        Rb = GetComponent<Rigidbody>();
        playerState = PlayerState.Waiting;
        currentMaxSpeed = maxSpeed;
        ball = FindObjectOfType<Ball>();
        ballRb = ball.GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        Goalie[] goalieObjects = FindObjectsOfType<Goalie>();
        for (int i = 0; i < goalieObjects.Length; i++)
        {
            if (goalieObjects[i].teamIndex == teamIndex)
            {
                myGoalie = goalieObjects[i];
            }
        }


        myGoalsPosition = GameplayManager.instance.GetGoalPositionForTeam(teamIndex);
        opponentsGoalsPosition = teamIndex == 0 ? GameplayManager.instance.GetGoalPositionForTeam(1) : GameplayManager.instance.GetGoalPositionForTeam(0);
        playerLayerMask = LayerMask.NameToLayer("Player");
        aiController = GetComponent<AiController>();
        aiController.Initialize();
    }

    public void OnGameplayStart()
    {


    }

    public void UpdateGameState(GameState _gameState)
    {
        if (gameState == _gameState)
        {
            return;
        }
        if (_gameState == GameState.Gameplay)
        {
            animator = GetComponent<Animator>();
            gameState = GameState.Gameplay;
        }
    }


    
    public void Update()
    {
        if (gameState == GameState.Gameplay)
        {

            // if we do not have the ball
            if (!HasBall)
            {
                // check if the ball is in a dribblable position
                if (CheckIfCanDribble())
                {
                    // check if ball isn't being dribbled by someone else
                    if (ball.owner == null)
                    {
                        // ensure that we are not sliding
                        if (!IsSliding)
                        {
                            // ensure that we are not being penalized for fouling
                            if (playerState != PlayerState.Penalized)
                            {
                                // check if it's been long enough since we last had the ball
                                if (CanDribble)
                                {
                                    HasBall = true;
                                    ball.SetOwner(this);
                                    Debug.Log("New owner");
                                    aiController.UpdateAiState(AiController.AIState.Dribbling);
                                    ball.transform.position = transform.position + transform.forward * 1.5f;
                                }
                            }

                        }

                    }
                }
            }
            if (isKicking)
            {
                kickBackswingElapsedTime += Time.deltaTime;
                float kickPowerCurveValue;
                if (chipModeEnabled)
                {
                    kickPowerCurveValue = chipPowerCurve.Evaluate(kickBackswingElapsedTime / maxKickBackswingTime);
                }
                else
                {
                    kickPowerCurveValue = kickPowerCurve.Evaluate(kickBackswingElapsedTime / maxKickBackswingTime);
                }

                kickForce = kickPowerFactor * kickPowerCurveValue;
                float kickHeightCurveValue;
                if (chipModeEnabled)
                {
                    kickHeightCurveValue = chipHeightCurve.Evaluate(kickBackswingElapsedTime / maxKickBackswingTime);
                }
                else
                {
                    kickHeightCurveValue = kickHeightCurve.Evaluate(kickBackswingElapsedTime / maxKickBackswingTime);
                }
                kickHeightForce = kickHeightFactor * kickHeightCurveValue;
                if (cameraPlayerGui)
                {
                    cameraPlayerGui.UpdatePowerMeter(kickBackswingElapsedTime / maxKickBackswingTime);
                }
                if (kickBackswingElapsedTime >= maxKickBackswingTime)
                {
                    EndKick();
                }
            }

        }

        
    }

    public void FixedUpdate()
    {
        currentPosition = Rb.position;
        float speed = ((currentPosition - previousPosition) / Time.fixedDeltaTime).magnitude;
        previousPosition = currentPosition;
        animator.SetFloat("speed", Mathf.Clamp(speed / (sprintingMaxSpeed - 4f), 0f, 1f));
        if (playerState == PlayerState.Playing)
        {
            if (IsSliding)
            {
                if (Rb.velocity.sqrMagnitude < maxSlideSpeed * maxSlideSpeed)
                {
                    Rb.AddForce(slideDirection * slideForce);

                }
                return;
            }
            unitGoalVelocity = GetDirectionOfMovement(horizontalInput, verticalInput).normalized;
            if ((horizontalInput != 0 || verticalInput != 0))
            {
                
                goalRotation = Quaternion.LookRotation(unitGoalVelocity, Vector3.up);
                
                Quaternion toRotation = Quaternion.LookRotation(unitGoalVelocity, Vector3.up);
                if (isKicking)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, kickingRotationSpeed * Time.deltaTime);
                }
                else
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
                }

            }


            Vector3 unitVelocity = new Vector3(Rb.velocity.x, 0f, Rb.velocity.z);
            float velDot = Vector3.Dot(unitGoalVelocity, unitVelocity);
            float accel = acceleration * AccelerationFactorFromDot.Evaluate(velDot);
            Vector3 _goalVelocity = unitGoalVelocity * currentMaxSpeed * speedFactor;
            
            goalVelocity = Vector3.MoveTowards(goalVelocity, _goalVelocity, accel * Time.fixedDeltaTime);
            Vector3 neededAccel = (goalVelocity - Rb.velocity) / Time.fixedDeltaTime;

            float maxAccel = maxAccelForce * MaxAccelerationFactorFromDot.Evaluate(velDot) * maxAccelForceFactor;

            neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);
            Rb.AddForce(Vector3.Scale(neededAccel * Rb.mass, forceScale));



        }

        RaycastHit hit;
        if (Physics.Raycast(groundChecker.position, -transform.up, out hit, defaultHeight, groundLayerMask))
        {
            if (!IsJumping)
            {
                IsGrounded = true;
            }
        }
        else
        {
            IsGrounded = false;
            if (IsJumping)
            {
                IsJumping = false;
            }

        }
        if (IsSprinting)
        {
            currentMaxSpeed = sprintingMaxSpeed;
        }
        else
        {
            currentMaxSpeed = maxSpeed;
        }
        
    }


    public Vector3 GetDirectionOfMovement(float _horizontalInput, float _verticalInput)
    {
 
        Vector3 forwardRelativeVerticalInput;
        Vector3 rightRelativeHorizontalInput;
        if (teamIndex == 1)
        {
            forwardRelativeVerticalInput = _verticalInput * -Vector3.forward;
            rightRelativeHorizontalInput = _horizontalInput * -Vector3.right;
        }
        else
        {
            forwardRelativeVerticalInput = _verticalInput * Vector3.forward;
            rightRelativeHorizontalInput = _horizontalInput * Vector3.right;
        }


        return forwardRelativeVerticalInput + rightRelativeHorizontalInput;
    }

    public virtual void StartKick()
    {
        if (!isKicking && !IsSliding)
        {
            Debug.Log("Starting kick");
            isKicking = true;
            canKick = false;
        }
    }

    public virtual void EndKick()
    {
        if (!isKicking)
        {
            return;
        }

        if (ball != null)
        {
            if (CheckIfCanDribble())
            {
                if (HasBall)
                {
                    audioSource.Play();
                    Vector3 directionToBall = (ball.transform.position - transform.position).normalized;
                    Vector3 sideCurveDirection = (transform.right * horizontalInput).normalized;
                    Vector3 topCurveDirection = (transform.forward * -verticalInput).normalized;
                    sideCurveDirection.y = 0;
                    sideCurveDirection = sideCurveDirection.normalized;
                    topCurveDirection.x = 0;
                    topCurveDirection = topCurveDirection.normalized;
                    directionToBall.y = 0;
                    directionToBall = directionToBall.normalized;
                    Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
                    Vector3 right = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
                    float sideDot = Vector3.SignedAngle(sideCurveDirection, forward, Vector3.up);
                    float topDot = Vector3.SignedAngle(topCurveDirection, right, Vector3.up);
                    Debug.Log("SideDot: " + sideDot + " TopDot: " + topDot);
                    Vector3 torque = new Vector3(-topDot * kickSpinFactor, -sideDot * kickSpinFactor, 0f);

                    ball.lastKickedBy = this;
                    ball.SetOwner(null);
                    HasBall = false;
                    StartCoroutine(BallPickupCooldownRoutine());
                    ballRb.AddForce(directionToBall * kickForce + new Vector3(0f, kickHeightForce, 0f), ForceMode.Impulse);

                    ballRb.AddForce(directionToBall * kickForce + new Vector3(0f, kickHeightForce, 0f), ForceMode.Impulse);
                    ballRb.AddTorque(torque, ForceMode.VelocityChange);

                }
            }

        }

        if (cameraPlayerGui)
        {
            cameraPlayerGui.UpdatePowerMeter(0f);
        }
        isKicking = false;
        kickBackswingElapsedTime = 0f;
    }

    public virtual void UpdateMaterial()
    {

        if (teamIndex == 1)
        {
            int randomSkinIndex = Random.Range(0, redTeamSkins.Length - 1);
            redTeamSkins[randomSkinIndex].SetActive(true);
            meshRenderer = blueTeamSkins[randomSkinIndex].GetComponent<SkinnedMeshRenderer>();
        }
        else
        {
            int randomSkinIndex = Random.Range(0, blueTeamSkins.Length - 1);
            blueTeamSkins[randomSkinIndex].SetActive(true);
            meshRenderer = blueTeamSkins[randomSkinIndex].GetComponent<SkinnedMeshRenderer>();
        }
    }

    public virtual void ResetValues()
    {
        HasBall = false;
        CanDribble = true;
    }

    public virtual void UpdatePlayerState(PlayerState newState)
    {
        if (newState == PlayerState.Penalized)
        {
            if (playerState != PlayerState.Penalized)
            {
                if (IsSliding)
                {
                    shouldBePenalized = true;
                    return;
                }
                playerState = PlayerState.Penalized;
                meshRenderer.material = penalizedMaterial;
                gameObject.layer = LayerMask.NameToLayer("PlayerPenalized");
                StartCoroutine(PenaltyTimeoutRoutine());


            }
        }
        else if (newState == PlayerState.Playing)
        {
            if (playerState != PlayerState.Playing)
            {
                gameObject.layer = LayerMask.NameToLayer("Player");
                meshRenderer.material = playingMaterial;
                playerState = PlayerState.Playing;
            }
        }
        else if (newState == PlayerState.Waiting)
        {
            playerState = PlayerState.Waiting;
            gameObject.layer = LayerMask.NameToLayer("Player");
            meshRenderer.material = playingMaterial;
        }
    }

    protected IEnumerator PenaltyTimeoutRoutine()
    {
        yield return new WaitForSeconds(penaltyTime);
        UpdatePlayerState(PlayerState.Playing);
    }

    public bool CheckIfCanDribble()
    {
        if ((ball.transform.position - transform.position).sqrMagnitude <= ballcanDribbleDistance)
        {
            Vector3 dir = (ball.transform.position - transform.position).normalized;
            dir.y = 0;
            dir = dir.normalized * 3f;
            Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            float angle = Vector3.SignedAngle(forward, dir.normalized, Vector3.up);
            if (angle >= -ballcanDribbleAngle && angle <= ballcanDribbleAngle)
            {
                if (Mathf.Abs(ball.transform.position.y - transform.position.y) < 1f)
                {
                    return true;
                }


            }

        }
        return false;
    }

    public void BallStolen()
    {

        ball.SetOwner(null);
        HasBall = false;
        StartCoroutine(BallPickupCooldownRoutine());
    }

    protected IEnumerator BallPickupCooldownRoutine()
    {
        CanDribble = false;
        yield return new WaitForSeconds(ballcanDribbleCooldown);
        CanDribble = true;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {
            if (IsSliding)
            {
                slidIntoBall = true;
            }
            if (!HasBall)
            {
                if (ball.owner == null)
                {

                }
                else if (ball.owner != this)
                {
                    ball.owner.BallStolen();
                    StartCoroutine(BallPickupCooldownRoutine());
                }


            }

        }
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (collision.collider.GetComponent<Player>())
            {
                if (collision.collider.GetComponent<Player>().teamIndex != teamIndex)
                {
                    if (IsSliding && slidIntoBall == false)
                    {
                        GameplayManager.instance.PlayerFoul(this, collision.collider.GetComponent<Player>());
                        Debug.Log("Foul on team " + teamIndex);
                    }
                }
            }

        }
    }

    public void Slide()
    {
        if (IsSliding || !IsGrounded || playerState != PlayerState.Playing || !CanSlide || HasBall)
        {
            return;
        }

        IsSliding = true;
        slideDirection = transform.forward;
        slideDirection.y = 0;
        slideDirection = slideDirection.normalized;
        StartCoroutine(SlideRoutine());
    }

    public void EndSlide()
    {
        //animator.SetBool("isSliding", false);

        slidIntoBall = false;
        IsSliding = false;
        if (shouldBePenalized)
        {
            shouldBePenalized = false;
            UpdatePlayerState(PlayerState.Penalized);
        }
        CanSlide = false;
        StartCoroutine(SlideCooldownRoutine());
    }

    protected IEnumerator SlideCooldownRoutine()
    {
        yield return new WaitForSeconds(slideCooldownTime);
        CanSlide = true;
    }

    public IEnumerator SlideRoutine()
    {
        yield return new WaitForSeconds(slideTime * .75f);
        //transform.DORotate(new Vector3(90f, 0f, 0f), slideTime / 4f, RotateMode.LocalAxisAdd);
        yield return new WaitForSeconds(slideTime * .25f);
        EndSlide();
    }

    public virtual void BallEvent(Player playerWithBall)
    {
        if (aiController == null)
        {
            aiController = GetComponent<AiController>();
        }
        if (teamWithBall != teamIndex)
        {
            if (teamWithBall == -1)
            {
                aiController.UpdateAiState(AiController.AIState.Idling);
            }
            else
            {
                aiController.UpdateAiState(AiController.AIState.Defending);
            }
            
        }
        else if (teamWithBall == teamIndex)
        {
            if (playerWithBall == this)
            {
                aiController.UpdateAiState(AiController.AIState.Dribbling);
            }
            else
            {
                aiController.UpdateAiState(AiController.AIState.Attacking);
            }
        }
    }

    public void SetNewController(HumanController humanController)
    {
        if (humanController)
        {
            if (aiController == null)
            {
                aiController = GetComponent<AiController>();
            }
            aiController.enabled = false;
            cameraPlayerGui = humanController.cameraPlayerGui;
        }
        else
        {
            if (aiController == null)
            {
                aiController = GetComponent<AiController>();
                aiController.Initialize();
            }
            cameraPlayerGui = null;
            aiController.enabled = true;
        }
    }
}
