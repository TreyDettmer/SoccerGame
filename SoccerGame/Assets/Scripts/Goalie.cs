using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goalie : MonoBehaviour
{
    private Rigidbody rb;
    private float desiredXOffset = 0f;
    public int teamIndex = -1;
    public float movementSpeed = .5f;
    public float movementRange = 4f;
    protected Ball ball;
    private float timePassed = 0f;
    private float xOrigin = 0f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ball = FindObjectOfType<Ball>();
        xOrigin = rb.position.x - movementRange;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        timePassed += Time.fixedDeltaTime * movementSpeed;
        
        rb.MovePosition(new Vector3(Mathf.Lerp(xOrigin, xOrigin + 2 * movementRange, Mathf.PingPong(timePassed, 1)), rb.position.y, rb.position.z));
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ball"))
        {


            if (ball.owner == null)
            {

            }
            else if (ball.owner != this)
            {
                ball.owner.BallStolen();
            }


         

        }
    }
}
