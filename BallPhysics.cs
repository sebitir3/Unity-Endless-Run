using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    public float speed = 20f;
    Rigidbody rbody;
    Transform tCamera;

    float boost = 1f;


    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        tCamera = GameObject.Find("Main Camera").transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 forward = tCamera.forward;
        Vector3 right = tCamera.right;

        if (Input.GetKey(KeyCode.Space))
        {
            boost = 2f;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            rbody.AddForce(forward * speed * boost);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            rbody.AddForce(-forward * speed * boost);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rbody.AddForce(-right * speed * boost);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            rbody.AddForce(right * speed * boost);
        }
    }

}
