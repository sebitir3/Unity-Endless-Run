using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    PlayerInputActions inputAction;

    //Variables that store our input from PlayerInputActions
    float horizontal = 0f;
    public float jump { get; private set; } = 0f;
    public float slide { get; private set; } = 0f;

    //Input Variables in general
    public float hPrev { get; private set; } = 0f;
    public float hNew { get; private set; } = 0f;
    public float vPrev { get; private set; } = 0f;
    public float vNew { get; private set; } = 0f;

    public bool active = true;

    //Lane Variables
    int currentLane = 0;
    int previousLane = 0;
    int directionBuffer = 0;

    float laneWidth = 7.5f;
    //The Getter Property of laneWidth
    public float LaneWidth { get { return laneWidth; } }
    //While we made this public before, we only want the inspector to change this variable and no other file / class
    [SerializeField]
    int numberOfLanes = 3;
    //The Getter Propery of numberOfLanes
    public int NumLanes { get { return numberOfLanes; } }
    Coroutine currentLaneChange;
    int laneChangeStackCalls = 0;
    /* how fast we move from one lane to another
     * 1/strafeSpeed = the amount of time for a lane change (in seconds)
     */
    public float strafeSpeed = 5f;

    //Gravity and Initial velocity
    [SerializeField]
    float gravity = -9.81f;
    [SerializeField]
    float initialVelocity = 5f;

    Animator anim;
    int jumpParameter;
    int slideParameter;
    State currentState = State.Run;

    public bool isUsingNewInputSystem = true;

    void Awake()
    {
        anim = GetComponent<Animator>();
        jumpParameter = Animator.StringToHash("Jump");
        slideParameter = Animator.StringToHash("Slide");
        
        transform.position = Vector3.zero;
        //Determine the width of each lane so we can move the player to the center of each lane equally spaced apart
        laneWidth /= numberOfLanes;

        inputAction = new PlayerInputActions();

        if (!isUsingNewInputSystem)
        {
            return;
        }
        //The Input.GetAxis from the Old Unity Input System - using Lambda Expressions
        inputAction.Player.Horizontal.performed += ctx => horizontal = ctx.ReadValue<float>();

        //The KeyDown and KeyUp of our Jump action - Using Lambda Expressions
        inputAction.Player.Jump.performed += ctx => jump = ctx.ReadValue<float>();
        inputAction.Player.Jump.canceled += ctx => jump = ctx.ReadValue<float>();

        //The KeyDown and KeyUp of our Slide action - using Lambda Expressions
        inputAction.Player.Slide.performed += ctx => slide = ctx.ReadValue<float>();
        inputAction.Player.Slide.canceled += ctx => slide = ctx.ReadValue<float>();
    }

    public void Reset()
    {
        active = true;
        currentLane = 0;
        transform.position = Vector3.zero;
        jump = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isUsingNewInputSystem)
        {
            UseNewInputSystem();
        }
        else
        {
            UseOldInputSystem();
        }
    }

    private void UseNewInputSystem()
    {
        hNew = horizontal;
        float hDelta = hNew - hPrev;
        anim.enabled = !GameManager.Instance.isPaused;
        if (!GameManager.Instance.isPaused)
        {
            if (Mathf.Abs(hDelta) > 0f && Mathf.Abs(hNew) > 0f && currentState == State.Run)
            {
                MovePlayer((int)hNew);
            }

            if (slide == 1f && currentState == State.Run)
            {
                currentState = State.Slide;
                anim.SetTrigger(slideParameter);
            }

            if (jump == 1f && currentState == State.Run)
            {
                currentState = State.Jump;
                StartCoroutine(Jump());
            }
        }

        jump = 0f;
        slide = 0f;
        hPrev = hNew;
    }

    private void UseOldInputSystem()
    {
        //Here we'll test out our Inputs.
        //Math.Abs() gets the absolute value of a number, regardless of if its positive or negative

        //Horizontal -- move left and move right
        hNew = Input.GetAxisRaw(InputNames.horizontalAxis); // returns -1, 0 or 1 with no smoothing
        vNew = Input.GetAxisRaw(InputNames.verticalAxis);

        float hDelta = hNew - hPrev;
        float vDelta = vNew - vPrev;
        anim.enabled = !GameManager.Instance.isPaused;
        if (!GameManager.Instance.isPaused)
        {
            if (Mathf.Abs(hDelta) > 0f && Mathf.Abs(hNew) > 0f && currentState == State.Run)
            {
                MovePlayer((int)hNew);
            }

            int v = 0;
            if (Mathf.Abs(vDelta) > 0f)
            {
                v = (int)vNew;
            }

            // Jumping
            if ((Input.GetButtonDown(InputNames.jumpButton) || v == 1) && currentState == State.Run)
            {
                currentState = State.Jump;
                StartCoroutine(Jump());

            }

            // Sliding 
            if ((Input.GetButtonDown(InputNames.slideButton) || v == -1) && currentState == State.Run)
            {
                currentState = State.Slide;
                anim.SetTrigger(slideParameter);
            }
        }
        vPrev = vNew;
        hPrev = hNew;
    }

    //Change lane the player is in based off of the direction on the input -- left = negative, right = positive
    private void MovePlayer(int direction)
    {
        if (currentLaneChange != null)
        {
            if (currentLane + direction != previousLane)
            {
                directionBuffer = direction;
                return;
            }
            //Move the player into the new lane by changing their transform.position
            //if the coroutine has run before, we need to stop it
            StopCoroutine(currentLaneChange);
            directionBuffer = 0;
        }

        previousLane = currentLane;
        currentLane = Mathf.Clamp(currentLane + direction, numberOfLanes / -2, numberOfLanes / 2);

        //When we start our coroutine, we need to save a reference to it so we can stop it later
        currentLaneChange = StartCoroutine(LaneChange());
    }

    void FinishSlide()
    { 
        currentState = State.Run;
    }

    //Jump Coroutine
    IEnumerator Jump()
    {
        // Animation - we set the boolean on our animator called anim to true at the start of the jump coroutine
        anim.SetBool(jumpParameter, true);
        
        // Calculate total time of jump
        float tFinal = (initialVelocity * 2f) / -gravity; 
        
        // Calculate transition time - as we're jumping in the air, the animation plays
        float tLand = tFinal - 0.125f;
        float t = Time.deltaTime;
        for (; t < tLand; t += Time.deltaTime)
        {
            float y = gravity * (t * t) / 2f + initialVelocity * t;
            Helpers.SetPositionY(transform, y);
            yield return null;
        }
        // Animation - we set the boolean on our animator called anim to false at the end of the jump coroutine
        anim.SetBool(jumpParameter, false);
        currentState = State.Run;
        
        //When the animation stops, we are falling back down to the ground - so continue falling as normal
        for (; t < tFinal; t += Time.deltaTime)
        { 
            float y = gravity * (t * t) / 2f + initialVelocity * t; 
            Helpers.SetPositionY(transform, y);
            yield return null; 
        } 
        
        Helpers.SetPositionY(transform, 0f);
    }

    //Strafe Movement Coroutine
    IEnumerator LaneChange()
    {
        //Where we are coming from
        Vector3 fromPosition = (Vector3.right * previousLane * laneWidth);

        //Where are are going to
        Vector3 toPosition = Vector3.right * currentLane * laneWidth;

        float t = (laneWidth - Vector3.Distance(transform.position.x * Vector3.right, toPosition)) / laneWidth;
        //perform a gradual linear interpolation to the new lane each frame
        //for loops are not limited to integers, they can use floats. In addition, here we're incrementing by something other than one!
        for (; t < 1f; t += strafeSpeed * Time.deltaTime / laneWidth)
        {
            transform.position = Vector3.Lerp(fromPosition + Vector3.up * transform.position.y, toPosition + Vector3.up * transform.position.y, t);
            yield return null;
        }

        //make sure when where we end up is at the final destination in case the calculations didn't reach 1 exactly
        transform.position = toPosition + Vector3.up * transform.position.y;

        //When we have successfully completed a lane change, our current coroutine (this one) stops, so our reference to it is no longer needed
        currentLaneChange = null;

        //if our input buffer has some input that moves us to the left or right
        //we only want to buffer a few inputs, otherwise our coroutine calls start to stack up too high - limit to only two calls
        //similar to how we can do i++ the ++i is a tad faster to run since the computer knows they have to perform an addition operation        
        if (directionBuffer != 0 && ++laneChangeStackCalls < 2)
        {
            MovePlayer(directionBuffer);
            directionBuffer = 0;
        }

        //reset the amount of calls back down to zero
        laneChangeStackCalls = 0;
    }

    void PauseEditor()
    {
        //this allows you to pause Unity's Editor while the game is playing
        //UnityEditor.EditorApplication.isPaused = true;
    }

    private void OnEnable()
    {
        inputAction.Player.Enable();
    }

    private void OnDisable()
    {
        inputAction.Player.Disable();
    }
}
