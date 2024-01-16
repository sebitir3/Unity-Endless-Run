using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyFirstScript : MonoBehaviour
{
    // Awake is called once at the beginning of a GameObject's lifecycle, before Start()
    void Awake()
    {
        Debug.Log("Awake");
    }

    // Start is called before the first frame update, after Awake()
    void Start()
    {
        Debug.Log("Start");
    }

    // Called every time the GameObject goes from inactive to active.
    void OnEnable()
    {
        Debug.Log("OnEnable");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Update");
    }

    //Called on a fixed basis. Where all the Physics Updates occur
    void FixedUpdate()
    {
        Debug.Log("Fixed Update");
    }

    //Called after Update. Useful for dealing with anything that requires all GameObjects to have updated their positions for the frame 
    void LateUpdate()
    {
        Debug.Log("Late Update");
    }
}
