using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private bool isLiningUpPass = false;
    [HideInInspector] public Player playerImPassingTo = null;

    private Vector3 desiredPosition = Vector3.zero;

    private Player opponentImDefending = null;
    private List<Vector3> shotPlacements = new List<Vector3>();
    private LayerMask goalLayerMask;
    [HideInInspector] public TeamBrain teamBrain;
    Transform formationSpot = null;


    public enum AIState
    {
        Idling,
        Dribbling,
        Shooting,
        Passing,
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
            myPlayer.IsSprinting = true;
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
        goalLayerMask = FindObjectOfType<Goal>().gameObject.layer;
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
        else if (_aiState == AIState.Passing)
        {
            aiState = AIState.Passing;
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
        if (aiState == AIState.Defending)
        {

            List<Player> opponentsSorted = new List<Player>(myPlayer.opponents);
            opponentsSorted = opponentsSorted.OrderBy(opponent => (transform.position - opponent.transform.position).sqrMagnitude).ToList();
            opponentImDefending = opponentsSorted.Count > 0 ? opponentsSorted[0] : null;

            if (opponentImDefending != null)
            {
                DefendOpponent();
            }
        }
        else if (aiState == AIState.Attacking)
        {
            GetOpen();
        }
        else if (aiState == AIState.Idling)
        {
            PursueBall();
        }
        else if (aiState == AIState.Dribbling)
        {
            DribbleBall();
        }
        else if (aiState == AIState.Shooting)
        {
            ShootBall();
        }
        else if (aiState == AIState.Passing)
        {
            PassBall();
        }

    }

    Player FindClosestOpponent()
    {
        float closestOpponentSqrMagnitude = 3000f;
        Player closestOpponent = myPlayer.ball.owner;
        for (int i = 0; i < myPlayer.opponents.Count; i++)
        {
            if (myPlayer.ball.owner != myPlayer.opponents[i])
            {
                float sqrMagnitude = (transform.position - myPlayer.opponents[i].transform.position).sqrMagnitude;
                if (sqrMagnitude < closestOpponentSqrMagnitude)
                {
                    closestOpponentSqrMagnitude = sqrMagnitude;
                    closestOpponent = myPlayer.opponents[i];
                }
            }
        }
        return closestOpponent;
    }

    void ShootBall()
    {
        Vector3 direction = currentShotPlacement - transform.position;
        Vector3 relativeDirection;
        if (myPlayer.teamIndex == 1)
        {
            relativeDirection = direction;
        }
        else
        {
            relativeDirection = -direction;
        }
        myPlayer.verticalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.z) / 8f) * -Mathf.Sign(relativeDirection.z);
        myPlayer.horizontalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.x) / 8f) * -Mathf.Sign(relativeDirection.x);
    }

    void PassBall()
    {
        //Vector3 direction = CalculateInterceptCourse(playerImPassingTo.transform.position, playerImPassingTo.currentVelocity, transform.position, myPlayer.sprintingMaxSpeed);
        Vector3 direction = playerImPassingTo.transform.position - transform.position;
        Vector3 relativeDirection;
        if (myPlayer.teamIndex == 1)
        {
            relativeDirection = direction;
        }
        else
        {
            relativeDirection = -direction;
        }
        Debug.DrawRay(transform.position + new Vector3(0, 1f, 0f), direction.normalized * 5f,Color.white);
        desiredPosition = transform.position + direction;
        myPlayer.verticalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.z) / 8f) * -Mathf.Sign(relativeDirection.z);
        myPlayer.horizontalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.x) / 8f) * -Mathf.Sign(relativeDirection.x);
    }


    public bool CheckForOpenTeammate()
    {
        Player optimalTeammateToPassTo = PitchGrid.instance.FindOptimalTeammateToPassTo(myPlayer);
        if (optimalTeammateToPassTo != null)
        {
            playerImPassingTo = optimalTeammateToPassTo;
            return true;
        }
        return false;
    }

    void DribbleBall()
    {
        Vector3 direction = Vector3.zero;
        if (CheckForOpenTeammate() && !isLiningUpPass)
        {
            Debug.Log("Passing!");
            isLiningUpPass = true;
            StartCoroutine(LineUpPassRoutine(1f));
            UpdateAiState(AIState.Passing);
            return;
        }
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
                    direction = currentShotPlacement - transform.position;
                    UpdateAiState(AIState.Shooting);
                    return;
                }
            }
            if (direction == Vector3.zero && currentShotPlacement != Vector3.zero)
            {
                direction = currentShotPlacement - transform.position;
            }
        }
        if (direction == Vector3.zero)
        {
            direction = myPlayer.opponentsGoalsPosition - transform.position;
        }
        Vector3 relativeDirection;
        if (myPlayer.teamIndex == 1)
        {
            relativeDirection = direction;
        }
        else
        {
            relativeDirection = -direction;
        }
        myPlayer.verticalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.z) / 8f) * -Mathf.Sign(relativeDirection.z);
        myPlayer.horizontalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.x) / 8f) * -Mathf.Sign(relativeDirection.x);
    }



    void PursueBall()
    {
        Vector3 direction = CalculateInterceptCourse(myPlayer.ball.transform.position, myPlayer.ballRb.velocity, transform.position, myPlayer.sprintingMaxSpeed);
        Vector3 relativeDirection;
        if (myPlayer.teamIndex == 1)
        {
            relativeDirection = direction;
        }
        else
        {
            relativeDirection = -direction;
        }
        myPlayer.verticalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.z) / 8f) * -Mathf.Sign(relativeDirection.z);
        myPlayer.horizontalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.x) / 8f) * -Mathf.Sign(relativeDirection.x);
    }

    void DefendOpponent()
    {
        if (myPlayer.ball.owner == opponentImDefending)
        {
            Vector3 directionToMyGoal = (myPlayer.myGoalsPosition - myPlayer.ball.owner.transform.position).normalized;
            directionToMyGoal.y = 0;
            directionToMyGoal.Normalize();


            // check if the opponent is already being defended
            bool isAlreadyDefended = false;
            for (int i = 0; i < myPlayer.teammates.Count; i++)
            {
                // check if teammate is in a defensive position against this opponent
                if ((myPlayer.teammates[i].transform.position - (myPlayer.ball.owner.transform.position + directionToMyGoal * 4f)).sqrMagnitude < 6f)
                {
                    isAlreadyDefended = true;
                    break;
                }
            }
            if (isAlreadyDefended)
            {
                desiredPosition = myPlayer.ball.transform.position;
            }
            else
            {
                desiredPosition = myPlayer.ball.owner.transform.position + directionToMyGoal * 4f;
            }
            if ((transform.position - desiredPosition).sqrMagnitude < 6)
            {
                myPlayer.IsSprinting = false;
            }
            else
            {
                myPlayer.IsSprinting = true;
            }
            Vector3 direction = CalculateInterceptCourse(desiredPosition, myPlayer.ball.owner.currentVelocity, transform.position, myPlayer.sprintingMaxSpeed);
            if (direction == Vector3.zero)
            {
                Debug.DrawRay(transform.position, myPlayer.ball.owner.transform.position - transform.position, Color.black);
            }
            else
            {
                Debug.DrawRay(transform.position, direction, Color.red);
            }
            if (direction == Vector3.zero)
            {
                direction = myPlayer.ball.owner.transform.position - transform.position;
            }

            Vector3 relativeDirection;
            if (myPlayer.teamIndex == 1)
            {
                relativeDirection = direction;
            }
            else
            {
                relativeDirection = -direction;
            }

            // calculate input based on distance

            float distanceToDesiredPosition = relativeDirection.magnitude;
            float dotToDesiredPosition = Vector3.Dot(transform.forward, relativeDirection.normalized);
            Vector3 velocityInDirection = Vector3.Project(myPlayer.currentVelocity, relativeDirection.normalized);
            float speedInDirection = Vector3.Dot(myPlayer.currentVelocity, relativeDirection.normalized);

            float stoppingDistance = 3f;
            float relativeZDistance = relativeDirection.z;
            float relativeXDistance = relativeDirection.x;
            //if (distanceToDesiredPosition < stoppingDistance)
            //{
            //    myPlayer.verticalInput = Mathf.Clamp01(Mathf.Abs(distanceToDesiredPosition / stoppingDistance)) * -Mathf.Sign(relativeDirection.z);
            //    myPlayer.horizontalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.x) / 8f) * -Mathf.Sign(relativeDirection.x);
            //}
            if (speedInDirection <= -myPlayer.maxSpeed + 3f)
            {
                myPlayer.verticalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.z)) * -Mathf.Sign(relativeDirection.z);
                myPlayer.horizontalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.x)) * -Mathf.Sign(relativeDirection.x);
            }
            else
            {
                if (relativeZDistance < stoppingDistance)
                {
                    myPlayer.verticalInput = Mathf.Clamp01(Mathf.Abs(relativeZDistance / stoppingDistance)) * -Mathf.Sign(relativeDirection.z);
                }
                else
                {
                    myPlayer.verticalInput = 1f * -Mathf.Sign(relativeDirection.z);
                }
                if (relativeXDistance < stoppingDistance)
                {
                    myPlayer.horizontalInput = Mathf.Clamp01(Mathf.Abs(relativeXDistance / stoppingDistance)) * -Mathf.Sign(relativeDirection.x);

                }
                else
                {
                    myPlayer.horizontalInput = 1f * -Mathf.Sign(relativeDirection.x);

                }
            }

        }
        else
        {

            //// center point of triangle created by ball, goal, and opponent
            //desiredPosition =  FindCentroid(myPlayer.ball.transform.position, myPlayer.myGoalsPosition, opponentImDefending.transform.position);
            // center point of triangle created by ball, goal, and opponent
            desiredPosition = GetOffballDefensivePosition();
            Vector3 direction = desiredPosition - transform.position;
            Vector3 relativeDirection;
            if (myPlayer.teamIndex == 1)
            {
                relativeDirection = direction;
            }
            else
            {
                relativeDirection = -direction;
            }
            myPlayer.verticalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.z) / 8f) * -Mathf.Sign(relativeDirection.z);
            myPlayer.horizontalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.x) / 8f) * -Mathf.Sign(relativeDirection.x);
        }
    }

    Vector3 GetOffballDefensivePosition()
    {
        Vector3 direction = myPlayer.ball.transform.position - opponentImDefending.transform.position;
        Vector3 goalPosition = opponentImDefending.transform.position + (direction * .5f);
        return goalPosition;
    }

    // move to a position where the player with ball can pass to me
    private void GetOpen()
    {
        
        if (formationSpot == null)
        {
            formationSpot = teamBrain.GetAssignedFormationSpot(myPlayer);
            
        }
        Vector3 whereToMove = formationSpot.position; //  PitchGrid.instance.FindOptimalSpaceForPlayer(myPlayer);
        Vector3 relativeDirection;
        if (myPlayer.teamIndex == 1)
        {
            relativeDirection = whereToMove - transform.position;
        }
        else
        {
            relativeDirection = -(whereToMove - transform.position);
        }
        myPlayer.verticalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.z) / 8f) * -Mathf.Sign(relativeDirection.z);
        myPlayer.horizontalInput = Mathf.Clamp01(Mathf.Abs(relativeDirection.x) / 8f) * -Mathf.Sign(relativeDirection.x);
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
            opponentImDefending = myPlayer.opponents[nearestOpponentIndex];
            //if (nearestOpponentSquareDistance <= detectionRadius * detectionRadius)
            //{
            //    if ((transform.position - myPlayer.ball.transform.position).sqrMagnitude < nearestOpponentSquareDistance)
            //    {
            //        opponentImDefending = null;
            //    }
            //    else
            //    {
            //        opponentImDefending = myPlayer.opponents[nearestOpponentIndex];
            //    }

            //}
            //else
            //{
            //    opponentImDefending = null;
            //}
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
                if (!Physics.Raycast(transform.position, (shotPlacements[shotPlacementIndex] - transform.position).normalized, (shotPlacements[i] - transform.position).magnitude, goalLayerMask))
                {
                    return shotPlacements[shotPlacementIndex];
                }
                
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

    // Gives the player time to rotate towards where they want to pass it
    private IEnumerator LineUpPassRoutine(float delay)
    {

        yield return new WaitForSeconds(delay);
        myPlayer.StartKick();
        StartCoroutine(PassPowerRoutine(CalculatePassPower()));
        isLiningUpPass = false;
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
        myPlayer.StopPoweringUpKick();
        
    }


    // determines how long to hold the shoot button down
    private float CalculatePassPower()
    {
        float squareDistance = (desiredPosition - transform.position).sqrMagnitude;
        if (squareDistance < 250f)
        {
            return .15f;
        }
        else if (squareDistance < 810f)
        {
            return .25f;
        }
        else
        {
            return .35f;
        }
    }

    // simulates holding down the kick button
    private IEnumerator PassPowerRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        myPlayer.StopPoweringUpKick();
        myPlayer.EndKick();
        
    }



    // Method from Unity user Bunny83 at https://answers.unity.com/questions/296949/how-to-calculate-a-position-to-fire-at.html
    public static Vector3 CalculateInterceptCourse(Vector3 aTargetPos, Vector3 aTargetSpeed, Vector3 aInterceptorPos, float aInterceptorSpeed)
    {
        Vector3 targetDir = aTargetPos - aInterceptorPos;
        float iSpeed2 = aInterceptorSpeed * aInterceptorSpeed;
        float tSpeed2 = aTargetSpeed.sqrMagnitude;
        float fDot1 = Vector3.Dot(targetDir, aTargetSpeed);
        float targetDist2 = targetDir.sqrMagnitude;
        float d = (fDot1 * fDot1) - targetDist2 * (tSpeed2 - iSpeed2);
        if (d < 0.1f)  // negative == no possible course because the interceptor isn't fast enough
            return aTargetPos;
        float sqrt = Mathf.Sqrt(d);
        float S1 = (-fDot1 - sqrt) / targetDist2;
        float S2 = (-fDot1 + sqrt) / targetDist2;
        if (S1 < 0.0001f)
        {
            if (S2 < 0.0001f)
                return aTargetPos;
            else
                return (S2) * targetDir + aTargetSpeed;
        }
        else if (S2 < 0.0001f)
            return (S1) * targetDir + aTargetSpeed;
        else if (S1 < S2)
            return (S2) * targetDir + aTargetSpeed;
        else
            return (S1) * targetDir + aTargetSpeed;
    }

    // when AiController is disabled, let the team brain know we aren't defending the ball
    public void NotifyBrainOfDetachment()
    {
        if (myPlayer && teamBrain)
        {
            teamBrain.StoppedDefendingOpponent(opponentImDefending);
            //if (opponentImDefending == myPlayer.ball.owner)
            //{
            //    teamBrain.PlayerStoppedDefendingBall(myPlayer);
            //}
        }
    }

}
