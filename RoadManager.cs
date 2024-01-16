using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadManager : Singleton<RoadManager>
{
    // This is an array of GameObjects, it doesn't have a predetermined size
    GameObject[] loadedPieces;

    //Our list of road pieces that will be changing in our Scene
    List<GameObject> roadPieces;

    //you can declare variables all on one line if they are of similar type
    Transform beginLeft, beginRight, endLeft, endRight;

    //Variable to calculate the rotation point of the current road piece
    Vector3 rotationPoint = Vector3.zero;

    //InspectorParameters
    [SerializeField]
    int numberOfPieces = 10;
    [SerializeField]
    string firstRoadPieceName = "Straight60m";
    [SerializeField]
    float roadSpeed = 20f;

    // Events and Delegates
    public delegate void AddPieceHandler(GameObject piece);
    public event AddPieceHandler OnAddPiece;

    // Use this for initialization
    void Start()
    {
        // Initialize OnAddPiece event with an empty method just in case something in the changes the execution order of our scripts
        OnAddPiece += x => { };

        //load all files of type GameObject from our RoadPieces folder inside our Resources
        loadedPieces = Resources.LoadAll<GameObject>("RoadPieces");

        // Initializing our List in memory -- initialize a new list of type GameObject (it only stores GameObjects)
        roadPieces = new List<GameObject>();

        roadPieces.Add(Instantiate(Resources.Load("RoadPieces/" + firstRoadPieceName)) as GameObject);
        roadPieces.Add(Instantiate(Resources.Load("RoadPieces/" + firstRoadPieceName)) as GameObject);

        //loop through all the loaded pieces using a for loop
        for (int i = 2; i < numberOfPieces; i++)
        {
            AddPiece();
        }

        roadPieces[0].transform.parent = roadPieces[1].transform;

        //Move the road past the first hard-coded road piece
        float halfRoadLength = (roadPieces[0].transform.Find("BeginLeft").position - roadPieces[0].transform.Find("EndLeft").position).magnitude / 2;
        roadPieces[0].transform.Translate(0f, 0f, -halfRoadLength, Space.World);

        // Get the 4 corner markers (children) of the first road piece
        SetCurrentRoadPiece();

    }

    public void Reset()
    {
        enabled = true;
        // Destroy road pieces from old road - clearing out list as well 

        Destroy(roadPieces[1]);
        roadPieces.Clear();

        // Generate new road by calling the start method again
        Start();
    }

    private void SetCurrentRoadPiece()
    {
        beginLeft = roadPieces[1].transform.Find("BeginLeft");
        beginRight = roadPieces[1].transform.Find("BeginRight");
        endLeft = roadPieces[1].transform.Find("EndLeft");
        endRight = roadPieces[1].transform.Find("EndRight");
        rotationPoint = GetRotationPoint(beginLeft, beginRight, endLeft, endRight);
    }

    void AddPiece()
    {

        /*We can utilize a library of C# called Random to create pseudo-random numbers for us
             *Random.Range starts from a value and goes up until(but not including) a max value*/


        int randomIndex = UnityEngine.Random.Range(0, loadedPieces.Length);


        /*Create an Instance of a random road piece from our array of loadedPieces,
         *then add that GameObject to our list of GameObjects called roadPieces*/


        roadPieces.Add(Instantiate(loadedPieces[randomIndex]));

        //Get references to the first two pieces we are processing - the newest and the previous
        Transform newPiece = roadPieces[roadPieces.Count - 1].transform;
        Transform previousPiece = roadPieces[roadPieces.Count - 2].transform;

        //Get the positions of our four Corner GameObjects -- the beginning 2 of new and the end 2 of previous
        beginLeft = newPiece.Find("BeginLeft");
        beginRight = newPiece.Find("BeginRight");

        endLeft = previousPiece.Find("EndLeft");
        endRight = previousPiece.Find("EndRight");

        //Compute the edges - the displacement of the beginning edge and end edges
        Vector3 beginEdge = beginRight.position - beginLeft.position;
        Vector3 endEdge = endRight.position - endLeft.position;

        //Compute the angle between the two edges -- the inner one
        float angle = Vector3.Angle(beginEdge, endEdge) * Mathf.Sign(Vector3.Cross(beginEdge, endEdge).y);

        //Rotate the new road piece to align with the previous
        newPiece.Rotate(0f, angle, 0f, Space.World);

        Vector3 displacement = endLeft.position - beginLeft.position;

        //Transform.Translate the new piece's GameObject to the new position
        //Space.World reflects the Scene's or Global axis and not the local piece's axis
        newPiece.Translate(displacement, Space.World);

        //Parent the newly created piece to the second road piece we hard-coded
        //transform.parent allows us to set a GameObject's parent to another Transform

        newPiece.parent = roadPieces[1].transform;

        //Here we pass into our event the newest piece being added to our road track 
        OnAddPiece(newPiece.gameObject);
    }

    void MoveRoadPieces(float distance)
    {
        // We can access a GameObject's tag simply by the . and can compare tags together like integers
        if (roadPieces[1].tag == Tags.straightPiece)
        {
            roadPieces[1].transform.Translate(0f, 0f, -distance, Space.World);
        }
        else
        {
            //Mathf.Abs is absolute value -- which disregards if the number is positive or negative
            float radius = Mathf.Abs(rotationPoint.x);

            //Rad2Deg converts Radians into degrees for us - so we can multiply it like a float to our previous equation
            float angle = (((roadSpeed * Time.deltaTime) / radius) * Mathf.Sign(roadPieces[1].transform.localScale.x)) * Mathf.Rad2Deg;

            //We have to rotate around the road piece and specify which vector3 point, which axis and how fast
            roadPieces[1].transform.RotateAround(rotationPoint, Vector3.up, angle);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.isPaused)
        {
            return;
        }
        MoveRoadPieces(roadSpeed * Time.deltaTime);


        // Step 1 - Determine when the parent road piece passes the origin on the x axis
        if (endLeft.position.z < 0f || endRight.position.z < 0f)
        {
            //Snap current road piece to the x axis
            float resetDistance = GetResetDistance();
            //Move Road piece into position
            MoveRoadPieces(-resetDistance);

            CycleRoadPieces();


            //force straight road pieces to align with world/global axis
            if (roadPieces[1].tag == Tags.straightPiece)
            {
                roadPieces[1].transform.rotation = new Quaternion(roadPieces[1].transform.rotation.x, 0f, 0f, roadPieces[1].transform.rotation.w);
                roadPieces[1].transform.position = new Vector3(0f, 0f, roadPieces[1].transform.position.z);
            }

            //Re-align road piece to x axis when pieces get deleted
            MoveRoadPieces(resetDistance);
        }

    }

    void CycleRoadPieces()
    {
        // Step 2 - Delete the piece that is behind the origin - this is Unity's GameObject.Destory() method
        Destroy(roadPieces[0]);
        //we also need to remove this road piece from our list, so we remove it both from the Scene and the list using RemoveAt(index)
        roadPieces.RemoveAt(0);

        // Step 3 - Add a new piece to the track - we have a method that does this!
        AddPiece();


        /*Step 4 - We need to reparent all our road pieces to the second indexed piece in our roadPieces list
        * You thought for loops were for incrementing, did you ? Well, they can also be used for decrementing as well!
         *this allows us to start from the end of our list and work backwards through it*/


        for (int i = roadPieces.Count - 1; i >= 0; i--)
                    {
                        // Step 5 - We need to unparent our GameObjects and reparent them to the second road piece in our list 
                        // [1] must be unparented before [0] can be reparented, so iterate through our list backwards
                        roadPieces[i].transform.parent = null;
                        roadPieces[i].transform.parent = roadPieces[1].transform;
                    }

        // Step 6 - Get the corner markers of the current index 1 road piece, so we can check again if we've passed the origin on this new piece
        SetCurrentRoadPiece();
    }

    // distance required move piece to align with world x-axis
    float GetResetDistance()
    {
        //Check to see which type of road piece we are dealing with - through use of Unity Tags
        if (roadPieces[1].tag == Tags.straightPiece)
        {
            //return the position of our straight piece - where it is on the z axis
            return -endLeft.transform.position.z;
        }
        else
        {
            //Get the End Edge of the curved road piece
            Vector3 endEdge = endRight.position - endLeft.position;
            //Calculate the angle of our road piece in relation to the global x axis
            float angle = Vector3.Angle(Vector3.right, endEdge);
            // Get the radius of our rotation point so we have a partial circle
            float radius = Mathf.Abs(rotationPoint.x);
            // convert angle to radians and calculate angular velocity - return total
            return angle * Mathf.Deg2Rad * radius;
        }
    }

    // A function that returns the rotation point of our road pieces
    public Vector3 GetRotationPoint(Transform beginLeft, Transform beginRight, Transform endLeft, Transform endRight)
    {
        // Compute edges from corner positions
        Vector3 beginEdge = beginLeft.position - beginRight.position;
        Vector3 endEdge = endLeft.position - endRight.position;
        // square magnitude of begin edge
        float a = Vector3.Dot(beginEdge, beginEdge);
        // project BeginEdge onto EndEdge
        float b = Vector3.Dot(beginEdge, endEdge);
        // square magnitude of end edge
        float e = Vector3.Dot(endEdge, endEdge);
        // difference between square magnitudes of beginEdge and endEdge minus the square of their Dot Product
        float difference = a * e - b * b;
        // the 3D vector between the beginLeft and endLeft position of our road piece
        Vector3 r = beginLeft.position - endLeft.position;

        float c = Vector3.Dot(beginEdge, r);
        float f = Vector3.Dot(endEdge, r);
        float s = (b * f - c * e) / difference;
        float t = (a * f - c * b) / difference;

        Vector3 rotationPointBegin = beginLeft.position + beginEdge * s;
        Vector3 rotationPointEnd = endLeft.position + endEdge * t;
        // return midpoint between two closest points
        return (rotationPointBegin + rotationPointEnd) / 2f;
    }
}
