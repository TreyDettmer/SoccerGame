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
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ball = FindObjectOfType<Ball>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(float value)
    {
        desiredXOffset += (value * movementSpeed);
        desiredXOffset = Mathf.Clamp(desiredXOffset, -movementRange, movementRange);   
    }

    private void FixedUpdate()
    {
        rb.MovePosition(new Vector3(desiredXOffset, rb.position.y, rb.position.z));
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
