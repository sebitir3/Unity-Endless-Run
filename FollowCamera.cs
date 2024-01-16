using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{

    public float zDistance;
    public float yDistance;
    public GameObject sphere;
    // Start is called before the first frame update
    void Start()
    {
        //alternatively, drag Sphere into this variable from Inspector
        sphere = GameObject.Find("Player");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.up * Time.deltaTime * 180);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.up * Time.deltaTime * -180);
        }
        transform.position = (sphere.transform.position + transform.forward * zDistance);
        transform.Translate(Vector3.up * yDistance);
    }
}
