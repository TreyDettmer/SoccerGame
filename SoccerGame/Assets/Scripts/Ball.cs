using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

    private Rigidbody rb;
    [SerializeField]
    private float radius = 0.5f;
    [SerializeField]
    private float airDensity = 0.1f;
    [SerializeField]
    private float artificialDragForce;

    private int collidedObjectCount = 0;

    public Player lastKickedBy;
    public Player owner;
    private Vector3 previousPosition;
    private Vector3 previousVelocity;
    [HideInInspector] public Vector3 currentVelocity;
    [HideInInspector] public Vector3 currentAccelerationVector;
    private TrailRenderer trailRenderer;
    bool isTrailRendererEnabled = true;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -1f || Mathf.Abs(transform.position.x) > 36f || Mathf.Abs(transform.position.z) > 60f)
        {
            if (lastKickedBy != null)
            {
                if (lastKickedBy.teamIndex == 0)
                {
                    GameplayManager.instance.HandleOutOfBounds(1);
                }
                else
                {
                    GameplayManager.instance.HandleOutOfBounds(0);
                }
            }
            else
            {
                GameplayManager.instance.HandleOutOfBounds();
            }
            
        }
    }

    private void FixedUpdate()
    {
        previousVelocity = currentVelocity;
        currentVelocity = (transform.position - previousPosition) / Time.fixedDeltaTime;
        previousPosition = transform.position;
        currentAccelerationVector = (currentVelocity - previousVelocity) / Time.fixedDeltaTime;
        if (owner == null)
        {
            if (!isTrailRendererEnabled)
            {
                trailRenderer.enabled = true;
                isTrailRendererEnabled = true;
            }
        }
        else
        {
            if (isTrailRendererEnabled)
            {
                trailRenderer.enabled = false;
                isTrailRendererEnabled = false;
            }
        }
        if (collidedObjectCount == 0)
        {
            var direction = Vector3.Cross(rb.angularVelocity, rb.velocity);
            var magnitude = 4 / 3f * Mathf.PI * airDensity * Mathf.Pow(radius, 3);
            rb.AddForce(magnitude * direction);
            
        }
        rb.AddForce(rb.velocity.normalized * -1f * artificialDragForce);
        rb.angularVelocity = rb.angularVelocity * .99f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        collidedObjectCount++;
        if (collision.collider.gameObject.GetComponent<Player>())
        {
            lastKickedBy = collision.collider.gameObject.GetComponent<Player>();
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        collidedObjectCount--;
        if (collidedObjectCount == 0)
        {
        }
    }

    public void SetOwner(Player player)
    {
        if (player != null)
        {
            owner = player;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.parent = owner.transform;
            rb.isKinematic = true;
            GameplayManager.instance.UpdateTeamWithBall(player.teamIndex);
        }
        else
        {
            rb.isKinematic = false;
            transform.parent = null;
            owner = null;
            GameplayManager.instance.UpdateTeamWithBall(-1);
        }
        
    }

}
