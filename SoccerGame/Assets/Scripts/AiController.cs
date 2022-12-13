using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiController : MonoBehaviour
{



    public Player myPlayer { get; set; }

    [SerializeField]
    private AIState aiState = AIState.Idling;

    public float detectionRadius = 20f;

    public float shotPlacementMaxXOffset = 4f;
    public float shotPlacementMaxYHeight = 2f;
    public float shotPlacementStep = 3f;
    // the radius of the spheres used during the shot path spherecasts
    public float shotClearanceNeeded = .5f;
    private Vector3 currentShotPlacement = Vector3.zero;
    private bool isLiningUpShot = false;

    private Vector3 desiredPosition = Vector3.zero;

    private Player opponentImDefending = null;
    private List<Vector3> shotPlacements = new List<Vector3>();



    public enum AIState
    {
        Idling,
        Dribbling,
        Shooting,
        Attacking,
        Defending
    }



    private void Awake()
    {

        
    }

    private void OnEnable()
    {
        if (myPlayer)
        {
            if (myPlayer.HasBall)
            {
                UpdateAiState(AIState.Dribbling);
            }
            else if (Player.teamWithBall == myPlayer.teamIndex)
            {
                UpdateAiState(AIState.Attacking);
            }
            if (Player.teamWithBall == -1)
            {
                UpdateAiState(AIState.Idling);
            }
            else if (Player.teamWithBall != myPlayer.teamIndex)
            {
                UpdateAiState(AIState.Defending);
            }

        }
    }

    private void OnDisable()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        myPlayer = GetComponent<Player>();

    }

    public void Initialize()
    {
        myPlayer = GetComponent<Player>();
        shotPlacements.Clear();
        float currentYOffset = 1f;
        while (currentYOffset <= (shotPlacementMaxYHeight + 1f) - ((shotPlacementMaxYHeight) / (shotPlacementStep + 1f)))
        {
            currentYOffset += (shotPlacementMaxYHeight) / (shotPlacementStep + 1f);
            float currentXOffset = -shotPlacementMaxXOffset + (2f * shotPlacementMaxXOffset) / (shotPlacementStep + 1f);
            while (currentXOffset <= shotPlacementMaxXOffset - ((2f * shotPlacementMaxXOffset) / (shotPlacementStep + 1f)))
            {

                shotPlacements.Add(myPlayer.opponentsGoalsPosition + new Vector3(currentXOffset, currentYOffset, 0f));
                currentXOffset += (2f * shotPlacementMaxXOffset) / (shotPlacementStep + 1f);

            }

        }
    }

    public void OnGameplayStart()
    {

    }

    public void UpdateAiState(AIState _aiState)
    {
        if (aiState == _aiState)
        {
            // ignore since we're already in this state
            return;
        }
        if (_aiState == AIState.Dribbling)
        {
            aiState = AIState.Dribbling;
        }
        else if (_aiState == AIState.Defending)
        {
            aiState = AIState.Defending;
        }
        else if (_aiState == AIState.Shooting)
        {
            aiState = AIState.Shooting;
        }
        else if (_aiState == AIState.Idling)
        {
            aiState = AIState.Idling;
        }
        else if (_aiState == AIState.Attacking)
        {
            aiState = AIState.Attacking;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        myPlayer.horizontalInput = 0f;
        myPlayer.verticalInput = 0f;
        GetInput();
    }


    private void GetInput()
    {
        desiredPosition = CalculateDesiredPosition();
        Vector3 position = transform.position;
        position.y = 0;
        desiredPosition.y = 0;
        Vector3 relativeDirection ;
        if (myPlayer.teamIndex == 1)
        {
            relativeDirection = -(position - desiredPosition);
        }
        else
        {
            relativeDirection = position - desiredPosition;
        }
        
        Debug.DrawRay(desiredPosition, Vector3.up * 5f, Color.black);
        Debug.DrawLine(transform.position, desiredPosition, Color.magenta);
        Debug.DrawLine(myPlayer.ball.transform.position, myPlayer.myGoalsPosition, Color.blue);


        if (myPlayer)
        {
            myPlayer.verticalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.z) / 5f) * -Mathf.Sign(relativeDirection.z);
            myPlayer.horizontalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.x) / 5f) * -Mathf.Sign(relativeDirection.x);
        }
    }



    private Vector3 CalculateDesiredPosition()
    {

        if (aiState == AIState.Dribbling)
        {
            if ((myPlayer.opponentsGoalsPosition - transform.position).sqrMagnitude < 1200)
            {
                if (!myPlayer.isKicking && !isLiningUpShot)
                {
                    Vector3 potentialShotPlacement = CheckForShotPath();
                    if (potentialShotPlacement != Vector3.zero)
                    {
                        currentShotPlacement = potentialShotPlacement;
                        isLiningUpShot = true;
                        Debug.Log("Shooting!");
                        StartCoroutine(LineUpShotRoutine(1f));
                        return currentShotPlacement;
                    }
                }
                if (currentShotPlacement != Vector3.zero)
                {
                    return currentShotPlacement;
                }
            }
            return myPlayer.opponentsGoalsPosition;
        }
        else if (aiState == AIState.Defending)
        {
            CheckForOpponentToDefend();
            if (opponentImDefending != null)
            {
                if (myPlayer.ball.owner == opponentImDefending)
                {
                    // point between my opponent and our goal
                    Vector3 nearestPoint = FindNearestPointOnLine(opponentImDefending.transform.position, myPlayer.myGoalsPosition, transform.position);
                    if ((transform.position - nearestPoint).sqrMagnitude < 10f)
                    {
                        // if we are between the oppponent and our goal, move towards the opponent
                        return myPlayer.ball.transform.position;
                    }
                    else
                    {
                        return nearestPoint;
                    }

                }
                else
                {
                    // move to center point of triangle created by ball, goal, and opponent
                    return FindCentroid(myPlayer.ball.transform.position, myPlayer.myGoalsPosition, opponentImDefending.transform.position);
                }
            }
            return FindNearestPointOnLine(myPlayer.ball.transform.position, myPlayer.myGoalsPosition, transform.position);
        }
        else if (aiState == AIState.Shooting)
        {
            return myPlayer.opponentsGoalsPosition;
        }
        else if (aiState == AIState.Idling)
        {

            if (Player.teamWithBall == -1)
            {
                return myPlayer.ball.transform.position;
            }
            return transform.position;
        }
        else if (aiState == AIState.Attacking)
        {
            return new Vector3(transform.position.x, 0f, myPlayer.opponentsGoalsPosition.z);
        }

        return transform.position;

    }

    public Vector3 FindNearestPointOnLine(Vector3 origin, Vector3 end, Vector3 point)
    {
        // ignore y difference
        origin.y = 0;
        end.y = 0;
        point.y = 0;

        Vector3 lineDir = (end - origin).normalized;
        Vector3 dirToPoint = (point - origin).normalized;
        float dot = Vector3.Dot(lineDir, dirToPoint);
        if (dot < 0)
        {
            return origin;
        }
        //Debug.Log("Dot: " + dot);
        float length = (origin - point).magnitude;
        float dotLength = length * dot;
        return origin + lineDir * dotLength;
    }

    private void CheckForOpponentToDefend()
    {
        if (myPlayer.opponents.Count > 0)
        {
            float nearestOpponentSquareDistance = 1000f;
            int nearestOpponentIndex = 0;
            for (int opponentIndex = 0; opponentIndex < myPlayer.opponents.Count; opponentIndex++)
            {
                float squareDistance = (transform.position - myPlayer.opponents[opponentIndex].transform.position).sqrMagnitude;
                if (squareDistance < nearestOpponentSquareDistance)
                {
                    nearestOpponentSquareDistance = squareDistance;
                    nearestOpponentIndex = opponentIndex;
                }
            }

            if (nearestOpponentSquareDistance <= detectionRadius * detectionRadius)
            {
                if ((transform.position - myPlayer.ball.transform.position).sqrMagnitude < nearestOpponentSquareDistance)
                {
                    opponentImDefending = null;
                }
                else
                {
                    opponentImDefending = myPlayer.opponents[nearestOpponentIndex];
                }

            }
            else
            {
                opponentImDefending = null;
            }
        }
    }


    // check if we have an open look at goal. Return a shot location if one exists
    private Vector3 CheckForShotPath()
    {

        int shotPlacementIndex = Random.Range(0, shotPlacements.Count - 1);
        RaycastHit hit;
        for (int i = 0; i < shotPlacements.Count; i++)
        {
            if (!Physics.SphereCast(transform.position, shotClearanceNeeded, (shotPlacements[shotPlacementIndex] - transform.position).normalized, out hit, (shotPlacements[i] - transform.position).magnitude, myPlayer.playerLayerMask))
            {
                return shotPlacements[shotPlacementIndex];
            }
            shotPlacementIndex += 1;
            shotPlacementIndex = shotPlacementIndex % shotPlacements.Count;
        }
        // all shot paths are blocked so don't shoot
        return Vector3.zero;
    }

    // Gives the player time to rotate towards where they want to shoot it
    private IEnumerator LineUpShotRoutine(float delay)
    {

        yield return new WaitForSeconds(delay);
        myPlayer.StartKick();
        StartCoroutine(ShotPowerRoutine(CalculateShotPower()));
        isLiningUpShot = false;
    }

    public Vector3 FindCentroid(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float x = (p1.x + p2.x + p3.x) / 3f;
        float z = (p1.z + p2.z + p3.z) / 3f;
        return new Vector3(x, 0f, z);
    }

    // determines how long to hold the shoot button down
    private float CalculateShotPower()
    {
        float squareDistance = (currentShotPlacement - transform.position).sqrMagnitude;
        if (squareDistance < 250f)
        {
            return .3f;
        }
        else if (squareDistance < 810f)
        {
            return .4f;
        }
        else
        {
            return .5f;
        }
    }

    // simulates holding down the kick button
    private IEnumerator ShotPowerRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        myPlayer.EndKick();
    }

}
