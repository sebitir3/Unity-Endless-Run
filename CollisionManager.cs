using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollisionManager : MonoBehaviour
{
    //The mask the player will use to check to see what it collides with
    int collisionMask;
    GameObject player;
    //Get a reference to the SkinnedMeshRenderer Component on the Robot GameObject
    public SkinnedMeshRenderer rend { get; private set; }
    Animator playerAnim;
    int slideCurveParameter;

    //Define an array of CollisionSpheres
    CollisionSphere[] collisionSpheres;

    
    //have a variable that lets us know if we are invulnerable or not
    public bool invincible { get; private set; } = false;
    // Inspector parameters - we can define them both on a single line and they both become visible in the inspector!
    [SerializeField]
    float blinkRate = 0.2f, blinkDuration = 3f;
    //This will be used to turn off and on the gizmo in Unity's Editor
    [SerializeField]
    bool debugSpheres = false;

    Vector3[] collisionSpheresDuringSlide;

    // Obstacle collision event
    public delegate void ObstacleCollisionHandler();
    public event ObstacleCollisionHandler OnObstacleCollision;

    void Start()
    {
        // Event initialization
        OnObstacleCollision += ObstacleCollision;

        //We'll need to find the robot gameobject in our scene
        player = GameObject.Find("Robot");
        //if it exists, great, if not, there's no need for collision manager so we can destroy it from the scene!
        if(!player)
        {
            //We will warn that the player robot gameobject is missing in the scene - escape characters \
            Debug.LogError("Could not find Player GameObject (searched \"Robot\")");
            Destroy(this);
        }
        //The component we want from the robot exists on one of its children. GetComponentInChildren helps us find it!
        rend = player.GetComponentInChildren<SkinnedMeshRenderer>();
        if (!rend)
        {
            Debug.LogError("Could not find SkinnedMeshRenderer component in Player children");
            Destroy(this);
        }

        playerAnim = player.GetComponent<Animator>();
        if (!playerAnim)
        {
            Debug.LogError("Animator Component not found on Player");
            Destroy(this);
        }
        slideCurveParameter = Animator.StringToHash("SlideCurve");

        collisionMask = GetLayerMask((int)Layer.Obstacle);
        
        // Import SphereCollider components into CollisionSphere objects using GetComponents<T>
        SphereCollider[] colliders = player.GetComponents<SphereCollider>();

        //the collisionSpheres are the same length as the SphereColliders we got from the robot GameObject
        collisionSpheres = new CollisionSphere[colliders.Length];
        //Step 2 - for every SphereCollider, construct a new CollisionSphere with the center and radius information passed in
        for (int i = 0; i < colliders.Length; i++)
        {
            collisionSpheres[i] = new CollisionSphere(colliders[i].center, colliders[i].radius);
        }

        Array.Sort(collisionSpheres, new CollisionSphereComparer());

        // Positions of CollisionSpheres mid-slide
        collisionSpheresDuringSlide = new Vector3[4];
        collisionSpheresDuringSlide[0] = new Vector3(0f, 0.2f, 0.75f);
        collisionSpheresDuringSlide[1] = new Vector3(0f, 0.25f, 0.25f);
        collisionSpheresDuringSlide[2] = new Vector3(0f, 0.55f, -0.15f);
        collisionSpheresDuringSlide[3] = new Vector3(0.4f, 0.7f, -0.28f);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(GameManager.Instance.isPaused)
        {
            return;
        }
        // A local list variable of colliders on obstacles we might run into
        List<Collider> collisions = new List<Collider>();
        
        //go through each collisionSphere and see whether they overlap with an obstacle
        for (int i = 0; i < collisionSpheres.Length; i++)
        {
            // Get vector that moves CollisionSphere to its final slide position
            Vector3 slideDisplacement = collisionSpheresDuringSlide[i] - collisionSpheres[i].offset;
            // Scale displacement by animation curve - like a linear interpolation between t = 0 and t = 1
            slideDisplacement *= playerAnim.GetFloat(slideCurveParameter);

            // Apply slide displacement to CollisionSphere's offset
            Vector3 offset = collisionSpheres[i].offset + slideDisplacement;

            //Physics.OverlapSphere returns an array of colliders overlapping a position and size on a given layermask
            //a for each loop is used on arrays or collections for getting a particular element out easily
            foreach (Collider c in Physics.OverlapSphere(player.transform.position + offset,
                                                        collisionSpheres[i].radius, collisionMask))
            { 
                //if our CollisionSpheres overlapped with something, we collided! add it to the list
                collisions.Add(c);
            } 
        }
        if (collisions.Count > 0)
        {
            OnObstacleCollision();
        }
    }

    void ObstacleCollision()
    {
        //if we are not invincible, meaning its our first time getting hit by an obstacle in a while...
        if (!invincible)
        {
            invincible = true;
            //start up a coroutine that will blink the player over time that doesn't interfere with the main game loop
            StartCoroutine(BlinkPlayer());
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !debugSpheres)
        {
            return;
        }

        for (int i = 0; i < collisionSpheres.Length; i++)
        {
            // Get vector that moves CollisionSphere to its final slide position
            Vector3 slideDisplacement = collisionSpheresDuringSlide[i] - collisionSpheres[i].offset;

            // Scale displacement by animation curve
            slideDisplacement *= playerAnim.GetFloat(slideCurveParameter);

            // Apply slide displacement to CollisionSphere's offset
            Vector3 offset = collisionSpheres[i].offset + slideDisplacement;

            //Select the Colour of the Gizmo to use to draw the collisionSphere
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(player.transform.position + offset, collisionSpheres[i].radius);
        }
    }

    //A Coroutine to help blink the player on and off over time
    IEnumerator BlinkPlayer()
    {
        rend.enabled = PlayerController.Instance.active;

        yield return new WaitUntil(() => PlayerController.Instance.active);
        //Time.time gets the current time as a float
        float startTime = Time.time;
        while (invincible)
        {
            // Toggle visibility - the ! is not, so this change from true to false and vice versa
            rend.enabled = !rend.enabled;
            // Check if blink period has expired
            if (Time.time >= startTime + blinkDuration)
            {
                rend.enabled = true;
                invincible = false;
            }
            yield return new WaitForSeconds(blinkRate);
        }
        rend.enabled = PlayerController.Instance.active;
    }


    //The params keyword lets us pass in any number of ints as arguments
    int GetLayerMask(params int[] indices) 
    { 
        //set the mask by default to be empty
        int mask = 0;
        //for every integer we passed in as a parameter
        for (int i = 0; i < indices.Length; i++)
        { 
            //we add that bit to the mask - turning on each light bulb we want without affecting the others
            mask |= 1 << indices[i];
        }
        //returned the combination of all the layers 
        return mask; 
    }

    //Ignores layers by inverting a predefined mask
    int GetLayerIgnoreMask(params int[] indices)
    {
        return ~GetLayerMask(indices);
    }
    
    //Add another layer to an existing mask, using the ref keyword. This keyword directly affects the variable passed in
    void AddLayers(ref int mask, params int[] indices)
    { 
        mask |= GetLayerMask(indices);
    } 
    
    //Remove another layer from an existing mask, using the ref keyword. This keyword directly affects the variable passed in    
    void RemoveLayers(ref int mask, params int[] indices)
    {  
        mask &= ~GetLayerMask(indices);
    } 

    //Binary XOR Operator copies the bit if it is set in one operand but not both
    void CopyLayers(ref int mask, params int[] indices)
    { 
        mask ^= GetLayerMask(indices);
    }

    //Create a struct called CollisionSphere - which is nested into the CollisionManager class
    //Structs are data types, meaning they contain direct access to their data
    struct CollisionSphere
    {
        //the location or origin of the sphere
        public Vector3 offset;
        //the size of the sphere
        public float radius;
        
        //Structs can also be built with constructors
        public CollisionSphere(Vector3 offset, float radius)
        {
            this.offset = offset;
            this.radius = radius;
        }

        /// <summary>
        /// This overloads the operator greater than to take in two CollisionSpheres - returning the comparison between two CollisionSpheres: lhs and rhs.
        /// </summary>
        /// <param name="lhs">The one we're checking</param>
        /// <param name="rhs">The one being compared to</param>
        /// <returns>The result if lhs offset.y value is greater than rhs offset.y</returns>
        public static bool operator >(CollisionSphere lhs, CollisionSphere rhs)
        {
            return lhs.offset.y > rhs.offset.y;
        }
        /// <summary>
        /// This overloads the operator greater than to take in two CollisionSpheres - returning the comparison between two CollisionSpheres: lhs and rhs.
        /// </summary>
        /// <param name="lhs">The one we're checking</param>
        /// <param name="rhs">The one being compared to</param>
        /// <returns>The result if lhs offset.y value is greater than rhs offset.y</returns>
        public static bool operator <(CollisionSphere lhs, CollisionSphere rhs)
        {
            return lhs.offset.y < rhs.offset.y;
        }
    }

    //Create a new struct which implements the IComparer interface, most interfaces start with an I as a convention to identify them
    struct CollisionSphereComparer : IComparer
    {
        //this method is part of the IComparer interface and must be implemented
        public int Compare(object a, object b)
        {
            //we only want to do comparisons between two collisionSpheres, not collisionSpheres and other types!
            if(!(a is CollisionSphere) || !(b is CollisionSphere))
            {
                //this is an error message that will show up on unity, the Environment.StackTrace will let us know what got us here
                Debug.LogError(Environment.StackTrace);
                //this is known as an exception, which will display a unique error message we can define for this particular type of error
                throw new ArgumentException("Cannot compare CollisionSpheres to non CollisionSpheres");
            }

            //if both objects are collisionSpheres, cast them into local variables so we can use our opertators on them
            CollisionSphere lhs = (CollisionSphere)a;
            CollisionSphere rhs = (CollisionSphere)b;

            if (lhs < rhs)
            {
                return -1;
            } else if (lhs > rhs)
            {
                return 1;
            } else
            {
                return 0;
            }
        }
    }
}
