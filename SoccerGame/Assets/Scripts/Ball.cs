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

    public PlayerController lastKickedBy;
    public PlayerController owner;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -1f)
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
    }
    private void OnCollisionExit(Collision collision)
    {
        collidedObjectCount--;
        if (collidedObjectCount == 0)
        {
        }
    }

    public void SetOwner(PlayerController playerController)
    {
        if (playerController != null)
        {
            owner = playerController;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.parent = owner.transform;
            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = false;
            transform.parent = null;
            owner = null;
        }
        
    }

}
