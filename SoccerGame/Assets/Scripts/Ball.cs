using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Debug.DrawRay(transform.position, rb.angularVelocity.normalized * 5f, Color.red);
        Vector3 normalizedVel = new Vector3(rb.velocity.x, 0f, rb.velocity.y).normalized;
        Vector3 midCross = Vector3.Cross(rb.angularVelocity.normalized, normalizedVel);
        Debug.DrawRay(transform.position,Vector3.Cross(rb.angularVelocity.normalized, normalizedVel) * 5f, Color.green);
        Vector3 finalCross = Vector3.Cross(rb.angularVelocity.normalized, midCross.normalized);
        Debug.DrawRay(transform.position, finalCross.normalized * 5f, Color.blue);
    }
}
